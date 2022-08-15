using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch(typeof(Structures_LumberjackStation), nameof(Structures_LumberjackStation.IncreaseAge))]
    public class Patch_LumberMine
    {
        public static bool Prefix(Structures_LumberjackStation __instance)
        {
            var type = __instance.Data.Type;
            if (type == StructureBrain.TYPES.BLOODSTONE_MINE_2 || type == StructureBrain.TYPES.LUMBERJACK_STATION_2)
                return false;
            return true;
        }
    }
}
