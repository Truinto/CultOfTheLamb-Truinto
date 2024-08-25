using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_Fleece
    {
        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.GetCursesDamageMultiplier))]
        [HarmonyPostfix]
        public static void Postfix1(ref float __result)
        {
            int playerFleece = DataManager.Instance.PlayerFleece;
            if (playerFleece == 2)
                __result = 1f; // this does nothing
        }

        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.GetDamageReceivedMultiplier))]
        [HarmonyPostfix]
        public static void Postfix2(ref float __result)
        {
            if (!Settings.State.disableFleecePenalty) return;
            __result = 0f;
        }

        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.GetLootMultiplier))]
        [HarmonyPostfix]
        public static void Postfix3(ref float __result)
        {
            if (!Settings.State.disableFleecePenalty) return;
            __result = 0f; // this does nothing
        }

        [HarmonyPatch(typeof(DataManager), nameof(DataManager.SetNewRun))]
        [HarmonyPostfix]
        public static void Postfix4()
        {
            if (!Settings.State.disableFleecePenalty) return;
            DataManager.instance.CanFindTarotCards = true;
        }
    }
}
