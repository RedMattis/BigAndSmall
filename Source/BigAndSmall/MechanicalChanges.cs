using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.Noise;
using Verse;
using RimWorld;
using UnityEngine;

namespace BigAndSmall
{
    [HarmonyPatch(typeof(Pawn), "HealthScale", MethodType.Getter)]
    public static class Pawn_HealthScale
    {
        public static void Postfix(ref float __result, Pawn __instance)
        {
            if (BigSmall.performScaleCalculations && BigSmall.humnoidScaler != null)
            {
                float quad = BigSmall.humnoidScaler.GetSizeChangeMultiplier(HumanoidPawnScaler.SizeChangeType.Quadratic, __instance);
                float linear = BigSmall.humnoidScaler.GetSizeChangeMultiplier(HumanoidPawnScaler.SizeChangeType.Linear, __instance);
                if (linear > quad) { quad = linear; } // Make sure small creatures don't get absolutely unreasonably low health.

                __result *= Mathf.Lerp(linear, quad, 0.25f);
            }
        }
    }

    [HarmonyPatch(typeof(Need_Food), "FoodFallPerTickAssumingCategory")]
    public static class Need_Food_FoodFallPerTickAssumingCategory
    {
        public static void Prefix(ref Pawn ___pawn, out float __state)
        {
            __state = ___pawn.def.race.baseHungerRate;
            if (BigSmall.performScaleCalculations
                && BigSmall.activePawn != null
                && BigSmall.activePawn.needs != null
                && BigSmall.humnoidScaler != null
                && BigSmall.activePawn.DevelopmentalStage < DevelopmentalStage.Baby)
            {
                float quad = BigSmall.humnoidScaler.GetSizeChangeMultiplier(HumanoidPawnScaler.SizeChangeType.Quadratic, BigSmall.activePawn);
                float linear = BigSmall.humnoidScaler.GetSizeChangeMultiplier(HumanoidPawnScaler.SizeChangeType.Linear, BigSmall.activePawn);
                ___pawn.def.race.baseHungerRate = __state * Mathf.Max(quad, linear);
            }
        }

        public static void Postfix(Pawn ___pawn, float __state)
        {
            ___pawn.def.race.baseHungerRate = __state;
        }
    }

    [HarmonyPatch(typeof(RaceProperties), "NutritionEatenPerDayExplanation")]
    public static class RaceProperties_NutritionEatenPerDayExplanation
    {
        public static void Prefix(ref Pawn p, out float __state)
        {
            __state = p.def.race.baseHungerRate;
            if (
                BigSmall.performScaleCalculations
                && BigSmall.activePawn != null
                && BigSmall.activePawn.needs != null
                && BigSmall.humnoidScaler != null
                && BigSmall.activePawn.DevelopmentalStage < DevelopmentalStage.Baby)
            {
                float quad = BigSmall.humnoidScaler.GetSizeChangeMultiplier(HumanoidPawnScaler.SizeChangeType.Quadratic, BigSmall.activePawn);
                float linear = BigSmall.humnoidScaler.GetSizeChangeMultiplier(HumanoidPawnScaler.SizeChangeType.Linear, BigSmall.activePawn);
                p.def.race.baseHungerRate = __state * Mathf.Max(quad, linear);
            }
        }

        public static void Postfix(Pawn p, float __state)
        {
            p.def.race.baseHungerRate = __state;
        }
    }


    [HarmonyPatch(typeof(Verb_MeleeAttack), "GetDodgeChance")]
    public static class VerbMeleeAttack_GetDodgeChance
    {
        public static void Postfix(ref float __result, LocalTargetInfo target)
        {
            if (BigSmall.performScaleCalculations 
                && BigSmall.humnoidScaler != null)
            {
                if (target.Thing is Pawn pawn)
                {
                    __result /= BigSmall.humnoidScaler.GetSizeChangeMultiplier(HumanoidPawnScaler.SizeChangeType.Linear, pawn);
                }
            }
        }
    }

    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.GetDamageFactorFor), new Type[]
        {
        typeof(Tool),
        typeof(Pawn),
        typeof(HediffComp_VerbGiver)
        })
    ]
    public static class VerbProperties_GetDamageFactorFor_Patch
    {
        public static void Postfix(ref float __result, Pawn attacker, VerbProperties __instance)
        {
            if (BigSmall.performScaleCalculations &&
                __instance.IsMeleeAttack && attacker != null
                && BigSmall.humnoidScaler != null)
            {
                float damageMultiplier = BigSmall.humnoidScaler.GetSizeChangeMultiplier(HumanoidPawnScaler.SizeChangeType.Linear, attacker);
                if (damageMultiplier > 1)
                {
                    // Make giants a bit less prone to instant-killing.
                    damageMultiplier = Mathf.Pow(damageMultiplier, 0.75f);
                }

                // Mostly for balance reasons, too much instant-death otherwise.
                __result *= damageMultiplier;
            }
        }
    }


    //// AdjustedArmorPenetration

    //[HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedArmorPenetration), new Type[]
    //    {
    //    typeof(Tool),
    //    typeof(Pawn),
    //    typeof(HediffComp_VerbGiver)
    //    })
    //]
    //public static class VerbProperties_AdjustedArmorPenetration_Patch
    //{
    //    public static void Postfix(ref float __result, Pawn attacker, VerbProperties __instance)
    //    {
    //        if (BigSmall.performScaleCalculations &&
    //            __instance.IsMeleeAttack && attacker != null
    //            && BigSmall.humnoidScaler != null)
    //        {
    //            float armorPenAdjustment = BigSmall.humnoidScaler.GetSizeChangeMultiplier(HumanoidPawnScaler.SizeChangeType.Linear, attacker) - 1;
    //            float extraArmorPen = 20 * armorPenAdjustment;

    //            if (extraArmorPen > 0)
    //            {
    //                // Make giants a bit less prone to instant-killing.
    //                armorPenAdjustment = Mathf.Pow(armorPenAdjustment, 0.50f);
    //                Log.Message($"DEBUG: Armor Pen is {armorPenAdjustment}");
    //            }
    //            else
    //            {
    //                armorPenAdjustment = 0;
    //            };

    //            // Mostly for balance reasons, too much instant-death otherwise.
    //            __result += armorPenAdjustment;
    //        }
    //    }
    //}

}
