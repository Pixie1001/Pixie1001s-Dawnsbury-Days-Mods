using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Display;
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
using Dawnsbury.Core.Tiles;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class HeartOfTheStorm {
        public static Creature Create(int radius, int dc) {
            //int radius = 3;
            //int dc = 22;

            Creature hazard = new Creature(IllustrationName.None, "Heart of the Storm", new List<Trait>() { Trait.Object, Trait.Indestructible, Trait.NoDeathOverhead }, 5, 0, 0, new Defenses(10, 10, 0, 0), 20, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
            .WithTactics(Tactic.DoNothing)
            .WithEntersInitiativeOrder(false)
            .AddQEffect(QEffect.FullImmunity())
            .AddQEffect(new QEffect() { Id = QEffectId.SeeInvisibility })
            ;

            var animation = hazard.AnimationData.AddAuraAnimation(IllustrationName.AngelicHaloCircle, radius);
            animation.Color = Color.Navy;

            QEffect effect = new QEffect("Maelstrom",
                $"All enemies that start their turn within {radius * 5}-feet of the heart of the storm must make a DC {dc} basic fortitude save or suffer 2d6 bludgeoning damage. On a failure, they're also knocked prone by the roiling currents." +
                "\n\nEnemies attempting to move through the maelstrom must made a Basic Fort save against 1d8 bludgeoning damage.") {
                //StartOfCombat = async self => {
                //    foreach (Tile tile in self.Owner.Battle.Map.AllTiles.Where(t => t.DistanceTo(self.Owner.Occupies) <= radius)) {
                //        tile.QEffects.Add(CommonQEffects.Maelstrom(dc, tile, self.Owner));
                //    }
                //},
                StateCheck = self => {
                    if (!self.Owner.Battle.AllCreatures.Any(cr => cr.Alive && cr.CreatureId == CreatureIds.MerfolkSeaWitch)) {
                        foreach (Tile tile in self.Owner.Battle.Map.AllTiles) {
                            tile.QEffects.RemoveAll(qf => qf.TileQEffectId == QEffectIds.Maelstrom);
                        }
                        self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                    }
                }
            };

            effect.AddGrantingOfTechnical(cr => cr.OwningFaction.IsPlayer, qfTechnical => {
                qfTechnical.Source = hazard;
                qfTechnical.Innate = false;
                qfTechnical.StartOfYourPrimaryTurn = async (self, owner) => {
                    if (owner.DistanceTo(self.Source.Occupies) <= radius) {
                        //SavingThrow st = new SavingThrow();
                        //CommonSpellEffects.DealBasicDamage();

                        CombatAction ca = new CombatAction(hazard, IllustrationName.TidalHands, "Maelstrom", [Trait.Water, Trait.Evocation], "", Target.Ranged(100))
                        .WithSavingThrow(new SavingThrow(Defense.Fortitude, dc))
                        .WithSoundEffect(SfxName.ElementalBlastWater)
                        .WithEffectOnEachTarget(async (spell, user, d, result) => {
                            await CommonSpellEffects.DealBasicDamage(spell, user, d, result, DiceFormula.FromText($"2d6", "Maelstrom"), DamageKind.Bludgeoning);
                            if (result < CheckResult.Success) {
                                d.AddQEffect(QEffect.Prone());
                            }
                        });

                        if (owner.Traits.Any(t => t.HumanizeTitleCase2() == "Eidolon") && owner.Battle.AllCreatures.Any(cr => cr.DistanceTo(qfTechnical.Source.Occupies) <= radius && cr.Traits.Any(t => t.HumanizeTitleCase2() == "Summoner"))) {
                            QEffect bond = owner.QEffects.FirstOrDefault(qf => qf.Id.HumanizeTitleCase2() == "Summoner_Shared HP");
                            ca.Target = Target.Emanation(radius).WithIncludeOnlyIf((area, target) => {
                                return target == owner || target == bond.Source;
                            });
                            ca.ChosenTargets.ChosenCreatures.Add(owner);
                            ca.ChosenTargets.ChosenCreatures.Add(bond.Source);
                            await ca.AllExecute();
                        } else if (owner.Traits.Any(t => t.HumanizeTitleCase2() == "Summoner") && owner.Battle.AllCreatures.Any(cr => cr.DistanceTo(qfTechnical.Source.Occupies) <= radius && cr.Traits.Any(t => t.HumanizeTitleCase2() == "Eidolon"))) {
                            return;
                        } else {
                            ca.ChosenTargets.ChosenCreatures.Add(owner);
                            ca.ChosenTargets.ChosenCreature = owner;
                            await ca.AllExecute();
                        }
                    };
                };
            });
            hazard.AddQEffect(effect);

            return hazard;
        }
    }
}