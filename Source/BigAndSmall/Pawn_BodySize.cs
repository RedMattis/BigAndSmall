using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.Noise;
using Verse;

namespace BigAndSmall
{
    [HarmonyPatch(typeof(Pawn), "BodySize", MethodType.Getter)]
    public static class Pawn_BodySize
    {
        public static void Postfix(ref float __result, Pawn __instance)
        {
            if (BigSmall.performScaleCalculations 
                && __instance.RaceProps.Humanlike
                && !BigSmall.skipBodySizePostFix
                && BigSmall.humnoidScaler != null)
            {
                __result = HumanoidPawnScaler.GetPawnBodySizeOffset(__instance, getTotalSize: true);
            }
        }
    }

}
