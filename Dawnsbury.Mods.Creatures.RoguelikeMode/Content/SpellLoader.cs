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
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using System.IO;
using static HarmonyLib.Code;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class SpellLoader {

        //public static List<ItemName> items = new List<ItemName>();

        //public static SpellId BrinyBolt = ModManager.RegisterNewSpell("RL_BrinyBolt", 1, (id, spellcaster, spellLevel, inCombat, spellInformation) => {
        //    return Spells.CreateModern(Illustrations.BrinyBolt, "Briny Bolt", new Trait[] { Trait.Attack, Trait.Evocation, Trait.Water, Trait.Primal, Trait.Arcane, ModTraits.Roguelike }, "You hurl a bolt of saltwater from your extended hand.",
        //        "Make a ranged spell attack against a target within range." +
        //        S.FourDegreesOfSuccess("The creature takes " + S.HeightenedVariable(spellLevel * 2 + 2, 4) + "d6 bludgeoning damage and is blinded for 1 round and dazzled for 1 minute as saltwater sprays into its eyes. The creature can spend an Interact action to rub its eyes and end the blinded condition, but not the dazzled condition.",
        //        "The creature takes " + S.HeightenedVariable(spellLevel * 2, 2) + "d6 bludgeoning damage and is blinded for 1 round. The creature can spend an Interact action wiping the salt water from its eyes to end the blinded condition.", null, null), Target.Ranged(12), spellLevel, null)
        //    .WithHeighteningNumerical(spellLevel, 1, inCombat, 1, "The damage increases by 2d6.")
        //    .WithActionCost(2)
        //    .WithProjectileCone(Illustrations.BrinyBolt, 7, ProjectileKind.Cone)
        //    .WithSoundEffect(SfxName.ElementalBlastWater)
        //    .WithSpellAttackRoll()
        //    .WithGoodnessAgainstEnemy((targeting, a, d) => {
        //        float score = 3.5f * targeting.OwnerAction.SpellLevel;
        //        if (!d.HasEffect(QEffectId.Blinded)) {
        //            score += 1.5f * d.Level;
        //        }
        //        if (!d.HasEffect(QEffectId.Dazzled)) {
        //            score += 0.5f * d.Level;
        //        }
        //        return score;
        //    })
        //    .WithEffectOnEachTarget(async (spell, caster, d, result) => {
        //        if (result >= CheckResult.Success) {
        //            string dmg = (result == CheckResult.CriticalSuccess ? (spellLevel * 2 + 2).ToString() : spellLevel * 2) + "d6";
        //            await CommonSpellEffects.DealDirectDamage(spell, DiceFormula.FromText(dmg, "Briny Bolt"), d, result, DamageKind.Bludgeoning);

        //            QEffect quenchableBlindness = QEffect.Blinded().WithExpirationAtStartOfSourcesTurn(caster, 1);
        //            quenchableBlindness.ProvideContextualAction = (Func<QEffect, Possibility>)(qfBlindness => new ActionPossibility(new CombatAction(qfBlindness.Owner, (Illustration)IllustrationName.RubEyes, "Rub eyes", new Trait[1] { Trait.Manipulate },
        //            "End the blinded condition affecting you because of {i}briny bolt{/i}.", (Target)Target.Self((Func<Creature, AI, float>)((cr, ai) => ai.AlwaysIfSmartAndTakingCareOfSelf))).WithActionCost(1).WithEffectOnSelf((Action<Creature>)(rubber => quenchableBlindness.ExpiresAt = ExpirationCondition.Immediately))).WithPossibilityGroup("Remove debuff"));
        //            d.AddQEffect(quenchableBlindness);
        //        }

        //        if (result == CheckResult.CriticalSuccess) {
        //            d.AddQEffect(QEffect.Dazzled().WithExpirationAtStartOfSourcesTurn(caster, 10));
        //        }
        //    })
        //    ;
        //});

        public static SpellId AgonisingDespair = ModManager.RegisterNewSpell("RL_AgonisingDespair", 3, (id, caster, level, inCombat, info) => {
            return Spells.CreateModern(Illustrations.AgonizingDespair, "Agonising Despair", new Trait[] {
                        Trait.Emotion,
                        Trait.Enchantment,
                        Trait.Fear,
                        Trait.Mental,
                        Trait.Arcane,
                        Trait.Divine,
                        Trait.Occult
                    }, "Your target's mind tumbles down a deep well of dread, dwelling so intently on deep-seated fears that it's painful.",
                    $"The target takes {S.HeightenedVariable((level - 1) * 2, 4)}d6 mental damage with a Will saving throw." +
                    S.FourDegreesOfSuccess("The target is unaffected.",
                    "The target takes half damage and becomes frightened 1.",
                    "The target takes full damage and becomes frightened 2.",
                    "The target takes double damage and becomes frightened 3."),
            Target.Ranged(12), level, SpellSavingThrow.Standard(Defense.Will))
            .WithSoundEffect(SfxName.Fear)
            .WithHeighteningOfDamageEveryLevel(level, 3, inCombat, "2d6")
            .WithGoodnessAgainstEnemy((t, a, d) => a.AI.Fear(d) + (level - 1) * 7)
            .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                if (result == CheckResult.CriticalSuccess) return;
                await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, $"{(level - 1) * 2}d6", DamageKind.Mental);
                target.AddQEffect(QEffect.Frightened(3 - (int)result));
            });
        });

        public static SpellId LesserDominate = ModManager.RegisterNewSpell("Lesser Dominate", 5, (id, caster, level, inCombat, info) => {
            return Spells.CreateModern((Illustration)IllustrationName.Dominate, "Lesser Dominate", new Trait[] {
                        Trait.Enchantment,
                        Trait.Incapacitation,
                        Trait.Mental,
                        Trait.AssumesDirectControl
                    }, "You take command of the target, forcing it to obey your orders.", "The target makes a Will save." + S.FourDegreesOfSuccess("The target is unaffected.", "The target is stunned 1.", "You gain control of the target until the end of their next turn.", "As failure, but you maintain control for 2 turns."),
            (Target)Target.Ranged(6)
            .WithAdditionalConditionOnTargetCreature((Func<Creature, Creature, Usability>)((a, d) => !d.HasTrait(Trait.Minion) && !d.Traits.Any(tr => tr.HumanizeLowerCase2() == "eidolon") ? Usability.Usable : Usability.NotUsableOnThisCreature("minion"))), 5, SpellSavingThrow.Standard(Defense.Will))
            .WithSoundEffect(SfxName.Mental)
            .WithGoodnessAgainstEnemy((Func<Target, Creature, Creature, float>)((t, a, d) => (float)d.HP))
            .WithEffectOnEachTarget((Delegates.EffectOnEachTarget)(async (spell, caster, target, result) => {
                if (result == CheckResult.Success)
                    target.AddQEffect(QEffect.Stunned(1));
                if (result > CheckResult.Failure)
                    return;
                Faction originalFaction = target.OwningFaction;
                target.OwningFaction = caster.OwningFaction;
                target.AddQEffect(new QEffect("Controlled", "You're controlled by " + caster?.ToString() + ".", result == CheckResult.CriticalFailure ? ExpirationCondition.CountsDownAtEndOfYourTurn : ExpirationCondition.ExpiresAtEndOfYourTurn, caster, (Illustration)IllustrationName.Dominate) {
                    Value = result == CheckResult.CriticalFailure ? 2 : 0,
                    StateCheck = (Action<QEffect>)(qf => {
                        if (caster.Alive)
                            return;
                        qf.Owner.Occupies.Overhead("end of control", Color.Lime, caster?.ToString() + " died and so can no longer dominate " + target?.ToString() + ".");
                        if (qf.Owner.OwningFaction != caster.OwningFaction)
                            return;
                        qf.Owner.OwningFaction = originalFaction;
                        qf.ExpiresAt = ExpirationCondition.Immediately;
                    }),
                    WhenExpires = self => {
                        self.Owner.Occupies.Overhead("end of control", Color.Lime, target?.ToString() + " shook off the domination.");
                        if (self.Owner.OwningFaction != caster.OwningFaction)
                            return;
                        self.Owner.OwningFaction = originalFaction;
                    }
                });
            }));
        });

        public static SpellId WakingNightmare = ModManager.RegisterNewSpell("WakingNightmare", 1, (id, caster, level, inCombat, info) => {
            return Spells.CreateModern((Illustration)IllustrationName.Fear, "Waking Nightmare", new Trait[] {
                        Trait.Cleric,
                        Trait.Emotion,
                        Trait.Enchantment,
                        Trait.Fear,
                        Trait.Mental,
                        Trait.Focus,
                        ModTraits.Roguelike
                    }, "You fill the creature's mind with a terrifying vision out of its nightmares.",
                    "The target makes a Will save." +
                    S.FourDegreesOfSuccess("The target is unaffected.", "The target is frightened 1.", "The target is frightened 2.", "The target is frightened 3."),
            (Target)Target.Ranged(6), level, SpellSavingThrow.Standard(Defense.Will))
            .WithSoundEffect(SfxName.Fear)
            .WithProjectileCone(IllustrationName.Fear, 15, ProjectileKind.Cone)
            .WithGoodnessAgainstEnemy((t, a, d) => a.AI.Fear(d))
            .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                if (result < CheckResult.CriticalSuccess)
                    target.AddQEffect(QEffect.Frightened(3 - (int)result));
            });
        });

        public static SpellId SharedNightmare = ModManager.RegisterNewSpell("SharedNightmare", 4, (id, caster, level, inCombat, info) => {
            return Spells.CreateModern((Illustration)IllustrationName.BestowCurse, "Shared Nightmare", new Trait[] {
                        Trait.Cleric,
                        Trait.Emotion,
                        Trait.Enchantment,
                        Trait.Incapacitation,
                        Trait.Mental,
                        Trait.Focus,
                        ModTraits.Roguelike
                    }, "Merging minds with the target, you swap disorienting visions from one another's nightmares.",
                    "One of you will become confused, but which it'll be depends on the target's Will save." +
                    S.FourDegreesOfSuccess("At the start of your next turn, you spend your first action with the confused condition, then act normally.", "The nightmare is unravelled, leaving you both unaffected.",
                    "As critial success, but the target is affected instead of you, spending its first action each turn confused. The duration is 1 minute.", "The target is confused for 1 minute."),
            (Target)Target.Ranged(6), level, SpellSavingThrow.Standard(Defense.Will))
            .WithSoundEffect(SfxName.Fear)
            .WithProjectileCone(IllustrationName.BestowCurse, 15, ProjectileKind.Cone)
            .WithGoodnessAgainstEnemy((t, a, d) => a.AI.Fear(d))
            .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                switch (result) {
                    case CheckResult.CriticalSuccess:
                        //caster.AddQEffect(QEffect.Confused(false, spell).WithExpirationAtStartOfOwnerTurn());
                        //break;
                        caster.AddQEffect(new QEffect("Shared Nightmare", "You are confused for your first action of each turn.", ExpirationCondition.ExpiresAtEndOfYourTurn, caster, IllustrationName.BestowCurse) {
                            CannotExpireThisTurn = true,
                            StartOfYourPrimaryTurn = async (self, you) => {
                                //self.Tag = you.Battle.RoundNumber;
                                int roundNum = -1;
                                if (self.Tag != null && self.Tag is int) {
                                    roundNum = (int)self.Tag;
                                }
                                if (roundNum < you.Battle.RoundNumber) {
                                    self.Tag = you.Battle.RoundNumber;
                                    var slowed = QEffect.Slowed(you.Actions.ActionsLeft - 1).WithExpirationNever();
                                    var confused = QEffect.Confused(false, spell).WithExpirationNever();
                                    you.Actions.UsedQuickenedAction = true;
                                    you.AddQEffect(slowed);
                                    you.AddQEffect(confused);
                                    await you.Battle.GameLoop.OneTurn(you);
                                    you.RemoveAllQEffects(qf => qf == slowed || qf == confused);
                                    you.Actions.ResetToFull();
                                    you.Actions.UseUpActions(1, ActionDisplayStyle.UsedUp, CombatAction.DefaultCombatAction);
                                }
                            }
                        });
                        break;
                    case CheckResult.Success:
                        break;
                    case CheckResult.Failure:
                        target.AddQEffect(new QEffect("Shared Nightmare", "You are confused for your first action of each turn.", ExpirationCondition.Never, caster, IllustrationName.BestowCurse) {
                            StartOfYourPrimaryTurn = async (self, you) => {
                                int roundNum = -1;
                                if (self.Tag != null && self.Tag is int) {
                                    roundNum = (int)self.Tag;
                                }
                                if (roundNum < you.Battle.RoundNumber) {
                                    self.Tag = you.Battle.RoundNumber;
                                    var slowed = QEffect.Slowed(you.Actions.ActionsLeft - 1).WithExpirationNever();
                                    var confused = QEffect.Confused(false, spell).WithExpirationNever();
                                    you.Actions.UsedQuickenedAction = true;
                                    you.AddQEffect(slowed);
                                    you.AddQEffect(confused);
                                    await you.Battle.GameLoop.OneTurn(you);
                                    you.RemoveAllQEffects(qf => qf == slowed || qf == confused);
                                    you.Actions.ResetToFull();
                                    you.Actions.UseUpActions(1, ActionDisplayStyle.UsedUp, CombatAction.DefaultCombatAction);
                                }
                            }
                        });
                        break;
                    case CheckResult.CriticalFailure:
                        caster.AddQEffect(QEffect.Confused(false, spell).WithExpirationNever());
                        break;
                }
            });
        });

        internal static void LoadSpells() {

            var spells = new List<SpellId>() { AgonisingDespair, LesserDominate, WakingNightmare, SharedNightmare };

            ModManager.RegisterActionOnEachSpell(spell => {
                if (spell.SpellId == SpellId.BoneSpray) {
                    spell.WithGoodnessAgainstEnemy((t, a, d) => 5.5f * spell.SpellLevel + (d.WeaknessAndResistance.Immunities.Contains(DamageKind.Bleed) ? 0f : spell.SpellLevel * 2 - 2));
                }

                if (spell.SpellId == SpellId.PummelingRubble) {
                    spell.WithGoodnessAgainstEnemy((t, a, d) => 5f * spell.SpellLevel);
                }

                if (spell.SpellId == SpellId.TrueStrike) {
                    // Gain goodness based on strike possiblities and their goodness?
                    spell.Target = Target.Self((cr, ai) => {
                        bool hasRangedAttack = false;
                        Creature nearestEnemy = cr.Battle.AllCreatures.MinBy(enemy => enemy.EnemyOf(cr) ? (float)enemy.DistanceTo(cr) : 1000f);
                        cr.RegeneratePossibilities();
                        foreach (PossibilitySection section in cr.Possibilities.Sections) {
                            if (section.Possibilities.Any(pos => pos is ActionPossibility
                            && (pos as ActionPossibility).CombatAction.HasTrait(Trait.Attack)
                            && ((pos as ActionPossibility)?.CombatAction.Target as CreatureTarget)?.RangeKind == RangeKind.Ranged
                            && ((pos as ActionPossibility)?.CombatAction.Target as CreatureTarget).CreatureTargetingRequirements.Any(r => r is MaximumRangeCreatureTargetingRequirement && cr.DistanceTo(nearestEnemy) <= (r as MaximumRangeCreatureTargetingRequirement).Range))) {
                                hasRangedAttack = true;
                            }
                        }

                        if (!hasRangedAttack && !cr.Battle.AllCreatures.Any(enemy => enemy.EnemyOf(cr) && enemy.IsAdjacentTo(cr))) {
                            return int.MinValue;
                        }

                        if (cr.Actions.AttackedThisManyTimesThisTurn > 0 || cr.FindQEffect(QEffectId.TrueStrike) != null) {
                            return int.MinValue;
                        } else if (cr.Actions.ActionsLeft <= 1) {
                            return int.MinValue;
                        }
                        return 5f;
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

                //if (spell.SpellId == SpellId.KineticRam) {
                //    // Good for close enemies, or enemies flanking allies
                //    spell.WithGoodnessAgainstEnemy((t, a, d) => {
                //        float score = 0.5f;
                //        foreach (Creature ally in a.Battle.AllCreatures.Where(cr => cr.OwningFaction == a.OwningFaction)) {
                //            if (d.PrimaryWeapon != null && ally.IsFlatfootedToBecause(d, d.CreateStrike(d.PrimaryWeapon)) == "flanking") {
                //                score += 3;
                //            }
                //            if (d.DistanceTo(a) < 3) {
                //                score += 2;
                //            }
                //        }
                //        return score;
                //    });
                //}

                if (spell.SpellId == SpellId.NeutralizePoison) {
                    spell.WithGoodness((t, a, d) => d.QEffects.Any(qf => qf.RepresentsPoison) ? 15f : int.MinValue);
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

                //if (spell.SpellId == SpellId.BrinyBolt) {
                //    spell.WithGoodnessAgainstEnemy((t, a, d) => {
                //        float score = 3.5f * t.OwnerAction.SpellLevel;
                //        if (!d.HasEffect(QEffectId.Blinded)) {
                //            score += 1.5f * d.Level;
                //        }
                //        if (!d.HasEffect(QEffectId.Dazzled)) {
                //            score += 0.5f * d.Level;
                //        }
                //        return score;
                //    });
                //}

                if (spell.SpellId == SpellId.RayOfEnfeeblement) {
                    spell.WithGoodnessAgainstEnemy((t, a, d) => {
                        float score = 0;

                        if (d.FindQEffect(QEffectId.Enfeebled)?.Value >= 2) {
                            return 0f;
                        }

                        if (d.Abilities.Get(Ability.Strength) > 0) {
                            score += 1;
                        }

                        if (d.Abilities.Get(Ability.Strength) > d.Abilities.Get(Ability.Dexterity)) {
                            score += a.AI.Fear(d);
                        }

                        if (d.FindQEffect(QEffectId.Enfeebled)?.Value == 1) {
                            score /= 2;
                        }
                        return score;
                    });
                }

                if (spell.SpellId == SpellId.Soothe) {
                    spell.WithGoodness((t, a, d) => {
                        float score = spell.SpellLevel * 9.5f + (d.QEffects.Any(qf => qf.Name == "Soothe") ? 0 : 1);
                        if (d.Damage >= d.MaxHP * 0.75) {
                            score *= 1.5f;
                        }
                        return score;
                    });
                }

                if (spell.SpellId == SpellId.Guidance) {
                    spell.WithGoodness((t, a, d) => {
                        int range = d.Speed;

                        if (a == d && a.Actions.ActionsLeft < 3)
                            return AIConstants.NEVER;

                        if (!d.Battle.AllCreatures.Any(cr => !cr.FriendOf(d) && cr.DistanceTo(d) <= range))
                            return AIConstants.NEVER;

                        return 2;
                    });
                }

                if (spell.SpellId == SpellId.FireShield) {
                    spell.WithGoodness((t, a, d) => {
                        return 13f;
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