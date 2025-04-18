using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Audio;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Intelligence;
using Microsoft.Xna.Framework;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class SorantonTheSkilled
    {
        public static Creature Create()
        {
            var creature = new Creature(IllustrationName.Portrait14,
                "Soranton the Skilled",
                [Trait.Human, Trait.Humanoid, Trait.Chaotic, Trait.Good],
                5, 15, 6,
                new Defenses(22, 9, 15, 12),
                75,
                new Abilities(3, 5, 2, 4, 2, 2),
                new Skills(athletics: 12, acrobatics: 13, stealth: 13, thievery: 13))
            .WithBasicCharacteristics()
            .WithIsNamedMonster()
            .AddQEffect(QEffect.AttackOfOpportunity())
            .AddQEffect(new QEffect("Disarming Flair", "When your Strike hits, you can attempt to disarm the target as a free action without applying or increasing your multiple attack penalty.", ExpirationCondition.Never, null, IllustrationName.None)
            {
                Innate = true,
                AfterYouTakeActionAgainstTarget = async (QEffect effect, CombatAction action, Creature target, CheckResult result) =>
                {
                    var user = effect.Owner;

                    if (result < CheckResult.Success || !action.HasTrait(Trait.Disarm) || target.HeldItems.FirstOrDefault((Item hi) => !hi.HasTrait(Trait.Grapplee)) == null)
                    {
                        return;
                    }

                    var disarm = new CombatAction(user, IllustrationName.Trip, "Disarming Flair", [], "When your Strike hits, you can attempt to disarm the target as a free action without applying or increasing your multiple attack penalty.", Target.ReachWithAnyWeapon().WithAdditionalConditionOnTargetCreature((_, target2) => target2 == target && target2.HeldItems.FirstOrDefault((Item hi) => !hi.HasTrait(Trait.Grapplee)) != null ? Usability.Usable : Usability.NotUsableOnThisCreature("no item to disarm")))
                    .WithActionCost(0)
                    .WithSoundEffect(SfxName.Trip)
                    .WithActionId(ActionId.Disarm)
                    .WithGoodness((Target _, Creature _, Creature _) => AIConstants.ALWAYS)
                    .WithActiveRollSpecification(new(TaggedChecks.SkillCheck(Skill.Athletics), Checks.DefenseDC(Defense.Reflex)))
                    .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                    {
                        if (result >= CheckResult.Success)
                        {
                            var disarmItem = target.HeldItems.FirstOrDefault((Item hi) => !hi.HasTrait(Trait.Grapplee));

                            if (disarmItem == null)
                            {
                                return;
                            }

                            if (target.HeldItems.Count((Item hi) => !hi.HasTrait(Trait.Grapplee)) >= 2)
                            {
                                disarmItem = ((await user.Battle.AskForConfirmation(user, IllustrationName.Disarm, "Which item would you like to disarm your target of?", target.HeldItems[0].Name, target.HeldItems[1].Name)) ? target.HeldItems[0] : target.HeldItems[1]);
                            }

                            if (result == CheckResult.CriticalSuccess)
                            {
                                target.HeldItems.Remove(disarmItem);
                                target.Occupies.DropItem(disarmItem);
                                return;
                            }

                            QEffect qEffect = new QEffect("Weakened grasp", "Attempts to disarm you gain a +2 circumstance bonus, and your attacks with this item take a -2 circumstance penalty.", ExpirationCondition.ExpiresAtStartOfYourTurn, user, IllustrationName.Disarm)
                            {
                                CannotExpireThisTurn = true,
                                Key = "Weakened grasp",
                                BonusToAttackRolls = (QEffect qf, CombatAction ca, Creature? cr) => (ca.Item == disarmItem) ? new Bonus(-2, BonusType.Circumstance, "Weakened grasp (Disarm)") : null,
                                StateCheck = delegate (QEffect qf)
                                {
                                    qf.Owner.Battle.AllCreatures.ForEach(delegate (Creature cr)
                                    {
                                        cr.AddQEffect(new QEffect(ExpirationCondition.Ephemeral)
                                        {
                                            BonusToAttackRolls = (QEffect qff, CombatAction caa, Creature? crr) => (crr == target && caa.ActionId == ActionId.Disarm) ? new Bonus(2, BonusType.Circumstance, "Weakened grasp (Disarm)") : null
                                        });
                                    });
                                }
                            };

                            qEffect.WithExpirationAtEndOfOwnerTurn();

                            target.AddQEffect(qEffect);
                        }
                        else
                        {
                            user.AddQEffect(QEffect.FlatFooted("Failed Disarm").WithExpirationAtStartOfOwnerTurn());
                        }
                    });

                    await user.Battle.GameLoop.FullCast(disarm);
                }/*,
                ProvideMainAction = delegate (QEffect effect)
                {
                    var user = effect.Owner;

                    Creature? attackTarget = null;

                    var lastAction = user.Actions.ActionHistoryThisTurn.LastOrDefault();

                    if (lastAction != null && lastAction.CheckResult >= CheckResult.Success && lastAction.HasTrait(Trait.Disarm) && lastAction.ChosenTargets.ChosenCreature != null)
                    {
                        attackTarget = lastAction.ChosenTargets.ChosenCreature;
                    }
                    
                    return (ActionPossibility)new CombatAction(user, IllustrationName.Trip, "Disarming Flair", [], "When your Strike hits, you can spend a action to attempt to disarm the target without applying or increasing your multiple attack penalty.", Target.ReachWithAnyWeapon().WithAdditionalConditionOnTargetCreature((_, target) => target == attackTarget ? (target.HeldItems.FirstOrDefault((Item hi) => !hi.HasTrait(Trait.Grapplee)) != null ? Usability.Usable : Usability.NotUsableOnThisCreature("no item to disarm")) : Usability.NotUsableOnThisCreature("not the target of your last action")))
                    .WithActionCost(0)
                    .WithSoundEffect(SfxName.Trip)
                    .WithActionId(ActionId.Disarm)
                    .WithGoodness((Target _, Creature _, Creature _) => AIConstants.ALWAYS)
                    .WithActiveRollSpecification(new(TaggedTaggedChecks.SkillCheck(Skill.Athletics), Checks.DefenseDC(Defense.Reflex)))
                    .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                    {
                        if (result >= CheckResult.Success)
                        {
                            var disarmItem = target.HeldItems.FirstOrDefault((Item hi) => !hi.HasTrait(Trait.Grapplee));

                            if (disarmItem == null)
                            {
                                return;
                            }

                            if (target.HeldItems.Count((Item hi) => !hi.HasTrait(Trait.Grapplee)) >= 2)
                            {
                                disarmItem = ((await target.Battle.AskForConfirmation(target, IllustrationName.Disarm, "Which item would you like to disarm your target of?", target.HeldItems[0].Name, target.HeldItems[1].Name)) ? target.HeldItems[0] : target.HeldItems[1]);
                            }

                            if (result == CheckResult.CriticalSuccess)
                            {
                                target.HeldItems.Remove(disarmItem);
                                target.Occupies.DropItem(disarmItem);
                                return;
                            }

                            QEffect qEffect = new QEffect("Weakened grasp", "Attempts to disarm you gain a +2 circumstance bonus, and your attacks with this item take a -2 circumstance penalty.", ExpirationCondition.ExpiresAtStartOfYourTurn, user, IllustrationName.Disarm)
                            {
                                CannotExpireThisTurn = true,
                                Key = "Weakened grasp",
                                BonusToAttackRolls = (QEffect qf, CombatAction ca, Creature? cr) => (ca.Item == disarmItem) ? new Bonus(-2, BonusType.Circumstance, "Weakened grasp (Disarm)") : null,
                                StateCheck = delegate (QEffect qf)
                                {
                                    qf.Owner.Battle.AllCreatures.ForEach(delegate (Creature cr)
                                    {
                                        cr.AddQEffect(new QEffect(ExpirationCondition.Ephemeral)
                                        {
                                            BonusToAttackRolls = (QEffect qff, CombatAction caa, Creature? crr) => (crr == target && caa.ActionId == ActionId.Disarm) ? new Bonus(2, BonusType.Circumstance, "Weakened grasp (Disarm)") : null
                                        });
                                    });
                                }
                            };

                           qEffect.WithExpirationAtEndOfOwnerTurn();

                            target.AddQEffect(qEffect);
                        }
                        else
                        {
                            user.AddQEffect(QEffect.FlatFooted("Failed Disarm").WithExpirationAtStartOfOwnerTurn());
                        }
                    });
                }*/
            })
            .AddQEffect(new("Fancy Footwork", "You are quickened. You can only use the additional action to Step.")
            {
                Id = QEffectId.Quickened,
                QuickenedFor = (CombatAction action) => action.ActionId == ActionId.Step ? true : false
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.Shortsword, "Parry", [], "You gain a +2 circumstance bonus to AC until your next turn.", Target.Self((Creature creature, AI ai) => creature.QEffects.FirstOrDefault((ef) => ef.Name == "Parrying") == null ? ai.GainBonusToAC(2) : AIConstants.NEVER))
                    .WithActionCost(1)
                    .WithSoundEffect(SfxName.RaiseShield)
                    .WithEffectOnSelf((Creature user) =>
                    {
                        user.AddQEffect(new("Parrying", "You have a +2 circumstance bonus to AC.", ExpirationCondition.ExpiresAtStartOfYourTurn, user, IllustrationName.Shortsword)
                        {
                            BonusToDefenses = (QEffect qEffect, CombatAction? attackAction, Defense defense) =>
                            {
                                if (defense != Defense.AC)
                                {
                                    return null;
                                }

                                return new Bonus(2, BonusType.Circumstance, "parrying");
                            },
                            CountsAsABuff = true,
                            DoNotShowUpOverhead = true
                        });
                    });
                }
            })
            .AddQEffect(new("Reflexive Riposte", "At the start of each of your turns when you regain your actions, you gain an additional reaction that can be used only to perform a Riposte."))
            .AddQEffect(new("Ripose {icon:Reaction}", "When an adjacent enemy critically misses you with a Strike, you can use your reaction to Strike that creature.")
            {
                StateCheck = (QEffect effect) =>
                {
                    var user = effect.Owner;

                    foreach (var creature in user.Battle.AllCreatures)
                    {
                        if (creature.EnemyOf(user) && creature.DistanceTo(user) <= 1)
                        {
                            creature.AddQEffect(new(ExpirationCondition.Ephemeral)
                            {
                                AfterYouTakeActionAgainstTarget = async (QEffect effect2, CombatAction action, Creature target, CheckResult result) =>
                                {
                                    if (result == CheckResult.CriticalFailure && action.IsMeleeAgainst(target) && target == user)
                                    {
                                        var usedExtraRiposte = target.QEffects.FirstOrDefault((qf) => qf.Name == "Used Extra Riposte") != null;
                                        
                                        if (!usedExtraRiposte || await target.AskToUseReaction($"{target.Name} critically missed you with a strike. Use your reaction to Ripose?"))
                                        {
                                            if (!usedExtraRiposte)
                                            {
                                                target.AddQEffect(new("Used Extra Riposte", "Soranton the Skilled has used his extra Riposte reaction.", ExpirationCondition.CountsDownAtStartOfSourcesTurn, target, IllustrationName.ReactionUsedUp));
                                            }

                                            target.Occupies.Overhead("Riposte!", Color.Black, "Soranton used Riposte.");

                                            if (user.PrimaryWeapon != null)
                                            {
                                                await user.MakeStrike(action.Owner, user.PrimaryWeapon, 0);
                                            }
                                            else
                                            {
                                                await user.MakeStrike(action.Owner, user.UnarmedStrike, 0);
                                            }
                                        }
                                    }
                                }
                            });
                        }
                    }
                }
            });

            creature = UtilityFunctions.AddManufacturedWeapon(creature, ItemName.Rapier, 15, [Trait.Finesse, Trait.Disarm], "2d8+7", DamageKind.Piercing, null, null);

            return creature;
        }
    }
}
