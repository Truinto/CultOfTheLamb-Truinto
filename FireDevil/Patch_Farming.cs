using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    /// <summary>
    /// Notes:<br/>
    /// Silos do not stack items. Every item is an <see cref="InventoryItem"/> with quantity of 1. See <seealso cref="Interaction_SiloFertilizer"/>.
    /// 
    /// Could make silos infinite see FollowerTask_Farm.DoingBegin "seedtypetoplant".
    /// </summary>
    [HarmonyPatch]
    public static class Patch_Farming
    {
        /// <summary>
        /// Changed farm to prioritise (but not force) seed type.<br/>
        /// Changed farm to take from closest silo, even if prioritising a seed.
        /// </summary>
        [HarmonyPatch(typeof(Structures_SiloSeed), nameof(Structures_SiloSeed.GetClosestSeeder))]
        [HarmonyPrefix]
        public static bool Prefix_GetSeedSiloForFarmPlot(Vector3 fromPosition, FollowerLocation location, InventoryItem.ITEM_TYPE prioritisedSeedType, List<Structures_SiloSeed> silos, ref Structures_SiloSeed __result)
        {
            bool modeStrict = true;

            silos ??= StructureManager.GetAllStructuresOfType<Structures_SiloSeed>(location);
            Structures_SiloSeed closest_silo = null;
            bool check_priority = prioritisedSeedType != 0;
            bool found_priority = false;
            foreach (var silo in silos)
            {
                if (silo.Data.Type == StructureBrain.TYPES.SEED_BUCKET || silo.Data.IsCollapsed || silo.Data.Inventory.Count <= 0)
                    continue;

                bool is_closer = closest_silo == null || Vector3.Distance(silo.Data.Position, fromPosition) < Vector3.Distance(closest_silo.Data.Position, fromPosition);
                if (!check_priority) // if no priority given, just get closest
                {
                    if (is_closer)
                        closest_silo = silo;
                    continue;
                }

                bool has_priority = silo.Data.Inventory.Any(a => a.type == (int)prioritisedSeedType);
                if (has_priority)
                {
                    if (is_closer || !found_priority) // if this is the first priority, ignore distance                                      
                        closest_silo = silo;

                    found_priority = true;
                }
                else if (!found_priority) // non-priority only allowed, if priority not found yet
                {
                    if (is_closer)
                        closest_silo = silo;
                }
            }
            __result = modeStrict && !found_priority && check_priority ? null : closest_silo;
            return false;
        }

        /// <summary>
        /// Changed seed-bucket logic to take priority seeds first, instead of all seeds in deposite order.<br/>
        /// Added option for seed-buckets to keep dispensing seeds/fertiliser, even if empty. (Part 1/3)
        /// </summary>
        [HarmonyPatch(typeof(FollowerTask_Farm), nameof(FollowerTask_Farm.DoingBegin))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_InfiniteSeeds1(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);

            tool.Seek(typeof(Structures_SiloFertiliser), nameof(Structures_SiloFertiliser.RemoveCompostAmount), [typeof(int)]);
            tool.ReplaceCall(removeCompostAmount);

            tool.Seek(typeof(Structures_SiloSeed), nameof(Structures_SiloSeed.GetCompost), [typeof(int)]);
            tool.ReplaceCall(getSeeds);

            tool.Seek(typeof(Structures_SiloSeed), nameof(Structures_SiloSeed.RemoveCompost), [typeof(List<InventoryItem.ITEM_TYPE>)]);
            tool.ReplaceCall(removeSeeds);

            return tool;

            static List<InventoryItem> removeCompostAmount(Structures_SiloFertiliser instance, int amount)
            {
                var list = instance.RemoveCompostAmount(amount);
                if (!Settings.State.siloBucketInfinite)
                    return list;
                amount -= list.Count;
                while (amount-- > 0)
                    list.Add(new(InventoryItem.ITEM_TYPE.POOP, 1));
                return list;
            }

            static List<InventoryItem.ITEM_TYPE> getSeeds(Structures_SiloSeed instance, int amount, [LocalParameter(indexByType: 0)] Structures_SiloSeed emptySeedSilo)
            {
                //var seed = StructureManager.GetStructureByID<Structures_FarmerPlot>(__instance._farmPlotID)?.GetPrioritisedSeedType() ?? InventoryItem.ITEM_TYPE.NONE;
                var seed = GetPrioritisedSeedType(emptySeedSilo);

                if (Settings.State.siloBucketInfinite)
                {
                    var list = new List<InventoryItem.ITEM_TYPE>(amount);
                    if (seed != InventoryItem.ITEM_TYPE.NONE) // return just the requested seed
                    {
                        for (int i = 0; i < amount; i++)
                            list.Add(seed);
                        Main.Print($"Seed bucket infinite prio: {list.Join()}");
                        return list;
                    }

                    var seeds = InventoryItem.AllSeeds;
                    for (int i = 0; i < amount; i++) // return all seeds equally
                    {
                        list.Add(seeds[i % seeds.Count]);
                    }
                    Main.Print($"Seed bucket infinite any: {list.Join()}");
                    return list;
                }

                if (seed != InventoryItem.ITEM_TYPE.NONE)
                {
                    var list = new List<InventoryItem.ITEM_TYPE>();
                    foreach (var item in instance.Data.Inventory) // return requested seeds first
                    {
                        if (item.type == (int)seed)
                        {
                            list.Add((InventoryItem.ITEM_TYPE)item.type);
                            if (list.Count >= amount)
                            {
                                Main.Print($"Seed bucket prio only: {list.Join()}");
                                return list;
                            }
                        }
                    }
                    foreach (var item in instance.Data.Inventory) // return all other seeds
                    {
                        if (item.type != (int)seed)
                        {
                            list.Add((InventoryItem.ITEM_TYPE)item.type);
                            if (list.Count >= amount)
                            {
                                Main.Print($"Seed bucket mixed: {list.Join()}");
                                return list;
                            }
                        }
                    }
                    Main.Print($"Seed bucket all: {list.Join()}");
                    return list;
                }

                Main.Print($"Seed bucket default logic");
                return instance.GetCompost(amount); // default logic
            }

            static void removeSeeds(Structures_SiloSeed instance, List<InventoryItem.ITEM_TYPE> items)
            {
                instance.RemoveCompost(items); // normal execution, works fine even when removing seeds that are not stored
            }
        }

        /// <summary>
        /// Added option for seed-buckets to keep dispensing seeds, even if empty. (Part 2/3)
        /// </summary>
        [HarmonyPatch(typeof(Structures_SiloSeed), nameof(Structures_SiloSeed.GetCompostCount))]
        [HarmonyPrefix]
        public static bool Prefix_InfiniteSeeds2(Structures_SiloSeed __instance, ref int __result)
        {
            if (!Settings.State.siloBucketInfinite || __instance.Data.Type != StructureBrain.TYPES.SEED_BUCKET)
                return true;
            __result = (int)__instance.Capacity;
            return false;
        }

        /// <summary>
        /// Added option for seed-buckets to keep dispensing fertiliser, even if empty. (Part 3/3)
        /// </summary>
        [HarmonyPatch(typeof(Structures_SiloFertiliser), nameof(Structures_SiloFertiliser.GetCompostCount))]
        [HarmonyPrefix]
        public static bool Prefix_InfiniteSeeds3(Structures_SiloFertiliser __instance, ref int __result)
        {
            if (!Settings.State.siloBucketInfinite || __instance.Data.Type != StructureBrain.TYPES.POOP_BUCKET)
                return true;
            __result = (int)__instance.Capacity;
            return false;
        }

        /// <summary>
        /// Gets seed type from closest sign in a 5 unit radius of <paramref name="building"/>.
        /// </summary>
        public static InventoryItem.ITEM_TYPE GetPrioritisedSeedType(StructureBrain building, List<StructureBrain> signs = null)
        {
            if (building == null)
                return InventoryItem.ITEM_TYPE.NONE;

            StructureBrain closest_sign = null;
            float closest_distance = Settings.State.farmSignRadius - 1f;
            foreach (StructureBrain sign in signs ?? StructureManager.GetAllStructuresOfType(StructureBrain.TYPES.FARM_PLOT_SIGN))
            {
                float distance = Vector3.Distance(building.Data.Position, sign.Data.Position);
                //Main.Print($"GetPrioritisedSeedType distance={distance} closest_distance={closest_distance}");
                if (distance < closest_distance)
                {
                    closest_sign = sign;
                    closest_distance = distance;
                }
            }

            if (closest_sign != null && closest_sign.Data.SignPostItem != 0)
                return InventoryItem.GetSeedType(closest_sign.Data.SignPostItem);
            return InventoryItem.ITEM_TYPE.NONE;
        }

        /// <summary>
        /// Changed it so the secondary button on fertilizer silo only takes normal fertilizer, instead of all types.
        /// </summary>
        [HarmonyPatch(typeof(Interaction_SiloFertilizer), nameof(Interaction_SiloFertilizer.OnSecondaryInteract))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_OnlyDepositNormalPoop(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);

            tool.Seek(typeof(Inventory), nameof(Inventory.GetItemQuantity), [typeof(InventoryItem.ITEM_TYPE)]);
            tool.ReplaceCall(patch);

            return tool;

            int patch(InventoryItem.ITEM_TYPE itemType)
            {
                if (itemType is InventoryItem.ITEM_TYPE.POOP_GOLD or InventoryItem.ITEM_TYPE.POOP_GLOW or InventoryItem.ITEM_TYPE.POOP_RAINBOW or InventoryItem.ITEM_TYPE.POOP_DEVOTION)
                    return 0;
                return Inventory.GetItemQuantity(itemType);
            }
        }

        /// <summary>
        /// Apply bonus resources when farming with nature necklace.
        /// </summary>
        [HarmonyPatch(typeof(FollowerTask_Farm), nameof(FollowerTask_Farm.ProgressTask))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_NatureNecklaceFix(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);

            tool.Seek(typeof(Structures_BerryBush), nameof(Structures_BerryBush.GetBerries));
            tool.InsertAfter(patch);

            return tool;

            static List<InventoryItem.ITEM_TYPE> patch(List<InventoryItem.ITEM_TYPE> __stack, FollowerTask_Farm __instance)
            {
                if (__instance._brain != null)
                {
                    float mult = __instance._brain.ResourceHarvestingMultiplier - 1f;
                    if (mult > 0f)
                    {
                        for (int i = __stack.Count - 1; i >= 0; i--)
                        {
                            if (UnityEngine.Random.value < mult)
                                __stack.Add(__stack[i]);
                        }
                    }
                }
                return __stack;
            }
        }
    }

    /// <summary>
    /// Change Farm Station radius
    /// </summary>
    [HarmonyPatch]
    public static class Patch_Farming2
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Structures_FarmerStation), nameof(Structures_FarmerStation.GetNextUnseededPlot));
            yield return AccessTools.Method(typeof(Structures_FarmerStation), nameof(Structures_FarmerStation.GetNextUnwateredPlot));
            yield return AccessTools.Method(typeof(Structures_FarmerStation), nameof(Structures_FarmerStation.GetNextUnfertilizedPlot));
            yield return AccessTools.Method(typeof(Structures_FarmerStation), nameof(Structures_FarmerStation.GetNextUnpickedPlot));
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_FarmStationRadius(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);

            tool.Seek(OpCodes.Ldc_R4, 6f);
            tool.ReplaceCall(patch);
            tool.Seek(OpCodes.Ldc_R4, 6f);
            tool.ReplaceCall(patch);

            return tool;

            static float patch()
            {
                return Settings.State.farmStationRadius;
            }
        }
    }

    /// <summary>
    /// Change Farm Sign radius
    /// </summary>
    [HarmonyPatch]
    public static class Patch_Farming3
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Interaction_FarmPlotSign), nameof(Interaction_FarmPlotSign.OnEnableInteraction));
            yield return AccessTools.Method(typeof(Interaction_FarmPlotSign), nameof(Interaction_FarmPlotSign.Update));
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler_FarmSignRadius(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);

            while (!tool.IsLast)
            {
                if (tool.Is(OpCodes.Ldc_R4, 5f))
                {
                    tool.ReplaceCall(patch);
                }
                tool.Index++;
            }

            return tool;

            static float patch()
            {
                return Settings.State.farmSignRadius;
            }
        }
    }
}
