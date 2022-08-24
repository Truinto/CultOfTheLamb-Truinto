using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public class Patch_Storages
    {
        [HarmonyPatch(typeof(Structures_Shrine), nameof(Structures_Shrine.SoulMax), MethodType.Getter)]
        [HarmonyPostfix]
        public static void Postfix1(ref int __result)
        {
            __result = (int)(__result * Settings.State.storageShrine);
        }

        [HarmonyPatch(typeof(Structures_Outhouse), nameof(Structures_Outhouse.Capacity))]
        [HarmonyPostfix]
        public static void Postfix2(ref int __result)
        {
            __result = (int)(__result * Settings.State.storageOuthouse);
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


        [HarmonyPatch(typeof(Interaction_SiloSeeder), nameof(Interaction_SiloSeeder.OnInteract))]
        [HarmonyPrefix]
        public static void Silo1(Interaction_SiloSeeder __instance)
        {
            __instance.StructureBrain.Capacity = Settings.State.storageSilo * 15f;
        }

        [HarmonyPatch(typeof(Interaction_SiloFertilizer), nameof(Interaction_SiloFertilizer.OnInteract))]
        [HarmonyPrefix]
        public static void Silo2(Interaction_SiloFertilizer __instance)
        {
            __instance.StructureBrain.Capacity = Settings.State.storageSilo * 15f;
        }

        [HarmonyPatch(typeof(Structures_SiloSeed), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void Silo3(Structures_SiloSeed __instance)
        {
            __instance.Capacity = Settings.State.storageSilo * 15f;
        }

        [HarmonyPatch(typeof(Structures_SiloFertiliser), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void Silo4(Structures_SiloFertiliser __instance)
        {
            __instance.Capacity = Settings.State.storageSilo * 15f;
        }

        [HarmonyPatch(typeof(Structures_CompostBin), nameof(Structures_CompostBin.AddPoop))]
        [HarmonyPrefix]
        public static void Compost(Structures_CompostBin __instance)
        {
            __instance.PoopCount += (int)(__instance.PoopToCreate * Settings.State.extraCompost);
        }


    }
}
