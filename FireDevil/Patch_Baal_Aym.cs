using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_Baal_Aym
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            foreach (var type in typeof(Interaction_CatLore).GetNestedTypes(AccessTools.all))
            {
                if (!type.IsClass)
                    continue;
                if (type.Name.StartsWith("<GiveDemon>"))
                {
                    var mi = type.GetMethod("MoveNext", AccessTools.all, null, [], null);
                    if (mi != null)
                        return mi;
                }
            }
            throw new Exception("Unable to find Interaction_CatLore.<GiveDemon>");
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_KeepBaalAym(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);

            tool.Seek(typeof(FollowerManager), nameof(FollowerManager.RemoveFollowerBrain));
            tool.ReplaceCall(patch);

            return tool;

            static void patch(int ID)
            {
                foreach (var summoner in StructureManager.GetAllStructuresOfType<Structures_Demon_Summoner>())
                {
                    summoner.Data.MultipleFollowerIDs.Remove(ID);
                }
            }
        }
    }
}
