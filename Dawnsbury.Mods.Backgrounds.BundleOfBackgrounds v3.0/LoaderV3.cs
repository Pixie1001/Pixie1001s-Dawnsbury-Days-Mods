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
using System.Reflection.Metadata;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Microsoft.Xna.Framework;
using System.Text;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Specific;
using System.Xml;
using Dawnsbury.Campaign.Path;

namespace Dawnsbury.Mods.Backgrounds.BundleOfBackgrounds {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class FeatNames {
        public static Dictionary<FeatId, FeatName> feats = new Dictionary<FeatId, FeatName>();

        public enum FeatId {
            UNDERWATER_MARAUDER,
            SLIPPERY_PREY,
            ESCAPE_ARTIST,
            HEFTY_HAULER,
            PILGRIMS_TOKEN,
            FOUNT_OF_KNOWLEDGE,
            NO_CAUSE_FOR_ALARM,
            THEATRICAL_DISTRACTION,
            SHARPENED_SENSES,
            SNAKE_OIL
        }

        internal static void RegisterFeatNames() {
            feats.Add(FeatId.UNDERWATER_MARAUDER, ModManager.RegisterFeatName("Underwater Marauder"));
            feats.Add(FeatId.SLIPPERY_PREY, ModManager.RegisterFeatName("Slippery Prey"));
            feats.Add(FeatId.ESCAPE_ARTIST, ModManager.RegisterFeatName("Escape Artist"));
            feats.Add(FeatId.HEFTY_HAULER, ModManager.RegisterFeatName("Hefty Hauler"));
            feats.Add(FeatId.PILGRIMS_TOKEN, ModManager.RegisterFeatName("Pilgrim's Token"));
            feats.Add(FeatId.FOUNT_OF_KNOWLEDGE, ModManager.RegisterFeatName("Fount of Knowledge"));
            feats.Add(FeatId.NO_CAUSE_FOR_ALARM, ModManager.RegisterFeatName("No Cause for Alarm {icon:TwoActions}"));
            feats.Add(FeatId.THEATRICAL_DISTRACTION, ModManager.RegisterFeatName("Theatrical Distraction"));
            feats.Add(FeatId.SHARPENED_SENSES, ModManager.RegisterFeatName("Sharpened Senses"));
            feats.Add(FeatId.SNAKE_OIL, ModManager.RegisterFeatName("Snake Oil"));
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class LoaderV3 {
        //internal static Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>> Creatures = new Dictionary<ModEnums.CreatureId, Func<Encounter?, Creature>>();
        // Action IDs
        internal static ActionId actNoCauseForAlarm = ModManager.RegisterEnumMember<ActionId>("No Cause For Alarm");
        internal static Trait tRare = ModManager.RegisterTrait("BoBRareTrait", new TraitProperties("Rare", true) { BackgroundColor = new Color(12, 20, 102), WhiteForeground = true });
        internal static Trait tBoB = ModManager.RegisterTrait("BoB", new TraitProperties("Bundle of Backgrounds", true) { });
        internal static ItemName iDragonWhisky = ModManager.RegisterEnumMember<ItemName>("BoB Dragon Whisky");
        internal static ItemName iRotgut = ModManager.RegisterEnumMember<ItemName>("BoB Rotgut");
        internal static ItemName iBerserkersBrew = ModManager.RegisterEnumMember<ItemName>("BoB Berserker's Brew");

        [DawnsburyDaysModMainMethod]
        public static void LoadMod() {
            FeatNames.RegisterFeatNames();
            BoBAssets.RegisterIllustrations();
            AddFeats(CreateGeneralFeats());
            AddFeats(CreateBackgrounds());
        }

        private static void AddFeats(IEnumerable<Feat> feats) {
            foreach (Feat feat in feats) {
                feat.Traits.Add(tBoB);
                ModManager.AddFeat(feat);
            }
        }

        private static IEnumerable<Feat> CreateGeneralFeats() {
            yield return new TrueFeat(FeatNames.feats[FeatNames.FeatId.UNDERWATER_MARAUDER], 1, "You've learned to fight underwater.", "You are not flat-footed while in water, and you don't take the usual penalties for using a bludgeoning or slashing melee weapon in water.", new Trait[] { Trait.General, Trait.Skill })
            .WithOnCreature((sheet, creature) => {
                creature.AddQEffect(new QEffect("Underwater Marauder", "You are not flat-footed while underwater, and don't take the usual penalties for using a bludgeoning or slashing melee weapon in water.") {
                    YouAcquireQEffect = (self, newEffect) => {
                        if (newEffect.Id == QEffectId.AquaticCombat && newEffect.Name != "Aquatic Combat (underwater marauder)") {
                            return new QEffect("Aquatic Combat (underwater marauder)", "You can't cast fire spells (but fire impulses still work).\nYou can't use slashing or bludgeoning ranged attacks.\nWeapon ranged attacks have their range increments halved.") {
                                Id = QEffectId.AquaticCombat,
                                DoNotShowUpOverhead = self.Owner.HasTrait(Trait.Aquatic),
                                Illustration = IllustrationName.ElementWater,
                                Innate = false,
                                StateCheck = (Action<QEffect>)(qfAquaticCombat =>
                                {
                                    if (qfAquaticCombat.Owner.HasTrait(Trait.Aquatic) || qfAquaticCombat.Owner.HasEffect(QEffectId.Swimming))
                                        return;
                                    qfAquaticCombat.Owner.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                                        Id = QEffectId.CountsAllTerrainAsDifficultTerrain
                                    });
                                }),
                                PreventTakingAction = (Func<CombatAction, string>)(action =>
                                {
                                    if (action.HasTrait(Trait.Impulse))
                                        return (string)null;
                                    if (action.HasTrait(Trait.Fire))
                                        return "You can't use fire actions underwater.";
                                    return action.HasTrait(Trait.Ranged) && action.HasTrait(Trait.Attack) && IsSlashingOrBludgeoning(action) ? "You can't use slashing or bludgeoning ranged attacks underwater." : (string)null;
                                })
                            };
                        }
                        return newEffect;
                    }
                });
            })
            .WithPrerequisite(sheet => {
                return sheet.Proficiencies.Get(Trait.Athletics) >= Proficiency.Trained;
            }, "You must be trained in Athletics");

            yield return new TrueFeat(FeatNames.feats[FeatNames.FeatId.SLIPPERY_PREY], 2, "You're able to escape bonds more easily than others.",
                "When you attempt to Escape using Acrobatics or Athletics, you reduce the multiple attack penalty for repeated attempts to –4 and –8 if you're trained in the skill. " +
                "The penalty becomes –3 and –6 if you're a master in the appropriate skill.", new Trait[] { Trait.General, Trait.Skill })
            .WithPermanentQEffect("Reduce the Escape DC from your MAP by 1 when using a skill. Increase by 2 if you have master proficiancy in the skill.", self => {
                self.BonusToSkillChecks = (skill, action, target) => {
                    if (action.ActionId != ActionId.Escape) {
                        return null;
                    }

                    int map = action.Owner.Actions.AttackedThisManyTimesThisTurn;
                    Trait tSkill = Trait.None;
                    if (skill == Skill.Acrobatics) {
                        tSkill = Trait.Acrobatics;
                    } else {
                        tSkill = Trait.Athletics;
                    }

                    if (map == 0 || action.Owner.Proficiencies.Get(tSkill) < Proficiency.Trained) {
                        return null;
                    }

                    int bonus = 1;
                    if (action.Owner.Proficiencies.Get(tSkill) >= Proficiency.Master) {
                        bonus = 2;
                    }

                    if (map == 1) {
                        return new Bonus(bonus, BonusType.Untyped, "Slippery Prey");
                    } else if (map > 1) {
                        return new Bonus(bonus * 2, BonusType.Untyped, "Slippery Prey");
                    }
                    return null;
                };
            })
            .WithPrerequisite(sheet => {
                return sheet.Proficiencies.Get(Trait.Athletics) >= Proficiency.Trained || sheet.Proficiencies.Get(Trait.Acrobatics) >= Proficiency.Trained;
            }, "You must be trained in Acrobatics or Athletics");

            yield return new TrueFeat(FeatNames.feats[FeatNames.FeatId.ESCAPE_ARTIST], 1, "You've a knack for slipping free from other's grasps.", "You gain a +1 circumstance bonus to escape checks.", new Trait[] { Trait.General, Trait.Homebrew })
            .WithPermanentQEffect("You gain a +1 circumstance bonus to escape checks.", self => {
                self.BonusToSkillChecks = (skill, action, target) => {
                    if (action.ActionId != ActionId.Escape) {
                        return null;
                    }

                    return new Bonus(1, BonusType.Circumstance, "Escape Artist", true);
                };
                self.BonusToAttackRolls = (self, action, target) => {
                    if (action.ActionId != ActionId.Escape) {
                        return null;
                    }

                    return new Bonus(1, BonusType.Circumstance, "Escape Artist", true);
                };
            });

            yield return new TrueFeat(FeatNames.feats[FeatNames.FeatId.HEFTY_HAULER], 1,
                "You can carry more than your frame implies.", "After each encounter, you're able to haul away additional miscellaneous treasures weaker parties would find too burdonsome to loot.\n\n" +
                "Increasing your gold reward by (5/8/10/12)% of the total value of the treasure collected from each encounter.\n\nThe amount of extra gold earned is dependent on the number of party " +
                "members with this feat - each giving increasingly diminishing returns as they struggle to scrounge for additional loot.", new Trait[] { Trait.General, Trait.Skill, Trait.Homebrew })
            .WithPermanentQEffect("Increase encounter reward by +(5/8/10/12)% gold, based on the number of party members with this feat and the total value of all item and gold rewards.", self => {
                self.StartOfCombat = async self => {
                    self.Tag = false;
                };
                self.EndOfCombat = async (self, won) => {
                    if (self.Tag == null) {
                        return;
                    }

                    bool disabled = (bool)self.Tag;
                    if (disabled) {
                        return;
                    }

                    int num = 0;
                    foreach (Creature adventurer in self.Owner.Battle.AllCreatures.Where(c => c.OwningFaction == self.Owner.OwningFaction)) {
                        QEffect? hh = adventurer.QEffects.FirstOrDefault(qf => qf.Name == FeatNames.feats[FeatNames.FeatId.HEFTY_HAULER].HumanizeTitleCase2());
                        if (hh != null) {
                            num += 1;
                            hh.Tag = true;
                        }
                    }

                    float multiplier = 0;
                    if (num == 1) {
                        multiplier = 0.05f;
                    } else if (num == 2) {
                        multiplier = 0.08f;
                    } else if (num == 3) {
                        multiplier = 0.1f;
                    } else if (num >= 4) {
                        multiplier = 0.12f;
                    }

                    int extraGold = 0;
                    foreach (Item loot in self.Owner.Battle.Encounter.Rewards) {
                        extraGold += loot.Price;
                    }
                    extraGold += self.Owner.Battle.Encounter.RewardGold;
                    self.Owner.Battle.CampaignState.CommonGold += (int)(extraGold * multiplier);
                };
            }).WithPrerequisite(sheet => {
                return sheet.Proficiencies.Get(Trait.Athletics) >= Proficiency.Trained;
            }, "You must be trained in Athletics");

            yield return new TrueFeat(FeatNames.feats[FeatNames.FeatId.PILGRIMS_TOKEN], 1, "You carry a small token of protection from a site holy to your faith, or you touched your religious symbol to a relic or altar at such a site.", "Your token alerts you to impending peril, granting you a +1 bonus to initiative rolls.", new Trait[] { Trait.General, Trait.Skill })
            .WithPermanentQEffect("+1 bonus to inititive.", self => {
                self.BonusToInitiative = self => new Bonus(1, BonusType.Untyped, "Pilgrim's Token");
            }).WithPrerequisite(sheet => {
                return sheet.Proficiencies.Get(Trait.Religion) >= Proficiency.Trained;
            }, "You must be trained in Religion");

            yield return new TrueFeat(FeatNames.feats[FeatNames.FeatId.NO_CAUSE_FOR_ALARM], 1,
                "You attempt to reduce panic.", "Attempt a Diplomacy check vs. an easy DC of the target's level, for each friendly creature in a 15-foot emanation around you that is frightened. Each of them is temporarily immune for the rest of the encounter.\n\n{b}Critical Success{/b} Reduce the creature's frightened value by 2.\n{b}Success{/b} Reduce the creature's frightened value by 1.", new Trait[] { Trait.General, Trait.Skill, Trait.Auditory, Trait.Concentrate, Trait.Emotion, Trait.Linguistic, Trait.Mental, Trait.Homebrew })
            .WithOnCreature((sheet, creature) => {
                creature.AddQEffect(new QEffect() {
                    ProvideActionIntoPossibilitySection = (self, section) => {
                        if (section.PossibilitySectionId != PossibilitySectionId.OtherManeuvers) {
                            return null;
                        }

                        return (ActionPossibility)new CombatAction(self.Owner, BoBAssets.imgs[BoBAssets.ImageId.NO_CAUSE_FOR_ALARM], "No Cause for Alarm",
                            new Trait[] { Trait.Auditory, Trait.Concentrate, Trait.Emotion, Trait.Linguistic, Trait.Mental },
                            "Attempt a Diplomacy check vs. an easy DC of the target's level, for each friendly creature in a 15-foot emanation around you that is frightened. Each of them is " +
                            "temporarily immune for the rest of the encounter.\n\n{b}Critical Success{/b} Reduce the creature's frightened value by 2.\n{b}Success{/b} Reduce the creature's frightened value by 1.",
                            Target.Emanation(3).WithIncludeOnlyIf((area, creature) =>
                                creature.FriendOfAndNotSelf(self.Owner) && creature.HasEffect(QEffectId.Frightened) && creature.QEffects.FirstOrDefault(
                                    qf => {
                                        if (qf.Tag == null || qf.Tag is not ActionId || qf.Source != self.Owner) {
                                            return false;
                                        }
                                        ActionId tag = (ActionId)qf.Tag;
                                        if (tag == actNoCauseForAlarm) {
                                            return true;
                                        }
                                        return false;
                                    }) == null)
                            ) {
                            ShortDescription = "Attempt a diplomacy check to reduce the Frightened condition of allies within 15-feet.",
                        }
                        .WithActionCost(2)
                        .WithActionId(actNoCauseForAlarm)
                        .WithSoundEffect(SfxName.PositiveMelody)
                        .WithProjectileCone(IllustrationName.CalmEmotions, 30, ProjectileKind.Cone)
                        .WithEffectOnEachTarget(async (action, user, target, _) => {
                            // Make roll
                            CheckResult result = CommonSpellEffects.RollCheck($"No Cause for Alarm ({target.Name})", new ActiveRollSpecification(Checks.SkillCheck(Skill.Diplomacy), Checks.FlatDC(GetDCByLevel(target.Level) - 2)), user, target);

                            // Get effect
                            QEffect? frightened = target.QEffects.FirstOrDefault(qf => qf.Id == QEffectId.Frightened);

                            // Reduce frightened value
                            if (frightened != null) {
                                if (result == CheckResult.CriticalSuccess) {
                                    frightened.Value -= 2;
                                    target.Occupies.Overhead("*frightened reduced by 2*", Color.White);
                                } else if (result == CheckResult.Success) {
                                    frightened.Value -= 1;
                                    target.Occupies.Overhead("*frightened reduced by 1*", Color.White);
                                } else {
                                    target.Occupies.Overhead("*attempt failed*", Color.Red);
                                }

                                if (frightened.Value == 0) {
                                    frightened.ExpiresAt = ExpirationCondition.Immediately;
                                }
                            }
                            target.AddQEffect(QEffect.ImmunityToTargeting(actNoCauseForAlarm, user));
                        })
                        ;
                    }
                });
            }).WithPrerequisite(sheet => {
                return sheet.Proficiencies.Get(Trait.Diplomacy) >= Proficiency.Trained;
            }, "You must be trained in Diplomacy");

            yield return new TrueFeat(FeatNames.feats[FeatNames.FeatId.FOUNT_OF_KNOWLEDGE], 1, "You've an almost encyclopedic knowledge of the world, hard won through years of study or extensive travels.", "You gain a +1 status bonus to all knowledge skill checks.", new Trait[] { Trait.General, Trait.Skill, Trait.Homebrew })
            .WithPermanentQEffect("+1 status bonus to knowledge skill checks.", self => {
                self.BonusToSkills = skill => {
                    if (new Skill[] { Skill.Arcana, Skill.Religion, Skill.Occultism, Skill.Nature, Skill.Society }.Contains(skill)) {
                        return new Bonus(1, BonusType.Status, "Fount of Knowledge");
                    }
                    return null;
                };
            }).WithPrerequisite(sheet => {
                return ((sheet.Proficiencies.Get(Trait.Arcana) >= Proficiency.Trained || sheet.Proficiencies.Get(Trait.Nature) >= Proficiency.Trained ||
                sheet.Proficiencies.Get(Trait.Occultism) >= Proficiency.Trained || sheet.Proficiencies.Get(Trait.Religion) >= Proficiency.Trained ||
                sheet.Proficiencies.Get(Trait.Society) >= Proficiency.Trained) && sheet.FinalAbilityScores.TotalScore(Ability.Intelligence) >= 12);
            }, "You must have 12 or more intelligence and be trained in Arcana, Nature, Occultism, Society or Religion");

            yield return new TrueFeat(FeatNames.feats[FeatNames.FeatId.THEATRICAL_DISTRACTION], 1, "Your thespian talents make you adept at redirecting the attention of others. The show must go on!", "You gain a +1 status bonus to checks made to Create a Diversion.", new Trait[] { Trait.General, Trait.Skill, Trait.Homebrew })
            .WithPermanentQEffect("+1 status bonus to checks made to Create a Diversion.", self => {
                self.BonusToSkillChecks = (skill, action, target) => {
                    if (action != null && action.ActionId == ActionId.CreateADiversion) {
                        return new Bonus(1, BonusType.Status, "Theatrical Distraction");
                    }
                    return null;
                };
            }).WithPrerequisite(sheet => {
                return sheet.Proficiencies.Get(Trait.Diplomacy) >= Proficiency.Trained || sheet.Proficiencies.Get(Trait.Performance) >= Proficiency.Trained;
            }, "You must be trained in Diplomacy or Performance");

            yield return new TrueFeat(FeatNames.feats[FeatNames.FeatId.SHARPENED_SENSES], 1, "Your senses are sharpen to a knife's edge, making you particularly adept at spotting traps or lurking enemies.", "You gain a +1 bonus to Seek checks.", new Trait[] { Trait.General, Trait.Skill, Trait.Homebrew })
            .WithPermanentQEffect("+1 status bonus to checks made to Create a Diversion.", self => {
                self.BonusToAttackRolls = (self, action, target) => {
                    if (action != null && action.ActionId == ActionId.Seek) {
                        return new Bonus(1, BonusType.Untyped, "Sharpened Senses");
                    }
                    return null;
                };
            });

            yield return new TrueFeat(FeatNames.feats[FeatNames.FeatId.SNAKE_OIL], 1, "You're adept at administering various healing tinctures, artefacts, crystals or martial techniques of questionable medical veracity to coax wounded companions back into battle.",
                "{b}Range{/b} touch\n{b}Requirements{/b} You must have a hand free, and your target cannot be at full hit points.\n\nMake a Deception check against DC 15. On a success, the target gains 1d6 temporary HP." +
                "\n\nRegardless of your result, the target is then temporarily immune to your Snake Oil for the rest of the day.\n\nIf you're expert in Deception, you can choose to make the check against DC 20. If you do, you grant 1d6 + 3 temporary HP on a success" +
                " instead. If you're a master in Deception, you can instead attempt a DC 30 check to grant 1d6 + 13 temporary hit points instead.", new Trait[] { Trait.General, Trait.Manipulate, Trait.Skill })
                .WithActionCost(1)
                .WithPrerequisite(values => values.GetProficiency(Trait.Deception) >= Proficiency.Trained, "You must be trained in Deception.")
                .WithPermanentQEffect("You can aid allies with placebo medicinal aid as an 'other maneuver'.", qf => qf.ProvideActionIntoPossibilitySection = (self, section) => {
                    if (section.PossibilitySectionId != PossibilitySectionId.OtherManeuvers)
                        return (Possibility)null;
                    CharacterSheet persistentCharacterSheet = self.Owner.PersistentCharacterSheet;
                    if (persistentCharacterSheet == null || persistentCharacterSheet.Calculated.GetProficiency(Trait.Medicine) < Proficiency.Expert) {
                        return (ActionPossibility)SnakeOil.CreateSnakeOilAction(self.Owner, Proficiency.Trained);
                    } else if (persistentCharacterSheet != null && persistentCharacterSheet.Calculated.GetProficiency(Trait.Medicine) == Proficiency.Expert) {
                        return (Possibility)new SubmenuPossibility((Illustration)IllustrationName.PotionOfSwimming, "Snake Oil") {
                            Subsections = {
                                new PossibilitySection("Snake Oil") {
                                    Possibilities = {
                                        (ActionPossibility) SnakeOil.CreateSnakeOilAction(self.Owner, Proficiency.Trained),
                                        (ActionPossibility) SnakeOil.CreateSnakeOilAction(self.Owner, Proficiency.Expert)
                                    }
                                }
                            }
                        };
                    } else {
                        return (Possibility)new SubmenuPossibility((Illustration)IllustrationName.PotionOfSwimming, "Snake Oil") {
                            Subsections = {
                                new PossibilitySection("Snake Oil") {
                                    Possibilities = {
                                        (ActionPossibility) SnakeOil.CreateSnakeOilAction(self.Owner, Proficiency.Trained),
                                        (ActionPossibility) SnakeOil.CreateSnakeOilAction(self.Owner, Proficiency.Expert),
                                        (ActionPossibility) SnakeOil.CreateSnakeOilAction(self.Owner, Proficiency.Master)
                                    }
                                }
                            }
                        };
                    }
                });

            //yield return new TrueFeat(FeatNames.feats[FeatNames.FeatId.DUBIOUS_KNOWLEDGE], 1, "You can carry more than your frame implies.", "After each encounter, you're able to haul away additional miscellaneous treasures weaker parties would find too burdonsome to loot, increasing your gold reward by 5%.", new Trait[] { Trait.General, Trait.Skill, Trait.Homebrew })
            //.WithPermanentQEffect("", self => {
            //    self.AfterYouTakeAction = async (self, action) => {
            //        if (action.Name != "Recall Weakness" || action.CheckResult != CheckResult.Failure) {
            //            return;
            //        }

            //    //    if (self.Owner.Battle.AskForConfirmation(self.Owner, IllustrationName.Action, "You failed your Recall Weakness check. Would you like to use Dubious Knowledge, to randomly either upgrade or downgrade your check by one degree of success?", "Yes").GetAwaiter().GetResult()) {
            //    //        if (R.Coin()) {
            //    //            action.ChosenTargets.ChosenCreature.AddQEffect(new QEffect("Recall Weakness -1", "The creature is taking a -1 circumstance penalty to its next saving throw.") {
            //    //                BonusToDefenses = (self, action, defence) => {
            //    //                    if (defence != Defense.AC) {
            //    //                        return new Bonus(-1, BonusType.Circumstance, "Recall Weakness");
            //    //                    }
            //    //                    return null;
            //    //                },
            //    //                Illustration = IllustrationName.Action,
            //    //                Source = self.Owner,
            //    //                ExpiresAt = ExpirationCondition.ExpiresAtEndOfSourcesTurn,
            //    //                CannotExpireThisTurn = true
            //    //            });
            //    //        }
            //    //        action.ChosenTargets.ChosenCreature.AddQEffect(new QEffect("Recall Weakness 1", "The creature is taking a +1 circumstance bonus to its next saving throw.") {
            //    //            BonusToDefenses = (self, action, defence) => {
            //    //                if (defence != Defense.AC) {
            //    //                    return new Bonus(1, BonusType.Circumstance, "Recall Weakness");
            //    //                }
            //    //                return null;
            //    //            },
            //    //            AfterYou
            //    //            Illustration = IllustrationName.Action,
            //    //            Source = self.Owner,
            //    //            ExpiresAt = ExpirationCondition.ExpiresAtEndOfSourcesTurn,
            //    //            CannotExpireThisTurn = true
            //    //        });
            //    //    }
            //    //};

            //    self.BeforeYourActiveRoll = ()

            //    //self.AdjustSavingThrowResult = (self, action, result) => {
            //    //    if (action.Name != "Recall Weakness" || result != CheckResult.Failure) {
            //    //        return result;
            //    //    }

            //    //    if (self.Owner.Battle.AskForConfirmation(self.Owner, IllustrationName.Action, "You failed your Recall Weakness check. Would you like to use Dubious Knowledge, to randomly either upgrade or downgrade your check by one degree of success?", "Yes").GetAwaiter().GetResult()) {
            //    //        if (R.Coin()) {
            //    //            return CheckResult.Success;
            //    //        }
            //    //        return CheckResult.CriticalFailure;
            //    //    }

            //    //    return result;
            //    //};
            //}).WithPrerequisite(sheet => {
            //    return sheet.Proficiencies.Get(Trait.Arcana) >= Proficiency.Trained || sheet.Proficiencies.Get(Trait.Nature) >= Proficiency.Trained ||
            //    sheet.Proficiencies.Get(Trait.Occultism) >= Proficiency.Trained || sheet.Proficiencies.Get(Trait.Religion) >= Proficiency.Trained ||
            //    sheet.Proficiencies.Get(Trait.Society) >= Proficiency.Trained;
            //}, "Trained in Arcana, Nature, Occultism, Society or Religion");
        }

