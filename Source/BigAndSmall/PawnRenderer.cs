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
    [HarmonyPatch(typeof(PawnRenderer), "BaseHeadOffsetAt")]
    public static class PawnRenderer_BaseHeadOffsetAt
    {
        public static void Postfix(ref Vector3 __result)
        {
            if (BigSmall.performScaleCalculations 
                && BigSmall.humnoidScaler != null
                && BigSmall.activePawn != null)
            {
                __result.z *= BigSmall.humnoidScaler.GetHeadRenderSize();
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]
    public static class PawnRenderer_RenderPawnInternal
    {
        /// <summary>
        /// Set the pawn schedueled for rendering as the active pawn for scaling.
        /// </summary>
        //[HarmonyPrefix]
        public static void Prefix(Pawn ___pawn)
        {
            BigSmall.activePawn = ___pawn;
        }


        /// <summary>
        /// Pawn has been processed, time to remove it.
        /// </summary>
        //[HarmonyPostfix]
        public static void Postfix()
        {
            BigSmall.activePawn = null;
        }
    }
}
