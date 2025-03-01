using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.Movement;
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
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using static Dawnsbury.Delegates;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class MerfolkHarrier {
        public static Creature Create() {
            QEffectId[] rootEffects = new QEffectId[] { QEffectId.Grabbed, QEffectId.Grappled, QEffectId.Restrained, QEffectId.Immobilized, QEffectId.Prone };

            return new Creature(IllustrationName.DarkMerfolk256, "Merfolk Harrier", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Merfolk, Trait.Humanoid, Trait.Aquatic }, 1, 7, 7, new Defenses(17, 4, 9, 6), 19,
            new Abilities(1, 4, 3, 0, 2, 2), new Skills(acrobatics: 7, athletics: 4))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    // Continue fighting if second action in melee
                    if (monster.Actions.ActionsLeft == 2 && monster.Actions.AttackedThisManyTimesThisTurn <= 1 && monster.Battle.AllCreatures.Any(cr => cr.Alive && cr.EnemyOf(monster) && cr.DistanceTo(monster) <= 1)) {
                        return options.Where(opt => opt.Text.Contains("Strike")).MaxBy(opt => opt.AiUsefulness.MainActionUsefulness);
                    }

                    // Use last action to escape melee
                    if (monster.Actions.UsedQuickenedAction && monster.Actions.ActionsLeft == 1 && !monster.QEffects.Any(qf => rootEffects.Contains(qf.Id)) && monster.Actions.AttackedThisManyTimesThisTurn > 0) {
                        List<Creature> allEnemies = monster.Battle.AllCreatures.Where<Creature>((Func<Creature, bool>)(cr => cr.EnemyOf(monster))).ToList<Creature>();
                        List<TileOption> list1 = options.Where<Option>((Func<Option, bool>)(opt => opt is TileOption tileOption5 && tileOption5.OptionKind == OptionKind.MoveHere)).Cast<TileOption>().ToList<TileOption>();
                        if (monster.Battle.AllCreatures.Any(cr => cr.Threatens(monster.Occupies))) {
                            list1 = list1.Where(opt => opt.Tile.DistanceTo(monster.Occupies) <= 3 && Pathfinding.GetPath(monster, opt.Tile, monster.Battle, new PathfindingDescription() {
                                Squares = 7,
                                Style = new MovementStyle() {
                                    ForcedMovement = false,
                                    MaximumSquares = 100,
                                    IgnoresUnevenTerrain = true,
                                    PermitsStep = true,
                                    Shifting = false
                                }
                            })?.Count <= 3).ToList();
                        }
                        TileOption tileOption6 = list1.MaxBy<TileOption, int>((Func<TileOption, int>)(movementOption => allEnemies.Sum<Creature>((Func<Creature, int>)(enemy => movementOption.Tile.DistanceTo(enemy.Occupies)))));
                        if (tileOption6 != null) {
                            return tileOption6;
                        }
                    } else if (monster.Actions.AttackedThisManyTimesThisTurn == 1 && !monster.Battle.AllCreatures.Any(cr => cr.Alive && cr.EnemyOf(monster) && cr.DistanceTo(monster) <= 1)) {
                        return options.Where(opt => opt.AiUsefulness.ObjectiveAction?.Action.ActionId == ActionId.Demoralize).ToList().MaxBy(opt => opt.AiUsefulness.MainActionUsefulness);
                    }
                        //} else if (monster.Actions.AttackedThisManyTimesThisTurn == 1 && monster.Actions.ActionsLeft == 2 && monster.Battle.AllCreatures.Any(cr => cr.Alive && cr.EnemyOf(monster) && cr.DistanceTo(monster) <= 1)) {
                    //    return options.Where(opt => opt.OptionKind != OptionKind.MoveHere).ToList().MaxBy(opt => opt.AiUsefulness.MainActionUsefulness);
                    //}


                    return null;
                };
            })
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Unarmed, Proficiency.Trained)
            .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .AddHeldItem(Items.CreateNew(CustomItems.DuelingSpear))
            .WithUnarmedStrike(new Item(Illustrations.MermaidTail, "tail", Trait.Unarmed, Trait.Brawling, Trait.Finesse, Trait.Agile).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning)))
            .AddQEffect(CommonQEffects.UnderwaterMarauder())
            .AddQEffect(new QEffect() {
                Id = QEffectId.Swimming
            })
            .AddQEffect(new QEffect() {
                Id = QEffectId.IgnoresDifficultTerrain
            })
            .AddQEffect(new QEffect() {
                Id = QEffectId.Flying
            })
            .AddQEffect(new QEffect("Mobility", "When you use Stride to move half your Speed or less, your movement does not trigger reactions.") {
                Id = QEffectId.Mobility
            })
            .AddQEffect(new QEffect("Lacerating Strikes", "Your strikes that deal piercing or slashing damage leave deep flesh wounds, that heal slowly in the brincy waters.") {
                AfterYouDealDamageOfKind = async (attacker, action, kind, defender) => {
                    if (action.HasTrait(Trait.Strike) && (kind == DamageKind.Slashing || kind == DamageKind.Piercing)) {
                        defender.AddQEffect(QEffect.PersistentDamage("1d6", DamageKind.Bleed));
                    }
                },
                AdditionalGoodness = (self, action, defender) => {
                    if (defender.FindQEffect(QEffectId.PersistentDamage)?.Key != $"PersistentDamage:{DamageKind.Bleed.ToStringOrTechnical()}"
                    && action.HasTrait(Trait.Strike) && action.Item != null
                    && action.Item.DetermineDamageKinds().ContainsOneOf(new DamageKind[] { DamageKind.Piercing, DamageKind.Slashing })) {
                        return 4f;
                    }
                    return 0f;
                }
            });
        }
    }
}