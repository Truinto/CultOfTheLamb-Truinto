using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    public static class TimeManagment
    {
        public static void OnLoad()
        {
            //TimeManager.OnNewDayStarted -= OnNewDay;
            //TimeManager.OnNewDayStarted += OnNewDay;
        }

        public static void OnNewDay()
        {
            var resource_chest = StructureManager.GetAllStructuresOfType<Structures_CollectedResourceChest>(FollowerLocation.Base);
        }
    }
}
