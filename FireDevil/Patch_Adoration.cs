using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FollowerBrain;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_Adoration
    {
        [HarmonyPatch(typeof(FollowerBrainStats), nameof(FollowerBrainStats.Adoration), MethodType.Setter)]
        [HarmonyPrefix]
        public static bool Prefix1(float value, FollowerBrainStats __instance)
        {
            if (!Settings.State.loyaltyOverflow)
                return true;

            if (value == 0f && __instance._info.Adoration > 100f && __instance._info.XPLevel < 9)
            {
                __instance._info.Adoration -= 100f;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(FollowerBrain), nameof(FollowerBrain.AddAdoration), typeof(Follower), typeof(AdorationActions), typeof(Action))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler2(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);

            tool.Seek(typeof(FollowerBrainStats), nameof(FollowerBrainStats.HasLevelledUp));
            tool.InsertAfter(patch);

            return tool;

            static bool patch(bool __stack)
            {
                if (!Settings.State.loyaltyOverflow)
                    return __stack;
                return false;
            }
        }
    }
}
