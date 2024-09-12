using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_Mating
    {
        [HarmonyPatch(typeof(Structures_MatingTent), nameof(Structures_MatingTent.SetEggInfo))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Patch_GoldenEgg(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);

            tool.Seek(OpCodes.Ldc_R4, 0.2f);
            tool.InsertAfter(patch);

            return tool;

            static float patch(float __stack)
            {
                return Settings.State.goldenEggChance;
            }
        }
    }
}
