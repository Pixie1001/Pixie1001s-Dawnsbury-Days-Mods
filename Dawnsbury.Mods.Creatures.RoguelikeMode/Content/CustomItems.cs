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
using System.Buffers.Text;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using FMOD;
using System.Xml.Linq;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class CustomItems {

        //public static List<ItemName> items = new List<ItemName>();

        public static ItemName AlicornPike { get; } = ModManager.RegisterNewItemIntoTheShop("AlicornPike", itemName => {
            var item = new Item(itemName, Illustrations.AlicornPike, "alicorn pike", 35, 3,
                Trait.Magical, Trait.GhostTouch, Trait.Reach, Trait.TwoHanded, Trait.Polearm, Trait.Martial, ModTraits.Roguelike)
            .WithDescription("{i}An illustrious pike, forged from the horn of a unicorn and infused with their goodly healing powers.{/i}\n\nWhilst wielding this pike, you gain Regeneration 4.")
            .WithWeaponProperties(new WeaponProperties("1d10", DamageKind.Piercing)
                .WithAdditionalDamage("1d4", DamageKind.Good)
            );

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect("Regeneration", "You heal for 4 at the beginning of each turn, while holding the Alicorn Pike", ExpirationCondition.Ephemeral, wielder, IllustrationName.PositiveAttunement) {
                    Value = 4,
                    StartOfYourPrimaryTurn = async (self, owner) => {
                        await owner.HealAsync("4", CombatAction.CreateSimple(owner, "Regeneration"));
                    }
                });
            };

            return item;
        });

        public static ItemName AlicornDagger { get; } = ModManager.RegisterNewItemIntoTheShop("AlicornDagger", itemName => {
            var item = new Item(itemName, Illustrations.AlicornDagger, "alicorn dagger", 35, 3,
                Trait.Magical, Trait.GhostTouch, Trait.Agile, Trait.Finesse, Trait.Thrown10Feet, Trait.VersatileS, Trait.WizardWeapon, Trait.Knife, Trait.Simple, ModTraits.Roguelike)
            .WithMainTrait(Trait.Dagger)
            .WithDescription("{i}An illustrious dagger, forged from the horn of a unicorn and infused with their goodly healing powers.{/i}\n\nWhilst wielding this dagger, you gain Regeneration 4.")
            .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Piercing)
                .WithAdditionalDamage("1d4", DamageKind.Good)
            );

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect("Regeneration", "You heal for 4 at the beginning of each turn, while holding the Alicorn Dagger", ExpirationCondition.Ephemeral, wielder, IllustrationName.PositiveAttunement) {
                    Value = 4,
                    StartOfYourPrimaryTurn = async (self, owner) => {
                        await owner.HealAsync("4", CombatAction.CreateSimple(owner, "Regeneration"));
                    }
                });
            };

            return item;
        });

        public static ItemName SpideryHalberd { get; } = ModManager.RegisterNewItemIntoTheShop("SpideryHalberd", itemName => {
            var item = new Item(itemName, Illustrations.SpideryHalberd, "Spidery Halberd", 35, 3,
                Trait.Magical, Trait.Reach, Trait.VersatileS, Trait.Martial, Trait.Polearm, Trait.TwoHanded, ModTraits.Roguelike)
            .WithMainTrait(Trait.Halberd)
            .WithDescription("{i}This jagged halberd's haft is adorned with spidery webs for added grip.{/i}\n\nThe spiderdy halberd deals +1d4 poison damage, and can be used to fire pinning webs at your enemies, with an escape DC equal to the higher of your class or spell DC.")
            .WithWeaponProperties(new WeaponProperties("1d10", DamageKind.Piercing)
                .WithAdditionalDamage("1d4", DamageKind.Poison)
            );

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect() {
                    ExpiresAt = ExpirationCondition.Ephemeral,
                    ProvideStrikeModifier = (item) => {
                        if (item != weapon || weapon.ItemModifications.Any(mod => mod.Kind == ItemModificationKind.UsedThisDay)) {
                            return null;
                        }

                        return new CombatAction(wielder, IllustrationName.Web, "Shoot Web", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Attack },
                        "{b}Range{/b} 30 feet\n\n{b}Target{/b} 1 enemy\n\nMake a ranged attack roll against the target. On a hit, the target is immobilized by a web trap, sticking them to the nearest surface. They must use the Escape (DC " + (int)(wielder.ClassOrSpellDC()) + ") action to free themselves.",
                        Target.Ranged(6)) {
                            ShortDescription = "On a hit, the target is immobilized by a web trap, until they use the Escape (DC " + (int)(wielder.ClassOrSpellDC()) + ") action to free themselves."
                        }
                        .WithProjectileCone(IllustrationName.Web, 5, ProjectileKind.Cone)
                        .WithSoundEffect(SfxName.AeroBlade)
                        .WithActionCost(1)
                        .WithActiveRollSpecification(new ActiveRollSpecification(Checks.Attack(new Item(IllustrationName.Web, "Web", new Trait[] { Trait.Attack, Trait.Unarmed, Trait.Finesse, Trait.Ranged })), Checks.DefenseDC(Defense.AC)))
                        .WithEffectOnEachTarget(async (spell, caster, target, checkResult) => {
                            if (checkResult >= CheckResult.Success) {
                                QEffect webbed = new QEffect($"Webbed (DC {caster.ClassOrSpellDC()})", "You cannot use any action with the move trait, until you break free of the webs.") {
                                    Id = QEffectId.Immobilized,
                                    Source = caster,
                                    PreventTakingAction = (ca) => !ca.HasTrait(Trait.Move) ? null : "You're immobilized.",
                                    Illustration = IllustrationName.Web,
                                    ProvideContextualAction = self => {
                                        CombatAction combatAction = new CombatAction(self.Owner, (Illustration)IllustrationName.Escape, "Escape from " + caster?.ToString() + "'s webs.", new Trait[] {
                                            Trait.Attack, Trait.AttackDoesNotTargetAC }, $"Make an unarmed attack, Acrobatics check or Athletics check against the escape DC ({(caster != null ? caster.ClassOrSpellDC() : 0)}) of the webs.",
                                            Target.Self((_, ai) => ai.EscapeFrom(caster))) {
                                            ActionId = ActionId.Escape
                                        };

                                        ActiveRollSpecification activeRollSpecification = (new ActiveRollSpecification[] {
                                            new ActiveRollSpecification(Checks.Attack(Item.Fist()), Checks.FlatDC(caster.ClassOrSpellDC())),
                                            new ActiveRollSpecification(Checks.SkillCheck(Skill.Athletics), Checks.FlatDC(caster.ClassOrSpellDC())),
                                            new ActiveRollSpecification(Checks.SkillCheck(Skill.Acrobatics), Checks.FlatDC(caster.ClassOrSpellDC()))
                                        }).MaxBy(roll => roll.DetermineBonus(combatAction, self.Owner, null).TotalNumber);

                                        return (ActionPossibility)combatAction
                                        .WithActiveRollSpecification(activeRollSpecification)
                                        .WithSoundEffect(combatAction.Owner.HasTrait(Trait.Female) ? SfxName.TripFemale : combatAction.Owner.HasTrait(Trait.Male) ? SfxName.TripMale : SfxName.BeastRoar)
                                        .WithEffectOnEachTarget(async (spell, a, d, cr) => {
                                            switch (cr) {
                                                case CheckResult.CriticalFailure:
                                                    a.AddQEffect(new QEffect("Cannot escape", "You can't Escape until your next turn.", ExpirationCondition.ExpiresAtStartOfYourTurn, a) {
                                                        PreventTakingAction = ca => !ca.Name.StartsWith("Escape") ? null : "You already tried to escape and rolled a critical failure."
                                                    });
                                                    break;
                                                case CheckResult.Success:
                                                    self.ExpiresAt = ExpirationCondition.Immediately;
                                                    break;
                                                case CheckResult.CriticalSuccess:
                                                    self.ExpiresAt = ExpirationCondition.Immediately;
                                                    int num = await self.Owner.StrideAsync("You escape and you may Stride 5 feet.", maximumFiveFeet: true, allowPass: true) ? 1 : 0;
                                                    break;
                                            }
                                        });
                                    }
                                };
                                target.AddQEffect(webbed);
                            }
                        });
                    }
                });
            };

            return item;
        });

        public static ItemName DuelingSpear { get; } = ModManager.RegisterNewItemIntoTheShop("DuelingSpear", itemName => new Item(itemName, Illustrations.DuelingSpear, "dueling spear", 0, 2,
            Trait.Disarm, Trait.Finesse, Trait.Uncommon, Trait.VersatileS, Trait.TwoHanded, Trait.Spear, Trait.Martial, ModTraits.Roguelike)
        .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Piercing)));

        public static ItemName Hatchet { get; } = ModManager.RegisterNewItemIntoTheShop("RL_Hatchet", itemName => new Item(itemName, Illustrations.Hatchet, "hatchet", 0, 0,
            Trait.Agile, Trait.Sweep, Trait.Thrown20Feet, Trait.Martial, ModTraits.Roguelike)
        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing)));

        public static ItemName LightHammer { get; } = ModManager.RegisterNewItemIntoTheShop("RL_Light hammer", itemName => new Item(itemName, Illustrations.LightHammer, "light hammer", 0, 0,
            Trait.Agile, Trait.Thrown20Feet, Trait.Martial, ModTraits.Roguelike)
        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Bludgeoning)));

        public static ItemName ScourgeOfFangs { get; } = ModManager.RegisterNewItemIntoTheShop("ScourgeOfFangs", itemName => {
            Item item = new Item(itemName, IllustrationName.Whip, "scourge of fangs", 3, 100,
                new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.Finesse, Trait.Reach, Trait.Flail, Trait.Trip, Trait.Simple, Trait.Disarm, Trait.VersatileP, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithMainTrait(Trait.Whip)
            .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Slashing) {
                ItemBonus = 1,
            }
            .WithAdditionalDamage("1d6", DamageKind.Mental))
            .WithDescription("{i}A coiled three pronged whip favoured by drow priestesses. The weapin is constructed from interlocked copper segments that end in the mechanical heads of vicious clacking serpents, that appear possessed of a have a cruel and malevolent intelligence.{/i}\n\n" +
            "Those that feel their bite are wrackled by incredible pain, suffering an additional 1d6 mental damage.\n\nIn addition, the serpents are eager to assist their wielder, allowing them to attack by willing the snakes to strike using their wisdom modifier.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect() {
                    ExpiresAt = ExpirationCondition.Ephemeral,
                    BonusToAttackRolls = (self, action, d) => {
                        int bonus = self.Owner.Abilities.Wisdom - int.Max(self.Owner.Abilities.Strength, self.Owner.Abilities.Dexterity);
                        if (action != null && action.Item != null && action.Item == weapon && bonus > 0) {
                            return new Bonus(bonus, BonusType.Untyped, "Wisdom");
                        }
                        return null;
                    }
                });
            };
            return item;
        });

        public static ItemName SceptreOfTheSpider { get; } = ModManager.RegisterNewItemIntoTheShop("SceptreOfTheSpider", itemName => {
            Item item = new Item(itemName, Illustrations.SceptreOfTheSpider, "sceptre of the spider", 2, 35,
                new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.Agile, Trait.Club, Trait.Simple, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning))
            .WithDescription("{i}This esoteric sceptre appears to have been forged from within the demonic lair of the spider queen herself.{/i}\n\n" +
            "While wielding the sceptre, you can use the 'Shoot Web' action once per encounter.\n\n" +
            "{b}Range{/b} 30 feet\n\n{b}Target{/b} 1 enemy\n\nMake a ranged attack roll against the target. On a hit, the target is immobilized by a web trap, sticking them to the nearest surface. They must use the Escape action vs. DC 15 + your level to free themselves.");

            int baseDC = 15;

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect() {
                    ExpiresAt = ExpirationCondition.Ephemeral,
                    ProvideStrikeModifier = (item) => {
                        if (item != weapon || weapon.ItemModifications.Any(mod => mod.Kind == ItemModificationKind.UsedThisDay)) {
                            return null;
                        }

                        return new CombatAction(wielder, IllustrationName.Web, "Shoot Web", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Attack },
                        "{b}Frequency{/b} once per encounter\n{b}Range{/b} 30 feet\n{b}Target{/b} 1 enemy\n\nMake a ranged attack roll against the target. On a hit, the target is immobilized by a web trap, sticking them to the nearest surface. They must use the Escape (DC " + (int)(wielder.Level + baseDC) + ") action to free themselves.",
                        Target.Ranged(6)) {
                            ShortDescription = "On a hit, the target is immobilized by a web trap, until they use the Escape (DC " + (int)(wielder.Level + baseDC) + ") action to free themselves."
                        }
                        .WithProjectileCone(IllustrationName.Web, 5, ProjectileKind.Cone)
                        .WithSoundEffect(SfxName.AeroBlade)
                        .WithActionCost(1)
                        .WithActiveRollSpecification(new ActiveRollSpecification(Checks.Attack(new Item(IllustrationName.Web, "Web", new Trait[] { Trait.Attack, Trait.Unarmed, Trait.Finesse, Trait.Ranged })), Checks.DefenseDC(Defense.AC)))
                        .WithEffectOnSelf(user => {
                            weapon.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));
                        })
                        .WithEffectOnEachTarget(async (spell, caster, target, checkResult) => {
                            if (checkResult >= CheckResult.Success) {
                                QEffect webbed = new QEffect($"Webbed (DC {baseDC + caster.Level})", "You cannot use any action with the move trait, until you break free of the webs.") {
                                    Id = QEffectId.Immobilized,
                                    Source = caster,
                                    PreventTakingAction = (ca) => !ca.HasTrait(Trait.Move) ? null : "You're immobilized.",
                                    Illustration = IllustrationName.Web,
                                    ProvideContextualAction = self => {
                                        CombatAction combatAction = new CombatAction(self.Owner, (Illustration)IllustrationName.Escape, "Escape from " + caster?.ToString() + "'s webs.", new Trait[] {
                                            Trait.Attack, Trait.AttackDoesNotTargetAC }, $"Make an unarmed attack, Acrobatics check or Athletics check against the escape DC ({baseDC + (caster != null ? caster.Level : 0)}) of the webs.",
                                            Target.Self((_, ai) => ai.EscapeFrom(caster))) {
                                            ActionId = ActionId.Escape
                                        };

                                        ActiveRollSpecification activeRollSpecification = (new ActiveRollSpecification[] {
                                            new ActiveRollSpecification(Checks.Attack(Item.Fist()), Checks.FlatDC(baseDC + caster.Level)),
                                            new ActiveRollSpecification(Checks.SkillCheck(Skill.Athletics), Checks.FlatDC(baseDC + caster.Level)),
                                            new ActiveRollSpecification(Checks.SkillCheck(Skill.Acrobatics), Checks.FlatDC(baseDC + caster.Level))
                                        }).MaxBy(roll => roll.DetermineBonus(combatAction, self.Owner, null).TotalNumber);

                                        return (ActionPossibility)combatAction
                                        .WithActiveRollSpecification(activeRollSpecification)
                                        .WithSoundEffect(combatAction.Owner.HasTrait(Trait.Female) ? SfxName.TripFemale : combatAction.Owner.HasTrait(Trait.Male) ? SfxName.TripMale : SfxName.BeastRoar)
                                        .WithEffectOnEachTarget(async (spell, a, d, cr) => {
                                            switch (cr) {
                                                case CheckResult.CriticalFailure:
                                                    a.AddQEffect(new QEffect("Cannot escape", "You can't Escape until your next turn.", ExpirationCondition.ExpiresAtStartOfYourTurn, a) {
                                                        PreventTakingAction = ca => !ca.Name.StartsWith("Escape") ? null : "You already tried to escape and rolled a critical failure."
                                                    });
                                                    break;
                                                case CheckResult.Success:
                                                    self.ExpiresAt = ExpirationCondition.Immediately;
                                                    break;
                                                case CheckResult.CriticalSuccess:
                                                    self.ExpiresAt = ExpirationCondition.Immediately;
                                                    int num = await self.Owner.StrideAsync("You escape and you may Stride 5 feet.", maximumFiveFeet: true, allowPass: true) ? 1 : 0;
                                                    break;
                                            }
                                        });
                                    }
                                };
                                target.AddQEffect(webbed);
                            }
                        });
                    },
                    EndOfCombat = async (self, won) => {
                        item.ItemModifications.RemoveAll(mod => mod.Kind == ItemModificationKind.UsedThisDay);
                    }
                });
            };
            return item;
        });


        //new Item(itemName, illustration, name, (int) level * 2 - 1, GetWandPrice((int) level * 2 - 1), traits.ToArray())
        //   .WithDescription(desc)
        //   .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning))

        public static ItemName ProtectiveAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("ProtectiveAmulet", itemName => {
            Item item = new Item(itemName, Illustrations.ProtectiveAmulet, "protective amulet", 3, 60, new Trait[] { Trait.Magical, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithDescription("{i}An eerie fetish, thrumming with protective magic bestowed by foul and unknowable beings. Though it's intended user has perished, some small measure of the amulet's origional power can still be invoked by holding the amulet aloft.{/i}\n\n" +
            "{b}Protective Amulet {icon:Reaction}{/b}.\n\n{b}Trigger{/b} While holding the amulet, you or an ally within 15-feet would be damaged by an attack.\n{b}Effect{/b} Reduce the damage by an amount equal to 1 + your level.\n\n" +
            "After using the amulet in this way, it cannot be used again until you recharge its magic as an {icon:Action} action.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                if (wielder.HasTrait(ModTraits.Witch)) {
                    QEffect effect = new QEffect("Protective Amulet {icon:Reaction}", "{b}Trigger{/b} You or a member of your coven within 15-feet would be damaged by an attack. {b}Effect{/b} Reduce the damage by an amount equal to 3 + your level.");
                    effect.ExpiresAt = ExpirationCondition.Ephemeral;
                    effect.AddGrantingOfTechnical(cr => cr.OwningFaction.IsEnemy && !cr.HasTrait(Trait.Animal), qf => {
                        qf.YouAreDealtDamage = async (self, a, damage, d) => {
                            if (effect.Owner.DistanceTo(d) > 3) {
                                return null;
                            }

                            if (await effect.Owner.AskToUseReaction((damage.Power != null ? "{b}" + a.Name + "{/b} uses {b}" + damage.Power.Name + "{/b} on " + "{b}" + qf.Owner.Name + "{/b}" :
                                "{b}" + qf.Owner.Name + "{/b} has been hit") + " for " + damage.Amount + $" damage, which provokes the protective powers of your Protective Amulet.\nUse your reaction to reduce the damage by {effect.Owner.Level + 3}?")) {
                                effect.Owner.Occupies.Overhead("*uses protective amulet*", Color.Black, $"{effect.Owner.Name} holds up their protective amulet to shield {qf.Owner.Name} from harm.");
                                qf.Owner.Occupies.Overhead($"*{3 + effect.Owner.Level} damage negated*", Color.Black);
                                Sfxs.Play(SfxName.Abjuration, 1f);
                                return new ReduceDamageModification(3 + effect.Owner.Level, "Protective Amulet");
                            }
                            return null;
                        };
                    });
                    wielder.AddQEffect(effect);
                } else {
                    QEffect effect = new QEffect("Protective Amulet {icon:Reaction}", "{b}Trigger{/b} You or an ally within 15-feet would be damaged by an attack. {b}Effect{/b} Reduce the damage by an amount equal to 1 + your level.");
                    effect.ExpiresAt = ExpirationCondition.Ephemeral;
                    effect.Tag = weapon;
                    effect.EndOfCombat = async (self, won) => {
                        item.ItemModifications.RemoveAll(mod => mod.Kind == ItemModificationKind.UsedThisDay);
                    };
                    effect.ProvideContextualAction = self => {
                        if (item.ItemName == ProtectiveAmulet && item.ItemModifications.Any(mod => mod.Kind == ItemModificationKind.UsedThisDay)) {
                            return (ActionPossibility)new CombatAction(effect.Owner, Illustrations.ProtectiveAmulet, "Recharge Amulet", new Trait[] { Trait.Magical, Trait.Abjuration, Trait.Concentrate },
                                "Concentrate on the amulet, recharging it with power so that its protective properties can be used once again.", Target.Self())
                            .WithActionCost(1)
                            .WithSoundEffect(SfxName.MinorAbjuration)
                            .WithEffectOnSelf(self => {
                                item.ItemModifications.RemoveAll(mod => mod.Kind == ItemModificationKind.UsedThisDay);
                            })
                            ;
                        }
                        return null;
                    };
                    effect.AddGrantingOfTechnical(cr => cr.OwningFaction.AlliedFactionOf(effect.Owner.OwningFaction), qf => {
                        qf.YouAreDealtDamage = async (self, a, damage, d) => {
                            if (weapon.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay) != null || effect.Owner.DistanceTo(d) > 3 || a == d.Battle.Pseudocreature) {
                                return null;
                            }

                            if (await effect.Owner.AskToUseReaction((damage.Power != null ? "{b}" + a.Name + "{/b} uses {b}" + damage.Power.Name + "{/b} on " + "{b}" + qf.Owner.Name + "{/b}" :
                                "{b}" + qf.Owner.Name + "{/b} has been hit") + " for " + damage.Amount + $" damage, which provokes the protective powers of your Protective Amulet.\nUse your reaction to reduce the damage by {effect.Owner.Level + 1}?")) {
                                item.WithModification(new ItemModification(ItemModificationKind.UsedThisDay));
                                effect.Owner.Occupies.Overhead("*uses protective amulet*", Color.Black, $"{effect.Owner.Name} holds up their protective amulet to shield {qf.Owner.Name} from harm.");
                                qf.Owner.Occupies.Overhead($"*{effect.Owner.Level + 1} damage negated*", Color.Black);
                                Sfxs.Play(SfxName.Abjuration, 1f);
                                return new ReduceDamageModification(effect.Owner.Level + 1, "Protective Amulet");
                            }
                            return null;
                        };
                    });
                    wielder.AddQEffect(effect);
                }
            };
            return item;
        });

        public static ItemName Hexshot { get; } = ModManager.RegisterNewItemIntoTheShop("Hexshot", itemName => {
            Item item = new Item(itemName, Illustrations.Hexshot, "hexshot", 3, 40,
                new Trait[] { Trait.Magical, Trait.VersatileB, Trait.FatalD8, Trait.Reload1, Trait.Crossbow, Trait.Simple, Trait.DoNotAddToCampaignShop, ModTraits.CasterWeapon, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike })
            .WithMainTrait(ModTraits.Hexshot)
            .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Piercing).WithRangeIncrement(8))
            .WithDescription("{i}This worn pistol is etched with malevolent purple runes that seem to glow brightly in response to spellcraft, loading the weapon's strange inscribed ammunition with power.{/i}\n\n" +
            "After the wielder expends a spell slot, the Hexshot becomes charged, gaining a +2 status bonus to hit, and dealing additional force damage equal to twice the level of the slot used.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect("Hexshot", "After expending a spell slot or using a scroll, your Hexshot becomes charged, gaining a +2 status bonus to hit, and dealing additional force damage equal to twice the level of the slot used.") {
                    ExpiresAt = ExpirationCondition.Ephemeral,
                    AfterYouTakeAction = async (self, spell) => {
                        if (spell != null && ((spell.HasTrait(Trait.Spell) || spell.HasTrait(Trait.Spellstrike)) && !spell.HasTrait(Trait.SustainASpell) && !spell.HasTrait(Trait.Cantrip) && !spell.HasTrait(Trait.Focus) || spell.Name.StartsWith("Channel Smite"))) {
                            string damage = spell.SpellLevel != 0 ? $"{spell.SpellLevel * 2}" : $"{(self.Owner.Level + 2) / 2 * 2}";
                            self.Owner.AddQEffect(new QEffect("Hexshot Charged",
                                $"Your Hexshot pistol is charged by the casting of a spell. The next shot you take with it gains a +2 status bonus, and deals an additional {damage} force damage.",
                                ExpirationCondition.Never, null, new SameSizeDualIllustration(Illustrations.StatusBackdrop, weapon.Illustration)) {
                                Value = spell.SpellLevel,
                                Key = "Hexshot Charged",
                                BonusToAttackRolls = (self, action, target) => {
                                    if (action != null && action.Item != null && action.Item.ItemName == Hexshot) {
                                        return new Bonus(2, BonusType.Status, "Hexshot Charged");
                                    }
                                    return null;
                                },
                                AddExtraKindedDamageOnStrike = (action, target) => {
                                    if (action != null && action.Item != null && action.Item.ItemName == Hexshot) {
                                        return new KindedDamage(DiceFormula.FromText($"{damage}", "Hexshot"), DamageKind.Force);
                                    }
                                    return null;
                                },
                                AdditionalGoodness = (self, action, target) => {
                                    if (action != null && action.Item != null && action.Item.ItemName == Hexshot) {
                                        return self.Value * 2 + 2;
                                    }
                                    return 0;
                                },
                                AfterYouTakeAction = async (self, action) => {
                                    if (action != null && action.Item != null && action.Item.ItemName == Hexshot && action.HasTrait(Trait.Strike)) {
                                        self.ExpiresAt = ExpirationCondition.Immediately;
                                    }
                                }
                            });
                        }
                    }
                });
            };

            return item;
        });

        public static ItemName SmokingSword { get; } = ModManager.RegisterNewItemIntoTheShop("Smoking Sword", itemName => {
            Item item = new Item(itemName, new WandIllustration(IllustrationName.ElementFire, IllustrationName.Longsword), "smoking sword", 3, 25,
                new Trait[] { Trait.Magical, Trait.Martial, Trait.Sword, Trait.Fire, Trait.VersatileP, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.BoostedWeapon, ModTraits.Roguelike
            })
            .WithMainTrait(Trait.Longsword)
            .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing))
            .WithDescription("{i}Smoke constantly belches from this longsword.{/i}\n\nAny hit with this sword deals 1 extra fire damage.\n\n" +
            "You can use a special action while holding the sword to command the blade's edges to light on fire.\n\n{b}Activate {icon:Action}.{/b} concentrate; {b}Effect.{/b} Until the end" +
            " of your turn, the sword deals 1d6 extra fire damage instead of just 1. After you use this action, you can't use it again until the end of the encounter.");
            item.WeaponProperties.AdditionalDamage.Add(("1", DamageKind.Fire));
            return item;
        });

        public static ItemName StormHammer { get; } = ModManager.RegisterNewItemIntoTheShop("Storm Hammer", itemName => {
            Item item = new Item(itemName, new DualIllustration(IllustrationName.ElementAir, IllustrationName.Warhammer), "storm hammer", 3, 25,
                new Trait[] { Trait.Magical, Trait.Shove, Trait.Martial, Trait.Hammer, Trait.Electricity, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.BoostedWeapon, ModTraits.Roguelike })
            .WithMainTrait(Trait.Warhammer)
            .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Bludgeoning))
            .WithDescription("{i}Sparks of crackling electricity arc from this warhammer, and the head thrums with distant thunder.{/i}\n\nAny hit with this hammer deals 1 extra electricity damage.\n\n" +
            "You can use a special action while holding the hammer to transform the sparks into lightning bolts.\n\n{b}Activate {icon:Action}.{/b} concentrate; {b}Effect.{/b} Until the end" +
            " of your turn, the hammer deals 1d6 extra electricity damage instead of just 1. After you use this action, you can't use it again until the end of the encounter.");
            item.WeaponProperties.AdditionalDamage.Add(("1", DamageKind.Electricity));
            return item;
        });

        public static ItemName ChillwindBow { get; } = ModManager.RegisterNewItemIntoTheShop("Chillwind Bow", itemName => {
            Item item = new Item(itemName, Illustrations.ChillwindBow, "chillwind bow", 3, 25,
                new Trait[] { Trait.Magical, Trait.OneHandPlus, Trait.DeadlyD10, Trait.Bow, Trait.Martial, Trait.RogueWeapon, Trait.ElvenWeapon, Trait.Cold, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.BoostedWeapon, ModTraits.Roguelike })
            .WithMainTrait(Trait.Shortbow)
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)
            .WithRangeIncrement(12))
            .WithDescription("{i}The yew of this bow is cold to the touch, and its arrows pool with fog as they're nocked.{/i}\n\nAny hit with this bow deals 1 extra cold damage.\n\n" +
            "You can use a special action while holding the bow to coat the bow in fridgid icy scales.\n\n{b}Activate {icon:Action}.{/b} concentrate; {b}Effect.{/b} Until the end" +
            " of your turn, the bow deals 1d6 extra cold damage instead of just 1. After you use this action, you can't use it again until the end of the encounter.");
            item.WeaponProperties.AdditionalDamage.Add(("1", DamageKind.Cold));
            return item;
        });

        public static ItemName Sparkcaster { get; } = ModManager.RegisterNewItemIntoTheShop("Sparkcaster", itemName => {
            Item item = new Item(itemName, new DualIllustration(IllustrationName.ElementAir, IllustrationName.HeavyCrossbow), "sparkcaster", 3, 25,
                new Trait[] { Trait.Magical, Trait.Reload2, Trait.Simple, Trait.Bow, Trait.TwoHanded, Trait.Crossbow, Trait.WizardWeapon, Trait.Electricity, Trait.DoNotAddToCampaignShop, ModTraits.CasterWeapon, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike })
            .WithMainTrait(Trait.HeavyCrossbow)
            .WithWeaponProperties(new WeaponProperties("1d10", DamageKind.Piercing)
            .WithRangeIncrement(24))
            .WithDescription("{i}Sparks of crackling electricity arc from this crossbow.{/i}\n\nAny hit with this crossbow deals 1 extra electricity damage.\n\n" +
            "You can use a special action while holding the crossbow to fire a crackling bolt of lightning in a great arc.\n\n{b}Activate {icon:Action}.{/b} concentrate, manipulate; {b}Effect.{/b} Each creature in a 30-foot line " +
            "suffers 2d6 electricity damage, mitigated by a basic Reflex save. After you use this action, you can't use it again until the end of the encounter.");
            item.WeaponProperties.AdditionalDamage.Add(("1", DamageKind.Electricity));

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                    ProvideStrikeModifierAsPossibility = item => {
                        if (item != weapon || item.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay) != null || wielder.PersistentCharacterSheet == null || weapon.EphemeralItemProperties.NeedsReload) {
                            return null;
                        }

                        CombatAction action = new CombatAction(wielder, weapon.Illustration, $"Activate {weapon.Name.CapitalizeEachWord()}", new Trait[] { Trait.Concentrate, Trait.Manipulate, Trait.Magical, Trait.Electricity, Trait.Evocation },
                            "{b}Frequency{/b} once per encounter\n{b}Range{/b} 30-foot line\n{b}Saving Throw{/b} Basic Reflex save\n\nEach creature in the line suffers 2d6 electricity damage, mitigated by a basic Reflex saving throw against the wielder's class or spell save DC.", Target.ThirtyFootLine()) {
                            ShortDescription = $"[30-foot line] {wielder.ClassOrSpellDC()} vs. basic Reflex save; 2d6 electricity damage."
                        }
                        .WithSavingThrow(new SavingThrow(Defense.Reflex, caster => caster != null ? caster.ClassOrSpellDC() : 0))
                        .WithActionCost(1)
                        .WithProjectileCone(IllustrationName.LightningBolt, 15, ProjectileKind.Ray)
                        .WithSoundEffect(SfxName.ElectricArc)
                        .WithEffectOnEachTarget(async (action, a, d, checkResult) => {
                            await CommonSpellEffects.DealBasicDamage(action, a, d, checkResult, "2d6", DamageKind.Electricity);
                        })
                        .WithEffectOnSelf(user => {
                            // Trigger reload
                            weapon.EphemeralItemProperties.NeedsReload = true;

                            // Expend use for encounter
                            weapon.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));

                            // Run end of combat cleanup
                            user.AddQEffect(new QEffect() {
                                Tag = weapon,
                                EndOfCombat = async (self, won) => {
                                    ItemModification used = weapon.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay);
                                    if (used != null) {
                                        weapon.ItemModifications.Remove(used);
                                    }
                                }
                            });
                        });

                        action.Item = weapon;

                        return (ActionPossibility)action;
                    }
                });
            };

            return item;
        });

        public static ItemName VipersSpit { get; } = ModManager.RegisterNewItemIntoTheShop("VipersSpit", itemName => {
            Item item = new Item(itemName, new DualIllustration(IllustrationName.AcidArrow, IllustrationName.HandCrossbow), "viper's spit", 3, 25,
                new Trait[] { Trait.Magical, Trait.Reload1, Trait.Simple, Trait.Bow, Trait.Crossbow, Trait.RogueWeapon, Trait.Acid, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike })
            .WithMainTrait(Trait.HandCrossbow)
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)
            .WithRangeIncrement(24))
            .WithDescription("{i}This viper shaped handcrossbow hisses and sizzles against the air.{/i}\n\nAny hit with this crossbow deals 1 extra acid damage.\n\n" +
            "You can use a special action while holding the crossbow to cause it to launch great splattering globlets of acid.\n\n{b}Activate {icon:Action}.{/b} concentrate, manipulate; {b}Effect.{/b} The weapon gains 1d6 acid splash damage until the end of your turn.\n\n" +
            "After you use this action, you can't use it again until the end of the encounter.");
            item.WeaponProperties.AdditionalDamage.Add(("1", DamageKind.Acid));

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                    ProvideStrikeModifierAsPossibility = item => {
                        if (item != weapon || item.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay) != null || wielder.PersistentCharacterSheet == null || weapon.EphemeralItemProperties.NeedsReload) {
                            return null;
                        }

                        CombatAction action = new CombatAction(wielder, weapon.Illustration, $"Activate {weapon.Name.CapitalizeEachWord()}", new Trait[] { Trait.Concentrate, Trait.Manipulate, Trait.Acid, Trait.Evocation },
                            "{b}Frequency{/b} once per encounter\nUntil the end of your turn, attacks made with the Viper's Spit gain +1d6 splash damage.", Target.ThirtyFootLine()) {
                            ShortDescription = $"{weapon.Name.CapitalizeEachWord()} gains an 1d6 acid splash damage until the end of your turn."
                        }
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.AcidSplash)
                        .WithEffectOnSelf(user => {
                            // Effect
                            weapon.WeaponProperties.AdditionalSplashDamageFormula = "1d6";
                            weapon.WeaponProperties.AdditionalSplashDamageKind = DamageKind.Acid;
                            weapon.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));

                            // Show effect
                            user.AddQEffect(new QEffect($"{weapon.Name.CapitalizeEachWord()} Activated", $"{weapon.Name.CapitalizeEachWord()} deals an extra 1d6 acid splash damage.") {
                                ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
                                Tag = weapon,
                                WhenExpires = (self) => {
                                    Item weapon = self.Tag as Item;
                                    weapon.WeaponProperties.AdditionalSplashDamageFormula = null;
                                }
                            });
                            // Run end of combat cleanup
                            user.AddQEffect(new QEffect() {
                                Tag = weapon,
                                EndOfCombat = async (self, won) => {
                                    weapon.WeaponProperties.AdditionalSplashDamageFormula = null;
                                    ItemModification used = weapon.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay);
                                    if (used != null) {
                                        weapon.ItemModifications.Remove(used);
                                    }
                                }
                            });
                        });

                        action.Item = weapon;

                        return (ActionPossibility)action;
                    }
                });
            };

            return item;
        });

        public static ItemName SpiderChopper { get; } = ModManager.RegisterNewItemIntoTheShop("Spider Chopper", itemName => {
            Item item = new Item(itemName, Illustrations.SpiderChopper, "spider chopper", 3, 25,
                               new Trait[] { Trait.Magical, Trait.Sweep, Trait.Martial, Trait.Axe, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike })
            .WithMainTrait(Trait.BattleAxe)
            .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing))
            .WithDescription("{i}An obsidian cleaver, erodated down to a brutal jagged edge by acidic spittle. The weapon seems to shifting and throb when in the presence of spiders.{/i}\n\n" +
            "The Spider Chopper deals +1d6 damage slashing damage to spiders.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                    AddExtraKindedDamageOnStrike = (strike, d) => {
                        if (strike == null || strike.Item != weapon) {
                            return null;
                        }

                        if (d.HasTrait(ModTraits.Spider) || d.Name.ToLower().Contains("spider")) {
                            return new KindedDamage(DiceFormula.FromText("1d6"), DamageKind.Slashing);
                        }
                        return null;
                    }
                });
            };

            return item;
        });

        public static ItemName Widowmaker { get; } = ModManager.RegisterNewItemIntoTheShop("Widowmaker", itemName => {
            Item item = new Item(itemName, Illustrations.Widowmaker, "widowmaker", 3, 25,
                               new Trait[] { Trait.Magical, Trait.Agile, Trait.Finesse, Trait.Thrown10Feet, Trait.VersatileS, Trait.Simple, Trait.Knife, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike })
            .WithMainTrait(Trait.Dagger)
            .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Piercing))
            .WithDescription("{i}A wicked looking dagger, with a small hollow at the tip of the blade, from which a steady supply of deadly poison drips.{/i}\n\nAttacks made against flat footed creatures using this dagger expose them to spider venom.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                QEffect poison = CommonQEffects.SpiderVenomAttack(weapon.Level + 14, weapon.Name).WithExpirationEphemeral();
                poison.AfterYouDealDamage = async (attacker, action, target) => {
                    if (!target.IsFlatFootedTo(attacker, action)) {
                        return;
                    }

                    if (action.Item == weapon && action.HasTrait(Trait.Strike)) {
                        Affliction poison = Affliction.CreateGiantSpiderVenom(weapon.Level + 14 + attacker.Level);

                        await Affliction.ExposeToInjury(poison, attacker, target);
                    }
                };
                wielder.AddQEffect(poison);
            };

            return item;
        });

        public static ItemName FlashingRapier { get; } = ModManager.RegisterNewItemIntoTheShop("Flashing Rapier", itemName => {
            Item item = new Item(itemName, IllustrationName.Rapier, "flashing rapier", 3, 25,
                               new Trait[] { Trait.Magical, Trait.DeadlyD8, Trait.Disarm, Trait.Finesse, Trait.Martial, Trait.Sword, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike })
            .WithMainTrait(Trait.Rapier)
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing))
            .WithDescription("{/i}A brilliant sparkling rapier, that causes the light to bend around its blade in strange prismatic patterns.{/i}" +
            "\n\nThe flashing rapier dazzles enemies on a crit, and can be activated {icon:Action} once per day to flash forward in a swirl of light, appearing in a new location 40ft away.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                    YouHaveCriticalSpecialization = (self, item, action, target) => {
                        if (item.ItemName == FlashingRapier) {
                            target.AddQEffect(QEffect.Dazzled().WithExpirationAtStartOfSourcesTurn(self.Owner, 1));
                        }
                        return false;
                    },
                    ProvideStrikeModifierAsPossibility = item => {
                        if (item != weapon || item.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay) != null) {
                            return null;
                        }

                        CombatAction action = new CombatAction(wielder, new SideBySideIllustration(IllustrationName.DazzlingFlash, weapon.Illustration), $"Activate {weapon.Name.CapitalizeEachWord()}", new Trait[] { Trait.Magical, Trait.Teleportation, Trait.Light },
                            "{b}Range{/b} 40-foot\n\nLaunch yourself forward as a zipping ribbon of line, appearing in an unoccupied space within range.", Target.TileYouCanSeeAndTeleportTo(6)) {
                            ShortDescription = $"Teleport to a space within 40 feet."
                        }
                        .WithItem(item)
                        .WithActionCost(1)
                        .WithProjectileCone(IllustrationName.DazzlingFlash, 5, ProjectileKind.Ray)
                        .WithSoundEffect(SfxName.DazzlingFlash)
                        .WithEffectOnChosenTargets(async (spell, caster, targets) => {
                            Tile target = targets.ChosenTile;
                            if (!target.IsTrulyGenuinelyFreeTo(caster)) {
                                target = target.GetShuntoffTile(caster);
                            }
                            caster.TranslateTo(target);
                            caster.AnimationData.ColorBlink(Color.LightGoldenrodYellow);
                            caster.Battle.SmartCenterAlways(target);
                            spell.Item.WithModification(new ItemModification(ItemModificationKind.UsedThisDay));
                        });

                        return (ActionPossibility)action;
                    }
                });
            };

            return item;
        });

        public static ItemName HungeringBlade { get; } = ModManager.RegisterNewItemIntoTheShop("Hungering Blade", itemName => {
            Item item = new Item(itemName, Illustrations.HungeringBlade, "hungering blade", 3, 25,
                new Trait[] { Trait.Magical, Trait.VersatileP, Trait.TwoHanded, Trait.Martial, Trait.Sword, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike })
            .WithMainTrait(Trait.Greatsword)
            .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing))
            .WithDescription("{i}A sinister greatsword made from cruel black steel and an inhospitable grip dotted by jagged spines. No matter how many times the blade is cleaned, it continues to ooze forth a constant trickle of blood.{/i}\n\n" +
            "The hungering blade causes its wielder to suffer 1d8 persistent bleed damage at the start of each turn, but deals an additional 1d4 negative damage on a hit. In addition, after felling an enemy with the blade, the wielder gains 5 temporary hit points.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                    StartOfYourPrimaryTurn = async (self, owner) => {
                        owner.AddQEffect(QEffect.PersistentDamage("1d8", DamageKind.Bleed));
                    },
                    AddExtraKindedDamageOnStrike = (strike, attacker) => {
                        if (attacker.WeaknessAndResistance.Immunities.Contains(DamageKind.Bleed)) {
                            return null;
                        }
                        if (strike == null || strike.Item != weapon) {
                            return null;
                        }
                        return new KindedDamage(DiceFormula.FromText("1d4"), DamageKind.Negative);
                    },
                    AfterYouDealDamage = async (a, strike, d) => {
                        if (strike == null || strike.Item != weapon) {
                            return;
                        }

                        if (d.HP <= 0) {
                            a.GainTemporaryHP(5);
                        }
                    }
                });
            };

            return item;
        });

        public static ItemName WebwalkerArmour { get; } = ModManager.RegisterNewItemIntoTheShop("Webwalker Armour", itemName => {
            return new Item(itemName, Illustrations.WebWalkerArmour, "webwalker armour", 2, 35,
                new Trait[] { Trait.Magical, Trait.LightArmor, Trait.Leather, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithArmorProperties(new ArmorProperties(2, 3, -1, 0, 12))
            .WithDescription("{i}Flexible jetblack armour of dark elven make, sewn from some exotic underdark silk.{/i}\n\nThe wearer of this armour may pass through webs unhindered.");
        });

        public static ItemName KrakenMail { get; } = ModManager.RegisterNewItemIntoTheShop("Kraken Mail", itemName => {
            return new Item(itemName, Illustrations.KrakenMail, "kraken mail", 2, 35,
                new Trait[] { Trait.Magical, Trait.MediumArmor, Trait.Leather, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithArmorProperties(new ArmorProperties(4, 1, -2, 1, 16))
            .WithDescription("{i}The eyes on this briny chitin armour blink periodically.{/i}\n\nThe wearer of this armour gains a swim speed, and a +2 status bonus to grapple rolls while submerged in water.");
        });

        public static ItemName RobesOfTheWarWizard { get; } = ModManager.RegisterNewItemIntoTheShop("Robes of the War Wizard", itemName => {
            return new Item(itemName, Illustrations.RobesOfTheWarWizard, "robes of the war wizard", 2, 35,
                new Trait[] { Trait.Magical, Trait.UnarmoredDefense, Trait.Armor, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithArmorProperties(new ArmorProperties(0, 5, 0, 0, 0))
            .WithDescription("{i}This bold red robe is enchanted to collect neither sweat, dust nor grime.{/i}\n\nThe wearer of this armour gains a +2 damage bonus per spell level to non-cantrip cone and emanation spells used against creatures within 15-feet of them.\n\nIn addition, they gain resistance 1 to acid, cold, electricity and fire damage.");
        });

        public static ItemName GreaterRobesOfTheWarWizard { get; } = ModManager.RegisterNewItemIntoTheShop("Greater Robes of the War Wizard", itemName => {
            return new Item(itemName, Illustrations.RobesOfTheWarWizard, "robes of the war wizard, greater", 4, 90,
                new Trait[] { Trait.Magical, Trait.UnarmoredDefense, Trait.Armor, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithArmorProperties(new ArmorProperties(0, 5, 0, 0, 0))
            .WithDescription("{i}This bold red robe is enchanted to collect neither sweat, dust nor grime.{/i}\n\nThe wearer of this armour gains a +3 damage bonus per spell level to non-cantrip cone and emanation spells used against creatures within 15-feet of them.\n\nIn addition, they gain resistance 2 to acid, cold, electricity and fire damage.");
        });

        public static ItemName WhisperMail { get; } = ModManager.RegisterNewItemIntoTheShop("Whisper Mail", itemName => {
            return new Item(itemName, Illustrations.WhisperMail, "whisper mail", 2, 35,
                new Trait[] { Trait.Magical, Trait.MediumArmor, Trait.Chain, Trait.MetalArmor, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithArmorProperties(new ArmorProperties(4, 1, -2, 1, 16))
            .WithDescription("{i}This ominous armour set whispers incessantly into the wearer's ears... Revealing profound and disturbing secrets.{/i}\n\nThe wearer of this armour imposes weakness 1 to all physical damage against adjacent enemies, and a +1 item bonus to Seek checks.");
        });

        public static ItemName DreadPlate { get; } = ModManager.RegisterNewItemIntoTheShop("Dread Plate", itemName => {
            return new Item(itemName, Illustrations.DreadPlate, "dread plate", 3, 70,
                new Trait[] { Trait.Magical, Trait.HeavyArmor, Trait.Bulwark, Trait.Plate, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithArmorProperties(new ArmorProperties(6, 0, -3, -2, 18))
            .WithDescription("{i}This cold, black steel suit of plate armour radiates a spiteful presence, feeding off its wearer's own lifeforce to strike at any who would dare mar it.{/i}\n\nWhile wearing this cursed armour, you take an additional 1d4 negative damage when damaged by an adjacent creature, but deal 1d6 negative damage in return.");
        });

        public static ItemName SpellbanePlate { get; } = ModManager.RegisterNewItemIntoTheShop("Spellbane Plate", itemName => {
            return new Item(itemName, Illustrations.SpellbanePlate, "spellbane plate", 3, 70,
                new Trait[] { Trait.Magical, Trait.HeavyArmor, Trait.Bulwark, Trait.Plate, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithArmorProperties(new ArmorProperties(6, 0, -3, -2, 18))
            .WithDescription("{i}This suit of armour is forged from a strange, mercurial alloy - perhaps a lost relic forged by dwarven smithes during the height of the starborn invasion.{/i}"+
            "\n\nWhile wearing this suit of armour, you gain a +1 item bonus to all spell saving throws, but cannot cast spells of your own.");
        });

        public static ItemName DolmanOfVanishing { get; } = ModManager.RegisterNewItemIntoTheShop("Dolman of Vanishing", itemName => {
            return new Item(itemName, Illustrations.DolmanOfVanishing, "dolman of vanishing", 3, 60,
                new Trait[] { Trait.Magical, Trait.Armor, Trait.UnarmoredDefense, Trait.Cloth, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithArmorProperties(new ArmorProperties(0, 5, 0, 0, 0))
            .WithDescription("{i}A skyblue robe of gossmer, that seems to evade the beholder's full attention span, no matter how hard they try to focus on it.{/i}\n\nThe wearer of this cloak gains a +2 item bonus to stealth and can hide in plain sight.");
        });

        public static ItemName MaskOfConsumption { get; } = ModManager.RegisterNewItemIntoTheShop("Mask of Consumption", itemName => {
            return new Item(itemName, Illustrations.MaskOfConsumption, "mask of consumption", 2, 30,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Mask)
            .WithDescription("{i}This cursed mask fills the wearer with a ghoulish, ravenous hunger for living flesh, alongside a terrible wasting pallor.{/i}\n\n" +
            "The wearer of this mask has their MaxHP halved. However, in return they gain a 1d10 unarmed slashing attack with the agile and fineese properties. " +
            "Creatures hit by the attack suffer 1d6 persistent bleeding damage, and if they're blessed of living flesh, the wear may consume it to heal for an amount of hit points " +
            "equal to damage dealt.")
            .WithPermanentQEffectWhenWorn((qfMoC, item) => {
                qfMoC.Innate = true;
                qfMoC.Name = "Mask of Consumption";
                qfMoC.Description = "Your HP is halved. In return, your hungry claws attack deals 1d6 persistent bleed damage and deals you for an amount equal to damage dealt.";
                qfMoC.StateCheck = self => {
                    if (qfMoC.Owner.Traits.Any(trait => trait.HumanizeTitleCase2() == "Summoner")) {
                        QEffect? eidolon = self.Owner.QEffects.FirstOrDefault(qf => qf.Id.HumanizeTitleCase2() == "Summoner_Shared HP");
                        if (eidolon != null && eidolon.Source != null)
                            eidolon.Source.DrainedMaxHPDecrease = self.Owner.MaxHP / 2;
                    }
                    self.Owner.DrainedMaxHPDecrease = self.Owner.MaxHP / 2;
                    Item unarmed = qfMoC.Owner.UnarmedStrike;
                    qfMoC.Owner.ReplacementUnarmedStrike = self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.DragonClaws, "hungry claws", "1d10", DamageKind.Slashing, Trait.Agile, Trait.Finesse, Trait.Brawling, Trait.Unarmed)
                    .WithAdditionalWeaponProperties(properties => {
                        properties.AdditionalPersistentDamageFormula = "1d6";
                        properties.AdditionalPersistentDamageKind = DamageKind.Bleed;
                    });
                };
                //qfMoC.AfterYouDealDamage = async (a, strike, d) => {
                //    if (strike != null && strike.HasTrait(Trait.Strike) && strike.Item != null && strike.Item.Name == a.UnarmedStrike.Name && d.IsLivingCreature) {
                //        await a.HealAsync(DiceFormula.FromText($"{a.TrueMaximumHP * 0.2}", "Mask of Consumption"), strike);
                //    }
                //};
                qfMoC.AddGrantingOfTechnical(cr => cr.OwningFaction.IsEnemy && cr.IsLivingCreature, qfTechnical => {
                    qfTechnical.Key = "Mask of Consumption Technical";
                    qfTechnical.AfterYouTakeAmountOfDamageOfKind = async (self, strike, damage, kind) => {
                        if (strike != null && strike.Owner != null && strike.Owner == qfMoC.Owner && strike.HasTrait(Trait.Strike) && strike.Item != null && strike.Item.Name == strike.Owner.UnarmedStrike.Name && self.Owner.IsLivingCreature) {
                            await strike.Owner.HealAsync(DiceFormula.FromText($"{damage}", "Mask of Consumption"), strike);
                        }
                    };
                });
            });
        });

        public static ItemName SpiderHatchling { get; } = ModManager.RegisterNewItemIntoTheShop("Spider Hatchling", itemName => {
            return new Item(itemName, Illustrations.HuntingSpider, "spider hatchling", 2, 30,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.AnimalCompanion)
            .WithDescription("{i}A small baby hunting spider, in search of a new master to love and cherish it.{/i}\n\n" +
            "If you do not already have a battle ready animal companion, you gain a unique Spider Hatchling to fight at your side, that otherwise functional identically to the Animal Companion class feat.\n\nIf the spider hatchling dies in battle, it cannot aid the party until it has time to heal during their next long rest.")
            .WithPermanentQEffectWhenWorn((qfMoC, item) => {
                qfMoC.Tag = false;
                qfMoC.Innate = false;
                qfMoC.StartOfCombat = async self => {
                    Creature? companion = self.Owner.Battle.AllCreatures.FirstOrDefault(cr => cr.FindQEffect(QEffectId.RangersCompanion)?.Source == self.Owner);
                    if (companion != null) {
                        //companion.RemoveAllQEffects(qf => qf.Id == QEffectId.RangersCompanion);
                        //companion.Battle.RemoveCreatureFromGame(companion);
                        self.Tag = true;
                        return;
                    }
                    if (item.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay) != null) {
                        self.Owner.Occupies.Overhead("no companion", Color.Green, "The spider hatchling is injured, and won't be able to fight besides the party until after their next long rest or downtime.");
                    } else {
                        // TODO: Replace with proper animal companion stats
                        int lvl = self.Owner.Level;
                        int prof = self.Owner.Level + 2;
                        Creature animalCompanion = new Creature(Illustrations.SpiderHatchling, "Spider Hatchling", [Trait.Animal, Trait.AnimalCompanion, Trait.Minion], lvl, 1 + prof, 6, new Defenses(10 + 3 + prof, 1 + prof, 3 + prof, 1 + prof), 7 * lvl,
                            new Abilities(3, 3, 1, -4, 1, 0), new Skills(stealth: 3 + prof, acrobatics: 3 + prof, athletics: 3 + prof))
                        .WithProficiency(Trait.Unarmed, Proficiency.Trained)
                        .WithEntersInitiativeOrder(false)
                        .WithProficiency(Trait.UnarmoredDefense, Proficiency.Trained)
                        .WithUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d6", DamageKind.Piercing))
                        //.WithAdditionalUnarmedStrike(new Item(Illustrations.StabbingAppendage, "leg", Trait.Unarmed, Trait.Agile).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)))
                        .AddQEffect(CommonQEffects.WebAttack(prof))
                        .AddQEffect(new QEffect() {
                            ProvideMainAction = qfSupport => (ActionPossibility)new CombatAction(qfSupport.Owner, qfSupport.Owner.Illustration,
                            "Support", [], "{i}Your spider drips poison from its stinger when you create an opening.{/i}\n\nUntil the start of your next turn, if you hit and damage a creature in your spider's reach, you also deal 1d6 persistent poison damage.\n\n{b}Special{/b} If the animal uses the Support action, the only other actions it can use on this turn are basic move actions; if it has already used any other action this turn, it can't Support you.",
                            Target.Self()
                            .WithAdditionalRestriction(qfSupport => !qfSupport.Actions.ActionHistoryThisTurn.Any(act => !act.HasTrait(Trait.Move)) ? null : "You already took a non-move action this turn.")) {
                                ShortDescription = "Until the start of your next turn, if you hit and damage a creature in your spider's reach, you also deal 1d6 persistent poison damage."
                            }.WithEffectOnSelf(caster => {
                                QEffect qEffect = new QEffect("Support", "Until the start of your next turn, if you hit and damage a creature in your spider's reach, you also deal 1d6 persistent poison damage.", ExpirationCondition.ExpiresAtStartOfSourcesTurn, self.Owner, qfSupport.Owner.Illustration) {
                                    DoNotShowUpOverhead = true,
                                    PreventTakingAction = ca => !ca.HasTrait(Trait.Move) && ca.ActionId != ActionId.EndTurn ? "You used Support so you can't take non-move actions anymore this turn." : null
                                };
                                self.Owner.AddQEffect(new QEffect(ExpirationCondition.ExpiresAtEndOfYourTurn) {
                                    AfterYouDealDamage = async (creature, action, defender) => {
                                        if (action.CheckResult < CheckResult.Success || !defender.IsAdjacentTo(caster))
                                            return;
                                        await qfSupport.Owner.FictitiousSingleTileMove(defender.Occupies);
                                        defender.AddQEffect(QEffect.PersistentDamage("1d6", DamageKind.Poison));
                                        await qfSupport.Owner.FictitiousSingleTileMove(qfSupport.Owner.Occupies);
                                    }
                                });
                                caster.AddQEffect(qEffect);
                            })
                        })
                        .AddQEffect(new QEffect() {
                            StateCheck = sc => {
                                if (sc.Owner.HasEffect(QEffectId.Dying) || !sc.Owner.Battle.InitiativeOrder.Contains(sc.Owner))
                                    return;
                                Creature owner = sc.Owner;
                                int index = (owner.Battle.InitiativeOrder.IndexOf(owner) + 1) % owner.Battle.InitiativeOrder.Count;
                                Creature creature = owner.Battle.InitiativeOrder[index];
                                owner.Actions.HasDelayedYieldingTo = creature;
                                if (owner.Battle.CreatureControllingInitiative == owner)
                                    owner.Battle.CreatureControllingInitiative = creature;
                                owner.Battle.InitiativeOrder.Remove(sc.Owner);
                            }
                        })
                        
                        ;
                        animalCompanion.MainName = self.Owner.Name + "'s " + animalCompanion.MainName;
                        animalCompanion.InitiativeControlledBy = self.Owner;
                        animalCompanion.AddQEffect(new QEffect() {
                            Id = QEffectId.RangersCompanion,
                            Source = self.Owner,
                            WhenMonsterDies = qfCompanion => item.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay))
                        });
                        Action<Creature, Creature> benefitsToCompanion = self.Owner.PersistentCharacterSheet?.Calculated.RangerBenefitsToCompanion;
                        if (benefitsToCompanion != null)
                            benefitsToCompanion(animalCompanion, self.Owner);
                        self.Owner.Battle.SpawnCreature(animalCompanion, self.Owner.OwningFaction, self.Owner.Occupies);
                    }
                };

                qfMoC.StateCheck = self => {
                    if (self.Tag is true) {
                        return;
                    }

                    Creature owner = self.Owner;
                    Creature animalCompanion = Ranger.GetAnimalCompanion(owner);
                    bool flag = owner.HasEffect(QEffectId.MatureAnimalCompanion);
                    if (animalCompanion == null || !animalCompanion.Actions.CanTakeActions())
                        return;
                    ActionPossibility fullCommand = new ActionPossibility(new CombatAction(owner, flag ? (Illustration)new SideBySideIllustration(animalCompanion.Illustration, (Illustration)IllustrationName.Action) : animalCompanion.Illustration, "Command your Animal Companion",
                    [Trait.Auditory], "Take 2 actions as your animal companion.\n\nYou can only command your animal companion once per turn.", (Target)Target.Self().WithAdditionalRestriction((Func<Creature, string>)(cr => self.UsedThisTurn ? "You already commanded your animal companion this turn." : (string)null))) {
                        ShortDescription = "Take 2 actions as your animal companion."
                    }.WithEffectOnSelf(async action => {
                        self.UsedThisTurn = true;
                        await CommonSpellEffects.YourMinionActs(animalCompanion);
                    }), flag ? PossibilitySize.Half : PossibilitySize.Full);
                    owner.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                        ProvideMainAction = (Func<QEffect, Possibility>)(qff => (Possibility)fullCommand)
                    });
                    if (!flag)
                        return;
                    owner.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                        ProvideMainAction = (Func<QEffect, Possibility>)(qff => (Possibility)new ActionPossibility(new CombatAction(owner, (Illustration)new SideBySideIllustration(animalCompanion.Illustration, (Illustration)IllustrationName.FreeAction), "Move on your own",
                        [Trait.Basic], "{i}You leave your mature animal companion to its own devices. It will do what's right.{/i}\n\nTake 1 action as your animal companion. You can only spend this action to move or to make a Strike.\n\nYou can't command your animal companion and leave it to move in its own in the same turn.",
                        (Target)Target.Self()
                        .WithAdditionalRestriction((Func<Creature, string>)(cr => self.UsedThisTurn ? "You already commanded your animal companion this turn." : (string)null)))
                        .WithActionCost(0)
                        .WithEffectOnSelf((Func<Creature, Task>)(async caster => {
                            self.UsedThisTurn = true;
                            animalCompanion.AddQEffect(new QEffect(ExpirationCondition.ExpiresAtEndOfYourTurn) {
                                Id = QEffectId.MoveOnYourOwn,
                                PreventTakingAction = (Func<CombatAction, string>)(ca => !ca.HasTrait(Trait.Move) && !ca.HasTrait(Trait.Strike) && ca.ActionId != ActionId.EndTurn ? "You can only move or make a Strike." : (string)null)
                            });
                            await CommonSpellEffects.YourMinionActs(animalCompanion);
                        })), PossibilitySize.Half))
                    });
                };
            });
        });

        public static ItemName ThrowersBandolier { get; } = ModManager.RegisterNewItemIntoTheShop("Thrower's Bandolier", itemName => {
            return new Item(itemName, Illustrations.ThrowersBandolier, "thrower's bandolier", 3, 35,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Worn, Trait.Conjuration, ModTraits.Roguelike })
            .WithDescription("{i}This bandolier is perfect for sharing expensive weapon runes between a wide array of side arms and thrown weapons.{/i}\n\n" +
            "The thrower's bandoldier holds an infinite number of daggers, hatchets and light hammers within it, which can be drawn from the bandolier as an action {icon:Action}, or a {icon:FreeAction} if you have the Quick Draw feat.\n\n" +
            "The bandolier, unlike most worn items, can also be invested with weapon runes. These runes will affect any weapon drawn from the bandolier, making them more powerful.")
            .WithPermanentQEffectWhenWorn((qfTB, item) => {
                qfTB.StartOfCombat = async self => {
                    var dagger = Items.CreateNew(ItemName.Dagger);
                    foreach (Item rune in item.Runes) {
                        dagger.WithModificationRune(rune.ItemName);
                    }
                    dagger.Traits.Add(Trait.EncounterEphemeral);

                    var hammer = Items.CreateNew(LightHammer);
                    foreach (Item rune in item.Runes) {
                        hammer.WithModificationRune(rune.ItemName);
                    }
                    hammer.Traits.Add(Trait.EncounterEphemeral);

                    var axe = Items.CreateNew(Hatchet);
                    foreach (Item rune in item.Runes) {
                        axe.WithModificationRune(rune.ItemName);
                    }
                    axe.Traits.Add(Trait.EncounterEphemeral);

                    qfTB.Tag = new List<Item>() { dagger, hammer, axe };
                };

                qfTB.ProvideActionIntoPossibilitySection = (self, section) => {
                    if (section.PossibilitySectionId != PossibilitySectionId.ItemActions || qfTB.Tag == null) {
                        return null;
                    }

                    int cost = qfTB.Owner.HasFeat(FeatName.QuickDraw) ? 0 : 1;

                    SubmenuPossibility menu = new SubmenuPossibility(Illustrations.ThrowersBandolier, "Thrower's Bandolier");
                    menu.Subsections.Add(new PossibilitySection("Thrower's Bandolier"));

                    menu.Subsections[0].AddPossibility((ActionPossibility)new CombatAction(qfTB.Owner, (qfTB.Tag as List<Item>)[0].Illustration, "Draw Dagger", new Trait[] { Trait.Manipulate, Trait.Basic },
                    $"Draw a {(qfTB.Tag as List<Item>)[0].Name} from your thrower's bandolier.",
                    Target.Self().WithAdditionalRestriction(you => you.HeldItems.Count == 0 || (you.HeldItems.Count == 1 && !you.HeldItems[0].TwoHanded) ? null : "free hand required"))
                    .WithActionCost(cost)
                    .WithSoundEffect(SfxName.ItemGet)
                    .WithEffectOnSelf(async user => {
                        user.HeldItems.Add((qfTB.Tag as List<Item>)[0].Duplicate());
                    }));

                    menu.Subsections[0].AddPossibility((ActionPossibility)new CombatAction(qfTB.Owner, (qfTB.Tag as List<Item>)[1].Illustration, "Draw Light Hammer", new Trait[] { Trait.Manipulate, Trait.Basic },
                    $"Draw a {(qfTB.Tag as List<Item>)[1].Name} from your thrower's bandolier.",
                    Target.Self().WithAdditionalRestriction(you => you.HeldItems.Count == 0 || (you.HeldItems.Count == 1 && !you.HeldItems[0].TwoHanded) ? null : "free hand required"))
                    .WithActionCost(cost)
                    .WithSoundEffect(SfxName.ItemGet)
                    .WithEffectOnSelf(async user => {
                        user.HeldItems.Add((qfTB.Tag as List<Item>)[1].Duplicate());
                    }));

                    menu.Subsections[0].AddPossibility((ActionPossibility)new CombatAction(qfTB.Owner, (qfTB.Tag as List<Item>)[2].Illustration, "Draw Hatchet", new Trait[] { Trait.Manipulate, Trait.Basic },
                    $"Draw a {(qfTB.Tag as List<Item>)[2].Name} from your thrower's bandolier.",
                    Target.Self().WithAdditionalRestriction(you => you.HeldItems.Count == 0 || (you.HeldItems.Count == 1 && !you.HeldItems[0].TwoHanded) ? null : "free hand required"))
                    .WithActionCost(cost)
                    .WithSoundEffect(SfxName.ItemGet)
                    .WithEffectOnSelf(async user => {
                        user.HeldItems.Add((qfTB.Tag as List<Item>)[2].Duplicate());
                    }));

                    return menu;
                };
            });
        });

        public static ItemName DeathDrinkerAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("DeathDrinkerAmulet", itemName => {
            return new Item(itemName, Illustrations.SpiritBeaconAmulet, "death drinker amulet", 2, 35,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Necklace)
            .WithDescription("{i}This eerie necklace seems to feed off necrotic energy, storing it within its amethyst gems for some unknowable purpose.{/i}\n\n" +
            "While wearing this necklace, you gain resist Negative 3")
            .WithPermanentQEffectWhenWorn((qfMoC, item) => {
                qfMoC.StateCheck = self => {
                    self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Negative, 3);
                };
            });
        });

        public static ItemName GreaterDeathDrinkerAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("GreaterDeathDrinkerAmulet", itemName => {
            return new Item(itemName, Illustrations.SpiritBeaconAmulet, "death drinker amulet, greater", 4, 60,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Necklace)
            .WithDescription("{i}This eerie necklace seems to feed off necrotic energy, storing it within its amethyst gems for some unknowable purpose.{/i}\n\n" +
            "While wearing this necklace, you gain resist Negative 7")
            .WithPermanentQEffectWhenWorn((qfMoC, item) => {
                qfMoC.StateCheck = self => {
                    self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Negative, 7);
                };
            });
        });

        public static ItemName SpiritBeaconAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("Spirit Beacon Amulet", itemName => {
            return new Item(itemName, Illustrations.SpiritBeaconAmulet, "spirit beacon amulet", 3, 60,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Necklace)
            .WithDescription("{i}This skull shaped necklace is cold to the touch.{/i}\n\n" +
            "You have a +1 item bonus to Occultism.\n\n" +
            "Once per day, as an {icon:Action}action, you can use the amulet to guide a restless spirit towards an unoccupied space within 30 feet. Restless spirits unnerve adjacent creatures and may be interacted with for additional benefits.")
            .WithItemAction((item, user) => {
                return new CombatAction(user, Illustrations.SpiritBeaconAmulet, "Attract Spirits", new Trait[] { Trait.Necromancy, Trait.Magical, Trait.Manipulate },
                    "{b}Frequency{/b} once per day\n\n{b}Range{/b} 30 feet\n\nCreate a Restless Spirit hazard in the target space. Restless spirits unnerve adjacent creatures and may be interacted with for additional benefits." +
                    "\n\nBut beware, learned adversaries with knowledge of the occult may be able to exploit these spirits to use against you.",
                    Target.RangedEmptyTileForSummoning(6))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.DeepNecromancy)
                .WithProjectileCone(Illustrations.RestlessSpirit, 10, ProjectileKind.Cone)
                .WithEffectOnEachTile(async (action, caster, tiles) => {
                    caster.Battle.SpawnCreature(CreatureList.Objects[ObjectId.RESTLESS_SPIRIT](caster.Battle.Encounter), caster.Battle.Gaia, tiles[0]);
                    item.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));
                })
                ;
            }, (item, _) => !item.ItemModifications.Any(mod => mod.Kind == ItemModificationKind.UsedThisDay))
            .WithPermanentQEffectWhenWorn((qfCoA, item) => {
                qfCoA.BonusToSkills = (skill) => skill == Skill.Occultism ? new Bonus(1, BonusType.Item, "Spirit Beacon Amulet") : null;
            });
        });

        public static ItemName DemonBoundRing { get; } = ModManager.RegisterNewItemIntoTheShop("DemonBoundRing", itemName => {
            return new Item(itemName, Illustrations.DemonBoundRing, "demon bound ring", 3, 60,
                new Trait[] { Trait.Magical, Trait.Worn, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithDescription("{i}Upon slipping on this ominous looking ring, incessant whispers fill your head, whispering arcane secrets and begging you to let it out.{/i}\n\n" +
            "While wearing this ring, the wear gains a +1 item bonus to arcana checks, and may use the Unleash Demon {icon:ThreeActions} action once per day.")
            .WithItemAction((item, user) => {
                return new CombatAction(user, Illustrations.DemonBoundRing, "Unleash Demon", new Trait[] { Trait.Conjuration, Trait.Magical, Trait.Manipulate },
                    "{b}Frequency{/b} once per day\n\n{b}Range{/b} 30 feet\n\nYou summon a Wrecker Demon whose level is equal to your own.\n\n" +
                    "At the start of your turn, there's a 25% chance that the demon will go slip from your control, turning against the party." +
                    "\n\nImmediately when you cast this spell and then once each turn when you Sustain this spell, you can take two actions as " +
                    "the summoner creature. If you don't Sustain this spell during a turn, the summoner creature will go away." +
                    "\n\nOnce rogue however, the demon can no longer be banished in this way and will persist even after you fall unconscious.",
                    Target.RangedEmptyTileForSummoning(6))
                .WithActionCost(3)
                .WithSoundEffect(SfxName.DeepNecromancy)
                .WithEffectOnEachTile(async (action, caster, tiles) => {
                    Creature demon = MonsterStatBlocks.CreateAbrikandilu();
                    if (caster.Level <= 2) {
                        demon.ApplyWeakAdjustments(false, true);
                    } else if (caster.Level == 3) {
                        demon.ApplyWeakAdjustments(false);
                    }
                    demon.AddQEffect(new QEffect("Bound Demon", $"This demon is bound to {caster.Name}'s will. At the start of its master's turn, there's a 25% chance that it will break free of their control and turn against the party.") {
                        Id = QEffectId.SummonedBy,
                        Source = caster
                    });
                    demon.EntersInitiativeOrder = false;
                    demon.Traits.Add(Trait.Minion);
                    QEffect sustainedeffect = new QEffect {
                        Id = QEffectId.SummonMonster,
                        CannotExpireThisTurn = true,
                        Source = caster,
                        ExpiresAt = ExpirationCondition.ExpiresAtEndOfSourcesTurn,
                        CountsAsBeneficialToSource = true,
                        StateCheck = self => {
                            if (self.Owner.HasEffect(QEffectId.Confused)) {
                                demon.AddQEffect(QEffect.Confused(false, CombatAction.CreateSimple(self.Owner)).WithExpirationEphemeral());
                            }
                        },
                        StartOfYourPrimaryTurn = async (self, creature) => {
                            int roll = R.NextD20();
                            if (roll <= 5) {
                                demon.Traits.Remove(Trait.Minion);
                                self.WhenExpires = null;
                                demon.OwningFaction = demon.Battle.Enemy;
                                demon.EntersInitiativeOrder = true;
                                demon.RecalculateLandSpeedAndInitiative();
                                demon.Initiative = DiceFormula.FromText("1d20+" + demon.InitiativeBonus).Roll().Item1;
                                int index = demon.Battle.InitiativeOrder.FindLastIndex((Creature cr) => cr.Initiative > demon.Initiative) + 1;
                                if (demon.Battle.GameLoop.InitialInitiativeOrderIsSet) {
                                    demon.Battle.Log(demon?.ToString() + " rolls for initiative: " + demon.Initiative);
                                }

                                demon.Battle.InitiativeOrder.Insert(index, demon);
                                demon.Occupies.Overhead("*breaks free*", Color.DarkRed, $"{caster.Name}'s summoned {demon.Name} breaks free from their control!");
                                self.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        },
                        WhenExpires = delegate {
                            if (!demon.DeathScheduledForNextStateCheck) {
                                demon.Occupies.Overhead("*banished*", Color.Black, demon?.ToString() + " is banished because its summoner didn't sustain the summoning spell.");
                                demon.DeathScheduledForNextStateCheck = true;
                            }
                        },
                        WhenMonsterDies = delegate (QEffect casterQf) {
                            if (!demon.DeathScheduledForNextStateCheck) {
                                demon.Occupies.Overhead("*banished*", Color.Black, demon?.ToString() + " is banished because its summoner didn't sustain the summoning spell.");
                                demon.DeathScheduledForNextStateCheck = true;
                            }
                            casterQf.Owner.Battle.GameLoop.NewStateCheckRequired = true;
                        }
                    };
                    caster.AddQEffect(sustainedeffect);
                    caster.AddQEffect(new QEffect("Sustaining " + action.Name, "You're sustaining an effect and it will expire if you don't sustain it every turn.", ExpirationCondition.Never, caster, demon.Illustration) {
                        Id = QEffectId.Sustaining,
                        Tag = sustainedeffect,
                        DoNotShowUpOverhead = true,
                        ProvideContextualAction = (QEffect qf) => (!sustainedeffect.CannotExpireThisTurn) ? new ActionPossibility(new CombatAction(qf.Owner, action.Illustration, "Sustain " + action.Name, new Trait[3]
                        {
                            Trait.Concentrate,
                            Trait.SustainASpell,
                            Trait.Basic
                        }, "The duration of " + action.Name + " continues until the end of your next turn.\n\nThe summoned creature takes its turn when you sustain the spell. You choose what actions it takes.",
                        Target.Self())
                        .WithEffectOnSelf(async self => {
                            sustainedeffect.CannotExpireThisTurn = true;
                            // await CommonSpellEffects.YourMinionActs(demon);
                            demon.Actions.ResetToFull();
                            await CommonSpellEffects.YourMinionActs(demon);
                            //TBattle battle = demon.Battle;
                            //Creature oldActiveCreature = battle.ActiveCreature;
                            //battle.ActiveCreature = demon;
                            //battle.SmartCenterIfNotVisible?.Invoke(demon.Occupies);
                            //await demon.Battle.GameLoop.MinionMainPhase(demon);
                            //battle.ActiveCreature = oldActiveCreature;

                        })).WithPossibilityGroup("Maintain an activity") : null,
                        StateCheck = delegate (QEffect qf) {
                            if (sustainedeffect.Owner.Destroyed || !sustainedeffect.Owner.HasEffect(sustainedeffect) || demon.DeathScheduledForNextStateCheck) {
                                qf.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        }
                    });

                    caster.Battle.SpawnCreature(demon, caster.OwningFaction, tiles[0]);
                    item.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));
                    demon.Actions.ResetToFull();
                    await CommonSpellEffects.YourMinionActs(demon);
                })
                ;
            }, (item, _) => !item.ItemModifications.Any(mod => mod.Kind == ItemModificationKind.UsedThisDay))
            .WithPermanentQEffectWhenWorn((qfCoA, item) => {
                qfCoA.BonusToSkills = (skill) => skill == Skill.Arcana ? new Bonus(1, BonusType.Item, "Demon Bound Ring") : null;
            });
        });

        public static ItemName HornOfTheHunt { get; } = ModManager.RegisterNewItemIntoTheShop("Horn of the Hunt", itemName => {
            return new Item(itemName, Illustrations.HornOfTheHunt, "horn of the hunt", 3, 60,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Conjuration, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Necklace)
            .WithDescription("{i}This ancient bone hunting horn is carved into the likeness of a snarling wolf, and seems possessed of some strange primal magic.{/i}\n\n" +
            "You have a +1 item bonus to Nature.\n\n" +
            "Once per day, you may blow the horn {icon:ThreeActions} to summon forth a pack of three wolves to attack an enemy of your choice. The wolves ignore all other creatures and return to the horn once their task is complete.")
            .WithItemAction((item, user) => {
                return new CombatAction(user, Illustrations.HornOfTheHunt, "Blow Horn", new Trait[] { Trait.Conjuration, Trait.Magical, Trait.Manipulate, Trait.Primal },
                    "{b}Frequency{/b} once per day\n\n{b}Range{/b} 30 feet\n\n{b}Target{/b} 1 enemy creature\n\n" +
                    "Summon 3 wolves in any unnocupied spaces within 15 feet of your target. The wolves' level is equal to that of the blower's - 3, and they will persist until their prey has been slain, carrying out their task to the best of their ability and ignoring all other creatures.",
                    Target.Ranged(6))
                .WithActionCost(3)
                .WithSoundEffect(SfxName.BeastRoar)
                .WithProjectileCone(Illustrations.HornOfTheHunt, 3, ProjectileKind.Cone)
                .WithEffectOnEachTarget(async (spell, caster, d, r) => {
                    QEffect mark = new QEffect("Mark of the Hunt", "This creature is being hunted by wolves.", ExpirationCondition.Never, caster, new SameSizeDualIllustration(Illustrations.StatusBackdrop, Illustrations.HornOfTheHunt));
                    d.AddQEffect(mark);

                    List<Creature> wolves = new List<Creature>();

                    for (int i = 0; i < 3; ++i) {
                        await caster.Battle.GameLoop.StateCheck();
                        List<Option> options = new List<Option>();
                        CombatAction summon = new CombatAction(caster, Illustrations.HornOfTheHunt, "Summon Hunting Hound", new Trait[] { }, "", Target.RangedEmptyTileForSummoning(100))
                        .WithEffectOnEachTile(async (_, _, subtiles) => {
                            Creature wolf = MonsterStatBlocks.CreateWolf();
                            wolf.AddQEffect(new QEffect("Call of the Hunt", $"This creature is compelled to attack {d.Name} and will vanish after its task is complete.", ExpirationCondition.Never, d, Illustrations.HornOfTheHunt) {
                                StateCheck = self => {
                                    if (!self.Source.AliveOrUnconscious || !self.Owner.Alive) {
                                        self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                                    }
                                },
                                AdditionalGoodness = (self, action, target) => {
                                    return target != self.Source ? -1000f : 10f;
                                }
                            });
                            // Scale wolf level
                            if (caster.Level <= 2) {
                                wolf.ApplyWeakAdjustments(false, true);
                            } else if (caster.Level == 3) {
                                wolf.ApplyWeakAdjustments(false, true);
                            } else if (caster.Level >= 5) {
                                wolf.ApplyEliteAdjustments();
                            }
                            caster.Battle.SpawnCreature(wolf, caster.Battle.GaiaFriends, subtiles[0]);
                            wolves.Add(wolf);
                        });

                        foreach (Tile tile in caster.Battle.Map.AllTiles) {
                            if (!(tile.IsTrulyGenuinelyFreeToEveryCreature && tile.DistanceTo(d.Occupies) <= 3)) {
                                continue;
                            }

                            Option option = Option.ChooseTile(summon.Name, tile, async delegate {
                                ChosenTargets target = new ChosenTargets();
                                target.ChosenTile = tile;
                                target.ChosenTiles.Add(tile);
                                await caster.Battle.GameLoop.FullCast(summon, target);
                            }, int.MinValue, true).WithIllustration(summon.Illustration);

                            options.Add(option);
                        }

                        if (options.Count > 0) {
                            Option chosenOption;
                            if (options.Count >= 2) {
                                options.Add(new CancelOption(true));
                                chosenOption = (await caster.Battle.SendRequest(new AdvancedRequest(caster, "Choose space to summon a wolf.", options) {
                                    TopBarText = $"Choose an empty space to Summon a Wolf or right-click to cancel. ({i + 1}/3)",
                                    TopBarIcon = Illustrations.HornOfTheHunt
                                })).ChosenOption;
                            } else
                                chosenOption = options[0];

                            if (chosenOption is CancelOption) {
                                d.RemoveAllQEffects(qf => qf == mark);
                                spell.RevertRequested = true;
                                return;
                            }
                            int num = await chosenOption.Action() ? 1 : 0;
                        }
                    }

                    mark.Tag = wolves;
                    mark.StateCheck = self => {
                        if (!(self.Tag as List<Creature>).Any(cr => cr.Alive)) {
                            self.ExpiresAt = ExpirationCondition.Immediately;
                        }
                    };

                    item.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));
                })
                ;
            }, (item, _) => !item.ItemModifications.Any(mod => mod.Kind == ItemModificationKind.UsedThisDay))
            .WithPermanentQEffectWhenWorn((qfCoA, item) => {
                qfCoA.BonusToSkills = (skill) => skill == Skill.Nature ? new Bonus(1, BonusType.Item, "horn of the hunt") : null;
            });
        });

        public static ItemName ShifterFurs { get; } = ModManager.RegisterNewItemIntoTheShop("Shifter Furs", itemName => {
            return new Item(itemName, Illustrations.ShifterFurs, "shifter furs", 3, 60,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Transmutation, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Cloak)
            .WithDescription("{i}This haggard fur cloak is has a musky, feral smell to it and seems to pulsate warmly... as if it were not simply a cloak, but the flesh of a living, breathing thing.{/i}\n\n" +
            "You have a +2 item bonus to Demoralize check made against animals, and gain the benefits of the Intimidating Glare feat.\n\n" +
            "Once per encounter, as a {icon:FreeAction} action, you may invoke the cloak's magic to assume a random animal form until the start of your next turn.")
            .WithItemAction((item, user) => {
                if (user.FindQEffect(QEffectIds.ShifterFurs) != null) {
                    return null;
                }

                return new CombatAction(user, Illustrations.ShifterFurs, "Activate Shifter Furs", new Trait[] { Trait.Transmutation, Trait.Magical }, "{b}Frequency{/b} once per encounter\n\n{b}Target{/b} self\n\nYou assume the form of a random enhanced animal form until the start of your next turn.", Target.Self())
                .WithActionCost(0)
                .WithSoundEffect(SfxName.BeastRoar)
                .WithEffectOnSelf(caster => {
                    List<Item> previouslyHeldItems = new List<Item>();
                    foreach (Item obj in caster.HeldItems.ToList<Item>()) {
                        if (obj.Grapplee == null) {
                            previouslyHeldItems.Add(obj);
                            caster.CarriedItems.Add(obj);
                            caster.HeldItems.Remove(obj);
                        }
                    }

                    QEffect transform = new QEffect() {
                        ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn,
                        PreventTakingAction = action => action.HasTrait(Trait.Spell) ? "Cannot cast spells whilst transformed." : null,
                        BonusToAttackRolls = (self, action, target) => action.HasTrait(Trait.BattleformAttack) ? new Bonus(2, BonusType.Status, "Shifter Furs"): null,
                        WhenExpires = self => {
                            foreach (Item obj in caster.HeldItems.ToList<Item>())
                                caster.DropItem(obj);
                            foreach (Item obj in previouslyHeldItems) {
                                if (caster.CarriedItems.Contains(obj)) {
                                    caster.CarriedItems.Remove(obj);
                                    caster.HeldItems.Add(obj);
                                }
                            }
                        }
                    };

                    int roll = R.Next(1, 4);
                    switch (roll) {
                        case 1:
                            transform.Illustration = IllustrationName.AnimalFormBear;
                            transform.Name = "Bear Form";
                            transform.Description = $"{user.Name} has assumed the form of a ferocious bear, capable of grappling its prey on a successful jaws attack and with a +2 item bonus to armour.";
                            transform.StateCheck = self => {
                                self.Owner.ReplacementIllustration = IllustrationName.AnimalFormBear;
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
                            goto case 10;
                        case 2:
                            transform.Illustration = IllustrationName.AnimalFormSnake;
                            transform.Name = "Serpent Form";
                            transform.Description = $"{user.Name} has assumed the form of a venomous serpent, capable of poisoning its prey on a successful jaws attack.";
                            transform.StateCheck = self => {
                                self.Owner.ReplacementIllustration = IllustrationName.AnimalFormSnake;
                                self.Owner.AddQEffect(Affliction.CreateInjuryQEffect(Affliction.CreateSnakeVenom("Snake Venom")).WithExpirationEphemeral());
                                self.Owner.AddQEffect(QEffect.Swimming().WithExpirationEphemeral());
                                self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Slashing, 2 + self.Owner.Level);
                                self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Piercing, 2 + self.Owner.Level);
                                self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d6", DamageKind.Piercing, Trait.BattleformAttack, Trait.AddsInjuryPoison).WithAdditionalWeaponProperties(properties => {
                                    properties.AdditionalDamage.Add(("1d4", DamageKind.Poison));
                                });
                                self.Owner.AddQEffect(QEffect.Swimming().WithExpirationEphemeral());
                            };
                            transform.SetBaseSpeedTo = 8;
                            goto case 10;
                        case 3:
                            transform.Illustration = IllustrationName.AnimalFormWolf;
                            transform.Name = "Wolf Form";
                            transform.StateCheck = self => {
                                self.Owner.ReplacementIllustration = IllustrationName.AnimalFormWolf;
                                self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d10", DamageKind.Piercing, Trait.BattleformAttack, Trait.Unarmed);
                                if (self.Owner.QEffects.FirstOrDefault(qf => qf.Name == "Sneak Attack") != null) {
                                    self.Owner.AddQEffect(QEffect.PackAttack(self.Owner.Name, "1d8").WithExpirationEphemeral());
                                    transform.Description = $"{user.Name} has assumed the form of a cunning wolf, granting them 1d8 pack attack damage.";
                                } else {
                                    self.Owner.AddQEffect(QEffect.SneakAttack("1d8").WithExpirationEphemeral());
                                    transform.Description = $"{user.Name} has assumed the form of a cunning wolf, granting them 1d8 sneak attack damage.";
                                }
                            };
                            goto case 10;
                        case 10:
                            transform.Description += " Whilst transformed, you cannot cast spells.";
                            user.AddQEffect(transform);
                            break;
                        default:
                            break;
                    }

                    user.AddQEffect(new QEffect() {
                        Id = QEffectIds.ShifterFurs,
                    });
                })
                ;
            }, (_, _) => true)
            .WithPermanentQEffectWhenWorn((qfCoA, item) => {
                qfCoA.BonusToSkillChecks = (skill, action, d) => action.ActionId == ActionId.Demoralize && d.HasTrait(Trait.Animal) ? new Bonus(2, BonusType.Item, "shifter furs") : null;
                qfCoA.Id = QEffectId.IntimidatingGlare;
                qfCoA.EndOfCombat = async (self, won) => {
                    ItemModification used = item.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay);
                    if (used != null) {
                        item.ItemModifications.Remove(used);
                    }
                };
            });
        });

        public static ItemName CloakOfAir { get; } = ModManager.RegisterNewItemIntoTheShop("Cloak of Air", itemName => {
            return new Item(itemName, Illustrations.CloakOfAir, "cloak of air", 3, 60,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Evocation, Trait.Elemental, Trait.Air, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Cloak)
            .WithDescription("This swooshing cloak seems to be perpetually billowing in the wind. While donned, it grants the wear several benefits.\n• The wearer of this cloak may dramatically swoosh it about, to send a blade of air at their enemies for 1d8 damage, 20ft range.\n• When you Leap while wearing this cloak, you increase the distance you can jump horizontally by 5 feet. {i}(This stacks with the Powerful Leap feat, but not other items){/i}\n• Kineticists wearing this cloak deal +2 damage with their air impulses.")
            .WithItemAction((item, user) => {
                Item weapon = new Item(IllustrationName.AerialBoomerang256, "Slicing Wind", new Trait[] { Trait.Ranged, Trait.Air, Trait.Magical, Trait.Unarmed }).WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing) {
                    VfxStyle = new VfxStyle(5, ProjectileKind.Cone, IllustrationName.AerialBoomerang256),
                    Sfx = SfxName.AeroBlade
                }.WithRangeIncrement(4));
                return user.CreateStrike(weapon);
                //return (ActionPossibility)new CombatAction(user, IllustrationName.AerialBoomerang256, "Cutting Wind", new Trait[] { Trait.Air, Trait.Evocation, Trait.Ranged, Trait.Magical }, Target.RangedCreature(5));
            }, (_, _) => true)
            .WithPermanentQEffectWhenWorn((qfCoA, item) => {
                qfCoA.Id = QEffectId.PowerfulLeap2;
                qfCoA.Innate = true;
                qfCoA.Name = "Cloak of Air";
                qfCoA.Description = "+2 damage to Air impulses.";
                qfCoA.BonusToDamage = (self, action, target) => {
                    if (action != null && action.HasTrait(Trait.Impulse) && action.HasTrait(Trait.Air)) {
                        return new Bonus(2, BonusType.Item, "Cloak of Air");
                    }
                    return null;
                };
            });
        });

        public static ItemName BloodBondAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("Blood Bond Amulet", itemName => {
            return new Item(itemName, Illustrations.BloodBondAmulet, "blood bond amulet", 3, 40,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Necklace)
            // TODO: Add description
            .WithDescription("These matching amulets link the lifeforce of those who wear them, allowing them to freely claw away the vitality of the paired wearer for themselves.\n\n" +
            "{b}Life Transfer {icon:Action}.{/b} You extract the lifeforce from an ally wearing a matching amulet, dealing 2d8 damage, and healing yourself for an equivalent amount of HP.")
            .WithItemAction((item, user) => {
                return new CombatAction(user, IllustrationName.BloodVendetta, "Life Transfer", new Trait[] { Trait.Magical, Trait.Necromancy, Trait.Healing }, "You extract the lifeforce from an ally wearing a matching amulet, dealing 2d8 damage, and healing yourself for an equivalent amount of HP.", Target.RangedFriend(3)
                .WithAdditionalConditionOnTargetCreature(new FriendCreatureTargetingRequirement())
                .WithAdditionalConditionOnTargetCreature((a, d) => {
                    if (d.CarriedItems.Any(i => i.ItemName == BloodBondAmulet && i.IsWorn == true)) {
                        return Usability.Usable;
                    } else {
                        return Usability.NotUsableOnThisCreature("No matching amulet");
                    }
                }))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.ElementalBlastWater)
                .WithProjectileCone(IllustrationName.VampiricExsanguination, 7, ProjectileKind.Ray)
                .WithEffectOnEachTarget(async (spell, caster, target, checkResult) => {
                    int prevHP = target.HP;
                    await CommonSpellEffects.DealDirectDamage(spell, DiceFormula.FromText("2d8", "Activate Blood Bond"), target, CheckResult.Success, DamageKind.Bleed);
                    int healAmount = prevHP - target.HP;
                    await caster.HealAsync(DiceFormula.FromText(healAmount.ToString(), "Activate Blood Bond"), spell);
                })
                ;
            }, (_, _) => true);
        });

        internal static void LoadItems() {

            //items.Add("wand of fireball", CreateWand(SpellId.Fireball, 3));
            //items.Add("wand of bless", CreateWand(SpellId.Fireball, 3));
            //items.Add("wand of bless", CreateWand(SpellId.Fireball, 3));

            List<ItemName> items = new List<ItemName>() { SpiderHatchling, AlicornDagger, AlicornPike, ThrowersBandolier, SpellbanePlate, SceptreOfTheSpider, DeathDrinkerAmulet, GreaterDeathDrinkerAmulet, RobesOfTheWarWizard, GreaterRobesOfTheWarWizard, WhisperMail, KrakenMail, DuelingSpear, DemonBoundRing, ShifterFurs, SmokingSword, StormHammer, ChillwindBow, Sparkcaster, HungeringBlade, SpiderChopper, WebwalkerArmour, DreadPlate, Hexshot, ProtectiveAmulet, MaskOfConsumption, FlashingRapier, Widowmaker, DolmanOfVanishing, BloodBondAmulet };

            // Wands
            CreateWand(SpellId.Fireball, null);
            CreateWand(SpellId.Bless, null);
            CreateWand(SpellId.Boneshaker, null);
            CreateWand(SpellId.MagicMissile, null);
            CreateWand(SpellId.Fear, null);
            CreateWand(SpellId.Fear, 3);
            CreateWand(SpellId.Blur, null);
            CreateWand(SpellId.MirrorImage, null);
            CreateWand(SpellId.TrueStrike, null);
            CreateWand(SpellId.AcidArrow, null);
            CreateWand(SpellId.Barkskin, null);
            CreateWand(SpellId.ObscuringMist, null);
            CreateWand(SpellId.Bane, null);
            CreateWand(SpellId.Grease, null);
            CreateWand(SpellId.MagicWeapon, null);
            CreateWand(SpellId.ShockingGrasp, 3);
            CreateWand(SpellId.BoneSpray, null);
            CreateWand(SpellId.SpiritualWeapon, null);
            CreateWand(SpellId.Restoration, null);
            CreateWand(SpellId.SummonElemental, 3);

            // Item QEffects

            // Armour effects
            ModManager.RegisterActionOnEachCreature(creature => {
                if (creature.BaseArmor == null) {
                    return;
                }

                if (creature.BaseArmor.ItemName == SpellbanePlate) {
                    creature.AddQEffect(new QEffect("Spellbane Plate", "You have a +1 item bonus vs. all spell saving throws but cannot cast spells of your own.") {
                        BonusToDefenses = (self, action, defence) => defence != Defense.AC && action != null && action.HasTrait(Trait.Spell) ? new Bonus(1, BonusType.Item, "Spellbane Plate") : null,
                        PreventTakingAction = action => action.HasTrait(Trait.Spell) ? "blocked by spellbane plate" : null
                    });
                }

                if (creature.BaseArmor.ItemName == WhisperMail) {
                    var aura = creature.AnimationData.AddAuraAnimation(IllustrationName.BaneCircle, 1, Color.Black);
                    aura.MaximumOpacity = 0.5f;
                    creature.AddQEffect(new QEffect("Whisper Mail", "Adjacent enemies gain weakness 1 to slashing, bludgeoning and piercing damage.") {
                        StateCheck = self => {
                            foreach (Creature enemy in self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsEnemy && cr.DistanceTo(self.Owner) <= 1)) {
                                enemy.AddQEffect(new QEffect("Whisper Mail Aura", "You suffer weakness 1 to physical damage, as the whisper mail imparts your openings to your enemies.", ExpirationCondition.Ephemeral, self.Owner, new SameSizeDualIllustration(Illustrations.StatusBackdrop, Illustrations.WhisperMail)));
                                enemy.WeaknessAndResistance.AddWeakness(DamageKind.Slashing, 1);
                                enemy.WeaknessAndResistance.AddWeakness(DamageKind.Bludgeoning, 1);
                                enemy.WeaknessAndResistance.AddWeakness(DamageKind.Piercing, 1);
                            }
                        },
                        BonusToAttackRolls = (self, action, target) => {
                            if (action != null && action.ActionId == ActionId.Seek) {
                                return new Bonus(1, BonusType.Item, "Whisper Mail");
                            }
                            return null;
                        }
                    });
                }

                if (creature.BaseArmor.ItemName == RobesOfTheWarWizard) {
                    creature.AddQEffect(new QEffect("Robes of the War Wizard", "You deal +2 damage per spell level against enemies with 15-feet, who you hit with a non-cantrip cone or emanation spell.") {
                        BonusToDamage = (self, action, target) => {

                            if (!action.HasTrait(Trait.Spell) || action.HasTrait(Trait.Cantrip) || self.Owner.DistanceTo(target) > 3) {
                                return null;
                            }

                            SpellVariant? varient = action.ChosenVariant;
                            DependsOnActionsSpentTarget? vas = action.Target is DependsOnActionsSpentTarget ? (DependsOnActionsSpentTarget)action.Target : null;

                            if (vas != null && !( (action.ActuallySpentActions == 1 && (vas.IfOneAction is EmanationTarget || vas.IfOneAction is ConeAreaTarget)) || (action.ActuallySpentActions == 2 && (vas.IfTwoActions is EmanationTarget || vas.IfTwoActions is ConeAreaTarget))
                            || (action.ActuallySpentActions == 3 && (vas.IfThreeActions is EmanationTarget || vas.IfThreeActions is ConeAreaTarget))) ) {
                                return null;
                            } else if (vas == null && varient != null && !(varient.TargetInThisVariant is EmanationTarget || varient.TargetInThisVariant is ConeAreaTarget)) {
                                return null;
                            } else if (vas == null && varient == null && !(action.Target is EmanationTarget || action.Target is ConeAreaTarget)) {
                                return null;
                            }

                            return new Bonus(2 * action.SpellLevel, BonusType.Item, "Robes of the War Wizard", true);
                        },
                        StateCheck = self => {
                            self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Acid, 1);
                            self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Cold, 1);
                            self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Fire, 1);
                            self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Electricity, 1);
                        }
                    });
                }

                if (creature.BaseArmor.ItemName == GreaterRobesOfTheWarWizard) {
                    creature.AddQEffect(new QEffect("Robes of the War Wizard", "You deal +3 damage per spell level against enemies with 15-feet, who you hit with a non-cantrip cone or emanation spell.") {
                        BonusToDamage = (self, action, target) => {

                            if (!action.HasTrait(Trait.Spell) || action.HasTrait(Trait.Cantrip) || self.Owner.DistanceTo(target) > 3) {
                                return null;
                            } 

                            SpellVariant? varient = action.ChosenVariant;
                            DependsOnActionsSpentTarget? vas = action.Target is DependsOnActionsSpentTarget ? (DependsOnActionsSpentTarget)action.Target : null;

                            if (vas != null && !((action.ActuallySpentActions == 1 && (vas.IfOneAction is EmanationTarget || vas.IfOneAction is ConeAreaTarget)) || (action.ActuallySpentActions == 2 && (vas.IfTwoActions is EmanationTarget || vas.IfTwoActions is ConeAreaTarget))
                            || (action.ActuallySpentActions == 3 && (vas.IfThreeActions is EmanationTarget || vas.IfThreeActions is ConeAreaTarget)))) {
                                return null;
                            } else if (vas == null && varient != null && !(varient.TargetInThisVariant is EmanationTarget || varient.TargetInThisVariant is ConeAreaTarget)) {
                                return null;
                            } else if (vas == null && varient == null && !(action.Target is EmanationTarget || action.Target is ConeAreaTarget)) {
                                return null;
                            }

                            return new Bonus(3 * action.SpellLevel, BonusType.Item, "Robes of the War Wizard", true);
                        },
                        StateCheck = self => {
                            self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Acid, 2);
                            self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Cold, 2);
                            self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Fire, 2);
                            self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Electricity, 2);
                        }
                    });
                }

                if (creature.BaseArmor.ItemName == KrakenMail) {
                    creature.AddQEffect(new QEffect() {
                        Id = QEffectId.Swimming,
                        BonusToAttackRolls = (self, action, target) => (self.Owner.HasEffect(QEffectId.AquaticCombat) || self.Owner.Occupies.Kind == TileKind.ShallowWater || self.Owner.Occupies.Kind == TileKind.Water) && action.ActionId == ActionId.Grapple ? new Bonus(2, BonusType.Status, "Kraken Mail", true) : null
                    });
                }

                if (creature.BaseArmor.ItemName == DreadPlate) {
                    creature.AddQEffect(new QEffect("Dread Plate", "You take an additional 1d4 negative damage when damaged by an adjacent creature, but deal 1d6 negative damage in return.") {
                        AfterYouTakeDamage = async (self, amount, kind, action, critical) => {
                            if (action == null || action.Owner == null) {
                                return;
                            }

                            if (!action.Owner.IsAdjacentTo(self.Owner)) {
                                return;
                            }

                            await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Dread Plate"), action.Owner, CheckResult.Success, DamageKind.Negative);
                            await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d4", "Dread Plate"), self.Owner, CheckResult.Success, DamageKind.Negative);
                        }
                    });
                }

                if (creature.BaseArmor.ItemName == WebwalkerArmour) {
                    creature.AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb });
                }

                if (creature.BaseArmor.ItemName == DolmanOfVanishing) {
                    creature.AddQEffect(new QEffect("Cloak of Vanishing", "This creature moves through webs unimpeded.") {
                        Id = QEffectId.HideInPlainSight,
                        BonusToSkills = skill => skill == Skill.Stealth ? new Bonus(2, BonusType.Item, "Dolman of Vanishing") : null
                    });
                }
            });

            // Boosted weapons
            ModManager.RegisterActionOnEachCreature(creature => {
                creature.AddQEffect(new QEffect() {
                    StateCheck = (qf) => {
                        Creature wielder = qf.Owner;

                        foreach (Item weapon in wielder.HeldItems) {
                            if (weapon.HasTrait(ModTraits.BoostedWeapon)) {
                                wielder.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                                    ProvideStrikeModifierAsPossibility = item => {
                                        if (item != weapon) {
                                            return null;
                                        }

                                        if (item.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay) != null) {
                                            return null;
                                        }

                                        if (wielder.PersistentCharacterSheet == null) {
                                            return null;
                                        }

                                        CombatAction action = new CombatAction(wielder, weapon.Illustration, $"Activate {weapon.Name.CapitalizeEachWord()}", new Trait[] { Trait.Concentrate },
                                            "{b}Frequency{/b} once per encounter\n\nUntil the end of your turn, this weapon deals 1d6 extra elemental damage instead of just 1.\n\nAfter you use this action, you can't do so again for the rest of the encounter.", Target.Self())
                                        .WithActionCost(1)
                                        .WithSoundEffect(SfxName.Abjuration)
                                        .WithEffectOnSelf(user => {
                                            // Effect
                                            weapon.WeaponProperties.AdditionalDamage[0] = ("1d6", weapon.WeaponProperties.AdditionalDamage[0].Item2);
                                            weapon.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));

                                            // Show effect
                                            user.AddQEffect(new QEffect($"{weapon.Name.CapitalizeEachWord()} Activated", "Extra elemental damage increased from 1 to 1d6.") {
                                                ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
                                                Tag = weapon,
                                                WhenExpires = (self) => {
                                                    Item weapon = self.Tag as Item;
                                                    weapon.WeaponProperties.AdditionalDamage[0] = ("1", weapon.WeaponProperties.AdditionalDamage[0].Item2);
                                                }
                                            });
                                            // Run end of combat cleanup
                                            user.AddQEffect(new QEffect() {
                                                Tag = weapon,
                                                EndOfCombat = async (self, won) => {
                                                    weapon.WeaponProperties.AdditionalDamage[0] = ("1", weapon.WeaponProperties.AdditionalDamage[0].Item2);
                                                    ItemModification used = weapon.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay);
                                                    if (used != null) {
                                                        weapon.ItemModifications.Remove(used);
                                                    }
                                                }
                                            });
                                        });

                                        action.Item = weapon;

                                        return (ActionPossibility)action;
                                    }
                                });
                            }
                        }
                    }
                });
            });

            // Wands
            ModManager.RegisterActionOnEachCreature(creature => {
                creature.AddQEffect(new QEffect() {
                    StateCheck = (qf) => {
                        Creature wandHolder = qf.Owner;

                        foreach (Item wand in wandHolder.HeldItems) {
                            if (wand.HasTrait(ModTraits.Wand)) {
                                wandHolder.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                                    ProvideStrikeModifierAsPossibility = item => {
                                        if (item != wand) {
                                            return null;
                                        }

                                        ItemModification? used = item.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay);

                                        if (used != null && used.Tag != null) {
                                            bool secondUse = (bool)used.Tag;
                                            if (secondUse == true) {
                                                return null;
                                            }
                                        }

                                        if (wandHolder.PersistentCharacterSheet == null) {
                                            return null;
                                        }

                                        if (!wandHolder.PersistentCharacterSheet.Calculated.SpellTraditionsKnown.ContainsOneOf(wand.Traits)) {
                                            return null;
                                        }

                                        Spell spell = (Spell)item.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.CustomPermanent && mod.Tag != null && mod.Tag is Spell).Tag;
                                        spell = spell.Duplicate(wandHolder, spell.SpellLevel, true);

                                        if (spell == null) {
                                            return null;
                                        }

                                        Possibility spellPossibility = Possibilities.CreateSpellPossibility(spell.CombatActionSpell);
                                        spellPossibility.PossibilitySize = PossibilitySize.Full;
                                        if (used != null && used.Tag != (object)true) {
                                            spellPossibility.Illustration = new ScrollIllustration(IllustrationName.Broken, spellPossibility.Illustration);
                                            spellPossibility.Caption += " (Overcharge)";
                                        }
                                        if ((spellPossibility as SubmenuPossibility) == null) {
                                            CombatAction action = (spellPossibility as ActionPossibility).CombatAction;
                                            action.Owner = wandHolder;
                                            action.Item = wand;
                                            action.SpellcastingSource = wandHolder.Spellcasting.Sources.FirstOrDefault(source => action.Traits.Contains(source.SpellcastingTradition));

                                            if (used != null && used.Tag != (object)true) {
                                                action.Illustration = new ScrollIllustration(IllustrationName.Broken, action.Illustration);
                                                action.Name += " (Overcharge)";
                                                action.Description += "\n\n{b}Overcharged.{/b} Overcharging your wand to cast this spell has a 50% chance of permanantly destroying it.";
                                            }
                                        } else {
                                            SubmenuPossibility spellVars = spellPossibility as SubmenuPossibility;
                                            bool firstLoop = true;
                                            foreach (var varient in spellVars.Subsections[0].Possibilities) {
                                                CombatAction action;
                                                if (varient is ChooseActionCostThenActionPossibility) {
                                                    action = (varient as ChooseActionCostThenActionPossibility).CombatAction;
                                                } else {
                                                    action = (varient as ChooseVariantThenActionPossibility).CombatAction;
                                                }
                                                action.Owner = wandHolder;
                                                action.Item = wand;
                                                action.SpellcastingSource = wandHolder.Spellcasting.Sources.FirstOrDefault(source => action.Traits.Contains(source.SpellcastingTradition));

                                                if (used != null && used.Tag != (object)true) {
                                                    if (firstLoop) {
                                                        action.Illustration = new ScrollIllustration(IllustrationName.Broken, action.Illustration);
                                                        action.Name += " (Overcharge)";
                                                        action.Description += "\n\n{b}Overcharged.{/b} Overcharging your wand to cast this spell has a 50% chance of permanently destroying it.";
                                                    }
                                                }
                                                firstLoop = false;
                                            }
                                        }
                                        return spellPossibility;
                                    },
                                    AfterYouTakeAction = async (self, action) => {
                                        if (action.SpellId != SpellId.None && action.Item != null && action.Item == wand) {
                                            ItemModification? used = wand.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay);

                                            if (used == null) {
                                                wand.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));
                                            } else {
                                                used.Tag = true;
                                                (CheckResult, string) result = Checks.RollFlatCheck(10);
                                                wandHolder.Occupies.Overhead(result.Item1 >= CheckResult.Success ? "Overcharge success!" : $"{wand.Name} was destoyed!", result.Item1 >= CheckResult.Success ? Color.Green : Color.Red, result.Item2, result.Item1 <= CheckResult.Failure ? $"{wand.Name} was permanantly destroyed from being overcharged!" : null);
                                                if (result.Item1 <= CheckResult.Failure) {
                                                    Sfxs.Play(SoundEffects.WandOverload, 1.2f);
                                                    wandHolder.HeldItems.Remove(wand);
                                                }
                                            }
                                        }
                                    }
                                });
                            }
                        }
                    }
                });
            });
        }

        internal static ItemName CreateWand(SpellId spellId, int? level) {
            Spell baseSpell = AllSpells.TemplateSpells.GetValueOrDefault(spellId);

            if (level == null || level < baseSpell.MinimumSpellLevel) {
                level = baseSpell.MinimumSpellLevel;
            }

            ItemModification mod = new ItemModification(ItemModificationKind.CustomPermanent) {
                Tag = baseSpell.Duplicate(null, (int)level, true)
            };

            List<Trait> traits = new List<Trait> { ModTraits.Wand, Trait.Magical, Trait.Unarmed, Trait.Melee, Trait.SpecificMagicWeapon, ModTraits.Roguelike };

            foreach (Trait trait in baseSpell.Traits) {
                if (new Trait[] { Trait.Divine, Trait.Occult, Trait.Arcane, Trait.Primal, Trait.Elemental }.Contains(trait)) {
                    traits.Add(trait);
                }
            }

            string name = level == baseSpell.MinimumSpellLevel ? $"wand of {baseSpell.Name.ToLower()}" : $"wand of level {level} {baseSpell.Name.ToLower()}";
            string desc = "Allows the wielder to cast {i}" + AllSpells.CreateModernSpellTemplate(spellId, Trait.None).ToSpellLink() + "{/i} once per day.";
            Illustration illustration = new WandIllustration(baseSpell.Illustration, Illustrations.Wand);

            /// Func<Item> factory = () => wand;

            //Items.ShopItems.Add(wand);

            return ModManager.RegisterNewItemIntoTheShop($"WandOf{baseSpell.Name}Lv{level}", itemName => {
                return new Item(itemName, illustration, name, (int)level * 2 - 1, GetWandPrice((int)level * 2 - 1), traits.ToArray())
               .WithDescription(desc)
               .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning))
               .WithModification(mod);
            });
        }

        private static int GetWandPrice(int level) {
            //switch (level) {
            //    case 1:
            //        return 20;
            //    case 2:
            //        return 30;
            //    case 3:
            //        return 60;
            //    case 4:
            //        return 100;
            //    case 5:
            //        return 160;
            //    case 6:
            //        return 200;
            //    case 7:
            //        return 360;
            //    case 8:
            //        return 500;
            //    case 9:
            //        return 700;
            //    default:
            //        return 0;
            //}
            switch (level) {
                case 1:
                    return 15;
                case 3:
                    return 45;
                case 5:
                    return 110;
                case 7:
                    return 260;
                case 9:
                    return 560;
                default:
                    return 0;
            }
        }
    }
}