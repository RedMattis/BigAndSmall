
using System.Linq;

using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace BigAndSmall
{
    /// <summary>
    /// Main class of the "Big and Small Races" mod. This mod intends to add dwarves and ogres using the Rimworld Gene system.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class BigSmall
    {
        public static HumanoidPawnScaler humnoidScaler;

        public static Pawn activePawn;

        public static bool performScaleCalculations = true;

        public static readonly List<ThingDef> AllPawnTypes;

        public static readonly Dictionary<float, Mesh> CachedMeshes;

        public static readonly Dictionary<float, Mesh> CachedInvertedMeshes;

        public static Dictionary<Pawn, float> CachedSize;

        /// <summary>
        /// So we can query the BodySize from before our changes. You'll get infinite recursion without this btw. :3
        /// </summary>
        public static bool skipBodySizePostFix = false;

        static BigSmall()
        {
            AllPawnTypes = (from def in DefDatabase<ThingDef>.AllDefsListForReading
                            where def.race != null
                            orderby def.label
                            select def).ToList();
            CachedMeshes = new Dictionary<float, Mesh>();
            CachedInvertedMeshes = new Dictionary<float, Mesh>();
            CachedSize = new Dictionary<Pawn, float>();

            ApplyHarmonyPatches();

            //foreach (var method in Harmony.GetAllPatchedMethods())
            //{
            //    try
            //    {
            //        Log.Message(method.ToString());
            //    }
            //    catch
            //    {
            //        Log.Message("Failed to print method");
            //    }
            //}

        }

        public static Mesh GetPawnMesh(float size, bool inverted)
        {
            if (inverted)
            {
                if (!CachedInvertedMeshes.ContainsKey(size))
                {
                    CachedInvertedMeshes[size] = MeshMakerPlanes.NewPlaneMesh(1.5f * size, flipped: true, backLift: true);
                }

                return CachedInvertedMeshes[size];
            }

            if (!CachedMeshes.ContainsKey(size))
            {
                CachedMeshes[size] = MeshMakerPlanes.NewPlaneMesh(1.5f * size, flipped: false, backLift: true);
            }

            return CachedMeshes[size];
        }

        static void ApplyHarmonyPatches()
        {
            var harmony = new Harmony("RedMattis.BigSmall");
            harmony.PatchAll();

        }
    }

    public class HumanoidPawnScaler : Verse.GameComponent
    {
        public Game currentGame;

        /// <summary>
        /// This dictionary is completely pointless at the moment since the optimization was removed.
        /// Granted, I am doubtful the optimization made a difference considering the low compexlity of the operations.
        /// </summary>
        //public Dictionary<Pawn, float> sizeVariationDictionary;

        //private List<Pawn> variedBodySizesDictionaryKeys;
        //private List<float> variedBodySizesDictionaryValues;

        public HumanoidPawnScaler(Game game)
        {
            BigSmall.humnoidScaler = this;
            currentGame = game;
        }

        /// <summary>
        /// This method could get overriden to let pawn size be changed by something else.
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="getTotalSize">Instead of fetching the offset, grab the total size with offset included.</param>
        /// <returns></returns>
        public static float GetPawnBodySizeOffset(Pawn pawn, bool getTotalSize = false)
        {
            float SM_BodySizeOffset = getTotalSize ? 1.0f : 0.0f;

            if (ModsConfig.BiotechActive
                && pawn.RaceProps.Humanlike
                && (pawn.needs != null || pawn.Dead)) // && pawn.needs != null
            {
                var dStage = pawn.DevelopmentalStage;
                float sizeFromAge = pawn.ageTracker.CurLifeStage.bodySizeFactor;
                float baseSize = pawn.RaceProps.baseBodySize;
                float previousTotalSize = sizeFromAge * baseSize;

                StatDef statOffsetDef = StatDef.Named("SM_BodySizeOffset");
                float sizeOffset = pawn.GetStatValue(statOffsetDef);
                
                StatDef statMultDef = StatDef.Named("SM_BodySizeMultiplier");
                float sizeMultiplier = pawn.GetStatValue(statMultDef);

                float sizeFromOffset = sizeOffset * sizeMultiplier * sizeFromAge;

                float sizeFromMultiplier = (baseSize * sizeMultiplier - baseSize) * sizeFromAge;

                SM_BodySizeOffset = sizeFromOffset + sizeFromMultiplier;
                float totalSize = SM_BodySizeOffset + previousTotalSize;
                if (getTotalSize)
                {
                    // Prevent babies from getting too large for cribs, or too smol in general.
                    if (dStage < DevelopmentalStage.Child)
                    {
                        totalSize = Mathf.Clamp(totalSize, 0.05f, 0.24f);
                    }
                    else if (totalSize < 0.15) totalSize = 0.15f;
                    return totalSize;
                }
                else if (totalSize < 0.05f && dStage < DevelopmentalStage.Child)
                {
                    SM_BodySizeOffset = -(previousTotalSize - 0.05f);
                }
                else if (totalSize > 0.24f && dStage < DevelopmentalStage.Child) // Don't permit babies too large to fit in cribs (0.25)
                {
                    SM_BodySizeOffset = -(previousTotalSize - 0.24f);
                }
                else if (totalSize < 0.10f && dStage == DevelopmentalStage.Child) // If child basically limit size to 0.10
                {
                    SM_BodySizeOffset = -(previousTotalSize - 0.10f);
                }
                else if (totalSize < 0.18f && dStage > DevelopmentalStage.Child) // If adult basically limit size to 0.18
                {
                    SM_BodySizeOffset = -(previousTotalSize - 0.18f);
                }
            }
            return SM_BodySizeOffset;
        }

        public float GetSizeChangeMultiplier(SizeChangeType type, Pawn pawn = null)
        {
            if (pawn == null)
                pawn = BigSmall.activePawn;

            if (pawn == null || Scribe.mode != 0)
            {
                return 1f;
            }

            if (!pawn.RaceProps.Humanlike)
            {
                return 1f;
            }

            float defaultSize = 1f;
            if (BigSmall.CachedSize.ContainsKey(pawn))
            {
                defaultSize = BigSmall.CachedSize[pawn];
            }

            if (BigSmall.humnoidScaler != null && pawn != null 
                && (pawn.needs != null || pawn.Dead))
            {
                float scaleMod = GetPawnBodySizeOffset(pawn, getTotalSize:false);
                //BigSmall.skipBodySizePostFix = true;
                float sizeFromAge = pawn.ageTracker.CurLifeStage.bodySizeFactor;
                float baseSize = pawn.RaceProps.baseBodySize;
                float prevBodySize = sizeFromAge * baseSize;
                float postBodySize = prevBodySize + scaleMod;
                //float postModScale = pawn.BodySize + scaleMod;  
                //float unmoddedScale = pawn.BodySize;
                float percentChange = postBodySize / prevBodySize;
                //BigSmall.skipBodySizePostFix = false;
                

                if (type == SizeChangeType.Quadratic)
                {
                    percentChange = Mathf.Pow(1 + percentChange, 2) - 1;
                }
                else if (type == SizeChangeType.Weight)
                {
                    percentChange = Mathf.Pow(1 + percentChange, 3) - 1;
                }

                // Let's not make humans sprites unreasonably small.
                if (percentChange < 0.2f) percentChange = 0.2f;
                return percentChange;
            }
            return defaultSize;
        }

        /// <summary>
        /// Used to get more realistic results from size changes.
        /// F.ex. most things scale quadratically, but weight/health scales by cube.
        /// 
        /// Technically a Rimworld Scale isn't really linear, but this type of change gives fairly good values when going upwards.
        /// Downwards is another story though, and we don't want small pawns to get utterly obliterated if something looks at the wrong.
        /// </summary>
        public enum SizeChangeType
        {
            Linear = 1,     // ...Height
            Quadratic = 2,  // Muscle Strength, food consumption, health, etc.
            Weight = 3      // Weight
        };

        public float GetHeadRenderSize(Pawn pawn = null)
        {
            float bodyRSize = GetBodyRenderSize(pawn);

            float headSize = bodyRSize;
            //if (headSize < 1)
            //{
            //    headSize = Mathf.Pow(bodyRSize, 0.8f);
            //    headSize = Math.Max(bodyRSize - 0.5f, headSize);
            //}
            //else
            //{
            //    headSize = Mathf.Pow(bodyRSize, 0.5f);
            //}

            return headSize;
            // Use the below if we can figure out a way to change the head size...
            //float headSize = GetActualSizeChangePercent();
            //if (headSize < 1)
            //{
            //    // Only shrink heads by fraction of the body to retain Rimworld's bobbleheaded apperance.
            //    headSize = Mathf.Pow(headSize, 0.40f);
            //}

            //return headSize;
        }

        public float GetBodyRenderSize(Pawn pawn = null)
        {
            if (Scribe.mode != LoadSaveMode.Inactive) { return 1; }

            if (pawn == null)
                pawn = BigSmall.activePawn;

            if (pawn == null || Scribe.mode != LoadSaveMode.Inactive) return 1;

            float bodySizeChange = GetSizeChangeMultiplier(SizeChangeType.Linear, pawn: pawn);

            if (bodySizeChange == 1)
            {
                return 1;
            }
            // Make Nisse babies smaller so they look plausible next to their parents.
            else if (bodySizeChange < 1 && pawn.DevelopmentalStage < DevelopmentalStage.Child)
            {
                //bodySizeChange = Mathf.Pow(bodySizeChange, 1.3f); // Let's make the babies even smaller.
                //Mathf.Clamp(bodySizeChange, 0.3f, 1.0f);
                bodySizeChange = Mathf.Pow(bodySizeChange, 0.80f);
            }
            else if (bodySizeChange < 1 && pawn.DevelopmentalStage < DevelopmentalStage.Adult)
            {
                bodySizeChange = Mathf.Pow(bodySizeChange, 0.75f);
            }
            else if (bodySizeChange < 1) // Don't make children/adults too small on screen.
            {
                bodySizeChange = Mathf.Pow(bodySizeChange, 0.65f);
            }
            else if (pawn.DevelopmentalStage < DevelopmentalStage.Child) // Babies should still be small-ish. even for large races.
            {
                bodySizeChange = Mathf.Pow(bodySizeChange, 0.40f);
            }
            else if (pawn.DevelopmentalStage < DevelopmentalStage.Adult) // Don't oversize children too much.
            {
                bodySizeChange = Mathf.Pow(bodySizeChange, 0.50f);
            }
            else // Don't make large characters unreasonably huge.
            {
                bodySizeChange = Mathf.Pow(bodySizeChange, 0.70f);
            }

            // Children are already chibified, applying the same scaling on them as you do for adutlts will look strange.
            //if (pawn.DevelopmentalStage < DevelopmentalStage.Child)
            //{
            //    bodySizeChange = Mathf.Lerp(bodySizeChange, 1, 0.50f);
            //}
            //else if (pawn.DevelopmentalStage == DevelopmentalStage.Child)
            //{
            //    bodySizeChange = Mathf.Lerp(bodySizeChange, 1, 0.50f);
            //}
            return bodySizeChange;
        }


        
    }

}
