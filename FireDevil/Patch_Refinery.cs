namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_Refinery
    {
        [HarmonyPatch(typeof(Structures_Refinery), nameof(Structures_Refinery.RefineryDeposit))]
        [HarmonyPrefix]
        public static void FinishedRefining(Structures_Refinery __instance)
        {
            if (!Settings.State.autoAddCookingTask)
                return;

            again:
            if (__instance.Data.QueuedResources.Count >= 5)
                return;

            Recipe last = default;
            int num_last = int.MaxValue;
            foreach (var recipe in Recipes)
            {
                int num_source = Inventory.GetItemQuantity(recipe.Ingredient.CostItem);
                int num_target = Inventory.GetItemQuantity(recipe.Refinable);
                num_target += __instance.GetItemQuantity(recipe.Refinable);

                if (num_source < num_target) // only refine if same amount of ingredient is left
                    continue;
                if (num_source < recipe.Ingredient.CostValue)
                    continue;
                if (num_target < num_last)
                {
                    last = recipe;
                    num_last = num_target;
                }
            }
            if (last.Refinable is not 0)
            {
                Inventory.ChangeItemQuantity((int)last.Ingredient.CostItem, -last.Ingredient.CostValue);
                __instance.Data.QueuedResources.Add(last.Refinable);
                goto again;
            }
        }

        public readonly struct Recipe(InventoryItem.ITEM_TYPE refinable)
        {
            public readonly InventoryItem.ITEM_TYPE Refinable = refinable;
            public readonly StructuresData.ItemCost Ingredient = Structures_Refinery.GetCost(refinable)[0];
        }

        public static Recipe[] Recipes = [
                new Recipe(InventoryItem.ITEM_TYPE.LOG_REFINED),
                new Recipe(InventoryItem.ITEM_TYPE.LOG_REFINED),
                new Recipe(InventoryItem.ITEM_TYPE.STONE_REFINED),
                new Recipe(InventoryItem.ITEM_TYPE.GOLD_REFINED),
                new Recipe(InventoryItem.ITEM_TYPE.SILK_THREAD),
            ];
    }
}
