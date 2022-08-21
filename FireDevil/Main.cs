global using HarmonyLib;
global using JetBrains.Annotations;
global using Newtonsoft.Json;
global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Linq;
global using System.Reflection;
global using System.Reflection.Emit;
global using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using UnityModManagerNet;

namespace FireDevil
{
    public static class Main
    {
        public static string ModPath;
        private static UnityModManager.ModEntry.ModLogger logger;
        private static Harmony harmony;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            ModPath = modEntry.Path;
            harmony = new(modEntry.Info.Id);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnHideGUI = OnHideGUI;
#if DEBUG
            //PatchSafe(typeof(Patch_GetTasks));
            PatchSafe(typeof(Patch_Demons));
#endif
            PatchSafe(typeof(Patch_Lockstate));
            PatchSafe(typeof(Patch_MurderAction));
            PatchSafe(typeof(Patch_LumberMine));
            PatchSafe(typeof(Patch_FollowerInfo));
            PatchSafe(typeof(Patch_Fleece));
            PatchSafe(typeof(Patch_Storages));
            PatchSafe(typeof(Patch_Adoration));

            var nullFinalizer = new HarmonyMethod(AccessTools.Method(typeof(Main), nameof(Main.NullFinalizer)));
            foreach (var patch in harmony.GetPatchedMethods().ToArray())
            {
                if (Harmony.GetPatchInfo(patch).Finalizers.Count == 0)
                {
                    harmony.Patch(patch, finalizer: nullFinalizer);
                    Print("Applied finalizer to " + patch.Name);
                }
            }

            return true;
        }

        private static GUIStyle StyleBox;
        private static GUIStyle StyleLine;
        private static GUILayoutOption DontExpand = GUILayout.ExpandWidth(false);
        private static bool[] Toggles = new bool[4];
        private static int LastTrait;
        private static FollowerTrait.TraitType[] AllTraits = (FollowerTrait.TraitType[])Enum.GetValues(typeof(FollowerTrait.TraitType));
        private static Regex RxClean = new("<.*?>");
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
                Checkbox(ref Settings.State.showMurderAction, "Always show murder action");
                Checkbox(ref Settings.State.showFollowerRole, "Show follower role in thoughts");
                Checkbox(ref Settings.State.infiniteMineI, "Infinite Lumber and Stone Mine");
                Checkbox(ref Settings.State.infiniteMineII, "Infinite Lumber and Stone Mine Level 2");
                Checkbox(ref Settings.State.instantPickup, "Instantly pickup mine resources");
                Checkbox(ref Settings.State.disableFleecePenalty, "Disable most fleece penalties");
                Checkbox(ref Settings.State.loyaltyOverflow, "Allow follower loyalty overflow to next level");
                //Checkbox(ref Settings.State.globalTaskPatch, "Injects new task types");

                NumberField(ref Settings.State.storageShrine, "Shrine soul maximum multiplier");
                NumberField(ref Settings.State.storageOuthouse, "Outhouse maximum multiplier");
                NumberField(ref Settings.State.storageSilo, "Silo maximum multiplier");

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