        private static IEnumerable<Feat> CreateBackgrounds() {
            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Sailor"), "You heard the call of the sea from a young age. Perhaps you signed onto a merchant's vessel, joined the navy, or even fell in with a crew of pirates and scalawags.",
                "You're trained in {b}Athletics{/b}. You gain the {b}Underwater Marauder{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Strength, Ability.Dexterity), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Athletics);
                sheet.GrantFeat(FeatNames.feats[FeatNames.FeatId.UNDERWATER_MARAUDER]);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Dawnsbury Under the Sea Resident"),
                "You were born amongst the merfolk and other benevolent aquotic denizens of Dawnsbury Under the Sea. Now you seek to make your mark on the strange and foreign world above the waves.",
                "You're trained in {b}Athletics{/b}. You gain the {b}Underwater Marauder{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Charisma, Ability.Strength), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Athletics);
                sheet.GrantFeat(FeatNames.feats[FeatNames.FeatId.UNDERWATER_MARAUDER]);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Guard"), "You served as a guard for the city, a rich patron or less scrupulous organisation, becoming adept both at protecting your charges and browbeating rabble in equal measure.",
                "You're trained in {b}Intimidation{/b}. You gain the {b}Bodyguard{/b} ability.\n\n{b}Bodyguard {icon:FreeAction}.{/b} Once per day, you can guard an adjacent ally, granting them a +1 circumstance bonus to AC whilst adjacent to you, until the start of your next turn.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Strength, Ability.Charisma), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Intimidation);
            })
            .WithOnCreature(creature => {
                creature.AddQEffect(new QEffect("Bodyguard", "Once per day, you can guard an adjacent ally as a {icon:FreeAction}, granting them a +1 circumstance bonus to AC whilst adjacent to you, until the start of your next turn.") {
                    StartOfCombat = async self => {
                        if (self.Owner.PersistentUsedUpResources.UsedUpActions.Contains("Guard Ally")) {
                            self.Name += " (Expended)";
                        }
                    },
                    ProvideContextualAction = self => {
                        if (self.Owner.PersistentUsedUpResources.UsedUpActions.Contains("Guard Ally")) {
                            return null;
                        }

                        if (self.Owner.Battle.AllCreatures.Where(c => c != self.Owner && c.IsAdjacentTo(self.Owner)).ToArray().Length < 1) {
                            return null;
                        }

                        return (ActionPossibility) new CombatAction(self.Owner, IllustrationName.ShieldSpell, "Guard Ally", new Trait[] { Trait.Basic }, "Target ally gains a +1 circumstance bonus to AC until the start of your next turn, so long as they remain adjacent to you.", Target.AdjacentFriend())
                        .WithSoundEffect(SfxName.RaiseShield)
                        .WithActionCost(0)
                        .WithEffectOnEachTarget(async (action, user, target, result) => {
                            target.AddQEffect(new QEffect("Guarded", $"+1 circumstance bonus to AC while adjacent to {user.Name}.") {
                                Illustration = IllustrationName.ShieldSpell,
                                Innate = false,
                                Source = user,
                                ExpiresAt = ExpirationCondition.ExpiresAtStartOfSourcesTurn,
                                BonusToDefenses = (self, action, defence) => {
                                    if (defence == Defense.AC && user.IsAdjacentTo(self.Owner)) {
                                        return new Bonus(1, BonusType.Circumstance, "Guarded", true);
                                    }
                                    return null;
                                }
                            });
                            user.PersistentUsedUpResources.UsedUpActions.Add("Guard Ally");
                            self.Name += " (Expended)";
                        })
                        ;
                    }
                });
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Labourer"),
                "You've spent years performing arduous physical labor, perhaps as penance for your crimes or as part of the efforts to rebuild after the Night of the Shooting Stars. " +
                "You may have embraced adventuring as an easier method to make your way in the world, or to use the fruits of your difficult lifestyle for a greater purpose.",
                "You're trained in {b}Athletics{/b}. You gain the {b}Hefty Hauler{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Strength, Ability.Constitution), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Athletics);
                sheet.GrantFeat(FeatNames.feats[FeatNames.FeatId.HEFTY_HAULER]);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Criminal"),
                "As an unscrupulous independent or as a member of an underworld organization, you lived a life of crime. You might have become an adventurer to seek redemption, to escape the law, or simply to get access to bigger and better loot.",
                "You're trained in {b}Stealth{/b}. You gain the {b}Escape Artist{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Dexterity, Ability.Intelligence), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Stealth);
                sheet.GrantFeat(FeatNames.feats[FeatNames.FeatId.ESCAPE_ARTIST]);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Despatch Runner"),
                "You served as a runner, carrying important despatch orders and battle reports between fronts during the war against the starborn. Your service is complete now, perhaps replaced by divination, " +
                "or your superiors left dead in a ditch, routed in battle. Now the time has come for you to stop running, and face your perils head on.",
                "You're trained in {b}Athletics{/b}. You gain the {b}Fleet{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Strength, Ability.Constitution), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Athletics);
                sheet.GrantFeat(FeatName.Fleet);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Scout"),
                "You called the wilderness home as you found trails and guided travelers. Your wanderlust could have called you to the adventuring life, or perhaps you served as a scout for soldiers and found you liked battle.",
                "You're trained in {b}Nature{/b}. You gain the {b}Fleet{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Dexterity, Ability.Wisdom), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Nature);
                sheet.GrantFeat(FeatName.Fleet);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Street Urchin"),
                "You eked out a living by picking pockets on the streets of a major city, never knowing where you'd find your next meal. While some folk adventure for the glory, you do so to survive.",
                "You're trained in {b}Thievery{/b}. You gain the {b}Toughness{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Dexterity, Ability.Constitution), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Thievery);
                sheet.GrantFeat(FeatName.Toughness);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Cook"),
                "You grew up in the kitchens of a tavern or other dining establishment and excelled there, becoming an exceptional cook. Baking, cooking, a little brewing on the side—you've spent lots of time out of sight. " +
                "It's about time you went out into the world to catch some sights for yourself.",
                "You're trained in {b}Diplomacy{/b}. You gain the {b}Gourmet Rations{/b} ability.\n\n{b}Gourmet Rations.{/b} With a few choice spices and culinary secrets, you can make an unforgettable meal from even the blandest of ingredients. " +
                "During the first encounter after a long rest, you and your allies each have a chance to gain a random food buff from your exquisite cooking." +
                "\n•{b}Fortifying Meal.{/b} +1 bonus to Fortitude saves" +
                "\n•{b}Invigorating Meal.{/b} +5-foot bonus to speed." +
                "\n•{b}Emboldening Meal.{/b} +1 bonus to Will saves." +
                "\n•{b}Hearty Meal.{/b} Gain temporary HP equal to the cook's level.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Intelligence, Ability.Constitution), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Diplomacy);
            })
            .WithOnCreature(cook => {
                cook.AddQEffect(new QEffect("Gourmet Rations", "During the first encounter after a long rest, you and your allies each have a chance to gain a random food buff.") {
                    StartOfCombat = async self => {
                        if (self.Owner.PersistentUsedUpResources.UsedUpActions.Contains("Gourmet Rations")) {
                            self.Name += " (Expended)";
                            return;
                        }
                        foreach (Creature ally in self.Owner.Battle.AllSpawnedCreatures.Where(cr => cr.OwningFaction.IsHumanControlled)) {
                            int chance = R.NextD20();
                            if (chance < 13) {
                                continue;
                            }
                            int meal = R.Next(1, 5);
                            if (meal == 1) {
                                ally.AddQEffect(new QEffect("Fortifying Meal", "+1 bonus to fortitude saves.") {
                                    Illustration = BoBAssets.imgs[BoBAssets.ImageId.F_MEAL],
                                    Innate = false,
                                    Key = "Fortifying Meal",
                                    BonusToDefenses = (self, action, defence) => {
                                        if (defence == Defense.Fortitude) {
                                            return new Bonus(1, BonusType.Untyped, "Fortifying Meal");
                                        }
                                        return null;
                                    }
                                });
                            }
                            else if (meal == 2) {
                                ally.AddQEffect(new QEffect("Invigorating Meal", "+5-foot bonus to speed.") {
                                    Illustration = BoBAssets.imgs[BoBAssets.ImageId.I_MEAL],
                                    Innate = false,
                                    Key = "Invigorating Meal",
                                    BonusToAllSpeeds = (self) => {
                                        return new Bonus(1, BonusType.Untyped, "Invigorating Meal");
                                    }
                                });
                            }
                            else if (meal == 3) {
                                // Hearty Meal
                                ally.GainTemporaryHP(self.Owner.Level);
                            }
                            else if (meal == 4) {
                                ally.AddQEffect(new QEffect("Emboldening Meal", "+1 bonus to will saves.") {
                                    Illustration = BoBAssets.imgs[BoBAssets.ImageId.E_MEAL],
                                    Innate = false,
                                    Key = "Emboldening Meal",
                                    BonusToDefenses = (self, action, defence) => {
                                        if (defence == Defense.Will) {
                                            return new Bonus(1, BonusType.Untyped, "Emboldening Meal");
                                        }
                                        return null;
                                    }
                                });
                            }
                        }
                        self.Owner.PersistentUsedUpResources.UsedUpActions.Add("Gourmet Rations");
                        self.Name += " (Expended)";
                    },
                });
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Fire Warden"),
                "Whether you fought against fires in the wilderness or in crowded city streets, you've had your fair share of dealing with uncontrolled flames. Battling thick smoke and toxic fumes, " +
                "you've broken down obstacles to save trapped people from a fiery grave, and you've studied the nature and source of fire itself to try and better learn how to fight it.",
                "You're trained in {b}Diplomacy{/b}.\n\nYou gain a +1 bonus to saving throws against effects with the fire trait.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Strength, Ability.Constitution), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Athletics);
                // From Lizardfolk mod (maybe just add it as a base feat, but remove if lizardfolk is detected?)
                //sheet.GrantFeat(AllFeats.All.FirstOrDefault(ft => ft.Name == "Breath Control").FeatName); // Breath Control
            })
            .WithOnCreature(fw => {
                fw.AddQEffect(new QEffect("Fire Warden", "+1 bonus to saving throws against effects with the fire trait.") {
                    Innate = true,
                    BonusToDefenses = (self, action, defence) => {
                        if (defence != Defense.AC && action != null && action.HasTrait(Trait.Fire)) {
                            return new Bonus(1, BonusType.Untyped, "Fire Warden");
                        }
                        return null;
                    }
                });
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Hermit"),
                "In an isolated place—like a cave, remote oasis, or secluded mansion—you lived a life of solitude. Adventuring might represent your first foray out among other people in some time. " +
                "This might be a welcome reprieve from solitude or an unwanted change, but in either case, you're likely still rough around the edges.",
                "You're trained in {b}Nature{/b}. You gain the {b}Fount of Knowledge{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Constitution, Ability.Intelligence), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Nature);
                sheet.GrantFeat(FeatNames.feats[FeatNames.FeatId.FOUNT_OF_KNOWLEDGE]);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Refugee"),
                "You come from a land very distant from the one you now find yourself in, driven by war, plague, or simply in the pursuit of opportunity. Regardless of your origin or the reason you left your home, " +
                "you find yourself an outsider in this new land. Adventuring is a way to support yourself while offering hope to those who need it most.",
                "You're trained in {b}Stealth{/b}. You gain the {b}Toughness{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Constitution, Ability.Wisdom), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Stealth);
                sheet.GrantFeat(FeatName.Toughness);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Librarian"),
                "You've spent your life curating, collecting and maintaining tomes of knowledge for a local library, guild archieve or magic academy, giving you a knack for deciphering magical scrolls. " +
                "You might have taken to adventuring to finance your acquisition of rare tomes, to explore occult mysteries that can't be found in the pages of a book, or perhaps to put your book learned skills to the test.",
                "You're trained in {b}Occultism{/b}. You gain the {b}Trick Magic Item{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Intelligence, Ability.Wisdom), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Occultism);
                sheet.GrantFeat(FeatName.TrickMagicItem);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Acolyte"),
                "You spent your early days in a religious monastery or cloister. You may have traveled out into the world to spread the message of your religion or because you cast away the teachings of your faith, " +
                "but deep down you'll always carry within you the lessons you learned.",
                "You're trained in {b}Religion{/b}. You gain the {b}Pilgrim's Token{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Intelligence, Ability.Wisdom), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Religion);
                sheet.GrantFeat(FeatNames.feats[FeatNames.FeatId.PILGRIMS_TOKEN]);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Herbalist"),
                "As a formally trained apothecary or a rural practitioner of folk medicine, you learned the healing properties of various herbs. You're adept at collecting the right natural cures in all sorts of environments and preparing them properly.",
                "You're trained in {b}Nature{/b}. You gain the {b}Concoct Poultice{/b} ability." +
                "\n\n{b}Concoct Poultice {icon:Action}.{/b} Concoct a single random potion from nearby ingredients, which appears in a free hand. The poultice functions as typical for a potion of its type, but is of ephemeral potency, " +
                "quickly rendering itself inert if not used by the end of the encounter.",
                new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Constitution, Ability.Wisdom), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Nature);
            })
            .WithOnCreature(c => {
                c.AddQEffect(new QEffect() {
                    ProvideActionIntoPossibilitySection = (self, section) => {
                        if (self.Owner.PersistentUsedUpResources.UsedUpActions.Contains("Concoct Poultice")) {
                            return null;
                        }

                        if (section.PossibilitySectionId != PossibilitySectionId.OtherManeuvers) {
                            return null;
                        }

                        return (ActionPossibility)new CombatAction(self.Owner, BoBAssets.imgs[BoBAssets.ImageId.CONCOCT_POULTICE], "Concoct Poultice", new Trait[] { Trait.Alchemical },
                            "Concoct a single random potion from nearby ingredients, which appears in a free hand. The poultice functions as typical for a potion of its type, but is of ephemeral potency, quickly rendering itself inert if not used by the end of the encounter.", Target.Self()
                            .WithAdditionalRestriction(c => {
                                if (self.Owner.HasFreeHand) {
                                    return null;
                                }
                                return "no free hand";
                            })) {
                            ShortDescription = "Concoct a random temporary potion or elixir, which appears in a free hand.",
                        }
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.AcidSplash)
                        .WithEffectOnSelf(creature => {
                            List<Item> potions = Items.ShopItems.Where(item => (item.HasTrait(Trait.Potion) || item.HasTrait(Trait.Elixir)) && !item.HasTrait(Trait.Uncommon) && item.Level <= self.Owner.Level).ToList();
                            potions = potions.Concat(potions.Where(item => item.HasTrait(Trait.Healing)).ToList()).ToList();
                            int index = R.Next(0, potions.Count);
                            Item potion = Items.CreateNew(potions[index].ItemName);
                            potion.Name = creature.Name + "'s " + potion.Name;
                            potion.Price = 0;
                            creature.AddHeldItem(potion);
                            creature.Occupies.Overhead($"*crafted {potion.Name}*", Color.Green);
                            self.Owner.PersistentUsedUpResources.UsedUpActions.Add("Concoct Poultice");
                            self.Tag = potion;
                        });
                    },
                    EndOfCombat = async (self, won) => {
                        if (self.Tag == null) {
                            return;
                        }

                        Item potion = (Item)self.Tag;
                        foreach (Creature ally in self.Owner.Battle.AllCreatures.Where(c => c.OwningFaction.IsHumanControlled)) {
                            ally.CarriedItems.Remove(potion);
                            ally.HeldItems.Remove(potion);
                        }
                        foreach (Tile tile in self.Owner.Battle.Map.Tiles) {
                            tile.DroppedItems.Remove(potion);
                        }
                    }
                });
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Bounty Hunter"),
                "Bringing in lawbreakers lined your pockets, and earned you a reputation that strikes fear into your marks. Maybe you had an altruistic motive and sought to bring in criminals to make the streets safer, " +
                "or maybe the coin was motivation enough. Your techniques for hunting down criminals transfer easily to the life of an adventurer.",
                "You're trained in {b}Intimidation{/b}. You gain the {b}Manhunter{/b} ability\n\n{b}Manhunter.{/b} You gain a +1 status bonus to checks to demoralise humanoid opponents.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Strength, Ability.Wisdom), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Intimidation);
            })
            .WithOnCreature(c => {
                c.AddQEffect(new QEffect("Manhunter", "Gain a +1 status bonus to checks to demoralise humanoid opponents.") {
                    BonusToSkillChecks = (skill, action, target) => {
                        if (action != null && action.ActionId == ActionId.Demoralize && new Trait[] { Trait.Humanoid, Trait.Elf, Trait.Dwarf, Trait.Goblin, Trait.Human, Trait.Kobold, Trait.Orc, Trait.Leshy, Trait.Goblin, Trait.Gnoll, Trait.Gnome }.ContainsOneOf(target.Traits)) {
                            return new Bonus(1, BonusType.Status, "Manhunter");
                        }
                        return null;
                    }
                });
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Aristocrat"),
                "The mantle of leadership is a heavy burden, one thrust upon you by birth or necessity. Perhaps you naively think yourself above your less privilaged adventuring companions you've so graciously deigned to personally travel alongside, " +
                "or long to escape the responsibilities of your post after maintaining the facade of propriety for so long. Regardless, others still look to you as a beacon of order amongst the turmoil of the starborn invasion.",
                "You're trained in {b}Diplomacy{/b}. You gain the {b}No Cause for Alarm{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Intelligence, Ability.Charisma), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Diplomacy);
                sheet.GrantFeat(FeatNames.feats[FeatNames.FeatId.NO_CAUSE_FOR_ALARM]);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Performer"),
                "Through an education in the arts or sheer dogged practice, you learned to entertain crowds. You might have been an actor, a dancer, a musician, a street magician, or any other sort of performer.",
                "You're trained in {b}Deception{/b}. You gain the {b}Theatrical Distraction{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Dexterity, Ability.Charisma), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Deception);
                sheet.GrantFeat(FeatNames.feats[FeatNames.FeatId.THEATRICAL_DISTRACTION]);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Musical Prodigy"),
                "Ever since you were young, you've been almost supernaturally skilled in a particular type of music. The people around you were sure you'd grow up to perform at royal courts or to become a world-famous composer, " +
                "but you've chosen a life of adventure instead. You might have given up on those dreams to find your own meaning, or you might find that adventuring allows you to experience unfiltered emotions and exploits that " +
                "you can translate into a wondrous symphony some day.",
                "You're trained in {b}Diplomacy{/b}. You gain the {b}Counter Tune{/b} ability.\n\n{b}Counter Tune {icon:Reaction}.{/b} Your musical talent is such that once per day, you can attempt to counteract an auditory action being" +
                " attempted by an enemy, by blotting it out with a soothing tune of your own. Make a diplomacy or performance check vs. their class or spell save DC. The counteract level of this effect is equal to half your level, rounded up.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Dexterity, Ability.Charisma), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Diplomacy);
            })
            .WithOnCreature(c => {
                QEffect effect = new QEffect("Counter Tune {icon:Reaction}", "Once per day, when an enemy creature attempts to use an action with the auditory trait, you can attempt to counteract it with a diplomacy or performace check.") {
                };
                c.AddQEffect(effect);

                if (c.PersistentUsedUpResources.UsedUpActions.Contains("Counter Tune")) {
                    effect.Name += " (Expended)";
                    return;
                }

                effect.AddGrantingOfTechnical(creature => creature.OwningFaction.EnemyFactionOf(c.OwningFaction), qfTechnical => {
                    if (c.PersistentUsedUpResources.UsedUpActions.Contains("Counter Tune")) {
                        return;
                    }

                    qfTechnical.Source = c;
                    qfTechnical.Name = "Counter Tune";
                    qfTechnical.FizzleOutgoingActions = async (self, action, strBuilder) => {
                        if (action == null) {
                            return false;
                        }

                        if (action.HasTrait(Trait.Auditory) && self.Owner.DistanceTo(self.Source) <= 6) {
                            // Determine params
                            string desc = "When an enemy within 30ft of you uses an ability with the Auditory trait, you may attempt to counteract it using a diplomacy or performance check.";
                            SpellcastingSource? spellcastingSource = action.SpellcastingSource;
                            int enemyDC = spellcastingSource != null ? spellcastingSource.GetSpellSaveDC() : self.Owner.ClassOrSpellDC();
                            ActiveRollSpecification activeRollSpecification = new ActiveRollSpecification(Checks.SkillCheck(new Skill[] { Skill.Diplomacy, Skill.Performance }), (CalculatedNumber.CalculatedNumberProducer)((action, attacker, defender) => new CalculatedNumber(enemyDC, "Action DC", new List<Bonus>())));
                            CheckBreakdown breakdown = CombatActionExecution.BreakdownAttack(new CombatAction(self.Source, (Illustration)IllustrationName.ArcaneCascade, "Counter Tune", new Trait[] { Trait.Abjuration, Trait.Attack }, desc, (Target)Target.Self())
                            .WithActiveRollSpecification(activeRollSpecification), self.Owner);

                            CheckResult? minimumNeededResult = Counteracting.DetermineMinimumNeededResult((self.Source.Level + 1) / 2, action.SpellId != SpellId.None ? action.SpellLevel : self.Owner.Level / 2);
                            if (!minimumNeededResult.HasValue) {
                                return false;
                            }
                            int targetDC = minimumNeededResult == CheckResult.CriticalSuccess ? enemyDC + 10 : minimumNeededResult == CheckResult.Success ? enemyDC : minimumNeededResult == CheckResult.Failure ? enemyDC - 10 : 0;

                            if (!await self.Source.AskToUseReaction($"{self.Owner} is attempting to use the auditory effect {action.Name}, with a target DC of {targetDC}. Would you like to use Counter Tune to attempt to counteract it?")) {
                                return false;
                            }

                            self.Source.PersistentUsedUpResources.UsedUpActions.Add("Counter Tune");
                            effect.Name += " (Expended)";

                            int num3;
                            switch (minimumNeededResult.GetValueOrDefault()) {
                                case CheckResult.Failure:
                                    num3 = breakdown.Misses + breakdown.Hits + breakdown.CritHits;
                                    break;
                                case CheckResult.Success:
                                    num3 = breakdown.Hits + breakdown.CritHits;
                                    break;
                                case CheckResult.CriticalSuccess:
                                    num3 = breakdown.CritHits;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                                    return false;
                            }
                            int successfulEvents = num3;
                            int num4 = successfulEvents * 5;
                            CheckBreakdownResult breakdownResult = new CheckBreakdownResult(breakdown);
                            self.Owner.Battle.Log(self.Source?.ToString() + " attempts to use {b}Counter Tune.{/b}", "Counter Tune", desc);
                            int checkResult = (int)breakdownResult.CheckResult;
                            CheckResult? nullable = minimumNeededResult;
                            int valueOrDefault = (int)nullable.GetValueOrDefault();
                            if (checkResult >= valueOrDefault & nullable.HasValue) {
                                self.Owner.Occupies.Overhead("counteracted!", Color.Red, self.Source?.ToString() + " {Green}counteracted{/Green} this (" + breakdownResult.D20Roll.ToString() + " >= " + (21 - successfulEvents).ToString() + ").", "Counter Tune", breakdown.DescribeWithFinalRollTotal(breakdownResult));
                                Sfxs.Play(SfxName.Abjuration);
                                strBuilder.Append("This action was counteracted. Any prerequisite resources were still expended, but it had no effect.");
                                return true;
                            }
                            self.Source.Occupies.Overhead("Counter Tune failed", Color.White, self.Source?.ToString() + " {Red}failed{/Red} to counteract this (" + breakdownResult.D20Roll.ToString() + " vs. " + (21 - successfulEvents).ToString() + ").", "Counter Tune", breakdown.DescribeWithFinalRollTotal(breakdownResult));
                            Sfxs.Play(SfxName.Miss);
                            return false;
                        }
                        return false;
                    };
                });
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Hunter"),
                "You stalked and took down animals and other creatures of the wild. Skinning animals, harvesting their flesh, and cooking them were also part of your training, all of which can give you useful resources while you adventure.",
                "You're trained in {b}Nature{/b}. You gain the {b}Sharpened Senses{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Dexterity, Ability.Wisdom), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Nature);
                sheet.GrantFeat(FeatNames.feats[FeatNames.FeatId.SHARPENED_SENSES]);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Academic"),
                "You have a knack for learning, and sequestered yourself from the outside world to learn all you could. You read about so many wondrous places and things in your books, and always dreamed about one day seeing the real things. " +
                "Eventually, that curiosity led you to leave your studies and become an adventurer.",
                "You're trained in {b}Arcana{/b}. You gain the {b}Fount of Knowledge{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Intelligence, Ability.Wisdom), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Arcana);
                sheet.GrantFeat(FeatNames.feats[FeatNames.FeatId.FOUNT_OF_KNOWLEDGE]);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Charlatan"),
                "You traveled from place to place, peddling false fortunes and snake oil in one town, pretending to be royalty in exile to seduce a wealthy heir in the next. Becoming an adventurer might be your next big scam or an attempt " +
                "to put your talents to use for a greater cause. Perhaps it's a bit of both, as you realize that after pretending to be a hero, you've become the mask.",
                "You're trained in {b}Deception{/b}. You gain the {b}Snake Oil{/b} feat.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Charisma, Ability.Intelligence), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Deception);
                sheet.GrantFeat(FeatNames.feats[FeatNames.FeatId.SNAKE_OIL]);
            });

            yield return new BackgroundSelectionFeat(ModManager.RegisterFeatName("Barkeep"),
                "You have five specialties: hefting barrels, drinking, polishing steins, drinking, and drinking. You worked in a bar, where you learned how to hold your liquor and rowdily socialize.",
                "You're trained in {b}Diplomacy{/b}. You gain the {b}Moonshine{/b} ability.\n\n{b}Moonshine {icon:Action}.{/b} You rifle through your bags for a bottle of moonshine and pop the cork to find out what the distillory process has produced. " +
                "Your moonshine can be of three distinct kinds, but its magical properties are quickly rendered inert if the beverage is not drunk by the end of the encounter." +
                "\n•{b}Rotgut.{/b} A powerful dwarven liquor that quickly renders the drinker numbingly drunk." +
                "\n•{b}Dragon Whisky.{/b} A fiery spirit so strong it's said to turn the drinker's breath to flame." +
                "\n•{b}Berserker's Brew.{/b} A potent spirit that that sends the drinker into a heedless, drunken rage." +
                "\n\nA free hand is required to hold the moonshine.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Charisma, Ability.Constitution), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Diplomacy);
            })
            .WithOnCreature(c => {
                c.AddQEffect(new QEffect() {
                    ProvideActionIntoPossibilitySection = (self, section) => {
                        if (self.Owner.PersistentUsedUpResources.UsedUpActions.Contains("Moonshine")) {
                            return null;
                        }

                        if (section.PossibilitySectionId != PossibilitySectionId.OtherManeuvers) {
                            return null;
                        }

                        return (ActionPossibility)new CombatAction(self.Owner, BoBAssets.imgs[BoBAssets.ImageId.MOONSHINE], "Moonshine", new Trait[] { Trait.Alchemical },
                            "You rifle through your bags for a bottle of moonshine and pop the cork to find out what the distillory process has produced. Your moonshine can be of three distinct kinds, but its magical properties are quickly rendered inert if the beverage is not drunk by the end of the encounter.\n\nA free hand is required to hold the moonshine.", Target.Self()
                            .WithAdditionalRestriction(c => {
                                if (self.Owner.HasFreeHand) {
                                    return null;
                                }
                                return "no free hand";
                            })) {
                            ShortDescription = "Produce a random temporary alcoholic beverage, which appears in a free hand.",
                        }
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.ItemGet)
                        .WithEffectOnSelf(creature => {
                            Item potion = CreateMoonshine(c.Level);
                            creature.AddHeldItem(potion);
                            creature.Occupies.Overhead($"*produced {potion.Name}*", Color.Green);
                            self.Owner.PersistentUsedUpResources.UsedUpActions.Add("Moonshine");
                            self.Tag = potion;
                        });
                    },
                    EndOfCombat = async (self, won) => {
                        if (self.Tag == null) {
                            return;
                        }

                        Item potion = (Item)self.Tag;
                        foreach (Creature ally in self.Owner.Battle.AllCreatures.Where(c => c.OwningFaction.IsHumanControlled)) {
                            ally.CarriedItems.Remove(potion);
                            ally.HeldItems.Remove(potion);
                        }
                        foreach (Tile tile in self.Owner.Battle.Map.Tiles) {
                            tile.DroppedItems.Remove(potion);
                        }
                    }
                });
            });

            // Rare Backgrounds

            Feat output = new BackgroundSelectionFeat(ModManager.RegisterFeatName("Blessed"),
                "You have been blessed by a divinity. For an unknown reason, and irrespective of your actual beliefs, a deity has granted you a boon to use for good or ill. Your blessing grants wisdom and insight to aid you in your struggles.",
                "You're trained in {b}Religion{/b}. You can cast the {b}Guidance{/b} cantrip as a divine innate spell.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Wisdom, Ability.Charisma), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Religion);
            })
            .WithOnCreature(creature => {
                creature.GetOrCreateSpellcastingSource(SpellcastingKind.Innate, ModManager.RegisterTrait("Blessed"), Ability.Charisma, Trait.Divine).WithSpells(new SpellId[] { SpellId.Guidance }, 1);
            })
            .WithRulesBlockForSpell(SpellId.Guidance);

            output.Traits.Add(tRare);
            yield return output;

            output = new BackgroundSelectionFeat(ModManager.RegisterFeatName("Revenant"),
                "You died. No real doubt about that. Arrow to the eye or knife to the throat, you were dead as dead can be. Then you got back up again. Maybe you had some unfinished business, " +
                "or maybe you were just so tough and so mean that Hell itself spat you out. Either way, you came back for a reason.",
                "You're trained in {b}Religion{/b}.\n\nYou gain the {b}Undead{/b} trait, causing negative energy to heal you while positive energy becomes harmful.", new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Constitution, Ability.Charisma), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Religion);
            })
            .WithOnCreature(creature => {
                creature.Traits.Add(Trait.Undead);
            });

            output.Traits.Add(tRare);
            yield return output;

            // Pact ideas
            // - -1 on attack rolls against fey
            // - Small chance to gain a -1 penalty on a skill checks, once per day
            // - Reduce gold rewards
            output = new BackgroundSelectionFeat(ModManager.RegisterFeatName("Feybound"),
                "You have spent time in the First World or another realm of the fey and aren't entirely the same person you were before. Perhaps you made a purchase at the legendary Witchmarket, saved a unicorn from poachers or " +
                "partook deeply of fey food and wine. Whatever the case, willingly or inadvertently, you made a bargain with the fey, the benefits of which come at a price.",
                "You're trained in {b}Nature{/b}.\n\nYou gain the {b}Fey's Fortune{/b} and {b}Fey Price{/b} abilities." +
                "\n\n{b}Fey's Fortune {icon:FreeAction}.{/b} Once per day, you may call upon your fey bargain to gain a +1 bonus to all skill checks until the end of your turn." +
                "\n\n{b}Fey Price.{/b} Your connection with the fey gives denizens of the first world power over you in turn. When you make an attack roll against a Fey creature, you suffer a -1 penalty.",
                new List<AbilityBoost>() { new LimitedAbilityBoost(Ability.Dexterity, Ability.Charisma), new FreeAbilityBoost() })
            .WithOnSheet(sheet => {
                sheet.GrantFeat(FeatName.Religion);
            })
            .WithOnCreature(creature => {
                // TODO: Add feybound
                creature.AddQEffect(new QEffect("Fey's Fortune", "Once per day, you may call upon your fey bargain as a {icon:FreeAction} to gain a +1 status bonus to all skill checks until the end of your turn.") {
                    StartOfCombat = async self => {
                        if (self.Owner.PersistentUsedUpResources.UsedUpActions.Contains("Fey's Fortune")) {
                            self.Name += " (Expended)";
                        }
                    },
                    ProvideActionIntoPossibilitySection = (self, section) => {
                        if (self.Owner.PersistentUsedUpResources.UsedUpActions.Contains("Fey's Fortune")) {
                            return null;
                        }

                        if (section.PossibilitySectionId == PossibilitySectionId.OtherManeuvers) {
                            return (ActionPossibility)new CombatAction(self.Owner, BoBAssets.imgs[BoBAssets.ImageId.FEY_FORTUNE], "Fey's Fortune", new Trait[] { Trait.Fey, Trait.Fortune, Trait.Basic, Trait.Magical },
                                "{b}Frequency{/b} Once per long rest.\n\nYou gain a +1 status bonus to all skill checks until the end of your turn.",
                                Target.Self()) {
                                ShortDescription = "You gain a +1 status bonus to all skill checks until the end of your turn."
                            }
                            .WithSoundEffect(SfxName.Guidance)
                            .WithEffectOnSelf(async user => {
                                user.AddQEffect(new QEffect("Fey Fortune", "Until the end of the turn, you gain a +1 bonus to all skill checks.") {
                                    Illustration = BoBAssets.imgs[BoBAssets.ImageId.FEY_FORTUNE],
                                    Innate = false,
                                    BonusToSkills = skill => new Bonus(1, BonusType.Status, "Fey's Fortune"),
                                    ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn
                                });
                                user.PersistentUsedUpResources.UsedUpActions.Add("Fey's Fortune");
                                self.Name += " (Expended)";
                            })
                            .WithActionCost(0);
                        }
                        return null;
                    }
                });

                creature.AddQEffect(new QEffect("Fey Price", "You suffer a -1 penalty to attack rolls made against Fey creatures.") {
                    BonusToAttackRolls = (self, action, target) => {
                        if (action == null || target == null) {
                            return null;
                        }
                        
                        if (target.HasTrait(Trait.Fey)) {
                            return new Bonus(-1, BonusType.Untyped, "Feybound");
                        }
                        return null;
                    }
                });
            });

            output.Traits.Add(tRare);
            yield return output;

            // TODO (Common)
            //  - Artist - Inspire on kill
            //  - 

            // TODO (Rare)
            //  - Unicorn Touched - Heal? Bestial Clarity?
            //  - Cultist - Invoke random miracle
            //  - Chosen One - Use to 
            //  - Feybound/Genie-blessed - activate to gain advantage on a skill check once per day
            //  - Revanant - Negative healing (possibly just make you undead)
            //  - 

        }

        internal static Item CreateMoonshine(int level) {
            int dw_dice = level < 3 ? 1 : level < 11 ? 2 : 3;
            Item dragonwhisky = new Item(iDragonWhisky, BoBAssets.imgs[BoBAssets.ImageId.DRAGON_WHISKY], "Dragon Whisky", level, 0, new Trait[] { Trait.Consumable, Trait.Potion, Trait.Alchemical }) {
                Description = "A fiery spirit so strong it's said to turn the drinker's breath to flame.\n\n{b}Area{/b} 15-foot cone\n\n{b}Saving Throw{/b} basic Reflex\n\n You deal " + dw_dice + "d6 fire damage to creatures in the area, with a basic Reflex save.",
                DrinkableEffect = (action, drinker) => {
                    drinker.AddQEffect(new QEffect("Dragon Whisky", $"Until the end of your turn, you may breath a gout of flame as a free action. The effects of the dragon whisky are then lost.") {
                        Illustration = BoBAssets.imgs[BoBAssets.ImageId.DRAGON_WHISKY],
                        Innate = false,
                        ProvideMainAction = self => {
                            return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.BreathWeapon, "Exhale Dragon Whisky", new Trait[] { Trait.Fire, Trait.Alchemical },
                                "{b}Range{/b} 15-foot cone\n{b}Saving Throw{/b} Reflex\n\nYou exhale a gout of flame. Deal " + dw_dice + "d6 fire damage to each creature in the area.", Target.Cone(3))
                            .WithProjectileCone(IllustrationName.BreathWeapon, 15, ProjectileKind.Cone)
                            .WithSavingThrow(new SavingThrow(Defense.Reflex, u => u.ClassOrSpellDC()))
                                .WithEffectOnEachTarget(async (action, self, target, checkResult) => {
                                    await CommonSpellEffects.DealBasicDamage(action, self, target, checkResult, $"{dw_dice}d6", DamageKind.Fire);
                                })
                            .WithEffectOnSelf(async user => {
                                user.RemoveAllQEffects(qf => qf.Name == "Dragon Whisky");
                            })
                            .WithSoundEffect(SfxName.Fireball)
                            .WithActionCost(0);
                        },
                        ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn
                    });
                }
            };

            string dice = level < 3 ? "1d6" : level < 6 ? "2d6 + 3" : "3d6 + 6";
            int bonus = level < 3 ? 0 : level < 6 ? 3 : 6;
            Item rotgut = new Item(iRotgut, BoBAssets.imgs[BoBAssets.ImageId.ROTGUT], "Rotgut", level, 0, new Trait[] { Trait.Consumable, Trait.Potion, Trait.Alchemical, Trait.Drinkable }) {
                Description = "A powerful dwarven liquor that quickly renders the drinker numbingly drunk.\n\nGain "+ dice + (bonus > 0 ? $"+{bonus}" : "") + " temporary hit points.",
                DrinkableEffect = (action, drinker) => {
                    int roll = DiceFormula.FromText(dice).RollResult() + bonus;
                    drinker.GainTemporaryHP(roll);
                }
            };

            int bonus2 = level < 3 ? 1 : level < 11 ? 2 : 3;
            Item berserkersBrew = new Item(iBerserkersBrew, BoBAssets.imgs[BoBAssets.ImageId.BERSERKERS_BREW], "Berserker's Brew", level, 0, new Trait[] { Trait.Consumable, Trait.Potion, Trait.Alchemical, Trait.Drinkable }) {
                DrinkableEffect = (action, drinker) => {
                    drinker.AddQEffect(new QEffect("Berserker's Brew", $"Gain a +{bonus2} item bonus to attack rolls and saves against fear effects.") {
                        Innate = false,
                        Illustration = BoBAssets.imgs[BoBAssets.ImageId.BERSERKERS_BREW],
                        Description = "A potent spirit that that sends the drinker into a heedless, drunken rage.\n\n{b}Benefit{/b} You gain a +" + bonus2 + " item bonus to attack rolls and saves against fear effects for the rest of the encounter.\n\n{b}Drawback{/b} If you perform an action with the concentrate trait, you must succeed on a DC 5 flat check, or the action is lost.",
                        BonusToAttackRolls = (self, action, target) => {
                            if (action.Name.StartsWith("Strike (")) {
                                return new Bonus(bonus2, BonusType.Item, "Berserker's Brew");
                            }
                            return null;
                        },
                        BonusToDefenses = (self, action, defence) => {
                            if (action != null && action.HasTrait(Trait.Fear)) {
                                return new Bonus(bonus2, BonusType.Item, "Berserker's Brew");
                            }
                            return null;
                        },
                        FizzleOutgoingActions = async (qf, ca, stringBuilder) => {
                            if (!ca.HasTrait(Trait.Concentrate))
                                return false;
                            (CheckResult, string) tuple = Checks.RollFlatCheck(5);
                            stringBuilder.AppendLine("Attempting a concentrate action while under the effects of Berserker's Brew: " + tuple.Item2);
                            return tuple.Item1 < CheckResult.Success;
                        }
                    });
                }
            };

            return new Item[] { dragonwhisky, rotgut, berserkersBrew }[R.Next(0, 3)];
        }

        internal static int GetDCByLevel(int level) {
            return 14 + level + (level / 3);
        }

        static bool IsSlashingOrBludgeoning(CombatAction action) {
            Item obj1 = action.Item;
            DamageKind? damageKind1;
            int num1;
            if (obj1 == null) {
                num1 = 0;
            } else {
                damageKind1 = obj1.WeaponProperties?.DamageKind;
                DamageKind damageKind2 = DamageKind.Slashing;
                num1 = damageKind1.GetValueOrDefault() == damageKind2 & damageKind1.HasValue ? 1 : 0;
            }
            if (num1 == 0) {
                Item obj2 = action.Item;
                int num2;
                if (obj2 == null) {
                    num2 = 0;
                } else {
                    damageKind1 = obj2.WeaponProperties?.DamageKind;
                    DamageKind damageKind3 = DamageKind.Bludgeoning;
                    num2 = damageKind1.GetValueOrDefault() == damageKind3 & damageKind1.HasValue ? 1 : 0;
                }
                if (num2 == 0)
                    return false;
            }
            Item obj3 = action.Item;
            return (obj3 != null ? (obj3.HasTrait(Trait.VersatileP) ? 1 : 0) : 0) == 0;
        }
    }
}
