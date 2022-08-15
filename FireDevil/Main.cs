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

            PatchSafe(typeof(Patch_MurderAction));
            PatchSafe(typeof(Patch_LumberMine));
            PatchSafe(typeof(Patch_FollowerInfo));
            //PatchSafe(typeof(Patch_GetTasks));

            return true;
        }

        #region Helper

        internal static void Print(string msg)
        {
            logger.Log(msg);
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

        #endregion

    }
}
