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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Medusa {
        public static Creature Create() {
            var creature = new Creature(Illustrations.Medusa,
                "Medusa",
                [Trait.Chaotic, Trait.Evil, Trait.Humanoid, ModTraits.Monstrous, ModTraits.ArcherMutator],
                7, 11, 4,
                new Defenses(25, 15, 16, 14),
                105,
                new Abilities(2, 5, 4, 2, 1, 2),
                new Skills(deception: 16, diplomacy: 14, stealth: 16))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (t, _, _, cr) => t.DistanceTo(cr.Occupies) <= 6, 1.5f, false);

                    return null;
                };
            })
            .WithCreatureId(CreatureIds.Medusa)
            .WithAdditionalUnarmedStrike(new Item(IllustrationName.Fang, "snake fangs", [Trait.Agile, Trait.Finesse, Trait.Unarmed, Trait.AddsInjuryPoison, Trait.Brawling]).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Piercing)).WithMonsterWeaponSpecialization(6))
            .AddHeldItem(Items.CreateNew(ItemName.CompositeShortbow).WithModificationPlusOne().WithMonsterWeaponSpecialization(6))
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Unarmed, Proficiency.Expert)
            .WithProficiency(Trait.Weapon, Proficiency.Master)
            .AddQEffect(QEffect.AllAroundVision())
            .AddQEffect(CommonQEffects.SerpentVenomAttack(18, null))
            .Builder
            .AddMainAction(you => new CombatAction(you, IllustrationName.Dominate, "Focus Gaze", [Trait.Arcane, Trait.Concentrate, Trait.Incapacitation, Trait.Transmutation, Trait.Visual, Trait.InflictsSlow],
            "The medusa fixes their glare at a creature they can see within 30 feet. The target must immediately attempt a Fortitude save against the medusa's petrifying gaze. If the creature was already slowed" +
            " by petrifying gaze before attempting its save, a failed save causes it to be permanantly petrified. A PC petrified in this way will result in an immediate game loss. " +
            "After attempting its save, the creature is then temporarily immune until the start of the medusa's next turn.",
            Target.Ranged(6).WithAdditionalConditionOnTargetCreature((a, d) => d.QEffects.Any(qf => qf.Key == "Medusa Gaze Immunity") ? Usability.NotUsableOnThisCreature("Already attempted to petrify this round") : Usability.Usable))
            .WithActionCost(1)
            .WithSavingThrow(new SavingThrow(Defense.Fortitude, 15 + you.Level))
            .WithSoundEffect(SfxName.Stoneskin)
            .WithProjectileCone(IllustrationName.Dominate, 5, ProjectileKind.Ray)
            .WithGoodness((_, a, d) => {
                var score = 100f;

                if (a.Actions.AttackedThisManyTimesThisTurn == 0)
                    return 2f;

                if (d.QEffects.Any(qf => qf.Name == "Avert Gaze"))
                    score -= 5;

                if (d.QEffects.Any(qf => qf.Key == "Partial Petrification"))
                    score += 10;

                return score;
            })
            .WithEffectOnEachTarget(async (action, caster, defender, result) => {
                defender.AddQEffect(new QEffect() { Key = "Medusa Gaze Immunity" }.WithExpirationAtStartOfSourcesTurn(caster, 1));

                if (result > CheckResult.Failure)
                    return;

                if (!defender.QEffects.Any(qf => qf.Key == "Partial Petrification")) {
                    defender.AddQEffect(new QEffect("Partial Petrification", "You're slowed 1 until the end of the encounter. If partially petrified again, you become fully petrified. A medusa's gaze is permanent, and suffering such a fate immediately causes the party to lose the encounter.") {
                        Innate = false,
                        Illustration = caster.Illustration,
                        Key = "Partial Petrification",
                        StateCheck = self => self.Owner.AddQEffect(QEffect.Slowed(1).WithExpirationEphemeral())
                    });
                } else {
                    Basilisk.Petrify(defender);
                    if (defender.PersistentCharacterSheet != null)
                        defender.Die();
                }
            }))
            .Done()
            ;

            var bitingSnakes = new QEffect("Biting Snakes {icon:Reaction}", "{b}Trigger{/b} A creature ends its turn adjacent to the medusa. {b}Effect{/b} The medusa makes a snake fangs Strike against the creature.");
            bitingSnakes.AddGrantingOfTechnical(cr => cr.EnemyOf(bitingSnakes.Owner) && cr.IsAdjacentTo(bitingSnakes.Owner), qfBitingSnakes => {
                qfBitingSnakes.EndOfYourTurnBeneficialEffect = async (self, you) => {
                    var ca = creature.CreateStrike("snake fangs").WithActionCost(0);
                    if (ca.CanBeginToUse(creature) && (ca.Target as CreatureTarget)!.IsLegalTarget(creature, you) && await bitingSnakes.Owner.AskToUseReaction($"Make a snake fangs attack against {you.Name}?")) {
                        ca.ChosenTargets = ChosenTargets.CreateSingleTarget(you);
                        await ca.AllExecute();
                    }
                };
            });
            creature.AddQEffect(bitingSnakes);

            var glance = new QEffect("Petrifying Gaze", "") {
                StartOfCombat = async self => {
                    self.Description = $"(arcane, aura, transmutation, visual) 30 feet. When a creature ends its turn in the aura it must attempt a DC {(15 + self.Owner.Level)} Fortitude save. If it fails, it’s slow 1 for 1 minute as its body slowly stiffens.";
                }
            };
            glance.AddGrantingOfTechnical(cr => !cr.HasEffect(QEffectId.Blinded) && cr.CreatureId != CreatureIds.PetrifiedGuardian, qfTech => {
                qfTech.StartOfYourEveryTurn = async (self, you) => {
                    if (you.IsImmuneTo(Trait.Visual) || you.QEffects.Any(qf => qf.Key == "Partial Petrification") || glance.Owner.DistanceTo(you) > 6 || glance.Owner.HasLineOfEffectTo(you.Occupies) > CoverKind.Greater)
                        return;

                    var ca = new CombatAction(glance.Owner, IllustrationName.Dominate, "Petrifying Glance", [Trait.Arcane, Trait.Aura, Trait.Transmutation, Trait.Visual, Trait.InflictsSlow], "", Target.Ranged(6))
                    .WithActionCost(0)
                    .WithSavingThrow(new SavingThrow(Defense.Fortitude, glance.Owner.Level + 18))
                    .WithSoundEffect(SfxName.Stoneskin)
                    .WithProjectileCone(IllustrationName.Dominate, 5, ProjectileKind.Ray)
                    .WithEffectOnEachTarget(async (action, caster, defender, result) => {
                        if (result <= CheckResult.Failure) {
                            if (!defender.Actions.ActionsUsed.Contains(ActionDisplayStyle.Slowed))
                                defender.Actions.UseUpActions(1, ActionDisplayStyle.Slowed, action);

                            defender.AddQEffect(new QEffect("Partial Petrification", "You're slowed 1 until the end of the encounter. If partially petrified again, you become fully petrified. A medusa's gaze is permanent, and suffering such a fate immediately causes the party to lose the encounter.") {
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
                };
            });

            Basilisk.AddGazeProtection(glance);

            creature.AnimationData.AddAuraAnimation(new MagicCircleAuraAnimation(Illustrations.BaneCircleWhite, Color.DarkGreen, 6) { MaximumOpacity = 0.6f });

            creature.AddQEffect(glance);

            return creature;
        }
    }
}
