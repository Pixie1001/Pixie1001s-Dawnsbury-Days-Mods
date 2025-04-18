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
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowFighter {
        public static Creature Create() {
            int poisonDC = 17;
            return new Creature(Illustrations.DrowFighter, "Drow Fighter", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid, ModTraits.MeleeMutator }, 1, 5, 6, new Defenses(15, 4, 10, 7), 18,
            new Abilities(2, 4, 2, 0, 1, 0), new Skills(acrobatics: 7, athletics: 5, stealth: 7, intimidation: 5))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;
                    // Check if has crossbow
                    Item? handcrossbow = creature.HeldItems.FirstOrDefault(item => item.ItemName == ItemName.HandCrossbow);

                    if (handcrossbow == null) {
                        return null;
                    }

                    // Check if crossbow is loaded
                    if (handcrossbow.EphemeralItemProperties.NeedsReload) {
                        // foreach (Option option in options.Where(opt => opt.AiUsefulness.ObjectiveAction != null && opt.AiUsefulness.ObjectiveAction.Action.Name.StartsWith("Reload") || opt.Text == "Reload")) {
                        foreach (Option option in options.Where(opt => opt.Text == "Reload" || (opt.AiUsefulness.ObjectiveAction != null && opt.AiUsefulness.ObjectiveAction.Action.Name == "Reload"))) {
                            option.AiUsefulness.MainActionUsefulness = 1f;
                        }
                    }

                    //if (creature.Actions.ActionsLeft != 1) {
                    //    foreach (Option option in options.Where(opt => opt.Text == "Reload")) {
                    //        option.AiUsefulness.MainActionUsefulness = int.MinValue;
                    //    }
                    //}

                    return null;
                };
            })
            .WithProficiency(Trait.Unarmed, Proficiency.Trained)
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(QEffect.AttackOfOpportunity(false))
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .AddHeldItem(Items.CreateNew(ItemName.Rapier))
            .AddHeldItem(Items.CreateNew(ItemName.HandCrossbow))
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    Item? rapier = self.Owner.HeldItems.FirstOrDefault(item => item.ItemName == ItemName.Rapier);
                    if (rapier == null) {
                        return null;
                    }

                    StrikeModifiers strikeModifiers = new StrikeModifiers() {
                        AdditionalBonusesToAttackRoll = new List<Bonus>() { new Bonus(1, BonusType.Circumstance, "Skewer") },
                        //OnEachTarget = async (a, d, result) => {
                        //    if (result >= CheckResult.Success) {
                        //        d.AddQEffect(QEffect.PersistentDamage("1d6", DamageKind.Bleed));
                        //    }
                        //}
                    };
                    CombatAction action = self.Owner.CreateStrike(rapier, -1, strikeModifiers);
                    action.ActionCost = 2;
                    action.Name = "Skewer";
                    action.Description = StrikeRules.CreateBasicStrikeDescription2(action.StrikeModifiers, additionalSuccessText: "If you dealt damage, inflict 1d6 persistent bleed damage.");
                    action.ShortDescription += " and inflict 1d6 persistent bleed damage.";
                    action.Illustration = new SideBySideIllustration(action.Illustration, IllustrationName.PersistentBleed);
                    action.WithGoodnessAgainstEnemy((target, attacker, defender) => {
                        return defender.QEffects.FirstOrDefault(qf => qf.Name.Contains(" persistent " + DamageKind.Bleed.ToString().ToLower() + " damage")) != null ? (8 + attacker.Abilities.Strength) * 1.1f : (4.5f + attacker.Abilities.Strength) * 1.1f;
                    });

                    return (ActionPossibility)action;
                },
                AfterYouDealDamage = async (a, action, d) => {
                    if (action.Name == "Skewer") {
                        d.AddQEffect(QEffect.PersistentDamage("1d6", DamageKind.Bleed));
                    }
                }
            })
            .AddQEffect(new QEffect("Lethargy Poison", "Enemies damaged by the drow fighter's hand crossbow attack, are afflicted by lethargy poison. {b}Stage 1{/b} slowed 1; {b}Stage 2{/b} slowed 1 for rest of encounter") {
                StartOfCombat = async self => {
                    self.Name += $" (DC {poisonDC + self.Owner.Level})";
                },
                AfterYouDealDamage = async (attacker, action, target) => {
                    if (action.Item != null && action.Item.ItemName == ItemName.HandCrossbow) {
                        Affliction poison = new Affliction(QEffectIds.LethargyPoison, "Lethargy Poison", attacker.Level + poisonDC, "{b}Stage 1{/b} slowed 1; {b}Stage 2{/b} slowed 1 for rest of encounter", 2, dmg => null, qf => {
                            if (qf.Value == 1) {
                                qf.Owner.AddQEffect(QEffect.Slowed(1).WithExpirationEphemeral());
                            }

                            if (qf.Value == 2) {
                                QEffect nEffect = QEffect.Slowed(1).WithExpirationNever();
                                nEffect.CounteractLevel = qf.CounteractLevel;
                                qf.Owner.AddQEffect(nEffect);
                                qf.Owner.RemoveAllQEffects(qf2 => qf2.Id == QEffectIds.LethargyPoison);
                                qf.Owner.Occupies.Overhead("*lethargy poison converted to slowed 1*", Color.Black);
                            }
                        });

                        await Affliction.ExposeToInjury(poison, attacker, target);
                    }
                },
                AdditionalGoodness = (self, action, target) => {
                    Item? handcrossbow = self.Owner.HeldItems.FirstOrDefault(item => item.ItemName == ItemName.HandCrossbow);
                    int dc = poisonDC + self.Owner.Level;

                    if (handcrossbow == null) {
                        return 0f;
                    }

                    if (action == null || action.Item != handcrossbow) {
                        return 0f;
                    }

                    //if (self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(self.Owner.OwningFaction) && cr.Threatens(self.Owner.Occupies)).ToArray().Length > 0) {
                    //    return 0f;
                    //}

                    if (target != null && !target.HasEffect(QEffectIds.LethargyPoison) && !target.HasEffect(QEffectId.Slowed)) {
                        float start = self.Owner.Battle.RoundNumber <= 1 ? target.Level * 4f : target.Level * 2f;
                        float percentage = dc - (target.Defenses.GetBaseValue(Defense.Fortitude) + 10.5f);
                        percentage *= 5f;
                        percentage += 50f;
                        start = start / 100 * percentage;
                        return start;
                    }

                    return 0f;
                }
            });
        }
    }
}