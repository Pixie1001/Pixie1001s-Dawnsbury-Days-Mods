using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class EncounterTables {
        public static List<EncounterCampaignStop>[] encounters = new List<EncounterCampaignStop>[3];
        public static List<EncounterCampaignStop>[] eliteEncounters = new List<EncounterCampaignStop>[3];
        public static List<EncounterCampaignStop> bossFights = new List<EncounterCampaignStop>();

        public static void LoadEncounterTables() {
            bossFights.Clear();

            for (int i = 0; i < encounters.Length; i++) {
                encounters[i] = new List<EncounterCampaignStop>();
            }

            for (int i = 0; i < eliteEncounters.Length; i++) {
                eliteEncounters[i] = new List<EncounterCampaignStop>();
            }

            // level1Encounters
            encounters[0].Add(new TypedEncounterCampaignStop<DrowAmbushLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<RatSwarmLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<InquisitrixTrapLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<SpiderNestLv1>());

            // level2Encounters
            encounters[1].Add(new TypedEncounterCampaignStop<DrowAmbushLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<RatSwarmLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<InquisitrixTrapLv2>());
            encounters[0].Add(new TypedEncounterCampaignStop<SpiderNestLv2>());

            // level1Encounters
            encounters[2].Add(new TypedEncounterCampaignStop<DrowAmbushLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<RatSwarmLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<InquisitrixTrapLv3>());
            encounters[0].Add(new TypedEncounterCampaignStop<SpiderNestLv3>());

            // level1EliteEncounters
            eliteEncounters[0].Add(new TypedEncounterCampaignStop<HallOfSmokeLv1>());
            eliteEncounters[0].Add(new TypedEncounterCampaignStop<WitchCovenLv1>());

            // level1EliteEncounters
            eliteEncounters[1].Add(new TypedEncounterCampaignStop<HallOfSmokeLv2>());
            eliteEncounters[0].Add(new TypedEncounterCampaignStop<WitchCovenLv2>());

            // level1EliteEncounters
            eliteEncounters[2].Add(new TypedEncounterCampaignStop<HallOfSmokeLv3>());
            eliteEncounters[0].Add(new TypedEncounterCampaignStop<WitchCovenLv3>());

            // Boss Fights
            bossFights.Add(new TypedEncounterCampaignStop<Boss_DriderFight>());
        }
    }
}
