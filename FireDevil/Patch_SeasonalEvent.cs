using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch(typeof(SeasonalEventManager), nameof(SeasonalEventManager.IsSeasonalEventActive))]
    public static class Patch_SeasonalEvent
    {
        public static void Postfix(SeasonalEventType eventType, ref bool __result)
        {
            var forced = Settings.State.forceSeasonalEvent;
            if (forced != SeasonalEventType.None)
                __result = eventType == forced;
        }
    }
}
