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
using System.IO;
using System.Text.Json.Nodes;
using System.Reflection.Metadata;
using Microsoft.VisualBasic.FileIO;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class AiFuncs {

        // AI functions to implement
        // - Run bonus on postion 
        // - Cowardly

        internal static void PositionalGoodness(Creature monster, List<Option> options, Func<Tile, Creature, bool> filter, float modifier, bool flat = true) {

            float localMod = 0;

            foreach (Option option in options.Where(o => o.OptionKind == OptionKind.MoveHere)) {

                TileOption? option2 = option as TileOption;
                if (option2 != null) {
                    int hits = monster.Battle.AllCreatures.Where(cr => filter(option2.Tile, cr)).Count();
                    if (hits > 0) {
                        localMod = flat ? modifier : modifier * hits;
                        option2.AiUsefulness.MainActionUsefulness += localMod;
                    }
                }
            }

            foreach (Option option in options.Where(o => o.OptionKind != OptionKind.MoveHere && o.AiUsefulness.MainActionUsefulness != 0)) {
                int hits = monster.Battle.AllCreatures.Where(cr => filter(monster.Occupies, cr)).Count();
                if (hits > 0) {
                    localMod = flat ? modifier : modifier * hits;
                    option.AiUsefulness.MainActionUsefulness += localMod;
                }
            }
        }

        /// <summary>
        /// The MONSTER considers all OPTIONS, and considers its distance to creatured defined in CHECK_DISTANCE_TO compared to the average distance of creatured defined in COMPARE_TO.
        /// If, compared to the average distance of compareTo, an option leaves it closer, it gains ATTRACTION. If an option leaves it further away, it gains AVERSION.
        /// </summary>
        internal static void AverageDistanceGoodness(Creature monster, List<Option> options, Func<Creature, bool> checkDistanceTo, Func<Creature, bool> compareTo, float attraction, float aversion) {
            // Get current proximity score
            float currScore = 0f;
            foreach (Creature enemy in monster.Battle.AllCreatures.Where(cr => checkDistanceTo(cr))) {
                currScore += monster.DistanceTo(enemy);
            }

            // Find how close allies are to the party
            List<Creature> allies = monster.Battle.AllCreatures.Where(cr => compareTo(cr)).ToList();
            float allyScore = 0;
            foreach (Creature ally in allies) {
                foreach (Creature enemy in monster.Battle.AllCreatures.Where(cr => checkDistanceTo(cr))) {
                    allyScore += ally.DistanceTo(enemy);
                }
            }
            allyScore /= allies.Count;

            foreach (Option option in options.Where(o => o.OptionKind == OptionKind.MoveHere && o is TileOption)) {
                TileOption option2 = option as TileOption;
                float personalScore = 0f;
                foreach (Creature enemy in monster.Battle.AllCreatures.Where(cr => checkDistanceTo(cr))) {
                    personalScore += option2.Tile.DistanceTo(enemy.Occupies);
                }

                if (option2.Text == "Stride" && monster.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(monster.OwningFaction) && cr.Threatens(monster.Occupies)).ToArray().Length == 0) {
                    // Closer than compareTo allies
                    if (personalScore > allyScore && currScore < allyScore) {
                        option2.AiUsefulness.MainActionUsefulness += aversion;
                    } else if (personalScore < allyScore) {
                        option2.AiUsefulness.MainActionUsefulness += attraction;
                    }
                } else if (option2.Text == "Step") {
                    // Closer than compareTo allies
                    if (personalScore > allyScore && currScore < allyScore) {
                        option2.AiUsefulness.MainActionUsefulness += aversion;
                    } else if (personalScore < allyScore) {
                        option2.AiUsefulness.MainActionUsefulness += attraction;
                    }
                }
            }
        }




    }
}