            if (Folder(ref Toggles[2], "Weapon Probability"))
            {
                var weaponType = EquipmentType.None;
                foreach (var weapon in WeaponWeight.GetWeapons())
                {
                    if (weapon.PrimaryEquipmentType != weaponType)
                    {
                        weaponType = weapon.PrimaryEquipmentType;
                        GUILayout.Space(5);
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(weapon.PrimaryEquipmentType.ToString(), StyleBox, DontExpand))
                        {
                            foreach (var weapon2 in WeaponWeight.GetWeapons())
                                if (weapon2.PrimaryEquipmentType == weaponType)
                                    weapon2.Weight = 0f;
                        }
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(weapon.GetLocalisedTitle() + ": ", GUILayout.Width(200));
                    weapon.Weight = GUILayout.HorizontalSlider(weapon.Weight, 0f, 10f, GUILayout.Width(100));
                    GUILayout.Label(weapon.Weight.ToString("0.0"));
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(10);
            }

            if (Folder(ref Toggles[3], "Followers"))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name", GUILayout.Width(150));
                GUILayout.Label("Age", GUILayout.Width(50));
                GUILayout.Label("Lifespan", GUILayout.Width(50));
                GUILayout.Label("Loyalty", GUILayout.Width(50));
                GUILayout.Label("State", GUILayout.Width(70));
                GUILayout.Label("Necklace", GUILayout.Width(100));
                GUILayout.Label("Married", GUILayout.Width(50));

                GUILayout.Label("Skin", GUILayout.Width(50));
                GUILayout.Label("Skin Name", GUILayout.Width(100));
                GUILayout.Label("Variant", GUILayout.Width(50));
                GUILayout.Label("Color", GUILayout.Width(50));

                GUILayout.EndHorizontal();

                foreach (var follower in DataManager.Instance.Followers)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(follower.Name, GUILayout.Width(150));
                    GUILayout.Label(follower.Age.ToString(), GUILayout.Width(50));
                    GUILayout.Label(follower.LifeExpectancy.ToString(), GUILayout.Width(50));
                    GUILayout.Label(follower.Adoration.ToString(), GUILayout.Width(50));
                    GUILayout.Label(follower.CursedState.ToString(), GUILayout.Width(70));
                    if (GUILayout.Button(follower.Necklace.ToString(), StyleBox, GUILayout.Width(100)))
                    {
                        switch (follower.Necklace)
                        {
                            case InventoryItem.ITEM_TYPE.NONE:
                                follower.Necklace = InventoryItem.ITEM_TYPE.Necklace_1;
                                break;
                            case InventoryItem.ITEM_TYPE.Necklace_1:
                            case InventoryItem.ITEM_TYPE.Necklace_2:
                            case InventoryItem.ITEM_TYPE.Necklace_3:
                            case InventoryItem.ITEM_TYPE.Necklace_4:
                                follower.Necklace++;
                                break;
                            default:
                                follower.Necklace = InventoryItem.ITEM_TYPE.NONE;
                                break;
                        }
                    }
                    if (GUILayout.Button(follower.MarriedToLeader ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(50)))
                        follower.MarriedToLeader = !follower.MarriedToLeader;

                    NumberField(ref follower.SkinCharacter, null, 0, WorshipperData.Instance.Characters.Count, 50f, n =>
                    {
                        follower.SkinVariation = Mathf.Clamp(follower.SkinVariation, 0, WorshipperData.Instance.Characters[follower.SkinCharacter].Skin.Count - 1);
                        follower.SkinName = WorshipperData.Instance.Characters[follower.SkinCharacter].Skin[follower.SkinVariation].Skin;
                        follower.SkinColour = Mathf.Clamp(follower.SkinColour, 0, WorshipperData.Instance.GetColourData(follower.SkinName).StartingSlotAndColours.Count - 1);
                    });
                    GUILayout.Label(follower.SkinName.ToString(), GUILayout.Width(100));
                    GUILayout.Label(follower.SkinVariation.ToString(), GUILayout.Width(50));
                    GUILayout.Label(follower.SkinColour.ToString(), GUILayout.Width(50));
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
                        Print("kitchen: " + kitchen.structure.Structure_Info.ID);
                }
                catch (Exception e) { PrintException(e); }
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry) => Settings.State.TrySave();

        private static void OnHideGUI(UnityModManager.ModEntry modEntry)
        {
        }

        private static void Checkbox(ref bool value, string label, Action<bool> action = null)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(value ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(20)))
            {
                value = !value;
                action?.Invoke(value);
            }
            GUILayout.Space(5);
            GUILayout.Label(label, DontExpand);
            GUILayout.EndHorizontal();
        }

        private static void NumberField(ref float value, string label, float width = 100f, Action<float> action = null)
        {
            if (label != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(label + ": ", DontExpand);
            }

            if (float.TryParse(GUILayout.TextField(value.ToString("0.0"), GUILayout.Width(width)), out float newvalue) && value != newvalue)
            {
                value = newvalue;
                action?.Invoke(newvalue);
            }

            if (label != null)
                GUILayout.EndHorizontal();
        }

        private static void NumberField(ref int value, string label, int min = int.MinValue, int max = int.MaxValue, float width = 100f, Action<int> action = null)
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
                    action?.Invoke(newvalue);
                }
            }

            if (label != null)
                GUILayout.EndHorizontal();
        }

        private static bool Folder(ref bool flag, string text)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(flag ? "<color=yellow><b>▶</b></color>" : "<color=lime><b>▼</b></color>", StyleBox, GUILayout.Width(20)))
                flag = !flag;
            GUILayout.Space(3);
            GUILayout.Label(text);
            GUILayout.EndHorizontal();

            return !flag;
        }

        #region GUI



        #endregion

        #region Helper

        internal static void Print(string msg)
        {
            logger.Log(msg);
        }

        internal static void PrintException(Exception e)
        {
            logger.Log("Firedevil Exception!");
            logger.LogException(e);
        }

        internal static void PatchSafe(Type patch)
        {
            try
            {
                Print("Patching " + patch.Name);
                harmony.CreateClassProcessor(patch).Patch();
            }
            catch (Exception e) { logger.LogException(e); }
        }

        internal static Exception NullFinalizer(Exception __exception)
        {
#if !DEBUG
            return null;
#endif
            if (__exception == null)
                return null;
            try
            {
                PrintException(__exception);
            }
            catch (Exception) { }
            return null;
        }

        #endregion

    }
}
