using Lamb.UI.FollowerInteractionWheel;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_FollowerCommands
    {
        [HarmonyPatch(typeof(FollowerCommandGroups), nameof(FollowerCommandGroups.DefaultCommands))]
        [HarmonyPostfix]
        public static void Default(ref List<CommandItem> __result)
        {
            if (__result.Any(a => a.Command == FollowerCommands.MakeDemand))
            {
                if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.WorkWorship_Intimidate))
                    __result.Insert(Math.Min(2, __result.Count), FollowerCommandItems.Intimidate());
                if (DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.WorkWorship_Inspire))
                    __result.Insert(Math.Min(2, __result.Count), FollowerCommandItems.Dance());
                if (!DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.WorkWorship_Intimidate)
                    && !DoctrineUpgradeSystem.GetUnlocked(DoctrineUpgradeSystem.DoctrineType.WorkWorship_Inspire)
                    && DataManager.Instance.ShowLoyaltyBars)
                    __result.Insert(Math.Min(2, __result.Count), FollowerCommandItems.Bless());
            }
        }

        [HarmonyPatch(typeof(FollowerCommandGroups), nameof(FollowerCommandGroups.MakeDemandCommands))]
        [HarmonyPostfix]
        public static void MakeDemand(ref List<CommandItem> __result)
        {
            if (Settings.State.showMurderAction)
            {
                if (!__result.Any(a => a.Command == FollowerCommands.Murder))
                    __result.Insert(Math.Min(1, __result.Count), FollowerCommandItems.Murder());
            }

            if (Settings.State.hideNecklaceAction)
            {
                __result.RemoveAll(a => a.Command is FollowerCommands.HideNecklace or FollowerCommands.RemoveNecklace);
            }
        }

        [HarmonyPatch(typeof(FollowerCommandGroups), nameof(FollowerCommandGroups.OldAgeCommands))]
        [HarmonyPostfix]
        public static void OldAge(Follower follower, ref List<CommandItem> __result)
        {
            if (Settings.State.showMurderAction)
            {
                if (!__result.Any(a => a.Command == FollowerCommands.Murder))
                    __result.Insert(Math.Min(1, __result.Count), FollowerCommandItems.Murder());
            }

            if (follower.Brain.Info.Necklace != 0)
            {
                __result.Add(FollowerCommandItems.RemoveNecklace(follower.Brain.Info.Necklace));
            }
        }

        [HarmonyPatch(typeof(FollowerCommandGroups), nameof(FollowerCommandGroups.DissenterCommands))]
        [HarmonyPostfix]
        public static void Dissenter(ref List<CommandItem> __result)
        {
            if (Settings.State.showMurderAction)
            {
                if (!__result.Any(a => a.Command == FollowerCommands.Murder))
                    __result.Insert(Math.Min(1, __result.Count), FollowerCommandItems.Murder());
            }
        }

        [HarmonyPatch(typeof(FollowerCommandGroups), nameof(FollowerCommandGroups.DrunkCommands))]
        [HarmonyPostfix]
        public static void Drunk(ref List<CommandItem> __result)
        {
            if (Settings.State.showMurderAction)
            {
                if (!__result.Any(a => a.Command == FollowerCommands.Murder))
                    __result.Insert(Math.Min(1, __result.Count), FollowerCommandItems.Murder());
            }
        }

        [HarmonyPatch(typeof(FollowerCommandGroups), nameof(FollowerCommandGroups.ScaredCommands))]
        [HarmonyPostfix]
        public static void Scared(ref List<CommandItem> __result)
        {
            if (Settings.State.showMurderAction)
            {
                if (!__result.Any(a => a.Command == FollowerCommands.Murder))
                    __result.Insert(Math.Min(1, __result.Count), FollowerCommandItems.Murder());
            }
        }

        [HarmonyPatch(typeof(FollowerCommandGroups), nameof(FollowerCommandGroups.ExistentialDreadCommands))]
        [HarmonyPostfix]
        public static void Dread(ref List<CommandItem> __result)
        {
            if (Settings.State.showMurderAction)
            {
                if (!__result.Any(a => a.Command == FollowerCommands.Murder))
                    __result.Insert(Math.Min(1, __result.Count), FollowerCommandItems.Murder());
            }
        }

        [HarmonyPatch(typeof(FollowerCommandGroups), nameof(FollowerCommandGroups.SpyCommands))]
        [HarmonyPostfix]
        public static void Spy(ref List<CommandItem> __result)
        {
            if (Settings.State.showMurderAction)
            {
                if (!__result.Any(a => a.Command == FollowerCommands.Murder))
                    __result.Insert(Math.Min(1, __result.Count), FollowerCommandItems.Murder());
            }
        }

        //[HarmonyPatch(typeof(FollowerCommandGroups), nameof(FollowerCommandGroups.WakeUpCommands))]
        //[HarmonyPostfix]
        //public static void Asleep(ref List<CommandItem> __result)
        //{
        //    if (Settings.State.showMurderAction) // this will soft lock
        //    {
        //        if (!__result.Any(a => a.Command == FollowerCommands.Murder))
        //            __result.Insert(Math.Min(1, __result.Count), FollowerCommandItems.Murder());
        //    }
        //}
    }

    [HarmonyPatch]
    public static class Patch_FollowerCommands2
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var type in typeof(interaction_FollowerInteraction).GetNestedTypes(AccessTools.all))
            {
                if (!type.IsClass)
                    continue;
                if (type.Name.StartsWith("<DanceRoutine>"))
                {
                    var mi = type.GetMethod("MoveNext", AccessTools.all, null, [], null);
                    if (mi != null)
                        yield return mi;
                }
                else if (type.Name.StartsWith("<BlessRoutine>"))
                {
                    var mi = type.GetMethod("MoveNext", AccessTools.all, null, [], null);
                    if (mi != null)
                        yield return mi;
                }
                else if (type.Name.StartsWith("<IntimidateRoutine>"))
                {
                    var mi = type.GetMethod("MoveNext", AccessTools.all, null, [], null);
                    if (mi != null)
                        yield return mi;
                }
            }
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);
            tool.Seek(OpCodes.Ldc_R4, 3f);
            tool.InsertAfter(patch);
            return tool;

            static float patch(float __stack)
            {
                return Settings.State.blessSuperRange ? 30f : 3f;
            }
        }
    }
}
