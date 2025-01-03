﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
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
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModTraits;
using Dawnsbury.Campaign.Encounters.A_Crisis_in_Dawnsbury;
using System.Buffers;
using System.Xml.Schema;
using Microsoft.Xna.Framework.Input;
using FMOD;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L6;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class CreatureList
    {
        internal static Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>> Creatures = new Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>>();
        internal static Dictionary<ObjectId, Func<Encounter?, Creature>> Objects = new Dictionary<ObjectId, Func<Encounter?, Creature>>();


        /// <summary>
        /// Gets the Creature object based off the id
        /// </summary>
        internal static Creature GetCreature<TEnum>(TEnum id) where TEnum : Enum
        {
            switch (id)
            {
                case ModEnums.CreatureId.UNSEEN_GUARDIAN:
                    return UnseenGuardian.Create();
                case ModEnums.CreatureId.DROW_ASSASSIN:
                    return DrowAssassin.Create();
                case ModEnums.CreatureId.DROW_FIGHTER:
                    return DrowFighter.Create();
                case ModEnums.CreatureId.DROW_SHOOTIST:
                    return DrowShootist.Create();
                case ModEnums.CreatureId.DROW_SNIPER:
                    return DrowSniper.Create();
                case ModEnums.CreatureId.DROW_PRIESTESS:
                    return DrowPriestess.Create();
                case ModEnums.CreatureId.DROW_TEMPLEGUARD:
                    return DrowTempleGuard.Create();
                case ModEnums.CreatureId.HUNTING_SPIDER:
                    return HuntingSpider.Create();
                case ModEnums.CreatureId.DRIDER:
                    return Drider.Create();
                case ModEnums.CreatureId.DROW_ARCANIST:
                    return DrowArcanist.Create();
                case ModEnums.CreatureId.DROW_SHADOWCASTER:
                    return DrowShadowcaster.Create();
                case ModEnums.CreatureId.DROW_INQUISITRIX:
                    return DrowInquisitrix.Create();
                case ModEnums.CreatureId.TREASURE_DEMON:
                    return TreasureDemon.Create();
                case ModEnums.CreatureId.DROW_RENEGADE:
                    return DrowRenegade.Create();
                case ModEnums.CreatureId.WITCH_CRONE:
                    return WitchCrone.Create();
                case ModEnums.CreatureId.WITCH_MOTHER:
                    return WitchMother.Create();
                case ModEnums.CreatureId.WITCH_MAIDEN:
                    return WitchMaiden.Create();
                case ModEnums.CreatureId.RAVENOUS_RAT:
                    return RavenousRat.Create();
                case ModEnums.CreatureId.WITCH_CULTIST:
                    return DevotedCultist.Create();
                case ModEnums.CreatureId.NUGLUB:
                    return Nuglub.Create();
                case ModEnums.CreatureId.HOMUNCULUS:
                    return Homunculus.Create();
                case ModEnums.CreatureId.ABYSSAL_HANDMAIDEN:
                    return AbyssalHandmaiden.Create();
                default:
                    throw new NotSupportedException($"The creature id '{id}' is not supported");
            }
        }

        /// <summary>
        /// Gets the Object object based off the id
        /// </summary>
        internal static Creature GetObject<TEnum>(TEnum id) where TEnum : Enum
        {
            switch (id)
            {
                case ModEnums.ObjectId.CHOKING_MUSHROOM:
                    return ChokingMushroom.Create();
                case ModEnums.ObjectId.BOOM_SHROOM:
                    return ExplosiveMushroom.Create();
                case ModEnums.ObjectId.ICE_FONT:
                    return IceFont.Create();
                case ModEnums.ObjectId.SPIDER_QUEEN_SHRINE:
                    return SpiderQueenShrin.Create();
                case ModEnums.ObjectId.RESTLESS_SPIRIT:
                    return RestlessSpirit.Create();
                case ModEnums.ObjectId.TEST_PILE:
                    return TestPile.Create();
                default:
                    throw new NotSupportedException($"The object id '{id}' is not supported");
            }
        }

        /// <summary>
        /// Registers the creatures through the ModManager then adds the creature to the running creature list
        /// </summary>
        internal static void RegisterAndAddCreatureToDictonary<TEnum>(Dictionary<TEnum, Func<Encounter?, Creature>> creatures, TEnum id, string? overridenCreatureName = null) where TEnum : Enum
        {
            Func<Encounter?, Creature> creatureFunction = encounter => (typeof(TEnum) == typeof(ModEnums.CreatureId)) ? GetCreature(id) : GetObject(id);
            ModManager.RegisterNewCreature(overridenCreatureName ?? creatureFunction(null).Name, creatureFunction);
            creatures.Add(id, creatureFunction);
        }

        internal static void LoadCreatures()
        {
            // TODO: Setup to teleport to random spot and be hidden at start of combat, so logic can be removed from encounter.

            // Level -1 Creatures
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.RAVENOUS_RAT);

            // Level 0 Creatures
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.HOMUNCULUS);

            // Level 1 Creatures - Drow
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.DROW_ASSASSIN);
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.DROW_FIGHTER);
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.DROW_SHOOTIST);
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.DROW_SNIPER);
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.DROW_ARCANIST);
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.DROW_RENEGADE);
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.WITCH_CULTIST);

            // Level 1 Creatures - Other
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.HUNTING_SPIDER);

            // Level 2 Creatures - Drow
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.DROW_TEMPLEGUARD);
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.DROW_INQUISITRIX);

            // Level 2 Creatures - Other
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.UNSEEN_GUARDIAN);
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.TREASURE_DEMON);
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.NUGLUB);

            // Level 3 Creatures - Drow
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.DROW_PRIESTESS);
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.DROW_SHADOWCASTER);

            // Level 3 Creatures - Other
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.DRIDER);
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.WITCH_CRONE, "Witch Crone");
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.WITCH_MOTHER, "Witch Mother");
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.WITCH_MAIDEN, "Witch Maiden");

            // Level 6 Creatures - Demons
            RegisterAndAddCreatureToDictonary(Creatures, ModEnums.CreatureId.ABYSSAL_HANDMAIDEN);
        }

        internal static void LoadObjects()
        {
            // Level 1 Hazards
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.ICE_FONT, "Scaling Font of Ice");

            // Level 2 Hazards
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.CHOKING_MUSHROOM);
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.BOOM_SHROOM);
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.SPIDER_QUEEN_SHRINE);
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.RESTLESS_SPIRIT);
            RegisterAndAddCreatureToDictonary(Objects, ModEnums.ObjectId.TEST_PILE, "TestPile");
        }

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        internal static class CommonMonsterActions
        {
            public static CombatAction CreateHide(Creature self)
            {
                return new CombatAction(self, (Illustration)IllustrationName.Hide, "Hide", new Trait[2] { Trait.Basic, Trait.AttackDoesNotTargetAC },
                    "Make one Stealth check against the Perception DCs of each enemy creature that can see you but that you have cover or concealment from. On a success, you become Hidden to that creature.",
                    Target.Self((cr, ai) => ai.HideSelf()).WithAdditionalRestriction(innerSelf => {
                        if (HiddenRules.IsHiddenFromAllEnemies(innerSelf, innerSelf.Occupies))
                            return "You're already hidden from all enemies.";
                        return !innerSelf.Battle.AllCreatures.Any(cr => cr.EnemyOf(innerSelf) && HiddenRules.HasCoverOrConcealment(innerSelf, cr)) ? "You don't have cover or concealment from any enemy." : null;
                    }))
                .WithActionId(ActionId.Hide)
                .WithSoundEffect(SfxName.Hide)
                .WithEffectOnSelf(innerSelf => {
                    int roll = R.NextD20();
                    foreach (Creature creature in innerSelf.Battle.AllCreatures.Where(cr => cr.EnemyOf(innerSelf)))
                    {
                        if (!innerSelf.DetectionStatus.HiddenTo.Contains(creature) && HiddenRules.HasCoverOrConcealment(innerSelf, creature))
                        {
                            CheckBreakdown breakdown = CombatActionExecution.BreakdownAttack(new CombatAction(innerSelf, (Illustration)IllustrationName.Hide, "Hide", new Trait[1]
                            {
                    Trait.Basic
                            }, "[this condition has no description]", Target.Self()).WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Stealth), Checks.DefenseDC(Defense.Perception))), creature);
                            CheckBreakdownResult breakdownResult = new CheckBreakdownResult(breakdown, roll);
                            string str8 = breakdown.DescribeWithFinalRollTotal(breakdownResult);
                            DefaultInterpolatedStringHandler interpolatedStringHandler;
                            if (breakdownResult.CheckResult >= CheckResult.Success)
                            {
                                innerSelf.DetectionStatus.HiddenTo.Add(creature);
                                Tile occupies = creature.Occupies;
                                Color lightBlue = Color.LightBlue;
                                string str9 = innerSelf?.ToString();
                                string str10 = creature?.ToString();
                                interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 3);
                                interpolatedStringHandler.AppendLiteral(" (");
                                interpolatedStringHandler.AppendFormatted(breakdownResult.D20Roll.ToString() + breakdown.TotalCheckBonus.WithPlus());
                                interpolatedStringHandler.AppendLiteral("=");
                                interpolatedStringHandler.AppendFormatted(breakdownResult.D20Roll + breakdown.TotalCheckBonus);
                                interpolatedStringHandler.AppendLiteral(" vs. ");
                                interpolatedStringHandler.AppendFormatted(breakdown.TotalDC);
                                interpolatedStringHandler.AppendLiteral(").");
                                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                                string log = str9 + " successfully hid from " + str10 + stringAndClear;
                                string logDetails = str8;
                                occupies.Overhead("hidden from", lightBlue, log, "Hide", logDetails);
                            }
                            else
                            {
                                Tile occupies = creature.Occupies;
                                Color red = Color.Red;
                                string str11 = innerSelf?.ToString();
                                string str12 = creature?.ToString();
                                interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 3);
                                interpolatedStringHandler.AppendLiteral(" (");
                                interpolatedStringHandler.AppendFormatted(breakdownResult.D20Roll.ToString() + breakdown.TotalCheckBonus.WithPlus());
                                interpolatedStringHandler.AppendLiteral("=");
                                interpolatedStringHandler.AppendFormatted(breakdownResult.D20Roll + breakdown.TotalCheckBonus);
                                interpolatedStringHandler.AppendLiteral(" vs. ");
                                interpolatedStringHandler.AppendFormatted(breakdown.TotalDC);
                                interpolatedStringHandler.AppendLiteral(").");
                                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                                string log = str11 + " failed to hide from " + str12 + stringAndClear;
                                string logDetails = str8;
                                occupies.Overhead("hide failed", red, log, "Hide", logDetails);
                            }
                        }
                    }
                });
            }

            // Insert new actions here
        }
    }
}
