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
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;
using Dawnsbury.Campaign.Encounters.Tutorial;
using HarmonyLib;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights;
using System.IO.Enumeration;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode
{

    // TODO: Add modular AI functions to making adding new enemies easier and lcean up creature list

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Loader {
        internal static string Credits { get; } =
            "{b}CREDITS for the Roguelike Mode Mod{/b}\n\n" +
            "{b}Design, direction and programming: {/b} Pixie1001\n" +
            "{b}Artists: {/b} Pixie1001\n" +
            "{b}Additional design: {/b} ...\n" +
            "{b}Additional programming: {/b} ...\n" +
            "{b}Playtesting: {/b} ...";

        internal static Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>> Creatures = new Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>>();

        [DawnsburyDaysModMainMethod]
        public static void LoadMod() {
            var harmony = new Harmony("Dawnsbury.Mods.GameModes.RoguelikeMode");
            Harmony.DEBUG = true;
            harmony.PatchAll();

            CustomItems.LoadItems();
            CreatureList.LoadCreatures();
            ScriptHooks.LoadHooks();
            SpellLoader.LoadSpells();
            LoadEncounters();
        }

        private static void LoadEncounters() {

            ModManager.RegisterEncounter<HallOfBeginnings>("HallOfBeginnings.tmx");
            RegisterEncounter<DrowAmbushLv1>("DrowAmbushLv2.tmx", "DrowAmbushLv1");
            RegisterEncounter<DrowAmbushLv2>("DrowAmbushLv2.tmx", "DrowAmbushLv2");
            RegisterEncounter<DrowAmbushLv3>("DrowAmbushLv2.tmx", "DrowAmbushLv3");
            RegisterEncounter<InquisitrixTrapLv1>("InquisitrixTrapLv2.tmx", "InquisitrixTrapLv1");
            RegisterEncounter<InquisitrixTrapLv2>("InquisitrixTrapLv2.tmx", "InquisitrixTrapLv2");
            RegisterEncounter<InquisitrixTrapLv3>("InquisitrixTrapLv2.tmx", "InquisitrixTrapLv3");
            RegisterEncounter<RatSwarmLv1>("RatSwarmLv2.tmx", "RatSwarmLv1");
            RegisterEncounter<RatSwarmLv2>("RatSwarmLv2.tmx", "RatSwarmLv2");
            RegisterEncounter<RatSwarmLv3>("RatSwarmLv2.tmx", "RatSwarmLv3");
            RegisterEncounter<SpiderNestLv1>("SpiderNest.tmx", "SpiderNestLv1");
            RegisterEncounter<SpiderNestLv2>("SpiderNest.tmx", "SpiderNestLv2");
            RegisterEncounter<SpiderNestLv3>("SpiderNest.tmx", "SpiderNestLv3");
            RegisterEncounter<AqueductsLv1>("Aqueducts.tmx", "AqueductsLv1");
            RegisterEncounter<AqueductsLv2>("Aqueducts.tmx", "AqueductsLv2");
            RegisterEncounter<AqueductsLv3>("Aqueducts.tmx", "AqueductsLv3");

            // Elite Fights
            RegisterEncounter<HallOfSmokeLv1>("Elite_HallOfSmokeLv2.tmx", "Elite_HallOfSmokeLv1");
            RegisterEncounter<HallOfSmokeLv2>("Elite_HallOfSmokeLv2.tmx", "Elite_HallOfSmokeLv2");
            RegisterEncounter<HallOfSmokeLv3>("Elite_HallOfSmokeLv2.tmx", "Elite_HallOfSmokeLv3");
            RegisterEncounter<WitchCovenLv1>("Elite_WitchCoven.tmx", "Elite_WitchCovenLv1");
            RegisterEncounter<WitchCovenLv2>("Elite_WitchCoven.tmx", "Elite_WitchCovenLv2");
            RegisterEncounter<WitchCovenLv3>("Elite_WitchCoven.tmx", "Elite_WitchCovenLv3");

            // Boss fights
            RegisterEncounter<Boss_DriderFight>("Boss_DriderFight.tmx", "Boss_DriderFight");

            // Other
            ModManager.RegisterEncounter<TestMap>("TestHall.tmx");
        }

        private static void RegisterEncounter<T>(string filename, string key) where T : Encounter {
            RegisteredEncounters.RegisteredEncounterInfo registeredEncounterInfo = new RegisteredEncounters.RegisteredEncounterInfo(() => (Encounter)Activator.CreateInstance(typeof(T), filename));
            RegisteredEncounters.RegisteredEncountersBySimpleFilename.Add(key, registeredEncounterInfo);
            RegisteredEncounters.RegisteredEncountersByType.Add(typeof(T), registeredEncounterInfo);
        }
    }
}
