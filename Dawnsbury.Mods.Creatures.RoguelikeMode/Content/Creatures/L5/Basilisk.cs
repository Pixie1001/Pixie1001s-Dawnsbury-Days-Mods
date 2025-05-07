using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Microsoft.Xna.Framework;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Audio;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.Intelligence;
using System.Text;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.AuraAnimations;
using System;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using System.Reflection.Metadata.Ecma335;
using Dawnsbury.Display;
using System.Numerics;
using System.Reflection.Metadata;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Basilisk {
        public static Creature Create() {
            var creature = new Creature(Illustrations.Basilisk,
                "Basilisk",
                [Trait.Beast, ModTraits.MeleeMutator],
                5, 11, 4,
                new Defenses(22, 14, 8, 11),
                75,
                new Abilities(4, -1, 5, -3, 2, 1),
                new Skills(athletics: 13, stealth: 8))
            .WithCreatureId(CreatureIds.Basilisk)
            .WithUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Jaws, "2d8", DamageKind.Piercing))
            .WithCharacteristics(false, true)
            .WithProficiency(Trait.Unarmed, Proficiency.Master)
            .Builder
            .AddMainAction(you => new CombatAction(you, IllustrationName.Dominate, "Petrifying Gaze", [Trait.Arcane, Trait.Concentrate, Trait.Incapacitation, Trait.Transmutation, Trait.Visual, Trait.InflictsSlow],
            $"The basilisk stares at a creature it can see within 30 feet. That creature must attempt a DC {you.Level + 17} Fortitude save. If it fails and has not already been slowed by Petrifying Glance or this ability," +
            " it becomes slowed 1. If the creature was already slowed by this ability or Petrifying Glance, a failed save causes the creature to be petrified until the end of the encounter.", Target.Ranged(6))
            .WithActionCost(2)
            .WithSavingThrow(new SavingThrow(Defense.Fortitude, 15 + you.Level))
            .WithSoundEffect(SfxName.Stoneskin)
            .WithProjectileCone(IllustrationName.Dominate, 5, ProjectileKind.Ray)
            .WithGoodness((_, a, d) => {
                var score = 100f;

                if (d.QEffects.Any(qf => qf.Name == "Avert Gaze"))
                    score -= 5;

                if (d.QEffects.Any(qf => qf.Key == "Partial Petrification"))
                    score += 10;

                return score;
            })
            .WithEffectOnEachTarget(async (action, caster, defender, result) => {
                if (result > CheckResult.Failure)
                    return;

                if (!defender.QEffects.Any(qf => qf.Key == "Partial Petrification")) {
                    defender.AddQEffect(new QEffect("Partial Petrification", "You're slowed 1 until the end of the encounter. If partially petrified again, you become fully petrified.") {
                        Innate = false,
                        Illustration = caster.Illustration,
                        Key = "Partial Petrification",
                        StateCheck = self => self.Owner.AddQEffect(QEffect.Slowed(1).WithExpirationEphemeral())
                    });
                } else {
                    Petrify(defender);
                }
            }))
            .Done()
            ;

            var glance = new QEffect("Petrifying Glance {icon:Reaction}", "") {
                StartOfCombat = async self => {
                    self.Description = $"(arcane, aura, transmutation, visual); {{b}}Trigger{{/b}} A creature within 30 feet that the basilisk can see starts its turn. {{b}}Effect{{/b}} The target must attempt a DC {(15 + self.Owner.Level)} Fortitude save. If it fails, it’s slow 1 for 1 minute as its body slowly stiffens.";
                }
            };
            glance.AddGrantingOfTechnical(cr => cr.EnemyOf(glance.Owner) && !cr.HasEffect(QEffectId.Blinded), qfTech => {
                qfTech.StartOfYourEveryTurn = async (self, you) => {
                    if (you.IsImmuneTo(Trait.Visual) || you.QEffects.Any(qf => qf.Key == "Partial Petrification") || glance.Owner.DistanceTo(you) > 6 || glance.Owner.HasLineOfEffectTo(you.Occupies) > CoverKind.Greater)
                        return;

                    if (await glance.Owner.AskToUseReaction($"Use Petrifying Glance to force {you.Name} to make a DC {(15 + glance.Owner.Level)} Fort save to avoid becoming slowed?")) {
                        var ca = new CombatAction(glance.Owner, IllustrationName.Dominate, "Petrifying Glance", [Trait.Arcane, Trait.Aura, Trait.Transmutation, Trait.Visual, Trait.InflictsSlow], "", Target.Ranged(6))
                        .WithActionCost(Constants.ACTION_COST_REACTION)
                        .WithSavingThrow(new SavingThrow(Defense.Fortitude, glance.Owner.Level + 15))
                        .WithSoundEffect(SfxName.Stoneskin)
                        .WithProjectileCone(IllustrationName.Dominate, 5, ProjectileKind.Ray)
                        .WithEffectOnEachTarget(async (action, caster, defender, result) => {
                            if (result <= CheckResult.Failure) {
                                if (!defender.QEffects.Any(qf => qf.Id == QEffectId.Slowed))
                                    defender.Actions.UseUpActions(1, ActionDisplayStyle.Slowed, action);

                                defender.AddQEffect(new QEffect("Partial Petrification", "You're slowed 1 until the end of the encounter. If partially petrified again, you become fully petrified.") {
                                    Innate = false,
                                    Illustration = glance.Owner.Illustration,
                                    Key = "Partial Petrification",
                                    StateCheck = self => self.Owner.AddQEffect(QEffect.Slowed(1).WithExpirationEphemeral())
                                });
                            }
                        })
                        ;
                        ca.ChosenTargets = ChosenTargets.CreateSingleTarget(you);
                        await ca.AllExecute();
                    }
                };
            });

            AddGazeProtection(glance);

            creature.AddQEffect(glance);

            return creature;
        }


        public static void Petrify(Creature target) {
            target.RemoveAllQEffects(qf => qf.Key == "Partial Petrification");
            target.Traits.Add(Trait.Object);
            target.AddQEffect(new QEffect("Petrified", "You've been turned to stone. You can't act, and you become an object with AC 9 and Hardness 8") {
                Id = QEffectId.Unconscious,
                Innate = false,
                Illustration = IllustrationName.BrokenPillar,
                ProvidesArmor = new Item(IllustrationName.BrokenPillar, "Petrified", []).WithArmorProperties(new ArmorProperties(9, 0, 0, 10, 10)),
                PreventTargetingBy = action => action.HasTrait(Trait.Healing) ? "petrified" : null
            });
            target.WithHardness(8);
        }

        public static void AddGazeProtection(QEffect effect) {
            effect.AddGrantingOfTechnical(cr => cr.EnemyOf(effect.Owner), qfTech => {
                qfTech.Key = "Avert Gaze Context";
                qfTech.ProvideContextualAction = self => {
                    var menu = new SubmenuPossibility(Illustrations.AvertGaze, "Visual Protection");
                    var section = new PossibilitySection("Visual Protection");
                    menu.Subsections.Add(section);

                    section.AddPossibility((ActionPossibility)new CombatAction(self.Owner, Illustrations.AvertGaze, "Avert Gaze", [Trait.Basic], "You avert your gaze from danger. You gain a +2 circumstance bonus to saves against visual abilities until the end of your next turn.",
                        Target.Self().WithAdditionalRestriction(user => user.QEffects.Any(qf => qf.Id == QEffectId.Blinded || ((string)qf.Tag == "CoverEyes")) ? "gaze already protected" : null))
                    .WithActionCost(1)
                    .WithSoundEffect(SfxName.StowItem)
                    .WithEffectOnSelf(caster => caster.AddQEffect(new QEffect("Avert Gaze", "You gain a +2 circumstance bonus to saves against visual abilities.", ExpirationCondition.ExpiresAtEndOfYourTurn, caster, Illustrations.AvertGaze) {
                        CannotExpireThisTurn = true,
                        BonusToDefenses = (self, action, defence) => action != null && action.HasTrait(Trait.Visual) && defence != Defense.AC ? new Bonus(2, BonusType.Circumstance, "Avert Gaze", true) : null,
                    })));

                    section.AddPossibility((ActionPossibility)new CombatAction(self.Owner, Illustrations.CoverEyes, "Cover Eyes", [Trait.Basic], "You squeeze your eyes shut, blinding yourself, but protecting you visual abilities until the end of your next turn.", Target.Self())
                    .WithActionCost(0)
                    .WithSoundEffect(SfxName.StowItem)
                    .WithEffectOnSelf(caster => {
                        var blindness = QEffect.Blinded().WithExpirationAtEndOfSourcesNextTurn(caster, true);
                        blindness.Tag = "CoverEyes";
                        caster.AddQEffect(blindness);
                    })
                    );

                    return menu;
                };
            });
        }
    }
}
