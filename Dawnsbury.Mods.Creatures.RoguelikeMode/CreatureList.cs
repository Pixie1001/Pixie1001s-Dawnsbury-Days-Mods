using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Description;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Dawnsbury.ThirdParty.SteamApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Intrinsics.Arm;
using System.Xml;
using Dawnsbury.Core.Mechanics.Damage;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;
using System.Text;
using static Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.BarbarianFeatsDb.AnimalInstinctFeat;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Metrics;
using Microsoft.Xna.Framework.Audio;
using static System.Reflection.Metadata.BlobBuilder;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Animations.Movement;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;
using Dawnsbury.Campaign.Encounters.A_Crisis_in_Dawnsbury;
using System.Buffers;
using System.Xml.Schema;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class CreatureList {
        internal static Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>> Creatures = new Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>>();

        internal static void LoadCreatures() {
            // TODO: Setup to teleport to random spot and be hidden at start of combat, so logic can be removed from encounter.

            // CREATURE - Unseen Guardian
            Creatures.Add(ModEnums.CreatureId.UNSEEN_GUARDIAN,
                encounter => new Creature(IllustrationName.ElectricityMephit256, "Unseen Guardian", new List<Trait>() { Trait.Lawful, Trait.Elemental, Trait.Air }, 2, 6, 8, new Defenses(16, 5, 11, 7), 30, new Abilities(2, 3, 3, 1, 3, 1), new Skills(stealth: 6))
                .WithAIModification(ai => {
                    ai.IsDemonHorizonwalker = true;
                    ai.OverrideDecision = (self, options) => {
                        Creature creature = self.Self;

                        return creature.Actions.ActionsLeft == 1 && creature.Battle.AllCreatures.All<Creature>((Func<Creature, bool>)(enemy => !enemy.EnemyOf(creature) || creature.DetectionStatus.EnemiesYouAreHiddenFrom.Contains<Creature>(enemy))) && !creature.DetectionStatus.Undetected ? options.Where<Option>((Func<Option, bool>)(opt => opt.OptionKind == OptionKind.MoveHere && opt.Text == "Sneak" && opt is TileOption)).ToList<Option>().GetRandom<Option>() : (Option)null;
                    };
                })
                .WithProficiency(Trait.Weapon, Proficiency.Trained)
                .AddQEffect(new QEffect("Obliviating Aura", "The unseen guardian feels slippery and elusive in its victim's minds, making it easy for them to lose track of it's postion. It gains a +20 bonus to checks made to sneak or hide and can hide in plain sight.") {
                    Id = QEffectId.HideInPlainSight,
                    Innate = true,
                    Illustration = IllustrationName.Blur,
                    BonusToSkillChecks = (skill, action, target) => {
                        if (action.Name == "Sneak" || action.Name == "Hide") {
                            return new Bonus(20, BonusType.Status, "Indistinct Form");
                        }
                        return null;
                    },
                    StartOfCombat = async self => {
                        List<Creature> party = self.Owner.Battle.AllCreatures.Where(c => c.OwningFaction.IsHumanControlled).ToList();

                        foreach (Creature player in party) {
                            self.Owner.DetectionStatus.HiddenTo.Add(player);
                        }
                        self.Owner.DetectionStatus.Undetected = true;
                        List<Tile> spawnPoints = self.Owner.Battle.Encounter.Map.AllTiles.Where(t =>
                        {
                            if (!t.IsFree) {
                                return false;
                            }

                            foreach (Creature pc in self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsHumanControlled)) {
                                if (pc.DistanceTo(t) < 4) {
                                    return false;
                                }
                            }
                            return true;
                        }).ToList();

                        Tile location = spawnPoints[R.Next(0, spawnPoints.Count)];
                        self.Owner.Occupies = location;
                        if (!location.IsTrulyGenuinelyFreeTo(self.Owner)) {
                            location = location.GetShuntoffTile(self.Owner);
                        }
                        self.Owner.TranslateTo(location);
                    }
                })
                .AddQEffect(new QEffect("Seek Vulnerability", "The Unseen Guardian's obliviating aura quickly falls apart as soon as a creatre's attention begins to settle on it, distrupting the magic. Successful seek attempts against a detected Unseen Guardian instead fully reveal it to all of the seeker's allies.") {
                    Innate = true,
                    AfterYouAreTargeted = async (self, action) => {
                        action.ChosenTargets.CheckResults.TryGetValue(self.Owner, out var result);
                        //if (action.ActionId == ActionId.Seek && result == CheckResult.Success) {
                        //    self.Owner.DetectionStatus.HiddenTo.Remove(action.Owner);
                        //    self.Owner.DetectionStatus.RecalculateIsHiddenToAnEnemy();
                        //} else if (action.ActionId == ActionId.Seek && result == CheckResult.CriticalSuccess) {
                        //    self.Owner.DetectionStatus.HiddenTo.Clear();
                        //    self.Owner.DetectionStatus.RecalculateIsHiddenToAnEnemy();
                        //}
                        if (action.ActionId == ActionId.Seek && result >= CheckResult.Success && !self.Owner.DetectionStatus.EnemiesYouAreHiddenFrom.Contains(action.Owner)) {
                            self.Owner.DetectionStatus.HiddenTo.Clear();
                            self.Owner.DetectionStatus.RecalculateIsHiddenToAnEnemy();
                        }
                    }
                })
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Bleed))
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Poison))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Paralyzed))
                .AddQEffect(QEffect.Flying())
                .AddQEffect(QEffect.SneakAttack("1d8"))
                .WithUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Fist, "Fists", "2d4", DamageKind.Bludgeoning, new Trait[] { Trait.Unarmed, Trait.Magical, Trait.Finesse, Trait.Melee, Trait.Agile }))
                .WithAdditionalUnarmedStrike(new Item(IllustrationName.FourWinds, "Slicing Wind", new Trait[] { Trait.Ranged, Trait.Electricity, Trait.Magical }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing) {
                    VfxStyle = new VfxStyle(5, ProjectileKind.Cone, IllustrationName.FourWinds),
                    Sfx = SfxName.AeroBlade
                }.WithRangeIncrement(4)))
            );

            ModManager.RegisterNewCreature("Unseen Guardian", Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN]);


            // CREATURE - Drow Assassin
            Creatures.Add(ModEnums.CreatureId.DROW_ASSASSIN,
                encounter => new Creature(IllustrationName.Shadow, "Drow Assassin", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid }, 1, 7, 6, new Defenses(18, 4, 10, 7), 18, new Abilities(-1, 4, 1, 2, 2, 1), new Skills(stealth: 10, acrobatics: 7))
                .WithAIModification(ai => {
                    ai.OverrideDecision = (self, options) => {

                        Creature creature = self.Self;

                        if (creature.HasEffect(QEffectIds.Lurking)) {
                            Creature stalkTarget = creature.Battle.AllCreatures.FirstOrDefault(c => c.QEffects.FirstOrDefault(qf => qf.Id == QEffectIds.Stalked && qf.Source == creature) != null);
                            foreach (Option option in options.Where(o => o.OptionKind == OptionKind.MoveHere && o.AiUsefulness.ObjectiveAction != null && o.AiUsefulness.ObjectiveAction.Action.ActionId == ActionId.Sneak)) {
                                TileOption? option2 = option as TileOption;
                                if (option2 != null && option2.Tile.DistanceTo(stalkTarget.Occupies) <= 5 && option2.Tile.HasLineOfEffectTo(stalkTarget.Occupies) <= CoverKind.Standard) {
                                    option2.AiUsefulness.MainActionUsefulness += 10;
                                }
                                //option2.AiUsefulness.MainActionUsefulness += 20 - option2.Tile.DistanceTo(stalkTarget.Occupies);
                            }
                            foreach (Option option in options.Where(o => o.OptionKind != OptionKind.MoveHere)) {
                                if (creature.Occupies.DistanceTo(stalkTarget.Occupies) <= 5 && creature.HasLineOfEffectTo(stalkTarget.Occupies) <= CoverKind.Standard) {
                                    option.AiUsefulness.MainActionUsefulness += 15;
                                }

                                //option2.AiUsefulness.MainActionUsefulness += 20 - option2.Tile.DistanceTo(stalkTarget.Occupies);
                            }
                        }

                        return null;
                    };
                })
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .AddQEffect(new QEffect() {
                    Id = QEffectId.SwiftSneak
                })
                .AddQEffect(new QEffect("Shadowsilk Cloak", "Target can always attempt to sneak or hide, even when unobstructed.") {
                    Id = QEffectId.HideInPlainSight,
                    Innate = true,
                    StartOfCombat = async self => {
                        List<Creature> party = self.Owner.Battle.AllCreatures.Where(c => c.OwningFaction.IsHumanControlled).ToList();
                        Creature target = party.OrderBy(c => c.HP / 100 * (c.Defenses.GetBaseValue(Defense.AC) * 5)).ToList()[0];

                        self.Owner.AddQEffect(new QEffect {
                            Id = QEffectIds.Lurking,
                            Value = 2,
                            PreventTakingAction = action => action.ActionId != ActionId.Sneak ? "Stalking prey, cannot act." : null,
                            StateCheck = self => {
                                if (!self.Owner.DetectionStatus.Undetected) {
                                    QEffect startled = QEffect.Stunned(2);
                                    startled.Illustration = IllustrationName.Dazzled;
                                    startled.Name = "Startled";
                                    startled.Description = "The assassin is startled by their premature discovery.\nAt the beginning of their next turn, they will lose 2 actions.\n\nThey can't take reactions.";
                                    self.Owner.Occupies.Overhead("*startled!*", Color.Black);
                                    self.Owner.AddQEffect(startled);
                                    Sfxs.Play(SfxName.DazzlingFlash);
                                    self.ExpiresAt = ExpirationCondition.Immediately;
                                }
                            },
                            BonusToSkillChecks = (skill, action, target) => {
                                if (skill == Skill.Stealth && action.Name == "Sneak") {
                                    return new Bonus(7, BonusType.Status, "Lurking");
                                }
                                return null;
                            },
                            ExpiresAt = ExpirationCondition.CountsDownAtEndOfYourTurn,
                        });

                        foreach (Creature player in party) {
                            self.Owner.DetectionStatus.HiddenTo.Add(player);
                        }
                        self.Owner.DetectionStatus.Undetected = true;
                        target.AddQEffect(CommonQEffects.Stalked(self.Owner));

                        self.Owner.AddQEffect(new QEffect() {
                            Id = QEffectId.Slowed,
                            Value = 1
                        });
                        await self.Owner.Battle.GameLoop.Turn(self.Owner, false);
                        self.Owner.RemoveAllQEffects(qf => qf.Id == QEffectId.Slowed);
                    }

                })
                .AddQEffect(new QEffect("Nimble Dodge {icon:Reaction}", "{b}Trigger{/b} The drow assassin is hit or critically hit by an attack. {b}Effect{/b} The drow assassin gains a +2 bonus to their Armor Class against the triggering attack.") {
                    YouAreTargetedByARoll = async (self, action, result) => {
                        if ((result.CheckResult == CheckResult.Success || result.CheckResult == CheckResult.CriticalSuccess) && result.ThresholdToDowngrade <= 2) {
                            if (self.UseReaction()) {
                                self.Owner.AddQEffect(new QEffect() {
                                    ExpiresAt = ExpirationCondition.Ephemeral,
                                    BonusToDefenses = (self, action, defence) => defence == Defense.AC ? new Bonus(2, BonusType.Untyped, "Nimble Dodge") : null
                                });
                                return true;
                            }
                        }
                        return false;
                    }
                })
                .AddQEffect(new QEffect() {
                    AdditionalGoodness = (self, action, target) => {
                        if (target.QEffects.FirstOrDefault(qf => qf.Id == QEffectIds.Stalked && qf.Source == self.Owner) != null) {
                            return 10f;
                        }
                        return 0f;
                    }
                })
                .AddQEffect(CommonQEffects.Drow())
                .AddQEffect(QEffect.SneakAttack("2d6"))
                .WithBasicCharacteristics()
                .WithProficiency(Trait.Dagger, Proficiency.Expert)
                .AddHeldItem(Items.CreateNew(ItemName.Dagger))
                .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "dagger"))
                );

            ModManager.RegisterNewCreature("Drow Assassin", Creatures[ModEnums.CreatureId.DROW_ASSASSIN]);


            // CREATURE - Drow Fighter
            int poisonDC = 17;
            Creatures.Add(ModEnums.CreatureId.DROW_FIGHTER,
            encounter => new Creature(Illustrations.DrowFighter, "Drow Fighter", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid }, 1, 5, 6, new Defenses(15, 4, 10, 7), 18,
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
                        OnEachTarget = async (a, d, result) => {
                            d.AddQEffect(QEffect.PersistentDamage("1d6", DamageKind.Bleed));
                        }
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

                    if (self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(self.Owner.OwningFaction) && cr.Threatens(self.Owner.Occupies)).ToArray().Length > 0) {
                        return 0f;
                    }

                    if (target != null && !target.HasEffect(QEffectIds.LethargyPoison) && !target.HasEffect(QEffectId.Slowed)) {
                        float start = 15f;
                        float percentage = (float) dc - ((float)target.Defenses.GetBaseValue(Defense.Fortitude) + 10.5f);
                        percentage *= 5f;
                        percentage += 50f;
                        start = start / 100 * percentage;
                        return start;
                    }

                    return 0f;
                }
            }));

            ModManager.RegisterNewCreature("Drow Fighter", Creatures[ModEnums.CreatureId.DROW_FIGHTER]);


            // CREATURE - Drow Shootist
            Creatures.Add(ModEnums.CreatureId.DROW_SHOOTIST,
            encounter => new Creature(Illustrations.DrowShootist, "Drow Shootist", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid }, 1, 10, 6, new Defenses(15, 4, 10, 7), 18,
            new Abilities(-1, 4, 1, 1, 2, 2), new Skills(acrobatics: 7, stealth: 7, deception: 7, intimidation: 5))
            //.WithAIModification(ai => {
            //    ai.OverrideDecision = (self, options) => {
            //        Creature creature = self.Self;
            //        // Check if has crossbow
            //        Item? handcrossbow = creature.HeldItems.FirstOrDefault(item => item.ItemName == ItemName.HandCrossbow);

            //        if (handcrossbow == null) {
            //            return null;
            //        }

            //        // Check if crossbow is loaded
            //        if (handcrossbow.EphemeralItemProperties.NeedsReload) {
            //            // foreach (Option option in options.Where(opt => opt.AiUsefulness.ObjectiveAction != null && opt.AiUsefulness.ObjectiveAction.Action.Name.StartsWith("Reload") || opt.Text == "Reload")) {
            //            foreach (Option option in options.Where(opt => opt.Text == "Reload" || (opt.AiUsefulness.ObjectiveAction != null && opt.AiUsefulness.ObjectiveAction.Action.Name == "Reload"))) {
            //                option.AiUsefulness.MainActionUsefulness = 1f;
            //            }
            //        }
            //        return null;
            //    };
            //})
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(QEffect.SneakAttack("1d8"))
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Melee, Proficiency.Trained)
            .WithProficiency(Trait.Ranged, Proficiency.Master)
            .AddHeldItem(Items.CreateNew(ItemName.HandCrossbow))
            .AddHeldItem(Items.CreateNew(ItemName.HandCrossbow))
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    Item? xbow = self.Owner.HeldItems.FirstOrDefault(item => item.ItemName == ItemName.HandCrossbow && !item.EphemeralItemProperties.NeedsReload);
                    if (xbow == null) {
                        return null;
                    }

                    StrikeModifiers strikeModifiers = new StrikeModifiers() {
                        OnEachTarget = async (a, d, result) => {
                            if (result == CheckResult.Success) {
                                d.AddQEffect(QEffect.FlatFooted("Distracting Shot").WithExpirationAtStartOfSourcesTurn(a, 0));
                            } else if (result == CheckResult.CriticalSuccess) {
                                d.AddQEffect(QEffect.FlatFooted("Distracting Shot").WithExpirationAtEndOfSourcesNextTurn(a));
                            }
                       
                        }
                    };
                    CombatAction action = self.Owner.CreateStrike(xbow, -1, strikeModifiers);
                    action.ActionCost = 2;
                    action.Name = "Distracting Shot";
                    action.Description = StrikeRules.CreateBasicStrikeDescription2(action.StrikeModifiers, additionalSuccessText: "The target is flat footed until the start of your next turn.", additionalCriticalSuccessText: "The target is flat footed until the end of your next turn.");
                    action.ShortDescription += " and the target is flat footed until the start of the Drow Shootist's next turn, or the end of a critical success.";
                    action.Illustration = new SideBySideIllustration(action.Illustration, IllustrationName.CreateADiversion);
                    action.WithGoodnessAgainstEnemy((target, attacker, defender) => {
                        return defender.QEffects.FirstOrDefault(qf => qf.Name == "Flat-footed") != null ? 2 : 6.5f;
                    });

                    return (ActionPossibility)action;
                }
            })
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    if (self.Owner.HeldItems.Count < 2) {
                        return null;
                    }

                    Item xbow1 = self.Owner.HeldItems[0];
                    Item xbow2 = self.Owner.HeldItems[1];
                    if (xbow1.ItemName != ItemName.HandCrossbow || xbow2.ItemName != ItemName.HandCrossbow) {
                        return null;
                    }

                    if (!xbow1.EphemeralItemProperties.NeedsReload || !xbow2.EphemeralItemProperties.NeedsReload) {
                        return null;
                    }

                    CombatAction action = new CombatAction(self.Owner, new SideBySideIllustration(IllustrationName.HandCrossbow, IllustrationName.HandCrossbow), "Reloading Trick", new Trait[] { Trait.Manipulate }, "", Target.Self())
                    .WithActionCost(1)
                    .WithSoundEffect(SfxName.OpenLock)
                    .WithEffectOnSelf(user => {
                        xbow1.EphemeralItemProperties.NeedsReload = false;
                        xbow2.EphemeralItemProperties.NeedsReload = false;
                    })
                    .WithGoodness((targeting, a, d) => 15)
                    ;
                    return (ActionPossibility)action;
                }
            })
            );

            ModManager.RegisterNewCreature("Drow Shootist", Creatures[ModEnums.CreatureId.DROW_SHOOTIST]);


            // CREATURE - Drow Priestess
            Creatures.Add(ModEnums.CreatureId.DROW_PRIESTESS,
            encounter => new Creature(Illustrations.DrowPriestess, "Drow Priestess", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid }, 3, 9, 6, new Defenses(20, 8, 7, 11), 39,
            new Abilities(1, 2, 1, 0, 4, 2), new Skills(deception: 9, stealth: 7, intimidation: 9))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;

                    // Bane AI
                    foreach (Option option in options.Where(o => o.Text == "Bane")) {
                        option.AiUsefulness.MainActionUsefulness = 30;
                    }
                    Option? expandBane = options.FirstOrDefault(o => o.Text == "Increase Bane radius");
                    if (expandBane != null) {
                        QEffect bane = creature.QEffects.FirstOrDefault(qf => qf.Name == "Bane");
                        (int, bool) temp = ((int, bool)) bane.Tag;
                        int radius = temp.Item1;

                        expandBane.AiUsefulness.MainActionUsefulness = 0f;
                        foreach (Creature enemy in creature.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction) && creature.DistanceTo(cr.Occupies) == radius + 1)) {
                            expandBane.AiUsefulness.MainActionUsefulness += 4;
                        }
                    }

                    // Demoralize AI
                    foreach (Option option in options.Where(o => o.Text == "Demoralize" || (o.AiUsefulness.ObjectiveAction != null && o.AiUsefulness.ObjectiveAction.Action.ActionId == ActionId.Demoralize))) {
                        option.AiUsefulness.MainActionUsefulness = 0f;
                    }

                    // Ally and enemy proximity AI
                    foreach (Option option in options.Where(o => o.OptionKind == OptionKind.MoveHere)) {
                        TileOption? option2 = option as TileOption;
                        if (option2 != null) {
                            //option2.AiUsefulness.MainActionUsefulness += creature.Battle.AllCreatures.Where(c => c != creature && c.OwningFaction == creature.OwningFaction && !c.HasTrait(Trait.Mindless) && c.DistanceTo(option2.Tile) <= 2 && c.HasLineOfEffectTo(option2.Tile) != CoverKind.Blocked).ToArray().Length;
                            //option2.AiUsefulness.MainActionUsefulness += creature.Battle.AllCreatures.Where(c => c.OwningFaction.EnemyFactionOf(creature.OwningFaction) && c.DistanceTo(option2.Tile) <= 2).ToArray().Length * 0.2f;
                            float mod1 = creature.Battle.AllCreatures.Where(c => c != creature && c.OwningFaction == creature.OwningFaction && !c.HasTrait(Trait.Mindless) && c.DistanceTo(option2.Tile) <= 2 && c.HasLineOfEffectTo(option2.Tile) != CoverKind.Blocked).ToArray().Length;
                            float mod2 = creature.Battle.AllCreatures.Where(c => c.OwningFaction.EnemyFactionOf(creature.OwningFaction) && c.DistanceTo(option2.Tile) <= 2).ToArray().Length * 0.2f;
                            option2.AiUsefulness.MainActionUsefulness += mod1 + mod2;
                        }
                    }
                    foreach (Option option in options.Where(o => o.OptionKind != OptionKind.MoveHere && o.AiUsefulness.MainActionUsefulness != 0)) {
                        float mod1 = creature.Battle.AllCreatures.Where(c => c != creature && c.OwningFaction == creature.OwningFaction && !c.HasTrait(Trait.Mindless) && c.DistanceTo(creature) <= 2 && creature.HasLineOfEffectTo(c.Occupies) != CoverKind.Blocked).ToArray().Length;
                        float mod2 = creature.Battle.AllCreatures.Where(c => c.OwningFaction.EnemyFactionOf(creature.OwningFaction) && c.DistanceTo(creature) <= 2).ToArray().Length * 0.2f;
                        option.AiUsefulness.MainActionUsefulness += mod1 + mod2;
                        //option.AiUsefulness.MainActionUsefulness += creature.Battle.AllCreatures.Where(c => c != creature && c.OwningFaction == creature.OwningFaction && !c.HasTrait(Trait.Mindless) && c.DistanceTo(creature) <= 2 && creature.HasLineOfEffectTo(c.Occupies) != CoverKind.Blocked).ToArray().Length;
                        //option.AiUsefulness.MainActionUsefulness += creature.Battle.AllCreatures.Where(c => c.OwningFaction.EnemyFactionOf(creature.OwningFaction) && c.DistanceTo(creature) <= 2).ToArray().Length * 0.2f;

                    }

                    return null;
                };
            })
            .AddQEffect(CommonQEffects.CruelTaskmistress("1d6"))
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(CommonQEffects.DrowClergy())
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .WithProficiency(Trait.Divine, Proficiency.Expert)
            .AddHeldItem(Items.CreateNew(CustomItems.ScourgeOfFangs))
            .WithSpellProficiencyBasedOnSpellAttack(11, Ability.Wisdom)
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Cleric, Ability.Wisdom, Trait.Divine).WithSpells(
                new SpellId[] { SpellId.Bane, SpellId.Fear, SpellId.Fear, SpellId.Fear, SpellId.ChillTouch },
                new SpellId[] { SpellId.Harm, SpellId.Harm, SpellId.Harm }).Done()
            );
            ModManager.RegisterNewCreature("Drow Priestess", Creatures[ModEnums.CreatureId.DROW_PRIESTESS]);


            // CREATURE - Drow Temple Guard
            Creatures.Add(ModEnums.CreatureId.DROW_TEMPLEGUARD,
            encounter => new Creature(Illustrations.DrowTempleGuard, "Drow Temple Guard", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid }, 2, 8, 6, new Defenses(18, 11, 8, 9), 28,
            new Abilities(4, 2, 3, 0, 2, 0), new Skills(athletics: 8, intimidation: 6))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (t, cr) => cr.HasEffect(QEffectIds.DrowClergy) && t.DistanceTo(cr.Occupies) <= 3, 2f);
                    AiFuncs.PositionalGoodness(monster, options, (t, cr) => cr.HasEffect(QEffectIds.DrowClergy) && t.DistanceTo(cr.Occupies) <= 2, 1f);

                    return null;
                };
            })
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(CommonQEffects.DrowBloodBond())
            .AddQEffect(CommonQEffects.RetributiveStrike(4, cr => cr.HasEffect(QEffectIds.DrowClergy), "a member of the drow clergy", true))
            //.AddQEffect(QEffect.AttackOfOpportunity())
            .AddQEffect(QEffect.DamageResistance(DamageKind.Negative, 5))
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .AddHeldItem(Items.CreateNew(ItemName.Halberd))
            );
            ModManager.RegisterNewCreature("Drow Temple Guard", Creatures[ModEnums.CreatureId.DROW_TEMPLEGUARD]);


            // CREATURE - Hunting spider
            Creatures.Add(ModEnums.CreatureId.HUNTING_SPIDER,
            encounter => new Creature(Illustrations.HuntingSpider, "Hunting Spider", new List<Trait>() { Trait.Animal, Traits.Spider }, 1, 7, 5, new Defenses(17, 6, 9, 5), 16,
            new Abilities(2, 4, 1, -5, 2, -2), new Skills(acrobatics: 7, stealth: 7, athletics: 5))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;
                    Option best = options.MaxBy(o => o.AiUsefulness.MainActionUsefulness);
                    return null;
                };
            })
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Ranged, Proficiency.Trained)
            .WithCharacteristics(false, true)
            .WithUnarmedStrike(new Item(IllustrationName.Jaws, "fangs", new Trait[] { Trait.Melee, Trait.Finesse, Trait.Unarmed, Trait.Brawling }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)))
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") {Id = QEffectId.IgnoresWeb})
            .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "fangs"))
            .AddQEffect(CommonQEffects.WebAttack(16))
            );
            ModManager.RegisterNewCreature("Hunting Spider", Creatures[ModEnums.CreatureId.HUNTING_SPIDER]);


            // CREATURE - Drider
            Creatures.Add(ModEnums.CreatureId.DRIDER,
            encounter => new Creature(Illustrations.Drider, "Drider", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Aberration, Traits.Spider }, 3, 6, 6, new Defenses(17, 12, 7, 6), 56,
            new Abilities(5, 3, 3, 1, 3, 2), new Skills(athletics: 10, intimidation: 8))
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Ranged, Proficiency.Expert)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(ItemName.Glaive))
            .WithUnarmedStrike(new Item(IllustrationName.Jaws, "fangs", new Trait[] { Trait.Melee, Trait.Finesse, Trait.Unarmed, Trait.Brawling }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)))
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(QEffect.AttackOfOpportunity())
            .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "fangs")) // Change to drider venom?
            .AddQEffect(CommonQEffects.WebAttack(16))
            .AddQEffect(CommonQEffects.MiniBoss())
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            );
            ModManager.RegisterNewCreature("Drider", Creatures[ModEnums.CreatureId.DRIDER]);


            // CREATURE - Drow Arcanist
            Creatures.Add(ModEnums.CreatureId.DROW_ARCANIST,
            encounter => new Creature(IllustrationName.DarkPoet256, "Drow Arcanist", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid }, 1, 7, 6, new Defenses(15, 4, 7, 10), 14,
            new Abilities(1, 3, 0, 5, 1, 1), new Skills(acrobatics: 10, intimidation: 6, arcana: 8, deception: 8))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;

                    // Get current proximity score
                    float currScore = 0f;
                    foreach (Creature enemy in creature.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction))) {
                        currScore += creature.DistanceTo(enemy);
                    }

                    // Find how close allies are to the party
                    List<Creature> allies = creature.Battle.AllCreatures.Where(cr => cr.Alive && cr.OwningFaction == creature.OwningFaction).ToList();
                    float allyScore = 0;
                    foreach (Creature ally in allies) {
                        foreach (Creature enemy in creature.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction))) {
                           allyScore += ally.DistanceTo(enemy);
                        }
                    }
                    allyScore /= allies.Count;

                    foreach (Option option in options.Where(o => o.OptionKind == OptionKind.MoveHere && o is TileOption)) {
                        TileOption option2 = option as TileOption;
                        float personalScore = 0f;
                        foreach (Creature enemy in creature.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction))) {
                            personalScore += option2.Tile.DistanceTo(enemy.Occupies);
                        }

                        if (option2.Text == "Stride" && creature.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction) && cr.Threatens(creature.Occupies)).ToArray().Length == 0) {
                            if (personalScore > allyScore && currScore < allyScore) {
                                option2.AiUsefulness.MainActionUsefulness += 5f;
                            } else if (personalScore < allyScore) {
                                option2.AiUsefulness.MainActionUsefulness -= 15f;
                            }
                        } else if (option2.Text == "Step") {
                            if (personalScore > allyScore && currScore < allyScore) {
                                option2.AiUsefulness.MainActionUsefulness += 5f;
                            } else if (personalScore < allyScore) {
                                option2.AiUsefulness.MainActionUsefulness -= 15f;
                            }
                        }
                    }

                    return null;
                };
            })
            .WithProficiency(Trait.Melee, Proficiency.Trained)
            .WithProficiency(Trait.Arcane, Proficiency.Expert)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(ItemName.RepeatingHandCrossbow))
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(new QEffect("Slip Away {icon:Reaction}", "{b}Trigger{/b} The drow arcanist is damaged by an attack. {b}Effect{/b} The drow arcanist makes a free step action and gains +1 AC until the end of their attacker's turn.") {
                AfterYouTakeDamage = async (self, amount, kind, action, critical) => {
                    if (!(action.HasTrait(Trait.Melee) || (action.Owner != null && action.Owner.IsAdjacentTo(self.Owner)))) {
                        return;
                    }

                    if (self.UseReaction()) {
                        self.Owner.AddQEffect(new QEffect("Slip Away", "+1 circumstance bonus to AC.") {
                            Illustration = IllustrationName.Shield,
                            BonusToDefenses = (self, action, defence) => defence == Defense.AC ? new Bonus(1, BonusType.Circumstance, "Slip Away") : null,
                            ExpiresAt = ExpirationCondition.ExpiresAtEndOfAnyTurn
                        });
                        await self.Owner.StepAsync("Choose tile for Slip Away");
                    }
                }
            })
            .AddQEffect(new QEffect("Dark Arts", "The drow arcanist excels at causing pain with their black practice. Their non-cantrip spells gain a +2 status bonus to damage.") {
                BonusToDamage = (qfSelf, spell, target) => { 
                    return spell.HasTrait(Trait.Spell) && !spell.HasTrait(Trait.Cantrip) && !spell.HasTrait(Trait.Focus) && spell.CastFromScroll == null ? new Bonus(2, BonusType.Status, "Dark Arts") : null;
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Wizard, Ability.Intelligence, Trait.Arcane).WithSpells(
                new SpellId[] { SpellId.MagicMissile, SpellId.MagicMissile, SpellId.GrimTendrils, SpellId.ChillTouch, SpellId.ProduceFlame, SpellId.Shield }).Done()
            );
            ModManager.RegisterNewCreature("Drow Arcanist", Creatures[ModEnums.CreatureId.DROW_ARCANIST]);

            // CREATURE - Drow Shadowcaster
            Creatures.Add(ModEnums.CreatureId.DROW_SHADOWCASTER,
            encounter => {
                Creature creature = Creatures[ModEnums.CreatureId.DROW_ARCANIST](encounter);
                creature.Level = 3;
                creature.Defenses = new Defenses(creature.Defenses.GetBaseValue(Defense.AC) + 2, creature.Defenses.GetBaseValue(Defense.Fortitude) + 2, creature.Defenses.GetBaseValue(Defense.Reflex) + 2, creature.Defenses.GetBaseValue(Defense.Will) + 2);
                creature.Perception += 2;
                foreach (Skill skill in Skills.RelevantSkills) {
                    if (creature.Skills.Get(skill) > 5) {
                        creature.Skills.Set(skill, creature.Skills.Get(skill) + 2);
                    }
                }
                creature.Spellcasting.Sources.Clear();
                creature.AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Wizard, Ability.Intelligence, Trait.Arcane).WithSpells(
                new SpellId[] { SpellId.MagicMissile, SpellId.MagicMissile, SpellId.GrimTendrils, SpellId.Fear, SpellId.ChillTouch, SpellId.ProduceFlame, SpellId.Shield },
                new SpellId[] { SpellId.MagicMissile, SpellId.AcidArrow, SpellId.HideousLaughter }).Done();
                return creature;
            });
            ModManager.RegisterNewCreature("Drow Shadowcaster", Creatures[ModEnums.CreatureId.DROW_SHADOWCASTER]);


            // CREATURE - Drow Inquisitrix
            string icDmg = "1d10";
            Creatures.Add(ModEnums.CreatureId.DROW_INQUISITRIX,
            encounter => new Creature(Illustrations.DrowInquisitrix, "Drow Inquisitrix", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid }, 2, 8, 6, new Defenses(17, 5, 8, 11), 25,
            new Abilities(2, 4, 1, 2, 2, 4), new Skills(acrobatics: 8, intimidation: 11, religion: 7))
            .WithProficiency(Trait.Martial, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Trained)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(CustomItems.ScourgeOfFangs))
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(CommonQEffects.DrowClergy())
            .AddQEffect(QEffect.SneakAttack("1d4"))
            .AddQEffect(new QEffect("Iron Command {icon:Reaction}", "{b}Trigger{/b} An enemy within 15 feet damages you. {b}Effect{/b} Your attacker must choose either to fall prone or suffer " + icDmg + " mental damage. You then deal +1d6 evil or negative damage against them with your strikes, until a new enemy earns your ire.") {
                AfterYouTakeDamage = async (self, amount, kind, action, critical) => {
                    if (action == null || action.Owner == null || action.Owner == action.Owner.Battle.Pseudocreature) {
                        return;
                    }

                    if (action.Owner.OwningFaction == self.Owner.OwningFaction) {
                        return;
                    }

                    if (self.UseReaction()) {
                        if (await action.Owner.Battle.AskForConfirmation(action.Owner, self.Owner.Illustration, $"{self.Owner.Name} uses Iron Command, urging you to kneel before your betters. Do you wish to drop prone in supplication, or refuse and suffer " + icDmg + " mental damage?", "Submit", "Defy")) {
                            action.Owner.AddQEffect(QEffect.Prone());
                        } else {
                            // TODO: Make a dummy action for this damage
                            CombatAction dummyAction = new CombatAction(self.Owner, self.Owner.Illustration, "Iron Command", new Trait[] { Trait.Divine, Trait.Emotion, Trait.Enchantment, Trait.Mental }, "You deal " + icDmg + " mental damage to a creature that attacked you, and refuses to kneel.", Target.Uncastable());
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
            })
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
                    action.ActionCost = 2;
                    action.Name = $"Channel Smite ({weapon.Name})";
                    action.Description = "You siphon the destructive energies of positive or negative energy through a melee attack and into your foe. Make a melee Strike and add the spell’s damage to the Strike’s damage. This is negative damage if you expended a harm spell or positive damage if you expended a heal spell. The spell is expended with no effect if your Strike fails or hits a creature that isn’t damaged by that energy type (such as if you hit a non-undead creature with a heal spell).";
                    action.ShortDescription += " and expends a casting of harm to inflict 1d8 negative damage.";
                    action.Illustration = new SideBySideIllustration(action.Illustration, IllustrationName.Harm);
                    action.WithGoodnessAgainstEnemy((target, attacker, defender) => {
                        return defender.HasTrait(Trait.Undead) ? -100f : 4.5f + action.TrueDamageFormula.ExpectedValue;
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
                new SpellId[] { SpellId.Harm, SpellId.Harm, SpellId.Harm }).Done()
            );
            ModManager.RegisterNewCreature("Drow Inquisitrix", Creatures[ModEnums.CreatureId.DROW_INQUISITRIX]);


            // CREATURE - Witch Crone
            Creatures.Add(ModEnums.CreatureId.WITCH_CRONE,
            encounter => new Creature(IllustrationName.SwampHag, "Agatha Agaricus", new List<Trait>() { Trait.Neutral, Trait.Evil, Trait.Human, Trait.Tiefling, Trait.Humanoid, Traits.Witch }, 3, 4, 5, new Defenses(17, 9, 6, 12), 60,
            new Abilities(2, 2, 3, 4, 2, 0), new Skills(nature: 13, occultism: 9, intimidation: 10, religion: 9))
            .WithProficiency(Trait.Unarmed, Proficiency.Expert)
            .WithProficiency(Trait.BattleformAttack, Proficiency.Master)
            .WithProficiency(Trait.Spell, Proficiency.Trained)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(new Item(IllustrationName.Fist, "nails", new Trait[] { Trait.Unarmed, Trait.Melee, Trait.Brawling, Trait.Finesse }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing)))
            .AddQEffect(new QEffect("Curse of Skittering Paws", "Nature itself turns against the party, calling forth swarms of critters to decend upon them so long as Agatha Agaricus lives.") {
                Tag = true,
                StartOfYourTurn = async (self, owner) => {
                    if (!owner.Alive) {
                        return;
                    }

                    if (self.Tag != null && self.Tag as bool? == false) {
                        self.Tag = true;
                        return;
                    }

                    List<Tile> spawnPoints = owner.Battle.Map.AllTiles.Where(t => {
                        if (!t.IsFree) {
                            return false;
                        }

                        foreach (Creature pc in owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsHumanControlled)) {
                            if (pc.DistanceTo(t) < 4) {
                                return false;
                            }
                        }
                        return true;
                    }).ToList();
                    Tile spawnPt = spawnPoints[R.Next(0, spawnPoints.Count)];
                    var options = new List<Creature>() { MonsterStatBlocks.CreateGiantRat(), Creatures[ModEnums.CreatureId.HUNTING_SPIDER](owner.Battle.Encounter), MonsterStatBlocks.CreateVenomousSnake(), MonsterStatBlocks.CreateWolf() };
                    Creature summon = options[R.Next(options.Count)];
                    summon.AddQEffect(new QEffect() {
                        AdditionalGoodness = (self, action, target) => {
                            if (action.HasTrait(Trait.Strike)) {
                                return 5;
                            }
                            return 0;
                        }
                    });
                    if (owner.Battle.Encounter.CharacterLevel == 1) {
                        self.Tag = false;
                    } else if (owner.Battle.Encounter.CharacterLevel < 3) {
                        summon.ApplyWeakAdjustments(false);
                    }
                    summon.AddQEffect(new QEffect(self.Name, "This creature's behaviour has been altered by a powerful curse. Once broken, it will revert to its natural behaviour and flee.") {
                        Innate = false,
                        Source = owner,
                        Illustration = owner.Illustration,
                        StateCheckWithVisibleChanges = async self => {
                            if (!self.Source.Alive) {
                                self.Owner.Occupies.Overhead($"*{self.Owner.Name} flees!*", Color.Green, $"With the cruse broken, {self.Owner.Name} flees from the fight.");
                                self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                            }
                        }
                    });
                    owner.Battle.SpawnCreature(summon, owner.OwningFaction, spawnPt.X, spawnPt.Y);
                    summon.Occupies.Overhead("*Curse of Skittering Paws*", Color.White, $"{summon.Name} is drawn to aide the coven by the curse of skittering paws.");
                    Sfxs.Play(SfxName.BeastRoar);
                },
            })
            .AddQEffect(new QEffect("Wild Shape", "At the start of each turn, if wounded, Agatha Agaricus takes on a new animal form, preventing her from casting spells but allowing her access to new attacks.") {
                StartOfYourTurn = async (self, owner) => {
                    if (owner.HP > owner.MaxHP * 0.8f) {
                        return;
                    }
                    QEffect transform = new QEffect() {
                        ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn,
                        PreventTakingAction = action => action.HasTrait(Trait.Spell) ? "Cannot cast spells whilst transformed." : null
                    };

                    int roll = R.Next(0, 5);
                    switch (roll) {
                        case 1:
                            transform.Illustration = Illustrations.AnimalFormBear;
                            transform.Name = "Bear Form";
                            transform.Description = $"{self.Owner.Name} has assumed the form of a ferocious bear, capable of grappling its prey on a successful jaws attack.";
                            transform.StateCheck = self => {
                                self.Owner.ReplacementIllustration = Illustrations.AnimalFormBear;
                                self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d10", DamageKind.Piercing, Trait.BattleformAttack).WithAdditionalWeaponProperties(properties => {
                                    properties.OnTarget = async (strike, a, d, result) => {
                                        if (result >= CheckResult.Success)
                                            await Possibilities.Grapple(a, d, result);
                                    };
                                });
                            };
                            transform.AdditionalUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.DragonClaws, "claws", "1d6", DamageKind.Slashing, Trait.Agile, Trait.BattleformAttack);
                            transform.BonusToDefenses = (self, action, defence) => {
                                if (defence == Defense.AC) {
                                    return new Bonus(2, BonusType.Item, "Natural Armour");
                                }
                                return null;
                            };
                            // ephemeral.ProvidesArmor = new Item(IllustrationName.None, "Natural Armour", new Trait[] { Trait.UnarmoredDefense, Trait.Armor }).WithArmorProperties(new ArmorProperties(4, 0, 0, 0, 0));
                            goto case 10;
                        case 2:
                            transform.Illustration = Illustrations.AnimalFormSnake;
                            transform.Name = "Serpent Form";
                            transform.Description = $"{self.Owner.Name} has assumed the form of a venomous serpent, capable of poisoning its prey on a successful jaws attack.";
                            transform.StateCheck = self => {
                                self.Owner.ReplacementIllustration = Illustrations.AnimalFormSnake;
                                self.Owner.AddQEffect(Affliction.CreateInjuryQEffect(Affliction.CreateSnakeVenom("Snake Venom")).WithExpirationEphemeral());
                                self.Owner.AddQEffect(QEffect.Swimming().WithExpirationEphemeral());
                                self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Slashing, 2 + self.Owner.Level);
                                self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Piercing, 2 + self.Owner.Level);
                                self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d6", DamageKind.Piercing, Trait.BattleformAttack, Trait.AddsInjuryPoison).WithAdditionalWeaponProperties(properties => {
                                    properties.AdditionalDamageFormula = "1d4";
                                    properties.AdditionalDamageKind = DamageKind.Poison;
                                });
                            };
                            transform.SetBaseSpeedTo = 8;
                            goto case 10;
                        case 3:
                            transform.Illustration = Illustrations.AnimalFormWolf;
                            transform.Name = "Wolf Form";
                            transform.Description = $"{self.Owner.Name} has assumed the form of a cunning wolf, making her cunningly adept at exploiting her foe's weaknesses.";
                            transform.StateCheck = self => {
                                self.Owner.ReplacementIllustration = Illustrations.AnimalFormWolf;
                                self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d10", DamageKind.Piercing, Trait.BattleformAttack);
                                self.Owner.AddQEffect(QEffect.SneakAttack("1d8").WithExpirationEphemeral());
                            };
                            goto case 10;
                        case 10:
                            self.Owner.AddQEffect(transform);
                            break;
                        default:
                            break;
                    }
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, Traits.Witch, Ability.Intelligence, Trait.Primal).WithSpells(
                level1: new SpellId[] { SpellId.Heal, SpellId.PummelingRubble, SpellId.PummelingRubble },
                level2: new SpellId[] { SpellId.Barkskin }).Done()
            );
            ModManager.RegisterNewCreature("Witch Crone", Creatures[ModEnums.CreatureId.WITCH_CRONE]);


            // CREATURE - Witch Mother
            Creatures.Add(ModEnums.CreatureId.WITCH_MOTHER,
            encounter => new Creature(IllustrationName.WaterElemental256, "Mother Cassandra", new List<Trait>() { Trait.Neutral, Trait.Evil, Trait.Human, Trait.Tiefling, Trait.Humanoid, Traits.Witch }, 2, 4, 5, new Defenses(16, 8, 5, 11), 40,
            new Abilities(0, 2, 3, 4, 2, 0), new Skills(nature: 9, occultism: 13, intimidation: 8))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (tile, cr) => cr.HasTrait(Traits.Witch) && cr.DistanceTo(tile) <= 3, 1.5f, false);
                    return null;
                };
            })
            .WithProficiency(Trait.Unarmed, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(new Item(IllustrationName.Fist, "nails", new Trait[] { Trait.Unarmed, Trait.Melee, Trait.Brawling, Trait.Finesse }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing)))
            .AddHeldItem(Items.CreateNew(CustomItems.ProtectiveAmulet))
            .AddQEffect(new QEffect("Curse of Dread", "The party are afflicted by a powerful supernatural uncertainty, as if fate itself will conspire against them so long as the caster lives.") {
                StateCheckWithVisibleChanges = async self => {
                    if (!self.Owner.Alive) {
                        return;
                    }
                    List<Creature> party = self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(self.Owner.OwningFaction)).ToList();
                    party.ForEach(cr => {
                        cr.AddQEffect(new QEffect("Curse of Dread", $"You're frightened 1 so long as {self.Owner.Name} lives.") {
                            ExpiresAt = ExpirationCondition.Ephemeral,
                            Innate = false,
                            Source = self.Owner,
                            Illustration = self.Owner.Illustration,
                            StateCheck = self => {
                                self.Owner.AddQEffect(QEffect.Frightened(1).WithExpirationEphemeral());
                            }
                        });
                    });
                },
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, Traits.Witch, Ability.Intelligence, Trait.Divine).WithSpells(
                level1: new SpellId[] { SpellId.GrimTendrils, SpellId.Heal, SpellId.Heal }).Done()
            );
            ModManager.RegisterNewCreature("Witch Mother", Creatures[ModEnums.CreatureId.WITCH_MOTHER]);


            // CREATURE - Witch Maiden
            Creatures.Add(ModEnums.CreatureId.WITCH_MAIDEN,
            encounter => new Creature(IllustrationName.SuccubusShapeshifted, "Harriet Hex", new List<Trait>() { Trait.Neutral, Trait.Evil, Trait.Human, Trait.Tiefling, Trait.Humanoid, Traits.Witch }, 2, 6, 5, new Defenses(15, 5, 8, 11), 30,
            new Abilities(0, 4, 3, 4, 2, 0), new Skills(nature: 9, occultism: 9, intimidation: 8, arcana: 13))
            .WithProficiency(Trait.Unarmed, Proficiency.Trained)
            .WithProficiency(Trait.Crossbow, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(CustomItems.Hexshot))
            .WithUnarmedStrike(new Item(IllustrationName.Fist, "nails", new Trait[] { Trait.Unarmed, Trait.Melee, Trait.Brawling, Trait.Finesse }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing)))
            .AddQEffect(new QEffect("Curse of Agony", $"The party are wracked by terrible pain, which will not abate so long as the caster lives.") {
                StateCheckWithVisibleChanges = async self => {
                    if (!self.Owner.Alive) {
                        return;
                    }

                    List<Creature> party = self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(self.Owner.OwningFaction)).ToList();
                    party.ForEach(cr => {
                        cr.AddQEffect(new QEffect("Curse of Agony", $"You suffer 1d6 mental damage at the start of each turn so long as {self.Owner.Name} lives.") {
                            ExpiresAt = ExpirationCondition.Ephemeral,
                            Innate = false,
                            Source = self.Owner,
                            Illustration = self.Owner.Illustration,
                            StartOfYourTurn = async (qfCurse, victim) => {
                                if (victim.Traits.Any(t => t.HumanizeTitleCase2() == "Eidolon")) {
                                    QEffect bond = victim.QEffects.FirstOrDefault(qf => qf.Id.HumanizeTitleCase2() == "Summoner_Shared HP");
                                    CombatAction action = new CombatAction(self.Owner, self.Illustration, "Curse of Agony", new Trait[] { Trait.Curse, Trait.Mental, Trait.Arcane }, "", Target.Emanation(100).WithIncludeOnlyIf((area, target) => {
                                        return target == victim || target == bond.Source;
                                    }))
                                    .WithEffectOnEachTarget(async (spell, user, d, result) => {
                                        await CommonSpellEffects.DealDirectDamage(spell, DiceFormula.FromText("1d6", "Curse of Agony"), d, CheckResult.Success, DamageKind.Mental);
                                    });
                                    action.ChosenTargets.ChosenCreatures.Add(victim);
                                    action.ChosenTargets.ChosenCreatures.Add(bond.Source);
                                    await action.AllExecute();
                                    return;
                                } else if (victim.Traits.Any(t => t.HumanizeTitleCase2() == "Summoner") && victim.Battle.AllCreatures.Any(cr => cr.Traits.Any(t => t.HumanizeTitleCase2() == "Eidolon") && cr.QEffects.FirstOrDefault(qf => qf.Id.HumanizeTitleCase2() == "Summoner_Shared HP").Source == victim)) {
                                    return;
                                }
                                CombatAction action2 = new CombatAction(self.Owner, self.Illustration, "Curse of Agony", new Trait[] { Trait.Curse, Trait.Mental, Trait.Arcane }, "", Target.Uncastable());
                                await CommonSpellEffects.DealDirectDamage(action2, DiceFormula.FromText("1d6", "Curse of Agony"), victim, CheckResult.Success, DamageKind.Mental);
                            }
                        });
                    });
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, Traits.Witch, Ability.Intelligence, Trait.Arcane).WithSpells(
                level1: new SpellId[] { SpellId.ChillTouch, SpellId.TrueStrike, SpellId.KineticRam, SpellId.FlourishingFlora },
                level2: new SpellId[] { SpellId.KineticRam, SpellId.TrueStrike }).Done()
            );
            ModManager.RegisterNewCreature("Witch Maiden", Creatures[ModEnums.CreatureId.WITCH_MAIDEN]);

            // Add new creature here

        }

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        internal static class CommonMonsterActions {
            public static CombatAction CreateHide(Creature self) {
                return new CombatAction(self, (Illustration)IllustrationName.Hide, "Hide", new Trait[2] { Trait.Basic, Trait.AttackDoesNotTargetAC },
                    "Make one Stealth check against the Perception DCs of each enemy creature that can see you but that you have cover or concealment from. On a success, you become Hidden to that creature.",
                    (Target)Target.Self(((cr, ai) => ai.HideSelf())).WithAdditionalRestriction((Func<Creature, string>)(innerSelf => {
                        if (HiddenRules.IsHiddenFromAllEnemies(innerSelf, innerSelf.Occupies))
                            return "You're already hidden from all enemies.";
                        return !innerSelf.Battle.AllCreatures.Any<Creature>((Func<Creature, bool>)(cr => cr.EnemyOf(innerSelf) && HiddenRules.HasCoverOrConcealment(innerSelf, cr))) ? "You don't have cover or concealment from any enemy." : (string)null;
                    })))
                .WithActionId(ActionId.Hide)
                .WithSoundEffect(SfxName.Hide)
                .WithEffectOnSelf((innerSelf => {
                    int roll = R.NextD20();
                    foreach (Creature creature in innerSelf.Battle.AllCreatures.Where<Creature>((Func<Creature, bool>)(cr => cr.EnemyOf(innerSelf)))) {
                        if (!innerSelf.DetectionStatus.HiddenTo.Contains(creature) && HiddenRules.HasCoverOrConcealment(innerSelf, creature)) {
                            CheckBreakdown breakdown = CombatActionExecution.BreakdownAttack(new CombatAction(innerSelf, (Illustration)IllustrationName.Hide, "Hide", new Trait[1]
                            {
                    Trait.Basic
                            }, "[this condition has no description]", (Target)Target.Self()).WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Stealth), Checks.DefenseDC(Defense.Perception))), creature);
                            CheckBreakdownResult breakdownResult = new CheckBreakdownResult(breakdown, roll);
                            string str8 = breakdown.DescribeWithFinalRollTotal(breakdownResult);
                            DefaultInterpolatedStringHandler interpolatedStringHandler;
                            if (breakdownResult.CheckResult >= CheckResult.Success) {
                                innerSelf.DetectionStatus.HiddenTo.Add(creature);
                                Tile occupies = creature.Occupies;
                                Color lightBlue = Color.LightBlue;
                                string str9 = innerSelf?.ToString();
                                string str10 = creature?.ToString();
                                interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 3);
                                interpolatedStringHandler.AppendLiteral(" (");
                                interpolatedStringHandler.AppendFormatted(breakdownResult.D20Roll.ToString() + breakdown.TotalCheckBonus.WithPlus());
                                interpolatedStringHandler.AppendLiteral("=");
                                interpolatedStringHandler.AppendFormatted<int>(breakdownResult.D20Roll + breakdown.TotalCheckBonus);
                                interpolatedStringHandler.AppendLiteral(" vs. ");
                                interpolatedStringHandler.AppendFormatted<int>(breakdown.TotalDC);
                                interpolatedStringHandler.AppendLiteral(").");
                                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                                string log = str9 + " successfully hid from " + str10 + stringAndClear;
                                string logDetails = str8;
                                occupies.Overhead("hidden from", lightBlue, log, "Hide", logDetails);
                            } else {
                                Tile occupies = creature.Occupies;
                                Color red = Color.Red;
                                string str11 = innerSelf?.ToString();
                                string str12 = creature?.ToString();
                                interpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 3);
                                interpolatedStringHandler.AppendLiteral(" (");
                                interpolatedStringHandler.AppendFormatted(breakdownResult.D20Roll.ToString() + breakdown.TotalCheckBonus.WithPlus());
                                interpolatedStringHandler.AppendLiteral("=");
                                interpolatedStringHandler.AppendFormatted<int>(breakdownResult.D20Roll + breakdown.TotalCheckBonus);
                                interpolatedStringHandler.AppendLiteral(" vs. ");
                                interpolatedStringHandler.AppendFormatted<int>(breakdown.TotalDC);
                                interpolatedStringHandler.AppendLiteral(").");
                                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                                string log = str11 + " failed to hide from " + str12 + stringAndClear;
                                string logDetails = str8;
                                occupies.Overhead("hide failed", red, log, "Hide", logDetails);
                            }
                        }
                    }
                }));
            }

            // Insert new actions here
        }
    }
}
