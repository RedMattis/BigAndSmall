using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace BigAndSmall
{
    public static partial class HarmonyPatches
    {
        private static bool HasSOS => ModsConfig.IsActive("kentington.saveourship2"); 
        private static bool HasVFE => ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core");

        private static bool NotNull(params object[] input)
        {
            if (input.All(o => o != null))
            {
                return true;
            }

            Log.Message("Signature match not found");
            foreach (var obj in input)
            {
                if (obj is MemberInfo memberObj)
                {
                    Log.Message($"\tValid entry:{memberObj}");
                }
            }

            return false;
        }
    }
}
