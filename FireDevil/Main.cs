global using HarmonyLib;
global using Newtonsoft.Json;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Reflection;
global using System.Reflection.Emit;
global using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using UnityModManagerNet;
using BepInEx;

namespace FireDevil
{
    public static class Main
    {
        #region Fields
        public const string version = "2.0.3";
        internal static ILogger logger;
        public static string ModPath;
        public static Harmony harmony;

        private static FieldInfo IntRange_Min = typeof(IntRange).GetField("<Min>k__BackingField", AccessTools.all);
        private static FieldInfo IntRange_Max = typeof(IntRange).GetField("<Max>k__BackingField", AccessTools.all);
        #endregion

        public static void Load()
        {
            ModPath ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            harmony ??= new Harmony("Truinto.FireDevil");
#if DEBUG
            //PatchSafe(typeof(Patch_GetTasks));
#endif
            PatchSafe(typeof(Patch_Weapons));
            PatchSafe(typeof(Patch_FollowerCommands));
            PatchSafe(typeof(Patch_FollowerCommands2));
            PatchSafe(typeof(Patch_LumberMine));
            PatchSafe(typeof(Patch_FollowerInfo));
            PatchSafe(typeof(Patch_Fleece));
            PatchSafe(typeof(Patch_Fleece2));
            PatchSafe(typeof(Patch_Storages));
            PatchSafe(typeof(Patch_Adoration));
            PatchSafe(typeof(Patch_RitualCost));
            PatchSafe(typeof(Patch_Farming));
            PatchSafe(typeof(Patch_Farming2));
            PatchSafe(typeof(Patch_Farming3));
            PatchSafe(typeof(Patch_Cooking));
            PatchSafe(typeof(Patch_Refinery));
            PatchSafe(typeof(Patch_FollowerInteractions));
            PatchSafe(typeof(Patch_Baal_Aym));
            PatchSafe(typeof(Patch_SeasonalEvent));
            PatchSafe(typeof(Patch_Mating));
#if !DEBUG
            var nullFinalizer = new HarmonyMethod(AccessTools.Method(typeof(Main), nameof(Main.NullFinalizer)));
            foreach (var patch in harmony.GetPatchedMethods().ToArray())
            {
                if (Harmony.GetPatchInfo(patch).Finalizers.Count == 0)
                {
                    harmony.Patch(patch, finalizer: nullFinalizer);
                    Print("Applied finalizer to " + patch.Name);
                }
            }
#endif
            CallSafe(Patch_Cooking.OnLoad);
            CallSafe(TimeManagment.OnLoad);
            CallSafe(UpdateStaticSettings);
        }

        public static void UpdateStaticSettings()
        {
            HarvestTotem.EFFECTIVE_DISTANCE = Settings.State.harvestTotemRadius;
            Structures_PropagandaSpeaker.EFFECTIVE_DISTANCE = Settings.State.propagandaSpeakerRadius;

            //IntRange_Min?.SetValue(MissionaryManager.SeedRange, 10); // todo
            //IntRange_Max?.SetValue(MissionaryManager.SeedRange, 16);
        }

        #region Helper

        public static void Print(string msg)
        {
            logger?.Log(msg);
        }

        internal static void PatchSafe(Type patch)
        {
            try
            {
                Print("Patching " + patch.Name);
                harmony.CreateClassProcessor(patch).Patch();
            }
            catch (Exception e) { Print(e.ToString()); }
        }

        internal static void CallSafe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e) { Print(e.ToString()); }
        }

        internal static Exception NullFinalizer(Exception __exception)
        {
#if !DEBUG
            return null;
#else
            if (__exception == null)
                return null;
            try
            {
                Print(__exception.ToString());
            }
            catch (Exception) { }
            return null;
#endif
        }

        #endregion
    }
