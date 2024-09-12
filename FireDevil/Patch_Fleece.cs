using MMBiomeGeneration;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PlayerFleeceManager;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_Fleece
    {
        /// <summary>
        /// FleeceType.Gold (1)
	    /// +100% damage taken
        /// </summary>
        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.GetDamageReceivedMultiplier))]
        [HarmonyPostfix]
        public static void Postfix_1(ref float __result)
        {
            if (!Settings.State.disableFleecePenalty) return;
            __result = 0f;
        }

        /// <summary>
        /// FleeceType.Green (2)
        /// -50% health
        /// </summary>
        [HarmonyPatch(typeof(DifficultyManager), nameof(DifficultyManager.ForceDifficulty), typeof(DifficultyManager.Difficulty))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_2_1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);
            tool.Seek(typeof(DataManager), nameof(DataManager.PlayerFleece));
            tool.InsertAfter(patch);
            return tool;

            static int patch(int __stack)
            {
                if (Settings.State.disableFleecePenalty)
                    return 0;
                return __stack;
            }
        }

        /// <summary>
        /// Fix for FleeceType.Green (2)
        /// </summary>
        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.GetCursesDamageMultiplier))]
        [HarmonyPrefix]
        public static bool Prefix_2_2(ref float __result)
        {
            __result = (FleeceType)DataManager.Instance.PlayerFleece switch
            {
                FleeceType.Gold => 1f + PlayerFleeceManager.damageMultiplier,
                FleeceType.Green => 2f,
                FleeceType.CurseInsteadOfWeapon => Settings.State.disableFleecePenalty ? 1f : 0.75f,
                FleeceType.OneHitKills => 11f,
                _ => 1f,
            };
            return false;
        }

        /// <summary>
        /// FleeceType.Green (2)
        /// -50% weapon damage
        /// </summary>
        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.GetWeaponDamageMultiplier))]
        [HarmonyPrefix]
        public static bool Prefix_2_3(ref float __result)
        {
            __result = (FleeceType)DataManager.Instance.PlayerFleece switch
            {
                FleeceType.Gold => PlayerFleeceManager.damageMultiplier,
                FleeceType.Green => Settings.State.disableFleecePenalty ? 0f : -0.5f,
                FleeceType.OneHitKills => 10f,
                _ => 0f,
            };
            return false;
        }

        /// <summary>
        /// FleeceType.Purple (3)
        /// can get more black hearts
        /// </summary>
        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.OnTarotCardPickedUp))]
        [HarmonyPrefix]
        public static bool Prefix_3_1(PlayerFarming playerFarming)
        {
            if (DataManager.Instance.PlayerFleece == 3)
            {
                if (playerFarming.health.BlackHearts <= 1f)
                    playerFarming.health.BlackHearts = 2f;
                else
                    playerFarming.health.BlackHearts += 1f;
            }
            return false;
        }

        /// <summary>
        /// FleeceType.Purple (3)
        /// poisoned on hit
        /// </summary>
        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.FleeceCausesPoisonOnHit))]
        [HarmonyPrefix]
        public static bool Prefix_3_2(ref bool __result)
        {
            __result = false;
            return !Settings.State.disableFleecePenalty;
        }

        /// <summary>
        /// FleeceType.White (4)
        /// cannot pick up cards
        /// </summary>
        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.FleecePreventTarotCards))]
        [HarmonyPrefix]
        public static bool Prefix_4(ref bool __result)
        {
            __result = false;
            return !Settings.State.disableFleecePenalty;
        }

        /// <summary>
        /// FleeceType.OneHitKills (7)
        /// one heart only
        /// </summary>
        [HarmonyPatch(typeof(HealthPlayer), nameof(HealthPlayer.InitHP))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_7_1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);
            tool.Seek(typeof(DataManager), nameof(DataManager.PlayerFleece));
            tool.InsertAfter(patch);
            return tool;

            static int patch(int __stack)
            {
                if (Settings.State.disableFleecePenalty)
                    return 0;
                return __stack;
            }
        }

        /// <summary>
        /// FleeceType.OneHitKills (7)
        /// one heart only
        /// </summary>
        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.FleecePreventsHealthPickups))]
        [HarmonyPrefix]
        public static bool Prefix_7_2(ref bool __result)
        {
            __result = false;
            return !Settings.State.disableFleecePenalty;
        }

        /// <summary>
        /// FleeceType.OneHitKills (7); FleeceType.Blue (5)
        /// one heart only
        /// </summary>
        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.FleeceNoRedHeartsToUse))]
        [HarmonyPrefix]
        public static bool Prefix_7_3(ref bool __result)
        {
            __result = false;
            return !Settings.State.disableFleecePenalty;
        }

        /// <summary>
        /// FleeceType.OneHitKills (7)
        /// one heart only
        /// </summary>
        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.FleecePreventsRespawn))]
        [HarmonyPrefix]
        public static bool Prefix_7_4(ref bool __result)
        {
            __result = false;
            return !Settings.State.disableFleecePenalty;
        }

        /// <summary>
        /// FleeceType.NoRolling (9)
        /// cannot roll
        /// </summary>
        [HarmonyPatch(typeof(PlayerFleeceManager), nameof(PlayerFleeceManager.FleecePreventsRoll))]
        [HarmonyPrefix]
        public static bool Prefix_9(ref bool __result)
        {
            __result = false;
            return !Settings.State.disableFleecePenalty;
        }

        /// <summary>
        /// FleeceType.BlindFaith (11)
        /// map disabled
        /// </summary>
        [HarmonyPatch(typeof(ActivateMiniMap), nameof(ActivateMiniMap.OnEnable))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_11(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);
            tool.Seek(typeof(DataManager), nameof(DataManager.PlayerFleece));
            tool.InsertAfter(patch);
            return tool;

            static int patch(int __stack)
            {
                if (Settings.State.disableFleecePenalty)
                    return 0;
                return __stack;
            }
        }
    }

    [HarmonyPatch]
    public static class Patch_Fleece2
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            foreach (var type in typeof(BiomeGenerator).GetNestedTypes(AccessTools.all))
            {
                if (!type.IsClass)
                    continue;
                if (type.Name.StartsWith("<ApplyCurrentDungeonModifiersIE>")) // Fleece 7
                {
                    var mi = type.GetMethod("MoveNext", AccessTools.all, null, [], null);
                    if (mi != null)
                        return mi;
                }
            }
            throw new Exception("Unable to find BiomeGenerator.<ApplyCurrentDungeonModifiersIE>");
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);
            tool.Seek(typeof(DataManager), nameof(DataManager.PlayerFleece));
            tool.InsertAfter(patch);
            return tool;

            static int patch(int __stack)
            {
                if (Settings.State.disableFleecePenalty)
                    return 0;
                return __stack;
            }
        }
    }
}
