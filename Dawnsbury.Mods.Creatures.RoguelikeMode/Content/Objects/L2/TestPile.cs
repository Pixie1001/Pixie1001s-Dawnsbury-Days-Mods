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
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2 {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class TestPile {
        public static Creature Create() {
            int radius = 2;
            QEffect qfCurrentDC = new QEffect() { Value = 17 };

            Creature hazard = new Creature(Illustrations.RestlessSpirit, "Test Pile", new List<Trait>() { Trait.Object }, 2, 0, 0, new Defenses(10, 10, 0, 0), 20, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
            .WithTactics(Tactic.DoNothing)
            .WithEntersInitiativeOrder(false)
            .AddQEffect(qfCurrentDC)
            .WithHardness(7)
            ;

            QEffect interactable = new QEffect("Interactable", "This is a debug object that can be used to inflict status effects on yourself for testing purposes ^^") {
                StateCheckWithVisibleChanges = async self => {
                    // Add contextual actions
                    foreach (Creature hero in self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsHumanControlled && cr.IsAdjacentTo(self.Owner))) {
                        hero.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                            ProvideContextualAction = qfContextActions => {
                                return new SubmenuPossibility(Illustrations.RestlessSpirit, "Interactions") {
                                    Subsections = {
                                                new PossibilitySection(hazard.Name) {
                                                    Possibilities = {
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, IllustrationName.Slowed, "Slowed 1", new Trait[] {},
                                                        "",
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(0)
                                                        .WithEffectOnSelf(caster => {
                                                            caster.AddQEffect(QEffect.Slowed(1));
                                                        }),
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, IllustrationName.Slowed, "Slowed 2", new Trait[] {},
                                                        "",
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(0)
                                                        .WithEffectOnSelf(caster => {
                                                            caster.AddQEffect(QEffect.Slowed(2));
                                                        }),
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, IllustrationName.Drained, "Drained 1", new Trait[] {},
                                                        "",
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(0)
                                                        .WithEffectOnSelf(caster => {
                                                            caster.AddQEffect(QEffect.Drained(1));
                                                        }),
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, IllustrationName.Paralyzed, "Paralyzed", new Trait[] {},
                                                        "",
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(0)
                                                        .WithEffectOnSelf(caster => {
                                                            caster.AddQEffect(QEffect.Paralyzed().WithExpirationNever());
                                                        }),
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, IllustrationName.Stunned, "Stunned 1", new Trait[] {},
                                                        "",
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(0)
                                                        .WithEffectOnSelf(caster => {
                                                            caster.AddQEffect(QEffect.Stunned(1).WithExpirationNever());
                                                        }),
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, IllustrationName.Stunned, "Stunned 2", new Trait[] {},
                                                        "",
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(0)
                                                        .WithEffectOnSelf(caster => {
                                                            caster.AddQEffect(QEffect.Stunned(2).WithExpirationNever());
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
            hazard.AddQEffect(interactable);

            return hazard;
        }
    }
}