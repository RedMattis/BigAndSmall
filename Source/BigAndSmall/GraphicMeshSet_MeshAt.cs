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
}
