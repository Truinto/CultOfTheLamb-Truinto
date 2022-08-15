using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace FireDevil
{
    public class Settings : BaseSettings<Settings>
    {
        public Settings() => version = 1;

        [JsonProperty] public bool showMurderAction = true;
        [JsonProperty] public bool showFollowerRole = true;
        [JsonProperty] public bool infiniteMineI = false;
        [JsonProperty] public bool infiniteMineII = true;
        [JsonProperty] public bool globalTaskPatch = true;

        protected override bool OnUpdate()
        {
            return true;
        }

        public static Settings State = TryLoad(Main.Path, "settings.json");
    }
}
