using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Tiles;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class AiFuncs {

        // AI functions to implement
        // - Run bonus on postion 
        // - Cowardly

        /// <summary>
        /// The MONSTER considers all OPTIONS, and determines which creatures in the encounter fit the conditions of FILTER(postion, self, is a step?, other creature).
        /// They then gain a goodness bonus equal their MODIFIER if filter returns true for a given postion. If FLAT is false, they gain this bonus for each creature that meets the conditions set by filter.
        /// </summary>
        internal static void PositionalGoodness(Creature monster, List<Option> options, Func<Tile, Creature, bool, Creature, bool> filter, float modifier, bool flat = true) {

            float localMod = 0;

            foreach (Option option in options.Where(o => o.OptionKind == OptionKind.MoveHere)) {

                TileOption? option2 = option as TileOption;
                if (option2 != null) {
                    int hits = monster.Battle.AllCreatures.Where(cr => filter(option2.Tile, monster, option2.Text == "Step", cr)).Count();
                    if (hits > 0) {
                        localMod = flat ? modifier : modifier * hits;
                        option2.AiUsefulness.MainActionUsefulness += localMod;
                    }
                }
            }

            foreach (Option option in options.Where(o => o.OptionKind != OptionKind.MoveHere && o.AiUsefulness.MainActionUsefulness != 0)) {
                int hits = monster.Battle.AllCreatures.Where(cr => filter(monster.Occupies, monster, false, cr)).Count();
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
                    personalScore += option2!.Tile.DistanceTo(enemy.Occupies);
                }

                if (option2!.Text == "Stride" && monster.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(monster.OwningFaction) && cr.Threatens(monster.Occupies)).ToArray().Length == 0) {
                    // Closer than compareTo allies
                    if (personalScore > allyScore && currScore < allyScore) {
                        option2.AiUsefulness.MainActionUsefulness += aversion;
                    } else if (personalScore < allyScore && currScore > allyScore) {
                        option2.AiUsefulness.MainActionUsefulness += attraction;
                    }
                } else if (option2.Text == "Step") {
                    // Closer than compareTo allies
                    if (personalScore > allyScore && currScore < allyScore) {
                        option2.AiUsefulness.MainActionUsefulness += aversion;
                    } else if (personalScore < allyScore && currScore > allyScore) {
                        option2.AiUsefulness.MainActionUsefulness += attraction;
                    }
                }
            }
        }




    }
}
