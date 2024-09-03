using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_Cooking
    {
        /// <summary>
        /// Remove max from satiation clamp.
        /// </summary>
        [HarmonyPatch(typeof(FollowerTask_Sleep), nameof(FollowerTask_Sleep.UpdateSleepChanges))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> SleepHunger(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);
            tool.Seek(a => a.Calls(AccessTools.PropertySetter(typeof(FollowerBrainStats), nameof(FollowerBrainStats.Satiation))));
            tool--;
            tool.ReplaceCall(patch);
            return tool;

            static float patch(float value, float min, float max)
            {
                return Mathf.Max(min, value);
            }
        }

        /// <summary>
        /// Feast adds 100 satiation, instead of setting to 100.
        /// </summary>
        [HarmonyPatch(typeof(FollowerTask_EatFeastTable), nameof(FollowerTask_EatFeastTable.OnEnd))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FeastHunger(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            var tool = new TranspilerTool(instructions, generator, original);
            tool.Seek(a => a.Calls(AccessTools.PropertySetter(typeof(FollowerBrainStats), nameof(FollowerBrainStats.Satiation))));
            tool.ReplaceCall(patch);
            return tool;

            static void patch(FollowerBrainStats instance, float value)
            {
                instance.Satiation += value;
            }
        }

        [HarmonyPatch(typeof(FollowerTask_Cook), nameof(FollowerTask_Cook.MealFinishedCooking))]
        [HarmonyPrefix]
        public static void FinishedCooking(FollowerTask_Cook __instance)
        {
            if (!Settings.State.autoAddCookingTask)
                return;

            if (__instance.kitchenStructure is not Structures_Kitchen kitchen)
                return;

            again:
            if (kitchen.Data.QueuedMeals.Count >= 5)
                return;

            foreach (var recipe in Recipes)
            {
                if (!DataManager.Instance.RecipesDiscovered.Contains(recipe.Meal))
                    continue;
                if (recipe.Ingredients.Any(a => Inventory.GetItemQuantity(a.type) < a.quantity))
                    continue;

                foreach (InventoryItem item in recipe.Ingredients)
                    Inventory.ChangeItemQuantity(item.type, -item.quantity);

                kitchen.Data.QueuedMeals.Add(new Interaction_Kitchen.QueuedMeal
                {
                    MealType = recipe.Meal,
                    CookingDuration = CookingData.GetMealCookDuration(recipe.Meal),
                    Ingredients = recipe.Ingredients,
                });
                Main.Print($"Auto cook: {recipe.Meal}");
                goto again; // we added a cooking task, so we can try add another
            }
        }

        public readonly struct Recipe(InventoryItem.ITEM_TYPE meal, CookingData.MealEffect[] effects, List<InventoryItem> ingredients)
        {
            public readonly InventoryItem.ITEM_TYPE Meal = meal;
            public readonly CookingData.MealEffect[] Effects = effects;
            public readonly List<InventoryItem> Ingredients = ingredients;
        }

        private static Recipe[] _Recipes;
        public static Recipe[] Recipes
        {
            get
            {
                if (_Recipes == null)
                {
                    var meals = CookingData.GetAllMeals();
                    _Recipes = new Recipe[meals.Length];
                    int size = 0;
                    for (int i = 0; i < meals.Length; i++)
                    {
                        if (meals[i] is InventoryItem.ITEM_TYPE.MEAL_DEADLY)
                            continue;

                        _Recipes[size++] = new(
                            meal: meals[i],
                            effects: CookingData.GetMealEffects(meals[i]),
                            ingredients: CookingData.GetRecipe(meals[i])[0]);
                    }
                    Array.Resize(ref _Recipes, size);
                    Array.Sort(_Recipes, compare);
                    Main.Print($"Cooking order: {_Recipes.Join(a => $"{a.Meal}")}");
                }
                return _Recipes;

                static int compare(Recipe t1, Recipe t2)
                {
                    return score(t1) - score(t2);
                }

                static int score(Recipe recipe)
                {
                    int score = recipe.Meal switch
                    {
                        InventoryItem.ITEM_TYPE.MEAL_GREAT => -20,
                        InventoryItem.ITEM_TYPE.MEAL_GREAT_MIXED => -15,
                        InventoryItem.ITEM_TYPE.MEAL_GREAT_MEAT => -10,
                        InventoryItem.ITEM_TYPE.MEAL_GREAT_FISH => -5,
                        InventoryItem.ITEM_TYPE.MEAL_GOOD_FISH => 0,
                        InventoryItem.ITEM_TYPE.MEAL_MEDIUM_VEG => 0,
                        InventoryItem.ITEM_TYPE.MEAL => 0,
                        InventoryItem.ITEM_TYPE.MEAL_BAD_MIXED => 1,
                        InventoryItem.ITEM_TYPE.MEAL_BERRIES => 2,
                        InventoryItem.ITEM_TYPE.MEAL_EGG => 3,
                        InventoryItem.ITEM_TYPE.MEAL_BAD_FISH => 4,
                        InventoryItem.ITEM_TYPE.MEAL_BAD_MEAT => 5,
                        InventoryItem.ITEM_TYPE.MEAL_MEDIUM_MIXED => 6,
                        InventoryItem.ITEM_TYPE.MEAL_MEAT => 7,
                        InventoryItem.ITEM_TYPE.MEAL_GRASS => 8,
                        InventoryItem.ITEM_TYPE.MEAL_POOP => 9,
                        InventoryItem.ITEM_TYPE.MEAL_FOLLOWER_MEAT => 10,
                        _ => 0,
                    };
                    score += recipe.Effects.FirstOrDefault(f => f.MealEffectType is CookingData.MealEffectType.CausesIllness or CookingData.MealEffectType.CausesIllPoopy or CookingData.MealEffectType.CausesExhaustion).Chance;
                    return score;
                }
            }
        }
    }
}
