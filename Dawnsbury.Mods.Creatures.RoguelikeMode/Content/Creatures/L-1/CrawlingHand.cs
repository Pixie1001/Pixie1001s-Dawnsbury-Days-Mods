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
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class CrawlingHand
    {
        public static Creature Create()
        {
            return new Creature(IllustrationName.UnknownCreature, "Crawling Hand", [Trait.Evil, Trait.Undead], -1, 5, 6, new Defenses(12, 2, 5, 2), 8, new Abilities(1, 3, 0, -4, 0, 0), new Skills(athletics: 5, stealth: 6, survival: 2))
                .WithBasicCharacteristics() // lol Crawling Hand can understand Common
                .AddQEffect(QEffect.TraitImmunity(Trait.Death))
                .AddQEffect(QEffect.TraitImmunity(Trait.Disease))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Paralyzed))
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Poison))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Unconscious))
                .AddQEffect(QEffect.TraitImmunity(Trait.Visual))
                .AddQEffect(QEffect.MonsterGrab())
                .AddQEffect(new QEffect("Mark Quarry", "After damaging a creature, this gains +1 circumstance bonus to damage rolls when Striking that creature.")
                {
                    AfterYouDealDamage = async (attacker, action, defender) =>
                    {
                        QEffect MarkQuarry()
                        {
                            QEffect quarry = attacker.FindQEffect(QEffectIds.MarkQuarry) ?? new QEffect()
                            {
                                Id = QEffectIds.MarkQuarry,
                                Name = "Quarry",
                                Illustration = IllustrationName.HuntPrey,
                                BonusToDamage = (qfBonusToDamage, strike, target) =>
                                {
                                    if (strike.HasTrait(Trait.Strike) && qfBonusToDamage.Tag is Creature quarry && quarry == target)
                                    {
                                        return new Bonus(1, BonusType.Circumstance, "Mark Quarry", true);
                                    }

                                    return null;
                                }
                            };
                            quarry.Tag = defender;
                            quarry.Description = $"Gain +1 circumstance to damage against {defender.Name}";
                            return quarry;
                        }

                        attacker.AddQEffect(MarkQuarry());
                    }
                })
                .AddQEffect(new QEffect("Grip Throat ", "A creature you grab or restrain must spend an extra action to cast spells or use actions with the verbal trait.")
                {
                    StateCheck = async (qfStateCheck) =>
                    {
                        Creature self = qfStateCheck.Owner;
                        foreach (Creature grabbed in self.Battle.AllCreatures.Where(creature => creature.QEffects.Any(qe => (qe.Id == QEffectId.Grappled || qe.Id == QEffectId.Grabbed || qe.Id == QEffectId.Restrained) && qe.Source != null && qe.Source == self)))
                        {
                            grabbed.AddQEffect(new QEffect("Gripped Throat ", "You must spend an extra action to cast spells or use actions with the verbal trait.")
                            {
                                ExpiresAt = ExpirationCondition.Ephemeral,
                                Illustration = IllustrationName.Grabbed,
                                PreventTakingAction = (action) =>
                                {
                                    if (!action.Owner.HasEffect(QEffectIds.LoosenedGrip) && ((action.HasTrait(Trait.Spell) && !action.HasTrait(Trait.SustainASpell)) || action.HasTrait(Trait.VerbalOnly)))
                                    {
                                        return "you must spend an action to 'Loosen Grip' to use this action";
                                    }

                                    return null;
                                },
                                AfterYouTakeAction = async (qfAfterAction, action) =>
                                {
                                    if (action.Owner.HasEffect(QEffectIds.LoosenedGrip) && ((action.HasTrait(Trait.Spell) && !action.HasTrait(Trait.SustainASpell)) || action.HasTrait(Trait.VerbalOnly)))
                                    {
                                        action.Owner.RemoveAllQEffects(qe => qe.Id == QEffectIds.LoosenedGrip);
                                    }
                                },
                                ProvideActionIntoPossibilitySection = (qfLoosenGrip, section) =>
                                {
                                    if (section.PossibilitySectionId == PossibilitySectionId.ContextualActions)
                                    {
                                        Creature self = qfLoosenGrip.Owner;
                                        return new ActionPossibility(new CombatAction(self, IllustrationName.Escape, "Loosen Grip", [], "Spend an action to loosen the grip around your troat to allow you to cast a spell or use an action with the verbal trait.", Target.Self()
                                                .WithAdditionalRestriction((innerSelf) =>
                                                {
                                                    if (innerSelf.HasEffect(QEffectIds.LoosenedGrip))
                                                    {
                                                        return "already loosened grip";
                                                    }

                                                    return null;
                                                }))
                                                .WithActionCost(1)
                                                .WithEffectOnSelf(async (innerSelf) =>
                                                {
                                                    innerSelf.AddQEffect(new QEffect("Loosened Grip", "You spent an action to loosen the grip around your troat.")
                                                    {
                                                        ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
                                                        Id = QEffectIds.LoosenedGrip,
                                                        Illustration = IllustrationName.Escape
                                                    });
                                                }));
                                    }

                                    return null;
                                }
                            });
                        }
                    }
                })
                .Builder
                .AddNaturalWeapon(NaturalWeaponKind.Claw, 7, [Trait.Agile, Trait.Finesse, Trait.Grab], "1d4+1", DamageKind.Slashing)
                .Done();
        }
    }
}