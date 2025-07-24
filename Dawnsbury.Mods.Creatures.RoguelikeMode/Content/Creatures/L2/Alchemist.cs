using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Animations.AuraAnimations;
using Microsoft.Xna.Framework;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Auxiliary;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Alchemist
    {
        public static Creature Create()
        {
            var creature = new Creature(Illustrations.DrowAlchemist,
                "Drow Alchemist",
                [ModTraits.Drow, Trait.Elf, Trait.Humanoid, Trait.Chaotic, Trait.Evil, ModTraits.ArcherMutator],
                2, 8, 5,
                new Defenses(17, 8, 11, 6),
                28,
                new Abilities(0, 3, 1, 4, 3, 0),
                new Skills(acrobatics: 7, crafting: 11, thievery: 7))
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Martial, Proficiency.Master)
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.AnimalForm, "Beast Elixir", [Trait.Manipulate, Trait.Morph, Trait.Basic], "You drink an elixir that transforms your hands and feet into brutal claws.", Target.Self())
                    .WithActionCost(1)
                    .WithGoodness((Target _, Creature user, Creature _) =>
                    {
                        if (user.Actions.AttackedThisManyTimesThisTurn == 0)
                        {
                            return AIConstants.NEVER;
                        }

                        // TODO: Replace this with .NextAI
                        return 9.3f + R.NextFloat();
                    })
                    .WithSoundEffect(Audio.SfxName.DrinkPotion)
                    .WithEffectOnSelf(async (Creature user) =>
                    {
                        user.AddQEffect(new("Beast Form", "You gain a claws unarmed attack that deals 1d10+4 slashing damage that has the agile and finesse traits, and you have a +10-foot item bonus to speed.", ExpirationCondition.Never, user, IllustrationName.DragonClaws)
                        {
                            PreventTakingAction = (CombatAction action) => action.Name == "Beast Elixir" ? "You are already under the effects of this elixir" : null,
                            BonusToAllSpeeds = (QEffect _) => new Bonus(2, BonusType.Item, "Beast Elixir"),
                            BonusToAttackRolls = (QEffect _, CombatAction action, Creature? _) => action.Name == "Strike (claws)" ? new Bonus(2, BonusType.Item, "Beast Form", true) : null
                        });

                        user.Builder.AddNaturalWeapon(NaturalWeaponKind.Claw, 9, [Trait.Agile, Trait.Finesse], "1d10+4", DamageKind.Slashing);
                    });
                }
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.ConeOfCold, "Cold Breath Elixir", [Trait.Manipulate, Trait.Cold, Trait.Basic], $"You drink an elizir that causes you to exhale lightning, dealing {ModifyDamageString(effect.Owner, "2d4")} cold damage to all creatures in a 15-foot cone (DC {GetDC(effect.Owner)} basic Fortitude save). Creatures that fail gain weakness 2 to bludgeoning and fire for 1 round (1 minute on a critical failure). You can't drink another breath weapon elixir for 1d4 rounds.", Target.Cone(3))
                    .WithActionCost(2)
                    .WithSavingThrow(new(Defense.Fortitude, GetDC(effect.Owner)))
                    .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.ConeOfCold))
                    .WithSoundEffect(Audio.SfxName.WintersClutch)
                    .WithGoodnessAgainstEnemy((Target _, Creature _, Creature target) =>
                    {
                        if (target.WeaknessAndResistance.Immunities.Contains(DamageKind.Cold))
                        {
                            return 0f;
                        }

                        var resistance = target.WeaknessAndResistance.Resistances.Find((resistance) => resistance.DamageKind == DamageKind.Cold);

                        if (resistance != null)
                        {
                            return 7f + R.NextFloat() - resistance.Value;
                        }

                        return 7f + R.NextFloat();
                    })
                    .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                    {
                        await CommonSpellEffects.DealBasicDamage(action, user, target, result, ModifyDamageString(effect.Owner, "2d4"), DamageKind.Cold);

                        if (result >= CheckResult.Success)
                        {
                            return;
                        }

                        var weakness = new QEffect("Frozen", "You are covered in ice and have weakness 2 to bludgeoning and fire damage.", ExpirationCondition.ExpiresAtStartOfSourcesTurn, user, IllustrationName.FrostRunestone)
                        {
                            StateCheck = delegate (QEffect qf)
                            {
                                qf.Owner.WeaknessAndResistance.AddWeakness(DamageKind.Bludgeoning, 2);
                                qf.Owner.WeaknessAndResistance.AddWeakness(DamageKind.Fire, 2);
                            }
                        };

                        if (result == CheckResult.CriticalFailure)
                        {
                            weakness.ExpiresAt = ExpirationCondition.Never;
                        }

                        target.AddQEffect(weakness);
                    })
                    .WithEffectOnSelf(async (Creature user) =>
                    {
                        user.AddQEffect(RechargingBreathWeapon("Cold Breath Elixir"));
                    });
                }
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.AcidArrow, "Great Acid Bomb", [Trait.Manipulate, Trait.Acid, Trait.Basic], $"You throw a bomb, dealing {ModifyDamageString(effect.Owner, "2d4")} acid damage plus 1d4 persistent acid damage to all creatures in a 10 foot burst within 20 feet (DC {GetDC(effect.Owner)} basic Reflex save). You can't throw another great bomb for 1d4 rounds.", Target.Burst(4, 2))
                    .WithActionCost(2)
                    .WithSavingThrow(new(Defense.Reflex, GetDC(effect.Owner)))
                    .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.AcidArrow))
                    .WithSoundEffect(Audio.SfxName.AcidSplash)
                    .WithGoodnessAgainstEnemy((Target _, Creature _, Creature target) =>
                    {
                        if (target.WeaknessAndResistance.Immunities.Contains(DamageKind.Acid))
                        {
                            return 0f;
                        }

                        var resistance = target.WeaknessAndResistance.Resistances.Find((resistance) => resistance.DamageKind == DamageKind.Acid);

                        if (resistance != null)
                        {
                            return 7f + R.NextFloat() - resistance.Value * 1.5f;
                        }

                        return 7f + R.NextFloat();
                    })
                    .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                    {
                        await CommonSpellEffects.DealBasicDamage(action, user, target, result, ModifyDamageString(effect.Owner, "2d4"), DamageKind.Acid);
                        await CommonSpellEffects.DealBasicPersistentDamage(target, result, "1d4", DamageKind.Acid);
                    })
                    .WithEffectOnSelf(async (Creature user) =>
                    {
                        user.AddQEffect(RechargingGreatBomb("Great Acid Bomb"));
                    });
                }
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.Fireball, "Great Fire Bomb", [Trait.Manipulate, Trait.Fire, Trait.Basic], $"You throw a bomb, dealing {ModifyDamageString(effect.Owner, "2d6")} fire damage to all creatures in a 10 foot burst within 20 feet (DC {GetDC(effect.Owner)} basic Reflex save). You can't throw another great bomb for 1d4 rounds.", Target.Burst(4, 2))
                    .WithActionCost(2)
                    .WithSavingThrow(new(Defense.Reflex, GetDC(effect.Owner)))
                    .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.Fireball))
                    .WithSoundEffect(Audio.SfxName.Fireball)
                    .WithGoodnessAgainstEnemy((Target _, Creature _, Creature target) =>
                    {
                        if (target.WeaknessAndResistance.Immunities.Contains(DamageKind.Fire))
                        {
                            return 0f;
                        }

                        var resistance = target.WeaknessAndResistance.Resistances.Find((resistance) => resistance.DamageKind == DamageKind.Fire);

                        if (resistance != null)
                        {
                            return 7f + R.NextFloat() - resistance.Value;
                        }

                        return 7f + R.NextFloat();
                    })
                    .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                    {
                        await CommonSpellEffects.DealBasicDamage(action, user, target, result, ModifyDamageString(effect.Owner, "2d6"), DamageKind.Fire);
                    })
                    .WithEffectOnSelf(async (Creature user) =>
                    {
                        user.AddQEffect(RechargingGreatBomb("Great Fire Bomb"));
                    });
                }
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.AnimalForm, "Healing Bomb", [Trait.Manipulate, Trait.Healing, Trait.Basic], "You throw a restorative bomb at an ally within 20 feet. The target regains 1d4+4 hit points. You can't use another healing bomb for 1d4 rounds.", Target.RangedFriend(4))
                    .WithActionCost(1)
                    .WithSoundEffect(Audio.SfxName.Healing)
                    .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.Heal))
                    .WithGoodness((Target _, Creature user, Creature target) =>
                    {
                        //Close to 1 when the target is close to 0 hit points. At or below 0 if the target is above max health - 6.
                        var hitPointRatio = ((target.HP * -1f) + target.MaxHPMinusDrained - 6f) / (target.MaxHPMinusDrained - 6f);

                        if (user.Actions.AttackedThisManyTimesThisTurn == 0 || hitPointRatio <= 0)
                        {
                            return AIConstants.NEVER;
                        }

                        return hitPointRatio + 9f + R.NextFloat();
                    })
                    .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                    {
                        await target.HealAsync("1d4+4", action);
                    })
                    .WithEffectOnSelf(async (Creature user) =>
                    {
                        user.AddQEffect(QEffect.Recharging("Healing Bomb"));
                    });
                }
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.PersistentBleed, "Lacerating Bomb", [Trait.Attack, Trait.Ranged, Trait.Basic], $"You throw a bomb, dealing {ModifyDamageString(effect.Owner, "2d4")} slashing damage plus 1 persistent bleed damage to a target within 20 feet. You can't throw another lava bomb for 1d4 rounds.", Target.Ranged(4))
                    .WithActionCost(1)
                    .WithActiveRollSpecification(new ActiveRollSpecification(Checks.Attack(Items.CreateNew(ItemName.AlchemistsFire)), Checks.DefenseDC(Defense.AC)))
                    .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.PersistentBleed))
                    .WithSoundEffect(Audio.SfxName.BoneSpray)
                    .WithGoodnessAgainstEnemy((Target _, Creature _, Creature target) =>
                    {
                        if (target.WeaknessAndResistance.Immunities.Contains(DamageKind.Slashing))
                        {
                            return AIConstants.NEVER;
                        }

                        var resistance = target.WeaknessAndResistance.Resistances.Find((resistance) => resistance.DamageKind == DamageKind.Piercing);

                        if (resistance != null)
                        {
                            return 9f + R.NextFloat() - resistance.Value;
                        }

                        return 9f + R.NextFloat();
                    })
                    .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                    {
                        await CommonSpellEffects.DealAttackRollDamage(action, user, target, result, ModifyDamageString(effect.Owner, "2d6"), DamageKind.Slashing);

                        if (result == CheckResult.CriticalSuccess)
                        {
                            target.AddQEffect(QEffect.PersistentDamage("2", DamageKind.Bleed));
                        }
                        else if (result == CheckResult.Success)
                        {
                            target.AddQEffect(QEffect.PersistentDamage("1", DamageKind.Bleed));
                        }
                    })
                    .WithEffectOnSelf(async (Creature user) =>
                    {
                        user.AddQEffect(QEffect.Recharging("Lacerating Bomb"));
                    });
                }
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.Fireball, "Lava Bomb", [Trait.Attack, Trait.Ranged, Trait.Fire, Trait.Basic], $"You throw a bomb, dealing {ModifyDamageString(effect.Owner, "2d8")} fire damage to a target within 20 feet. You can't throw another lava bomb for 1d4 rounds.", Target.Ranged(4))
                    .WithActionCost(1)
                    .WithActiveRollSpecification(new ActiveRollSpecification(Checks.Attack(Items.CreateNew(ItemName.AlchemistsFire)), Checks.DefenseDC(Defense.AC)))
                    .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.Fireball))
                    .WithSoundEffect(Audio.SfxName.FireRay)
                    .WithGoodnessAgainstEnemy((Target _, Creature _, Creature target) =>
                    {
                        if (target.WeaknessAndResistance.Immunities.Contains(DamageKind.Fire))
                        {
                            return AIConstants.NEVER;
                        }

                        var resistance = target.WeaknessAndResistance.Resistances.Find((resistance) => resistance.DamageKind == DamageKind.Fire);

                        if (resistance != null)
                        {
                            return 9f + R.NextFloat() - resistance.Value;
                        }

                        return 9f + R.NextFloat();
                    })
                    .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                    {
                        await CommonSpellEffects.DealAttackRollDamage(action, user, target, result, ModifyDamageString(effect.Owner, "2d8"), DamageKind.Fire);
                    })
                    .WithEffectOnSelf(async (Creature user) =>
                    {
                        user.AddQEffect(QEffect.Recharging("Lava Bomb"));
                    });
                }
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.LightningBolt, "Lightning Breath Elixir", [Trait.Manipulate, Trait.Electricity, Trait.Basic], $"You drink an elixir that causes you to exhale lightning, dealing {ModifyDamageString(effect.Owner, "2d4")} electricity damage to all creatures in a 20-foot line (DC {GetDC(effect.Owner)} basic Reflex save). You can't drink another breath weapon elixir for 1d4 rounds.", Target.Line(4))
                    .WithActionCost(2)
                    .WithSavingThrow(new(Defense.Reflex, GetDC(effect.Owner)))
                    .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.LightningBolt))
                    .WithSoundEffect(Audio.SfxName.ElectricBlast)
                    .WithGoodnessAgainstEnemy((Target _, Creature _, Creature target) =>
                    {
                        if (target.WeaknessAndResistance.Immunities.Contains(DamageKind.Electricity))
                        {
                            return 0f;
                        }

                        var resistance = target.WeaknessAndResistance.Resistances.Find((resistance) => resistance.DamageKind == DamageKind.Electricity);

                        if (resistance != null)
                        {
                            return 7f + R.NextFloat() - resistance.Value;
                        }

                        return 7f + R.NextFloat();
                    })
                    .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                    {
                        await CommonSpellEffects.DealBasicDamage(action, user, target, result, ModifyDamageString(effect.Owner, "2d6"), DamageKind.Electricity);
                    })
                    .WithEffectOnSelf(async (Creature user) =>
                    {
                        user.AddQEffect(RechargingBreathWeapon("Lightning Breath Elixir"));
                    });
                }
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.ElectricArc, "Lightning Shield Elixir", [Trait.Manipulate, Trait.Electricity, Trait.Basic], $"You drink an elixir that surrounds you in bands of electricity. Enemies that end their turn within 5 feet of you take {ModifyDamageString(effect.Owner, "2d4")} electricity damage (DC {GetDC(effect.Owner)} basic Reflex save). You gain 3 temporary hit points when you drink this elixir and at the start of your turn.", Target.Self())
                    .WithActionCost(1)
                    .WithGoodness((Target _, Creature user, Creature _) =>
                    {
                        if (user.Actions.AttackedThisManyTimesThisTurn == 0)
                        {
                            return AIConstants.NEVER;
                        }

                        return 9.35f + R.NextFloat();
                    })
                    .WithSoundEffect(Audio.SfxName.DrinkPotion)
                    .WithEffectOnSelf(async (Creature user) =>
                    {
                        user.AddQEffect(new("Lightning Barrier", $"Creatures that end their turn adjacent to you take {ModifyDamageString(effect.Owner, "1d6")} electricity damage. At the start of your turn, you gain 3 temporary hit points.", ExpirationCondition.Never, user, IllustrationName.ElectricArc)
                        {
                            PreventTakingAction = (CombatAction action) => action.Name == "Lightning Shield Elixir" ? "You are already under the effects of this elixir" : null,
                            EndOfYourTurnBeneficialEffect = async (QEffect _, Creature creature) =>
                            {
                                creature.GainTemporaryHP(3);
                            },
                            StateCheck = (effect) =>
                            {
                                var alchemist = effect.Owner;

                                foreach (Creature creature in alchemist.Battle.AllCreatures)
                                {
                                    if (creature.IsAdjacentTo(alchemist))
                                    {
                                        creature.AddQEffect(new("In Lightning Barrier", $"At the end of your turn, you take {ModifyDamageString(alchemist, "1d6")} electricity damage (DC {GetDC(alchemist)} basic Reflex save).", ExpirationCondition.Ephemeral, alchemist, IllustrationName.ElectricArc)
                                        {
                                            Source = alchemist,
                                            EndOfYourTurnDetrimentalEffect = async (QEffect damageEffect, Creature effectOwner) =>
                                            {
                                                if (damageEffect.Source == null)
                                                {
                                                    return;
                                                }

                                                var action = new CombatAction(damageEffect.Source, IllustrationName.ElectricArc, "Lightning Barrier", [], "", Target.AdjacentCreature());
                                                action.ChosenTargets.ChosenCreature = effectOwner;

                                                var saveResult = CommonSpellEffects.RollSavingThrow(effectOwner, action, Defense.Fortitude, GetDC(effect.Owner));

                                                await CommonSpellEffects.DealBasicDamage(action, damageEffect.Source, effectOwner, saveResult, ModifyDamageString(alchemist, "1d6"), DamageKind.Electricity);
                                            }
                                        });
                                    }
                                }
                            }
                        });

                        user.GainTemporaryHP(3);

                        user.AnimationData.AddAuraAnimation(new MagicCircleAuraAnimation(IllustrationName.BaneCircle, Color.LightGoldenrodYellow, 0.85f));
                    });
                }
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.Fear, "Frightening Bomb", [Trait.Attack, Trait.Ranged, Trait.Mental, Trait.Basic], $"You throw a bomb, dealing {ModifyDamageString(effect.Owner, "2d4")} mental damage to a target within 20 feet. On a hit, the target becomes frightened 1 (frightened 2 on a critical hit). You can't throw another frightening bomb for 1d4 rounds.", Target.Ranged(4))
                    .WithActionCost(1)
                    .WithActiveRollSpecification(new ActiveRollSpecification(Checks.Attack(Items.CreateNew(ItemName.AlchemistsFire)), Checks.DefenseDC(Defense.AC)))
                    .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.Fear))
                    .WithSoundEffect(Audio.SfxName.Fear)
                    .WithGoodnessAgainstEnemy((Target _, Creature _, Creature target) => !target.HasEffect(QEffectId.Frightened) ? 9f + R.NextFloat() : AIConstants.NEVER)
                    .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                    {
                        await CommonSpellEffects.DealAttackRollDamage(action, user, target, result, ModifyDamageString(effect.Owner, "2d4"), DamageKind.Mental);

                        if (result == CheckResult.CriticalSuccess)
                        {
                            target.AddQEffect(QEffect.Frightened(2));
                        }
                        else if (result == CheckResult.Success)
                        {
                            target.AddQEffect(QEffect.Frightened(1));
                        }
                    })
                    .WithEffectOnSelf(async (Creature user) =>
                    {
                        user.AddQEffect(QEffect.Recharging("Frightening Bomb"));
                    });
                }
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.Sickened, "Sickening Bomb", [Trait.Attack, Trait.Ranged, Trait.Poison, Trait.Basic], $"You throw a bomb, dealing {ModifyDamageString(effect.Owner, "2d4")} poison damage to a target within 20 feet. On a hit, the target becomes sickened 1 (sickened 2 on a critical hit). You can't throw another sickening bomb for 1d4 rounds.", Target.Ranged(4))
                    .WithActionCost(1)
                    .WithActiveRollSpecification(new ActiveRollSpecification(Checks.Attack(Items.CreateNew(ItemName.AlchemistsFire)), Checks.DefenseDC(Defense.AC)))
                    .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.Sickened))
                    .WithSoundEffect(Audio.SfxName.AcidSplash)
                    .WithGoodnessAgainstEnemy((Target _, Creature _, Creature target) => !target.HasEffect(QEffectId.Sickened) ? 9f + R.NextFloat() : AIConstants.NEVER)
                    .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                    {
                        await CommonSpellEffects.DealAttackRollDamage(action, user, target, result, ModifyDamageString(effect.Owner, "2d4"), DamageKind.Poison);

                        if (result == CheckResult.CriticalSuccess)
                        {
                            target.AddQEffect(QEffect.Sickened(2, GetDC(effect.Owner)));
                        }
                        else if (result == CheckResult.Success)
                        {
                            target.AddQEffect(QEffect.Sickened(1, GetDC(effect.Owner)));
                        }
                    })
                    .WithEffectOnSelf(async (Creature user) =>
                    {
                        user.AddQEffect(QEffect.Recharging("Sickening Bomb"));
                    });
                }
            })
            .AddQEffect(new()
            {
                ProvideMainAction = (QEffect effect) =>
                {
                    return (ActionPossibility)new CombatAction(effect.Owner, IllustrationName.ConeOfCold, "Slowing Bomb", [Trait.Attack, Trait.Ranged, Trait.Cold, Trait.Basic], $"You throw a bomb, dealing {ModifyDamageString(effect.Owner, "2d4")} cold damage to a target within 20 feet. On a hit, the target must make a DC {GetDC(effect.Owner)} Fortitude save or become Slowed 1 for 1 round (Slowed 2 on a critical failure). You can't throw another slowing bomb for 1d4 rounds.", Target.Ranged(4))
                    .WithActionCost(1)
                    .WithActiveRollSpecification(new ActiveRollSpecification(Checks.Attack(Items.CreateNew(ItemName.AlchemistsFire)), Checks.DefenseDC(Defense.AC)))
                    .WithProjectileCone(VfxStyle.BasicProjectileCone(IllustrationName.ConeOfCold))
                    .WithSoundEffect(Audio.SfxName.RayOfFrost)
                    .WithGoodnessAgainstEnemy((Target _, Creature _, Creature target) => target.QEffects.FirstOrDefault((effect) => effect.Name != null && effect.Name.StartsWith("Slowed")) == null ? 9.1f + R.NextFloat() : AIConstants.NEVER)
                    .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                    {
                        await CommonSpellEffects.DealAttackRollDamage(action, user, target, result, ModifyDamageString(effect.Owner, "2d4"), DamageKind.Cold);

                        if (result >= CheckResult.Success)
                        {
                            var saveResult = CommonSpellEffects.RollSavingThrow(target, action, Defense.Fortitude, GetDC(effect.Owner));

                            if (saveResult == CheckResult.Failure)
                            {
                                target.AddQEffect(QEffect.Slowed(1).WithExpirationAtStartOfSourcesTurn(user, 0));
                            }
                            else if (saveResult == CheckResult.CriticalFailure)
                            {
                                target.AddQEffect(QEffect.Slowed(2).WithExpirationAtStartOfSourcesTurn(user, 0));
                            }
                        }
                    })
                    .WithEffectOnSelf(async (Creature user) =>
                    {
                        user.AddQEffect(QEffect.Recharging("Slowing Bomb"));
                    });
                }
            })
            .AddQEffect(new("Alchemical Items", "The alchemist carries a bandolier full of alchemical vials of all shapes and sizes. Who knows what all of them do?"));

            UtilityFunctions.AddNaturalWeapon(creature, "dagger", IllustrationName.Dagger, 9, [Trait.Agile, Trait.Finesse, Trait.VersatileS], "1d4+2", DamageKind.Piercing, (weaponProperties) => weaponProperties.WithAdditionalPersistentDamage("1d4", DamageKind.Bleed));
            
            return creature;
        }

        /*public static Creature ApplyEliteAdjustments(Creature creature, bool supreme = false)
        {
            int level = creature.Level;
            int num = ((level <= 4) ? ((level > 1) ? 15 : 10) : ((level > 19) ? 30 : 20));
            int hpModification = num;
            creature = creature.ApplyAdjustments(supreme ? DifficultyAdjustments.Supreme : DifficultyAdjustments.Elite, supreme ? QEffectId.Supreme : QEffectId.Elite, hpModification);

            foreach
        }*/

        public static int GetDC(Creature creature)
        {
            if (creature.Level <= 1)
            {
                return 17;
            }
            else if (creature.Level == 2)
            {
                return 18;
            }
            else
            {
                return 20;
            }
        }

        public static string ModifyDamageString(Creature creature, string damage)
        {
            if (creature.Level <= 1)
            {
                return damage + "+-2";
            }
            else if (creature.Level == 2)
            {
                return damage;
            }
            else
            {
                return damage + "+2";
            }
        }

        public static QEffect RechargingBreathWeapon(string actionName, Dice rechargeAfterHowManyTurns = Dice.D4)
        {
            string actionName2 = actionName;
            int value = R.Next(2, (int)(rechargeAfterHowManyTurns + 2));
            return new QEffect("Recharging " + actionName2, "This creature can't use " + actionName2 + " until the value counts down to zero.", ExpirationCondition.CountsDownAtEndOfYourTurn, null, IllustrationName.Recharging)
            {
                Id = QEffectId.Recharging,
                CountsAsADebuff = true,
                PreventTakingAction = (CombatAction ca) => ca.Name != null && ca.Name.Contains("Breath") ? "This ability is recharging." : null,
                Value = value
            };
        }

        public static QEffect RechargingGreatBomb(string actionName, Dice rechargeAfterHowManyTurns = Dice.D4)
        {
            string actionName2 = actionName;
            int value = R.Next(2, (int)(rechargeAfterHowManyTurns + 2));
            return new QEffect("Recharging " + actionName2, "This creature can't use " + actionName2 + " until the value counts down to zero.", ExpirationCondition.CountsDownAtEndOfYourTurn, null, IllustrationName.Recharging)
            {
                Id = QEffectId.Recharging,
                CountsAsADebuff = true,
                PreventTakingAction = (CombatAction ca) => ca.Name != null && ca.Name.Contains("Great") ? "This ability is recharging." : null,
                Value = value
            };
        }
    }
}
