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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class YoungChimera
    {
        public static Creature Create()
        {
            var breathWeaponList = new List<DragonType>()
            {
                new DragonType(){DamageKind = DamageKind.Acid, Target = Target.Line(12), Defense = Defense.Reflex, Traits = [Trait.Acid], Illustration = IllustrationName.AcidArrow, Description = "You deal 6d6 acid damage to all creatures a 60-foot line (DC 22 basic Reflex save). You can’t use Dragon Breath again for 1d4 rounds.", SoundEffect = SfxName.AcidSplash},
                new DragonType(){DamageKind = DamageKind.Electricity, Target = Target.Line(12), Defense = Defense.Reflex, Traits = [Trait.Electricity], Illustration = IllustrationName.LightningBolt, Description = "You deal 6d6 electricity damage to all creatures a 60-foot line (DC 22 basic Reflex save). You can’t use Dragon Breath again for 1d4 rounds.", SoundEffect = SfxName.ElectricBlast},
                new DragonType(){DamageKind = DamageKind.Poison, Target = Target.Cone(6), Defense = Defense.Fortitude, Traits = [Trait.Poison], Illustration = IllustrationName.AcidSplash, Description = "You deal 6d6 poison damage to all creatures a 30-foot cone (DC 22 basic Fortitude save). You can’t use Dragon Breath again for 1d4 rounds.", SoundEffect = SfxName.SprayPerfume},
                new DragonType(){DamageKind = DamageKind.Fire, Target = Target.Cone(6), Defense = Defense.Reflex, Traits = [Trait.Fire], Illustration = IllustrationName.Fireball, Description = "You deal 6d6 fire damage to all creatures a 30-foot cone (DC 22 basic Reflex save). You can’t use Dragon Breath again for 1d4 rounds.", SoundEffect = SfxName.Fireball},
                new DragonType(){DamageKind = DamageKind.Cold, Target = Target.Cone(6), Defense = Defense.Reflex, Traits = [Trait.Cold], Illustration = IllustrationName.ConeOfCold, Description = "You deal 6d6 cold damage to all creatures a 30-foot cone (DC 22 basic Reflex save). You can’t use Dragon Breath again for 1d4 rounds.", SoundEffect = SfxName.WintersClutch},
                new DragonType(){DamageKind = DamageKind.Mental, Target = Target.Cone(6), Defense = Defense.Will, Traits = [Trait.Mental], Illustration = IllustrationName.Daze, Description = "You deal 6d6 mental damage to all creatures a 30-foot cone (DC 22 basic Will save). You can’t use Dragon Breath again for 1d4 rounds.", SoundEffect = SfxName.Mental}
            };

            var chosenBreathWeapon = breathWeaponList[R.Next(breathWeaponList.Count)];

            var creature = new Creature(Illustrations.Chimera,
                "Young Chimera",
                [Trait.Beast, Trait.Evil, Trait.Chaotic],
                5, 12, 8,
                new Defenses(22, 14, 12, 10),
                75,
                new Abilities(5, 1, 4, -3, 1, 0),
                new Skills(athletics: 13, acrobatics: 10, stealth: 13))
            .WithCharacteristics(false, true)
            .AddQEffect(QEffect.Flying())
            .AddQEffect(new("Multiple Reactions", "A chimera gains 2 extra reactions each round that it can use only to make Attacks of Opportunity. It must use a different head for each reaction, and it can't use more than one on the same triggering action.")
            {
                Tag = new List<Item>(),
                StartOfYourPrimaryTurn = async (QEffect effect, Creature user) =>
                {
                    //Set up the list that extra reactions come from. These are removed as they are used, then refreshed here every turn.
                    if (effect.Tag is List<Item> itemList)
                    {
                        itemList.Clear();

                        itemList.Add(user.UnarmedStrike);

                        var goatHornsEffect = user.QEffects.FirstOrDefault((qEffect) => qEffect.AdditionalUnarmedStrike != null && qEffect.AdditionalUnarmedStrike.Name == "goat horns");

                        if (goatHornsEffect != null)
                        {
                            itemList.Add(goatHornsEffect.AdditionalUnarmedStrike!);
                        }

                        var lionJawsEffect = user.QEffects.FirstOrDefault((qEffect) => qEffect.AdditionalUnarmedStrike != null && qEffect.AdditionalUnarmedStrike.Name == "lion jaws");

                        if (lionJawsEffect != null)
                        {
                            itemList.Add(lionJawsEffect.AdditionalUnarmedStrike!);
                        }
                    }
                },
                StartOfCombat = async (QEffect effect) =>
                {
                    //Set up the list that extra reactions come from. These are removed as they are used, then refreshed here every turn.
                    if (effect.Tag is List<Item> itemList)
                    {
                        itemList.Clear();

                        itemList.Add(effect.Owner.UnarmedStrike);

                        var goatHornsEffect = effect.Owner.QEffects.FirstOrDefault((qEffect) => qEffect.AdditionalUnarmedStrike != null && qEffect.AdditionalUnarmedStrike.Name == "goat horns");

                        if (goatHornsEffect != null)
                        {
                            itemList.Add(goatHornsEffect.AdditionalUnarmedStrike!);
                        }

                        var lionJawsEffect = effect.Owner.QEffects.FirstOrDefault((qEffect) => qEffect.AdditionalUnarmedStrike != null && qEffect.AdditionalUnarmedStrike.Name == "lion jaws");

                        if (lionJawsEffect != null)
                        {
                            itemList.Add(lionJawsEffect.AdditionalUnarmedStrike!);
                        }
                    }
                }
            })
            .AddQEffect(new("Attack of Opportunity{icon:Reaction}", "When a creature leaves a square within your reach, makes a ranged attack or uses a move or manipulate action, you can Strike it for free. On a critical hit, you also disrupt the manipulate action.")
            {
                Id = QEffectId.AttackOfOpportunity,
                WhenProvoked = async delegate (QEffect attackOfOpportunityQEffect, CombatAction provokingAction)
                {
                    var user = attackOfOpportunityQEffect.Owner;
                    var target = provokingAction.Owner;
                    if ((await OfferAndMakeReactiveStrike(user, target, "{b}" + target.Name + "{/b} uses {b}" + provokingAction.Name + "{/b} which provokes.\nUse your reaction to make an attack of opportunity?", "*attack of opportunity*", 1)).GetValueOrDefault() == CheckResult.CriticalSuccess && provokingAction.HasTrait(Trait.Manipulate))
                    {
                        provokingAction.Disrupted = true;
                    }
                }
            })
            .Builder
            .AddMainAction((Creature creature) =>
            {
                return new CombatAction(creature, IllustrationName.BreathWeapon, "Dragon Breath", chosenBreathWeapon.Traits, chosenBreathWeapon.Description, chosenBreathWeapon.Target)
                .WithActionCost(2)
                .WithSavingThrow(new(chosenBreathWeapon.Defense, GetDC(creature)))
                .WithProjectileCone(VfxStyle.BasicProjectileCone(chosenBreathWeapon.Illustration))
                .WithSoundEffect(chosenBreathWeapon.SoundEffect)
                .WithGoodnessAgainstEnemy((Target _, Creature _, Creature target) =>
                {
                    if (target.WeaknessAndResistance.Immunities.Contains(chosenBreathWeapon.DamageKind))
                    {
                        return 0f;
                    }
                    var resistance = target.WeaknessAndResistance.Resistances.Find((resistance) => resistance.DamageKind == chosenBreathWeapon.DamageKind);
                    if (resistance != null)
                    {
                        return 21f - resistance.Value;
                    }

                    return 21f;
                })
                .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                {
                    await CommonSpellEffects.DealBasicDamage(action, user, target, result, ModifyDamageString(user, "6d6"), chosenBreathWeapon.DamageKind);
                })
                .WithEffectOnSelf(async (Creature user) =>
                {
                    user.AddQEffect(QEffect.Recharging("Dragon Breath"));
                });
            })
            .AddMainAction((Creature creature) =>
            {
                return new CombatAction(creature, new SideBySideIllustration(IllustrationName.Jaws, IllustrationName.Horn), "Three-Headed Strike", [], "You make a Strike with your dragon jaws, lion jaws, and goat horns, each at a –2 penalty and targeting a different creature. These Strikes count as only one attack for your multiple attack penalty, and the penalty doesn’t increase until after you have made all three attacks.", Target.MultipleCreatureTargets(Target.ReachWithAnyWeapon(), Target.ReachWithAnyWeapon(), Target.ReachWithAnyWeapon()).WithMustBeDistinct())
                .WithActionCost(2)
                .WithGoodnessAgainstEnemy((Target _, Creature user, Creature target) =>
                {
                    var action = user.CreateStrike("dragon jaws");

                    var bonusTotal = 0;

                    foreach (var bonus in target.Defenses.DetermineDefenseBonuses(user, action, Defense.AC, target))
                    {
                        if (bonus != null)
                        {
                            bonusTotal += bonus.Amount;
                        }
                    }

                    return user.AI.DealDamageWithAttack(action, 13, bonusTotal, target, 14f);
                })
                .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult _) =>
                {
                    var attacks = 0;

                    if (!user.QEffects.Any((effect) => effect.Name == "AttackCount"))
                    {
                        user.AddQEffect(new(ExpirationCondition.Never)
                        {
                            Name = "AttackCount",
                            Value = 0,
                            BonusToAttackRolls = (QEffect _, CombatAction combatAction, Creature? _) => combatAction.HasTrait(Trait.Attack) ? new Bonus(-2, BonusType.Untyped, "Three-Headed Strike", false) : null
                        });
                    }
                    
                    var counter = user.QEffects.FirstOrDefault((effect) => effect.Name == "AttackCount");

                    if (counter != null)
                    {
                        attacks = counter.Value;
                    }

                    var attackToUse = user.UnarmedStrike;

                    if (attacks == 1)
                    {
                        var goatHorns = user.QEffects.FirstOrDefault((qEffect) => qEffect.AdditionalUnarmedStrike != null && qEffect.AdditionalUnarmedStrike.Name == "goat horns");

                        if (goatHorns != null)
                        {
                            attackToUse = goatHorns.AdditionalUnarmedStrike!;
                        }
                    }
                    else if (attacks != 0)
                    {
                        var lionJaws = user.QEffects.FirstOrDefault((qEffect) => qEffect.AdditionalUnarmedStrike != null && qEffect.AdditionalUnarmedStrike.Name == "lion jaws");

                        if (lionJaws != null)
                        {
                            attackToUse = lionJaws.AdditionalUnarmedStrike!;
                        }
                    }

                    await user.MakeStrike(target, attackToUse, 0);

                    if (attacks >= 2)
                    {
                        user.RemoveAllQEffects((effect) => effect.Name == "AttackCount");
                    }
                    else if (counter != null)
                    {
                        counter.Value++;
                    }
                });
            })
            .Done();
            creature.UnarmedStrike = null;

            creature = UtilityFunctions.AddNaturalWeapon(creature, "dragon jaws", IllustrationName.Jaws, 15, [], "2d4+7", DamageKind.Piercing, (weaponProperties) => weaponProperties.WithAdditionalDamage("2d4", chosenBreathWeapon.DamageKind));
            creature = UtilityFunctions.AddNaturalWeapon(creature, "goat horns", IllustrationName.Horn, 15, [], "2d6+7", DamageKind.Piercing, null);
            creature = UtilityFunctions.AddNaturalWeapon(creature, "lion jaws", IllustrationName.CelestialLion, 15, [], "2d6+7", DamageKind.Piercing, null);
            creature = UtilityFunctions.AddNaturalWeapon(creature, "claw", IllustrationName.DragonClaws, 15, [Trait.Agile], "2d4+7", DamageKind.Slashing, null);
            
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
