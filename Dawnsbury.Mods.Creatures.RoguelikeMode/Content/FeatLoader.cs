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
using System.IO;
using static HarmonyLib.Code;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Core.CharacterBuilder.Selections.Selected;
using System.Xml.Linq;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Kineticist;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Archetypes;
using Dawnsbury.Core.StatBlocks.Monsters.L_1;
using System.IO.Pipelines;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class FeatLoader {

        //public static FeatName RatMonarchDedication { get; } = ModManager.RegisterFeatName("Rat Monarch Dedication");
        public static FeatName PlagueRats { get; } = ModManager.RegisterFeatName("Plague Rats");
        public static FeatName SwarmLord { get; } = ModManager.RegisterFeatName("Swarm Lord");
        public static FeatName DireRats { get; } = ModManager.RegisterFeatName("RL_Dire Rats", "Dire Rats");
        public static FeatName BurrowingDeath { get; } = ModManager.RegisterFeatName("RL_Burrowing Death", "Burrowing Death");
        public static FeatName Incubator { get; } = ModManager.RegisterFeatName("RL_Incubator", "Incubator");
        public static FeatName PowerOfTheRatFiend { get; } = ModManager.RegisterFeatName("Power of the Rat Fiend");
        public static FeatName NightmareDomain { get; } = ModManager.RegisterFeatName("Nightmares");
        public static FeatName MonasticWeaponry { get; } = ModManager.RegisterFeatName("RL_MonasticWeaponry", "Monastic Weaponry");

        internal static void LoadFeats() {
            AddFeats(CreateFeats());
        }

        private static void AddFeats(IEnumerable<Feat> feats) {
            foreach (Feat feat in feats) {
                ModManager.AddFeat(feat);
            }
        }

        private static IEnumerable<Feat> CreateFeats() {
            List<Trait> classTraits = new List<Trait>();
            AllFeats.All.ForEach(ft => {
                if (ft is ClassSelectionFeat classFeat) {
                    classTraits.Add(classFeat.ClassTrait);
                }
            });

            yield return new TrueFeat(MonasticWeaponry, 1, "You have trained with the traditional weaponry of your monastery or school.",
                "You become trained in simple and martial monk weapons. When your proficiency rank for unarmed attacks increases to expert or master, your proficiency rank for these weapons increases to expert or master as well." +
                "\n\nYou can use melee monk weapons with any of your monk feats or monk abilities that normally require unarmed attacks, though not if the feat or ability requires you to use a single specific type of attack, such as Crane Stance." +
                "\n\n{b}Special{/b} If you have the Brawling Focus feat, the Critical Specialization effect also extends to your monk weapons.",
                [Trait.Monk, ModTraits.Roguelike], null)
            .WithOnSheet(sheet => {
                sheet.Proficiencies.AddProficiencyAdjustment((item) => item.Contains(Trait.MonkWeapon) && item.Contains(Trait.Martial), Trait.Simple);
            })
            .WithOnCreature(you => {
                if (you.HasFeat(FeatName.BrawlingFocus)) {
                    you.AddQEffect(new QEffect() {
                        YouHaveCriticalSpecialization = (self, item, action, defender) => item != null && item.HasTrait(Trait.MonkWeapon)
                    });
                }

                ReplaceFlurryOfBlows(you);
            });

            yield return new TrueFeat(ModManager.RegisterFeatName("RL_MonasticArcherStance", "Monastic Archer Stance"), 2, "You enter a specialized stance for a unique martial art centered around the use of a bow.",
                "While in this stance, the only Strikes you can make are those using longbows, shortbows, or bows with the monk trait. You can use Flurry of Blows with these bows. " +
                "You can use your other monk feats or monk abilities that normally require unarmed attacks with these bows when attacking within half the first range increment (normally 50 feet for a longbow and 30 feet for a shortbow), " +
                "so long as the feat or ability doesn't require a single, specific Strike." +
                "\n\nSpecial When you select this feat, you become trained in the longbow, shortbow, and any simple and martial bows with the monk trait. At later levels, your proficancy with these weapons scales with your unarmed attacks.",
                [Trait.Monk, Trait.Stance, ModTraits.Roguelike], null)
            .WithIllustration(Illustrations.MonasticArcherStance)
            .WithOnSheet(sheet => {
                sheet.Proficiencies.AddProficiencyAdjustment((item) => (item.Contains(Trait.MonkWeapon) && item.Contains(Trait.Martial) && item.Contains(Trait.Bow)) || item.ContainsOneOf([Trait.Longbow, Trait.Shortbow, Trait.CompositeLongbow, Trait.CompositeShortbow]), Trait.Simple);
            })
            .WithOnCreature(you => {
                if (!you.HasFeat(MonasticWeaponry))
                    ReplaceFlurryOfBlows(you);
            })
            .WithPermanentQEffect(null, (qfMAS) => {
                qfMAS.ProvideMainAction = self => {
                    return new ActionPossibility(new CombatAction(self.Owner, Illustrations.MonasticArcherStance, "Monastic Archer Stance", [Trait.Monk, Trait.Stance],
                        "Enter a stance.\n\nWhile in this stance, you can use your monk feats or monk abilities that normally require unarmed attacks with longbows, shortsbows and monk bows instead.\n\nYou can't enter this stance if you're wearing armour.",
                        Target.Self().WithAdditionalRestriction(self => {
                            if (self.QEffects.Any(qf => qf.Id == QEffectIds.MonasticArcherStance))
                                return "You're already in this stance.";
                            if (self.Armor.WearsArmor)
                                return "You're wearing armour.";
                            return null;
                        })) {
                        ShortDescription = "You can use your monk feats or monk abilities that normally require unarmed attacks with longbows, shortsbows and monk bows instead."
                    }
                    .WithActionCost(1)
                    .WithEffectOnSelf(user => {
                        var stance = KineticistCommonEffects.EnterStance(user, Illustrations.MonasticArcherStance,
                            "Monastic Archer Stance", "While in this stance, you can use your monk feats or monk abilities " +
                            "that normally require unarmed attacks with longbows, shortsbows and monk bows instead.", QEffectIds.MonasticArcherStance);
                        stance.PreventTakingAction = action => action.HasTrait(Trait.Strike)
                            && !((action.Item != null
                            && action.Item.HasTrait(Trait.MonkWeapon)
                            && action.Item.HasTrait(Trait.Bow)
                            && !action.Item.HasTrait(Trait.Advanced)) || new Trait?[] { Trait.Longbow, Trait.Shortbow, Trait.CompositeLongbow, Trait.CompositeShortbow }.Contains(action?.Item?.MainTrait))
                            ? "While in the monastic Archer Stance, the only Strikes you can make are those using longbows, shortbows, or bows with the monk trait." : null;  

                    })) {
                        PossibilityGroup = Constants.POSSIBILITY_GROUP_STANCES
                    }
                    ;
                };
            });

            yield return new TrueFeat(ModManager.RegisterFeatName("RL_ShootingStarStance", "Shooting Star Stance"), 2, "You enter a stance that lets you throw shuriken with lightning speed.",
                "While in this stance, you can use your monk feats or monk abilities that normally require unarmed attacks with shuriken instead.",
                [Trait.Monk, Trait.Stance, ModTraits.Roguelike], null)
            .WithIllustration(IllustrationName.SprayOfStars)
            .WithPermanentQEffect(null, (qfSSS) => {
                qfSSS.ProvideMainAction = self => {
                    return new ActionPossibility(new CombatAction(self.Owner, IllustrationName.SprayOfStars, "Shooting Star Stance", [Trait.Monk, Trait.Stance],
                        "Enter a stance.\n\nWhile in this stance, you can use your monk feats or monk abilities that normally require unarmed attacks with shuriken instead.\n\nUnlike most monk stances, you can enter this stance even if you're wearing armour.",
                        Target.Self().WithAdditionalRestriction(self => {
                            if (self.QEffects.Any(qf => qf.Name == "Shooting Star Stance"))
                                return "You're already in this stance.";
                            return null;
                        })) {
                        ShortDescription = "You can use your monk feats or monk abilities that normally require unarmed attacks with shuriken instead."
                    }
                    .WithActionCost(1)
                    .WithEffectOnSelf(user => {
                        var stance = KineticistCommonEffects.EnterStance(user, IllustrationName.SprayOfStars,
                            "Shooting Star Stance", "While in this stance, you can use your monk feats or monk abilities " +
                            "that normally require unarmed attacks with shuriken instead.", QEffectIds.ShootingStarStance);;
                        
                    })) {
                        PossibilityGroup = Constants.POSSIBILITY_GROUP_STANCES
                    }
                    ;
                };
            })
            .WithPrerequisite(sheet => sheet.HasFeat(MonasticWeaponry), "You must have the Monastic Weaponry feat.");

            yield return new TrueFeat(ModManager.RegisterFeatName("RL_PeafowlStance", "Peafowl Stance"), 4, "You enter a tall and proud stance while remaining mobile, with all the grace and composure of a peafowl.",
                "While in this stance, the only Strikes you can make are melee Strikes with the required sword. Once per round, after you hit with a monk sword Strike, you can Step as a free action as your next action.",
                [Trait.Monk, ModTraits.Roguelike], null)
            .WithOnCreature(you => {
                you.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        return new ActionPossibility(new CombatAction(self.Owner, IllustrationName.Bird256, "Peafowl Stance", [Trait.Monk, Trait.Stance],
                            "{b}Requirements{/b} You are wielding a sword that has the monk trait in one hand.\n\nEnter a stance.\n\nWhile in this stance, the only Strikes you can make are melee Strikes with the required sword. Once per round, after you hit with a monk sword Strike, you can Step as a free action as your next action.\n\nUnlike most monk stances, you can enter this stance even if you're wearing armour.",
                            Target.Self().WithAdditionalRestriction(self => {
                                if (self.QEffects.Any(qf => qf.Name == "Peafowl Stance"))
                                    return "You're already in this stance.";
                                else if (!self.HeldItems.Any(item => item.HasTrait(Trait.Sword) && item.HasTrait(Trait.MonkWeapon) && !item.HasTrait(Trait.TwoHanded))) return "You must be wielding a sword that has the monk trait in one hand.";
                                return null;
                            })) {
                            ShortDescription = "Enter a stance where the only Strikes you can make are melee Strikes with one handed monk swords. Once per round, after you hit with a monk sword Strike, you can Step as a free action as your next action."
                        }
                        .WithActionCost(1)
                        .WithEffectOnSelf(user => {
                            var stance = KineticistCommonEffects.EnterStance(user, IllustrationName.Bird256,
                                "Peafowl Stance", "While in this stance, the only Strikes you can make are melee Strikes with one handed monk swords. Once per round, after you hit with a monk sword Strike, you can Step as a free action as your next action.");
                            stance.AfterYouTakeActionAgainstTarget = async (_, action, target, result) => {
                                if (stance.Owner.QEffects.Any(qf => qf.Key == "Peafowl Stance Step Used")) return;
                                if (action.HasTrait(Trait.Strike) && action.Item != null && action.Item.HasTrait(Trait.Sword) && action.Item.HasTrait(Trait.MonkWeapon) && !action.Item.HasTrait(Trait.TwoHanded)) {
                                    // Add a temp QF to limit use until next round
                                    if (await stance.Owner.Battle.AskForConfirmation(stance.Owner, stance.Illustration!, "Would you like to use your one free step action per round provided by Peafowl Stance?", "Yes")) {
                                        await stance.Owner.StepAsync("Peafowl Stance", false, true);
                                        stance.Owner.AddQEffect(new QEffect() { Key = "Peafowl Stance Step Used" }.WithExpirationAtStartOfOwnerTurn());
                                    }
                                }
                            };
                            stance.PreventTakingAction = action => action.HasTrait(Trait.Strike) && (action.Item == null || !action.Item.HasTrait(Trait.Sword) || !action.Item.HasTrait(Trait.MonkWeapon) || action.Item.HasTrait(Trait.TwoHanded) || !action.HasTrait(Trait.Melee)) ?
                            "You can only strike with monk swords while in Peafowl Stance." : null;
                            stance.StateCheck = _ => {
                                if (!stance.Owner.HeldItems.Any(item => item.HasTrait(Trait.Sword) && item.HasTrait(Trait.MonkWeapon) && !item.HasTrait(Trait.TwoHanded)))
                                    stance.ExpiresAt = ExpirationCondition.Immediately;
                            };
                        })) {
                            PossibilityGroup = Constants.POSSIBILITY_GROUP_STANCES
                        };
                    }
                });
            })
            .WithIllustration(IllustrationName.Bird256)
            .WithPrerequisite(sheet => sheet.HasFeat(MonasticWeaponry), "You must have the Monastic Weaponry feat.");

            //yield return new TrueFeat(ModManager.RegisterFeatName("RL_AdvancedMonasticWeaponry", "Advanced Monastic Weaponry"), 6, "Your rigorous training regimen allows you to wield complex weaponry with ease.",
            //    "For the purposes of proficiency, you treat advanced monk weapons as if they were martial monk weapons.",
            //    [Trait.Monk, ModTraits.Roguelike], null)
            //.WithOnSheet(sheet => {
            //    sheet.Proficiencies.AddProficiencyAdjustment((item) => item.Contains(Trait.MonkWeapon) && item.Contains(Trait.Advanced), Trait.Simple);
            //})
            //.WithPrerequisite(sheet => sheet.HasFeat(MonasticWeaponry), "You must have the Monastic Weaponry feat.");

            var nightmareDomain = ClericClassFeatures.CreateDomain(NightmareDomain, "You fill minds with horror and dread.", SpellLoader.WakingNightmare, SpellLoader.SharedNightmare);
            nightmareDomain.Traits.Add(ModTraits.Roguelike);
            ClericClassFeatures.AllDomainFeats.Add(nightmareDomain);

            //AllFeats.All.Find(ft => ft.FeatName == FeatName.AdvancedDomain).Subfeats.Add(CreateAdvancedDomainFeat(Trait.Cleric, nightmareDomain));
            //AllFeats.All.Find(ft => ft.FeatName == FeatName.AdvancedDeitysDomain).Subfeats.Add(CreateAdvancedDomainFeat(Trait.Champion, nightmareDomain));
            //AllFeats.All.Find(ft => ft.FeatName == FeatName.DomainFluency).Subfeats.Add(CreateAdvancedDomainFeat(Trait.Oracle, nightmareDomain));
            yield return nightmareDomain;

            yield return CreateAdvancedDomainFeat(Trait.Cleric, nightmareDomain);
            yield return CreateAdvancedDomainFeat(Trait.Champion, nightmareDomain);
            yield return CreateAdvancedDomainFeat(Trait.Oracle, nightmareDomain);

            var echidnaDiety = new DeitySelectionFeat(ModManager.RegisterFeatName("Diety: The Echidna"),
                "While the other deities busied themselves crafting the sky, peoples and land of Our Point of Light, The The Echidna refused to collaborate with their efforts, instead crafting all manner of unusual and grotesque" +
                " creations to populate the plane. Whilst The The Echidna's followers insists that her creation are just as valuable as any other creature, with many unique and" +
                " exciting characteristics that exemplify their goddess' passion and creativity, their often violent and predatory nature is a topic of great controversy that makes The The Echidna a particularly unpopular deity." +
                "\n\nDespite these differences, The The Echidna has always been quick to offer succour to outcasts and pariahs, insisting that all creatures should be celebrated for their differences, regardless of their heritage or" +
                " unsightly ailments. She is also often worshipped as a goddess of fertility, or invoked by scholars to seek twisted wisdom from nightmares.",
                "{b}• Edicts{/b} Find beauty and cuteness in The The Echidna's cherished creations, bring power to outcasts and the downtrodden, reveal the corruption and flaws in those with the hubris to who claim perfection.\n" +
                "{b}• Anathema{/b} Ostracise another for a deforminity or mental illness, contribute to the extinction of endangered monstrous species.",
                [NineCornerAlignment.ChaoticEvil, NineCornerAlignment.ChaoticNeutral, NineCornerAlignment.ChaoticGood, NineCornerAlignment.NeutralEvil],
                [FeatName.HealingFont, FeatName.HarmfulFont], [FeatName.DomainFamily, FeatName.DomainMight, NightmareDomain], ItemName.Ranseur, [SpellId.MagicFang, SpellId.SummonAnimal, SpellId.AnimalForm, SpellId.Confusion], Skill.Intimidation);

            echidnaDiety.Traits.Add(ModTraits.Roguelike);

            AllFeats.All.Find(ft => ft.FeatName == FeatName.Cleric)?.Subfeats?.Add(echidnaDiety);
            yield return echidnaDiety;

            yield return new Feat(PowerOfTheRatFiend, null, "", [], null);

            //yield return 

            FeatName Drow = ModManager.RegisterFeatName("RL_Drow_ElfHeritage", "Drow");
            FeatName GreaterDrowResilience = ModManager.RegisterFeatName("RL_Greater Drow Resilience", "Greater Drow Resilience");
            FeatName DrowWeaponFamiliarity = ModManager.RegisterFeatName("RL_DrowWeaponFamiliarity", "Drow Weapon Familiarity");
            FeatName DrowTerrorTactics = ModManager.RegisterFeatName("RL_DrowTerrorTactics", "Drow Terror Tactics");
            FeatName ChildOfTheSpider = ModManager.RegisterFeatName("RL_ChildOfTheSpider", "Child of the Spider");
            FeatName DrowMagic = ModManager.RegisterFeatName("RL_DrowMagic", "Drow Magic");
            FeatName SpiderAffinity = ModManager.RegisterFeatName("RL_SpiderAffinity", "Spider Affinity");
            FeatName DrowLethargyPoisoner = ModManager.RegisterFeatName("RL_LegargyPoisoner", "Drow Legargy Poisoner");

            // Drow Heritage
            var drow = new HeritageSelectionFeat(Drow, "Said to be the ancestors of the Demon Queen of Spider's subjects before her apotheosis to the ranks of the Starborn, that followed her down on her pilgrimage to the other side. " +
                "Exactly what happened to them is lost to time, but reports soon followed of sinister elves with striking lavander skin and violet eyes emerging from the Below to raid and pillage.\n\n" +
                "Though may drow still worship the Demon Queen of Spiders, it is not unheard of to encounter defectors who have cast off the cruel ways of their people to life a more fulfilling life, or otherwise oppose the Starborn in the hopes of seizing power within drow high society for themselves.",
                "You gain a +1 status bonus to saves against mental effects and cantrips.", [ModTraits.Drow])
            .WithOnCreature(creature => {
                creature.AddQEffect(new QEffect(creature.HasFeat(GreaterDrowResilience) ? "Greater " : "" + "Drow Resilience", creature.HasFeat(GreaterDrowResilience) ? "+2 status bonus to vs. mental saves; +1 status bonus vs. spells" : "+1 status bonus vs. mental saves; +1 status bonus vs. cantrips") {
                    BonusToDefenses = (self, action, defence) => {
                        if (action == null) {
                            return null;
                        }

                        if (action.HasTrait(Trait.Mental) && !(action.SpellId != SpellId.None && action.Owner.HeldItems.Any(item => item.ItemName == CustomItems.StaffOfSpellPenetration)) && defence != Defense.AC) {
                            return new Bonus(creature.HasFeat(GreaterDrowResilience) ? 2 : 1, BonusType.Status, self.Name!);
                        }

                        if ((action.HasTrait(Trait.Cantrip) || (creature.HasFeat(GreaterDrowResilience) && action.HasTrait(Trait.Spell))) && !action.Owner.HasEffect(QEffectId.SpellPenetration) && defence != Defense.AC) {
                            return new Bonus(1, BonusType.Status, self.Name!);
                        }

                        return null;
                    }
                });
            });
            AllFeats.All.Find(ft => ft.FeatName == FeatName.Elf)?.Subfeats?.Add(drow);
            yield return drow;

            yield return new TrueFeat(DrowWeaponFamiliarity, 1, "You're trained in the archetypal weaponry of the cruel and secretive drow.", "You are trained with repeating hand crossbows, rapiers and whips.\n\nIn addition, for the purposes of determining your proficiency, you treat whips and rapiers as simple weapons and repeating hand crossbows as martial weapons.", [ModTraits.Drow, Trait.Elf])
            .WithOnSheet(sheet => {
                sheet.Proficiencies.Set(Trait.RepeatingHandCrossbow, Proficiency.Trained);
                sheet.Proficiencies.Set(Trait.Whip, Proficiency.Trained);
                sheet.Proficiencies.Set(Trait.Rapier, Proficiency.Trained);

                Trait[] drowWeapons = [Trait.RepeatingHandCrossbow, Trait.Rapier, Trait.Whip];

                sheet.Proficiencies.AddProficiencyAdjustment((item) => item.ContainsOneOf(drowWeapons) && item.Contains(Trait.Martial), Trait.Simple);
                sheet.Proficiencies.AddProficiencyAdjustment((item) => item.ContainsOneOf(drowWeapons) && item.Contains(Trait.Advanced), Trait.Martial);
            })
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.");

            yield return new TrueFeat(DrowTerrorTactics, 1, "Drow are taught from a young age that fear can be as lethal a weapon as any blade.", "You gain a +1 circumstance bonus to Demoralize enemies you have damaged during your turn.", [ModTraits.Drow, Trait.Elf])
            .WithPermanentQEffectAndSameRulesText(qfSelf => {
                qfSelf.AfterYouDealDamage = async (you, action, target) => {
                    if (target.Occupies == null || action == null) return;

                    target.AddQEffect(new QEffect() {
                        Id = QEffectIds.DrowTerrorTactics,
                        Source = you,
                        ExpiresAt = ExpirationCondition.ExpiresAtEndOfSourcesTurn
                    });
                };
                qfSelf.BonusToAttackRolls = (self, action, target) => target != null && action.ActionId == ActionId.Demoralize && target.FindQEffect(QEffectIds.DrowTerrorTactics)?.Source == self.Owner ? new Bonus(1, BonusType.Circumstance, "Drow terror tactics", true) : null;
            })
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.")
            .WithPrerequisite(values => values.Proficiencies.Get(Trait.Intimidation) >= Proficiency.Trained, "You must be trained in intimidation.");

            yield return new TrueFeat(ChildOfTheSpider, 5, "Since the Demon Queen of Spiders lead the drow down into the sunless depths of the Below, they've always had a deep connection with spiders.", $"You gain the ability to walk through webs unimpeded, and can cast {AllSpells.CreateModernSpellTemplate(SpellId.Web, Trait.Elf, 2).ToSpellLink()} as a 2nd level innate spell once per day. In addition, you can intimidate spiders even if you don't share a language.", [ModTraits.Drow, Trait.Elf])
            .WithOnSheet(sheet => { sheet.SetProficiency(Trait.Spell, Proficiency.Trained); })
            .WithOnCreature(creature => {
                creature.AddQEffect(new QEffect("Child of the Spider", "You can move through webs unimpeded and ignore the penalty to demoralize spiders that do not speak common.") {
                    BonusToSkillChecks = (skill, action, target) =>
                        target != null && action.ActionId ==  ActionId.Demoralize && !action.Owner.HasFeat(FeatName.IntimidatingGlare) && target.HasTrait(ModTraits.Spider) && target.DoesNotSpeakCommon ? new Bonus(4, BonusType.Circumstance, "Spider affinity") : null,
                    Id = QEffectId.IgnoresWeb
                });
                var source = creature.GetOrCreateSpellcastingSource(SpellcastingKind.Innate, Trait.Elf, Ability.Charisma, Trait.Divine);
                source.WithSpells([SpellId.Web], 2);
            })
            .WithRulesBlockForSpell(SpellId.Web, Trait.Elf)
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.");

            yield return new TrueFeat(DrowMagic, 5, "The demonic magic running through your veins grants you access to the ability to both dispel and invoke duplicity.",
                $"You may cast both the {AllSpells.CreateModernSpellTemplate(SpellId.FaerieFire, Trait.Elf, 2).ToSpellLink()} and {AllSpells.CreateModernSpellTemplate(SpellId.ObscuringMist, Trait.Elf, 2).ToSpellLink()} spells once per day as innate 2nd level spells.", [ModTraits.Drow, Trait.Elf])
            .WithOnSheet(sheet => { sheet.SetProficiency(Trait.Spell, Proficiency.Trained); })
            .WithOnCreature(creature => {
                var source = creature.GetOrCreateSpellcastingSource(SpellcastingKind.Innate, Trait.Elf, Ability.Charisma, Trait.Divine);
                source.WithSpells([SpellId.FaerieFire], 2);
                source.WithSpells([SpellId.ObscuringMist], 2);
            })
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.");

            yield return new TrueFeat(GreaterDrowResilience, 5, "Your natural resilience from magic and trickery grows stronger.",
                $"Your drow resilience now protects you against all spells, not just cantrips, and you gain a +2 status bonus against mental effects instead of a +1.", [ModTraits.Drow, Trait.Elf])
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.");

            yield return new TrueFeat(SpiderAffinity, 9, "Spiders yield to the dirge of demonic power in your blood, coming to your defence when called upon for aid.", "Once per day, you may cast {i}summon spider{/i} as an innate spell, heightened to the highest slot level.", [ModTraits.Drow, Trait.Elf])
            .WithOnCreature(creature => {
                var source = creature.GetOrCreateSpellcastingSource(SpellcastingKind.Innate, Trait.Elf, Ability.Charisma, Trait.Divine);
                source.WithSpells([SpellLoader.SummonSpider], (creature.Level + 1) / 2);
            })
            .WithRulesBlockForSpell(SpellLoader.SummonSpider, Trait.Elf)
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.");

            yield return new TrueFeat(DrowLethargyPoisoner, 9, "Lethargy poison is commonly used in hit-and-run tactics by drow; the ambusher retreats until the poison sets in and the victim falls unconscious.", "Once per day, you may coat a weapon you are holding with Drow Lethargy Poison", [ModTraits.Drow, Trait.Elf])
            .WithOnCreature(creature => {
                creature.AddQEffect(new QEffect() {
                    StartOfCombat = async self => {
                        if (self.Owner.PersistentUsedUpResources.UsedUpActions.Contains("DrowLethargyPoisoner"))
                            self.Name += " (expended)";
                    },
                    ProvideMainAction = self => {

                        if (self.Owner.PersistentUsedUpResources.UsedUpActions.Contains("DrowLethargyPoisoner")) return null;

                        bool IsValidTargetForPoison(Item wp) {
                            return wp.HasTrait(Trait.Weapon) && wp.WeaponProperties != null && !wp.Runes.Any(rune => rune.RuneProperties?.RuneKind == RuneKind.WeaponPoison);
                        }

                        return new ActionPossibility(new CombatAction(self.Owner, IllustrationName.AlchemicalPoison, "Apply Lethargy Poison", [Trait.Manipulate, Trait.Concentrate],
                            "{b}Frequency{/b} Once per day\n\nUsing a free hand, apply drow lethargy poison to a weapon you're carrying.\n\n" +
                            $"{{b}}Drow Lethargy Poison (DC {SkillChallengeTables.GetDCByLevel(self.Owner.Level)}){{/b}}\n" +
                            "{b}Stage 1{/b} slowed 1; {b}Stage 2{/b} slowed 1 for rest of encounter.",
                            Target.Self().WithAdditionalRestriction(user => !user.HasFreeHand ? "no-free-hand" : !user.HeldItems.Any(wp => IsValidTargetForPoison(wp)) ? "no-poisonable-weapons" : null))
                            .WithActionCost(1)
                            .WithSoundEffect(SfxName.ItemGet)
                            .WithEffectOnSelf(async (action, user) => {
                                var dc = SkillChallengeTables.GetDCByLevel(self.Owner.Level);

                                var poison = new Item(IllustrationName.AlchemicalPoison, "drow lethargy poison", [Trait.Alchemical, Trait.Consumable, Trait.Poison]) {
                                    AlchemicalDC = dc
                                }
                                .WithRuneProperties(new RuneProperties(
                                    "poisoned",
                                    RuneKind.WeaponPoison,
                                    null,
                                    $"On a hit, the target contracts drow lethargy poison (DC {dc} Fortitude save negates).",
                                    (rune, item) => {
                                        item.Traits.Add(Trait.Poisoned);
                                        item.WeaponProperties!.AdditionalSuccessDescription += $" {{Blue}}On a hit, the target contracts drow lethargy poison (DC {dc} Fortitude save negates).{{/}}";
                                        item.WeaponProperties!.AdditionalPoisonOnTarget = async (spell, caster, target, result) => {
                                            if (result >= CheckResult.Success) {
                                                var affliction = new Affliction(QEffectIds.LethargyPoison, "Drow Lethargy Poison", dc, "{b}Stage 1{/b} slowed 1; {b}Stage 2{/b} slowed 1 for rest of encounter", 2, dmg => null, qf => {
                                                    if (qf.Value == 1) {
                                                        qf.Owner.AddQEffect(QEffect.Slowed(1).WithExpirationEphemeral());
                                                    }

                                                    if (qf.Value == 2) {
                                                        QEffect nEffect = QEffect.Slowed(1).WithExpirationNever();
                                                        nEffect.CounteractLevel = qf.CounteractLevel;
                                                        qf.Owner.AddQEffect(nEffect);
                                                        qf.Owner.RemoveAllQEffects(qf2 => qf2.Id == QEffectIds.LethargyPoison);
                                                        qf.Owner.Overhead("*lethargy poison converted to slowed 1*", Color.Black);
                                                    }
                                                });

                                                await Affliction.ExposeToInjury(affliction, caster, target);

                                            }

                                            if (result != CheckResult.Failure) {
                                                AlchemicalItems.DestroyAllPoisonsOn(item);
                                            }
                                        };
                                    }));

                                var weapons = user.HeldItems.Where(wp => IsValidTargetForPoison(wp));
                                if (weapons.Count() == 0) {
                                    action.RevertRequested = true;
                                    return;
                                };
                                Item weapon = weapons.Count() == 1
                                ? user.HeldItems.First(IsValidTargetForPoison)
                                : (await user.Battle.AskForConfirmation(user, action.Illustration, "Which weapon would you like to poison?",
                                    user.HeldItems[0].Name, user.HeldItems[1].Name))
                                    ? user.HeldItems[0]
                                    : user.HeldItems[1];

                                RunestoneRules.AddRuneTo(poison, weapon);

                                self.Owner.PersistentUsedUpResources.UsedUpActions.Add("DrowLethargyPoisoner");
                                self.Name += " (expended)";
                            })
                            ).WithPossibilityGroup(Constants.POSSIBILITY_GROUP_ANCESTRY_POWERS);
                    },
                });
            })
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.");



            // Rat monarch

            var ratMonarch = ArchetypeFeats.CreateAgnosticArchetypeDedication(ModTraits.RatMonarch, "The Rat Monarch lords over their flock of rodents, that emerge from the most neglected corners of the abyss to fulfill their master's will.",
                "You gain the following actions, allowing to summon forth and direct a swarm of vicious rats:\n\n" +
                "{b}Call Rats {icon:TwoActions}.{/b}\n" +
                "{b}Range{/b} self\n\nSpawn 2 friendly rat familiars into the nearest unoccupied space available to you." +
                "Your familiars have the base statistics of a Giant Rat, but their level is equal to your own -2, adjusting their defences and attack bonuses and increasing their max HP by 3 per level.\n\n" +
                "{b}Command Swarm {icon:Action}.{/b}\n" +
                "{b}Range{/b} 30 feet\n{b}Target{/b} 1 enemy creature\n\n" +
                "You mark the target creature with a demonic curse, attracting your rat familiars to them. While marked, your familiars gain a +1 status bonus to attack, and a +2 damage bonus against the target and are more likely to attack them."
                , null)
            .WithOnCreature((sheet, creature) => {
                creature.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        var action = (ActionPossibility)new CombatAction(self.Owner, IllustrationName.SummonAnimal, "Call Rats", new Trait[] { Trait.Conjuration, Trait.Manipulate },
                            "{b}Range{/b} self\n\nSpawn 2 friendly rat familiars into the nearest unoccupied space available to you.\n\n" +
                            "Your familiars have the base statistics of a Giant Rat, but their level is equal to your own -2, adjusting their defences and attack bonuses and increasing their max HP by 3 per level.", Target.Self()) {
                            ShortDescription = "Summoner 3 familirs into nearby unoccupied spaces."
                        }
                        .WithActionCost(2)
                        .WithSoundEffect(SfxName.ScratchFlesh)
                        .WithEffectOnSelf(async (action, caster) => {
                            for (int i = 0; i <= 1; i++) {
                                SpawnRatFamiliar(self.Owner);
                            }
                        });
                        action.PossibilityGroup = "Rat Monarch";
                        action.PossibilitySize = PossibilitySize.Half;
                        return action;
                    },
                });

                creature.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        var action = (ActionPossibility)new CombatAction(self.Owner, IllustrationName.Command, "Command Swarm", new Trait[] { Trait.Manipulate },
                            "{b}Range{/b} 30 feet\n{b}Target{/b} 1 enemy creature\n\n" +
                            "You mark the target creature with a demonic curse, attracting your rat familiars to them. While marked, your familiars gain a +1 status bonus to attack, and a +2 damage bonus against the target and are more likely to attack them.", Target.Ranged(6)) {
                            ShortDescription = "Command your rat familiars to attack the target creature, granting them a +1 status bonus to attack rolls, and a +2 bonus to damage against them until you move the mark."
                        }
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.ScratchFlesh)
                        .WithEffectOnEachTarget(async (action, caster, target, result) => {
                            caster.Battle.AllCreatures.ForEach(cr => cr.RemoveAllQEffects(qf => qf.Id == QEffectIds.CommandSwarm && qf.Source == caster));
                            QEffect effect = new QEffect("Marked for the Swarm",
                                $"{caster.Name}'s rat familiars are drawn to you, causing them to deal +2 damage and gain a +1 bonus to attack rolls.",
                                ExpirationCondition.Never, caster, IllustrationName.GiantRat256) { Id = QEffectIds.CommandSwarm };
                            effect.AddGrantingOfTechnical(cr => cr.QEffects.Any(qf => qf.Id == QEffectIds.RatFamiliar && qf.Source == caster), qfMark => {
                                qfMark.AdditionalGoodness = (self, action, d) => d.QEffects.Any(qf => qf.Id == QEffectIds.CommandSwarm && qf.Source == caster) ? 20f : -5f;
                                qfMark.BonusToAttackRolls = (self, action, d) => d != null && d.QEffects.Any(qf => qf.Id == QEffectIds.CommandSwarm && qf.Source == caster) ? new Bonus(1, BonusType.Status, "Marked for the Swarm", true) : null;
                                qfMark.BonusToDamage = (self, action, d) => action.HasTrait(Trait.Strike) && d.QEffects.Any(qf => qf.Id == QEffectIds.CommandSwarm && qf.Source == caster) ? new Bonus(2, BonusType.Untyped, "Marked for the Swarm", true) : null;
                            });
                            target.AddQEffect(effect);
                        });
                        action.PossibilityGroup = "Rat Monarch";
                        action.PossibilitySize = PossibilitySize.Half;
                        return action;
                    },
                });
            })
            .WithPrerequisite(sheet => {
                //if (CampaignState.Instance?.AdventurePath?.Name == "Roguelike Mode" && CampaignState.Instance.Heroes.First(h => h.CharacterSheet == sheet.Sheet).LongTermEffects.Effects.Any(lte => lte.Id.ToStringOrTechnical() == "Power of the Rat Fiend")) {
                if (sheet.Sheet.SelectedFeats.TryGetValue("Power of the Rat Fiend", out SelectedChoice val)) {
                    return true;
                } else {
                    return false;
                }
            }, "This archetype can only be taken by one who has stolen the power of the Rat Fiend.");

            ratMonarch.Traits.Add(ModTraits.Event);
            ratMonarch.Traits.Add(ModTraits.Roguelike);

            yield return ratMonarch;

            yield return new TrueFeat(PlagueRats, 4, "Your rats grow larger and more vicious, carrying a wasting otherwordly plague.",
                "Your rat familiars gain +5 HP and inflict Rat Plague on a successful jaws attack, an affliction with a DC equal to the higher of your class or spell DC.\n\n{b}Rat Plague{/b}\n{b}Stage 1{/b} 1d6 poison damage and enfeebled 1; {b}Stage 2{/b} 2d6 poison damage and enfeebled 2; {b}Stage 3{/b} 3d6 poison damage and enfeebled 2.",
                new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            .WithAvailableAsArchetypeFeat(ModTraits.RatMonarch);

            yield return new TrueFeat(SwarmLord, 4, "You and your rats fight as one.",
                "When you hit a creature with an attack roll, each adjacent rat familiar may use its reaction to make a jaws attack against them. Attacks triggered by ranged attacks suffer a -2 penalty.",
                new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            .WithAvailableAsArchetypeFeat(ModTraits.RatMonarch)
            .WithOnCreature((sheet, creature) => {
                creature.AddQEffect(new QEffect("Swarm Lord", "When you hit a creature with an attack roll, each adjacent rat familiar may use its reaction to make a jaws attack against them. Attacks triggered by ranged attacks suffer a -2 penalty.") {
                    AfterYouTakeActionAgainstTarget = async (self, action, target, result) => {
                        if (!action.HasTrait(Trait.Attack) || result <= CheckResult.Failure) {
                            return;
                        }
                        bool ranged = !self.Owner.IsAdjacentTo(target) && (action.Target is not CreatureTarget || action.Target is CreatureTarget ct && ct.RangeKind == RangeKind.Ranged);

                        foreach (Creature rat in self.Owner.Battle.AllCreatures.Where(cr => cr.QEffects.Any(qf => qf.Id == QEffectIds.RatFamiliar && qf.Source == self.Owner) && cr.IsAdjacentTo(target))) {
                            if (!await rat.AskToUseReaction("Would you like to attack the target of your master's attack?")) {
                                return;
                            }
                            
                            StrikeModifiers mod = new StrikeModifiers();
                            if (ranged) {
                                mod.AdditionalBonusesToAttackRoll = new List<Bonus>() { new Bonus(-2, BonusType.Untyped, "Ranged Trigger") };
                            }
                            CombatAction ca = rat.CreateStrike(rat.UnarmedStrike, 0, mod).WithActionCost(0);
                            ca.ChosenTargets.ChosenCreature = target;
                            ca.ChosenTargets.ChosenCreatures.Add(target);
                            await ca.AllExecute();
                        }
                    }
                });
            });

            yield return new TrueFeat(BurrowingDeath, 6, "You command your rats to swarm over the subject of your ire, burrowing their into their flesh with suicidal determination.",
                "You learn the {i}burrowing death{/i} focus spell. Increase the number of Focus Points in your focus pool by 1.\n\n{b}Special.{/b} Your Rat Monarch spellcasting DC is equal to your highest class or spell save DC.",
                new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            .WithAvailableAsArchetypeFeat(ModTraits.RatMonarch)
            .WithActionCost(2)
            .WithIllustration(Illustrations.BurrowingDeath)
            .WithOnSheet(sheet => {
                // sheet.SetProficiency(ModTraits.RatMonarch, (Proficiency)Math.Max((int)sheet.GetProficiency(sheet.Class?.ClassTrait ?? Trait.Spell), (int)sheet.GetProficiency(Trait.Spell)));
                sheet.AddFocusSpellAndFocusPoint(ModTraits.RatMonarch, sheet.FinalAbilityScores.KeyAbility, SpellLoader.BurrowingDeath);
            })
            .WithOnCreature((sheet, creature) => {
                creature.Proficiencies.Set(ModTraits.RatMonarch, (Proficiency)Math.Max((int)creature.Proficiencies.Get(sheet.Class?.ClassTrait ?? Trait.Spell), (int)creature.Proficiencies.Get(Trait.Spell)));
            })
            .WithRulesBlockForSpell(SpellLoader.BurrowingDeath);

            yield return new TrueFeat(Incubator, 6, "The corpses of your foes bulge and writhe, as fresh subjects burrow free of their carcass to serve you.",
                "After you or one of your familiars reduces an enemy to 0 HP, you summon a rat familiar in their place.",
                new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            .WithAvailableAsArchetypeFeat(ModTraits.RatMonarch)
            .WithPermanentQEffectAndSameRulesText((qfSelf) => {
                qfSelf.AfterYouDealDamage = async (you, _, target) => {
                    if (target.HP <= 0 && target.EnemyOf(you) && target.IsLivingCreature) {
                        you.Overhead("*incubator*", Color.White, $"A rat crawls out from {target.Name}'s corpse to serve {you.Name}.");
                        SpawnRatFamiliar(you, target.Occupies);
                    }
                };
            });

            yield return new TrueFeat(DireRats, 8, "Your rats grow larger and more aggressive.",
                "Your rat familiar's jaw attack deals an extra damage die, and they gain a +1 bonus to their strength, dexterity, constitution and wisdom modifiers.",
                new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            .WithPermanentQEffectAndSameRulesText(qf => { })
            .WithAvailableAsArchetypeFeat(ModTraits.RatMonarch);
        }

        //internal static Feat CreateAdvancedDomainFeat(Trait forClass, Feat domainFeat) {
        //    var name = domainFeat.FeatName.HumanizeTitleCase2();
        //    var advancedSpell = (SpellId)domainFeat.Tag!;
        //    //var spell = AllSpells.All.Concat((List<SpellId>)typeof(ModManager).GetProperty("NewSpells", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null)).FirstOrDefault(spell => spell.SpellId == advancedSpell);
        //    var spell = AllSpells.CreateModernSpellTemplate(advancedSpell, forClass);
        //    return new Feat(ModManager.RegisterFeatName("AdvancedDomain:" + forClass.HumanizeTitleCase2() + ":" + name, name + ": " + spell.Name), "Your studies or prayers have unlocked deeper secrets of the " + name.ToLower() + " domain.",
        //        $"You learn the {forClass.HumanizeTitleCase2().ToLower()} focus spell " + AllSpells.CreateSpellLink(advancedSpell, forClass) + ", and you gain 1 focus point, up to a maximum 3.", [], null)
        //    .WithIllustration(spell.Illustration)
        //    .WithRulesBlockForSpell(advancedSpell, forClass)
        //    .WithPrerequisite(values => values.HasFeat(domainFeat.FeatName), "You must have the " + name + " domain.")
        //    .WithOnSheet(sheet => {
        //        if (sheet.Sheet.Class?.ClassTrait == Trait.Cleric) {
        //            sheet.AddFocusSpellAndFocusPoint(Trait.Cleric, Ability.Wisdom, advancedSpell);
        //        } else if (sheet.Sheet.Class?.ClassTrait == Trait.Champion) {
        //            sheet.AddFocusSpellAndFocusPoint(Trait.Champion, Ability.Charisma, advancedSpell);
        //        } else if (sheet.Sheet.Class?.ClassTrait == Trait.Oracle) {
        //            sheet.AddFocusSpellAndFocusPoint(Trait.Oracle, Ability.Charisma, advancedSpell);
        //        }
        //    });
        //}

        internal static Feat CreateAdvancedDomainFeat(Trait forClass, Feat domainFeat) {
            var name = domainFeat.FeatName.HumanizeTitleCase2();
            var advancedSpell = (SpellId)domainFeat.Tag!;
            //var spell = AllSpells.All.Concat((List<SpellId>)typeof(ModManager).GetProperty("NewSpells", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null)).FirstOrDefault(spell => spell.SpellId == advancedSpell);
            var spell = AllSpells.CreateModernSpellTemplate(advancedSpell, forClass);
            return new TrueFeat(ModManager.RegisterFeatName("AdvancedDomain:" + forClass.HumanizeTitleCase2() + ":" + name, name + ": " + spell.Name), 8, "Your studies or prayers have unlocked deeper secrets of the " + name.ToLower() + " domain.",
                $"You learn the {forClass.HumanizeTitleCase2().ToLower()} focus spell " + AllSpells.CreateSpellLink(advancedSpell, forClass) + ", and you gain 1 focus point, up to a maximum 3.", [forClass, ModTraits.Roguelike], null)
            .WithIllustration(spell.Illustration)
            .WithRulesBlockForSpell(advancedSpell, forClass)
            .WithPrerequisite(values => values.HasFeat(domainFeat.FeatName), "You must have the " + name + " domain.")
            .WithOnSheet(sheet => {
                if (sheet.Sheet.Class?.ClassTrait == Trait.Cleric) {
                    sheet.AddFocusSpellAndFocusPoint(Trait.Cleric, Ability.Wisdom, advancedSpell);
                } else if (sheet.Sheet.Class?.ClassTrait == Trait.Champion) {
                    sheet.AddFocusSpellAndFocusPoint(Trait.Champion, Ability.Charisma, advancedSpell);
                } else if (sheet.Sheet.Class?.ClassTrait == Trait.Oracle) {
                    sheet.AddFocusSpellAndFocusPoint(Trait.Oracle, Ability.Charisma, advancedSpell);
                }
            });
        }

        internal static void SpawnRatFamiliar(Creature master, Tile? location=null) {
            Creature rat = GiantRat.CreateGiantRat();
            rat.MainName = master.MainName + "'s Rat Familiar";
            rat.Level = master.Level - 2;
            rat.MaxHP += (master.Level - 1) * 3;
            rat.Defenses.Set(Defense.AC, rat.Defenses.GetBaseValue(Defense.AC) + master.Level - 1);
            rat.Defenses.Set(Defense.Fortitude, rat.Defenses.GetBaseValue(Defense.Fortitude) + master.Level - 1);
            rat.Defenses.Set(Defense.Reflex, rat.Defenses.GetBaseValue(Defense.Reflex) + master.Level - 1);
            rat.Defenses.Set(Defense.Will, rat.Defenses.GetBaseValue(Defense.Will) + master.Level - 1);
            rat.Initiative += master.Level - 1;
            rat.ProficiencyLevel += master.Level - 1;
            rat.AddQEffect(new QEffect("Familiar", "This creature is permanantly slowed 1.") { Id = QEffectId.Slowed, Value = 1 });
            rat.AddQEffect(CommonQEffects.CantOpenDoors());
            rat.AddQEffect(new QEffect() {
                Id = QEffectIds.RatFamiliar,
                Source = master,
                StateCheck = self => {
                    if (self.Owner.HP == 0) {
                        self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                    }
                }
            });
            // Check for super rat feat here
            if (master.HasFeat(PlagueRats)) {
                rat.MaxHP += 5;
                rat.AddQEffect(CommonQEffects.RatPlagueAttack(master, "jaws"));
            }
            if (master.HasFeat(Incubator)) {
                rat.AddQEffect(new QEffect("Incubator", "On reducing an enemy to 0 HP, the rat familiar summons another rat familiar under its master's control.") {
                    AfterYouDealDamage = async (you, _, target) => {
                        if (target.HP <= 0 && target.EnemyOf(master) && target.IsLivingCreature) {
                            you.Overhead("*incubator*", Color.White, $"A rat crawls out from {target.Name}'s corpse to serve {master.Name}.");
                            SpawnRatFamiliar(master, target.Occupies);
                        }
                    }
                });
            }

            if (master.HasFeat(DireRats)) {
                rat.UnarmedStrike.WeaponProperties!.DamageDieCount = 2;
                rat.Abilities.Set(Ability.Dexterity, rat.Abilities.Dexterity + 1);
                rat.Abilities.Set(Ability.Strength, rat.Abilities.Strength + 1);
                rat.Abilities.Set(Ability.Constitution, rat.Abilities.Constitution + 1);
                rat.Abilities.Set(Ability.Wisdom, rat.Abilities.Wisdom + 1);
                rat.Defenses.Set(Defense.AC, rat.Defenses.GetBaseValue(Defense.AC) + 1);
                rat.Defenses.Set(Defense.Fortitude, rat.Defenses.GetBaseValue(Defense.Fortitude) + 1);
                rat.Defenses.Set(Defense.Reflex, rat.Defenses.GetBaseValue(Defense.Reflex) + 1);
                rat.Defenses.Set(Defense.Will, rat.Defenses.GetBaseValue(Defense.Will) + 1);

            }
            master.Battle.SpawnCreature(rat, master.Battle.GaiaFriends, location ?? master.Occupies);
        }

        internal static void ReplaceFlurryOfBlows(Creature monk) {
            monk.RemoveAllQEffects(qf => qf.ProvideMainAction != null && ((ActionPossibility)qf.ProvideMainAction(qf))?.CombatAction?.Name == "Flurry of Blows");

            monk.AddQEffect(new QEffect() {
                ProvideMainAction = qfSelf => {
                    return (ActionPossibility)new CombatAction(qfSelf.Owner, IllustrationName.FlurryOfBlows,
                        "Flurry of Blows", [Trait.Monk, Trait.Flourish],
                        "Make two unarmed or shuriken Strikes.\n\nIf both hit the same creature, " +
                        "combine their damage for the purpose of resistances and weaknesses. Apply your multiple attack penalty to the Strikes normally." +
                        "\n\nAs it has the flourish trait, you can use Flurry of Blows only once per turn.",
                    Target.Self()
                    .WithAdditionalRestriction(user => {
                        if (user.QEffects.Any(qf => qf.Id == QEffectIds.ShootingStarStance)) {
                            Item? shuriken = null;

                            if (user.CarriedItems.Any(item => item.ItemName == CustomItems.Shuriken) || user.CarriedItems.FirstOrDefault(item => item.ItemName == CustomItems.ThrowersBandolier && item.IsWorn) != null) {
                                shuriken = Items.CreateNew(CustomItems.Shuriken);
                                CombatAction combatAction = StrikeRules.CreateStrike(user, shuriken, RangeKind.Ranged, -1, true);
                                var result = combatAction.CanBeginToUse(user);

                                if (user.HasFreeHand && shuriken != null && combatAction.CanBeginToUse(user).CanBeUsed)
                                    return null;
                            }
                        }

                        if (user.QEffects.Any(qf => qf.Id == QEffectIds.MonasticArcherStance)) {
                            Item? bow = user.HeldItems.FirstOrDefault(wpn => (wpn.HasTrait(Trait.MonkWeapon) && wpn.HasTrait(Trait.Bow) && !wpn.HasTrait(Trait.Advanced)) || new Trait[] { Trait.Longbow, Trait.Shortbow, Trait.CompositeLongbow, Trait.CompositeShortbow }.Contains(wpn.MainTrait));
                            if (bow != null) {
                                CombatAction combatAction = StrikeRules.CreateStrike(user, bow, RangeKind.Ranged, -1, true);
                                if (combatAction.CanBeginToUse(user) && (bow.HasTrait(Trait.TwoHanded) || user.HasFreeHand)) {
                                    return null;
                                }
                            }
                        }

                        if ((!user.CanMakeBasicUnarmedAttack
                        && user.QEffects.All(qf => qf.AdditionalUnarmedStrike == null)
                        && !user.HeldItems.Any(wp => wp.HasTrait(Trait.MonkWeapon)))
                        || user.PrimaryWeapon == null
                        || user.QEffects.Any(qf => qf.PreventTakingAction != null && qf.PreventTakingAction(user.CreateStrike(user.PrimaryWeapon)) != null))
                            return $"You must be able to make a melee unarmed{(user.HasFeat(MonasticWeaponry) ? " or monk weapon" : "")} strike{(user.QEffects.Any(qf => qf.Id == QEffectIds.ShootingStarStance || qf.Id == QEffectIds.MonasticArcherStance) ? " or ranged attack appropriate to your stance" : "")} to use Flurry of Blows.";

                        if (user.MeleeWeapons.Any(weapon => (weapon.HasTrait(Trait.Unarmed) || weapon.HasTrait(Trait.MonkWeapon)) && CommonRulesConditions.CouldMakeStrike(user, weapon))) {
                            return null;
                        }

                        return "There is no nearby enemy or you can't make attacks.";
                    })) {
                        ShortDescription = $"Make two unarmed{(qfSelf.Owner.HasFeat(MonasticWeaponry) ? " or monk weapon" : "")} Strikes."
                    }
                    .WithActionCost(1)
                    .WithActionId(ActionId.FlurryOfBlows)
                    .WithEffectOnEachTarget(async (spell, self, target, irrelevantResult) => {
                        var chosenCreatures = new List<Creature>();
                        int hpBefore = -1;
                        for (int i = 0; i < 2; i++) {
                            await self.Battle.GameLoop.StateCheck();
                            var possibilities = new List<Option>();

                            if (self.QEffects.Any(qf => qf.Id == QEffectIds.ShootingStarStance)) {
                                // Add shurikens
                                Item? bandolier = self.CarriedItems.FirstOrDefault(item => item.ItemName == CustomItems.ThrowersBandolier && item.IsWorn);

                                if (bandolier != null && self.CarriedItems.Any(item => item.ItemName == CustomItems.Shuriken)) {
                                    var uniqueShurikens = new List<(Item?, Item)>();
                                    int numShurikens = 0;
                                    var allShurikens = new List<(Item?, Item)>();
                                    foreach (var item in self.CarriedItems) {
                                        if (item.ItemName == CustomItems.Shuriken)
                                            allShurikens.Add((null, item));
                                        foreach (var subItem in item.StoredItems) {
                                            if (subItem.ItemName == CustomItems.Shuriken)
                                                allShurikens.Add((item, subItem));
                                        }
                                    }
                                    allShurikens.ForEach(tuple => {
                                        numShurikens += 1;
                                        bool unique = true;
                                        foreach ((Item?, Item) skn in uniqueShurikens) {
                                            if (tuple.Item2.Name == skn.Item2.Name)
                                                unique = false;
                                        }
                                        if (unique)
                                            uniqueShurikens.Add(tuple);
                                    });

                                    List<CombatAction> shurikenThrows = new List<CombatAction>();
                                    foreach ((Item?, Item) shuriken in uniqueShurikens) {
                                        var strike = StrikeRules.CreateStrike(self, shuriken.Item2, RangeKind.Ranged, -1, true).WithActionCost(0);
                                        (strike.Target as CreatureTarget)?.WithAdditionalConditionOnTargetCreature((a, d) => a.HasFreeHand ? Usability.Usable : Usability.NotUsable("no-free-hand"));
                                        strike.WithPrologueEffectOnChosenTargetsBeforeRolls(async (action, user, targets) => {
                                            if (shuriken.Item1 != null)
                                                shuriken.Item1.StoredItems.Remove(shuriken.Item2);
                                            else
                                                user.CarriedItems.Remove(shuriken.Item2);
                                            user.AddHeldItem(shuriken.Item2);
                                        });
                                        strike.Name += " (" + shuriken.Item2.Name + ")";
                                        shurikenThrows.Add(strike);
                                    }

                                    if (bandolier != null) {
                                        var shuriken = Items.CreateNew(CustomItems.Shuriken);
                                        shuriken.Traits.Add(Trait.EncounterEphemeral);
                                        foreach (Item rune in bandolier.Runes) {
                                            if (rune.RuneProperties?.CanBeAppliedTo == null || rune.RuneProperties?.CanBeAppliedTo(rune, shuriken) == null)
                                                shuriken.WithModificationRune(rune.ItemName);
                                        }

                                        var strike = StrikeRules.CreateStrike(self, shuriken, RangeKind.Ranged, -1, true).WithActionCost(0);
                                        (strike.Target as CreatureTarget)?.WithAdditionalConditionOnTargetCreature((a, d) => a.HasFreeHand ? Usability.Usable : Usability.NotUsable("no-free-hand"));
                                        strike.WithPrologueEffectOnChosenTargetsBeforeRolls(async (action, user, targets) => {
                                            user.AddHeldItem(shuriken);
                                        });
                                        strike.Name += " from bandolier (" + shuriken.Name + ")";
                                        shurikenThrows.Add(strike);
                                    }

                                    foreach (var strike in shurikenThrows) {
                                        GameLoop.AddDirectUsageOnCreatureOptions(strike, possibilities, false);
                                    }
                                }
                            } else if (self.QEffects.Any(qf => qf.Id == QEffectIds.MonasticArcherStance)) {
                                Item? bow = self.HeldItems.FirstOrDefault(wpn => (wpn.HasTrait(Trait.MonkWeapon) && wpn.HasTrait(Trait.Bow) && !wpn.HasTrait(Trait.Advanced)) || new Trait[] { Trait.Longbow, Trait.Shortbow, Trait.CompositeLongbow, Trait.CompositeShortbow }.Contains(wpn.MainTrait));
                                if (bow != null) {
                                    var combatAction = self.CreateStrike(bow);
                                    (combatAction.Target as CreatureTarget)?.CreatureTargetingRequirements.Add(new MaximumRangeCreatureTargetingRequirement(bow.WeaponProperties!.RangeIncrement / 2));
                                    combatAction.WithActionCost(0);
                                    GameLoop.AddDirectUsageOnCreatureOptions(combatAction, possibilities, true);
                                }
                            }

                            foreach (var item in self.MeleeWeapons.Where(weapon => weapon.HasTrait(Trait.Unarmed) || weapon.HasTrait(Trait.MonkWeapon))) {
                                var combatAction = self.CreateStrike(item);
                                combatAction.WithActionCost(0);
                                GameLoop.AddDirectUsageOnCreatureOptions(combatAction, possibilities, true);
                            }

                            if (self.HasEffect(QEffectId.FlurryOfManeuvers)) {
                                foreach (var maneuverAction in CombatManeuverPossibilities.GetAllShoveGrappleAndTripOptions(self)) {
                                    GameLoop.AddDirectUsageOnCreatureOptions(maneuverAction.WithActionCost(0), possibilities, true);
                                }
                            }

                            if (possibilities.Count > 0) {
                                Option chosenOption;
                                if (possibilities.Count >= 2 || i == 0) {
                                    if (i == 0) possibilities.Add(new CancelOption(true));
                                    var result = await self.Battle.SendRequest(new AdvancedRequest(self, "Choose a creature to Strike.", possibilities) {
                                        TopBarText = (i == 0 ? "Choose a creature to Strike or right-click to cancel. (1/2)" : "Choose a creature to Strike. (2/2)"),
                                        TopBarIcon = IllustrationName.Fist
                                    });
                                    chosenOption = result.ChosenOption;
                                } else {
                                    chosenOption = possibilities[0];
                                }

                                if (chosenOption is CreatureOption creatureOption) {
                                    if (hpBefore == -1) {
                                        hpBefore = creatureOption.Creature.HP;
                                    }

                                    chosenCreatures.Add(creatureOption.Creature);
                                }

                                if (chosenOption is CancelOption) {
                                    spell.RevertRequested = true;
                                    return;
                                }

                                await chosenOption.Action();
                            }
                        }

                        if (self.HasEffect(QEffectId.StunningFist) && (chosenCreatures.Count == 1 || chosenCreatures.Count == 2 && chosenCreatures[0] == chosenCreatures[1])) {
                            if (chosenCreatures[0].HP < hpBefore) {
                                var stunningFistAction = CombatAction.CreateSimple(self, "Stunning Fist", Trait.Incapacitation);
                                var stunningFistResult = CommonSpellEffects.RollSavingThrow(chosenCreatures[0], stunningFistAction, Defense.Fortitude, self.Proficiencies.Get(Trait.Monk).ToNumber(self.ProficiencyLevel) + self.Abilities.Get(self.Abilities.KeyAbility) + 10);
                                if (stunningFistResult <= CheckResult.Failure) {
                                    chosenCreatures[0].AddQEffect(QEffect.Stunned(stunningFistResult == CheckResult.CriticalFailure ? 3 : 1));
                                }
                            }
                        }

                        Steam.CollectAchievement("MONK");
                    });
                }
            });
        }
    }
}