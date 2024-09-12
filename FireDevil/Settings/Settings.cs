using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireDevil
{
    public class Settings : BaseSettings<Settings>
    {
        public Settings() => version = 5;

        [JsonProperty] public bool hideNecklaceAction = false;
        [JsonProperty] public bool autoAddCookingTask = true;
        [JsonProperty] public bool showMurderAction = true;
        [JsonProperty] public bool showFollowerRole = true;
        [JsonProperty] public bool infiniteMineI = false;
        [JsonProperty] public bool infiniteMineII = true;
        [JsonProperty] public bool instantPickup = true;
        [JsonProperty] public bool redirectPlayerInventory = false;
        [JsonProperty] public bool hideItemPopup = false;
        [JsonProperty] public bool disableFleecePenalty = false;
        [JsonProperty] public bool loyaltyOverflow = true;
        [JsonProperty] public bool freeHeavyAttack = true;
        [JsonProperty] public bool siloBucketInfinite = false;
        [JsonProperty] public bool blessSuperRange = true;
        [JsonProperty] public bool zombiesDoWork = true;

        [JsonProperty] public float goldenEggChance = 0.2f;
        [JsonProperty] public float storageShrineMult = 1.0f;
        [JsonProperty] public float storageOuthouseMult = 1.0f;
        [JsonProperty] public float storageSiloMult = 1.0f;

        [JsonProperty] public float compostMult = 1.0f;
        [JsonProperty] public int compostCost = 25;

        [JsonProperty] public float ritualCostMult = 0.5f;
        [JsonProperty] public float ritualCooldownMult = 0.5f;

        [JsonProperty] public float harvestTotemRadius = 7f;
        [JsonProperty] public float propagandaSpeakerRadius = 8f;
        [JsonProperty] public float farmStationRadius = 6f;
        [JsonProperty] public float farmSignRadius = 5f;

        [JsonProperty] public SeasonalEventType forceSeasonalEvent = SeasonalEventType.None;

        protected override bool OnUpdate()
        {
            return true;
        }

        public static Settings State = TryLoad(Main.ModPath, "settings.json");
    }
}
