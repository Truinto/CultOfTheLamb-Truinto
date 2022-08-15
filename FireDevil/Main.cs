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
using UnityModManagerNet;

namespace FireDevil
{
    public static class Main
    {
        public static string Path;
        internal static UnityModManager.ModEntry.ModLogger logger;
        private static Harmony harmony;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            logger = modEntry.Logger;
            Path = modEntry.Path;
            harmony = new(modEntry.Info.Id);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
#if DEBUG
            //PatchSafe(typeof(Patch_GetTasks));
#endif
            PatchSafe(typeof(Patch_MurderAction));
            PatchSafe(typeof(Patch_LumberMine));
            PatchSafe(typeof(Patch_FollowerInfo));

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
        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (StyleBox == null)
            {
                StyleBox = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter };
                StyleLine = new GUIStyle() { fixedHeight = 1, margin = new RectOffset(0, 0, 4, 4), };
                StyleLine.normal.background = new Texture2D(1, 1);
            }

            // Settings
            GUILayout.Label("Settings");
            GUILayout.Space(5);
            Checkbox(ref Settings.State.showMurderAction, "Always show murder action");
            Checkbox(ref Settings.State.showFollowerRole, "Show follower role in thoughts");
            Checkbox(ref Settings.State.infiniteMineI, "Infinite Lumber and Stone Mine");
            Checkbox(ref Settings.State.infiniteMineII, "Infinite Lumber and Stone Mine Level 2");
            Checkbox(ref Settings.State.globalTaskPatch, "Injects new task types");
            GUILayout.Space(10);

            // Game must be running
            if (DataManager.instance == null)
                return;

            // Doctrines Toggles
            GUILayout.Label("Doctrines");
            GUILayout.Space(5);
            foreach (var doctrine in Unlocks.GetDoctrines())
                if (GUILayout.Button(Unlocks.GetGUIText(doctrine), GUILayout.ExpandWidth(false)))
                    Unlocks.ToggleUnlock(doctrine);
            GUILayout.Space(10);

            // Weapon Weights
            foreach (var weapon in WeaponWeight.GetWeapons())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(weapon.GetLocalisedTitle() + ": ", GUILayout.Width(200));
                weapon.Weight = GUILayout.HorizontalSlider(weapon.Weight, 0f, 10f, GUILayout.Width(100));
                GUILayout.Label(weapon.Weight.ToString("0.0"));
                GUILayout.EndHorizontal();
            }

            // Debug
            if (GUILayout.Button("Debug Crows", GUILayout.ExpandWidth(false)))
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

        private static void Checkbox(ref bool value, string label, Action<bool> action = null)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(value ? "<color=green><b>✔</b></color>" : "<color=red><b>✖</b></color>", StyleBox, GUILayout.Width(20)))
            {
                value = !value;
                action?.Invoke(value);
            }
            GUILayout.Space(5);
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        private static void NumberField(ref float value, string label)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(label + ": ", GUILayout.ExpandWidth(false));
            if (float.TryParse(GUILayout.TextField(value.ToString(), GUILayout.Width(230f)), out float newvalue))
                value = newvalue;

            GUILayout.EndHorizontal();
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
