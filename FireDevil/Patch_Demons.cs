using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch]
    public class Patch_Demons
    {
        [HarmonyPatch(typeof(DemonModel), nameof(DemonModel.AvailableFollowersForDemonConversion))]
        public static bool Prefix(ref List<Follower> __result)
        {
            List<Follower> list = new List<Follower>(FollowerManager.FollowersAtLocation(FollowerLocation.Base));
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var brain = list[i].Brain.Info;
                if ((brain.CursedState != Thought.None && brain.CursedState != Thought.OldAge) || FollowerManager.FollowerLocked(brain.ID, false))
                {
                    list.RemoveAt(i);
                }
            }
            __result = list;
            return false;
        }
    }
}
