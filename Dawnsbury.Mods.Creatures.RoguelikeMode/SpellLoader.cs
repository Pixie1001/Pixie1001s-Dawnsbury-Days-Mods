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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class SpellLoader {

        //public static List<ItemName> items = new List<ItemName>();

        internal static void LoadSpells() {
            ModManager.RegisterActionOnEachSpell(spell => {
                if (spell.SpellId == SpellId.BoneSpray) {
                    spell.WithGoodnessAgainstEnemy((t, a, d) => 5.5f * spell.SpellLevel + (d.WeaknessAndResistance.Immunities.Contains(DamageKind.Bleed) ? 0f : spell.SpellLevel * 2 - 2));
                }

                if (spell.SpellId == SpellId.PummelingRubble) {
                    spell.WithGoodnessAgainstEnemy((t, a, d) => 5f * spell.SpellLevel);
                }

                if (spell.SpellId == SpellId.TrueStrike) {
                    // Gain goodness based on strike possiblities and their goodness?
                    spell.WithGoodness((t, a, d) => {
                        if (a.Actions.AttackedThisManyTimesThisTurn > 0) {
                            return int.MinValue;
                        } else if (a.Actions.ActionsLeft <= 1) {
                            return int.MinValue;
                        }
                        return 10f;
                    });
                    spell.WithEffectOnEachTarget(async (action, a, d, checkResult) => {
                        d.AddQEffect(new QEffect() {
                            AdditionalGoodness = (self, action, target) => {
                                if (action != null && action.HasTrait(Trait.Attack)) {
                                    return 10f;
                                }
                                return -5f;
                            },
                            AfterYouMakeAttackRoll = (qfSelf, result) => qfSelf.ExpiresAt = ExpirationCondition.Immediately
                        });
                    });
                }

                if (spell.SpellId == SpellId.KineticRam) {
                    // Good for close enemies, or enemies flanking allies
                    spell.WithGoodnessAgainstEnemy((t, a, d) => {
                        float score = 0.5f;
                        foreach (Creature ally in a.Battle.AllCreatures.Where(cr => cr.OwningFaction == a.OwningFaction)) {
                            if (d.PrimaryWeapon != null && ally.IsFlatfootedToBecause(d, d.CreateStrike(d.PrimaryWeapon)) == "flanking") {
                                score += 3;
                            }
                            if (d.DistanceTo(a) < 3) {
                                score += 2;
                            }
                        }
                        return score;
                    });
                }

                if (spell.SpellId == SpellId.FlourishingFlora) {
                    spell.WithGoodnessAgainstEnemy((t, a, d) => 5f * spell.SpellLevel);
                    spell.Variants.ForEach(v => v.GoodnessModifier = (ai, utility) => {
                        if (v.Id == "CACTI") {
                            return utility;
                        } else if (v.Id == "FLOWERS") {
                            return utility + 3;
                        } else if (v.Id == "FRUITS") {
                            return utility;
                        } else if (v.Id == "ROOTS") {
                            return utility;
                        }
                        return utility;
                    });
                }

                if (spell.SpellId == SpellId.HideousLaughter) {
                    spell.WithGoodnessAgainstEnemy((t, a, d) => {
                        float score = d.QEffects.Any(qf => qf.Id == QEffectId.Slowed) ? 0 : 7 * a.Level;
                        string[] reactions = new string[] { "attack of opportunity", "reactive strike", "stand still", "hunted prey", 
                            "glimpse of redemption", "retributive strike", "liberating step", "implement's interruption", "ring bell",
                            "amulet's abeyance", "weapon", "amulet", "bell" };
                        if (d.QEffects.Any(qf => reactions.Any(str => qf.Name.ToLower().StartsWith(str)))) {
                            score += 3 * a.Level;
                        }
                        return score;
                    });
                }

            });

        //    if (spell.SpellId == SpellId.FlourishingFlora) {
        //        spell.WithGoodnessAgainstEnemy((t, a, d) => 5f * spell.SpellLevel);
        //        spell.Variants.ForEach(v => v.GoodnessModifier = (ai, utility) => {

        //        }));
        //});
        }
    }
}

// .WithGoodnessAgainstEnemy((t, a, d) => d.IsLivingCreature ? (4.0 * t.OwnerAction.SpellLevel * 2.0) : 0.0f))