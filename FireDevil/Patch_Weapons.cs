using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_Weapons
    {
        [HarmonyPatch(typeof(PlayerWeapon), nameof(PlayerWeapon.HeavyAttackFervourCost), MethodType.Getter)]
        [HarmonyPostfix]
        public static void FervourCost(ref int __result)
        {
            if (Settings.State.freeHeavyAttack)
                __result = 0;
        }
    }
}
