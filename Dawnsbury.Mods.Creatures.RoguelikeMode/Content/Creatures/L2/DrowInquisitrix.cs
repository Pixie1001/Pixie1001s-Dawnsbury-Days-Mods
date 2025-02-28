using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using System.Threading;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowInquisitrix {
        public static Creature Create() {
            Creature monster = new Creature(Illustrations.DrowInquisitrix, "Drow Inquisitrix", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid, Trait.Female }, 2, 8, 6, new Defenses(17, 5, 8, 11), 25,
                new Abilities(2, 4, 1, 2, 2, 4), new Skills(acrobatics: 8, intimidation: 11, religion: 7))
                .WithAIModification(ai => {
                    ai.OverrideDecision = (self, options) => {
                        Creature creature = self.Self;
                        foreach (Option opt in options.Where(o => o.OptionKind == OptionKind.NonSpecified && o.Text == "Harm")) {
                            if (opt is ChooseNumberOfActionThenActionOption) {
                                var opt2 = (ChooseNumberOfActionThenActionOption)opt;
                                if (opt2.NumberOfActions == 3) {
                                    opt.AiUsefulness.MainActionUsefulness = int.MinValue;
                                }
                            }
                        }
                        return null;
                    };
                })
                .WithProficiency(Trait.Martial, Proficiency.Expert)
                .WithProficiency(Trait.Spell, Proficiency.Trained)
                .WithBasicCharacteristics()
                .AddHeldItem(Items.CreateNew(CustomItems.ScourgeOfFangs))
                .AddQEffect(CommonQEffects.Drow())
                .AddQEffect(CommonQEffects.DrowClergy())
                .AddQEffect(QEffect.SneakAttack("1d4"))
                .AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        if (self.Owner.Spellcasting.PrimarySpellcastingSource.Spells.FirstOrDefault(spell => spell.SpellId == SpellId.Harm) == null) {
                            return null;
                        }

                        Item weapon = self.Owner.PrimaryWeapon;

                        StrikeModifiers strikeModifiers = new StrikeModifiers() {
                            OnEachTarget = async (a, d, result) => {
                                //if (result >= CheckResult.Success) {
                                //    await CommonSpellEffects.DealDirectDamage(a.Spellcasting.PrimarySpellcastingSource.Spells.First(spell => spell.SpellId == SpellId.Harm), DiceFormula.FromText("1d8"), d, result, DamageKind.Negative);
                                //}
                                a.Spellcasting.PrimarySpellcastingSource.Spells.RemoveFirst(spell => spell.SpellId == SpellId.Harm);
                            }
                        };

                        if (weapon == null) {
                            return null;
                        }

                        CombatAction action = self.Owner.CreateStrike(weapon, -1, strikeModifiers);
                        action.Traits.Add(Trait.Divine);
                        action.Traits.Add(Trait.Spell);
                        action.SpellLevel = 1;
                        action.ActionCost = 2;
                        action.Name = $"Channel Smite ({weapon.Name})";
                        action.Description = "You siphon the destructive energies of positive or negative energy through a melee attack and into your foe. Make a melee Strike and add the spell’s damage to the Strike’s damage. This is negative damage if you expended a harm spell or positive damage if you expended a heal spell. The spell is expended with no effect if your Strike fails or hits a creature that isn’t damaged by that energy type (such as if you hit a non-undead creature with a heal spell).";
                        action.ShortDescription += " and expends a casting of harm to inflict 1d8 negative damage.";
                        action.Illustration = new SideBySideIllustration(action.Illustration, IllustrationName.Harm);
                        action.WithGoodnessAgainstEnemy((target, attacker, defender) => {
                            //float additionalDamage1 = action.Item?.WeaponProperties?.AdditionalDamageFormula != null ? DiceFormula.FromText(action.Item.WeaponProperties.AdditionalDamageFormula).ExpectedValue : 0;
                            //float additionalDamage2 = action.Item?.WeaponProperties?.AdditionalDamageFormula2 != null ? DiceFormula.FromText(action.Item.WeaponProperties.AdditionalDamageFormula2).ExpectedValue : 0;
                            float bonusDmg = 0f;
                            if (action.Item?.WeaponProperties != null) {
                                foreach (var dmgSource in action.Item.WeaponProperties.AdditionalDamage) {
                                    bonusDmg += DiceFormula.FromText(dmgSource.Item1).ExpectedValue;
                                }
                            }
                            return defender.HasTrait(Trait.Undead) ? -100f : 4.5f + action.TrueDamageFormula.ExpectedValue + bonusDmg;
                        });

                        return (ActionPossibility)action;
                    },
                    AddExtraKindedDamageOnStrike = (action, d) => {
                        if (action == null || !action.Name.StartsWith("Channel Smite (")) {
                            return null;
                        }
                        return new KindedDamage(DiceFormula.FromText("1d8", "Harm"), DamageKind.Negative);
                    }
                })
                .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Cleric, Ability.Charisma, Trait.Divine).WithSpells(
                    new SpellId[] { SpellId.Harm, SpellId.Harm, SpellId.Harm }).Done();

            string icDmg = "1d4";

            monster.AddQEffect(new QEffect("Iron Command {icon:Reaction}", "ADDED LATER") {
                StartOfCombat = async self => {
                    icDmg = $"1d4+{self.Owner.Level}";
                    self.Description = "{b}Trigger{/b} An enemy within 15 feet damages you. {b}Effect{/b} Your attacker must choose either to fall prone or suffer " + icDmg +
                    " mental damage. You then deal +1d6 evil or negative damage against them with your strikes, until a new enemy earns your ire.";
                },
                AfterYouTakeDamage = async (self, amount, kind, action, critical) => {
                    if (action == null || action.Owner == null || action.Owner == action.Owner.Battle.Pseudocreature) {
                        return;
                    }

                    if (action.Owner.OwningFaction == self.Owner.OwningFaction) {
                        return;
                    }

                    if (await self.Owner.AskToUseReaction($"{action.Owner.Name} dares to strike you! Do you wish to use your iron command to demand they kneel at your feet in supplication?")) {
                        if (await action.Owner.Battle.AskForConfirmation(action.Owner, self.Owner.Illustration,
                        $"{self.Owner.Name} uses Iron Command, urging you to kneel before your betters. Do you wish to drop prone in supplication, or refuse and suffer " + icDmg +
                        " mental damage?", "Submit", "Defy")) {
                            action.Owner.AddQEffect(QEffect.Prone());
                        } else {
                            // TODO: Make a dummy action for this damage
                            CombatAction dummyAction = new CombatAction(self.Owner, self.Owner.Illustration, "Iron Command", new Trait[] { Trait.Divine, Trait.Emotion, Trait.Enchantment, Trait.Mental },
                            "You deal " + icDmg + " mental damage to a creature that attacked you, and refuses to kneel.", Target.Uncastable());
                            await CommonSpellEffects.DealDirectDamage(dummyAction, DiceFormula.FromText(icDmg, "Iron Command"), action.Owner, CheckResult.Success, DamageKind.Mental);
                        }

                        DamageKind type = DamageKind.Evil;
                        if (!action.Owner.HasTrait(Trait.Good) && !action.Owner.HasTrait(Trait.Undead)) {
                            type = DamageKind.Negative;
                        }

                        self.Owner.RemoveAllQEffects(qf => qf.Name == "Inquisitrix Mandate" && qf.Source == self.Owner);

                        self.Owner.AddQEffect(new QEffect("Inquisitrix Mandate", $"You deal +1d6 {type.HumanizeTitleCase2()} damage against {action.Owner.Name} for daring to strike against you.") {
                            Source = self.Owner,
                            Illustration = IllustrationName.BestowCurse,
                            AddExtraKindedDamageOnStrike = (strike, target) => {
                                if (strike.HasTrait(Trait.Strike) && target == action.Owner) {
                                    return new KindedDamage(DiceFormula.FromText("1d6", "Inquisitrix Mandate"), type);
                                }
                                return null;
                            },
                            AdditionalGoodness = (self, action, target) => {
                                if (action.HasTrait(Trait.Strike) && target == action.Owner) {
                                    return 3.5f;
                                }
                                return 0f;
                            },
                            ExpiresAt = ExpirationCondition.Never
                        });
                    }
                }
            });

            return monster;
        }
    }
}