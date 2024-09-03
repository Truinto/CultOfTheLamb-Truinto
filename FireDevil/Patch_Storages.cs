using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_Storages
    {
        /// <summary>
        /// Stores items directly to player inventory, instead of structure inventory.
        /// </summary>
        [HarmonyPatch(typeof(StructureBrain), nameof(StructureBrain.DepositItem))]
        public static bool Prefix_RedirectPlayerInventory(InventoryItem.ITEM_TYPE type, int quantity, StructureBrain __instance)
        {
            if (!Settings.State.redirectPlayerInventory)
                return true;

            if (__instance is Structures_FarmerStation or Structures_Refinery or Structures_OfferingShrine)
            {
                Inventory.AddItem((int)type, quantity, true);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Structures_Shrine), nameof(Structures_Shrine.SoulMax), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Postfix1(ref int __result)
        {
            __result = (int)(__result * Settings.State.storageShrineMult);
        }

        [HarmonyPatch(typeof(Structures_Outhouse), nameof(Structures_Outhouse.Capacity))]
        [HarmonyPostfix]
        public static void Postfix2(ref int __result)
        {
            __result = (int)(__result * Settings.State.storageOuthouseMult);
        }

        [HarmonyPatch(typeof(LumberjackStation), nameof(LumberjackStation.Update))]
        [HarmonyPrefix]
        public static void InstantMine(LumberjackStation __instance)
        {
            if (!__instance.Activating || !Settings.State.instantPickup)
                return;

            var inventory = __instance.StructureInfo.Inventory;
            if (inventory.Count < 2)
                return;

            var item = inventory[0];
            Inventory.AddItem(item.type, inventory.Count - 1, false);
            inventory.Clear();
            inventory.Add(item);
        }

        [HarmonyPatch(typeof(Structures_SiloSeed), nameof(Structures_SiloSeed.Capacity), MethodType.Getter)]
        [HarmonyPrefix]
        public static void Silo1(ref float __result)
        {
            __result *= Settings.State.storageSiloMult;
        }

        [HarmonyPatch(typeof(Structures_SiloFertiliser), nameof(Structures_SiloFertiliser.Capacity), MethodType.Getter)]
        [HarmonyPrefix]
        public static void Silo2(ref float __result)
        {
            __result *= Settings.State.storageSiloMult;
        }

        [HarmonyPatch(typeof(Structures_CompostBin), nameof(Structures_CompostBin.AddPoop), typeof(int))]
        [HarmonyPrefix]
        public static void Compost(ref int amount)
        {
            amount = (int)(amount * Settings.State.compostMult);
        }

        [HarmonyPatch(typeof(Structures_CompostBin), nameof(Structures_CompostBin.CompostCost), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Compost2(ref int __result)
        {
            __result = Settings.State.compostCost;
        }
    }
}
