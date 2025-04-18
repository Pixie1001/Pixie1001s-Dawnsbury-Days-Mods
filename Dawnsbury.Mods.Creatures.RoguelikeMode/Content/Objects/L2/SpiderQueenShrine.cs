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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class SpiderQueenShrine {
        public static Creature Create() {
            int radius = 2;
            QEffect qfCurrentDC = new QEffect() { Value = 15 };

            Creature hazard = new Creature(Illustrations.SpiderShrine, "Spider Queen Shrine", new List<Trait>() { Trait.Object }, 2, 0, 0, new Defenses(10, 10, 0, 0), 20, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
            .WithTactics(Tactic.DoNothing)
            .WithEntersInitiativeOrder(false)
            .AddQEffect(CommonQEffects.Hazard())
            .AddQEffect(QEffect.ArmorBreak(2))
            .AddQEffect(qfCurrentDC)
            .WithHardness(7)
            ;

            var animation = hazard.AnimationData.AddAuraAnimation(IllustrationName.BaneCircle, radius);
            animation.Color = Color.Black;

            QEffect effect = new QEffect("Blessings of the Spider Queen", $"All spiders, demons and drow within {radius * 5}-feet of this shrine gain a +1 bonus to AC, saves and attacks rolls.");

            QEffect interactable = new QEffect("Interactable", "You can use Religion, Thievery and Crafting to interact with this shrine.") {
                StateCheckWithVisibleChanges = async self => {
                    // Add contextual actions
                    foreach (Creature hero in self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsPlayer && cr.IsAdjacentTo(self.Owner))) {
                        hero.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                            ProvideContextualAction = qfContextActions => {
                                return new SubmenuPossibility(Illustrations.SpiderShrine, "Interactions") {
                                    Subsections = {
                                                new PossibilitySection(hazard.Name) {
                                                    Possibilities = {
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, IllustrationName.Consecrate, "Consecrate", new Trait[] { Trait.Manipulate, Trait.Basic, Trait.Divine },
                                                        "Make a Religion check against DC " + qfCurrentDC.Value + "." + S.FourDegreesOfSuccess(null,
                                                        "The shrine's will be disabled.", null, null),
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(2)
                                                        .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Religion), Checks.FlatDC(qfCurrentDC.Value)))
                                                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                            if (result >= CheckResult.Success) {
                                                                target.RemoveAllQEffects(qf => qf.Name == "Blessings of the Spider Queen");
                                                                animation.MaximumOpacity = 0;
                                                            }
                                                        }),
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, Illustrations.SpiderShrine, "Recall Knowledge", new Trait[] { Trait.Manipulate, Trait.Basic },
                                                        "Make a Religion or Crafting check against DC " + qfCurrentDC.Value + "." + S.FourDegreesOfSuccess("Reduce all DCs on this hazard by 3.",
                                                        "Reduce all DCs on this hazard by 2.", null, "You take 1d6 evil damage and increase all DCs on this hazard by 1."),
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(1)
                                                        .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Religion, Skill.Crafting), Checks.FlatDC(qfCurrentDC.Value - 2)))
                                                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                            if (result == CheckResult.CriticalFailure) {
                                                                await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Spider Queen's Wrath"), caster, CheckResult.Success, DamageKind.Evil);
                                                                qfCurrentDC.Value += 1;
                                                            }
                                                            if (result == CheckResult.Success) {
                                                                qfCurrentDC.Value -= 2;
                                                            }
                                                            if (result == CheckResult.CriticalSuccess) {
                                                                qfCurrentDC.Value -= 3;
                                                            }
                                                        }),
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, Illustrations.SpiderShrine, "Disrupt Armour", new Trait[] { Trait.Manipulate, Trait.Basic },
                                                            "Make a Thievery check against DC " + qfCurrentDC.Value + "." + S.FourDegreesOfSuccess("Reduce the shrine's hardness by 6.", "Reduce the shrine's hardness by 3.",  null, "You take 1d6 evil damage."),
                                                            Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner))
                                                            )
                                                        .WithActionCost(1)
                                                        .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Thievery), Checks.FlatDC(qfCurrentDC.Value)))
                                                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                            if (result == CheckResult.CriticalFailure) {
                                                                await CommonSpellEffects.DealDirectDamage(spell, DiceFormula.FromText("1d6", "Spider Queen's Wrath"), caster, CheckResult.Success, DamageKind.Evil);
                                                            }
                                                            if (result < CheckResult.Success)
                                                                return;
                                                            target.WeaknessAndResistance.Hardness -= result == CheckResult.CriticalSuccess ? 6 : 3;
                                                            if (target.WeaknessAndResistance.Hardness > 0)
                                                                return;
                                                            target.WeaknessAndResistance.Hardness = 0;
                                                        }),
                                                    }

                                                }
                                            }
                                };
                            }
                        });
                    }
                }
            };

            effect.AddGrantingOfTechnical(cr => (cr.HasTrait(ModTraits.Spider) || cr.HasTrait(ModTraits.Drow) || cr.HasTrait(Trait.Demon)) && cr.DistanceTo(effect.Owner) <= radius, qfTechnical => {
                qfTechnical.Name = "Blessings of the Spider Queen";
                qfTechnical.Description = "+1 bonus to AC, saves and attacks rolls.";
                qfTechnical.Illustration = new SameSizeDualIllustration(Illustrations.StatusBackdrop, Illustrations.SpiderShrine);
                qfTechnical.Innate = false;
                qfTechnical.CountsAsABuff = true;
                qfTechnical.Key = "Blessings of the Spider Queen";
                qfTechnical.BonusToAttackRolls = (self, action, target) => {
                    return new Bonus(1, BonusType.Untyped, "Spider Queen Shrine");
                };
                qfTechnical.BonusToDefenses = (self, action, defence) => {
                    return new Bonus(1, BonusType.Untyped, "Spider Queen Shrine");
                };
            });

            effect.AddGrantingOfTechnical(cr => cr.HasTrait(ModTraits.Spider) || cr.HasTrait(ModTraits.Drow) || cr.HasTrait(Trait.Demon), qfTechnical => {
                qfTechnical.Key = "Blessings of the Spider Queen (Goodness)";
                qfTechnical.AdditionalGoodness = (self, action, target) => {
                    if (self.Owner.DistanceTo(effect.Owner) <= radius) {
                        return 2;
                    }
                    return 0f;
                };
            });

            hazard.AddQEffect(effect);
            hazard.AddQEffect(interactable);

            return hazard;
        }
    }
}