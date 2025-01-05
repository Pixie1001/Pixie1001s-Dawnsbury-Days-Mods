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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2 {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class ChokingMushroom {
        public static Creature Create() {
            QEffect qfCurrentDC = new QEffect() { Value = 14 };

            Creature hazard = new Creature(Illustrations.ChokingMushroom, "Choking Mushroom", new List<Trait>() { Trait.Object, Trait.Plant }, 2, 0, 0, new Defenses(10, 10, 0, 0), 20, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
            .WithTactics(Tactic.DoNothing)
            .WithEntersInitiativeOrder(false)
            .AddQEffect(qfCurrentDC)
            .AddQEffect(CommonQEffects.Hazard())
            .AddQEffect(new QEffect("Choking Spores", "This predatory mushroom exhudes a cloud of poisonous spores to suffocate its prey. Creatures walking through the spores suffer 1d4 poison damage vs. a DC 17 Basic fortitude save, and become sickened 1 on a critical failure."))
            ;

            QEffect effect = new QEffect("Interactable", "You can use Medicine, Nature and Occultism to interact with this mushroom.") {
                WhenMonsterDies = self => {
                    foreach (Tile tile in self.Tag as List<Tile>) {
                        if (tile.QEffects.Any(qf => qf.TileQEffectId == QEffectIds.ChokingSpores)) {
                            tile.QEffects.RemoveAll(qf => qf.TileQEffectId == QEffectIds.ChokingSpores);
                        }
                    }
                },
                StateCheckWithVisibleChanges = async self => {
                    if (!self.Owner.Alive) {
                        return;
                    }

                    self.Owner.WeaknessAndResistance.AddWeakness(DamageKind.Fire, 5);
                    self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Piercing, 5);

                    if (self.Tag == null) {
                        self.Tag = self.Owner.Battle.Map.AllTiles.Where(t => t.DistanceTo(self.Owner.Occupies) <= 2 && !new TileKind[] { TileKind.BlocksMovementAndLineOfEffect, TileKind.Tree, TileKind.Rock, TileKind.Wall }.Contains(t.Kind)).ToList();
                    }

                    // Add contextual actions
                    foreach (Creature hero in self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsHumanControlled && cr.IsAdjacentTo(self.Owner))) {
                        hero.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                            ProvideContextualAction = qfContextActions => {
                                return new SubmenuPossibility(Illustrations.ChokingMushroom, "Interactions") {
                                    Subsections = {
                                                new PossibilitySection(hazard.Name) {
                                                    Possibilities = {
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, Illustrations.ChokingMushroom, "Soothe Mushroom", new Trait[] { Trait.Manipulate, Trait.Basic },
                                                        "Folktales speak of ancient rites and traditions used by cavern folk to appease the mushroom forests. Make an Occultism check against DC " + qfCurrentDC.Value + "." + S.FourDegreesOfSuccess("The mushroom will stop emitting spores for the rest of the encounter.",
                                                        "The mushroom will stop emitting spores for 2 rounds.", null, "You take 1d6 poison damage."),
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(1)
                                                        .WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Occultism), Checks.FlatDC(qfCurrentDC.Value)))
                                                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                            if (result == CheckResult.CriticalFailure) {
                                                                if (caster.FindQEffect(QEffectIds.MushroomInoculation) == null) {
                                                                    await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Choking Spores"), caster, CheckResult.Success, DamageKind.Poison);
                                                                }
                                                            }
                                                            if (result >= CheckResult.Success) {
                                                                //foreach (Tile tile in self.Tag as List<Tile>) {
                                                                //    if (tile.QEffects.Any(qf => qf.TileQEffectId == QEffectIds.ChokingSpores)) {
                                                                //        tile.QEffects.RemoveAll(qf => qf.TileQEffectId == QEffectIds.ChokingSpores);
                                                                //    }
                                                                //}
                                                                self.WhenMonsterDies(self);
                                                            }
                                                            if (result == CheckResult.Success) {
                                                                target.AddQEffect(new QEffect("Soothed", "The choking mushroom has stopped emitting spores.") {
                                                                    Value = 2,
                                                                    Id = QEffectId.Recharging,
                                                                    ExpiresAt = ExpirationCondition.CountsDownAtStartOfSourcesTurn,
                                                                    Source = caster,
                                                                    Illustration = IllustrationName.Soothe,
                                                                    Innate = false
                                                                });
                                                            }
                                                            if (result == CheckResult.CriticalSuccess) {
                                                                target.AddQEffect(new QEffect("Soothed", "The choking mushroom has stopped emitting spores.") {
                                                                    Value = 100,
                                                                    Id = QEffectId.Recharging,
                                                                    ExpiresAt = ExpirationCondition.Never,
                                                                    Source = caster,
                                                                    Illustration = IllustrationName.Soothe,
                                                                    Innate = false
                                                                });
                                                            }
                                                            await hazard.Battle.GameLoop.StateCheck();
                                                        }),
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, Illustrations.ChokingMushroom, "Recall Knowledge", new Trait[] { Trait.Manipulate, Trait.Basic },
                                                        "Make a Nature check against DC " + qfCurrentDC.Value + "." + S.FourDegreesOfSuccess("Reduce all DCs on this hazard by 3.", "Reduce all DCs on this hazard by 2.", null, "You take 1d6 poison damage and increase all DCs on this hazard by 1."),
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(1)
                                                        .WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Nature), Checks.FlatDC(qfCurrentDC.Value - 2)))
                                                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                            if (result == CheckResult.CriticalFailure) {
                                                                if (caster.FindQEffect(QEffectIds.MushroomInoculation) == null) {
                                                                    await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Choking Spores"), caster, CheckResult.Success, DamageKind.Poison);
                                                                }
                                                                qfCurrentDC.Value += 1;
                                                            }
                                                            if (result == CheckResult.Success) {
                                                                qfCurrentDC.Value -= 2;
                                                            }
                                                            if (result == CheckResult.CriticalSuccess) {
                                                                qfCurrentDC.Value -= 3;
                                                            }
                                                        }),
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, IllustrationName.DashOfHerbs, "Harvest Pollen", new Trait[] { Trait.Manipulate, Trait.Basic, Trait.Alchemical },
                                                            "Make a Medicine check against DC " + qfCurrentDC.Value + "." + S.FourDegreesOfSuccess("Heal your or an adjacent ally for 3d6 HP and inoculate them against the effects of spore clouds. This mushroom then cannot be harvested from again.",
                                                            "As per a critical success, but you only heal for 2d6 HP.", null, "You take 1d6 poison damage."),
                                                            Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner))
                                                            .WithAdditionalConditionOnTargetCreature((a, d) => !a.HasFreeHand ? Usability.NotUsable("No free hand") : Usability.Usable)
                                                            .WithAdditionalConditionOnTargetCreature((a, d) => {
                                                                if (d.QEffects.Any(qf => qf.Id == QEffectIds.Harvested)) {
                                                                    return Usability.NotUsableOnThisCreature("Already harvested");
                                                                }
                                                                return Usability.Usable;
                                                            }))
                                                        .WithActionCost(1)
                                                        .WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Medicine), Checks.FlatDC(qfCurrentDC.Value)))
                                                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                            if (result == CheckResult.CriticalFailure) {
                                                                if (caster.FindQEffect(QEffectIds.MushroomInoculation) == null) {
                                                                    await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Choking Spores"), caster, CheckResult.Success, DamageKind.Poison);
                                                                }
                                                            }
                                                            if (result >= CheckResult.Success) {
                                                                //target.AddQEffect(new QEffect() { Id = QEffectIds.Harvested });
                                                                target.AddQEffect(new QEffect("Harvested", "Pollen cannot be harvested again", ExpirationCondition.Never, caster, IllustrationName.DashOfHerbs) { Id = QEffectIds.Harvested });
                                                                List<Option> options = new List<Option>();

                                                                foreach (Creature ally in caster.Battle.AllCreatures.Where(cr => cr.OwningFaction.AlliedFactionOf(caster.OwningFaction) && cr.DistanceTo(caster) <= 1)) {
                                                                    options.Add(new CreatureOption(ally, $"Heal {ally.Name} with pollen.", async() => {
                                                                        await ally.HealAsync(DiceFormula.FromText($"{(result == CheckResult.Success ? "2" : "3")}d6", "Harvest Mushroom"), CombatAction.CreateSimple(target));
                                                                        ally.Occupies.Overhead("*healed*", Color.Green, $"{ally.Name} was healed by {caster.Name} using healing pollen.");
                                                                        ally.AddQEffect(new QEffect("Inoculating Spores", "You're inoculated against the effects of the choking spores.", ExpirationCondition.Never, self.Owner, new SameSizeDualIllustration(Illustrations.StatusBackdrop, Illustrations.ChokingMushroom)) {
                                                                            Id = QEffectIds.MushroomInoculation
                                                                        });
                                                                    }, 7, true));
                                                                }
                                                                var chosenOption = (await caster.Battle.SendRequest(new AdvancedRequest(caster, "Select an ally to heal with the nectar.", options) {
                                                                    IsMainTurn = false,
                                                                    TopBarIcon = IllustrationName.DashOfHerbs,
                                                                    TopBarText = "Select an ally to heal with the nectar."
                                                                })).ChosenOption;
                                                                await chosenOption.Action();
                                                            }
                                                        }),
                                                    }

                                                }
                                            }
                                };
                            }
                        });
                    }

                    if (self.Owner.QEffects.Any(qf => qf.Id == QEffectId.Recharging) || !self.Owner.Alive || self.Owner.Destroyed) {
                        return;
                    }

                    // Apply poison to tiles
                    foreach (Tile tile in self.Tag as List<Tile>) {
                        if (!tile.QEffects.Any(qf => qf.TileQEffectId == QEffectIds.ChokingSpores)) {
                            TileQEffect spores = new TileQEffect(tile) {
                                Name = "Choking Spores",
                                VisibleDescription = "Toxic spores used by Choking Mushrooms to hunt for nutrients. Suffer 1d4 poison damage vs. a Basic (DC 17) fort save after entering or starting your turn within the spores, that inflicts sickened 1 on a critical failure.",
                                TileQEffectId = QEffectIds.ChokingSpores,
                                ExpiresAt = ExpirationCondition.Never,
                                Illustration = IllustrationName.Fog,
                                TransformsTileIntoHazardousTerrain = true,
                                AfterCreatureBeginsItsTurnHere = async victim => {
                                    if (victim.IsImmuneTo(Trait.Poison) || victim.WeaknessAndResistance.Immunities.Contains(DamageKind.Poison) || victim.FindQEffect(QEffectIds.MushroomInoculation) != null) {
                                        return;
                                    }
                                    CheckResult result = CommonSpellEffects.RollSavingThrow(victim, CombatAction.DefaultCombatAction, Defense.Fortitude, 17);
                                    await CommonSpellEffects.DealBasicDamage(CombatAction.CreateSimple(hazard, "Choking Spores", Trait.Poison), hazard, victim, result, new KindedDamage(DiceFormula.FromText("1d4", "Choking Spores"), DamageKind.Poison));
                                    if (result == CheckResult.CriticalFailure) {
                                        victim.AddQEffect(QEffect.Sickened(1, 17));
                                    }
                                },
                                StateCheck = self => {
                                    self.Owner.FoggyTerrain = true;
                                }
                            };
                            spores.AfterCreatureEntersHere = async victim => await spores.AfterCreatureBeginsItsTurnHere(victim);
                            tile.AddQEffect(spores);
                        }
                    }
                }
            };

            //effect.AddGrantingOfTechnical(cr => !cr.OwningFaction.IsHumanControlled, qfShroom => {
            //    qfShroom.ai
            //});

            hazard.AddQEffect(effect);

            return hazard;
        }
    }
}