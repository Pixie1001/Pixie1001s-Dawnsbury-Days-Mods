using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Tables {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class EncounterTables {
        public static List<EncounterCampaignStop>[] encounters = new List<EncounterCampaignStop>[8];
        public static List<EncounterCampaignStop>[] eliteEncounters = new List<EncounterCampaignStop>[8];
        public static List<EncounterCampaignStop>[] bossFights = new List<EncounterCampaignStop>[2];

        public static void ApplySpecialEncounters(CampaignState state) {
            var output = "";
            var index = 0;

            if (state.Tags.TryGetValue("DefendTheReliquary", out output) && Int32.TryParse(output, out index)) {
                state.AdventurePath!.CampaignStops[index] = new TypedEncounterCampaignStop<DefendTheReliquary>();
                state.AdventurePath!.CampaignStops[index].Index = index;
            }

            // Add more event encounters here
        }

        public static void LoadEncounterTables() {
            bossFights[0] = new List<EncounterCampaignStop>();
            bossFights[1] = new List<EncounterCampaignStop>();

            for (int i = 0; i < encounters.Length; i++) {
                encounters[i] = new List<EncounterCampaignStop>();
            }

            for (int i = 0; i < eliteEncounters.Length; i++) {
                eliteEncounters[i] = new List<EncounterCampaignStop>();
            }

            // level1Encounters
            encounters[0].Add(new TypedEncounterCampaignStop<AlchemicalAmbushLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<BesetByMinnowsLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<Colosseum1Lv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<Colosseum2Lv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<CorruptedSwampLv1>());
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
            encounters[0].Add(new TypedEncounterCampaignStop<KoboldWarrenLv1>());
            encounters[0].Add(new TypedEncounterCampaignStop<KoboldAmbushLv1>());

            // level2Encounters
            encounters[1].Add(new TypedEncounterCampaignStop<AlchemicalAmbushLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<BesetByMinnowsLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<Colosseum1Lv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<Colosseum2Lv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<CorruptedSwampLv2>());
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
            encounters[1].Add(new TypedEncounterCampaignStop<KoboldWarrenLv2>());
            encounters[1].Add(new TypedEncounterCampaignStop<KoboldAmbushLv2>());

            // level3Encounters
            encounters[2].Add(new TypedEncounterCampaignStop<AlchemicalAmbushLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<BesetByMinnowsLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<Colosseum1Lv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<Colosseum2Lv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<CorruptedSwampLv3>());
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
            encounters[2].Add(new TypedEncounterCampaignStop<KoboldWarrenLv3>());
            encounters[2].Add(new TypedEncounterCampaignStop<KoboldAmbushLv3>());

            // level4Encounters
            encounters[3].Add(new TypedEncounterCampaignStop<DemonWebPits>());
            encounters[3].Add(new TypedEncounterCampaignStop<DrowSentinels>());
            encounters[3].Add(new TypedEncounterCampaignStop<EarthenGuardians>());
            encounters[3].Add(new TypedEncounterCampaignStop<FeedingFrenzy>());
            encounters[3].Add(new TypedEncounterCampaignStop<GuardedPassage>());
            encounters[3].Add(new TypedEncounterCampaignStop<MagesTower>());
            encounters[3].Add(new TypedEncounterCampaignStop<RottingVigils>());
            encounters[3].Add(new TypedEncounterCampaignStop<RuinedHamlet>());
            encounters[3].Add(new TypedEncounterCampaignStop<SuccubusCult>());
            encounters[3].Add(new TypedEncounterCampaignStop<SplitTheParty>());

            // level5Encounters
            encounters[4].Add(new TypedEncounterCampaignStop<DemonGate>());
            encounters[4].Add(new TypedEncounterCampaignStop<BirthingPools>());
            encounters[4].Add(new TypedEncounterCampaignStop<CathedralOfTheSpider>());
            encounters[4].Add(new TypedEncounterCampaignStop<CultistCheckPoint>());
            encounters[4].Add(new TypedEncounterCampaignStop<Archways>());
            encounters[4].Add(new TypedEncounterCampaignStop<CultistPatrol1>());
            encounters[4].Add(new TypedEncounterCampaignStop<CultistPatrol2>());
            encounters[4].Add(new TypedEncounterCampaignStop<DemonicAmbush>());
            encounters[4].Add(new TypedEncounterCampaignStop<DrowScoutingParty>());
            encounters[4].Add(new TypedEncounterCampaignStop<DragonflyTemple>());
            encounters[4].Add(new TypedEncounterCampaignStop<FrozenCrevasse>());
            encounters[4].Add(new TypedEncounterCampaignStop<HarvestedVillage>());
            encounters[4].Add(new TypedEncounterCampaignStop<VipersNest>());
            encounters[4].Add(new TypedEncounterCampaignStop<NightmareDomain>());
            encounters[4].Add(new TypedEncounterCampaignStop<AbandonedVillage>());

            // level6Encounters
            encounters[5].Add(new TypedEncounterCampaignStop<DemonGate>());
            encounters[5].Add(new TypedEncounterCampaignStop<BirthingPools>());
            encounters[5].Add(new TypedEncounterCampaignStop<CathedralOfTheSpider>());
            encounters[5].Add(new TypedEncounterCampaignStop<CultistCheckPoint>());
            encounters[5].Add(new TypedEncounterCampaignStop<Archways>());
            encounters[5].Add(new TypedEncounterCampaignStop<CultistPatrol1>());
            encounters[5].Add(new TypedEncounterCampaignStop<CultistPatrol2>());
            encounters[5].Add(new TypedEncounterCampaignStop<DemonicAmbush>());
            encounters[5].Add(new TypedEncounterCampaignStop<DrowScoutingParty>());
            encounters[5].Add(new TypedEncounterCampaignStop<DragonflyTemple>());
            encounters[5].Add(new TypedEncounterCampaignStop<FrozenCrevasse>());
            encounters[5].Add(new TypedEncounterCampaignStop<HarvestedVillage>());
            encounters[5].Add(new TypedEncounterCampaignStop<VipersNest>());
            encounters[5].Add(new TypedEncounterCampaignStop<NightmareDomain>());
            encounters[5].Add(new TypedEncounterCampaignStop<AbandonedVillage>());

            // level7Encounters
            encounters[6].Add(new TypedEncounterCampaignStop<DemonGate>());
            encounters[6].Add(new TypedEncounterCampaignStop<BirthingPools>());
            encounters[6].Add(new TypedEncounterCampaignStop<CathedralOfTheSpider>());
            encounters[6].Add(new TypedEncounterCampaignStop<CultistCheckPoint>());
            encounters[6].Add(new TypedEncounterCampaignStop<Archways>());
            encounters[6].Add(new TypedEncounterCampaignStop<CultistPatrol1>());
            encounters[6].Add(new TypedEncounterCampaignStop<CultistPatrol2>());
            encounters[6].Add(new TypedEncounterCampaignStop<DemonicAmbush>());
            encounters[6].Add(new TypedEncounterCampaignStop<DrowScoutingParty>());
            encounters[6].Add(new TypedEncounterCampaignStop<DragonflyTemple>());
            encounters[6].Add(new TypedEncounterCampaignStop<FrozenCrevasse>());
            encounters[6].Add(new TypedEncounterCampaignStop<HarvestedVillage>());
            encounters[6].Add(new TypedEncounterCampaignStop<VipersNest>());
            encounters[6].Add(new TypedEncounterCampaignStop<NightmareDomain>());
            encounters[6].Add(new TypedEncounterCampaignStop<AbandonedVillage>());

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

            // level5EliteEncounters
            eliteEncounters[4].Add(new TypedEncounterCampaignStop<SpinnerOfLies>());
            eliteEncounters[4].Add(new TypedEncounterCampaignStop<ArisasCourt>());
            eliteEncounters[4].Add(new TypedEncounterCampaignStop<ChimeraDen>());
            //eliteEncounters[4].Add(new TypedEncounterCampaignStop<EchidnaditeHighPriestess>());
            eliteEncounters[4].Add(new TypedEncounterCampaignStop<GrandTemple>());
            eliteEncounters[4].Add(new TypedEncounterCampaignStop<Medusa>());

            // level6EliteEncounters
            eliteEncounters[5].Add(new TypedEncounterCampaignStop<SpinnerOfLies>());
            eliteEncounters[5].Add(new TypedEncounterCampaignStop<ArisasCourt>());
            eliteEncounters[5].Add(new TypedEncounterCampaignStop<ChimeraDen>());
            //eliteEncounters[5].Add(new TypedEncounterCampaignStop<EchidnaditeHighPriestess>());
            eliteEncounters[5].Add(new TypedEncounterCampaignStop<GrandTemple>());
            eliteEncounters[5].Add(new TypedEncounterCampaignStop<Medusa>());

            // level7EliteEncounters
            eliteEncounters[6].Add(new TypedEncounterCampaignStop<SpinnerOfLies>());
            eliteEncounters[6].Add(new TypedEncounterCampaignStop<ArisasCourt>());
            eliteEncounters[6].Add(new TypedEncounterCampaignStop<ChimeraDen>());
            //eliteEncounters[6].Add(new TypedEncounterCampaignStop<EchidnaditeHighPriestess>());
            eliteEncounters[6].Add(new TypedEncounterCampaignStop<GrandTemple>());
            eliteEncounters[6].Add(new TypedEncounterCampaignStop<Medusa>());

            // Low Lvl Boss Fights
            bossFights[0].Add(new TypedEncounterCampaignStop<Boss_DriderFight>());
            bossFights[0].Add(new TypedEncounterCampaignStop<Boss_WitchCoven>());
            bossFights[0].Add(new TypedEncounterCampaignStop<Boss_Handmaiden>());
            bossFights[0].Add(new TypedEncounterCampaignStop<Boss_FrozenTemple>());
            bossFights[0].Add(new TypedEncounterCampaignStop<Boss_CoralCourt>());

            // High Lvl Boss Fights
            bossFights[1].Add(new TypedEncounterCampaignStop<Boss_DragonWitch>());
            bossFights[1].Add(new TypedEncounterCampaignStop<Boss_DrowPrincesses>());
        }
    }
}
