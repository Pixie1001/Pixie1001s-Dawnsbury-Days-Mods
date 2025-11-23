using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Encounters.Tutorial;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level4;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using HarmonyLib;
using System;
using System.IO.Enumeration;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Loader
    {
        public static string UnderdarkName = "Below";

        public static string[] Seed = ["null"];

        internal static string Credits { get; } =
            "{b}CREDITS for the Roguelike Mode Mod{/b}\n\n" +
            "{b}Lead design, writing, direction and programming: {/b} Pixie1001\n" +
            "{b}Artists: {/b} Pixie1001, Nacraova, Lobot922\n" +
            "{b}Additional design: {/b} SudoProgramming, DINGLEBOB, El Moondo\n" +
            "{b}Additional writers: {/b} El Moondo\n" +
            "{b}Additional programming: {/b} SudoProgramming, DINGLEBOB, El Moondo\n" +
            "{b}Playtesting: {/b} Petr, Beets, SudoProgramming, L-star, RussischerZar, Vinicius Medeiros, FlamingQaz";

        internal static Dictionary<CreatureId, Func<Encounter?, Creature>> Creatures = new Dictionary<CreatureId, Func<Encounter?, Creature>>();

        [DawnsburyDaysModMainMethod]
        public static void LoadMod()
        {
            var harmony = new Harmony("Dawnsbury.Mods.GameModes.RoguelikeMode");
            Harmony.DEBUG = false;
            harmony.PatchAll();

            ParryLogic.Load("Roguelike Mode", new ModdedIllustration("RoguelikeModeAssets/Icons/ParryT7.png"), new ModdedIllustration("RoguelikeModeAssets/Icons/ParryT7.png"));
            ActionIds.RegisterConflictedIds();
            QEffectIds.RegisterConflictedIds();
            CustomItems.LoadItems();
            CreatureList.LoadCreatures();
            CreatureList.LoadObjects();
            CreatureList.LoadTiles();
            CreatureList.ModifyCreatures();
            LTEs.LoadLongTermEffects();
            SpellLoader.LoadSpells();
            FeatLoader.LoadFeats();
            MonsterMutatorTable.LoadMutators();
            LoadEncounters();

            ModManager.RegisterBooleanSettingsOption("RL_HideDeathIcon", "Roguelike Mode: Hide failed run icons.",
                "By default saves files with at least one death or restart will be marked by a scary green skull icon. Enable this if you want play the Roguelike mode like a regular randomly generated adventure path, without worrying about how many times you died or restarted an encounter.", false);

            ModManager.RegisterBooleanSettingsOption("RL_Corruption2", "Roguelike Mode: Enable monster archetypes in free encounter mode.",
                "Makes regular encounters from the Roguelike mode harder.", false);
        }

        private static void LoadEncounters()
        {
            ModManager.RegisterEncounter<HallOfBeginnings>("HallOfBeginnings.tmx");

            RegisterEncounter<AlchemicalAmbushLv1>("AlchemicalAmbush.tmx", "AlchemicalAmbushLv1");
            ModManager.RegisterEncounter<AlchemicalAmbushLv2>("AlchemicalAmbush.tmx");
            RegisterEncounter<AlchemicalAmbushLv3>("AlchemicalAmbush.tmx", "AlchemicalAmbushLv3");
            RegisterEncounter<DrowAmbushLv1>("DrowAmbush.tmx", "DrowAmbushLv1");
            RegisterEncounter<DrowAmbushLv2>("DrowAmbush.tmx", "DrowAmbushLv2");
            RegisterEncounter<DrowAmbushLv3>("DrowAmbush.tmx", "DrowAmbushLv3");
            RegisterEncounter<Colosseum1Lv1>("Colosseum.tmx", "Colosseum1Lv1");
            RegisterEncounter<Colosseum1Lv2>("Colosseum.tmx", "Colosseum1Lv2");
            RegisterEncounter<Colosseum1Lv3>("Colosseum.tmx", "Colosseum1Lv3");
            RegisterEncounter<Colosseum2Lv1>("Colosseum.tmx", "Colosseum2Lv1");
            ModManager.RegisterEncounter<Colosseum2Lv2>("Colosseum.tmx");
            RegisterEncounter<Colosseum2Lv3>("Colosseum.tmx", "Colosseum2Lv3");
            RegisterEncounter<CorruptedSwampLv1>("CorruptedSwamp.tmx", "CorruptedSwampLv1");
            ModManager.RegisterEncounter<CorruptedSwampLv2>("CorruptedSwamp.tmx");
            RegisterEncounter<CorruptedSwampLv3>("CorruptedSwamp.tmx", "CorruptedSwampLv3");
            RegisterEncounter<InquisitrixTrapLv1>("InquisitrixTrapLv2.tmx", "InquisitrixTrapLv1");
            RegisterEncounter<InquisitrixTrapLv2>("InquisitrixTrapLv2.tmx", "InquisitrixTrapLv2");
            RegisterEncounter<InquisitrixTrapLv3>("InquisitrixTrapLv2.tmx", "InquisitrixTrapLv3");
            RegisterEncounter<RatSwarmLv1>("RatSwarmLv2.tmx", "RatSwarmLv1");
            RegisterEncounter<RatSwarmLv2>("RatSwarmLv2.tmx", "RatSwarmLv2");
            RegisterEncounter<RatSwarmLv3>("RatSwarmLv2.tmx", "RatSwarmLv3");
            RegisterEncounter<SpiderNestLv1>("SpiderNest.tmx", "SpiderNestLv1");
            RegisterEncounter<SpiderNestLv2>("SpiderNest.tmx", "SpiderNestLv2");
            RegisterEncounter<SpiderNestLv3>("SpiderNest.tmx", "SpiderNestLv3");
            RegisterEncounter<TempleOfTheSpiderQueenLv1>("TempleOfTheSpiderQueen.tmx", "TempleOfTheSpiderQueenLv1");
            RegisterEncounter<TempleOfTheSpiderQueenLv2>("TempleOfTheSpiderQueen.tmx", "TempleOfTheSpiderQueenLv2");
            RegisterEncounter<TempleOfTheSpiderQueenLv3>("TempleOfTheSpiderQueen.tmx", "TempleOfTheSpiderQueenLv3");
            RegisterEncounter<AbandonedTempleLv1>("AbandonedTemple.tmx", "AbandonedTempleLv1");
            ModManager.RegisterEncounter<AbandonedTempleLv2>("AbandonedTemple.tmx");
            RegisterEncounter<AbandonedTempleLv3>("AbandonedTemple.tmx", "AbandonedTempleLv3");
            RegisterEncounter<FungalForestlv1>("FungalForest.tmx", "FungalForestLv1");
            RegisterEncounter<FungalForestlv2>("FungalForest.tmx", "FungalForestLv2");
            RegisterEncounter<FungalForestlv3>("FungalForest.tmx", "FungalForestLv3");
            RegisterEncounter<HideAndSeekLv1>("HideAndSeek.tmx", "HideAndSeekLv1");
            RegisterEncounter<HideAndSeekLv2>("HideAndSeek.tmx", "HideAndSeekLv2");
            RegisterEncounter<HideAndSeekLv3>("HideAndSeek.tmx", "HideAndSeekLv3");
            RegisterEncounter<ShadowCasterSanctumLv1>("ShadowcasterSanctum.tmx", "ShadowCasterSanctumLv1");
            RegisterEncounter<ShadowCasterSanctumLv2>("ShadowcasterSanctum.tmx", "ShadowCasterSanctumLv2");
            RegisterEncounter<ShadowCasterSanctumLv3>("ShadowcasterSanctum.tmx", "ShadowCasterSanctumLv3");
            RegisterEncounter<MysteriousRoom_FeyL1>("MysteriousRoom_Fey.tmx", "MysteriousRoom_FeyL1");
            RegisterEncounter<MysteriousRoom_FeyL2>("MysteriousRoom_Fey.tmx", "MysteriousRoom_FeyL2");
            RegisterEncounter<MysteriousRoom_FeyL3>("MysteriousRoom_Fey.tmx", "MysteriousRoom_FeyL3");
            RegisterEncounter<MysteriousRoom_HomunculusL1>("MysteriousRoom_Homunculus.tmx", "MysteriousRoom_HomunculusL1");
            ModManager.RegisterEncounter<MysteriousRoom_HomunculusL2>("MysteriousRoom_Homunculus.tmx");
            RegisterEncounter<MysteriousRoom_HomunculusL3>("MysteriousRoom_Homunculus.tmx", "MysteriousRoom_HomunculusL3");
            RegisterEncounter<MysteriousRoom_ArtRoomL1>("MysteriousRoom_ArtRoom.tmx", "MysteriousRoom_ArtRoomL1");
            RegisterEncounter<MysteriousRoom_ArtRoomL2>("MysteriousRoom_ArtRoom.tmx", "MysteriousRoom_ArtRoomL2");
            RegisterEncounter<MysteriousRoom_ArtRoomL3>("MysteriousRoom_ArtRoom.tmx", "MysteriousRoom_ArtRoomL3");
            RegisterEncounter<HatcheryLv1>("Hatchery.tmx", "HatcheryLv1");
            ModManager.RegisterEncounter<HatcheryLv2>("Hatchery.tmx");
            RegisterEncounter<HatcheryLv3>("Hatchery.tmx", "HatcheryLv3");
            RegisterEncounter<GraveEncounterLv1>("GraveEncounter.tmx", "GraveEncounterLv1");
            RegisterEncounter<GraveEncounterLv2>("GraveEncounter.tmx", "GraveEncounterLv2");
            RegisterEncounter<GraveEncounterLv3>("GraveEncounter.tmx", "GraveEncounterLv3");
            RegisterEncounter<RitualSiteLv1>("RitualSite.tmx", "RitualSiteLv1");
            RegisterEncounter<RitualSiteLv2>("RitualSite.tmx", "RitualSiteLv2");
            RegisterEncounter<RitualSiteLv3>("RitualSite.tmx", "RitualSiteLv3");
            RegisterEncounter<DrowPatrolLv1>("DrowPatrol.tmx", "DrowPatrolLv1");
            RegisterEncounter<DrowPatrolLv2>("DrowPatrol.tmx", "DrowPatrolLv2");
            RegisterEncounter<DrowPatrolLv3>("DrowPatrol.tmx", "DrowPatrolLv3");
            RegisterEncounter<MerfolkHuntingPartyLv1>("MerfolkhuntingParty.tmx", "MerfolkHuntingPartyLv1");
            ModManager.RegisterEncounter<MerfolkHuntingPartyLv2>("MerfolkhuntingParty.tmx");
            RegisterEncounter<MerfolkHuntingPartyLv3>("MerfolkhuntingParty.tmx", "MerfolkHuntingPartyLv3");
            RegisterEncounter<CultOfTheBrineLv1>("CultOfTheBrine.tmx", "CultOfTheBrineLv1");
            ModManager.RegisterEncounter<CultOfTheBrineLv2>("CultOfTheBrine.tmx");
            RegisterEncounter<CultOfTheBrineLv3>("CultOfTheBrine.tmx", "CultOfTheBrineLv3");
            RegisterEncounter<ChosenOfTheKrakenLv1>("ChosenOfTheKraken.tmx", "ChosenOfTheKrakenLv1");
            ModManager.RegisterEncounter<ChosenOfTheKrakenLv2>("ChosenOfTheKraken.tmx");
            RegisterEncounter<ChosenOfTheKrakenLv3>("ChosenOfTheKraken.tmx", "ChosenOfTheKrakenLv3");
            ModManager.RegisterEncounter<BesetByMinnowsLv1>("BesetByMinnows1.tmx");
            ModManager.RegisterEncounter<BesetByMinnowsLv2>("BesetByMinnows2.tmx");
            ModManager.RegisterEncounter<BesetByMinnowsLv3>("BesetByMinnows3.tmx");
            RegisterEncounter<DrowSalavagingPartyLv1>("DrowSalvagingParty.tmx", "DrowSalvagingPartyLv1");
            ModManager.RegisterEncounter<DrowSalavagingPartyLv2>("DrowSalvagingParty.tmx");
            RegisterEncounter<DrowSalavagingPartyLv3>("DrowSalvagingParty.tmx", "DrowSalvagingPartyLv3");
            RegisterEncounter<KoboldAmbushLv1>("KoboldAmbush.tmx", "KoboldAmbushLv1");
            ModManager.RegisterEncounter<KoboldAmbushLv2>("KoboldAmbush.tmx");
            RegisterEncounter<KoboldAmbushLv3>("KoboldAmbush.tmx", "KoboldAmbushLv3");
            RegisterEncounter<KoboldWarrenLv1>("KoboldWarren.tmx", "KoboldWarrenLv1");
            ModManager.RegisterEncounter<KoboldWarrenLv2>("KoboldWarren.tmx");
            RegisterEncounter<KoboldWarrenLv3>("KoboldWarren.tmx", "KoboldWarrenLv3");

            // Level 4 Fights
            ModManager.RegisterEncounter<DemonWebPits>("DemonWebPits.tmx");
            ModManager.RegisterEncounter<DrowSentinels>("DrowSentinels.tmx");
            ModManager.RegisterEncounter<EarthenGuardians>("EarthenGuardians.tmx");
            ModManager.RegisterEncounter<FeedingFrenzy>("FeedingFrenzy.tmx");
            ModManager.RegisterEncounter<GuardedPassage>("GuardedPassage.tmx");
            ModManager.RegisterEncounter<MagesTower>("MagesTower.tmx");
            ModManager.RegisterEncounter<RottingVigils>("RottingVigils.tmx");
            ModManager.RegisterEncounter<RuinedHamlet>("RuinedHamlet.tmx");
            ModManager.RegisterEncounter<SuccubusCult>("SuccubusCult.tmx");
            ModManager.RegisterEncounter<SplitTheParty>("SplittheParty.tmx");

            // High Level Encounters
            ModManager.RegisterEncounter<DemonGate>("HL_N_DemonGate.tmx");
            ModManager.RegisterEncounter<BirthingPools>("HL_N_BirthingPools.tmx");
            ModManager.RegisterEncounter<CathedralOfTheSpider>("HL_N_CathedralOfTheSpider.tmx");
            ModManager.RegisterEncounter<CultistCheckPoint>("HL_N_CultistCheckPoint.tmx");
            ModManager.RegisterEncounter<CultistPatrol1>("HL_N_CultistPatrol.tmx");
            ModManager.RegisterEncounter<CultistPatrol2>("HL_N_CultistPatrol2.tmx");
            ModManager.RegisterEncounter<DemonicAmbush>("HL_N_DemonicAmbush.tmx");
            ModManager.RegisterEncounter<DragonflyTemple>("HL_N_DragonflyTemple.tmx");
            ModManager.RegisterEncounter<DrowScoutingParty>("HL_N_DrowScoutingParty.tmx");
            ModManager.RegisterEncounter<FrozenCrevasse>("HL_N_FrozenCrevasse.tmx");
            ModManager.RegisterEncounter<HarvestedVillage>("HL_N_HarvestedVillage.tmx");
            ModManager.RegisterEncounter<NightmareDomain>("HL_N_NightmareDomain.tmx");
            ModManager.RegisterEncounter<VipersNest>("HL_N_ViperNest.tmx");
            ModManager.RegisterEncounter<AbandonedVillage>("HL_N_AbandonedVillage.tmx");
            ModManager.RegisterEncounter<Archways>("HL_N_Archways.tmx");

            // High Level Elite Encounters
            ModManager.RegisterEncounter<SpinnerOfLies>("HL_Elite_WeaverOfLies.tmx");
            ModManager.RegisterEncounter<ArisasCourt>("HL_Elite_Arisa'sCourt.tmx");
            ModManager.RegisterEncounter<ChimeraDen>("HL_Elite_ChimeraDen.tmx");
            ModManager.RegisterEncounter<EchidnaditeHighPriestess>("HL_Elite_EchidnaditeHighPriestess.tmx");
            ModManager.RegisterEncounter<GrandTemple>("HL_Elite_GrandTemple.tmx");
            ModManager.RegisterEncounter<Medusa>("HL_Elite_Medusa.tmx");

            // Skill Challenge Fights
            ModManager.RegisterEncounter<DefendTheReliquary>("Event_ReliquaryDefence.tmx");

            // Elite Fights
            RegisterEncounter<HallOfSmokeLv1>("Elite_HallOfSmokeLv2.tmx", "Elite_HallOfSmokeLv1");
            RegisterEncounter<HallOfSmokeLv2>("Elite_HallOfSmokeLv2.tmx", "Elite_HallOfSmokeLv2");
            RegisterEncounter<HallOfSmokeLv3>("Elite_HallOfSmokeLv2.tmx", "Elite_HallOfSmokeLv3");
            RegisterEncounter<AntiPartyLv1>("Antiparty.tmx", "AntipartyLv1");
            ModManager.RegisterEncounter<AntiPartyLv2>("Antiparty.tmx");
            RegisterEncounter<AntiPartyLv3>("Antiparty.tmx", "AntipartyLv3");
            RegisterEncounter<AqueductsLv1>("Aqueducts.tmx", "AqueductsLv1");
            RegisterEncounter<AqueductsLv2>("Aqueducts.tmx", "AqueductsLv2");
            RegisterEncounter<AqueductsLv3>("Aqueducts.tmx", "AqueductsLv3");
            RegisterEncounter<MaidenOfTheLostLv1>("Elite_WitchMaiden.tmx", "MaidenOfTheLostLv1");
            ModManager.RegisterEncounter<MaidenOfTheLostLv2>("Elite_WitchMaiden.tmx");
            RegisterEncounter<MaidenOfTheLostLv3>("Elite_WitchMaiden.tmx", "MaidenOfTheLostLv3");
            RegisterEncounter<MotherOfThePoolLv1>("Elite_WitchMother.tmx", "MotherOfThePoolLv1");
            ModManager.RegisterEncounter<MotherOfThePoolLv2>("Elite_WitchMother.tmx");
            RegisterEncounter<MotherOfThePoolLv3>("Elite_WitchMother.tmx", "MotherOfThePoolLv3");
            RegisterEncounter<CroneOfTheWildsLv1>("Elite_WitchCrone.tmx", "CroneOfTheWildsLv1");
            ModManager.RegisterEncounter<CroneOfTheWildsLv2>("Elite_WitchCrone.tmx");
            RegisterEncounter<CroneOfTheWildsLv3>("Elite_WitchCrone.tmx", "CroneOfTheWildsLv3");
            RegisterEncounter<GrandStaircaseL1>("GrandStaircase.tmx", "GrandStaircaseL1");
            RegisterEncounter<GrandStaircaseL2>("GrandStaircase.tmx", "GrandStaircaseL2");
            RegisterEncounter<GrandStaircaseL3>("GrandStaircase.tmx", "GrandStaircaseL3");
            RegisterEncounter<LairOfTheDriderLv1>("Elite_LairOfTheDrider.tmx", "LairOfTheDriderLv1");
            RegisterEncounter<LairOfTheDriderLv2>("Elite_LairOfTheDrider.tmx", "LairOfTheDriderLv2");
            RegisterEncounter<LairOfTheDriderLv3>("Elite_LairOfTheDrider.tmx", "LairOfTheDriderLv3");

            // Low Lvl Boss fights
            ModManager.RegisterEncounter<Boss_DriderFight>("Boss_DriderFight.tmx");
            ModManager.RegisterEncounter<Boss_WitchCoven>("Elite_WitchCoven.tmx");
            ModManager.RegisterEncounter<Boss_Handmaiden>("Boss_Handmaiden.tmx");
            ModManager.RegisterEncounter<Boss_FrozenTemple>("FrozenTemple.tmx");
            ModManager.RegisterEncounter<Boss_CoralCourt>("Boss_CourtOfTheCoralQueen.tmx");

            // High Lvl Boss fights
            ModManager.RegisterEncounter<Boss_DrowPrincesses>("HL_Boss_DrowPrincesses.tmx");
            ModManager.RegisterEncounter<Boss_DragonWitch>("HL_Boss_DragonWitch.tmx");

            // Skill Challenges
            ModManager.RegisterEncounter<SkillChallengeEncounter>("SkillChallenge.tmx");

            // Other
            ModManager.RegisterEncounter<TestMap>("TestHall.tmx");
        }

        private static void RegisterEncounter<T>(string filename, string key) where T : Encounter
        {
            RegisteredEncounters.RegisteredEncounterInfo registeredEncounterInfo = new RegisteredEncounters.RegisteredEncounterInfo(() => (Encounter)Activator.CreateInstance(typeof(T), filename)!);
            RegisteredEncounters.RegisteredEncountersBySimpleFilename.Add(key, registeredEncounterInfo);
            RegisteredEncounters.RegisteredEncountersByType.Add(typeof(T), registeredEncounterInfo);
        }
    }
}

// public static void RegisterInlineTooltip(string code, string tooltipText)
