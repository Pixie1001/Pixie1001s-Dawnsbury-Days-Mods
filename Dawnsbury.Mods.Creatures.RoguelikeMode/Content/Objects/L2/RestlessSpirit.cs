using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class RestlessSpirit {
        public static Creature Create() {
            int radius = 1;
            QEffect qfCurrentDC = new QEffect() { Value = 14 };

            Creature hazard = new Creature(Illustrations.RestlessSpirit, "Restless Spirit", new List<Trait>() { Trait.Object, ModTraits.Haunt }, 2, 0, 0, new Defenses(10, 10, 0, 0), 20, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
            .WithSpawnAsGaia()
            .WithTactics(Tactic.DoNothing)
            .WithEntersInitiativeOrder(false)
            .AddQEffect(qfCurrentDC)
            .AddQEffect(QEffect.ObjectImmunities())
            //.AddQEffect(CommonQEffects.Hazard())
            .WithHardness(1000)
            ;

            var animation = hazard.AnimationData.AddAuraAnimation(IllustrationName.BaneCircle, radius);
            animation.Color = Color.GhostWhite;

            QEffect interactable = new QEffect("Interactable", "You can use Diplomacy, Occultism and Religion to interact with this spirit.") {
                StateCheckWithVisibleChanges = async self => {
                    // Add contextual actions
                    foreach (Creature hero in self.Owner.Battle.AllCreatures.Where(cr => !cr.OwningFaction.IsGaiaPure && cr.IsAdjacentTo(self.Owner))) {
                        hero.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                            ProvideContextualAction = qfContextActions => {
                                return new SubmenuPossibility(Illustrations.RestlessSpirit, "Interactions") {
                                    Subsections = {
                                        new PossibilitySection(hazard.Name) {
                                            Possibilities = {
                                                (ActionPossibility)new CombatAction(qfContextActions.Owner, IllustrationName.Consecrate, "Release Spirits", new Trait[] { Trait.Manipulate, Trait.Basic },
                                                "Make a Religion check against DC " + (qfCurrentDC.Value + 2) + "." + S.FourDegreesOfSuccess(null, "Release the spirit, destroying the hazard and gaining a +1 status bonus to attack, saving throws and strike damage for the rest of the encounter.",
                                                null, "You take 1d6 negative damage."),
                                                Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                .WithActionCost(1)
                                                .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Religion), Checks.FlatDC(qfCurrentDC.Value + 2)))
                                                .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                    if (result == CheckResult.CriticalFailure) {
                                                        Sfxs.Play(SfxName.Necromancy, 0.7f);
                                                        await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Restless Spirits"), caster, CheckResult.Success, DamageKind.Negative);
                                                    }
                                                    if (result >= CheckResult.Success) {
                                                        Sfxs.Play(SfxName.HolyWard);
                                                        caster.AddQEffect(new QEffect("Spirit Boon", "The spirits of the dead aid in gratitude for helping them find peace. You gain a +1 status bonus to all attack, save and strike damage rolls until the end of the encounter.", ExpirationCondition.Never, null, IllustrationName.Consecrate) {
                                                            BonusToDefenses = (qf, action, defence) => defence != Defense.AC ? new Bonus(1, BonusType.Status, "Inspired") : null,
                                                            BonusToAttackRolls = (_, _, _) => new Bonus(1, BonusType.Status, "Inspired"),
                                                            BonusToDamage = (qf, action, target) => action.HasTrait(Trait.Strike) ? new Bonus(1, BonusType.Status, "Inspired") : null,
                                                        });
                                                        caster.Battle.RemoveCreatureFromGame(self.Owner);
                                                    }
                                                }),
                                                (ActionPossibility)new CombatAction(qfContextActions.Owner, Illustrations.RestlessSpirit, "Command Spirits", new Trait[] { Trait.Manipulate, Trait.Auditory, Trait.Basic, Trait.Incapacitation, ModTraits.Haunt },
                                                "{b}Range{/b} 20 feet\n{b}Target{/b} 1 living creature\n{b}Saving throw{/b} Will\n\nYou direct the spirit to haunt an enemy, using your Occultism DC." + S.FourDegreesOfSuccess(
                                                "You take 1d6 negative damage and become frightened 2.",
                                                "The hazard is destroyed and the target gains frightened 1.", "The hazard is destroyed and the target gains frightened 2.",
                                                "The hazard is destroyed and the target gains the controlled condition until the end of their next turn."),
                                                Target.Ranged(4).WithAdditionalConditionOnTargetCreature(new LivingCreatureTargetingRequirement()))
                                                .WithSpellInformation((qfContextActions.Owner.Level + 1) / 2, "", null)
                                                .WithActionCost(2)
                                                .WithSavingThrow(new SavingThrow(Defense.Will, caster => caster != null ? caster.Skills.Get(Skill.Occultism) + 10 : 10))
                                                .WithSoundEffect(SfxName.MajorNegative)
                                                .WithProjectileCone(Illustrations.RestlessSpirit, 10, ProjectileKind.Cone)
                                                .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                    if (result == CheckResult.CriticalSuccess) {
                                                        Sfxs.Play(SfxName.Necromancy, 0.7f);
                                                        await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Restless Spirits"), caster, CheckResult.Success, DamageKind.Negative);
                                                        caster.AddQEffect(QEffect.Frightened(2));
                                                    } else if (result == CheckResult.Success) {
                                                        target.AddQEffect(QEffect.Frightened(1));
                                                        caster.Battle.RemoveCreatureFromGame(self.Owner);
                                                    } else if (result == CheckResult.Failure) {
                                                        target.AddQEffect(QEffect.Frightened(2));
                                                        caster.Battle.RemoveCreatureFromGame(self.Owner);
                                                    } else if (result == CheckResult.CriticalFailure) {
                                                        Faction originalFaction = target.OwningFaction;
                                                        target.OwningFaction = caster.OwningFaction;
                                                        target.AddQEffect(new QEffect("Controlled", "You're controlled by " + caster.ToString() + ".", ExpirationCondition.ExpiresAtEndOfYourTurn, caster, IllustrationName.Dominate) {
                                                            CountsAsADebuff = true,
                                                            Value = 1,
                                                            Id = QEffectId.Slowed,
                                                            WhenExpires = qf => {
                                                                qf.Owner.OwningFaction = originalFaction;
                                                            },
                                                            StateCheck = qf => {
                                                                if (caster.Alive)
                                                                    return;
                                                                qf.Owner.Overhead("end of control", Color.Lime, caster.ToString() + " died and so can no longer dominate " + target?.ToString() + ".");
                                                                if (qf.Owner.OwningFaction != caster.OwningFaction)
                                                                    return;
                                                                qf.Owner.OwningFaction = originalFaction;
                                                                qf.ExpiresAt = ExpirationCondition.Immediately;
                                                            }
                                                        });
                                                        caster.Battle.RemoveCreatureFromGame(self.Owner);
                                                    }
                                                }),
                                                (ActionPossibility)new CombatAction(qfContextActions.Owner, Illustrations.RestlessSpirit, "Soothe Spirits", new Trait[] { Trait.Manipulate, Trait.Auditory, Trait.Basic },
                                                "Make a Diplomacy check against DC " + qfCurrentDC.Value + "." + S.FourDegreesOfSuccess("Reduce all DCs on this hazard by 3.",
                                                "Reduce all DCs on this hazard by 2.", null, "You take 1d6 negative damage and increase all DCs on this hazard by 1."),
                                                Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                .WithActionCost(1)
                                                .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Diplomacy), Checks.FlatDC(qfCurrentDC.Value - 2)))
                                                .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                    if (result == CheckResult.CriticalFailure) {
                                                        Sfxs.Play(SfxName.Necromancy, 0.7f);
                                                        await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Restless Spirits"), caster, CheckResult.Success, DamageKind.Negative);
                                                        qfCurrentDC.Value += 1;
                                                    }
                                                    if (result == CheckResult.Success) {
                                                        qfCurrentDC.Value -= 2;
                                                    }
                                                    if (result == CheckResult.CriticalSuccess) {
                                                        qfCurrentDC.Value -= 3;
                                                    }
                                                }),
                                            }
                                        }
                                    }
                                };
                            }
                        });
                    }
                }
            };
            QEffect aura = new QEffect("Unnerving Presence",
                "This spirit died a horrible and tragic death, inflicting unnerving flashbacks of its untimely demise on neary creatures. Creatures within the aura suffer -1 status penalty to all checks and DCs.") {
                StartOfCombat = async self => {
                    foreach (Creature enemy in self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsEnemy && cr.EntersInitiativeOrder)) {
                        Func<AI, List<Option>, Option?> newBehaviour = (ai, options) => {
                            Creature monster = ai.Self;

                            AiFuncs.PositionalGoodness(monster, options, (pos, thisMonster, step, otherCreature) => otherCreature == self.Owner && pos.IsAdjacentTo(otherCreature.Occupies), -2);

                            //if (monster.Skills.Get(Skill.Occultism) > 6 && options.Max(o => o.AiUsefulness.MainActionUsefulness) < 15 && monster.Actions.ActionsLeft == 3) {
                            //    AiFuncs.PositionalGoodness(monster, options, (pos, thisMonster, step, otherCreature) => otherCreature == self.Owner && pos.IsAdjacentTo(otherCreature.Occupies), 3);
                            //} else {

                            //}

                            return null;
                        };
                        if (enemy.AI.OverrideDecision != null) {
                            enemy.AI.OverrideDecision = newBehaviour + enemy.AI.OverrideDecision;
                        } else {
                            enemy.AI.OverrideDecision = newBehaviour;
                        }
                    }
                }
            };
            interactable.AddGrantingOfTechnical(cr => (cr.OwningFaction.IsEnemy || cr.OwningFaction.IsGaiaFriends) && !cr.HasTrait(Trait.Mindless) && cr.Abilities.Intelligence > -3, qfTechnical => {
                qfTechnical.ProvideActionIntoPossibilitySection = (self, section) => {
                    if (section.PossibilitySectionId != PossibilitySectionId.InvisibleActions || self.Owner.Skills.Get(Skill.Occultism) <= 6) {
                        return null;
                    }
                    ActionPossibility ap = (ActionPossibility)new CombatAction(self.Owner, Illustrations.RestlessSpirit, "Command Spirits", new Trait[] { Trait.Manipulate, Trait.Auditory, Trait.Basic, Trait.Incapacitation, ModTraits.Haunt },
                    "{b}Range{/b} 20 feet\n{b}Target{/b} 1 living creature\n{b}Saving throw{/b} Will\n\nYou direct the spirit to haunt an enemy, using your Occultism DC." + S.FourDegreesOfSuccess("You take 1d6 negative damage and become frightened 2.",
                    "The hazard is destroyed and the target gains frightened 1.", "The hazard is destroyed and the target gains frightened 2.",
                    "The hazard is destroyed and the target gains the controlled condition until the end of their next turn."),
                    Target.Ranged(4).WithAdditionalConditionOnTargetCreature(new LivingCreatureTargetingRequirement()).WithAdditionalConditionOnTargetCreature((a, d) => a.IsAdjacentTo(hazard) ? Usability.Usable : Usability.NotUsable("Must be adjacent to the restless soul to command it.")))
                    .WithSpellInformation((self.Owner.Level + 1) / 2, "", null)
                    .WithActionCost(2)
                    .WithSavingThrow(new SavingThrow(Defense.Will, caster => caster != null ? caster.Skills.Get(Skill.Occultism) + 10 : 10))
                    .WithSoundEffect(SfxName.MajorNegative)
                    .WithProjectileCone(Illustrations.RestlessSpirit, 10, ProjectileKind.Cone)
                    .WithGoodnessAgainstEnemy((t, a, d) => {
                        return (a.Skills.Get(Skill.Occultism) > 6 && (a.Level + 1) / 2 >= (d.Level + 1) / 2 ? a.Level * 7 : int.MinValue);
                    })
                    .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                        if (result == CheckResult.CriticalSuccess) {
                            Sfxs.Play(SfxName.Necromancy, 0.7f);
                            await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Restless Spirits"), caster, CheckResult.Success, DamageKind.Negative);
                            caster.AddQEffect(QEffect.Frightened(2));
                        } else if (result == CheckResult.Success) {
                            target.AddQEffect(QEffect.Frightened(1));
                            caster.Battle.RemoveCreatureFromGame(interactable.Owner);
                        } else if (result == CheckResult.Failure) {
                            target.AddQEffect(QEffect.Frightened(2));
                            caster.Battle.RemoveCreatureFromGame(interactable.Owner);
                        } else if (result == CheckResult.CriticalFailure) {
                            Faction originalFaction = target.OwningFaction;
                            target.OwningFaction = caster.OwningFaction;
                            target.AddQEffect(new QEffect("Controlled", "You're controlled by " + caster.ToString() + ".", ExpirationCondition.ExpiresAtEndOfYourTurn, caster, IllustrationName.Dominate) {
                                CountsAsADebuff = true,
                                Value = 1,
                                Id = QEffectId.Slowed,
                                WhenExpires = qf => {
                                    qf.Owner.OwningFaction = originalFaction;
                                },
                                StateCheck = qf => {
                                    if (caster.Alive)
                                        return;
                                    qf.Owner.Overhead("end of control", Color.Lime, caster.ToString() + " died and so can no longer dominate " + target?.ToString() + ".");
                                    if (qf.Owner.OwningFaction != caster.OwningFaction)
                                        return;
                                    qf.Owner.OwningFaction = originalFaction;
                                    qf.ExpiresAt = ExpirationCondition.Immediately;
                                }
                            });
                            caster.Battle.RemoveCreatureFromGame(interactable.Owner);
                        }
                    });
                    return ap;
                };
            });

            aura.AddGrantingOfTechnical(cr => cr.IsLivingCreature && !cr.HasTrait(Trait.Mindless), qfTechnical => {
                qfTechnical.StateCheck = self => {
                    if (self.Owner.DistanceTo(aura.Owner) <= radius) {
                        self.Owner.AddQEffect(new QEffect("Unnerved", $"You suffer a -1 status penalty to all checks and DCs while within {aura.Owner.Name}'s aura.",
                            ExpirationCondition.Ephemeral, aura.Owner, new SameSizeDualIllustration(Illustrations.StatusBackdrop, Illustrations.RestlessSpirit)) {
                            Key = "Ghost Zone Debuff",
                            BonusToAllChecksAndDCs = qf => new Bonus(-1, BonusType.Status, "unnerving presence"),
                            CountsAsADebuff = true
                        });
                    }
                };
            });

            hazard.AddQEffect(interactable);
            hazard.AddQEffect(aura);

            return hazard;
        }
    }
}