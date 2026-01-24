using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch(typeof(Structures_LumberjackStation), nameof(Structures_LumberjackStation.IncreaseAge))]
    public static class Patch_LumberMine
    {
        public static bool Prefix(Structures_LumberjackStation __instance)
        {
            return __instance.Data.Type switch
            {
                StructureBrain.TYPES.BLOODSTONE_MINE or StructureBrain.TYPES.LUMBERJACK_STATION => !Settings.State.infiniteMineI,
                StructureBrain.TYPES.BLOODSTONE_MINE_2 or StructureBrain.TYPES.LUMBERJACK_STATION_2 => !Settings.State.infiniteMineII,
                _ => true,
            };
        }
    }
}
