using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public class Patch_RitualCost
    {
        [HarmonyPatch(typeof(UpgradeSystem), nameof(UpgradeSystem.ApplyRitualDiscount))]
        [HarmonyPostfix]
        public static void Postfix1(ref List<StructuresData.ItemCost> __result)
        {
            foreach (StructuresData.ItemCost item in __result)
            {
                if (item.CostItem != InventoryItem.ITEM_TYPE.FOLLOWERS)
                {
                    item.CostValue = Mathf.Max(1, Mathf.FloorToInt(item.CostValue * Settings.State.ritualCostMult));
                }
            }
        }

        [HarmonyPatch(typeof(UpgradeSystem), nameof(UpgradeSystem.AddCooldown))]
        [HarmonyPostfix]
        public static void Prefix2(Type type, ref float duration)
        {
            duration *= Settings.State.ritualCooldownMult;
        }
    }
}
