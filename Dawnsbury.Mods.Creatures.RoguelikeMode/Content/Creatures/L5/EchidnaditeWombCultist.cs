﻿using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using System.Threading;
using System;
using static System.Collections.Specialized.BitVector32;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Campaign.Path;
using Microsoft.Xna.Framework;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.Intelligence;
using System.Collections.Generic;
using Dawnsbury.Core.Mechanics.Rules;
using System.Reflection.Metadata;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class EchidnaditeWombCultist {
        public static Creature Create() {
            Creature monster = new Creature(Illustrations.EWombCultist, "Echidnadite Womb Cultist", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Human, Trait.Humanoid, ModTraits.MeleeMutator }, 5, 12, 5, new Defenses(19, 15, 9, 12), 98,
                new Abilities(3, 2, 3, -1, 3, 1), new Skills(nature: 10))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (pos, you, step, cr) => you.DistanceTo(cr) <= 2 && cr.FriendOf(you) && !cr.HasTrait(Trait.Celestial) && (cr.HasTrait(Trait.Animal) || cr.HasTrait(Trait.Beast)), 0.5f, false);

                    return null;
                };
            })
            .WithCreatureId(CreatureIds.EchidnaditeWombCultist)
            .WithProficiency(Trait.Unarmed, Proficiency.Master)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Jaws, "2d8", DamageKind.Slashing, []))
            .AddQEffect(CommonQEffects.MothersProtection())
            .AddQEffect(CommonQEffects.BlessedOfEchidna())
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {

                    StrikeModifiers strikeModifiers = new StrikeModifiers() {
                        OnEachTarget = async (a, d, result) => {
                            if (result >= CheckResult.Success && !a.HasEffect(QEffectIds.Exhausted)) {
                                // Flesh for the Womb
                                if (!a.QEffects.Any(qf => qf.Name == "Flesh for the Womb")) {
                                    a.AddQEffect(new QEffect("Flesh for the Womb", $"When this condition reaches 4, the {a.BaseName} will give birth to a terrible beast.", ExpirationCondition.Never, a, IllustrationName.GluttonsJaw) {
                                        Value = result == CheckResult.Success ? 1 : 2,
                                    });
                                } else {
                                    int stacks = a.QEffects.First(qf => qf.Name == "Flesh for the Womb").Value + (result == CheckResult.Success ? 1 : 2);
                                    a.QEffects.First(qf => qf.Name == "Flesh for the Womb").Value = stacks;
                                    if (stacks >= 4) {
                                        Tile pos = a.Occupies;
                                        a.AddQEffect(new QEffect("Exhausted", "You're exhausted from giving birth, losing half your Max HP and taking a -2 status penalty to all checks and DCs. You cannot give birth again this combat.",
                                            ExpirationCondition.Never, a, IllustrationName.RayOfEnfeeblement) {
                                            StateCheck = self => self.Owner.DrainedMaxHPDecrease = self.Owner.MaxHP / 2,
                                            BonusToAllChecksAndDCs = (qf) => new Bonus(-2, BonusType.Status, "exhausted"),
                                            Id = QEffectIds.Exhausted
                                        });
                                        a.RemoveAllQEffects(qf => qf.Name == "Flesh for the Womb");
                                        var list = MonsterStatBlocks.MonsterExemplars.Where(pet => (pet.HasTrait(Trait.Animal) || pet.HasTrait(Trait.Beast)) && CommonEncounterFuncs.Between(pet.Level, a.Level, a.Level + 3) && !pet.HasTrait(Trait.Celestial) && !pet.HasTrait(Trait.NonSummonable)).ToArray();
                                        int rand = R.Next(0, list.Count());

                                        if (list.Count() == 0)
                                            return;

                                        Creature newForm = MonsterStatBlocks.MonsterFactories[list[rand].Name](self.Owner.Battle.Encounter, self.Owner.Occupies);

                                        if (newForm.Level - a.Level >= 4) {
                                            newForm.ApplyWeakAdjustments(false, true);
                                        } else if (newForm.Level - a.Level == 3) {
                                            newForm.ApplyWeakAdjustments(false);
                                        } else if (newForm.Level - a.Level == 1) {
                                            newForm.ApplyEliteAdjustments();
                                        } else if (newForm.Level - a.Level == 0) {
                                            newForm.ApplyEliteAdjustments(true);
                                        }
                                        Sfxs.Play(SfxName.AcidSplash);
                                        a.Battle.SpawnCreature(newForm, a.OwningFaction, pos);
                                    }
                                }
                            }
                        }
                    };

                    CombatAction? action = self.Owner.CreateStrike(self.Owner.UnarmedStrike, -1, strikeModifiers);
                    action.WithPrologueEffectOnChosenTargetsBeforeRolls(async (action, user, targets) => {
                        action.Tag = targets.ChosenCreature?.HP;
                    });

                    if (action == null)
                        return null;

                    action.Name = "Bloody Feast";
                    //action.Traits.Add(Trait.Flourish);
                    action.Description = StrikeRules.CreateBasicStrikeDescription2(action.StrikeModifiers, additionalSuccessText: "If you dealt damage, gain a stack of Flesh for the Womb (or 2 on a critical hit) and heal for an amount equal to the damage dealt.");
                    action.ShortDescription += "; If you dealt damage, gain a stack of Flesh for the Womb (or 2 on a critical hit) and heal for an amount equal to the damage dealt.";
                    action.Illustration = new SideBySideIllustration(action.Illustration, IllustrationName.BloodVendetta);
                    action.WithGoodnessAgainstEnemy((target, attacker, defender) => {
                        return AIConstants.ALWAYS;
                    });

                    return (ActionPossibility)action;
                },
                AfterYouDealDamage = async (a, action, d) => {
                    if (action.Name == "Bloody Feast") {
                        if (action.Tag == null || action.Tag is not int)
                            return;
                        await a.HealAsync($"{(int)action.Tag - d.HP}", action);
                    }
                }
            })
            ;
            return monster;
        }
    }
}

