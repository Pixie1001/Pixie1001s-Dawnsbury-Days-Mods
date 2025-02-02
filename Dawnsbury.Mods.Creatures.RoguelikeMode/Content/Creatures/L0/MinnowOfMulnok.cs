using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Animations.AuraAnimations;
using Microsoft.Xna.Framework;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class MinnowOfMulnok
    {
        public static Creature Create()
        {
            var creature = new Creature(Illustrations.MInnowOfMulnok,
                "Minnow of Mulnok",
                [Trait.Aquatic, Trait.Animal, Trait.Demon, Trait.Chaotic, Trait.Evil],
                0, 6, 7,
                new Defenses(15, 6, 8, 3),
                15,
                new Abilities(1, 3, 2, -4, 0, -2),
                new Skills(athletics: 5, survival: 4))
            .WithCharacteristics(false, true)
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Good, 3))
            .AddQEffect(QEffect.DamageImmunity(DamageKind.Fire))
            .AddQEffect(QEffect.Swimming())
            .AddQEffect(new("Boiling Aura", "Creatures that end their turn adjacent to you take 1d4 fire damage.")
            {
                StateCheck = (effect) =>
                {
                    var minnow = effect.Owner;

                    foreach (Creature creature in minnow.Battle.AllCreatures)
                    {
                        if (creature.BaseName != "Minnow of Mulnok" && creature.IsAdjacentTo(minnow) && creature.QEffects.FirstOrDefault((qEffect) => qEffect.Name == "Boiling") == null)
                        {
                            creature.AddQEffect(new("Boiling", "At the end of your turn, you take 1d4 fire damage.", ExpirationCondition.Ephemeral, minnow, IllustrationName.WallOfFire)
                            {
                                EndOfYourTurnDetrimentalEffect = async (QEffect boilingEffect, Creature effectOwner) =>
                                {
                                    await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d4"), effectOwner, CheckResult.Failure, DamageKind.Fire);
                                }
                            });
                        }
                    }
                }
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.Walk, "Group Swim", [Trait.Flourish], "You command each of your ally minnows to stride as a free action.", Target.Self())
                    .WithActionCost(1)
                    .WithEffectOnSelf(async (CombatAction combatAction, Creature user) =>
                    {
                        for (int i = 0; i < user.Battle.AllCreatures.Count; i++)
                        {
                            var creature = user.Battle.AllCreatures[i];

                            if (creature.FriendOfAndNotSelf(user) && creature.BaseName == "Minnow of Mulnok")
                            {
                                await creature.StrideAsync("Select where you want to stride.", allowPass: true);

                                if (!user.Battle.AllCreatures.Contains(creature))
                                {
                                    i--;
                                }
                            }
                        }
                    })
                    .WithGoodness((Target _, Creature user, Creature _) =>
                    {
                        if (user.Actions.ActionsLeft < 2 || user.Actions.ActionHistoryThisTurn.Where((action) => action.Name == "Group Swim") == null)
                        {
                            return AIConstants.NEVER;
                        }

                        var enemies = user.Battle.AllCreatures.Where((creature) => creature.EnemyOf(user));

                        var adjacency = false;

                        foreach (var enemy in enemies)
                        {
                            if (user.IsAdjacentTo(enemy))
                            {
                                adjacency = true;

                                break;
                            }
                        }

                        if (!adjacency)
                        {
                            return AIConstants.NEVER;
                        }

                        var minnows = user.Battle.AllCreatures.Where((creature) => creature.FriendOf(user) && creature.BaseName == "Minnow of Mulnok");

                        var total = minnows.Count();

                        if (total < 2)
                        {
                            return AIConstants.NEVER;
                        }

                        var activeCount = 0;

                        foreach (var minnow in minnows)
                        {
                            if (minnow == user)
                            {
                                continue;
                            }

                            adjacency = false;

                            foreach (var subMinnow in minnows)
                            {
                                if (subMinnow != minnow && subMinnow.IsAdjacentTo(minnow))
                                {
                                    adjacency = true;

                                    break;
                                }
                            }

                            if (!adjacency)
                            {
                                activeCount++;
                            }
                        }

                        if (activeCount >= (total - 1) / 4 + 1)
                        {
                            return AIConstants.ALWAYS;
                        }

                        return AIConstants.NEVER;

                        /*var minnows = user.Battle.AllCreatures.Where((creature) => creature.FriendOf(user) && creature.BaseName == "Minnow of Mulnok" && !creature.Actions.IsReactionUsedUp && creature != user);

                        var total = minnows.Count();

                        if (total < 2)
                        {
                            return AIConstants.NEVER;
                        }

                        var enemies = user.Battle.AllCreatures.Where((creature) => creature.EnemyOf(user));

                        var activeCount = 0;

                        foreach (var minnow in minnows)
                        {
                            var adjacency = false;

                            foreach (var subMinnow in minnows)
                            {
                                if (subMinnow != minnow && subMinnow.IsAdjacentTo(minnow))
                                {
                                    adjacency = true;

                                    break;
                                }
                            }

                            if (!adjacency)
                            {
                                activeCount++;
                                continue;
                            }

                            adjacency = false;

                            foreach (var enemy in enemies)
                            {
                                if (minnow.IsAdjacentTo(enemy))
                                {
                                    adjacency = true;

                                    break;
                                }
                            }

                            if (!adjacency)
                            {
                                activeCount++;
                            }
                        }

                        if (activeCount >= 2)
                        {
                            return AIConstants.ALWAYS;
                        }

                        return AIConstants.NEVER;*/
                    });
                }
            })
            .AddQEffect(new("Mesmerizing Movement", "When an adjacent minnow takes damage, you can use your reaction to take that damage instead.")
            {
                StateCheck = (effect) =>
                {
                    var minnow = effect.Owner;

                    foreach (Creature creature in minnow.Battle.AllCreatures)
                    {
                        if (creature.FriendOfAndNotSelf(minnow) && creature.BaseName == "Minnow of Mulnok" && creature.IsAdjacentTo(minnow))
                        {
                            creature.AddQEffect(new(ExpirationCondition.Ephemeral)
                            {
                                Source = minnow,
                                YouAreDealtDamage = async (QEffect qEffect, Creature _, DamageStuff damageStuff, Creature defender) =>
                                {
                                    var source = qEffect.Source;

                                    if (source == null || source.Actions.IsReactionUsedUp || damageStuff.Amount == 0 || (source.HP < defender.HP && damageStuff.Amount < defender.HP) || damageStuff.Amount >= source.HP || defender.QEffects.FirstOrDefault((q) => q.Name == "DamageMarker") != null)
                                    {
                                        return null;
                                    }

                                    if (await source.AskToUseReaction($"Use your reaction to take {damageStuff.Amount} damage instead of {defender.Name}?"))
                                    {
                                        source.AddQEffect(new(ExpirationCondition.Ephemeral)
                                        {
                                            Name = "DamageMarker"
                                        });

                                        await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText(damageStuff.Amount.ToString()), source, CheckResult.Failure, DamageKind.Untyped);

                                        return new ReduceDamageModification(damageStuff.Amount, $"Protected by {source.Name}");
                                    }

                                    return null;
                                }
                            });
                        }
                    }
                }
            })
            .AddQEffect(QEffect.PackAttack("minnow", "1d4"))
            .AddQEffect(new("School Frenzy", "All minnows of mulnok adjacent to another minnow of mulnok have a +1 circumstance bonus to AC, attack rolls, and saving throws.")
            {
                StateCheck = (effect) =>
                {
                    var minnow = effect.Owner;

                    foreach (Creature creature in minnow.Battle.AllCreatures)
                    {
                        if (creature.FriendOfAndNotSelf(minnow) && creature.BaseName == "Minnow of Mulnok" && creature.IsAdjacentTo(minnow) && creature.QEffects.FirstOrDefault((qEffect) => qEffect.Name == "Frenzied") == null)
                        {
                            creature.AddQEffect(new("Frenzied", "You have a +1 circumstance bonus to AC, attack rolls, and saving throws.", ExpirationCondition.Ephemeral, minnow, IllustrationName.Rage)
                            {
                                BonusToAttackRolls = (_, _, _) =>
                                {
                                    return new(1, BonusType.Circumstance, "School Frenzy", true);
                                },
                                BonusToDefenses = (_, _, defense) =>
                                {
                                    if (defense != Defense.AC || defense != Defense.Fortitude || defense != Defense.Reflex || defense != Defense.Will)
                                    {
                                        return null;
                                    }

                                    return new(1, BonusType.Circumstance, "School Frenzy", true);
                                }
                            });
                        }

                        /*if (creature.FriendOfAndNotSelf(minnow) && creature.BaseName == "Minnow of Mulnok")
                        {
                            creature.AddQEffect(new()
                            {
                                Source = minnow,
                                AdditionalGoodness = (QEffect qEffect, CombatAction action, Creature defender) =>
                                {
                                    if (action.ActionId != ActionId.Stride)
                                    {
                                        return 0f;
                                    }

                                    var targetTile = action.ChosenTargets.ChosenTile;

                                    var source = qEffect.Source;
                                    var user = qEffect.Owner;

                                    if (targetTile != null && source != null && targetTile.IsAdjacentTo(source.Occupies))
                                    {
                                        return AIConstants.ALWAYS;
                                    }

                                    return 0f;
                                }
                            });
                        }*/
                    }
                }
            });

            creature.AnimationData.AddAuraAnimation(new MagicCircleAuraAnimation(IllustrationName.BaneCircle, Color.Maroon, 0.85f));

            creature.UnarmedStrike = null;

            UtilityFunctions.AddNaturalWeapon(creature, "jaws", IllustrationName.Jaws, 8, [], "1d4+1", DamageKind.Piercing);

            return creature;
        }
    }
}
