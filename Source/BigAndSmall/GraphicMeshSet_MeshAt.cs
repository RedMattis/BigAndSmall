using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
//using VariedBodySizes;
using Verse;

namespace BigAndSmall
{
    public static partial class HarmonyPatches
    {
        [HarmonyPatch(typeof(GraphicMeshSet), nameof(GraphicMeshSet.MeshAt))]
        public static class GraphicMeshSet_MeshAt
        {
            public static void Postfix(GraphicMeshSet __instance, ref Mesh __result, Rot4 rot)
            {
                //// We PostFix their method instead if it exists.

                if (BigSmall.performScaleCalculations
                && BigSmall.humnoidScaler != null
                && BigSmall.activePawn != null)
                {
                    //if (HasVFE)
                    //{
                    //    //BigSmall.humnoidScaler.GetBodyRenderSize(BigSmall.activePawn, setVEFBody: true);
                    //}
                    //else
                    //{
                    __result = BigSmall.GetPawnMesh(BigSmall.humnoidScaler.GetBodyRenderSize(BigSmall.activePawn), rot.AsInt == 3);
                    //}
                }

            }
        }
    }

    //[HarmonyPatch(typeof(HumanlikeMeshPoolUtility), nameof(HumanlikeMeshPoolUtility.HumanlikeBodyWidthForPawn))]
    //public static class HumanlikeBodyWidthForPawn_Patch
    //{
    //    public static void Postfix(ref float __result, Pawn pawn)
    //    {
    //        Log.Message($"Testing of HumanlikeBodyWith should run... {ModsConfig.BiotechActive}, {pawn.ageTracker.CurLifeStage.bodyWidth.HasValue}");
    //        if (ModsConfig.BiotechActive && pawn.ageTracker.CurLifeStage.bodyWidth.HasValue)
    //        {
    //            Log.Message("Postfixing HumanlikeBodyWidthFor pawn...");
    //            if (BigSmall.performScaleCalculations
    //            && BigSmall.humnoidScaler != null
    //            && BigSmall.activePawn != null)
    //            {
    //                float oldResult = __result;
    //                __result = BigSmall.humnoidScaler.GetBodyRenderSize(pawn);

    //                Log.Message($"did postfix of HumanlikeBodyWidthFor pawn. multiplied by {BigSmall.humnoidScaler.GetBodyRenderSize(pawn)} for a total result of {__result}. Was {oldResult}");
    //            }
    //        }

    //    }
    //}

}
