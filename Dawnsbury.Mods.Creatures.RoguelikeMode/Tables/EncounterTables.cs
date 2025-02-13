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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Tables {
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
            encounters[0].Add(new TypedEncounterCampaignStop<AlchemicalAmbushLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<BesetByMinnowsLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<DrowAmbushLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<RatSwarmLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<InquisitrixTrapLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<SpiderNestLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<TempleOfTheSpiderQueenLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<AbandonedTempleLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<FungalForestlv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<HideAndSeekLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<ShadowCasterSanctumLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<MysteriousRoom_FeyL1>());
            encounters[0].Add(new TypedEncounterCampaignStop<MysteriousRoom_HomunculusL1>());
            encounters[0].Add(new TypedEncounterCampaignStop<MysteriousRoom_ArtRoomL1>());
            encounters[0].Add(new TypedEncounterCampaignStop<GraveEncounterLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<HatcheryLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<RitualSiteLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<DrowPatrolLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<MerfolkHuntingPartyLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<CultOfTheBrineLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<ChosenOfTheKrakenLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<DrowSalavagingPartyLv1>());

            // level2Encounters
            encounters[1].Add(new TypedEncounterCampaignStop<AlchemicalAmbushLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<BesetByMinnowsLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<DrowAmbushLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<RatSwarmLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<InquisitrixTrapLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<SpiderNestLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<TempleOfTheSpiderQueenLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<AbandonedTempleLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<FungalForestlv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<HideAndSeekLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<ShadowCasterSanctumLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<MysteriousRoom_FeyL2>());
            encounters[1].Add(new TypedEncounterCampaignStop<MysteriousRoom_HomunculusL2>());
            encounters[1].Add(new TypedEncounterCampaignStop<MysteriousRoom_ArtRoomL2>());
            encounters[1].Add(new TypedEncounterCampaignStop<GraveEncounterLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<HatcheryLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<RitualSiteLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<DrowPatrolLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<MerfolkHuntingPartyLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<CultOfTheBrineLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<ChosenOfTheKrakenLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<DrowSalavagingPartyLv2>());

            // level3Encounters
            encounters[2].Add(new TypedEncounterCampaignStop<AlchemicalAmbushLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<BesetByMinnowsLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<DrowAmbushLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<RatSwarmLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<InquisitrixTrapLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<SpiderNestLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<TempleOfTheSpiderQueenLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<AbandonedTempleLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<FungalForestlv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<HideAndSeekLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<ShadowCasterSanctumLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<MysteriousRoom_FeyL3>());
            encounters[2].Add(new TypedEncounterCampaignStop<MysteriousRoom_HomunculusL3>());
            encounters[2].Add(new TypedEncounterCampaignStop<MysteriousRoom_ArtRoomL3>());
            encounters[2].Add(new TypedEncounterCampaignStop<GraveEncounterLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<HatcheryLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<RitualSiteLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<DrowPatrolLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<MerfolkHuntingPartyLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<CultOfTheBrineLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<ChosenOfTheKrakenLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<DrowSalavagingPartyLv3>());

            // level1EliteEncounters
            eliteEncounters[0].Add(new TypedEncounterCampaignStop<HallOfSmokeLv1>());
            eliteEncounters[0].Add(new TypedEncounterCampaignStop<AqueductsLv1>());
            eliteEncounters[0].Add(new TypedEncounterCampaignStop<AntiPartyLv1>());
            eliteEncounters[0].Add(new TypedEncounterCampaignStop<MaidenOfTheLostLv1>());
            eliteEncounters[0].Add(new TypedEncounterCampaignStop<MotherOfThePoolLv1>());
            eliteEncounters[0].Add(new TypedEncounterCampaignStop<CroneOfTheWildsLv1>());
            eliteEncounters[0].Add(new TypedEncounterCampaignStop<GrandStaircaseL1>());
            eliteEncounters[0].Add(new TypedEncounterCampaignStop<LairOfTheDriderLv1>());

            // level2EliteEncounters
            eliteEncounters[1].Add(new TypedEncounterCampaignStop<HallOfSmokeLv2>());
            eliteEncounters[1].Add(new TypedEncounterCampaignStop<AqueductsLv2>());
            eliteEncounters[1].Add(new TypedEncounterCampaignStop<AntiPartyLv2>());
            eliteEncounters[1].Add(new TypedEncounterCampaignStop<MaidenOfTheLostLv2>());
            eliteEncounters[1].Add(new TypedEncounterCampaignStop<MotherOfThePoolLv2>());
            eliteEncounters[1].Add(new TypedEncounterCampaignStop<CroneOfTheWildsLv2>());
            eliteEncounters[1].Add(new TypedEncounterCampaignStop<GrandStaircaseL2>());
            eliteEncounters[1].Add(new TypedEncounterCampaignStop<LairOfTheDriderLv2>());

            // level3EliteEncounters
            eliteEncounters[2].Add(new TypedEncounterCampaignStop<HallOfSmokeLv3>());
            eliteEncounters[2].Add(new TypedEncounterCampaignStop<AqueductsLv3>());
            eliteEncounters[2].Add(new TypedEncounterCampaignStop<AntiPartyLv3>());
            eliteEncounters[2].Add(new TypedEncounterCampaignStop<MaidenOfTheLostLv3>());
            eliteEncounters[2].Add(new TypedEncounterCampaignStop<MotherOfThePoolLv3>());
            eliteEncounters[2].Add(new TypedEncounterCampaignStop<CroneOfTheWildsLv3>());
            eliteEncounters[2].Add(new TypedEncounterCampaignStop<GrandStaircaseL3>());
            eliteEncounters[2].Add(new TypedEncounterCampaignStop<LairOfTheDriderLv3>());

            // Boss Fights
            bossFights.Add(new TypedEncounterCampaignStop<Boss_DriderFight>());
            bossFights.Add(new TypedEncounterCampaignStop<Boss_WitchCoven>());
            bossFights.Add(new TypedEncounterCampaignStop<Boss_Handmaiden>());
            bossFights.Add(new TypedEncounterCampaignStop<Boss_FrozenTemple>());
            bossFights.Add(new TypedEncounterCampaignStop<Boss_CoralCourt>());
        }
    }
}
