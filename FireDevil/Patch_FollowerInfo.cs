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
        private static StringBuilder _Sb = new(32);

        [HarmonyPatch(typeof(FollowerInformationBox), nameof(FollowerInformationBox.ConfigureImpl))]
        [HarmonyPostfix]
        public static void Postfix1(FollowerInformationBox __instance)
        {
            if (Settings.State.showFollowerRole)
            {
                _Sb.Append("Days:");
                _Sb.Append(__instance._followerInfo.MemberDuration);
                _Sb.Append(" Age:");
                _Sb.Append(__instance._followerInfo.Age);
                //sb.Append(" Role:");
                //sb.Append(FollowerRoleInfo.GetLocalizedName(__instance._followerInfo.FollowerRole));

                _Sb.Append(" ");
                bool useDelimiter = false;
                int length = _Sb.Length;
                foreach (var trait in __instance._followerInfo.Traits)
                {
                    if (useDelimiter)
                    {
                        if (length <= 40)
                            _Sb.Append(", ");
                        else
                        {
                            _Sb.Append(",\n");
                            length = 0;
                        }
                    }

                    string trait_str = FollowerTrait.GetLocalizedTitle(trait);

                    if (FollowerTrait.IsPositiveTrait(trait))
                        _Sb.Append("<color=green>");
                    else
                        _Sb.Append("<color=red>");
                    _Sb.Append(trait_str);
                    _Sb.Append("</color>");
                    length += trait_str.Length;
                    useDelimiter = true;
                }

                __instance.FollowerRole.text = _Sb.ToString();
                _Sb.Clear();
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

                _Sb.Append("\nTraits:");
                bool useDelimiter = false;
                foreach (var trait in __instance._followerInfo.Traits)
                {
                    if (useDelimiter)
                        _Sb.Append(", ");
                    _Sb.Append(FollowerTrait.GetLocalizedDescription(trait));
                    useDelimiter = true;
                }

                __instance._ageText.text += _Sb.ToString();
                _Sb.Clear();
            }
        }
    }
}
