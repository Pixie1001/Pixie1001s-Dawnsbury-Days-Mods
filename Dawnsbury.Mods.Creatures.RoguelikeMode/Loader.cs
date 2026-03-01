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
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level4;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using HarmonyLib;
using System;
using System.IO.Enumeration;
using Dawnsbury.Campaign.Path;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Loader
    {
        public static bool[] SetIcon = [false];
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

            // ParryLogic.Load("Roguelike Mode", new ModdedIllustration("RoguelikeModeAssets/Icons/ParryT7.png"), new ModdedIllustration("RoguelikeModeAssets/Icons/ParryT7.png"));
            ParryLogic.Load("Roguelike Mode", Illustrations.HuntingSpider, Illustrations.HuntingSpider);
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

            ModManager.RegisterBooleanSettingsOption("RL_AllowRatMonarch", "Roguelike Mode: Unlock Skill Challenge Dedications.",
                "Allows characters to take secret event only archetype dedication feats, such as the Rat Monarch, in all game modes. These archetypes are deliberately designed to be much stronger than a typical archetype, and are not recommended for a balanced challenge.", false);
        }

        private static void LoadEncounters()
        {
            ModManager.RegisterEncounter<HallOfBeginnings>("HallOfBeginnings.tmx");

            ModManager.RegisterEncounter<AlchemicalAmbushLv1>("AlchemicalAmbush.tmx");
            ModManager.RegisterEncounter<DrowAmbushLv1>("DrowAmbush.tmx");
            ModManager.RegisterEncounter<Colosseum1Lv1>("Colosseum.tmx");
            RegisterEncounter<Colosseum2Lv1>("Colosseum.tmx", "ColosseumMagicVarient");
            ModManager.RegisterEncounter<CorruptedSwampLv1>("CorruptedSwamp.tmx");
            ModManager.RegisterEncounter<InquisitrixTrapLv1>("InquisitrixTrapLv2.tmx");
            ModManager.RegisterEncounter<RatSwarmLv1>("RatSwarmLv2.tmx");
            ModManager.RegisterEncounter<SpiderNestLv1>("SpiderNest.tmx");
            ModManager.RegisterEncounter<TempleOfTheSpiderQueenLv1>("TempleOfTheSpiderQueen.tmx");
            ModManager.RegisterEncounter<AbandonedTempleLv1>("AbandonedTemple.tmx");
            ModManager.RegisterEncounter<FungalForestlv1>("FungalForest.tmx");
            ModManager.RegisterEncounter<HideAndSeekLv1>("HideAndSeek.tmx");
            ModManager.RegisterEncounter<ShadowCasterSanctumLv1>("ShadowcasterSanctum.tmx");
            ModManager.RegisterEncounter<MysteriousRoom_FeyL1>("MysteriousRoom_Fey.tmx");
            ModManager.RegisterEncounter<MysteriousRoom_HomunculusL1>("MysteriousRoom_Homunculus.tmx");
            ModManager.RegisterEncounter<MysteriousRoom_ArtRoomL1>("MysteriousRoom_ArtRoom.tmx");
            ModManager.RegisterEncounter<HatcheryLv1>("Hatchery.tmx");
            ModManager.RegisterEncounter<GraveEncounterLv1>("GraveEncounter.tmx");
            ModManager.RegisterEncounter<RitualSiteLv1>("RitualSite.tmx");
            ModManager.RegisterEncounter<DrowPatrolLv1>("DrowPatrol.tmx");
            ModManager.RegisterEncounter<MerfolkHuntingPartyLv1>("MerfolkhuntingParty.tmx");
            ModManager.RegisterEncounter<CultOfTheBrineLv1>("CultOfTheBrine.tmx");
            ModManager.RegisterEncounter<ChosenOfTheKrakenLv1>("ChosenOfTheKraken.tmx");
            ModManager.RegisterEncounter<BesetByMinnowsLv1>("BesetByMinnows1.tmx");
            ModManager.RegisterEncounter<BesetByMinnowsLv2>("BesetByMinnows2.tmx");
            ModManager.RegisterEncounter<BesetByMinnowsLv3>("BesetByMinnows3.tmx");
            ModManager.RegisterEncounter<DrowSalavagingPartyLv1>("DrowSalvagingParty.tmx");
            ModManager.RegisterEncounter<KoboldAmbushLv1>("KoboldAmbush.tmx");
            ModManager.RegisterEncounter<KoboldWarrenLv1>("KoboldWarren.tmx");

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
            RegisterEncounter<AntiPartyLv1>("Antiparty.tmx", "AntipartyLv1");
            RegisterEncounter<AqueductsLv1>("Aqueducts.tmx", "AqueductsLv1");
            RegisterEncounter<MaidenOfTheLostLv1>("Elite_WitchMaiden.tmx", "MaidenOfTheLostLv1");
            RegisterEncounter<MotherOfThePoolLv1>("Elite_WitchMother.tmx", "MotherOfThePoolLv1");
            RegisterEncounter<CroneOfTheWildsLv1>("Elite_WitchCrone.tmx", "CroneOfTheWildsLv1");
            RegisterEncounter<GrandStaircaseL1>("GrandStaircase.tmx", "GrandStaircaseL1");
            RegisterEncounter<LairOfTheDriderLv1>("Elite_LairOfTheDrider.tmx", "LairOfTheDriderLv1");

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
