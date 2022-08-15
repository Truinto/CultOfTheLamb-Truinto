using Lamb.UI.FollowerInteractionWheel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_MurderAction
    {
        [HarmonyPatch(typeof(FollowerCommandGroups), nameof(FollowerCommandGroups.MakeDemandCommands))]
        [HarmonyPostfix]
        public static void Postfix1(ref List<CommandItem> __result)
        {
            if (!Settings.State.showMurderAction)
                return;

            if (!__result.Any(a => a.Command == FollowerCommands.Murder))
                __result.Insert(1, FollowerCommandItems.Murder());
        }

        [HarmonyPatch(typeof(FollowerCommandGroups), nameof(FollowerCommandGroups.DissenterCommands))]
        [HarmonyPostfix]
        public static void Postfix2(ref List<CommandItem> __result)
        {
            if (!Settings.State.showMurderAction)
                return;

            if (!__result.Any(a => a.Command == FollowerCommands.Murder))
                __result.Insert(2, FollowerCommandItems.Murder());
        }

        [HarmonyPatch(typeof(FollowerCommandGroups), nameof(FollowerCommandGroups.OldAgeCommands))]
        [HarmonyPostfix]
        public static void Postfix3(ref List<CommandItem> __result)
        {
            if (!Settings.State.showMurderAction)
                return;

            if (!__result.Any(a => a.Command == FollowerCommands.Murder))
                __result.Insert(1, FollowerCommandItems.Murder());
        }
    }
}
