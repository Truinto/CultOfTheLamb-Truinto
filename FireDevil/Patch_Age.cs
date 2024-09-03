using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_Age
    {
        //[HarmonyPatch(typeof(FollowerBrain), nameof(FollowerBrain.BecomeDisciple))]
        //[HarmonyPostfix]
        //public static void BecomeDisciple(FollowerBrain __instance)
        //{
        //    __instance.AddTrait(FollowerTrait.TraitType.Immortal);
        //}
    }
}
