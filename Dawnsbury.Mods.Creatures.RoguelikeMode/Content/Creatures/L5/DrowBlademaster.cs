using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using System;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowBlademaster {
        public static Creature Create() {
            int poisonDC = 17;
            return new Creature(Illustrations.DrowBlademaster, "Drow Blademaster", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid, ModTraits.MeleeMutator },
                5, 13, 6, new Defenses(22, 9, 15, 12), 75,
            new Abilities(3, 5, 3, 0, 2, 1), new Skills(acrobatics: 13, athletics: 10, stealth: 12, intimidation: 10, deception: 12))
            .WithCreatureId(CreatureIds.DrowBlademaster)
            .WithProficiency(Trait.Unarmed, Proficiency.Trained)
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(QEffect.AttackOfOpportunity(false))
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Weapon, Proficiency.Master)
            .AddHeldItem(Items.CreateNew(ItemName.Shortsword).WithModificationPlusOneStriking().WithMonsterWeaponSpecialization(2))
            .AddHeldItem(Items.CreateNew(ItemName.Shortsword).WithModificationPlusOneStriking().WithMonsterWeaponSpecialization(2))
            .AddQEffect(new QEffect("Double Slice", "You can attack with both weapons at the same time.") {
                ProvideMainAction = (qfSelf) => {
                    if (qfSelf.Owner.HeldItems.Count(itm => itm.HasTrait(Trait.Weapon) && itm.HasTrait(Trait.Melee)) == 2) {
                        return new ActionPossibility(new CombatAction(qfSelf.Owner, IllustrationName.Swords, "Double Slice", [Trait.Fighter, Trait.Basic, Trait.AlwaysHits, Trait.IsHostile],
                            "Make two Strikes against the same target, one with each of your two melee weapons, each using your current multiple attack penalty." +
                            "\n\nIf the second Strike is made with a non-agile weapon it takes a –2 penalty. Combine the damage for the purposes of weakness and resistance. This counts as two attacks when calculating your multiple attack penalty.",
                                    Target.ReachWithBothWeapons().WithAdditionalConditionOnTargetCreature((a, d) => {
                                        return a.CreateStrike(a.HeldItems.First()).CanBeginToUse(a);
                                    }))
                                .WithActionCost(2)
                                .WithGoodnessAgainstEnemy((_, a, d) => a.CreateStrike(a.HeldItems[0] ?? a.UnarmedStrike).StrikeModifiers.CalculatedTrueDamageFormula?.ExpectedValueMinimumOne * 2 ?? int.MinValue)
                                .WithEffectOnChosenTargets((async (fighter, targets) => {
                                    var map = fighter.Actions.AttackedThisManyTimesThisTurn;
                                    var enemy = targets.ChosenCreature!;
                                    var qPenalty = new QEffect("Double Slice penalty", QEffect.NoDescription, ExpirationCondition.Never, fighter, IllustrationName.None) {
                                        BonusToAttackRolls = (_, ca, de) => {
                                            if (!ca.HasTrait(Trait.Agile)) {
                                                return new Bonus(-2, BonusType.Untyped, "Double Slice penalty");
                                            }

                                            return null;
                                        }
                                    };

                                    int hits = 0;
                                    
                                    if (fighter.HeldItems.Count >= 1)
                                        hits += await fighter.MakeStrike(enemy, fighter.HeldItems[0], map) >= CheckResult.Success ? 1 : 0;
                                    fighter.AddQEffect(qPenalty);
                                    if (fighter.HeldItems.Count >= 2) {
                                        hits += await fighter.MakeStrike(enemy, fighter.HeldItems[1], map) >= CheckResult.Success ? 1 : 0; ;
                                    }
                                    fighter.RemoveAllQEffects(qfr => qfr == qPenalty);
                                    if (hits >= 2)
                                        fighter.AddQEffect(new QEffect() { Key = "flensing slice", Source = enemy }.WithExpirationAtEndOfOwnerTurn());
                                })).WithTargetingTooltip((power, target, index) => power.Description))
                            .WithPossibilityGroup(Constants.POSSIBILITY_GROUP_FIGHTER_POWERS);
                    }

                    return null;
                }
            })
            .Builder
            .AddMainAction(you => {
                return new CombatAction(you, new SideBySideIllustration(IllustrationName.Swords, IllustrationName.BloodVendetta), "Flensing Slice", [Trait.Melee], "...", Target.ReachWithAnyWeapon()
                    .WithAdditionalConditionOnTargetCreature((a, d) => a.QEffects.Any(qf => qf.Key == "flensing slice" && qf.Source == d) ? Usability.Usable : Usability.NotUsableOnThisCreature("can only be used on creatures hit by both attacks from double slice this turn")))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.SwordStrike)
                .WithGoodnessAgainstEnemy((target, attacker, defender) => {
                    return Math.Max(attacker.HeldItems[0]?.WeaponProperties?.DamageDieCount ?? 1, attacker.HeldItems[1]?.WeaponProperties?.DamageDieCount ?? 1) * 6.5f + (defender.QEffects.FirstOrDefault(qf => qf.Name == "Flat-footed") != null ? 2 : 6.5f);
                })
                .WithEffectOnEachTarget(async (action, user, defender, result) => {
                    defender.AddQEffect(QEffect.FlatFooted("Flensing slice").WithExpirationAtStartOfSourcesTurn(user, 1));
                    defender.AddQEffect(QEffect.PersistentDamage($"{Math.Max(user.HeldItems[0]?.WeaponProperties?.DamageDieCount ?? 1, user.HeldItems[1]?.WeaponProperties?.DamageDieCount ?? 1)}d8", DamageKind.Bleed));
                })
                ;
            })
            .AddMainAction(you => {
                return new CombatAction(you, new SideBySideIllustration(IllustrationName.Swords, Illustrations.Parry), "Twin Parry", [], "...", Target.Self((user, ai) => ai.GainBonusToAC(1))
                    .WithAdditionalRestriction(user => user.HeldItems.Where(item => item.WeaponProperties != null).Count() >= 2 ? null : "you must be dual wielding melee weapons"))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.RaiseShield)
                .WithEffectOnSelf(async (action, user) => {
                    user.AddQEffect(new QEffect("Twin Parry", "You have a +1 circumstance bonus to AC.", ExpirationCondition.ExpiresAtStartOfYourTurn, you, Illustrations.Parry) {
                        Id = QEffectIds.Parry,
                        BonusToDefenses = delegate (QEffect parrying, CombatAction? bonk, Defense defense) {
                            if (defense == Defense.AC) {
                                return new Bonus(1, BonusType.Circumstance, "parry");
                            } else return null;
                        },
                        Tag = (user.HeldItems[0], user.HeldItems[1]),
                        StateCheck = (qf) => {
                            if (!qf.Owner.HeldItems.Contains(((ValueTuple<Item, Item>)qf.Tag).Item1) || !qf.Owner.HeldItems.Contains(((ValueTuple<Item, Item>)qf.Tag).Item2)) {
                                qf.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        }
                    });
                })
                ;
            })
            .Done()
            ;
        }
    }
}