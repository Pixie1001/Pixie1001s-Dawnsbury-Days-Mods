using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
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
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModTraits;
using Dawnsbury.Campaign.Encounters.A_Crisis_in_Dawnsbury;
using System.Buffers;
using System.Xml.Schema;
using Microsoft.Xna.Framework.Input;
using FMOD;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class CreatureList {
        internal static Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>> Creatures = new Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>>();
        internal static Dictionary<ObjectId, Func<Encounter?, Creature>> Objects = new Dictionary<ObjectId, Func<Encounter?, Creature>>();

        internal static void LoadCreatures() {
            // TODO: Setup to teleport to random spot and be hidden at start of combat, so logic can be removed from encounter.

            // CREATURE - Unseen Guardian
            Creatures.Add(ModEnums.CreatureId.UNSEEN_GUARDIAN,
                encounter => new Creature(IllustrationName.ElectricityMephit256, "Unseen Guardian", new List<Trait>() { Trait.Lawful, Trait.Elemental, Trait.Air }, 2, 6, 8, new Defenses(16, 5, 11, 7), 30, new Abilities(2, 3, 3, 1, 3, 1), new Skills(stealth: 2))
                .WithAIModification(ai => {
                    ai.IsDemonHorizonwalker = true;
                    ai.OverrideDecision = (self, options) => {
                        Creature creature = self.Self;

                        if (creature.HasEffect(QEffectIds.Lurking)) {
                            return options.Where(opt => opt.OptionKind == OptionKind.MoveHere && opt.Text == "Sneak" && opt is TileOption).ToList().GetRandom();
                        }

                        return creature.Actions.ActionsLeft == 1 && creature.Battle.AllCreatures.All(enemy => !enemy.EnemyOf(creature) || creature.DetectionStatus.EnemiesYouAreHiddenFrom.Contains(enemy)) && !creature.DetectionStatus.Undetected ? options.Where(opt => opt.OptionKind == OptionKind.MoveHere && opt.Text == "Sneak" && opt is TileOption).ToList().GetRandom() : null;
                    };
                })
                .WithProficiency(Trait.Weapon, Proficiency.Trained)
                .AddQEffect(new QEffect("Obliviating Aura", "The unseen guardian feels slippery and elusive in its victim's minds, making it easy for them to lose track of its postion. It gains a +20 bonus to checks made to sneak or hide and can hide in plain sight.") {
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
                        //List<Creature> party = self.Owner.Battle.AllCreatures.Where(c => c.OwningFaction.IsPlayer).ToList();

                        //foreach (Creature player in party) {
                        //    self.Owner.DetectionStatus.HiddenTo.Add(player);
                        //}
                        //self.Owner.DetectionStatus.Undetected = true;
                        //List<Tile> spawnPoints = self.Owner.Battle.Encounter.Map.AllTiles.Where(t => {
                        //    if (!t.IsFree) {
                        //        return false;
                        //    }

                        //    foreach (Creature pc in self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsPlayer)) {
                        //        if (pc.DistanceTo(t) < 4) {
                        //            return false;
                        //        }
                        //    }
                        //    return true;
                        //}).ToList();

                        //Tile location = spawnPoints[R.Next(0, spawnPoints.Count)];
                        //self.Owner.Occupies = location;
                        //if (!location.IsTrulyGenuinelyFreeTo(self.Owner)) {
                        //    location = location.GetShuntoffTile(self.Owner);
                        //}
                        //self.Owner.TranslateTo(location);

                        List<Creature> party = self.Owner.Battle.AllCreatures.Where(c => c.OwningFaction.IsHumanControlled).ToList();
                        Creature target = party.OrderBy(c => c.HP / 100 * c.Defenses.GetBaseValue(Defense.AC) * 5).ToList()[0];

                        // TODO: Set so that lurking ends after taking their bonus turn
                        self.Owner.AddQEffect(new QEffect {
                            Id = QEffectIds.Lurking,
                            PreventTakingAction = action => action.ActionId != ActionId.Sneak ? "Stalking prey, cannot act." : null,
                            BonusToSkillChecks = (skill, action, target) => {
                                if (skill == Skill.Stealth && action.Name == "Sneak") {
                                    return new Bonus(20, BonusType.Status, "Lurking");
                                }
                                return null;
                            },
                            ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
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
                .WithAdditionalUnarmedStrike(new Item(IllustrationName.FourWinds, "Slicing Wind", new Trait[] { Trait.Ranged, Trait.Air, Trait.Magical }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing) {
                    VfxStyle = new VfxStyle(5, ProjectileKind.Cone, IllustrationName.FourWinds),
                    Sfx = SfxName.AeroBlade
                }.WithRangeIncrement(4)))
            );
            ModManager.RegisterNewCreature("Unseen Guardian", Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN]);


            // CREATURE - Drow Assassin
            Creatures.Add(ModEnums.CreatureId.DROW_ASSASSIN,
                encounter => new Creature(Illustrations.DrowAssassin, "Drow Assassin", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid }, 1, 7, 6, new Defenses(18, 4, 10, 7), 18, new Abilities(-1, 4, 1, 2, 2, 1), new Skills(stealth: 10, acrobatics: 7))
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
                    Id = QEffectId.SwiftSneak,
                    BonusToInitiative = self => new Bonus(-20, BonusType.Untyped, "Patient Stalker")
                })
                .AddQEffect(new QEffect("Shadowsilk Cloak", "Target can always attempt to sneak or hide, even when unobstructed.") {
                    Id = QEffectId.HideInPlainSight,
                    Innate = true,
                    StartOfCombat = async self => {
                        List<Creature> party = self.Owner.Battle.AllCreatures.Where(c => c.OwningFaction.IsHumanControlled).ToList();
                        Creature target = party.OrderBy(c => c.HP / 100 * c.Defenses.GetBaseValue(Defense.AC) * 5).ToList()[0];

                        // TODO: Set so that lurking ends after taking their bonus turn
                        self.Owner.AddQEffect(new QEffect {
                            Id = QEffectIds.Lurking,
                            PreventTakingAction = action => action.ActionId != ActionId.Sneak ? "Stalking prey, cannot act." : null,
                            //StateCheck = self => {
                            //    if (!self.Owner.DetectionStatus.Undetected) {
                            //        QEffect startled = QEffect.Stunned(2);
                            //        startled.Illustration = IllustrationName.DazzlingFlash;
                            //        startled.Name = "Startled";
                            //        startled.Description = "The assassin is startled by their premature discovery.\nAt the beginning of their next turn, they will lose 2 actions.\n\nThey can't take reactions.";
                            //        self.Owner.Occupies.Overhead("*startled!*", Color.Black);
                            //        self.Owner.AddQEffect(startled);
                            //        Sfxs.Play(SfxName.DazzlingFlash);
                            //        self.ExpiresAt = ExpirationCondition.Immediately;
                            //    }
                            //},
                            BonusToSkillChecks = (skill, action, target) => {
                                if (skill == Skill.Stealth && action.Name == "Sneak") {
                                    return new Bonus(7, BonusType.Status, "Lurking");
                                }
                                return null;
                            },
                            ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
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
            encounter => new Creature(Illustrations.DrowFighter, "Drow Fighter", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid }, 1, 5, 6, new Defenses(15, 4, 10, 7), 18,
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
                        foreach (Option option in options.Where(opt => opt.Text == "Reload" || opt.AiUsefulness.ObjectiveAction != null && opt.AiUsefulness.ObjectiveAction.Action.Name == "Reload")) {
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
                        float percentage = dc - (target.Defenses.GetBaseValue(Defense.Fortitude) + 10.5f);
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
            encounter => new Creature(Illustrations.DrowShootist, "Drow Shootist", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid }, 1, 10, 6, new Defenses(15, 4, 10, 7), 18,
            new Abilities(-1, 4, 1, 1, 2, 2), new Skills(acrobatics: 7, stealth: 7, deception: 7, intimidation: 5))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;
                    foreach (Option option in options.Where(opt => opt.Text == "Reload" || opt.AiUsefulness.ObjectiveAction != null && opt.AiUsefulness.ObjectiveAction.Action.Name == "Reload")) {
                        option.AiUsefulness.MainActionUsefulness = 0f;
                    }
                    return null;
                };
            })
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

                    CombatAction action = new CombatAction(self.Owner, new SideBySideIllustration(IllustrationName.HandCrossbow, IllustrationName.HandCrossbow), "Reloading Trick", new Trait[] { Trait.Manipulate }, "The Drow Shootist relaods both of their hand crossbows", Target.Self((cr, ai) => 15))
                    .WithActionCost(1)
                    .WithSoundEffect(SfxName.OpenLock)
                    .WithEffectOnSelf(user => {
                        xbow1.EphemeralItemProperties.NeedsReload = false;
                        xbow2.EphemeralItemProperties.NeedsReload = false;
                    })
                    ;
                    return (ActionPossibility)action;
                }
            })
            );

            ModManager.RegisterNewCreature("Drow Shootist", Creatures[ModEnums.CreatureId.DROW_SHOOTIST]);


            // CREATURE - Drow Sniper
            Creatures.Add(ModEnums.CreatureId.DROW_SNIPER,
            encounter => new Creature(Illustrations.DrowSniper, "Drow Sniper", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid }, 1, 10, 6, new Defenses(15, 4, 10, 7), 18,
            new Abilities(-1, 4, 1, 1, 2, 2), new Skills(acrobatics: 7, stealth: 7, deception: 7, intimidation: 5))
            .AddQEffect(CommonQEffects.Drow())
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Melee, Proficiency.Trained)
            .WithProficiency(Trait.Ranged, Proficiency.Master)
            .AddHeldItem(Items.CreateNew(ItemName.Longbow))
            .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "longbow"))
            );
            ModManager.RegisterNewCreature("Drow Sniper", Creatures[ModEnums.CreatureId.DROW_SNIPER]);


            // CREATURE - Drow Priestess
            Creatures.Add(ModEnums.CreatureId.DROW_PRIESTESS,
            encounter => new Creature(Illustrations.DrowPriestess, "Drow Priestess", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid, Trait.Female }, 3, 9, 6, new Defenses(20, 8, 7, 11), 39,
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
                        (int, bool) temp = ((int, bool))bane.Tag;
                        int radius = temp.Item1;

                        expandBane.AiUsefulness.MainActionUsefulness = 0f;
                        foreach (Creature enemy in creature.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction) && creature.DistanceTo(cr.Occupies) == radius + 1)) {
                            expandBane.AiUsefulness.MainActionUsefulness += 4;
                        }
                    }

                    // Demoralize AI
                    foreach (Option option in options.Where(o => o.Text == "Demoralize" || o.AiUsefulness.ObjectiveAction != null && o.AiUsefulness.ObjectiveAction.Action.ActionId == ActionId.Demoralize)) {
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
            .WithProficiency(Trait.Weapon, Proficiency.Trained)
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
            encounter => new Creature(Illustrations.DrowTempleGuard, "Drow Temple Guard", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid }, 2, 8, 6, new Defenses(18, 11, 8, 9), 28,
            new Abilities(4, 2, 3, 0, 2, 0), new Skills(athletics: 8, intimidation: 6))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (t, _, _, cr) => cr.HasEffect(QEffectIds.DrowClergy) && t.DistanceTo(cr.Occupies) <= 3, 2f);
                    AiFuncs.PositionalGoodness(monster, options, (t, _, _, cr) => cr.HasEffect(QEffectIds.DrowClergy) && t.DistanceTo(cr.Occupies) <= 2, 1f);

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
            encounter => new Creature(Illustrations.HuntingSpider, "Hunting Spider", new List<Trait>() { Trait.Animal, ModTraits.Spider }, 1, 7, 5, new Defenses(17, 6, 9, 5), 16,
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
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "fangs"))
            .AddQEffect(CommonQEffects.WebAttack(16))
            );
            ModManager.RegisterNewCreature("Hunting Spider", Creatures[ModEnums.CreatureId.HUNTING_SPIDER]);


            // CREATURE - Drider
            Creatures.Add(ModEnums.CreatureId.DRIDER,
            encounter => new Creature(Illustrations.Drider, "Drider", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Aberration, ModTraits.Spider, Trait.Female }, 3, 6, 6, new Defenses(17, 12, 7, 6), 56,
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
            encounter => new Creature(Illustrations.DrowArcanist, "Drow Arcanist", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid }, 1, 7, 6, new Defenses(15, 4, 7, 10), 14,
            new Abilities(1, 3, 0, 4, 1, 1), new Skills(acrobatics: 10, intimidation: 6, arcana: 8, deception: 8))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;
                    AiFuncs.PositionalGoodness(creature, options, (t, crSelf, step, cr) => (step || crSelf.Occupies == t) && t.IsAdjacentTo(cr.Occupies) && cr.OwningFaction.EnemyFactionOf(crSelf.OwningFaction), -0.2f, false);
                    AiFuncs.AverageDistanceGoodness(creature, options, cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction), cr => cr.OwningFaction.AlliedFactionOf(creature.OwningFaction), -15, 5);

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
                    if (!(action.HasTrait(Trait.Melee) || action.Owner != null && action.Owner.IsAdjacentTo(self.Owner))) {
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
            encounter => new Creature(Illustrations.DrowShadowcaster, "Drow Shadowcaster", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid }, 3, 9, 6, new Defenses(17, 6, 9, 12), 31,
            new Abilities(1, 3, 0, 4, 1, 2), new Skills(acrobatics: 10, intimidation: 11, arcana: 13, deception: 11))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;
                    AiFuncs.PositionalGoodness(creature, options, (t, crSelf, step, cr) => (step || crSelf.Occupies == t) && t.IsAdjacentTo(cr.Occupies) && cr.OwningFaction.EnemyFactionOf(crSelf.OwningFaction), -0.2f, false);
                    AiFuncs.AverageDistanceGoodness(creature, options, cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction), cr => cr.OwningFaction.AlliedFactionOf(creature.OwningFaction), -15, 5);

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
                    if (!(action.HasTrait(Trait.Melee) || action.Owner != null && action.Owner.IsAdjacentTo(self.Owner))) {
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
            .AddQEffect(new QEffect("Dark Arts", "The drow shadwcaster excels at causing pain with their black practice. Their non-cantrip spells gain a +4 status bonus to damage.") {
                BonusToDamage = (qfSelf, spell, target) => {
                    return spell.HasTrait(Trait.Spell) && !spell.HasTrait(Trait.Cantrip) && !spell.HasTrait(Trait.Focus) && spell.CastFromScroll == null ? new Bonus(4, BonusType.Status, "Dark Arts") : null;
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Wizard, Ability.Intelligence, Trait.Arcane).WithSpells(
            new SpellId[] { SpellId.MagicMissile, SpellId.MagicMissile, SpellId.GrimTendrils, SpellId.Fear, SpellId.ChillTouch, SpellId.ProduceFlame, SpellId.Shield },
            new SpellId[] { SpellId.AcidArrow, SpellId.AcidArrow, SpellId.HideousLaughter }).Done()
            );
            ModManager.RegisterNewCreature("Drow Shadowcaster", Creatures[ModEnums.CreatureId.DROW_SHADOWCASTER]);


            // CREATURE - Drow Inquisitrix
            string icDmg = "1d8";
            Creatures.Add(ModEnums.CreatureId.DROW_INQUISITRIX,
            encounter => new Creature(Illustrations.DrowInquisitrix, "Drow Inquisitrix", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid, Trait.Female }, 2, 8, 6, new Defenses(17, 5, 8, 11), 25,
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


            // CREATURE - Loot Imp
            Creatures.Add(ModEnums.CreatureId.TREASURE_DEMON,
            encounter => new Creature(Illustrations.TreasureDemon, "Treasure Demon", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Fiend, Trait.Demon, Trait.NoPhysicalUnarmedAttack }, 2, 8, 5, new Defenses(17, 5, 8, 11), 25,
            new Abilities(-1, 4, 0, 3, 1, -1), new Skills(acrobatics: 8, thievery: 10))
            .WithBasicCharacteristics()
            .AddQEffect(new QEffect("Treasure Hoarder",
            $"Treasure Demons hop between dimensions, often travelling through the safety of the {Loader.UnderdarkName} to endow the demon lord's mortal servants with funds for their foul schemes. Kill it before it escapes to steal its delivery for yourselves.") {
                Id = QEffectId.FleeingAllDanger
            })
            .AddQEffect(new QEffect("Emergency Planeshift", "When this condition expires, the treasure demon will teleport to safety along with its loot.", ExpirationCondition.CountsDownAtEndOfYourTurn, null, IllustrationName.DimensionDoor) {
                Value = 3,
                WhenExpires = async self => {
                    if (self.Value == 0) {
                        self.Owner.Battle.SmartCenter(self.Owner.Occupies.X, self.Owner.Occupies.Y);
                        self.Owner.Occupies.Overhead($"Escaped!", Color.Black, "The treasure demon escaped with its loot!");
                        self.Owner.AnimationData.ColorBlink(Color.White);
                        Sfxs.Play(SfxName.SpellFail);
                        self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                    }
                },
                WhenCreatureDiesAtStateCheckAsync = async self => {
                    if (self.Owner.Battle.CampaignState == null) {
                        return;
                    }
                    int amount = 0;
                    for (int i = 0; i < self.Owner.Battle.Encounter.CharacterLevel; i++) {
                        amount += R.NextD20();
                    }
                    self.Owner.Occupies.Overhead($"{amount} gold", Color.Goldenrod, "The party looted {b}" + amount + " gold{/b} from the treasure demon.");
                    self.Tag = amount;
                },
                EndOfCombat = async (self, victory) => {
                    if (victory && self.Tag != null) {
                        self.Owner.Battle.CampaignState.CommonGold += (int)self.Tag;
                    }
                }
            })
            );
            ModManager.RegisterNewCreature("Treasure Demon", Creatures[ModEnums.CreatureId.TREASURE_DEMON]);

            // CREATURE - Loot Imp
            Creatures.Add(ModEnums.CreatureId.DROW_RENEGADE,
            encounter => {
                Creature creature = new Creature(Illustrations.DrowRenegade, "Drow Renegade", new List<Trait>() { Trait.Good, Trait.Elf, Trait.Humanoid, Trait.Female, ModTraits.Drow }, 1, 7, 5, new Defenses(16, 10, 7, 7), 25,
                new Abilities(4, 2, 3, 1, 1, 2), new Skills(deception: 7, athletics: 9)) {
                    SpawnAsFriends = true
                }
                .WithBasicCharacteristics()
                .WithProficiency(Trait.Melee, Proficiency.Expert)
                .AddHeldItem(Items.CreateNew(ItemName.Greatsword))
                .AddQEffect(QEffect.AttackOfOpportunity())
                .AddQEffect(CommonQEffects.Drow())
                .AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.Moonbeam, "Crescent Moon Strike", new Trait[] { Trait.Magical, Trait.Divine }, "...", Target.Cone(5).WithIncludeOnlyIf((area, cr) => cr.OwningFaction.IsEnemy)) {
                            ShortDescription = "Deal 3d6 fire damage (basic Reflex save) to each enemy creature within a 25ft cone. On a critical failure, targets are dazzled for 1 round. The Drow Renegade cannot use this attack again for 1d4 rounds."
                        }
                        .WithSavingThrow(new SavingThrow(Defense.Reflex, cr => cr.Level + cr.Abilities.Charisma + 4 + 10))
                        .WithActionCost(2)
                        .WithProjectileCone(IllustrationName.Moonbeam, 15, ProjectileKind.Cone)
                        .WithSoundEffect(SfxName.DivineLance)
                        .WithEffectOnEachTarget(async (spell, user, defender, result) => {
                            await CommonSpellEffects.DealBasicDamage(spell, user, defender, result, DiceFormula.FromText("3d6", "Crescent Moon Strike"), DamageKind.Fire);
                            if (result == CheckResult.CriticalFailure) {
                                defender.AddQEffect(QEffect.Dazzled().WithExpirationAtStartOfSourcesTurn(user, 1));
                            }
                        })
                        .WithEffectOnSelf(user => {
                            user.AddQEffect(QEffect.Recharging("Crescent Moon Strike"));
                            //user.AddQEffect(QEffect.CannotUseForXRound("Crescent Moon Strike", user, DiceFormula.FromText("1d4").Roll().Item1 + 1));
                        })
                        .WithGoodnessAgainstEnemy((cone, a, d) => {
                            return 3.5f * 3 + (d.QEffects.FirstOrDefault(qf => qf.Name == "Dazzled" || qf.Id == QEffectId.Blinded) == null ? 2f : 0f);
                        })
                        ;
                    }
                });
                return creature;
            });


            ModManager.RegisterNewCreature("Drow Renegade", Creatures[ModEnums.CreatureId.DROW_RENEGADE]);

            // CREATURE - Witch Crone
            Creatures.Add(ModEnums.CreatureId.WITCH_CRONE,
            encounter => new Creature(IllustrationName.SwampHag, "Agatha Agaricus", new List<Trait>() { Trait.Neutral, Trait.Evil, Trait.Human, Trait.Tiefling, Trait.Humanoid, ModTraits.Witch, Trait.Female }, 3, 4, 5, new Defenses(17, 9, 6, 12), 60,
            new Abilities(2, 2, 3, 4, 2, 0), new Skills(nature: 13, occultism: 9, intimidation: 10, religion: 9))
            .WithProficiency(Trait.Unarmed, Proficiency.Trained)
            .WithProficiency(Trait.BattleformAttack, Proficiency.Expert)
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
                    await summon.Battle.Cinematics.PlayCutscene(async cin => {
                        cin.EnterCutscene();
                        summon.Battle.SmartCenter(summon.Occupies.X, summon.Occupies.Y);
                        Sfxs.Play(SfxName.BeastRoar, 0.75f);
                        summon.Occupies.Overhead("*Curse of Skittering Paws*", Color.White, $"{summon.Name} is drawn to aide the coven by the curse of skittering paws.");
                        await cin.WaitABit();
                        cin.ExitCutscene();
                    });
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
                                    properties.WithOnTarget(async (strike, a, d, result) => {
                                        if (result >= CheckResult.Success)
                                            await Possibilities.Grapple(a, d, result);
                                    });
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
                            Sfxs.Play(SfxName.BeastRoar, 1.33f);
                            self.Owner.AddQEffect(transform);
                            break;
                        default:
                            break;
                    }
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, ModTraits.Witch, Ability.Intelligence, Trait.Primal).WithSpells(
                level1: new SpellId[] { SpellId.Heal, SpellId.PummelingRubble, SpellId.PummelingRubble },
                level2: new SpellId[] { SpellId.Barkskin }).Done()
            );
            ModManager.RegisterNewCreature("Witch Crone", Creatures[ModEnums.CreatureId.WITCH_CRONE]);


            // CREATURE - Witch Mother
            Creatures.Add(ModEnums.CreatureId.WITCH_MOTHER,
            encounter => new Creature(IllustrationName.WaterElemental256, "Mother Cassandra", new List<Trait>() { Trait.Neutral, Trait.Evil, Trait.Human, Trait.Tiefling, Trait.Humanoid, ModTraits.Witch, Trait.Female }, 2, 4, 5, new Defenses(16, 8, 5, 11), 40,
            new Abilities(0, 2, 3, 4, 2, 0), new Skills(nature: 9, occultism: 13, intimidation: 8))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (tile, _, _, cr) => cr.HasTrait(ModTraits.Witch) && cr.DistanceTo(tile) <= 3, 1.5f, false);
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
            .AddSpellcastingSource(SpellcastingKind.Prepared, ModTraits.Witch, Ability.Intelligence, Trait.Divine).WithSpells(
                level1: new SpellId[] { SpellId.GrimTendrils, SpellId.Heal, SpellId.Heal }).Done()
            );
            ModManager.RegisterNewCreature("Witch Mother", Creatures[ModEnums.CreatureId.WITCH_MOTHER]);


            // CREATURE - Witch Maiden
            Creatures.Add(ModEnums.CreatureId.WITCH_MAIDEN,
            encounter => new Creature(IllustrationName.SuccubusShapeshifted, "Harriet Hex", new List<Trait>() { Trait.Neutral, Trait.Evil, Trait.Human, Trait.Tiefling, Trait.Humanoid, ModTraits.Witch, Trait.Female }, 2, 6, 5, new Defenses(15, 5, 8, 11), 30,
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
            .AddSpellcastingSource(SpellcastingKind.Prepared, ModTraits.Witch, Ability.Intelligence, Trait.Arcane).WithSpells(
                level1: new SpellId[] { SpellId.ChillTouch, SpellId.TrueStrike, SpellId.FlourishingFlora, SpellId.FlourishingFlora },
                level2: new SpellId[] { SpellId.TrueStrike, SpellId.TrueStrike }).Done()
            );
            ModManager.RegisterNewCreature("Witch Maiden", Creatures[ModEnums.CreatureId.WITCH_MAIDEN]);


            // CREATURE - Ravenous Rat
            Creatures.Add(ModEnums.CreatureId.RAVENOUS_RAT,
            encounter => {
                Creature monster = MonsterStatBlocks.CreateGiantRat();
                monster.MainName = "Ravenous Rat";
                monster.AddQEffect(QEffect.PackAttack("ravenous rat", "1d4"));
                monster.WithTactics(Tactic.PackAttack);
                return monster;
            }
            );
            ModManager.RegisterNewCreature("Ravenous Rat", Creatures[ModEnums.CreatureId.RAVENOUS_RAT]);

            // Add new creature here

        }

        internal static void LoadObjects() {
            // HAZARD - Deep Mushroom
            Objects.Add(ObjectId.CHOKING_MUSHROOM,
                encounter => {
                    QEffect qfCurrentDC = new QEffect() { Value = 15 };

                    Creature hazard = new Creature(Illustrations.ChokingMushroom, "Choking Mushroom", new List<Trait>() { Trait.Object, Trait.Plant }, 2, 0, 0, new Defenses(10, 10, 0, 0), 20, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
                    .WithTactics(Tactic.DoNothing)
                    .WithEntersInitiativeOrder(false)
                    .AddQEffect(qfCurrentDC)
                    .AddQEffect(CommonQEffects.Hazard())
                    .AddQEffect(new QEffect("Choking Spores", "This predatory mushroom exhudes a cloud of poisonous spores to suffocate its prey. Creatures walking through the spores suffer 1d4 poison damage vs. a DC 17 Basic fortitude save, and become sickened 1 on a critical failure."))
                    ;

                    QEffect effect = new QEffect("Interactable", "You can use Medicine, Nature and Occultism to interact with this mushroom.") {
                        StateCheckWithVisibleChanges = async self => {
                            if (!self.Owner.Alive) {
                                return;
                            }

                            self.Owner.WeaknessAndResistance.AddWeakness(DamageKind.Fire, 5);
                            self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Piercing, 5);

                            if (self.Tag == null) {
                                self.Tag = self.Owner.Battle.Map.AllTiles.Where(t => t.DistanceTo(self.Owner.Occupies) <= 2 && !new TileKind[] { TileKind.BlocksMovementAndLineOfEffect, TileKind.Tree, TileKind.Rock, TileKind.Wall }.Contains(t.Kind)).ToList();
                            }

                            // Add contextual actions
                            foreach (Creature hero in self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsHumanControlled && cr.IsAdjacentTo(self.Owner))) {
                                hero.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                                    ProvideContextualAction = qfContextActions => {
                                        return new SubmenuPossibility(Illustrations.ChokingMushroom, "Interactions") {
                                            Subsections = {
                                                new PossibilitySection(hazard.Name) {
                                                    Possibilities = {
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, Illustrations.ChokingMushroom, "Soothe Mushroom", new Trait[] { Trait.Manipulate, Trait.Basic },
                                                        "Folktales speak of ancient rites and traditions used by cavern folk to appease the mushroom forests. Make an Occultism check against DC " + qfCurrentDC.Value + "." + S.FourDegreesOfSuccess("The mushroom will stop emitting spores for the rest of the encounter.",
                                                        "The mushroom will stop emitting spores for 2 rounds.", null, "You take 1d6 poison damage."),
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(1)
                                                        .WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Occultism), Checks.FlatDC(qfCurrentDC.Value)))
                                                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                            if (result == CheckResult.CriticalFailure) {
                                                                if (caster.FindQEffect(QEffectIds.MushroomInoculation) == null) {
                                                                    await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Choking Spores"), caster, CheckResult.Success, DamageKind.Poison);
                                                                }
                                                            }
                                                            if (result >= CheckResult.Success) {
                                                                foreach (Tile tile in self.Tag as List<Tile>) {
                                                                    if (tile.QEffects.Any(qf => qf.TileQEffectId == QEffectIds.ChokingSpores)) {
                                                                        tile.QEffects.RemoveAll(qf => qf.TileQEffectId == QEffectIds.ChokingSpores);
                                                                    }
                                                                }
                                                            }
                                                            if (result == CheckResult.Success) {
                                                                target.AddQEffect(new QEffect("Soothed", "The choking mushroom has stopped emitting spores.") {
                                                                    Value = 2,
                                                                    Id = QEffectId.Recharging,
                                                                    ExpiresAt = ExpirationCondition.CountsDownAtStartOfSourcesTurn,
                                                                    Source = caster,
                                                                    Illustration = IllustrationName.Soothe,
                                                                    Innate = false
                                                                });
                                                            }
                                                            if (result == CheckResult.CriticalSuccess) {
                                                                target.AddQEffect(new QEffect("Soothed", "The choking mushroom has stopped emitting spores.") {
                                                                    Value = 100,
                                                                    Id = QEffectId.Recharging,
                                                                    ExpiresAt = ExpirationCondition.Never,
                                                                    Source = caster,
                                                                    Illustration = IllustrationName.Soothe,
                                                                    Innate = false
                                                                });
                                                            }
                                                            await hazard.Battle.GameLoop.StateCheck();
                                                        }),
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, Illustrations.ChokingMushroom, "Recall Knowledge", new Trait[] { Trait.Manipulate, Trait.Basic },
                                                        "Make a Nature check against DC " + qfCurrentDC.Value + "." + S.FourDegreesOfSuccess("Reduce all DCs on this font by 3.", "Reduce all DCs on this font by 2.", null, "You take 1d6 poison damage and increase all DCs on this font by 1."),
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(1)
                                                        .WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Nature), Checks.FlatDC(qfCurrentDC.Value - 2)))
                                                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                            if (result == CheckResult.CriticalFailure) {
                                                                if (caster.FindQEffect(QEffectIds.MushroomInoculation) == null) {
                                                                    await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Choking Spores"), caster, CheckResult.Success, DamageKind.Poison);
                                                                }
                                                                qfCurrentDC.Value += 1;
                                                            }
                                                            if (result == CheckResult.Success) {
                                                                qfCurrentDC.Value -= 2;
                                                            }
                                                            if (result == CheckResult.CriticalSuccess) {
                                                                qfCurrentDC.Value -= 3;
                                                            }
                                                        }),
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, IllustrationName.DashOfHerbs, "Harvest Pollen", new Trait[] { Trait.Manipulate, Trait.Basic, Trait.Alchemical },
                                                            "Make a Medicine check against DC " + qfCurrentDC.Value + "." + S.FourDegreesOfSuccess("Heal your or an adjacent ally for 3d6 HP and inoculate them against the effects of spore clouds. This mushroom then cannot be harvested from again.",
                                                            "As per a critical success, but you only heal for 2d6 HP.", null, "You take 1d6 poison damage."),
                                                            Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner))
                                                            .WithAdditionalConditionOnTargetCreature((a, d) => !a.HasFreeHand ? Usability.NotUsable("No free hand") : Usability.Usable)
                                                            .WithAdditionalConditionOnTargetCreature((a, d) => {
                                                                if (d.QEffects.Any(qf => qf.Id == QEffectIds.Harvested)) {
                                                                    return Usability.NotUsableOnThisCreature("Already harvested");
                                                                }
                                                                return Usability.Usable;
                                                            }))
                                                        .WithActionCost(1)
                                                        .WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Medicine), Checks.FlatDC(qfCurrentDC.Value)))
                                                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                            if (result == CheckResult.CriticalFailure) {
                                                                if (caster.FindQEffect(QEffectIds.MushroomInoculation) == null) {
                                                                    await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Choking Spores"), caster, CheckResult.Success, DamageKind.Poison);
                                                                }
                                                            }
                                                            if (result >= CheckResult.Success) {
                                                                //target.AddQEffect(new QEffect() { Id = QEffectIds.Harvested });
                                                                target.AddQEffect(new QEffect("Harvested", "Pollen cannot be harvested again", ExpirationCondition.Never, caster, IllustrationName.DashOfHerbs) { Id = QEffectIds.Harvested });
                                                                List<Option> options = new List<Option>();

                                                                foreach (Creature ally in caster.Battle.AllCreatures.Where(cr => cr.OwningFaction.AlliedFactionOf(caster.OwningFaction) && cr.DistanceTo(caster) <= 1)) {
                                                                    options.Add(new CreatureOption(ally, $"Heal {ally.Name} with pollen.", async() => {
                                                                        await ally.HealAsync(DiceFormula.FromText($"{(result == CheckResult.Success ? "2" : "3")}d6", "Harvest Mushroom"), CombatAction.CreateSimple(target));
                                                                        ally.Occupies.Overhead("*healed*", Color.Green, $"{ally.Name} was healed by {caster.Name} using healing pollen.");
                                                                        ally.AddQEffect(new QEffect("Inoculating Spores", "You're inoculated against the effects of the choking spores.", ExpirationCondition.Never, self.Owner, new SameSizeDualIllustration(Illustrations.StatusBackdrop, Illustrations.ChokingMushroom)) {
                                                                            Id = QEffectIds.MushroomInoculation
                                                                        });
                                                                    }, 7, true));
                                                                }
                                                                var chosenOption = (await caster.Battle.SendRequest(new AdvancedRequest(caster, "Select an ally to heal with the nectar.", options) {
                                                                    IsMainTurn = false,
                                                                    TopBarIcon = IllustrationName.DashOfHerbs,
                                                                    TopBarText = "Select an ally to heal with the nectar."
                                                                })).ChosenOption;
                                                                await chosenOption.Action();
                                                            }
                                                        }),
                                                    }

                                                }
                                            }
                                        };
                                    }
                                });
                            }

                            if (self.Owner.QEffects.Any(qf => qf.Id == QEffectId.Recharging)) {
                                return;
                            }

                            // Apply poison to tiles
                            foreach (Tile tile in self.Tag as List<Tile>) {
                                if (!tile.QEffects.Any(qf => qf.TileQEffectId == QEffectIds.ChokingSpores)) {
                                    TileQEffect spores = new TileQEffect(tile) {
                                        Name = "Choking Spores",
                                        VisibleDescription = "Toxic spores used by Choking Mushrooms to hunt for nutrients. Suffer 1d4 poison damage vs. a Basic (DC 17) fort save after entering or starting your turn within the spores, that inflicts sickened 1 on a critical failure.",
                                        TileQEffectId = QEffectIds.ChokingSpores,
                                        ExpiresAt = ExpirationCondition.Never,
                                        Illustration = IllustrationName.Fog,
                                        TransformsTileIntoHazardousTerrain = true,
                                        AfterCreatureBeginsItsTurnHere = async victim => {
                                            if (victim.IsImmuneTo(Trait.Poison) || victim.WeaknessAndResistance.Immunities.Contains(DamageKind.Poison) || victim.FindQEffect(QEffectIds.MushroomInoculation) != null) {
                                                return;
                                            }
                                            CheckResult result = CommonSpellEffects.RollSavingThrow(victim, CombatAction.DefaultCombatAction, Defense.Fortitude, 17);
                                            await CommonSpellEffects.DealBasicDamage(CombatAction.CreateSimple(hazard, "Choking Spores", Trait.Poison), hazard, victim, result, new KindedDamage(DiceFormula.FromText("1d4", "Choking Spores"), DamageKind.Poison));
                                            if (result == CheckResult.CriticalFailure) {
                                                victim.AddQEffect(QEffect.Sickened(1, 17));
                                            }
                                        },
                                        StateCheck = self => {
                                            self.Owner.FoggyTerrain = true;
                                        }
                                    };
                                    spores.AfterCreatureEntersHere = async victim => await spores.AfterCreatureBeginsItsTurnHere(victim);
                                    tile.AddQEffect(spores);
                                }
                            }
                        }
                    };

                    //effect.AddGrantingOfTechnical(cr => !cr.OwningFaction.IsHumanControlled, qfShroom => {
                    //    qfShroom.ai
                    //});

                    hazard.AddQEffect(effect);

                    return hazard;
                }
            );
            ModManager.RegisterNewCreature("Choking Mushroom", Objects[ObjectId.CHOKING_MUSHROOM]);


            // HAZARD - Explosive Mushroom
            Objects.Add(ObjectId.BOOM_SHROOM,
                encounter => {
                    Creature hazard = new Creature(Illustrations.BoomShroom, "Explosive Mushroom", new List<Trait>() { Trait.Object, Trait.Plant }, 2, 0, 0, new Defenses(10, 10, 0, 0), 20, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
                    .WithTactics(Tactic.DoNothing)
                    .WithEntersInitiativeOrder(false)
                    .AddQEffect(CommonQEffects.Hazard())
                    .AddQEffect(new QEffect("Combustible Spores",
                    "This mushroom expels a cloud of highly reactive spores. Upon taking fire damage, the spores ignite in a devastating chain reaction, dealing 4d6 fire damage vs. a DC 17 Basic reflex save to each creature within a 2 tile radius.") {
                        AfterYouTakeDamageOfKind = async (self, action, kind) => {
                            string name = self.Owner.Name;

                            if (!self.UsedThisTurn && (kind == DamageKind.Fire || action != null && action.HasTrait(Trait.Fire))) {
                                CombatAction explosion = new CombatAction(self.Owner, IllustrationName.Fireball, "Combustible Spores", new Trait[] { Trait.Fire }, "", Target.SelfExcludingEmanation(2))
                                .WithSoundEffect(SfxName.Fireball)
                                .WithSavingThrow(new SavingThrow(Defense.Reflex, 17))
                                .WithProjectileCone(IllustrationName.Fireball, 15, ProjectileKind.Cone)
                                .WithEffectOnEachTarget(async (spell, a, d, r) => {
                                    await CommonSpellEffects.DealBasicDamage(spell, a, d, r, DiceFormula.FromText("4d6", "Combustible Spores"), DamageKind.Fire);
                                })
                                ;
                                self.Owner.Battle.AllCreatures.Where(cr => cr != self.Owner && cr.DistanceTo(self.Owner.Occupies) <= 2 && cr.HasLineOfEffectTo(self.Owner.Occupies) < CoverKind.Blocked).ForEach(cr => explosion.ChosenTargets.ChosenCreatures.Add(cr));
                                await CommonAnimations.CreateConeAnimation(self.Owner.Battle, self.Owner.Occupies.ToCenterVector(), self.Owner.Battle.Map.AllTiles.Where(t => t.DistanceTo(self.Owner.Occupies) <= 2).ToList(), 15, ProjectileKind.Cone, IllustrationName.Fireball);
                                self.UsedThisTurn = true;
                                await explosion.AllExecute();
                                self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                                self.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        }
                    })
                    ;

                    return hazard;
                }
            );
            ModManager.RegisterNewCreature("Explosive Mushroom", Objects[ObjectId.BOOM_SHROOM]);


            // HAZARD - Spider Queen Shrine
            Objects.Add(ObjectId.SPIDER_QUEEN_SHRINE,
                encounter => {
                    int radius = 2;
                    QEffect qfCurrentDC = new QEffect() { Value = 17 };

                    Creature hazard = new Creature(Illustrations.SpiderShrine, "Spider Queen Shrine", new List<Trait>() { Trait.Object }, 2, 0, 0, new Defenses(10, 10, 0, 0), 20, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
                    .WithTactics(Tactic.DoNothing)
                    .WithEntersInitiativeOrder(false)
                    .AddQEffect(CommonQEffects.Hazard())
                    .AddQEffect(qfCurrentDC)
                    .WithHardness(7)
                    ;

                    var animation = hazard.AnimationData.AddAuraAnimation(IllustrationName.KineticistAuraCircle, radius);
                    animation.Color = Color.Black;

                    QEffect effect = new QEffect("Blessings of the Spider Queen", $"All spiders and drow within {radius * 5}-feet of this shrine gain a +1 bonus to AC, saves and attacks rolls.");

                    QEffect interactable = new QEffect("Interactable", "You can use Religion, Thievery and Crafting to interact with this shrine.") {
                        StateCheckWithVisibleChanges = async self => {
                            // Add contextual actions
                            foreach (Creature hero in self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsHumanControlled && cr.IsAdjacentTo(self.Owner))) {
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
                                                        .WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Religion), Checks.FlatDC(qfCurrentDC.Value)))
                                                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                            if (result >= CheckResult.Success) {
                                                                target.RemoveAllQEffects(qf => qf.Name == "Blessings of the Spider Queen");
                                                                animation.MaximumOpacity = 0;
                                                            }
                                                        }),
                                                        (ActionPossibility)new CombatAction(qfContextActions.Owner, Illustrations.SpiderShrine, "Recall Knowledge", new Trait[] { Trait.Manipulate, Trait.Basic },
                                                        "Make a Religion or Crafting check against DC " + qfCurrentDC.Value + "." + S.FourDegreesOfSuccess("Reduce all DCs on this font by 3.",
                                                        "Reduce all DCs on this font by 2.", null, "You take 1d6 evil damage and increase all DCs on this font by 1."),
                                                        Target.AdjacentCreature().WithAdditionalConditionOnTargetCreature(new SpecificCreatureTargetingRequirement(self.Owner)))
                                                        .WithActionCost(1)
                                                        .WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Religion, Skill.Crafting), Checks.FlatDC(qfCurrentDC.Value - 2)))
                                                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                                            if (result == CheckResult.CriticalFailure) {
                                                                if (caster.FindQEffect(QEffectIds.MushroomInoculation) == null) {
                                                                    await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Spider Queen's Wrath"), caster, CheckResult.Success, DamageKind.Evil);
                                                                }
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
                                                        .WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Thievery), Checks.FlatDC(qfCurrentDC.Value)))
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

                    effect.AddGrantingOfTechnical(cr => (cr.HasTrait(ModTraits.Spider) || cr.HasTrait(Drow)) && cr.DistanceTo(effect.Owner) <= radius, qfTechnical => {
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

                    effect.AddGrantingOfTechnical(cr => cr.HasTrait(ModTraits.Spider) || cr.HasTrait(ModTraits.Drow), qfTechnical => {
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
            );
            ModManager.RegisterNewCreature("Spider Queen Shrine", Objects[ObjectId.SPIDER_QUEEN_SHRINE]);
        }

        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        internal static class CommonMonsterActions {
            public static CombatAction CreateHide(Creature self) {
                return new CombatAction(self, (Illustration)IllustrationName.Hide, "Hide", new Trait[2] { Trait.Basic, Trait.AttackDoesNotTargetAC },
                    "Make one Stealth check against the Perception DCs of each enemy creature that can see you but that you have cover or concealment from. On a success, you become Hidden to that creature.",
                    Target.Self((cr, ai) => ai.HideSelf()).WithAdditionalRestriction(innerSelf => {
                        if (HiddenRules.IsHiddenFromAllEnemies(innerSelf, innerSelf.Occupies))
                            return "You're already hidden from all enemies.";
                        return !innerSelf.Battle.AllCreatures.Any(cr => cr.EnemyOf(innerSelf) && HiddenRules.HasCoverOrConcealment(innerSelf, cr)) ? "You don't have cover or concealment from any enemy." : null;
                    }))
                .WithActionId(ActionId.Hide)
                .WithSoundEffect(SfxName.Hide)
                .WithEffectOnSelf(innerSelf => {
                    int roll = R.NextD20();
                    foreach (Creature creature in innerSelf.Battle.AllCreatures.Where(cr => cr.EnemyOf(innerSelf))) {
                        if (!innerSelf.DetectionStatus.HiddenTo.Contains(creature) && HiddenRules.HasCoverOrConcealment(innerSelf, creature)) {
                            CheckBreakdown breakdown = CombatActionExecution.BreakdownAttack(new CombatAction(innerSelf, (Illustration)IllustrationName.Hide, "Hide", new Trait[1]
                            {
                    Trait.Basic
                            }, "[this condition has no description]", Target.Self()).WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Stealth), Checks.DefenseDC(Defense.Perception))), creature);
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
                                interpolatedStringHandler.AppendFormatted(breakdownResult.D20Roll + breakdown.TotalCheckBonus);
                                interpolatedStringHandler.AppendLiteral(" vs. ");
                                interpolatedStringHandler.AppendFormatted(breakdown.TotalDC);
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
                                interpolatedStringHandler.AppendFormatted(breakdownResult.D20Roll + breakdown.TotalCheckBonus);
                                interpolatedStringHandler.AppendLiteral(" vs. ");
                                interpolatedStringHandler.AppendFormatted(breakdown.TotalDC);
                                interpolatedStringHandler.AppendLiteral(").");
                                string stringAndClear = interpolatedStringHandler.ToStringAndClear();
                                string log = str11 + " failed to hide from " + str12 + stringAndClear;
                                string logDetails = str8;
                                occupies.Overhead("hide failed", red, log, "Hide", logDetails);
                            }
                        }
                    }
                });
            }

            // Insert new actions here
        }
    }
}
