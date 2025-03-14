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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class FlailSnail
    {
        public static Creature Create()
        {
            var creature = new Creature(Illustrations.FlailSnail,
                "Flail Snail",
                [Trait.Animal],
                5, 15, 3,
                new Defenses(23, 16, 8, 12),
                60,
                new Abilities(4, 0, 5, -4, 0, -4),
                new Skills(survival: 12))
            .WithCharacteristics(false, false)
            .AddQEffect(new("Snail Mucin", "You leave a trail of slime everywhere you go, with the effects of Grease. In addition, your mucin sticks you to the ground, granting you a +5 circumstance bonus against checks to trip you.")
            {
                BonusToDefenses = (QEffect _, CombatAction? action, Defense defense) =>
                {
                    if (action != null)
                    {
                        if (action.ActionId == ActionId.Trip)
                        {
                            return new Bonus(5, BonusType.Circumstance, "snail mucin", true);
                        }
                        else if (action.Name == "Grease")
                        {
                            return new Bonus(40, BonusType.Untyped, "snail mucin", true);
                        }
                    }

                    return null;
                },
                StartOfYourEveryTurn = async (QEffect _, Creature user) =>
                {
                    user.Actions.PassedBalanceCheckThisTurn = true;
                },
                StateCheck = (QEffect effect) =>
                {
                    if (!effect.Owner.Occupies.QEffects.Any((tileEffect) => tileEffect.BalanceDC != 0 && tileEffect.BalanceDC >= GetDC(effect.Owner)))
                    {
                        var tileEffect = new TileQEffect(effect.Owner.Occupies);

                        tileEffect.BalanceDC = GetDC(effect.Owner);
                        tileEffect.BalanceAllowsReflexSave = true;
                        tileEffect.Illustration = IllustrationName.GreaseTile;
                        tileEffect.VisibleDescription = "{b}Grease.{/b} A creature that enters here must make an Acrobatics check or stop moving or even fall prone.";
                        tileEffect.TransformsTileIntoHazardousTerrain = true;

                        effect.Owner.Occupies.AddQEffect(tileEffect);
                    }
                }
            })
            .AddQEffect(new("Warp Magic", "Spells that target you have a 50% chance to bounce off of your shell.")
            {
                FizzleIncomingActions = async (QEffect effect, CombatAction action, StringBuilder stringBuilder) =>
                {
                    var user = effect.Owner;
                    
                    if (!action.HasTrait(Trait.Arcane) && !action.HasTrait(Trait.Divine) && !action.HasTrait(Trait.Occult) && !action.HasTrait(Trait.Primal))
                    {
                        return false;
                    }

                    if (R.Coin())
                    {
                        return false;
                    }

                    var rand = R.Next(4);

                    if (rand == 0)
                    {
                        return true;
                    }
                    else if (rand == 1)
                    {
                        var reboundAction = new CombatAction(user, IllustrationName.CascadeCountermeasure, "Reflect", [Trait.Magical], "", Target.SelfExcludingEmanation(2))
                        .WithActionCost(0)
                        .WithGoodness((_, _, _) => AIConstants.ALWAYS)
                        .WithSavingThrow(new(Defense.Fortitude, GetDC(user)))
                        .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.MagicMissile))
                        .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                        {
                            await CommonSpellEffects.DealBasicDamage(action, user, target, result, "2d8", DamageKind.Force);
                        });

                        user.Battle.Log($"{user.Name}'s shell reflects the {action.Name}, dealing damage to everyone around it.");

                        await user.Battle.GameLoop.FullCast(reboundAction);

                        return true;
                    }
                    else if (rand == 2)
                    {
                        user.Battle.Log($"{user.Name} absorbs the {action.Name} and gains 10 temporary hit points.");

                        user.GainTemporaryHP(10);

                        return true;
                    }
                    else
                    {
                        var reboundAction = new CombatAction(user, IllustrationName.CascadeCountermeasure, "Rebound", [Trait.Magical], "", Target.Ranged(100).WithAdditionalConditionOnTargetCreature((_, creature) => creature == action.Owner ? Usability.Usable : Usability.NotUsableOnThisCreature("not spellcaster")))
                        .WithActionCost(0)
                        .WithGoodness((_, _, _) => AIConstants.ALWAYS)
                        .WithSavingThrow(new(Defense.Will, GetDC(user)))
                        .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.MagicMissile))
                        .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                        {
                            await CommonSpellEffects.DealBasicDamage(action, user, target, result, "3d8", DamageKind.Mental);
                        });

                        user.Battle.Log($"{user.Name}'s shell rebounds the {action.Name}, dealing damage to the caster.");

                        await user.Battle.GameLoop.FullCast(reboundAction);

                        return true;
                    }
                }
            })
            .Builder
            .AddMainAction((Creature creature) =>
            {
                return new CombatAction(creature, new SideBySideIllustration(IllustrationName.RubEyes, IllustrationName.RubEyes), "Flurry of Flails", [], "You make two eye flail strikes.", Target.Self())//Target.MultipleCreatureTargets(Target.ReachWithAnyWeapon(), Target.ReachWithAnyWeapon()))
                .WithActionCost(1)
                .WithGoodness((Target _, Creature user, Creature _) =>
                {
                    var action = user.CreateStrike("eye flail");

                    foreach (var enemy in user.Battle.AllCreatures.Where((creature2) => creature2.EnemyOf(user) && creature2.HP > 0))
                    {
                        if (user.DistanceTo(enemy) <= 1)
                        {
                            var bonusTotal = 0;

                            foreach (var bonus in enemy.Defenses.DetermineDefenseBonuses(user, action, Defense.AC, enemy))
                            {
                                if (bonus != null)
                                {
                                    bonusTotal += bonus.Amount;
                                }
                            }

                            return user.AI.DealDamageWithAttack(action, 13, bonusTotal, enemy, 14f) * 2f;
                        }
                    }

                    return AIConstants.NEVER;
                })
                .WithEffectOnEachTarget(async (CombatAction _, Creature user, Creature _, CheckResult _) =>
                {
                    var strike = user.CreateStrike("eye flail").WithActionCost(0);

                    await user.Battle.GameLoop.FullCast(strike);
                    await user.Battle.GameLoop.FullCast(strike);
                });
            })
            .Done();
            creature.UnarmedStrike = null;

            creature = UtilityFunctions.AddNaturalWeapon(creature, "eye flail", IllustrationName.Flail, 15, [Trait.Sweep, Trait.Forceful], "2d8+6", DamageKind.Bludgeoning, null);
            
            return creature;
        }

        public static async Task<CheckResult?> OfferAndMakeReactiveStrike(Creature attacker, Creature target, string question, string overhead, int numberOfStrikes)
        {
            var itemListEffect = attacker.QEffects.FirstOrDefault((effect) => effect.Name == "Multiple Reactions");

            if (itemListEffect != null && itemListEffect.Tag is List<Item> itemList && itemList.Count > 0)
            {
                var item = itemList[0];

                CombatAction strike = attacker.CreateStrike(item, 0).WithActionCost(0);
                strike.Traits.Add(Trait.AttackOfOpportunity);
                CreatureTarget creatureTarget = (CreatureTarget)strike.Target;
                CheckResult? bestCheckResult = null;
                if ((bool)strike.CanBeginToUse(attacker) && creatureTarget.IsLegalTarget(attacker, target).CanBeUsed && (itemList.Count > 1 || await attacker.Battle.AskToUseReaction(attacker, question)))
                {
                    itemList.RemoveAt(0);
                    int map = attacker.Actions.AttackedThisManyTimesThisTurn;
                    attacker.Occupies.Overhead(overhead, Color.White);

                    for (int i = 0; i < numberOfStrikes; i++)
                    {
                        CheckResult checkResult = await attacker.MakeStrike(strike, target);
                        if (!bestCheckResult.HasValue)
                        {
                            bestCheckResult = checkResult;
                        }
                        else if (checkResult > bestCheckResult)
                        {
                            bestCheckResult = checkResult;
                        }
                    }

                    attacker.Actions.AttackedThisManyTimesThisTurn = map;
                }

                return bestCheckResult;
            }

            return null;
        }

        private struct DragonType
        {
            public Defense Defense;

            public string Description;

            public DamageKind DamageKind;

            public IllustrationName Illustration;

            public SfxName SoundEffect;

            public Target Target;

            public Trait[] Traits;
        }

        #region Methods

        public static int GetDC(Creature creature)
        {
            if (creature.Level <= 4)
            {
                return 20;
            }
            else if (creature.Level == 5)
            {
                return 22;
            }
            else
            {
                return 24;
            }
        }

        public static string ModifyDamageString(Creature creature, string damage)
        {
            if (creature.Level <= 4)
            {
                return damage + "+-4";
            }
            else if (creature.Level == 5)
            {
                return damage;
            }
            else
            {
                return damage + "+4";
            }
        }

        #endregion
    }
}
