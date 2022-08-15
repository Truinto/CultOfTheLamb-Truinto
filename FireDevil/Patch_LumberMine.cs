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
            switch (__instance.Data.Type)
            {
                case StructureBrain.TYPES.BLOODSTONE_MINE:
                case StructureBrain.TYPES.LUMBERJACK_STATION:
                    return !Settings.State.infiniteMineI;

                case StructureBrain.TYPES.BLOODSTONE_MINE_2:
                case StructureBrain.TYPES.LUMBERJACK_STATION_2:
                    return !Settings.State.infiniteMineII;

                default:
                    return true;
            }
        }
    }
}
