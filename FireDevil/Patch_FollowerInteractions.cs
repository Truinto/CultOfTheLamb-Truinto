using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_FollowerInteractions
    {
        [HarmonyPatch(typeof(FollowerBrain), nameof(FollowerBrain.GetPersonalTask))]
        [HarmonyPostfix]
        public static void Patch_ZombiesWork(FollowerBrain __instance, ref FollowerTask __result)
        {
            if (!Settings.State.zombiesDoWork)
                return;
            if (__result is FollowerTask_Zombie)
                __result = null;
            else if (__result is FollowerTask_Sleep && __instance.HasTrait(FollowerTrait.TraitType.Zombie))
                __result = null;            
        }

        [HarmonyPatch(typeof(FollowerState_Zombie), nameof(FollowerState_Zombie.MaxSpeed), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Patch_ZombiesWalk(ref float __result)
        {
            if (!Settings.State.zombiesDoWork)
                return;
            __result = 1f;
        }

        /// <summary>
        /// make followers with loyality necklace never start a fight
        /// </summary>
        [HarmonyPatch(typeof(FollowerBrain), nameof(FollowerBrain.CheckForInteraction), typeof(FollowerBrain), typeof(float))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Patch_Loyality(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);
            tool.Seek(typeof(DataManager), nameof(DataManager.TimeSinceLastFollowerFight));
            tool.InsertAfter(patch);
            tool.Seek(typeof(DataManager), nameof(DataManager.TimeSinceLastFollowerEaten));
            tool.InsertAfter(patch);
            return tool;

            static float patch(float __stack, FollowerBrain __instance)
            {
                if (__instance.Info.Necklace == InventoryItem.ITEM_TYPE.Necklace_Loyalty)
                    return float.PositiveInfinity;
                return __stack;
            }
        }

        /// <summary>
        /// make zombies not eat, if wearing loyalty necklace or victim is immortal
        /// </summary>
        [HarmonyPatch(typeof(FollowerTask_FightFollower), nameof(FollowerTask_FightFollower.OnStart))]
        [HarmonyPostfix]
        public static void Patch_ZombieDontEat(FollowerTask_FightFollower __instance)
        {
            if (__instance.isEatingOtherFollower)
            {
                FollowerBrainInfo zombie;
                FollowerBrainInfo victim;

                if (__instance.Brain.Info.HasTrait(FollowerTrait.TraitType.Zombie) || __instance.Brain.Info.ID == FollowerManager.SozoID)
                {
                    zombie = __instance.Brain.Info;
                    victim = __instance.OtherChatTask.Brain.Info;
                }
                else if (__instance.OtherChatTask.Brain.Info.HasTrait(FollowerTrait.TraitType.Zombie) || __instance.OtherChatTask.Brain.Info.ID == FollowerManager.SozoID)
                {
                    zombie = __instance.OtherChatTask.Brain.Info;
                    victim = __instance.Brain.Info;
                }
                else
                    return;

                if (zombie.Necklace == InventoryItem.ITEM_TYPE.Necklace_Loyalty)
                    __instance.isEatingOtherFollower = false;

                if (victim.HasTrait(FollowerTrait.TraitType.Immortal))
                    __instance.isEatingOtherFollower = false;
            }
        }

        /// <summary>
        /// remove zombie groan
        /// </summary>
        [HarmonyPatch(typeof(FollowerTask_Zombie), nameof(FollowerTask_Zombie.Setup))]
        [HarmonyPostfix]
        public static void Patch_ZombieBeQuiet(FollowerTask_Zombie __instance)
        {
            AudioManager.Instance.StopLoop(__instance.loopingSound);
        }
    }
}
