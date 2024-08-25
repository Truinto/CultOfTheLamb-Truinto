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
        public Settings() => version = 2;

        [JsonProperty] public bool showMurderAction = true;
        [JsonProperty] public bool showFollowerRole = true;
        [JsonProperty] public bool infiniteMineI = false;
        [JsonProperty] public bool infiniteMineII = true;
        [JsonProperty] public bool instantPickup = true;
        [JsonProperty] public bool disableFleecePenalty = false;
        [JsonProperty] public bool loyaltyOverflow = true;

        [JsonProperty] public float storageShrineMult = 1.0f;
        [JsonProperty] public float storageOuthouseMult = 1.0f;
        [JsonProperty] public float storageSiloMult = 1.0f;

        [JsonProperty] public float compostMult = 1.0f;
        [JsonProperty] public float ritualCostMult = 0.5f;
        [JsonProperty] public float ritualCooldownMult = 0.5f;

        [JsonProperty] public float harvestTotemRadius = 7f;

        protected override bool OnUpdate()
        {
            return true;
        }

        public static Settings State = TryLoad(Main.ModPath, "settings.json");
    }
}
