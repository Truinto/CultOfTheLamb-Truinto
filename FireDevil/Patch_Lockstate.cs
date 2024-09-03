#if UMM

using UnityModManagerNet;

namespace FireDevil
{
    [HarmonyPatch(typeof(UnityModManager.UI), nameof(UnityModManager.UI.ToggleWindow), typeof(bool))]
    public static class Patch_Lockstate
    {
        public static void Prefix()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public static void Postfix()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}

#endif