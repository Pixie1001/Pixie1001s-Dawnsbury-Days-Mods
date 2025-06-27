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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class FeatLoader {

        //public static FeatName RatMonarchDedication { get; } = ModManager.RegisterFeatName("Rat Monarch Dedication");
        public static FeatName PlagueRats { get; } = ModManager.RegisterFeatName("Plague Rats");
        public static FeatName SwarmLord { get; } = ModManager.RegisterFeatName("Swarm Lord");
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
            .WithIllustration(IllustrationName.Shortbow)
            .WithOnSheet(sheet => {
                sheet.Proficiencies.AddProficiencyAdjustment((item) => item.Contains(Trait.MonkWeapon) && item.Contains(Trait.Martial) && item.Contains(Trait.Bow) || item.Contains(Trait.Longbow) || item.Contains(Trait.Shortbow), Trait.Simple);
            })
            .WithOnCreature(you => {
                if (!you.HasFeat(MonasticWeaponry))
                    ReplaceFlurryOfBlows(you);
            })
            .WithPermanentQEffect(null, (qfMAS) => {
                qfMAS.ProvideMainAction = self => {
                    return new ActionPossibility(new CombatAction(self.Owner, IllustrationName.Shortbow, "Monastic Archer Stance", [Trait.Monk, Trait.Stance],
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
                        var stance = KineticistCommonEffects.EnterStance(user, IllustrationName.Shortbow,
                            "Monastic Archer Stance", "While in this stance, you can use your monk feats or monk abilities " +
                            "that normally require unarmed attacks with longbows, shortsbows and monk bows instead.", QEffectIds.MonasticArcherStance);
                        stance.PreventTakingAction = action => action.HasTrait(Trait.Strike)
                            && !((action.Item != null
                            && action.Item.HasTrait(Trait.MonkWeapon)
                            && action.Item.HasTrait(Trait.Bow)
                            && !action.Item.HasTrait(Trait.Advanced)) || new Trait?[] { Trait.Longbow, Trait.Shortbow }.Contains(action?.Item?.MainTrait))
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

            yield return new TrueFeat(ModManager.RegisterFeatName("RL_AdvancedMonasticWeaponry", "Advanced Monastic Weaponry"), 6, "Your rigorous training regimen allows you to wield complex weaponry with ease.",
                "For the purposes of proficiency, you treat advanced monk weapons as if they were martial monk weapons.",
                [Trait.Monk, ModTraits.Roguelike], null)
            .WithOnSheet(sheet => {
                sheet.Proficiencies.AddProficiencyAdjustment((item) => item.Contains(Trait.MonkWeapon) && item.Contains(Trait.Advanced), Trait.Simple);
            })
            .WithPrerequisite(sheet => sheet.HasFeat(MonasticWeaponry), "You must have the Monastic Weaponry feat.");

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

        internal static void SpawnRatFamiliar(Creature master) {
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
            master.Battle.SpawnCreature(rat, master.Battle.GaiaFriends, master.Occupies);
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
                            Item? bow = user.HeldItems.FirstOrDefault(wpn => (wpn.HasTrait(Trait.MonkWeapon) && wpn.HasTrait(Trait.Bow) && !wpn.HasTrait(Trait.Advanced)) || new Trait[] { Trait.Longbow, Trait.Shortbow }.Contains(wpn.MainTrait));
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
                                        (strike.Target as CreatureTarget).WithAdditionalConditionOnTargetCreature((a, d) => a.HasFreeHand ? Usability.Usable : Usability.NotUsable("no-free-hand"));
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
                                        (strike.Target as CreatureTarget).WithAdditionalConditionOnTargetCreature((a, d) => a.HasFreeHand ? Usability.Usable : Usability.NotUsable("no-free-hand"));
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
                                Item? bow = self.HeldItems.FirstOrDefault(wpn => (wpn.HasTrait(Trait.MonkWeapon) && wpn.HasTrait(Trait.Bow) && !wpn.HasTrait(Trait.Advanced)) || new Trait[] { Trait.Longbow, Trait.Shortbow }.Contains(wpn.MainTrait));
                                if (bow != null) {
                                    var combatAction = self.CreateStrike(bow);
                                    (combatAction.Target as CreatureTarget).CreatureTargetingRequirements.Add(new MaximumRangeCreatureTargetingRequirement(bow.WeaponProperties.RangeIncrement / 2));
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
                                var stunningFistAction = CombatAction.CreateSimpleIncapacitation(self, "Stunning Fist", self.MaximumSpellRank);
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