#if UMM
    public static class Main_UMM
    {
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnHideGUI = OnHideGUI;

            Main.logger = new Logger_UMM(modEntry.Logger);
            Main.ModPath = modEntry.Path;
            Main.harmony = new Harmony("Truinto.FireDevil");

            Main.PatchSafe(typeof(Patch_Lockstate));
            Main.Load();

            return true;
        }

        private static GUIStyle StyleBox;
        private static GUIStyle StyleLine;
        private static GUILayoutOption DontExpand = GUILayout.ExpandWidth(false);
        private static bool[] Toggles = new bool[5];
        private static int LastTrait;
        private static Regex RxClean = new("<.*?>");
        private static FollowerTrait.TraitType[] _AllTraits;
        public static FollowerTrait.TraitType[] AllTraits
        {
            get
            {
                if (_AllTraits == null)
                {
                    var values = (FollowerTrait.TraitType[])Enum.GetValues(typeof(FollowerTrait.TraitType));
                    _AllTraits = new FollowerTrait.TraitType[values.Length - 1];
                    Array.Copy(values, 1, _AllTraits, 0, _AllTraits.Length);
                }
                return _AllTraits;
            }
        }
        public static readonly InventoryItem.ITEM_TYPE[] Necklaces = InventoryItem.ItemsThatCanBeGivenToFollower.Where(w => w.ToString().StartsWith("Necklace_")).ToArray();

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (StyleBox == null)
            {
                StyleBox = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter };
                StyleLine = new GUIStyle() { fixedHeight = 1, margin = new RectOffset(0, 0, 4, 4), };
                StyleLine.normal.background = new Texture2D(1, 1);
            }

            if (Folder(ref Toggles[0], "Settings"))
            {
                GUILayout.Space(5);
                //Checkbox(ref Settings.State.globalTaskPatch, "Injects new task types");
                Checkbox(ref Settings.State.hideNecklaceAction, "Hide necklace actions");
                Checkbox(ref Settings.State.autoAddCookingTask, "Automatically add cooking tasks");
                Checkbox(ref Settings.State.showMurderAction, "Always show murder action");
                Checkbox(ref Settings.State.showFollowerRole, "Show follower role in thoughts");
                Checkbox(ref Settings.State.infiniteMineI, "Infinite Lumber and Stone Mine");
                Checkbox(ref Settings.State.infiniteMineII, "Infinite Lumber and Stone Mine Level 2");
                Checkbox(ref Settings.State.instantPickup, "Instantly pickup mine resources");
                Checkbox(ref Settings.State.redirectPlayerInventory, "Redirect items to player inventory (farm, refinery)");
                Checkbox(ref Settings.State.hideItemPopup, "Hide popup when item is collected");
                Checkbox(ref Settings.State.disableFleecePenalty, "Disable most fleece penalties");
                Checkbox(ref Settings.State.loyaltyOverflow, "Allow follower loyalty overflow to next level");
                Checkbox(ref Settings.State.freeHeavyAttack, "Heavy attacks consume no fervour");
                Checkbox(ref Settings.State.siloBucketInfinite, "Seed bucket gives out infinte seeds even if empty");
                Checkbox(ref Settings.State.blessSuperRange, "Bless has 10x bigger radius");
                Checkbox(ref Settings.State.zombiesDoWork, "Zombies do work");

                NumberField(ref Settings.State.goldenEggChance, "Golden egg chance");
                NumberField(ref Settings.State.storageShrineMult, "Shrine soul maximum multiplier");
                NumberField(ref Settings.State.storageOuthouseMult, "Outhouse maximum multiplier");
                NumberField(ref Settings.State.storageSiloMult, "Silo maximum multiplier");
                NumberField(ref Settings.State.compostMult, "Compost multiplier");
                NumberField(ref Settings.State.compostCost, "Compost cost (grass)");
                NumberField(ref Settings.State.ritualCostMult, "Ritual cost multiplier");
                NumberField(ref Settings.State.ritualCooldownMult, "Ritual cooldown multiplier");
                NumberField(ref Settings.State.harvestTotemRadius, "Harvest Totem radius", 100f, Main.UpdateStaticSettings);
                NumberField(ref Settings.State.propagandaSpeakerRadius, "Propaganda Speaker radius", 100f, Main.UpdateStaticSettings);
                NumberField(ref Settings.State.farmStationRadius, "Farm Station radius", 100f);
                NumberField(ref Settings.State.farmSignRadius, "Farm Sign radius", 100f);

                EnumField(ref Settings.State.forceSeasonalEvent, "forceSeasonalEvent", 100f);

                GUILayout.Space(10);
            }

            if (Folder(ref Toggles[1], "Doctrines"))
            {
                GUILayout.Space(5);
                foreach (var doctrine in Unlocks.GetDoctrines())
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(DoctrineUpgradeSystem.GetUnlocked(doctrine) ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(20)))
                        Unlocks.ToggleUnlock(doctrine);
                    GUILayout.Space(5);
                    GUILayout.Label(DoctrineUpgradeSystem.GetSermonCategoryLocalizedName(DoctrineUpgradeSystem.GetCategory(doctrine)), GUILayout.Width(200));
                    GUILayout.Label(Unlocks.GetLevel(doctrine).ToString(), GUILayout.Width(30));
                    string name = DoctrineUpgradeSystem.GetLocalizedName(doctrine);
                    int num = name.IndexOf('>');
                    if (num > 0)
                        name = name.Substring(num + 1);
                    GUILayout.Label(name, DontExpand);
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(10);
            }

            // TODO: item editor
            //InventoryItem.ITEM_TYPE.PLEASURE_POINT
            //Inventory.AddItem

            //todo DataManager.AllNecklaces
            if (Folder(ref Toggles[2], "Followers"))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name", GUILayout.Width(150));
                GUILayout.Label("Age", GUILayout.Width(30));
                GUILayout.Label("Lifespan", GUILayout.Width(50));
                GUILayout.Label("State", GUILayout.Width(70));
                GUILayout.Label("Necklace", GUILayout.Width(100));
                GUILayout.Label("Role", GUILayout.Width(100));
                GUILayout.Label("Married", GUILayout.Width(50));
                GUILayout.Label("Faith", GUILayout.Width(50));
                GUILayout.Label("Tax", GUILayout.Width(50));
                GUILayout.Label("Hunger", GUILayout.Width(50));

                GUILayout.Label("Skin Name", GUILayout.Width(100));
                //GUILayout.Label("Skin", GUILayout.Width(50));
                //GUILayout.Label("Variant", GUILayout.Width(50));
                //GUILayout.Label("Color", GUILayout.Width(50));

                GUILayout.EndHorizontal();

                foreach (var follower in DataManager.Instance.Followers)
                {
                    GUILayout.BeginHorizontal();

                    StringField(ref follower.Name, 150f);
                    GUILayout.Label(follower.Age.ToString().ColorCond(follower.Age >= follower.LifeExpectancy, "red"), GUILayout.Width(30));
                    GUILayout.Label(follower.LifeExpectancy.ToString(), GUILayout.Width(50));
                    if (GUILayout.Button(follower.CursedState.ToString(), StyleBox, GUILayout.Width(70)))
                    {
                        if (follower.CursedState == Thought.OldAge)
                            follower.LifeExpectancy = follower.Age + 20;
                        follower.CursedState = Thought.None;
                    }
                    EnumField(ref follower.Necklace, width: 100f, values: Necklaces);
                    EnumField(ref follower.FollowerRole, width: 100f);
                    if (GUILayout.Button(follower.MarriedToLeader ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(50)))
                        follower.MarriedToLeader = !follower.MarriedToLeader;
                    if (GUILayout.Button(follower.FaithEnforcer ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(50)))
                        follower.MarriedToLeader = !follower.MarriedToLeader;
                    if (GUILayout.Button(follower.TaxEnforcer ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(50)))
                        follower.MarriedToLeader = !follower.MarriedToLeader;
                    GUILayout.Label(follower._satiation.ToString("F2"), GUILayout.Width(50));

                    GUILayout.Label(follower.SkinName.ToString(), GUILayout.Width(100));
                    //NumberField(ref follower.SkinCharacter, null, 0, WorshipperData.Instance.Characters.Count, 50f, n =>
                    //{
                    //    follower.SkinVariation = Mathf.Clamp(follower.SkinVariation, 0, WorshipperData.Instance.Characters[follower.SkinCharacter].Skin.Count - 1);
                    //    follower.SkinName = WorshipperData.Instance.Characters[follower.SkinCharacter].Skin[follower.SkinVariation].Skin;
                    //    follower.SkinColour = Mathf.Clamp(follower.SkinColour, 0, WorshipperData.Instance.GetColourData(follower.SkinName).StartingSlotAndColours.Count - 1);
                    //});
                    //NumberField(ref follower.SkinVariation, null, 0, WorshipperData.Instance.Characters[follower.SkinCharacter].Skin.Count - 1, 50f);
                    //NumberField(ref follower.SkinColour, null, 0, WorshipperData.Instance.GetColourData(follower.SkinName).StartingSlotAndColours.Count - 1, 50f);

                    /* new from: Villager_Info.NewCharacter
                    int index = Random.Range(0, WorshipperData.Instance.Characters.Count);
                    villager_Info.SkinVariation = Random.Range(0, WorshipperData.Instance.Characters[index].Skin.Count);
                    villager_Info.SkinName = WorshipperData.Instance.Characters[index].Skin[villager_Info.SkinVariation].Skin;
                    villager_Info.SkinColour = Random.Range(0, WorshipperData.Instance.GetColourData(villager_Info.SkinName).SlotAndColours.Count);
                     */

                    if (GUILayout.Button(LastTrait == follower.ID ? "<b>▼</b> Traits" : "<b>▶</b> Traits", StyleBox, GUILayout.Width(70)))
                        LastTrait = LastTrait == follower.ID ? -1 : follower.ID;

                    GUILayout.EndHorizontal();

                    if (LastTrait == follower.ID)
                    {
                        foreach (var trait in AllTraits)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(5);

                            bool hasTrait = follower.Traits.Contains(trait);
                            if (GUILayout.Button(hasTrait ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(50)))
                                if (hasTrait)
                                    follower.Traits.Remove(trait);
                                else
                                    follower.Traits.Add(trait);
                            GUILayout.Space(5);
                            GUILayout.Label($"{FollowerTrait.GetLocalizedTitle(trait)}: {RxClean.Replace(FollowerTrait.GetLocalizedDescription(trait), "")}", DontExpand);

                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }

            if (Folder(ref Toggles[3], "Dead Followers"))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name", GUILayout.Width(150));
                GUILayout.Label("Age", GUILayout.Width(30));
                GUILayout.Label("Lifespan", GUILayout.Width(50));
                GUILayout.Label("Married", GUILayout.Width(50));
                GUILayout.Label("Faith", GUILayout.Width(50));
                GUILayout.Label("Tax", GUILayout.Width(50));

                GUILayout.Label("Skin Name", GUILayout.Width(100));
                //GUILayout.Label("Skin", GUILayout.Width(50));
                //GUILayout.Label("Variant", GUILayout.Width(50));
                //GUILayout.Label("Color", GUILayout.Width(50));

                GUILayout.EndHorizontal();

                for (int i = 0; i < DataManager.Instance.Followers_Dead.Count; i++)
                {
                    var follower = DataManager.Instance.Followers_Dead[i];
                    GUILayout.BeginHorizontal();

                    StringField(ref follower.Name, 150f);
                    GUILayout.Label(follower.Age.ToString().ColorCond(follower.Age >= follower.LifeExpectancy, "red"), GUILayout.Width(30));
                    GUILayout.Label(follower.LifeExpectancy.ToString(), GUILayout.Width(50));
                    if (GUILayout.Button(follower.MarriedToLeader ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(50)))
                        follower.MarriedToLeader = !follower.MarriedToLeader;
                    if (GUILayout.Button(follower.FaithEnforcer ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(50)))
                        follower.MarriedToLeader = !follower.MarriedToLeader;
                    if (GUILayout.Button(follower.TaxEnforcer ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(50)))
                        follower.MarriedToLeader = !follower.MarriedToLeader;

                    GUILayout.Label(follower.SkinName.ToString(), GUILayout.Width(100));

                    if (!FollowerManager.UniqueFollowerIDs.Contains(follower.ID) && GUILayout.Button("Remove", StyleBox, GUILayout.Width(70)))
                    {
                        DataManager.Instance.Followers_Dead.Remove(follower);
                        DataManager.Instance.Followers_Dead_IDs.Remove(follower.ID);
                        FollowerManager.RemoveFollowerBrain(follower.ID);
                        ObjectiveManager.FailUniqueFollowerObjectives(follower.ID);
                    }

                    if (GUILayout.Button(LastTrait == follower.ID ? "<b>▼</b> Traits" : "<b>▶</b> Traits", StyleBox, GUILayout.Width(70)))
                        LastTrait = LastTrait == follower.ID ? -1 : follower.ID;

                    GUILayout.EndHorizontal();

                    if (LastTrait == follower.ID)
                    {
                        foreach (var trait in AllTraits)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(5);

                            bool hasTrait = follower.Traits.Contains(trait);
                            if (GUILayout.Button(hasTrait ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(50)))
                                if (hasTrait)
                                    follower.Traits.Remove(trait);
                                else
                                    follower.Traits.Add(trait);
                            GUILayout.Space(5);
                            GUILayout.Label($"{FollowerTrait.GetLocalizedTitle(trait)}: {RxClean.Replace(FollowerTrait.GetLocalizedDescription(trait), "")}", DontExpand);

                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }

            // Debug
            if (GUILayout.Button("Debug Crows", DontExpand))
            {
                try
                {
                    foreach (var structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
                    {
                        if (structureBrain is Structures_Scarecrow _scareCrow)
                        {
                            _scareCrow.HasBird = true;
                            _scareCrow.OnCatchBird?.Invoke();
                        }
                    }

                    foreach (var kitchen in Interaction_Kitchen.Kitchens)
                        Main.Print("kitchen: " + kitchen.structure.Structure_Info.ID);
                }
                catch (Exception e) { Main.Print(e.ToString()); }
            }
            if (GUILayout.Button("Debug +1000 gold", DontExpand))
            {
                Inventory.AddItem(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1000, true);
            }
            if (GUILayout.Button("Debug farm plots", DontExpand))
            {
                foreach (var silo in StructureManager.GetAllStructuresOfType<Structures_SiloSeed>(FollowerLocation.Base))
                {
                    Main.Print($"silo content={silo.Data.Inventory.Join(a => $"{(InventoryItem.ITEM_TYPE)a.type}:{a.quantity}")}");
                }

                foreach (var plot in StructureManager.GetAllStructuresOfType<Structures_FarmerPlot>(FollowerLocation.Base))
                {
                    Main.Print($"plant={plot.Data.GrowthStage:00.00} seed={(InventoryItem.ITEM_TYPE)(plot.GetPlantedSeed()?.type ?? 0)}");
                }
            }
            if (GUILayout.Button("Debug unkill Ratau", DontExpand))
            {
                DataManager.Instance.RatauKilled = false;
            }
            if (GUILayout.Button("Debug zombie job", DontExpand))
            {
                var tasks = new SortedList<float, FollowerTask>();
                foreach (var farm in StructureManager.GetAllStructuresOfType<Structures_FarmerStation>(FollowerLocation.Base))
                    farm.GetAvailableTasks(ScheduledActivity.None, tasks);

                var task = tasks.GetEnumerator();
                foreach (var follower in FollowerBrain.AllBrains)
                {
                    if (follower.HasTrait(FollowerTrait.TraitType.Zombie))
                    {
                        if (!task.MoveNext())
                            break;
                        follower.HardSwapToTask(task.Current.Value);
                        follower.ShouldReconsiderTask = false;
                        Main.Print($"force {follower.Info.Name} into task {task.Current.Value}");
                    }
                }
            }
            if (GUILayout.Button("Debug check skins", DontExpand))
            {
                foreach (WorshipperData.SkinAndData item in WorshipperData.Instance.GetSkinsAll())
                {
                    bool achievement = false;
                    if (!DataManager.GetFollowerSkinUnlocked(item.Skin[0].Skin)
                        && !DataManager.instance.DLCSkins.Contains(item.Skin[0].Skin)
                        && !DataManager.instance.SpecialEventSkins.Contains(item.Skin[0].Skin))
                    {
                        achievement = true;
                    }
                    Main.Print($"Skin '{item.Skin[0].Skin}' is achievement={achievement}");
                }
                WorshipperData.Instance.ExportJson();
                Main.Print($"Exported list to {Application.persistentDataPath + "/CotL Skin Variations.json"}");
            }

            //DataManager.FollowerSkinsBlacklist
            if (!DataManager.instance.FollowerSkinsUnlocked.Contains("DogTeddy") && GUILayout.Button("Debug unlock DogTeddy", DontExpand))
                DataManager.instance.FollowerSkinsUnlocked.Add("DogTeddy");
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry) => Settings.State.TrySave();

        private static void OnHideGUI(UnityModManager.ModEntry modEntry)
        {
        }

        private static void Checkbox(ref bool value, string label, Action action = null)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(value ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(20)))
            {
                value = !value;
                action?.Invoke();
            }
            GUILayout.Space(5);
            GUILayout.Label(label, DontExpand);
            GUILayout.EndHorizontal();
        }

        private static void NumberField(ref float value, string label = null, float width = 100f, Action action = null)
        {
            if (label != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(label + ": ", DontExpand);
            }

            if (float.TryParse(GUILayout.TextField(value.ToString("0.0"), GUILayout.Width(width)), out float newvalue) && value != newvalue)
            {
                value = newvalue;
                action?.Invoke();
            }

            if (label != null)
                GUILayout.EndHorizontal();
        }

        private static void NumberField(ref int value, string label = null, int min = int.MinValue, int max = int.MaxValue, float width = 100f, Action action = null)
        {
            if (label != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(label + ": ", DontExpand);
            }

            if (int.TryParse(GUILayout.TextField(value.ToString(), GUILayout.Width(width)), out int newvalue))
            {
                newvalue = Mathf.Clamp(newvalue, min, max);
                if (value != newvalue)
                {
                    value = newvalue;
                    action?.Invoke();
                }
            }

            if (label != null)
                GUILayout.EndHorizontal();
        }

        private static void EnumField<T>(ref T value, string label = null, float width = 100f, Action action = null, T[] values = null)
        {
            if (label != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(label + ": ", DontExpand);
            }

            if (GUILayout.Button(value.ToString(), StyleBox, GUILayout.Width(width)))
            {
                values ??= (T[])typeof(T).GetEnumValues();
                int index = values.IndexOf(value) + 1;
                if (index >= values.Length)
                    index = 0;
                value = values[index];
                action?.Invoke();
            }

            if (label != null)
                GUILayout.EndHorizontal();
        }

        private static void StringField(ref string value, float width = 100f, Action action = null)
        {
            //string newvalue = GUILayout.TextField(CursorIndex == CursorCounter ? CursorBuffer : value, GUILayout.Width(width));
            string newvalue = GUILayout.TextField(value, GUILayout.Width(width));
            if (newvalue != value)
            {
                newvalue = newvalue.Replace("\r", "R").Replace("\n", "N"); // TODO: only set when ENTER is pressed
                value = newvalue;
                action?.Invoke();
            }
        }

        private static bool Folder(ref bool flag, string text)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(flag ? "<color=lime><b>▼</b></color>" : "<color=yellow><b>▶</b></color>", StyleBox, GUILayout.Width(20)))
                flag = !flag;
            GUILayout.Space(3);
            GUILayout.Label(text);
            GUILayout.EndHorizontal();

            return flag;
        }

        private static string ColorCond(this string text, bool cond, string ifTrue, string ifFalse = null)
        {
            if (cond)
                return $"<color={ifTrue}>{text}</color>";
            else if (ifFalse != null)
                return $"<color={ifFalse}>{text}</color>";
            else
                return text;
        }
    }
#elif BEPINEX
    [BepInPlugin("Truinto.FireDevil", "FireDevil", Main.version)]
    public class Main_Bep : BaseUnityPlugin
    {
        public void Awake()
        {
            Main.logger = new Logger_Bep(this.Logger);
            Main.ModPath = Path.GetDirectoryName(this.Info.Location);
            Main.harmony = new Harmony("Truinto.FireDevil");

            Main.Load();
            Main.Print($"FireDevil is loaded!");
        }
    }
#endif
}
