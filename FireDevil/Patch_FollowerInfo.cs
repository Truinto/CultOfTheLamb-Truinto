using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public static class Patch_FollowerInfo
    {
        private static StringBuilder sb = new(32);

        [HarmonyPatch(typeof(FollowerInformationBox), nameof(FollowerInformationBox.ConfigureImpl))]
        [HarmonyPostfix]
        public static void Postfix1(FollowerInformationBox __instance)
        {
            if (Settings.State.showFollowerRole)
            {
                sb.Append("Days:");
                sb.Append(__instance._followerInfo.MemberDuration);
                sb.Append(" Age:");
                sb.Append(__instance._followerInfo.Age);
                //sb.Append(" Role:");
                //sb.Append(FollowerRoleInfo.GetLocalizedName(__instance._followerInfo.FollowerRole));

                sb.Append(" ");
                bool useDelimiter = false;
                int length = sb.Length;
                foreach (var trait in __instance._followerInfo.Traits)
                {
                    if (useDelimiter)
                    {
                        if (length <= 40)
                            sb.Append(", ");
                        else
                        {
                            sb.Append(",\n");
                            length = 0;
                        }
                    }

                    string trait_str = FollowerTrait.GetLocalizedTitle(trait);

                    if (FollowerTrait.IsPositiveTrait(trait))
                        sb.Append("<color=green>");
                    else
                        sb.Append("<color=red>");
                    sb.Append(trait_str);
                    sb.Append("</color>");
                    length += trait_str.Length;
                    useDelimiter = true;
                }

                __instance.FollowerRole.text = sb.ToString();
                sb.Clear();
            }
        }

        //[HarmonyPatch(typeof(DeadFollowerInformationBox), nameof(DeadFollowerInformationBox.ConfigureImpl))]
        //[HarmonyPostfix]
        public static void Postfix2(DeadFollowerInformationBox __instance)
        {
            if (Settings.State.showFollowerRole)
            {
                //sb.Append("Days:");
                //sb.Append(__instance._followerInfo.MemberDuration);
                //sb.Append(" Age:");
                //sb.Append(__instance._followerInfo.Age);
                //sb.Append(" Role:");
                //sb.Append(FollowerRoleInfo.GetLocalizedName(__instance._followerInfo.FollowerRole));

                sb.Append("\nTraits:");
                bool useDelimiter = false;
                foreach (var trait in __instance._followerInfo.Traits)
                {
                    if (useDelimiter)
                        sb.Append(", ");
                    sb.Append(FollowerTrait.GetLocalizedDescription(trait));
                    useDelimiter = true;
                }

                __instance._ageText.text += sb.ToString();
                sb.Clear();
            }
        }
    }
}
