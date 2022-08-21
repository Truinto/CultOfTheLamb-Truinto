using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public class Patch_Adoration
    {
        [HarmonyPatch(typeof(FollowerBrainStats), nameof(FollowerBrainStats.Adoration), MethodType.Setter)]
        [HarmonyPrefix]
        public static bool Prefix1(float value, FollowerBrainStats __instance)
        {
            if (!Settings.State.loyaltyOverflow)
                return true;

            if (value == 0f && __instance._info.Adoration > 100f)
            {
                __instance._info.Adoration -= 100f;



                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(FollowerBrainStats), nameof(FollowerBrainStats.HasGivenDiscipleReward), MethodType.Getter)]
        [HarmonyPrefix]
        public static void Prefix2(ref bool __result, FollowerBrainStats __instance)
        {
            if (Settings.State.loyaltyOverflow && __instance.HasLevelledUp)
            {
                __instance._info.HasGivenDiscipleReward = false;
            }
        }

        /// <summary>
        /// ignores if (this.Stats.HasLevelledUp || !DataManager.Instance.ShowLoyaltyBars)
        /// </summary>
        [HarmonyPatch(typeof(FollowerBrain), nameof(FollowerBrain.AddAdoration), typeof(Follower), typeof(FollowerBrain.AdorationActions), typeof(Action))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler3(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var code = instr as List<CodeInstruction> ?? instr.ToList();
            int index = 0;

            code.NextJumpNever(ref index);
            code.NextJumpAlways(ref index);

            return code;
        }
    }
}
