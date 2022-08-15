using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch(typeof(FollowerInformationBox), nameof(FollowerInformationBox.ConfigureImpl))]
    public class Patch_FollowerInfo
    {
        public static void Postfix(FollowerInformationBox __instance)
        {
            __instance.FollowerRole.text += $" | {__instance.FollowerInfo.FollowerRole}";
        }
    }
}
