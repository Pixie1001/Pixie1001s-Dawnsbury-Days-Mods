using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Audio;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Core.Roller;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Ardamok
    {
        public static Creature Create()
        {
            var creature = new Creature(Illustrations.Ardamok,
                "Ardamok",
                [Trait.Animal, ModTraits.MeleeMutator],
                2, 6, 5,
                new Defenses(18, 11, 6, 4),
                30,
                new Abilities(4, 1, 3, -4, 1, -4),
                new Skills(athletics: 9, stealth: 5))
            .WithCharacteristics(false, true)
            .AddQEffect(new("Attack of Opportunity{icon:Reaction}", "When a creature leaves a square within your reach, makes a ranged attack or uses a move or manipulate action, you can tail Strike it for free. On a critical hit, you also disrupt the manipulate action."))
            .AddQEffect(TailAttackOfOpportunity())
            .Builder
            .AddMainAction((creature) =>
            {
                return new CombatAction(creature, IllustrationName.Shield, "Retract", [], "You pull your head, tail, and legs into your body to protect yourself. Your speed is reduced to 15 feet, you have a +3 circumstance bonus to your AC, you can't make claw strikes, and you lose Attack of Opportunity.",
                    Target.Self((Creature user, AI ai) =>
                    {
                        var nearestEnemyDistance = -1;

                        foreach (var enemy in user.Battle.AllCreatures.Where((enemy) => enemy.HP > 0 && enemy.EnemyOf(user)))
                        {
                            var distance = user.DistanceTo(enemy);

                            if (nearestEnemyDistance == -1 || distance < nearestEnemyDistance)
                            {
                                nearestEnemyDistance = distance;
                            }
                        }

                        return !user.QEffects.Any((ef) => ef.Name == "Retracted") && (user.Actions.AttackedThisManyTimesThisTurn >= 1 || nearestEnemyDistance > 3) ? ai.GainBonusToAC(3) : AIConstants.NEVER;
                    }))
                .WithActionCost(2)
                .WithSoundEffect(SfxName.RaiseShield)
                .WithEffectOnSelf(async (Creature user) =>
                {
                    user.RemoveAllQEffects((QEffect effect) => effect.Id == QEffectId.AttackOfOpportunity);

                    user.AddQEffect(new("Retracted", "Your speed is reduced to 15 feet, you have a +3 circumstance bonus to your AC, you can't make claw strikes, and you lose Attack of Opportunity.", ExpirationCondition.Never, user, IllustrationName.Shield)
                    {
                        Name = "Retracted",
                        BonusToDefenses = (QEffect qEffect, CombatAction? attackAction, Defense defense) =>
                        {
                            if (defense != Defense.AC)
                            {
                                return null;
                            }

                            return new Bonus(3, BonusType.Circumstance, "retracted");
                        },
                        BonusToAllSpeeds = (_) => new Bonus(-2, BonusType.Untyped, "retracted", false),
                        CountsAsABuff = true,
                        PreventTakingAction = (CombatAction action) =>
                        {
                            return action.Name == "Strike (claw)" ? "You can't make claw strikes while retracted." : null;
                        },
                        WhenExpires = (QEffect qEffect) =>
                        {
                            qEffect.Owner.AddQEffect(TailAttackOfOpportunity());
                        },
                        DoNotShowUpOverhead = true
                    });
                });
            })
            .AddMainAction((creature) =>
            {
                return new CombatAction(creature, new SideBySideIllustration(IllustrationName.Action, IllustrationName.StarHit), "Sudden Lunge", [], "{b}Requirement{/b} You are retracted. {b}Effect{/b} You extend your limbs and charge. You lose the benefits of Retract, then you Stride twice and make a beak Strike. If this Strike hits, it deals an additional 1d6 damage.",
                    Target.Ranged(creature.Speed * 2 + 1, (Target _, Creature user, Creature target) =>
                    {
                        var action = user.CreateStrike("beak");

                        var bonusTotal = 0;

                        foreach (var bonus in target.Defenses.DetermineDefenseBonuses(user, action, Defense.AC, target))
                        {
                            if (bonus != null)
                            {
                                bonusTotal += bonus.Amount;
                            }
                        }

                        return user.DistanceTo(target) > 1 ? user.AI.DealDamageWithAttack(action, 11, bonusTotal, target, 14f) : user.AI.DealDamageWithAttack(action, 11, bonusTotal, target, 10f);
                    }).WithAdditionalConditionOnTargetCreature((Creature user, Creature _) => !user.QEffects.Any((ef) => ef.Name == "Retracted") || user.Actions.ActionHistoryThisTurn.Any((action) => action.HasTrait(Trait.Move)) ? Usability.NotUsable("not retracted") : Usability.Usable))
                .WithActionCost(2)
                .WithSoundEffect(SfxName.StandUp)
                .WithEffectOnEachTarget(async (CombatAction _, Creature user, Creature target, CheckResult _) =>
                {
                    user.RemoveAllQEffects((QEffect effect) => effect.Name == "Retracted");

                    user.AddQEffect(new(ExpirationCondition.Never)
                    {
                        Tag = user,
                        AddExtraStrikeDamage = (_, _) => (DiceFormula.FromText("1d6", "lunging"), DamageKind.Piercing),
                        BonusToAllSpeeds = (_) => new Bonus(user.Speed, BonusType.Untyped, "lunging", true),
                        DoNotShowUpOverhead = true
                    });

                    await user.StrideAsync("Choose where to stride.", strideTowards:target.Occupies);

                    var strike = user.CreateStrike("beak");
                    strike.ActionCost = 0;

                    await user.Battle.GameLoop.FullCast(strike);

                    user.RemoveAllQEffects((QEffect effect) => effect.Tag == user);
                });
            })
            .AddMainAction((creature) =>
            {
                return new CombatAction(creature, new SideBySideIllustration(IllustrationName.StarHit, IllustrationName.Tail), "Tail Thrash", [], "You make a tail Strike against each enemy within your reach. Each attack counts toward your multiple attack penalty, but your penalty doesn't increase until you have made all of your attacks.",
                    Target.Self((Creature user, AI ai) =>
                    {
                        var action = user.CreateStrike("tail");

                        var target = Target.ReachWithWeaponOfTrait(Trait.Reach);
                        target.SetOwnerAction(action);
                        var targets = target.GetLegalTargetCreatures(user);

                        if (targets.Count < 3)
                        {
                            return 0f;
                        }

                        var goodness = 0f;

                        foreach (var targetCreature in targets)
                        {
                            var bonusTotal = 0;

                            foreach (var bonus in targetCreature.Defenses.DetermineDefenseBonuses(user, action, Defense.AC, targetCreature))
                            {
                                if (bonus != null)
                                {
                                    bonusTotal += bonus.Amount;
                                }
                            }

                            goodness += user.AI.DealDamageWithAttack(action, 11, bonusTotal, targetCreature, 6.5f);
                        }

                        return goodness;
                    }))
                .WithActionCost(3)
                .WithEffectOnSelf(async (Creature user) =>
                {
                    var action = user.CreateStrike("tail");

                    var target = Target.ReachWithWeaponOfTrait(Trait.Reach);
                    target.SetOwnerAction(action);
                    var targets = target.GetLegalTargetCreatures(user);

                    foreach (var targetCreature in targets)
                    {
                        //Default unarmed strike is tail
                        await user.MakeStrike(targetCreature, user.UnarmedStrike, 0);
                    }
                });
            })
            .Done();

            creature.UnarmedStrike = null;
            creature = UtilityFunctions.AddNaturalWeapon(creature, "tail", IllustrationName.Tail, 11, [Trait.Reach], "1d4+4", DamageKind.Bludgeoning, null);
            creature = UtilityFunctions.AddNaturalWeapon(creature, "beak", IllustrationName.Jaws, 11, [], "1d10+4", DamageKind.Piercing, null);
            creature = UtilityFunctions.AddNaturalWeapon(creature, "claw", IllustrationName.DragonClaws, 11, [Trait.Agile], "1d8+4", DamageKind.Slashing, null);

            return creature;
        }

        private static QEffect TailAttackOfOpportunity()
        {
            var effect = QEffect.AttackOfOpportunity("Attack of Opportunity", "When a creature leaves a square within your reach, makes a ranged attack or uses a move or manipulate action, you can tail Strike it for free. On a critical hit, you also disrupt the manipulate action.",
                                (effect, _) => effect.Owner.QEffects.Any((QEffect qf) => qf.Name == "Retracted") ? false : true, false);

            effect.Innate = false;

            return effect;
        }
    }
}
