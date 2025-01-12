using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Description;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Dawnsbury.ThirdParty.SteamApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Intrinsics.Arm;
using System.Xml;
using Dawnsbury.Core.Mechanics.Damage;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;
using System.Text;
using static Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.BarbarianFeatsDb.AnimalInstinctFeat;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Metrics;
using Microsoft.Xna.Framework.Audio;
using static System.Reflection.Metadata.BlobBuilder;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Campaign.Encounters.Tutorial;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights;
using System.IO.Enumeration;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using HarmonyLib;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode
{

    // TODO: Add modular AI functions to making adding new enemies easier and lcean up creature list

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Loader
    {
        public static string UnderdarkName = "Below";

        internal static string Credits { get; } =
            "{b}CREDITS for the Roguelike Mode Mod{/b}\n\n" +
            "{b}Lead design, writing, direction and programming: {/b} Pixie1001\n" +
            "{b}Artists: {/b} Pixie1001\n" +
            "{b}Additional design: {/b} SudoProgramming, Dinglebob\n" +
            "{b}Additional writers: {/b} ...\n" +
            "{b}Additional programming: {/b} SudoProgramming, Dinglebob\n" +
            "{b}Playtesting: {/b} Petr, Beets, SudoProgramming";

        internal static Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>> Creatures = new Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>>();

        [DawnsburyDaysModMainMethod]
        public static void LoadMod()
        {
            var harmony = new Harmony("Dawnsbury.Mods.GameModes.RoguelikeMode");
            Harmony.DEBUG = true;
            harmony.PatchAll();

            CustomItems.LoadItems();
            CreatureList.LoadCreatures();
            CreatureList.LoadObjects();
            LTEs.LoadLongTermEffects();
            //LTEs.LoadBoons();
            //ScriptHooks.LoadHooks();
            SpellLoader.LoadSpells();
            LoadEncounters();
        }

        private static void LoadEncounters()
        {
            ModManager.RegisterEncounter<MerfolkHuntingPartyLv2>("MerfolkHuntingParty.tmx");
            ModManager.RegisterEncounter<CultOfTheBrineLv2>("CultOfTheBrine.tmx");

            ModManager.RegisterEncounter<HallOfBeginnings>("HallOfBeginnings.tmx");

            RegisterEncounter<DrowAmbushLv1>("DrowAmbush.tmx", "DrowAmbushLv1");
            RegisterEncounter<DrowAmbushLv2>("DrowAmbush.tmx", "DrowAmbushLv2");
            RegisterEncounter<DrowAmbushLv3>("DrowAmbush.tmx", "DrowAmbushLv3");
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
            RegisterEncounter<AbandonedTempleLv2>("AbandonedTemple.tmx", "AbandonedTempleLv2");
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
            RegisterEncounter<MysteriousRoom_HomunculusL2>("MysteriousRoom_Homunculus.tmx", "MysteriousRoom_HomunculusL2");
            RegisterEncounter<MysteriousRoom_HomunculusL3>("MysteriousRoom_Homunculus.tmx", "MysteriousRoom_HomunculusL3");
            RegisterEncounter<MysteriousRoom_ArtRoomL1>("MysteriousRoom_ArtRoom.tmx", "MysteriousRoom_ArtRoomL1");
            RegisterEncounter<MysteriousRoom_ArtRoomL2>("MysteriousRoom_ArtRoom.tmx", "MysteriousRoom_ArtRoomL2");
            RegisterEncounter<MysteriousRoom_ArtRoomL3>("MysteriousRoom_ArtRoom.tmx", "MysteriousRoom_ArtRoomL3");
            RegisterEncounter<HatcheryLv1>("Hatchery.tmx", "HatcheryLv1");
            RegisterEncounter<HatcheryLv2>("Hatchery.tmx", "HatcheryLv2");
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
            RegisterEncounter<MerfolkHuntingPartyLv2>("MerfolkhuntingParty.tmx", "MerfolkHuntingPartyLv2");
            RegisterEncounter<MerfolkHuntingPartyLv3>("MerfolkhuntingParty.tmx", "MerfolkHuntingPartyLv3");
            RegisterEncounter<CultOfTheBrineLv1>("CultOfTheBrine.tmx", "CultOfTheBrineLv1");
            RegisterEncounter<CultOfTheBrineLv2>("CultOfTheBrine.tmx", "CultOfTheBrineLv2");
            RegisterEncounter<CultOfTheBrineLv3>("CultOfTheBrine.tmx", "CultOfTheBrineLv3");

            // Elite Fights
            RegisterEncounter<HallOfSmokeLv1>("Elite_HallOfSmokeLv2.tmx", "Elite_HallOfSmokeLv1");
            RegisterEncounter<HallOfSmokeLv2>("Elite_HallOfSmokeLv2.tmx", "Elite_HallOfSmokeLv2");
            RegisterEncounter<HallOfSmokeLv3>("Elite_HallOfSmokeLv2.tmx", "Elite_HallOfSmokeLv3");
            RegisterEncounter<AntiPartyLv1>("Antiparty.tmx", "AntipartyLv1");
            RegisterEncounter<AntiPartyLv2>("Antiparty.tmx", "AntipartyLv2");
            RegisterEncounter<AntiPartyLv3>("Antiparty.tmx", "AntipartyLv3");
            RegisterEncounter<AqueductsLv1>("Aqueducts.tmx", "AqueductsLv1");
            RegisterEncounter<AqueductsLv2>("Aqueducts.tmx", "AqueductsLv2");
            RegisterEncounter<AqueductsLv3>("Aqueducts.tmx", "AqueductsLv3");
            RegisterEncounter<MaidenOfTheLostLv1>("Elite_WitchMaiden.tmx", "MaidenOfTheLostLv1");
            RegisterEncounter<MaidenOfTheLostLv2>("Elite_WitchMaiden.tmx", "MaidenOfTheLostLv2");
            RegisterEncounter<MaidenOfTheLostLv3>("Elite_WitchMaiden.tmx", "MaidenOfTheLostLv3");
            RegisterEncounter<MotherOfThePoolLv1>("Elite_WitchMother.tmx", "MotherOfThePoolLv1");
            RegisterEncounter<MotherOfThePoolLv2>("Elite_WitchMother.tmx", "MotherOfThePoolLv2");
            RegisterEncounter<MotherOfThePoolLv3>("Elite_WitchMother.tmx", "MotherOfThePoolLv3");
            RegisterEncounter<CroneOfTheWildsLv1>("Elite_WitchCrone.tmx", "CroneOfTheWildsLv1");
            RegisterEncounter<CroneOfTheWildsLv2>("Elite_WitchCrone.tmx", "CroneOfTheWildsLv2");
            RegisterEncounter<CroneOfTheWildsLv3>("Elite_WitchCrone.tmx", "CroneOfTheWildsLv3");
            RegisterEncounter<GrandStaircaseL1>("GrandStaircase.tmx", "GrandStaircaseL1");
            RegisterEncounter<GrandStaircaseL2>("GrandStaircase.tmx", "GrandStaircaseL2");
            RegisterEncounter<GrandStaircaseL3>("GrandStaircase.tmx", "GrandStaircaseL3");

            // Boss fights
            ModManager.RegisterEncounter<Boss_DriderFight>("Boss_DriderFight.tmx");
            ModManager.RegisterEncounter<Boss_WitchCoven>("Elite_WitchCoven.tmx");
            ModManager.RegisterEncounter<Boss_Handmaiden>("Boss_Handmaiden.tmx");
            ModManager.RegisterEncounter<Boss_FrozenTemple>("FrozenTemple.tmx");

            // Skill Challenges
            RegisterEncounter<Level1SkillChallenge>("SkillChallenge.tmx", "SkillChallengeLv1");
            RegisterEncounter<Level2SkillChallenge>("SkillChallenge.tmx", "SkillChallengeLv2");
            RegisterEncounter<Level3SkillChallenge>("SkillChallenge.tmx", "SkillChallengeLv3");

            // Other
            ModManager.RegisterEncounter<TestMap>("TestHall.tmx");
        }

        private static void RegisterEncounter<T>(string filename, string key) where T : Encounter
        {
            RegisteredEncounters.RegisteredEncounterInfo registeredEncounterInfo = new RegisteredEncounters.RegisteredEncounterInfo(() => (Encounter)Activator.CreateInstance(typeof(T), filename));
            RegisteredEncounters.RegisteredEncountersBySimpleFilename.Add(key, registeredEncounterInfo);
            RegisteredEncounters.RegisteredEncountersByType.Add(typeof(T), registeredEncounterInfo);
        }
    }
}
