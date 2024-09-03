using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    [HarmonyPatch(typeof(FollowerBrain), nameof(FollowerBrain.GetDesiredTask_Work))]
    public static class Patch_GetTasks
    {
        public static bool Prefix(FollowerLocation location, FollowerBrain __instance, ref List<FollowerTask> __result)
        {
            var tasks = new SortedList<float, FollowerTask>(new FollowerBrain.DuplicateKeyComparer<float>());
            foreach (var structureBrain in StructureManager.StructuresAtLocation(location))
            {
                if (structureBrain is ITaskProvider provider)
                {
                    provider.GetAvailableTasks(ScheduledActivity.Work, tasks);
                }
                else if (structureBrain is Structures_Scarecrow _scareCrow && _scareCrow.HasBird && !_scareCrow.ReservedForTask)
                {
                    tasks.Add(23f, new FollowerTask_Scarecrow(_scareCrow.Data.ID));
                }
            }

            __result = new List<FollowerTask>(tasks.Values);
            return false;
        }
    }
}
