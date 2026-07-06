using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
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
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.Mechanics.Zoning;
using Dawnsbury.Display.Illustrations;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class Brazier {
        public static Creature Create() {

            Creature hazard = new Creature(Illustrations.Brazier, "Brazier", new List<Trait>() { Trait.Object, Trait.Indestructible }, 2, 0, 0, new Defenses(10, 10, 0, 0), 100, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
            .WithSpawnAsGaia()
            .WithTactics(Tactic.DoNothing)
            .WithEntersInitiativeOrder(false);

            QEffect effect = new QEffect("Interactable", "Several lit torches lie inside the brazier, that might be used as an improved weapon against fear averse foes.") {
                StateCheckWithVisibleChanges = async self => {
                    // Add contextual actions
                    foreach (Creature hero in self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsControlledByHumanUser && cr.IsAdjacentTo(self.Owner))) {
                        hero.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                            ProvideContextualAction = qfContextActions => {
                                return new SubmenuPossibility(Illustrations.Brazier, "Interactions") {
                                    Subsections = {
                                        new PossibilitySection(hazard.Name) {
                                            Possibilities = {
                                                (ActionPossibility)new CombatAction(qfContextActions.Owner, Illustrations.Torch, "Pick up torch", [Trait.Manipulate, Trait.Basic],
                                                "Collect a torch from the brazier.",
                                                Target.Self().WithAdditionalRestriction(user => user.HasFreeHand ? null : "No free hand"))
                                                .WithActionCost(1)
                                                .WithEffectOnSelf(async (spell, caster) => {
                                                    var torch = Items.CreateNew(CustomItems.Torch);
                                                    torch.Traits.Add(Trait.EncounterEphemeral);
                                                    caster.AddHeldItem(torch);
                                                })
                                            }

                                        }
                                    }
                                };
                            }
                        });
                    }
                }
            };

            hazard.AddQEffect(effect);

            return hazard;
        }
    }
}