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
        [HarmonyPatch(typeof(Graphic), "MeshAt")]
        public static class Graphic_MeshAt
        {
            public static void Prefix(ref Vector2 ___drawSize, out Vector2 __state)
            {
                __state = ___drawSize;
                if (BigSmall.performScaleCalculations
                    && BigSmall.activePawn != null
                    && BigSmall.humnoidScaler != null)
                {
                    float variedBodySize = BigSmall.humnoidScaler.GetBodyRenderSize(BigSmall.activePawn);

                    ___drawSize = new Vector2(___drawSize.x * variedBodySize, ___drawSize.y * variedBodySize );
                }
            }

            public static void Postfix(ref Vector2 ___drawSize, Vector2 __state)
            {
                ___drawSize = __state;
            }
        }
    }

}
