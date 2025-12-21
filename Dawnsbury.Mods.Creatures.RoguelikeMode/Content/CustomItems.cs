using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.Movement;
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
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Monsters.L1;
using Dawnsbury.Core.StatBlocks.Monsters.L4;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using FMOD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading;
using static Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.BarbarianFeatsDb.AnimalInstinctFeat;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using static HarmonyLib.Code;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class CustomItems {

        public static ItemGreaterGroup ItemGroupWands { get; } = ModManager.RegisterEnumMember<ItemGreaterGroup>("Wands");
        // public static ItemGreaterGroup ItemGroupRoguelikeMagicWeapons { get; } = ModManager.RegisterEnumMember<ItemGreaterGroup>("Roguelike magic weapons");
        public static ItemGreaterGroup ItemGroupRoguelikeMagicArmour { get; } = ModManager.RegisterEnumMember<ItemGreaterGroup>("Roguelike magic armour");
        public static ItemGreaterGroup ItemGroupRoguelikeMagicItem { get; } = ModManager.RegisterEnumMember<ItemGreaterGroup>("Roguelike magic items");

        public static ItemName FightingFan { get; } = ModManager.RegisterNewItemIntoTheShop("RL_FightingFan", itemName => new Item(itemName, Illustrations.FightingFan, "fighting fan", 0, 2,
            Trait.Uncommon, Trait.Agile, Trait.Backstabber, Trait.DeadlyD6, Trait.Finesse, Trait.Knife, Trait.Martial, Trait.MonkWeapon, ModTraits.Roguelike)
        .WithMainTrait(ModTraits.FightingFan)
        .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Slashing)));

        public static ItemName Javelin { get; } = ModManager.RegisterNewItemIntoTheShop("RL_Javelin", itemName => new Item(itemName, Illustrations.Javelin, "javelin", 0, 2,
            Trait.Thrown30Feet, ModTraits.ThrownOnly, Trait.Knife, Trait.Simple, ModTraits.Roguelike)
        .WithMainTrait(ModTraits.Javelin)
        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)));

        public static ItemName Kusarigama { get; } = ModManager.RegisterNewItemIntoTheShop("RL_Kusarigama", itemName => new Item(itemName, Illustrations.Kusarigama, "kusarigama", 0, 2,
            Trait.Uncommon, Trait.Disarm, Trait.Reach, Trait.Trip, Trait.VersatileB, Trait.TwoHanded, Trait.Knife, Trait.Martial, Trait.MonkWeapon, ModTraits.Roguelike)
        .WithMainTrait(ModTraits.Kusarigama)
        .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing)));

        public static ItemName Sai { get; } = ModManager.RegisterNewItemIntoTheShop("RL_Sai", itemName => new Item(itemName, Illustrations.Sai, "sai", 0, 2,
            Trait.Uncommon, Trait.Agile, Trait.Disarm, Trait.Finesse, Trait.VersatileB, Trait.Knife, Trait.Martial, Trait.MonkWeapon, ModTraits.Roguelike)
        .WithMainTrait(ModTraits.Sai)
        .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Piercing)));

        public static ItemName Nunchaku { get; } = ModManager.RegisterNewItemIntoTheShop("RL_Nunchaku", itemName => new Item(itemName, Illustrations.Nunchuck, "nunchaku", 0, 0,
            Trait.Uncommon, Trait.Backswing, Trait.Disarm, Trait.Finesse, Trait.Club, Trait.Martial, Trait.MonkWeapon, ModTraits.Roguelike)
        .WithMainTrait(ModTraits.Nunchaku)
        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Bludgeoning)));

        public static ItemName Kama { get; } = ModManager.RegisterNewItemIntoTheShop("RL_Kama", itemName => new Item(itemName, Illustrations.Kama, "kama", 0, 1,
            Trait.Uncommon, Trait.Agile, Trait.Trip, Trait.Knife, Trait.Martial, Trait.MonkWeapon, ModTraits.Roguelike)
        .WithMainTrait(ModTraits.Kama)
        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing)));

        public static ItemName HookSword { get; } = ModManager.RegisterNewItemIntoTheShop("RL_HookSword", itemName => {
            Item item = new Item(itemName, Illustrations.HookSword, "hook sword", 0, 2,
            ModTraits.Parry, ModTraits.Twin, Trait.Disarm, Trait.Trip, Trait.Uncommon, Trait.Sword, Trait.Advanced, Trait.MonkWeapon, ModTraits.Roguelike)
            .WithMainTrait(ModTraits.HookSword)
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing));

            item.ProvidesItemAction = (wielder, wpn) => {
                if (wielder.QEffects.Any(qf => qf.Id == QEffectIds.Parry && qf.Tag == wpn) || wielder.Proficiencies.Get(wpn.Traits) < Proficiency.Trained) return null!;

                return (ActionPossibility)new CombatAction(wielder, new SideBySideIllustration(Illustrations.Parry, wpn.Illustration), $"Parry ({item.Name})", [], "You raise your weapon to parry oncoming attacks, granting yourself a +1 circumstance bonus to AC.", Target.Self())
                .WithDescription("You position your weapon defensively.",
                    "{b}Requirements{/b} You are wielding this weapon, and your proficiency with it is trained or better.\n\nYou gain a +1 circumstance bonus to AC until the start of your next turn.")
                .WithSoundEffect(SfxName.RaiseShield)
                .WithActionCost(1)
                .WithItem(wpn)
                .WithEffectOnSelf(you => {
                    you.AddQEffect(new QEffect("Parrying with " + item.Name, "You have a +1 circumstance bonus to AC.", ExpirationCondition.ExpiresAtStartOfYourTurn, you, Illustrations.Parry) {
                        Id = QEffectIds.Parry,
                        BonusToDefenses = delegate (QEffect parrying, CombatAction? bonk, Defense defense) {
                            if (defense == Defense.AC) {
                                return new Bonus(1, BonusType.Circumstance, "Parry");
                            } else return null;
                        },
                        Tag = item,
                        StateCheck = (qf) => {
                            if (!qf.Owner.HeldItems.Contains(item)) {
                                qf.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        }
                    });
                    you.AddQEffect(new QEffect() {
                        BonusToDefenses = (self, action, def) => {
                            if (def == Defense.AC && self.Owner.QEffects.Any(qf => qf.Name?.ToLower() == "twin parry") && self.Owner.HeldItems.Count == 2
                            && self.Owner.HeldItems[0].Traits.Any(trait => trait.HumanizeLowerCase2() == "parry")
                            && self.Owner.HeldItems[1].Traits.Any(trait => trait.HumanizeLowerCase2() == "parry")) {
                                return new Bonus(2, BonusType.Circumstance, "Twin parry");
                            } else return null;
                        },
                    });
                });
            };

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect() {
                    ExpiresAt = ExpirationCondition.Ephemeral,
                    BonusToDamage = (self, action, defender) => {
                        if (action == null || !action.HasTrait(ModTraits.Twin) || action.Item != item) return null;
                        if (self.Owner.Actions.ActionHistoryThisTurn.Any(a => a.HasTrait(Trait.Strike) && a.Item?.ItemName == action.Item.ItemName && a.Item != action.Item))
                            return new Bonus(action.Item.WeaponProperties?.DamageDieCount ?? 0, BonusType.Circumstance, "Twin");
                        return null;
                    }
                });
            };
            return item;
        });

        public static ItemName DuelingSpear { get; } = ModManager.RegisterNewItemIntoTheShop("DuelingSpear", itemName => new Item(itemName, Illustrations.DuelingSpear, "dueling spear", 0, 2,
            Trait.Disarm, Trait.Finesse, Trait.Uncommon, Trait.VersatileS, Trait.TwoHanded, Trait.Spear, Trait.Martial, ModTraits.Roguelike)
        .WithMainTrait(ModTraits.DuelingSpear)
        .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Piercing)));

        public static ItemName Hatchet { get; } = ModManager.RegisterNewItemIntoTheShop("RL_Hatchet", itemName => new Item(itemName, Illustrations.Hatchet, "hatchet", 0, 0,
            Trait.Agile, Trait.Sweep, Trait.Thrown20Feet, Trait.Martial, Trait.Axe, ModTraits.Roguelike)
        .WithMainTrait(ModTraits.Hatchet)
        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing)));

        public static ItemName LightHammer { get; } = ModManager.RegisterNewItemIntoTheShop("RL_Light hammer", itemName => new Item(itemName, Illustrations.LightHammer, "light hammer", 0, 0,
            Trait.Agile, Trait.Thrown20Feet, Trait.Martial, Trait.Hammer, ModTraits.Roguelike)
        .WithMainTrait(ModTraits.LightHammer)
        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Bludgeoning)));

        public static ItemName Shuriken { get; } = ModManager.RegisterNewItemIntoTheShop("RL_Shuriken", itemName => new Item(itemName, Illustrations.Shuriken, "shuriken", 0, 0,
            Trait.Agile, Trait.Thrown20Feet, ModTraits.ThrownOnly, Trait.Martial, Trait.Knife, ModTraits.Reload0, Trait.MonkWeapon, ModTraits.Roguelike)
        .WithMainTrait(ModTraits.Shuriken)
        .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Slashing)));

        public static ItemName ScourgeOfFangs { get; } = ModManager.RegisterNewItemIntoTheShop("ScourgeOfFangs", itemName => {
            Item item = new Item(itemName, IllustrationName.Whip, "scourge of fangs", 3, 60,
                new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.Finesse, Trait.Reach, Trait.Flail, Trait.Trip, Trait.Simple, Trait.Disarm, Trait.VersatileP, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithMainTrait(Trait.Whip)
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
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

        public static ItemName GreaterScourgeOfFangs { get; } = ModManager.RegisterNewItemIntoTheShop("GreaterScourgeOfFangs", itemName => {
            Item item = new Item(itemName, IllustrationName.Whip, "greater scourge of fangs", 7, 360,
                new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.Finesse, Trait.Reach, Trait.Flail, Trait.Trip, Trait.Simple, Trait.Disarm, Trait.VersatileP, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithMainTrait(Trait.Whip)
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
            .WithWeaponProperties(new WeaponProperties("2d4", DamageKind.Slashing) {
                ItemBonus = 1,
            }
            .WithAdditionalDamage("2d6", DamageKind.Mental))
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

        public static ItemName AlicornPike { get; } = ModManager.RegisterNewItemIntoTheShop("AlicornPike", itemName => {
            var item = new Item(itemName, Illustrations.AlicornPike, "alicorn pike", 4, 30,
                Trait.Magical, Trait.GhostTouch, Trait.Reach, Trait.TwoHanded, Trait.Polearm, Trait.Martial, Trait.DoNotAddToCampaignShop, Trait.Forceful, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike)
            .WithDescription("{i}An illustrious pike, forged from the horn of a unicorn and infused with their goodly healing powers.{/i}\n\nWhilst wielding this pike, you gain Regeneration 4.")
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
            .WithWeaponProperties(new WeaponProperties("1d10", DamageKind.Piercing)
                .WithAdditionalDamage("1d4", DamageKind.Good)
            );

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect("Regeneration", "You heal for 4 at the beginning of each turn, while holding the Alicorn Pike", ExpirationCondition.Ephemeral, wielder, IllustrationName.PositiveAttunement) {
                    Value = 4,
                    StartOfYourPrimaryTurn = async (self, owner) => {
                        await owner.HealAsync("4", CombatAction.CreateSimple(owner, "Regeneration"));
                    },
                    ProvideActionIntoPossibilitySection = (self, section) => {
                        if (section.PossibilitySectionId != PossibilitySectionId.ItemActions)
                            return null;

                        return (ActionPossibility)new CombatAction(self.Owner, new SideBySideIllustration(IllustrationName.Walk, Illustrations.AlicornPike), "Powerful Charge", new Trait[] { Trait.Move },
                            "Stride up to twice your speed in a direct line, then strike. If you moved at least 20-feet, the strike deals +1d6 damage." +
                            "\n\nThis movement will not path around hazards or attacks of opportunity.",
                            Target.Self((user, ai) => {
                                if (!user.Battle.AllCreatures.Any(cr => cr.EnemyOf(user) && cr.Threatens(user.Occupies)) && user.Battle.AllCreatures.Any(cr => cr.EnemyOf(user) && !cr.DetectionStatus.IsUndetectedTo(user) && user.HasLineOfEffectTo(cr.Occupies) <= CoverKind.Lesser && user.DistanceTo(cr) <= user.Speed * (user.HasEffect(QEffectId.AquaticCombat) ? 0.75f : 1.5f) && user.DistanceTo(cr) > 4)) {
                                    return 15f;
                                }
                                return 0f;
                            })) {
                            ShortDescription = "Stride up to twice your speed, then strike. If you travelled at least 20-feet and only in a straight line, the strike deals +1d6 damage."
                        }
                        .WithActionCost(2)
                        .WithSoundEffect(SfxName.Footsteps)
                        .WithEffectOnSelf(async (action, self) => {
                            self.AddQEffect(new QEffect() {
                                Key = "Powerful Charge",
                                AdditionalGoodness = (self, action, d) => d.OwningFaction.EnemyFactionOf(self.Owner.OwningFaction) ? 100f : 0f
                            });

                            MovementStyle movementStyle = new MovementStyle() {
                                MaximumSquares = self.Speed * 2,
                                ShortestPath = false,
                                PermitsStep = false,
                                IgnoresUnevenTerrain = false,
                            };

                            Tile startingTile = self.Occupies;
                            Tile? destTile = await UtilityFunctions.GetChargeTiles(self, movementStyle, 4, "Choose where to Stride with Powerful Charge or right-click to cancel", IllustrationName.Haste);

                            if (destTile == null) {
                                action.RevertRequested = true;
                            } else {
                                movementStyle.Shifting = self.HasEffect(QEffectId.Mobility) && destTile.InIteration.RequiresProvokingAttackOfOpportunity;
                                await self.MoveTo(destTile, action, movementStyle);
                                QEffect? chargeBonus = null;
                                if (self.DistanceTo(startingTile) >= 4) {
                                    self.AddQEffect(chargeBonus = new QEffect("Charge Bonus", "+1d6 damage on your next strike action.") {
                                        AddExtraStrikeDamage = (action, user) => {
                                            return (DiceFormula.FromText("1d6", "Powerful Charge"), DamageKind.Piercing);
                                        },
                                        Illustration = IllustrationName.Horn,
                                    });
                                }

                                self.RemoveAllQEffects(qf => qf.Key == "Powerful Charge");

                                await CommonCombatActions.StrikeAdjacentCreature(self);
                                if (chargeBonus != null) {
                                    chargeBonus.ExpiresAt = ExpirationCondition.Immediately;
                                }
                            }
                        });
                    }
                });
            };

            return item;
        });

        public static ItemName AlicornDagger { get; } = ModManager.RegisterNewItemIntoTheShop("AlicornDagger", itemName => {
            var item = new Item(itemName, Illustrations.AlicornDagger, "alicorn dagger", 4, 30,
                Trait.Magical, Trait.GhostTouch, Trait.Agile, Trait.Finesse, Trait.Thrown10Feet, Trait.VersatileS, Trait.WizardWeapon, Trait.Knife, Trait.Simple, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike)
            .WithMainTrait(Trait.Dagger)
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
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

        public static ItemName ChillingDemise { get; } = ModManager.RegisterNewItemIntoTheShop("ChillingDemise", itemName => {
            var item = new Item(itemName, Illustrations.ChillingDemise, "chilling demise", 7, 360,
                Trait.Magical, Trait.Forceful, Trait.Sweep, Trait.Finesse, Trait.Martial, Trait.DoNotAddToCampaignShop, Trait.SpecificMagicWeapon, ModTraits.Roguelike)
            .WithMainTrait(Trait.Scimitar)
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}A frigid scimitar, imbued with the power of a blue dragon and said to be part of a matching set wielded by a legendary drow renegade.{/i}\n\nChilling Demise deals an extra 1d8 cold damage. On a critical hit, the target is also slowed 1 for 1 round (DC 24 Fortitude save negates). In addition, its wielder gains resistance 10 against fire damage.\n\n" +
            "It has the Finesse trait and gains the Agile traits if wielded alongside its sister blade, Glimmer.")
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing)
                .WithAdditionalDamage("1d8", DamageKind.Cold)
                .WithOnTarget(async (spell, caster, target, result) =>
                {
                    if (result == CheckResult.CriticalSuccess) {
                        spell.Traits.Add(Trait.InflictsSlow);
                        if (CommonSpellEffects.RollSavingThrow(target, spell, Defense.Fortitude, 24) <= CheckResult.Failure) {
                            target.AddQEffect(QEffect.Slowed(1).WithExpirationAtStartOfSourcesTurn(caster, 1));
                        }
                    }
                })
                .WithItemBonus(2)
            );

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.WeaknessAndResistance.AddResistance(DamageKind.Fire, 10);

                wielder.AddQEffect(new QEffect() {
                    AdjustStrikeAction = (self, action) => {
                        if (action.Item?.ItemName == ChillingDemise && self.Owner.HeldItems.Any(item => item.ItemName == Glimmer)) {
                            action.Traits.Add(Trait.Agile);
                        }
                    }
                }.WithExpirationEphemeral());
            };

            return item;
        });

        public static ItemName Glimmer { get; } = ModManager.RegisterNewItemIntoTheShop("RL_Glimmer", itemName => {
            var item = new Item(itemName, Illustrations.Glimmer, "glimmer", 7, 360,
                Trait.Magical, Trait.Forceful, Trait.Sweep, Trait.Finesse, Trait.Martial, Trait.DoNotAddToCampaignShop, Trait.SpecificMagicWeapon, ModTraits.Roguelike)
            .WithMainTrait(Trait.Scimitar)
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}A glimmering scimitar, forged by the elves of old and said to be part of a matching set wielded by a legendary drow renegade.{/i}" +
            "\n\nGlimmer grants its wielder a +2 circumstance bonus to AC, has the Finesse trait and gains the Agile traits if wielded alongside its sister blade, Chilling Demise. " +
            "In addition, if you have previously attacked with Chilling Demise this turn, Glimmer deals 4 extra damage.")
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing) {
                ItemBonus = 2
            });

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect() {
                    BonusToDefenses = (self, action, def) => def == Defense.AC ? new Bonus(2, BonusType.Circumstance, "glimmer") : null,
                    AdjustStrikeAction = (self, action) => {
                        if (action.Item?.ItemName == Glimmer && self.Owner.HeldItems.Any(item => item.ItemName == ChillingDemise)) {
                            action.Traits.Add(Trait.Agile);
                        }
                    },
                    BonusToDamage = (self, action, defender) => {
                        if (action == null || action.Item != item) return null;
                        if (self.Owner.Actions.ActionHistoryThisTurn.Any(a => a.HasTrait(Trait.Strike) && a.Item?.ItemName == ChillingDemise && a.Item != action.Item))
                            return new Bonus(4, BonusType.Circumstance, "Matching set");
                        return null;
                    }
                }.WithExpirationEphemeral());

            };

            return item;
        });

        public static ItemName SpideryHalberd { get; } = ModManager.RegisterNewItemIntoTheShop("SpideryHalberd", itemName => {
            var item = new Item(itemName, Illustrations.SpideryHalberd, "Spidery Halberd", 3, 40,
                Trait.Magical, Trait.Reach, Trait.VersatileS, Trait.Martial, Trait.Polearm, Trait.TwoHanded, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike)
            .WithMainTrait(Trait.Halberd)
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
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
                                            Target.Self((_, ai) => ai.EscapeFrom(caster!))) {
                                            ActionId = ActionId.Escape
                                        };

                                        ActiveRollSpecification activeRollSpecification = (new ActiveRollSpecification[] {
                                            new ActiveRollSpecification(Checks.Attack(Item.Fist()), Checks.FlatDC(caster!.ClassOrSpellDC())),
                                            new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Athletics), Checks.FlatDC(caster.ClassOrSpellDC())),
                                            new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Acrobatics), Checks.FlatDC(caster.ClassOrSpellDC()))
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

        public static ItemName SceptreOfTheSpider { get; } = ModManager.RegisterNewItemIntoTheShop("SceptreOfTheSpider", itemName => {
            Item item = new Item(itemName, Illustrations.SceptreOfTheSpider, "sceptre of the spider", 2, 35,
                new Trait[] { Trait.Magical, Trait.WizardWeapon, Trait.SpecificMagicWeapon, Trait.Agile, Trait.Club, Trait.Simple, Trait.Finesse, Trait.DoNotAddToCampaignShop, ModTraits.CasterWeapon, ModTraits.Roguelike })
            .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning))
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
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
                            weapon.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay) {
                                
                            });
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
                                            Target.Self((_, ai) => ai.EscapeFrom(caster!))) {
                                            ActionId = ActionId.Escape
                                        };

                                        ActiveRollSpecification activeRollSpecification = (new ActiveRollSpecification[] {
                                            new ActiveRollSpecification(Checks.Attack(Item.Fist()), Checks.FlatDC(baseDC + caster!.Level)),
                                            new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Athletics), Checks.FlatDC(baseDC + caster.Level)),
                                            new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Acrobatics), Checks.FlatDC(baseDC + caster.Level))
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

        public static ItemName SceptreOfPandemonium { get; } = ModManager.RegisterNewItemIntoTheShop("SceptreOfPandemonium", itemName => {
            Item item = new Item(itemName, Illustrations.SceptreOfPandemonium, "sceptre of pandemonium", 8, 415,
                new Trait[] { Trait.Magical, Trait.WizardWeapon, Trait.SpecificMagicWeapon, Trait.Agile, Trait.Club, Trait.Simple, Trait.Finesse, Trait.DoNotAddToCampaignShop, ModTraits.CasterWeapon, ModTraits.Roguelike })
            .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning))
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}This gaudy jewled sceptre is legended to have passed int othe treasure troves of many a would be ruler of Our Point of Light... Each undone at the hands of rioting mobs after their kingdoms collapsed in anarchy.{/i}\n\n" +
            $"Once per encounter, the rod can be used to cast {AllSpells.CreateModernSpellTemplate(SpellId.Confusion, Trait.Innate).ToSpellLink()} with a DC of 21 or the wielder's spell save DC if its higher.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect() {
                    ExpiresAt = ExpirationCondition.Ephemeral,
                    ProvideStrikeModifier = (SoP) => {
                        if (SoP != weapon || weapon.ItemModifications.Any(mod => mod.Kind == ItemModificationKind.UsedThisDay)) {
                            return null;
                        }

                        var spell = AllSpells.CreateModernSpellTemplate(SpellId.Confusion, Trait.Innate, 4).CombatActionSpell;
                        spell.WithSpellSavingThrow(null);
                        spell.WithSavingThrow(new SavingThrow(Defense.Will, Math.Max(21, wielder.Spellcasting?.PrimarySpellcastingSource?.GetSpellSaveDC() ?? 0)));
                        spell.Owner = wielder;
                        spell.Item = weapon;
                        spell.WithEffectOnSelf(async (action, user) => {
                            action.Item?.UseUp();
                        });
                        return spell;
                    }
                });
            };
            return item;
        });

        public static ItemName RunestoneOfPandemomium { get; } = ModManager.RegisterNewItemIntoTheShop("RunestoneOfPandemomium", itemName => {
            Item item = new Item(itemName, Illustrations.RuneOfPandemonium, "runestone of {i}pandemonium{/i}", 8, 415,
               [Trait.Runestone, Trait.Enchantment, Trait.Magical, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike])
            .WithItemGreaterGroup(ItemGreaterGroup.PropertyRunes)
            .WithRuneProperties(new RuneProperties("pandemonium", RuneKind.WeaponProperty, "This ever shifting runestone, swirling in maddening ever tessellating patterns.",
            "The weapon deals an extra 1d4 mental damage, and confuses the target until the end of their next turn on a critical hit.", rune => {
                if (rune.WeaponProperties != null) {
                    rune.WeaponProperties.WithAdditionalDamage("1d4", DamageKind.Mental).WithOnTarget(async (spell, caster, target, result) => {
                        if (result == CheckResult.CriticalSuccess) {
                            target.AddQEffect(QEffect.Confused(false, spell).WithExpirationAtEndOfOwnerTurn());
                        }
                    });
                }
            }))
            ;
            return item;
        });

        public static ItemName RunestoneOfMirrors { get; } = ModManager.RegisterNewItemIntoTheShop("RunestoneOfMirrors", itemName => {
            Item item = new Item(itemName, Illustrations.RuneOfMirrors, "runestone of {i}mirrors{/i}", 8, 415,
               [Trait.Runestone, Trait.Illusion, Trait.Magical, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike])
            .WithItemGreaterGroup(ItemGreaterGroup.PropertyRunes)
            .WithRuneProperties(new RuneProperties("mirrored", RuneKind.WeaponProperty, "This small gem shard is polished to a perfect mirror shine that reflect back distorted, grinning dopplegangers of the beholder.",
            "The weapon deals an extra 1d6 slashing damage, and creates a mirror image of the wielder as per the {i}mirror image{/i} spell, on a critical hit.", rune => {
                if (rune.WeaponProperties != null) {
                    rune.WeaponProperties.WithAdditionalDamage("1d6", DamageKind.Mental).WithOnTarget(async (spell, caster, target, result) => {
                        if (result == CheckResult.CriticalSuccess) {
                            var qf = Level2Spells.CreateMirrorImageEffect(caster);
                            qf.Value = 1;
                            caster.AddQEffect(qf);
                        }
                    });
                }
            }))
            ;
            return item;
        });

        public static ItemName CloakOfDuplicity { get; } = ModManager.RegisterNewItemIntoTheShop("RL_CloakOfDuplicity", itemName => {
            Item item = new Item(itemName, Illustrations.CloakOfDuplicity, "cloak of duplicity", 8, 415,
                new Trait[] { Trait.Magical, Trait.Worn, Trait.Illusion, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Cloak)
            .WithItemGroup("Roguelike mode")
            .WithOnCreatureWhenWorn((item, wearer) => {
                wearer.AddQEffect(new QEffect() {
                    StartOfCombat = async self => {
                        self.Owner.AddQEffect(Level2Spells.CreateMirrorImageEffect(self.Owner));
                    }
                });
            })
            .WithDescription("{i}The powerful illusory enchantment laid upon this cloak often causes the beholder to snatch at air as they attempt to locate its true postion.{/i}\n\n" +
            "While wearing this cloak, you benefit from the effects of the {i}mirror image{/i} spell at the start of each combat.");

            return item;
        });

        public static ItemName RunestoneOfOpportunism { get; } = ModManager.RegisterNewItemIntoTheShop("RunestoneOfOpportunism", itemName => {
            Item item = new Item(itemName, Illustrations.RuneOfMirrors, "runestone of {i}opportunism{/i}", 8, 415,
               [Trait.Runestone, Trait.Illusion, Trait.Magical, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike])
            .WithItemGreaterGroup(ItemGreaterGroup.PropertyRunes)
            .WithRuneProperties(new RuneProperties("opportunism", RuneKind.WeaponProperty, "By embedding the teeth of a vanquished chimera into steel, a measure of its ruthless opportunism can be imparted upon the victor's weapon.",
            "All attacks made using this weapon outside of your deal double damage.", rune => {
                if (rune.WeaponProperties != null) {
                    rune.WeaponProperties.WithOnTarget(async (spell, caster, target, result) => {
                        if (spell.HasTrait(Trait.Strike) && caster.Battle.ActiveCreature != caster && result >= CheckResult.Success) {
                            await CommonSpellEffects.DealAttackRollDamage(spell, caster, target, result, spell.TrueDamageFormula ?? DiceFormula.FromText("1d4"), spell.Item?.WeaponProperties?.DamageKind ?? DamageKind.Slashing);
                        }
                    });
                }
            }))
            ;
            return item;
        });

        public static ItemName LunaRunestone { get; } = ModManager.RegisterNewItemIntoTheShop("LunaRunestone", itemName => {
            Item item = new Item(itemName, Illustrations.LunaRunestone, "runestone of {i}luna{/i}", 7, 360,
               [Trait.Runestone, Trait.Evocation, Trait.Fire, Trait.Silver, Trait.Divine, Trait.Magical, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike])
            .WithItemGreaterGroup(ItemGreaterGroup.PropertyRunes)
            .WithRuneProperties(new RuneProperties("luna", RuneKind.WeaponProperty, "A sacred runestone, blessed by a high priestess of The Cerulean Sky.",
            "The weapon counts as being silvered and deals an extra 1d6 fire damage. The weapon can also be used to make the Crecent Moon Strike action." +
            "\n\n{b}Crescent Moon Strike{/b} {icon:TwoActions}\n" +
            "Deal 8d6 fire damage (basic Reflex save) to each enemy creature within a 25ft cone. On a critical failure, targets are dazzled for 1 round. You cannot use this attack again for 1d4 rounds.", weapon => {
                if (weapon.WeaponProperties != null) {
                    weapon.WeaponProperties.WithAdditionalDamage("1d6", DamageKind.Fire);
                    weapon.Traits.Add(Trait.Silver);
                    weapon.StateCheckWhenWielded = (wielder, item) => {
                        wielder.AddQEffect(new QEffect() {
                            ExpiresAt = ExpirationCondition.Ephemeral,
                            ProvideMainAction = self => {
                                return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.Moonbeam, "Crescent Moon Strike", [Trait.Magical, Trait.Divine, Trait.Silver],
                                $"Deal 8d6 fire damage (basic Reflex save) to each enemy creature within a 25ft cone. On a critical failure, targets are dazzled for 1 round. You cannot use this attack again for 1d4 rounds.",
                                Target.Cone(5).WithIncludeOnlyIf((area, cr) => cr.OwningFaction.IsEnemy)) {
                                    ShortDescription = $"Deal 8d6 fire damage (basic Reflex save) to each enemy creature within a 25ft cone. On a critical failure, targets are dazzled for 1 round. You cannot use this attack again for 1d4 rounds."
                                }
                                .WithSavingThrow(new SavingThrow(Defense.Reflex, self.Owner.ClassOrSpellDC()))
                                .WithActionCost(2)
                                .WithProjectileCone(IllustrationName.Moonbeam, 15, ProjectileKind.Cone)
                                .WithSoundEffect(SfxName.DivineLance)
                                .WithEffectOnEachTarget(async (spell, user, defender, result) => {
                                    await CommonSpellEffects.DealBasicDamage(spell, user, defender, result, DiceFormula.FromText(8 + "d6", "Crescent Moon Strike"), DamageKind.Fire);
                                    if (result == CheckResult.CriticalFailure) {
                                        defender.AddQEffect(QEffect.Dazzled().WithExpirationAtStartOfSourcesTurn(user, 1));
                                    }
                                })
                                .WithEffectOnSelf(user => {
                                    user.AddQEffect(QEffect.Recharging("Crescent Moon Strike"));
                                })
                                .WithGoodnessAgainstEnemy((cone, a, d) => {
                                    return 3.5f * 8 + (d.QEffects.FirstOrDefault(qf => qf.Name == "Dazzled" || qf.Id == QEffectId.Blinded) == null ? 2f : 0f);
                                })
                                ;
                            }
                        });
                    };
                }
            }))
            ;
            return item;
        });



        public static ItemName RingOfMonsters { get; } = ModManager.RegisterNewItemIntoTheShop("RL_RingOfMonsters", itemName => {
            Item item = new Item(itemName, Illustrations.RingOfMonsters, "ring of monsters", 6, 200,
                new Trait[] { Trait.Magical, Trait.Worn, Trait.Abjuration, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithOnCreatureWhenWorn((item, wearer) => {
                wearer.AddQEffect(new QEffect() {
                    BonusToDefenses = (self, action, defence) => defence != Defense.AC && action?.Owner != null && action.Owner.Traits.Any(t => (t == Trait.Beast || t == Trait.Animal || t == ModTraits.Monstrous) && t != Trait.Celestial) ? new Bonus(2, BonusType.Item, "Ring of monsters") : null
                });
            })
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}A ring fashioned by a high priestess of the Echidna to aid them in their monster conservation efforts.{/i}\n\n" +
            "While wearing this ring, you gain a +2 item bonus to all saving throws against non-celestial beasts, animals and foes of a monstrous nature.");

            return item;
        });

        public static ItemName StaffOfSpellPenetration { get; } = ModManager.RegisterNewItemIntoTheShop("Staff of Spell Penetration", itemName => {
            Item item = new Item(itemName, Illustrations.StaffOfSpellPenetration, "staff of spell penetration", 2, 40,
                new Trait[] { Trait.Magical, Trait.Club, Trait.Simple, Trait.WizardWeapon, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.CasterWeapon, ModTraits.Roguelike })
            .WithMainTrait(Trait.Staff)
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Bludgeoning))
            .WithDescription("{i}These ancient staves were said to be forged by surface elves during their schism with the Drow, allowing their great wizards to overcome their dark cousin's shadowy tolerance to spellwork.{/i}\n\n" +
            "While wielding this staff, your spells ignore spell resistance and the bonus to saves against mental spells possessed by Drow.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect() {
                    ExpiresAt = ExpirationCondition.Ephemeral,
                    Id = QEffectId.SpellPenetration
                });
            };
            return item;
        });

        //new Item(itemName, illustration, name, (int) level * 2 - 1, GetWandPrice((int) level * 2 - 1), traits.ToArray())
        //   .WithDescription(desc)
        //   .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning))

        public static ItemName ProtectiveAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("ProtectiveAmulet", itemName => {
            Item item = new Item(itemName, Illustrations.ProtectiveAmulet, "protective amulet", 3, 60, new Trait[] { Trait.Magical, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike, Trait.Abjuration })
            .WithItemGroup("Roguelike mode")
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
                                effect.Owner.Overhead("*uses protective amulet*", Color.Black, $"{effect.Owner.Name} holds up their protective amulet to shield {qf.Owner.Name} from harm.");
                                qf.Owner.Overhead($"*{3 + effect.Owner.Level} damage negated*", Color.Black);
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
                                effect.Owner.Overhead("*uses protective amulet*", Color.Black, $"{effect.Owner.Name} holds up their protective amulet to shield {qf.Owner.Name} from harm.");
                                qf.Owner.Overhead($"*{effect.Owner.Level + 1} damage negated*", Color.Black);
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

        public static ItemName GnomishPuzzleBox { get; } = ModManager.RegisterNewItemIntoTheShop("GnomishPuzzleBox", itemName => {
            Item item = new Item(itemName, IllustrationName.PrismaticHexahedron, "gnomish puzzle box", 2, 25, [Trait.Magical, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike, Trait.Transmutation])
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}This intricate cube whirs, twitches and glows with even the slightest touch, ready to defend the one who proved themselves worthy be solving it.{/i}\n\n" +
            "{b}Deploy Puzzle Box {icon:Action}{/b}. The puzzle box deploys in a space within 20 feet, as an immobile construct capable of autonomously shocking nearby enemies, that scales with the user's level.\n\n" +
            "The cube then returns to its owners inventory at the end of each encounter, to be deployed again.");

            item.ProvidesItemAction = (wielder, item) => {
                return (ActionPossibility)new CombatAction(wielder, item.Illustration, "Deploy Puzzle Box", [Trait.Transmutation, Trait.Concentrate, Trait.Manipulate],
                    "{b}Frequency{/b} once per encounter\n{b}Range{/b} 20 feet\n\nDeploy the Gnomish Puzzle Box to an occupied space within range, where it will zap any enemies within a 15 foot range of it until destroyed.", Target.RangedEmptyTileForSummoning(4))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.Throw)
                .WithEffectOnSelf(self => {
                    wielder.HeldItems.Remove(item);
                    self.AddQEffect(new QEffect() {
                        EndOfCombat = async (self, won) => {
                            if (CampaignState.Instance != null) {
                                wielder.Occupies.DroppedItems.Add(item);
                            }
                        }
                    });
                })
                .WithEffectOnEachTile(async (action, user, tiles) => {
                    var baseDC = SkillChallengeTables.GetDCByLevel(user.Level);

                    var zap = new Item(IllustrationName.ElectricArc, "zap", [Trait.Electricity, Trait.Agile, Trait.Ranged, Trait.Unarmed])
                        .WithWeaponProperties(new WeaponProperties((user.Level <= 3 ? 1 : user.Level <= 7 ? 2 : 3) + "d6", DamageKind.Electricity) {
                            Sfx = SfxName.ElectricBlast,
                            VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.ElectricArc)
                        }.WithRangeIncrement(3).WithMaximumRange(3));

                    var sentry = new Creature(IllustrationName.PrismaticHexahedron, "Gnomish Sentry Cube", [Trait.Construct, Trait.Mindless, Trait.MetalArmor, Trait.Conjuration], level: user.Level, perception: baseDC, speed: 0,
                        new Defenses(baseDC - 2, baseDC + 2, baseDC - 4, user.Level),
                        hp: 4 * (user.Level + 1), new Abilities(-3, 3, 5, -5, 3, -5), new Skills())
                    .WithCharacteristics(false, false)
                    .WithProficiency(Trait.Unarmed, Proficiency.Expert)
                    .AddQEffect(QEffect.Flying())
                    .AddQEffect(QEffect.Immobilized())
                    .AddQEffect(QEffect.AllAroundVision())
                    .AddQEffect(QEffect.SmokeVision())
                    .WithUnarmedStrike(zap)
                    ;

                    user.Battle.SpawnCreature(sentry, user.OwningFaction.IsEnemy ? user.Battle.Enemy : user.Battle.GaiaFriends, tiles[0]);
                });
            };
            return item;
        });

        public static ItemName Hexshot { get; } = ModManager.RegisterNewItemIntoTheShop("Hexshot", itemName => {
            Item item = new Item(itemName, Illustrations.Hexshot, "hexshot", 3, 40,
                new Trait[] { Trait.Magical, Trait.VersatileB, Trait.FatalD8, Trait.Reload1, Trait.Firearm, Trait.Simple, Trait.DoNotAddToCampaignShop, Trait.WizardWeapon, Trait.RogueWeapon, ModTraits.CasterWeapon, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike })
            .WithMainTrait(ModTraits.Hexshot)
            .WithItemGreaterGroup(ItemGreaterGroup.RangedMagicWeapons)
            .WithItemGroup("Roguelike mode")
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
                                        return new Bonus(2, BonusType.Status, "Hexshot charged");
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

        public static ItemName SerpentineBow { get; } = ModManager.RegisterNewItemIntoTheShop("Serpentine Bow", itemName => {
            Item item = new Item(itemName, Illustrations.SerpentineBow, "serpentine bow", 3, 25,
                new Trait[] { Trait.Magical, Trait.Propulsive, Trait.OneHandPlus, Trait.DeadlyD10, Trait.Bow, Trait.Martial, Trait.RogueWeapon, Trait.ElvenWeapon, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike })
            .WithMainTrait(Trait.CompositeShortbow)
            .WithItemGreaterGroup(ItemGreaterGroup.RangedMagicWeapons)
            .WithItemGroup("Roguelike mode")
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)
            .WithRangeIncrement(12))
            .WithDescription("{i}This lethal bow is engraved into the shape of a pair of two entwined brass serpents, that seem to lend a portion of their potency venom to each arrow nocked within it.{/i}\n\n...");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(CommonQEffects.SerpentVenomAttack(wielder.ClassOrSpellDC() - wielder.Level, weapon.Name).WithExpirationEphemeral());
            };

            return item;
        });

        public static ItemName MedusaEyeChoker { get; } = ModManager.RegisterNewItemIntoTheShop("MedusaEyeChoker", itemName => {
            Item item = new Item(itemName, Illustrations.MedusaEyeChoker, "medusa eye choker", 7, 360,
                new Trait[] { Trait.Magical, Trait.Worn, Trait.Transmutation, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Necklace)
            .WithItemGroup("Roguelike mode")
            .WithOncePerDayWhenWornAction((choker, wearer) => {
                return new CombatAction(wearer, choker.Illustration, "Petrify", [Trait.Arcane, Trait.Transmutation, Trait.Visual, Trait.Manipulate, Trait.InflictsSlow, Trait.Incapacitation, Trait.Basic],
                    "{b}Frequency{/b} Once per day\n{b}Range{/b} 30 feet\n{b}Target{/b} 1 creature\n{b}Saving throw{/b} Fortitude\n\n" +
                    "You affix the gaze of the Medusa Eye Choker upon an enemy, turning their flesh to stone." +
                    S.FourDegreesOfSuccess("The target is unaffected.", "The target is slowed 1 for 1 round", "The target is slowed 1 for 1 minute.", "The target is permanently petrified."), Target.Ranged(6))
                .WithSoundEffect(SfxName.Stoneskin)
                .WithSavingThrow(new SavingThrow(Defense.Fortitude, wearer.ClassOrSpellDC()))
                .WithEffectOnEachTarget(async (action, user, defender, result) => {
                    if (result == CheckResult.Success) {
                        defender.AddQEffect(QEffect.Slowed(1).WithExpirationAtEndOfOwnerTurn());
                    } else if (result == CheckResult.Failure) {
                        defender.AddQEffect(QEffect.Slowed(1).WithExpirationNever());
                    } else if (result == CheckResult.CriticalFailure) {
                        Basilisk.Petrify(defender);
                        defender.AddQEffect(CommonQEffects.Hazard());
                    }
                })
                ;
            })
            .WithDescription("{i}Forged from a trophy from your encounter with the deadly medusa, that you might turn a portion of her cursed power upon your enemies.{/i}\n\nYou may activate the choker once per day to Petrify {icon:Action} a creature.\n\n" +
            "{b}Frequency{/b} Once per day\n{b}Range{/b} 30 feet\n{b}Target{/b} 1 creature\n{b}Saving throw{/b} Fortitude\n\n" +
            "You affix the gaze of the Medusa Eye Choker upon an enemy, turning their flesh to stone." +
            S.FourDegreesOfSuccess("The target is unaffected.", "The target is slowed 1 for 1 round", "The target is slowed 1 for 1 minute.", "The target is permanently petrified."));
            return item;
        });

        public static ItemName SmokingSword { get; } = ModManager.RegisterNewItemIntoTheShop("Smoking Sword", itemName => {
            Item item = new Item(itemName, new WandIllustration(IllustrationName.ElementFire, IllustrationName.Longsword), "smoking sword", 3, 25,
                new Trait[] { Trait.Magical, Trait.Martial, Trait.Sword, Trait.Fire, Trait.VersatileP, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.BoostedWeapon, ModTraits.Roguelike
            })
            .WithMainTrait(Trait.Longsword)
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing))
            .WithDescription("{i}Smoke constantly belches from this longsword.{/i}\n\nAny hit with this sword deals 1 extra fire damage.\n\n" +
            "You can use a special action while holding the sword to command the blade's edges to light on fire.\n\n{b}Activate {icon:Action}.{/b} concentrate; {b}Effect.{/b} Until the end" +
            " of your turn, the sword deals 1d6 extra fire damage instead of just 1. After you use this action, you can't use it again until the end of the encounter.");
            item.WeaponProperties?.AdditionalDamage.Add(("1", DamageKind.Fire));
            return item;
        });

        public static ItemName StormHammer { get; } = ModManager.RegisterNewItemIntoTheShop("Storm Hammer", itemName => {
            Item item = new Item(itemName, new DualIllustration(IllustrationName.ElementAir, IllustrationName.Warhammer), "storm hammer", 3, 25,
                new Trait[] { Trait.Magical, Trait.Shove, Trait.Martial, Trait.Hammer, Trait.Electricity, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.BoostedWeapon, ModTraits.Roguelike })
            .WithMainTrait(Trait.Warhammer)
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Bludgeoning))
            .WithDescription("{i}Sparks of crackling electricity arc from this warhammer, and the head thrums with distant thunder.{/i}\n\nAny hit with this hammer deals 1 extra electricity damage.\n\n" +
            "You can use a special action while holding the hammer to transform the sparks into lightning bolts.\n\n{b}Activate {icon:Action}.{/b} concentrate; {b}Effect.{/b} Until the end" +
            " of your turn, the hammer deals 1d6 extra electricity damage instead of just 1. After you use this action, you can't use it again until the end of the encounter.");
            item.WeaponProperties?.AdditionalDamage.Add(("1", DamageKind.Electricity));
            return item;
        });

        public static ItemName ChillwindBow { get; } = ModManager.RegisterNewItemIntoTheShop("Chillwind Bow", itemName => {
            Item item = new Item(itemName, Illustrations.ChillwindBow, "chillwind bow", 3, 25,
                new Trait[] { Trait.Magical, Trait.OneHandPlus, Trait.DeadlyD10, Trait.Bow, Trait.Martial, Trait.RogueWeapon, Trait.ElvenWeapon, Trait.Cold, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.BoostedWeapon, ModTraits.Roguelike })
            .WithMainTrait(Trait.Shortbow)
            .WithItemGreaterGroup(ItemGreaterGroup.RangedMagicWeapons)
            .WithItemGroup("Roguelike mode")
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)
            .WithRangeIncrement(12))
            .WithDescription("{i}The yew of this bow is cold to the touch, and its arrows pool with fog as they're nocked.{/i}\n\nAny hit with this bow deals 1 extra cold damage.\n\n" +
            "You can use a special action while holding the bow to coat the bow in fridgid icy scales.\n\n{b}Activate {icon:Action}.{/b} concentrate; {b}Effect.{/b} Until the end" +
            " of your turn, the bow deals 1d6 extra cold damage instead of just 1. After you use this action, you can't use it again until the end of the encounter.");
            item.WeaponProperties?.AdditionalDamage.Add(("1", DamageKind.Cold));
            return item;
        });

        public static ItemName Sparkcaster { get; } = ModManager.RegisterNewItemIntoTheShop("Sparkcaster", itemName => {
            Item item = new Item(itemName, new DualIllustration(IllustrationName.ElementAir, IllustrationName.HeavyCrossbow), "sparkcaster", 3, 25,
                new Trait[] { Trait.Magical, Trait.Reload2, Trait.Simple, Trait.Bow, Trait.TwoHanded, Trait.Crossbow, Trait.WizardWeapon, Trait.Electricity, Trait.DoNotAddToCampaignShop, ModTraits.CasterWeapon, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike })
            .WithMainTrait(Trait.HeavyCrossbow)
            .WithItemGreaterGroup(ItemGreaterGroup.RangedMagicWeapons)
            .WithItemGroup("Roguelike mode")
            .WithWeaponProperties(new WeaponProperties("1d10", DamageKind.Piercing)
            .WithRangeIncrement(24))
            .WithDescription("{i}Sparks of crackling electricity arc from this crossbow.{/i}\n\nAny hit with this crossbow deals 1 extra electricity damage.\n\n" +
            "You can use a special action while holding the crossbow to fire a crackling bolt of lightning in a great arc.\n\n{b}Activate {icon:Action}.{/b} concentrate, manipulate; {b}Effect.{/b} Each creature in a 30-foot line " +
            "suffers 2d6 electricity damage, mitigated by a basic Reflex save. After you use this action, you can't use it again until the end of the encounter.");
            item.WeaponProperties?.AdditionalDamage.Add(("1", DamageKind.Electricity));

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
            .WithItemGreaterGroup(ItemGreaterGroup.RangedMagicWeapons)
            .WithItemGroup("Roguelike mode")
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)
            .WithRangeIncrement(24))
            .WithDescription("{i}This viper shaped handcrossbow hisses and sizzles against the air.{/i}\n\nAny hit with this crossbow deals 1 extra acid damage.\n\n" +
            "You can use a special action while holding the crossbow to cause it to launch great splattering globlets of acid.\n\n{b}Activate {icon:Action}.{/b} concentrate, manipulate; {b}Effect.{/b} The weapon gains 1d6 acid splash damage until the end of your turn.\n\n" +
            "After you use this action, you can't use it again until the end of the encounter.");
            item.WeaponProperties?.AdditionalDamage.Add(("1", DamageKind.Acid));

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                    ProvideStrikeModifierAsPossibility = item => {
                        if (item != weapon || item.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay) != null || wielder.PersistentCharacterSheet == null || weapon.EphemeralItemProperties.NeedsReload) {
                            return null;
                        }

                        CombatAction action = new CombatAction(wielder, weapon.Illustration, $"Activate {weapon.Name.CapitalizeEachWord()}", new Trait[] { Trait.Concentrate, Trait.Manipulate, Trait.Acid, Trait.Evocation },
                            "{b}Frequency{/b} once per encounter\nUntil the end of your turn, attacks made with the Viper's Spit gain +1d6 splash damage.", Target.Self()) {
                            ShortDescription = $"{weapon.Name.CapitalizeEachWord()} gains an 1d6 acid splash damage until the end of your turn."
                        }
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.AcidSplash)
                        .WithEffectOnSelf(user => {
                            // Effect
                            weapon.WeaponProperties!.AdditionalSplashDamageFormula = "1d6";
                            weapon.WeaponProperties.AdditionalSplashDamageKind = DamageKind.Acid;
                            weapon.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));

                            // Show effect
                            user.AddQEffect(new QEffect($"{weapon.Name.CapitalizeEachWord()} Activated", $"{weapon.Name.CapitalizeEachWord()} deals an extra 1d6 acid splash damage.") {
                                ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
                                Tag = weapon,
                                WhenExpires = (self) => {
                                    Item weapon = self.Tag as Item;
                                    weapon!.WeaponProperties!.AdditionalSplashDamageFormula = null;
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
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
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
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
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
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
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
                            if (target == null) return;
                            if (!target.IsTrulyGenuinelyFreeTo(caster)) {
                                target = target.GetShuntoffTile(caster);
                            }
                            caster.TranslateTo(target);
                            caster.AnimationData.ColorBlink(Color.LightGoldenrodYellow);
                            caster.Battle.SmartCenterCreatureAlways(caster);
                            spell.Item?.WithModification(new ItemModification(ItemModificationKind.UsedThisDay));
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
            .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
            .WithItemGroup("Roguelike mode")
            .WithWeaponProperties(new WeaponProperties("1d12", DamageKind.Slashing))
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
            .WithItemGroup("Roguelike mode")
            .WithArmorProperties(new ArmorProperties(2, 3, -1, 0, 12))
            .WithDescription("{i}Flexible jetblack armour of dark elven make, sewn from some exotic underdark silk.{/i}\n\nThe wearer of this armour may pass through webs unhindered.");
        });

        public static ItemName KrakenMail { get; } = ModManager.RegisterNewItemIntoTheShop("Kraken Mail", itemName => {
            return new Item(itemName, Illustrations.KrakenMail, "kraken mail", 2, 35,
                new Trait[] { Trait.Magical, Trait.MediumArmor, Trait.Leather, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithArmorProperties(new ArmorProperties(4, 1, -2, 1, 16))
            .WithDescription("{i}The eyes on this briny chitin armour blink periodically.{/i}\n\nThe wearer of this armour gains a swim speed, and a +2 status bonus to grapple rolls while submerged in water.");
        });

        public static ItemName InquisitrixLeathers { get; } = ModManager.RegisterNewItemIntoTheShop("Inquisitrix Leathers", itemName => {
            return new Item(itemName, Illustrations.InquisitrixLeathers, "inquisitrix leathers", 3, 50,
                new Trait[] { Trait.Magical, Trait.LightArmor, Trait.Leather, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithArmorProperties(new ArmorProperties(2, 3, -1, 0, 12))
            .WithDescription("{i}The severe skin tight leather bodysuit of an infamous drow inquisitrix, said to grant a portion of the bafeful power granted to their order by the demon queen of spiders.{/i}\n\nThe wearer of this armour gains a +1 item bonus to intimidation, and the iron command reaction." +
            "\n\n{b}Iron Command.{/b}\n{b}Trigger{/b} An enemy within 15 feet damages you.\n{b}Effect{/b} Your attacker suffers 1d6 + half your level (minimum 1) mental damage (basic Will save mitigates).");
        });

        public static ItemName RobesOfTheWarWizard { get; } = ModManager.RegisterNewItemIntoTheShop("Robes of the War Wizard", itemName => {
            return new Item(itemName, Illustrations.RobesOfTheWarWizard, "robes of the war wizard", 2, 35,
                new Trait[] { Trait.Magical, Trait.UnarmoredDefense, Trait.Armor, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithArmorProperties(new ArmorProperties(0, 5, 0, 0, 0))
            .WithDescription("{i}This bold red robe is enchanted to collect neither sweat, dust nor grime.{/i}\n\nThe wearer of this armour gains a +2 damage bonus per spell level to non-cantrip cone and emanation spells used against creatures within 15-feet of them.\n\nIn addition, they gain resistance 1 to acid, cold, electricity and fire damage.");
        });

        public static ItemName GreaterRobesOfTheWarWizard { get; } = ModManager.RegisterNewItemIntoTheShop("Greater Robes of the War Wizard", itemName => {
            return new Item(itemName, Illustrations.RobesOfTheWarWizard, "greater robes of the war wizard", 4, 90,
                new Trait[] { Trait.Magical, Trait.UnarmoredDefense, Trait.Armor, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithArmorProperties(new ArmorProperties(0, 5, 0, 0, 0))
            .WithDescription("{i}This bold red robe is enchanted to collect neither sweat, dust nor grime.{/i}\n\nThe wearer of this armour gains a +3 damage bonus per spell level to non-cantrip cone and emanation spells used against creatures within 15-feet of them.\n\nIn addition, they gain resistance 2 to acid, cold, electricity and fire damage.");
        });

        public static ItemName WhisperMail { get; } = ModManager.RegisterNewItemIntoTheShop("Whisper Mail", itemName => {
            return new Item(itemName, Illustrations.WhisperMail, "whisper mail", 2, 35,
                new Trait[] { Trait.Magical, Trait.MediumArmor, Trait.Chain, Trait.MetalArmor, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithArmorProperties(new ArmorProperties(4, 1, -2, 1, 16))
            .WithDescription("{i}This ominous armour set whispers incessantly into the wearer's ears... Revealing profound and disturbing secrets.{/i}\n\nThe wearer of this armour imposes weakness 1 to all physical damage against adjacent enemies, and a +1 item bonus to Seek checks.");
        });

        public static ItemName DreadPlate { get; } = ModManager.RegisterNewItemIntoTheShop("Dread Plate", itemName => {
            return new Item(itemName, Illustrations.DreadPlate, "dread plate", 3, 70,
                new Trait[] { Trait.Magical, Trait.HeavyArmor, Trait.Bulwark, Trait.Plate, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithArmorProperties(new ArmorProperties(6, 0, -3, -2, 18))
            .WithDescription("{i}This cold, black steel suit of plate armour radiates a spiteful presence, feeding off its wearer's own lifeforce to strike at any who would dare mar it.{/i}\n\nWhile wearing this cursed armour, you take an additional 1d4 negative damage when damaged by an adjacent creature, but deal 1d6 negative damage in return.");
        });

        public static ItemName SpellbanePlate { get; } = ModManager.RegisterNewItemIntoTheShop("Spellbane Plate", itemName => {
            return new Item(itemName, Illustrations.SpellbanePlate, "spellbane plate", 3, 70,
                new Trait[] { Trait.Magical, Trait.HeavyArmor, Trait.Bulwark, Trait.Plate, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithArmorProperties(new ArmorProperties(6, 0, -3, -2, 18))
            .WithDescription("{i}This suit of armour is forged from a strange, mercurial alloy - perhaps a lost relic forged by dwarven smithes during the height of the starborn invasion.{/i}" +
            "\n\nWhile wearing this suit of armour, you gain a +1 item bonus to all spell saving throws, but cannot cast spells of your own.");
        });

        public static ItemName DolmanOfVanishing { get; } = ModManager.RegisterNewItemIntoTheShop("Dolman of Vanishing", itemName => {
            return new Item(itemName, Illustrations.DolmanOfVanishing, "dolman of vanishing", 3, 60,
                new Trait[] { Trait.Magical, Trait.Armor, Trait.UnarmoredDefense, Trait.Cloth, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithArmorProperties(new ArmorProperties(0, 5, 0, 0, 0))
            .WithDescription("{i}A skyblue robe of gossmer, that seems to evade the beholder's full attention span, no matter how hard they try to focus on it.{/i}\n\nThe wearer of this cloak gains a +2 item bonus to stealth and can hide in plain sight.");
        });

        public static ItemName MaskOfConsumption { get; } = ModManager.RegisterNewItemIntoTheShop("Mask of Consumption", itemName => {
            return new Item(itemName, Illustrations.MaskOfConsumption, "mask of consumption", 2, 30,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithWornAt(Trait.Mask)
            .WithDescription("{i}This cursed mask fills the wearer with a ghoulish, ravenous hunger for living flesh, alongside a terrible wasting pallor.{/i}\n\n" +
            "The wearer of this mask has their Max HP halved. However, in return they gain a 1d10 unarmed slashing attack with the agile and finesse properties. " +
            "Creatures hit by the attack suffer 1d6 persistent bleeding damage, and if they're blessed of living flesh, the wearer may consume it to heal for an amount of hit points " +
            "equal to the damage dealt.\n\n{b}Note.{/b} To unequip this cursed item, simply place it in the common loot field at the bottom of the character sheets. It won't despawn between encounters.")
            .WithPermanentQEffectWhenWorn((qfMoC, item) => {
                qfMoC.Innate = true;
                qfMoC.Id = QEffectId.Drained;
                qfMoC.Name = "Mask of Consumption";
                qfMoC.Description = "Your HP is halved. In return, your hungry claws attack deals 1d6 persistent bleed damage and heals you for an amount equal to the damage dealt.";
                qfMoC.StateCheck = self => {
                    self.Owner.DrainedMaxHPDecrease = self.Owner.MaxHP / 2;
                    Item unarmed = qfMoC.Owner.UnarmedStrike;
                    qfMoC.Owner.ReplacementUnarmedStrike = self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.DragonClaws, "hungry claws", "1d10", DamageKind.Slashing, Trait.Agile, Trait.Finesse, Trait.Brawling, Trait.Unarmed)
                    .WithAdditionalWeaponProperties(properties => {
                        properties.AdditionalPersistentDamageFormula = "1d6";
                        properties.AdditionalPersistentDamageKind = DamageKind.Bleed;
                    });
                };
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

            Func<Creature, Creature> companion = (owner) => {
                int level = owner.Level;
                int prof = level + 2;

                return new Creature(new SpiderIllustration(Illustrations.SpiderHatchling, IllustrationName.Bear256), "Spider Hatchling", [Trait.Animal, Trait.AnimalCompanion, Trait.BaseGameAnimalCompanion, Trait.Minion, ModTraits.Spider], level, 1 + prof, 6, new Defenses(10 + 3 + prof, 1 + prof, 3 + prof, 1 + prof), 7 * level,
                    new Abilities(3, 3, 1, -4, 1, 0), new Skills())
                .WithProficiency(Trait.Stealth, Proficiency.Trained)
                .WithUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d6", DamageKind.Piercing))
                .WithAdditionalUnarmedStrike(new Item(Illustrations.StabbingAppendage, "leg", Trait.Unarmed, Trait.Agile).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)))
                .AddQEffect(QEffect.Webwalk())
                .AddQEffect(QEffect.WebSense())
                .AddQEffect(CommonQEffects.WebAttack(10 + prof))
                .AddQEffect(new QEffect() {
                    ProvideMainAction = qfSupport => (ActionPossibility)new CombatAction(qfSupport.Owner, qfSupport.Owner.Illustration,
                    "Support", [], "{i}Your spider drips poison from its stinger when you create an opening.{/i}\n\nUntil the start of your next turn, if you hit and damage a creature in your spider's reach, you also deal 1d6 persistent poison damage.\n\n{b}Special{/b} If the animal uses the Support action, the only other actions it can use on this turn are basic move actions; if it has already used any other action this turn, it can't Support you.",
                    Target.Self()
                    .WithAdditionalRestriction(qfSupport => !qfSupport.Actions.ActionHistoryThisTurn.Any(act => !act.HasTrait(Trait.Move)) ? null : "You already took a non-move action this turn.")) {
                        ShortDescription = "Until the start of your next turn, if you hit and damage a creature in your spider's reach, you also deal 1d6 persistent poison damage."
                    }.WithEffectOnSelf(caster => {
                        QEffect qEffect = new QEffect("Support", "Until the start of your next turn, if you hit and damage a creature in your spider's reach, you also deal 1d6 persistent poison damage.", ExpirationCondition.ExpiresAtStartOfSourcesTurn, owner, qfSupport.Owner.Illustration) {
                            DoNotShowUpOverhead = true,
                            PreventTakingAction = ca => !ca.HasTrait(Trait.Move) && ca.ActionId != ActionId.EndTurn ? "You used Support so you can't take non-move actions anymore this turn." : null
                        };
                        owner.AddQEffect(new QEffect(ExpirationCondition.ExpiresAtEndOfYourTurn) {
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
                });
            };

            return MakeAnimalCompanionItem(new Item(itemName, new SpiderIllustration(Illustrations.SpiderHatchling, IllustrationName.Bear256), "spider hatchling", 3, 45, [Trait.Magical, Trait.Invested, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike]),
                companion, "Spider Hatchling", "A small baby hunting spider, in search of a new master to love and cherish it.");
        });

        public static ItemName SacredSerpent { get; } = ModManager.RegisterNewItemIntoTheShop("Sacred Serpent", itemName => {

            Func<Creature, Creature> companion = (owner) => {
                int level = owner.Level;
                int prof = level + 2;

                return new Creature(IllustrationName.VenomousSnake256, "Sacred Serpent", [Trait.Animal, Trait.AnimalCompanion, Trait.BaseGameAnimalCompanion, Trait.Minion],
                    level, perception: 1 + prof, speed: 6, new Defenses(10 + 3 + prof, 1 + prof, 3 + prof, 1 + prof), hp: 6 * level,
                    new Abilities(3, 3, 1, -4, 1, 0), new Skills())
                .WithProficiency(Trait.Stealth, Proficiency.Trained)
                .WithUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d8", DamageKind.Piercing, Trait.Finesse))
                .AddQEffect(QEffect.Flying())
                .AddQEffect(new QEffect() {
                    ProvideMainAction = qfSupport => (ActionPossibility)new CombatAction(qfSupport.Owner, qfSupport.Owner.Illustration,
                    "Support", [], "{i}Your sacred serpent holds your enemies with its coils, interfering with their reactions.{/i}\n\nUntil the start of your next turn, any creature of equal or lower level your sacred serpent threatens can’t use their reaction.\n\n{b}Special{/b} If the animal uses the Support action, the only other actions it can use on this turn are basic move actions; if it has already used any other action this turn, it can't Support you.",
                    Target.Self()
                    .WithAdditionalRestriction(qfSupport => !qfSupport.Actions.ActionHistoryThisTurn.Any(act => !act.HasTrait(Trait.Move)) ? null : "You already took a non-move action this turn.")) {
                        ShortDescription = $"Until the start of your next turn, adjacent creatures of your level or lower cannot use their reaction."
                    }.WithEffectOnSelf(caster => {
                        QEffect qEffect = new QEffect("Support", "Adjacent creatures of your level or lower cannot use their reaction.", ExpirationCondition.ExpiresAtStartOfSourcesTurn, owner, qfSupport.Owner.Illustration) {
                            DoNotShowUpOverhead = true,
                            PreventTakingAction = ca => !ca.HasTrait(Trait.Move) && ca.ActionId != ActionId.EndTurn ? "You used Support so you can't take non-move actions anymore this turn." : null
                        };

                        qEffect.AddGrantingOfTechnical(cr => cr.EnemyOf(caster) && caster.DistanceTo(cr.Occupies) <= (caster.UnarmedStrike.HasTrait(Trait.Reach) ? 2 : 1) && cr.Level <= caster.Level, qfTech => {
                            qfTech.Id = QEffectId.CannotTakeReactions;
                            qfTech.Illustration = caster.Illustration;
                            qfTech.Name = "Binding Coils";
                            qfTech.Description = $"You cannot use reactions again {owner.Name}.";
                            qfTech.Source = caster;
                            qfTech.Key = "RL_Coiled";
                        });
                        caster.AddQEffect(qEffect);
                    })
                });
            };

            return MakeAnimalCompanionItem(new Item(itemName, IllustrationName.VenomousSnake256, "sacred serpent", 3, 45, [Trait.Magical, Trait.Invested, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike]),
                companion, "Sacred Serpent", "A whimsical winged danger noodle, gifted to the party from one reptile enthusiaist to another by an Azata.");
        });

        public static ItemName GreaterSacredSerpent { get; } = ModManager.RegisterNewItemIntoTheShop("Mature Sacred Serpent", itemName => {

            Func<Creature, Creature> companion = (owner) => {
                int level = owner.Level;
                int prof = level + 2;

                return new Creature(IllustrationName.VenomousSnake256, "Mature Sacred Serpent", [Trait.Animal, Trait.AnimalCompanion, Trait.BaseGameAnimalCompanion, Trait.Minion],
                    level, perception: 1 + prof, speed: 6, new Defenses(10 + 3 + prof, 1 + prof, 3 + prof, 1 + prof), hp: 6 * level,
                    new Abilities(3, 3, 1, -4, 1, 0), new Skills())
                .WithProficiency(Trait.Stealth, Proficiency.Trained)
                .WithUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d8", DamageKind.Piercing, Trait.Finesse))
                .AddQEffect(QEffect.Flying())
                .AddQEffect(new QEffect() {
                    ProvideMainAction = qfSupport => (ActionPossibility)new CombatAction(qfSupport.Owner, qfSupport.Owner.Illustration,
                    "Support", [], "{i}Your sacred serpent holds your enemies with its coils, interfering with their reactions.{/i}\n\nUntil the start of your next turn, any creature of equal or lower level your sacred serpent threatens can’t use their reaction.\n\n{b}Special{/b} If the animal uses the Support action, the only other actions it can use on this turn are basic move actions; if it has already used any other action this turn, it can't Support you.",
                    Target.Self()
                    .WithAdditionalRestriction(qfSupport => !qfSupport.Actions.ActionHistoryThisTurn.Any(act => !act.HasTrait(Trait.Move)) ? null : "You already took a non-move action this turn.")) {
                        ShortDescription = $"Until the start of your next turn, adjacent creatures of your level or lower cannot use their reaction."
                    }.WithEffectOnSelf(caster => {
                        QEffect qEffect = new QEffect("Support", "Adjacent creatures of your level or lower cannot use their reaction.", ExpirationCondition.ExpiresAtStartOfSourcesTurn, owner, qfSupport.Owner.Illustration) {
                            DoNotShowUpOverhead = true,
                            PreventTakingAction = ca => !ca.HasTrait(Trait.Move) && ca.ActionId != ActionId.EndTurn ? "You used Support so you can't take non-move actions anymore this turn." : null
                        };

                        qEffect.AddGrantingOfTechnical(cr => cr.EnemyOf(caster) && caster.DistanceTo(cr.Occupies) <= (caster.UnarmedStrike.HasTrait(Trait.Reach) ? 2 : 1) && cr.Level <= caster.Level, qfTech => {
                            qfTech.Id = QEffectId.CannotTakeReactions;
                            qfTech.Illustration = caster.Illustration;
                            qfTech.Name = "Binding Coils";
                            qfTech.Description = $"You cannot use reactions again {owner.Name}.";
                            qfTech.Source = caster;
                            qfTech.Key = "RL_Coiled";
                        });
                        caster.AddQEffect(qEffect);
                    })
                });
            };

            return MakeAnimalCompanionItem(new Item(itemName, IllustrationName.VenomousSnake256, "mature sacred serpent", 7, 300, [Trait.Magical, Trait.Invested, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike]),
                companion, "Mature Sacred Serpent", "A whimsical winged danger noodle, gifted to the party from one reptile enthusiaist to another by an Azata.", true);
        });

        public static ItemName CompanionBunny { get; } = ModManager.RegisterNewItemIntoTheShop("Companion Bunny", itemName => {

            Func<Creature, Creature> companion = (owner) => {
                int level = owner.Level;
                int prof = level + 2;

                return new Creature(Illustrations.CompanionBunny, "Companion Bunny", [Trait.Animal, Trait.AnimalCompanion, Trait.BaseGameAnimalCompanion, Trait.Minion],
                    level, perception: 1 + prof, speed: 6, new Defenses(10 + 3 + prof, 1 + prof, 3 + prof, 1 + prof), hp: 6 * level,
                    new Abilities(3, 3, 1, -4, 1, 0), new Skills())
                .WithProficiency(Trait.Stealth, Proficiency.Trained)
                .WithUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d8", DamageKind.Piercing, Trait.Finesse))
                .WithAdditionalUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Slam, "claw", "1d6", DamageKind.Slashing, Trait.Finesse, Trait.Agile))
                .AddQEffect(new QEffect() {
                    ProvideMainAction = qfSupport => (ActionPossibility)new CombatAction(qfSupport.Owner, qfSupport.Owner.Illustration,
                    "Support", [], "{i}Your companion bunny pricks its ears, preparing to adorably thump the ground in warning at the first sign of attack.{/i}\n\nUntil the start of your next turn, all creatures within 10 feet of your companion bunny suffer a -1 circumstance penalty to attack rolls against you.\n\n{b}Special{/b} If the animal uses the Support action, the only other actions it can use on this turn are basic move actions; if it has already used any other action this turn, it can't Support you.",
                    Target.Self()
                    .WithAdditionalRestriction(qfSupport => !qfSupport.Actions.ActionHistoryThisTurn.Any(act => !act.HasTrait(Trait.Move)) ? null : "You already took a non-move action this turn.")) {
                        ShortDescription = $"Until the start of your next turn, all creatures within 10 feet of your companion bunny suffer a -1 circumstance penalty to attack rolls against you."
                    }.WithEffectOnSelf(caster => {
                        QEffect qEffect = new QEffect("Support", $"Enemy creatures within 10 feet suffer a -1 circumstance penalty to attack {owner.Name}", ExpirationCondition.ExpiresAtStartOfSourcesTurn, owner, qfSupport.Owner.Illustration) {
                            DoNotShowUpOverhead = true,
                            PreventTakingAction = ca => !ca.HasTrait(Trait.Move) && ca.ActionId != ActionId.EndTurn ? "You used Support so you can't take non-move actions anymore this turn." : null
                        };

                        qEffect.AddGrantingOfTechnical(cr => cr.EnemyOf(caster) && caster.DistanceTo(cr.Occupies) <= 2, qfTech => {
                            qfTech.Illustration = IllustrationName.TortoiseAndTheHare;
                            qfTech.Name = "Warning Thump";
                            qfTech.Description = $"You suffer a -1 circumstance penalty to attacks rolls against {owner.Name}.";
                            qfTech.Source = caster;
                            qfTech.BonusToAttackRolls = (self, action, target) => target == owner ? new Bonus(-1, BonusType.Circumstance, "Warning thump") : null;
                        });
                        caster.AddQEffect(qEffect);
                    })
                });
            };

            return MakeAnimalCompanionItem(new Item(itemName, Illustrations.CompanionBunny, "companion bunny", 3, 60, [Trait.Magical, Trait.Invested, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike]),
                companion, "Companion Bunny", "The snuggle pouch is the most important part of bun anatomy.");
        });

        public static ItemName GreaterCompanionBunny { get; } = ModManager.RegisterNewItemIntoTheShop("GreaterCompanionBunny", itemName => {

            Func<Creature, Creature> companion = (owner) => {
                int level = owner.Level;
                int prof = level + 2;

                return new Creature(Illustrations.CompanionBunny, "Greater Companion Bunny", [Trait.Animal, Trait.AnimalCompanion, Trait.BaseGameAnimalCompanion, Trait.Minion],
                    level, perception: 1 + prof, speed: 6, new Defenses(10 + 3 + prof, 1 + prof, 3 + prof, 1 + prof), hp: 6 * level,
                    new Abilities(3, 3, 1, -4, 1, 0), new Skills())
                .WithProficiency(Trait.Stealth, Proficiency.Trained)
                .WithUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d8", DamageKind.Piercing, Trait.Finesse))
                .WithAdditionalUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Slam, "claw", "1d6", DamageKind.Slashing, Trait.Finesse, Trait.Agile))
                .AddQEffect(new QEffect() {
                    ProvideMainAction = qfSupport => (ActionPossibility)new CombatAction(qfSupport.Owner, qfSupport.Owner.Illustration,
                    "Support", [], "{i}Your companion bunny pricks its ears, preparing to adorably thump the ground in warning at the first sign of attack.{/i}\n\nUntil the start of your next turn, all creatures within 10 feet of your companion bunny suffer a -1 circumstance penalty to attack rolls against you.\n\n{b}Special{/b} If the animal uses the Support action, the only other actions it can use on this turn are basic move actions; if it has already used any other action this turn, it can't Support you.",
                    Target.Self()
                    .WithAdditionalRestriction(qfSupport => !qfSupport.Actions.ActionHistoryThisTurn.Any(act => !act.HasTrait(Trait.Move)) ? null : "You already took a non-move action this turn.")) {
                        ShortDescription = $"Until the start of your next turn, all creatures within 10 feet of your companion bunny suffer a -1 circumstance penalty to attack rolls against you."
                    }.WithEffectOnSelf(caster => {
                        QEffect qEffect = new QEffect("Support", $"Enemy creatures within 10 feet suffer a -1 circumstance penalty to attack {owner.Name}", ExpirationCondition.ExpiresAtStartOfSourcesTurn, owner, qfSupport.Owner.Illustration) {
                            DoNotShowUpOverhead = true,
                            PreventTakingAction = ca => !ca.HasTrait(Trait.Move) && ca.ActionId != ActionId.EndTurn ? "You used Support so you can't take non-move actions anymore this turn." : null
                        };

                        qEffect.AddGrantingOfTechnical(cr => cr.EnemyOf(caster) && caster.DistanceTo(cr.Occupies) <= 2, qfTech => {
                            qfTech.Illustration = IllustrationName.TortoiseAndTheHare;
                            qfTech.Name = "Warning Thump";
                            qfTech.Description = $"You suffer a -1 circumstance penalty to attacks rolls against {owner.Name}.";
                            qfTech.Source = caster;
                            qfTech.BonusToAttackRolls = (self, action, target) => target == owner ? new Bonus(-1, BonusType.Circumstance, "Warning thump") : null;
                        });
                        caster.AddQEffect(qEffect);
                    })
                });
            };

            return MakeAnimalCompanionItem(new Item(itemName, Illustrations.CompanionBunny, "greater companion bunny", 7, 360, [Trait.Magical, Trait.Invested, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike]),
                companion, "Greater Companion Bunny", "The snuggle pouch is the most important part of bun anatomy.", true);
        });

        public static ItemName LivingCloak { get; } = ModManager.RegisterNewItemIntoTheShop("LivingCloak", itemName => {
            return new Item(itemName, Illustrations.LivingCloak, "living cloak", 7, 360,
                [Trait.Magical, Trait.Worn, Trait.Invested, Trait.Cloak, Trait.Transmutation, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike])
            .WithWornAt(Trait.Cloak)
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}This enchanted cloak shifts and swirls with a mind of its own.{/i}\n\n" +
            "When you make a melee attack or are targeted by a strike, there's a 75% chance your living cloak will shift to assist you, either providing a +2 item bonus to AC or making a 2d4+2 bludgeoning damage follow up strike.")
            .WithPermanentQEffectWhenWorn((qfItem, item) => {
                qfItem.Innate = true;
                qfItem.Name = "Living Cloak";
                qfItem.Description = "When you make a melee attack or are targeted by a strike, there's a 75% chance your living cloak will shift to assist you, either providing a +2 item bonus to AC or making a 2d4+2 bludgeoning damage follow up strike.";
                qfItem.AfterYouTakeActionAgainstTarget = async (self, action, target, result) => {
                    if (!(target.DistanceTo(self.Owner) <= 1 && action.HasTrait(Trait.Attack) && R.NextD20() >= 16)) return;

                    var ca = CombatAction.CreateSimple(self.Owner, "Strike (living cloak)", Trait.Melee, Trait.Strike, Trait.Transmutation, Trait.UsableEvenWhenUnconsciousOrParalyzed, Trait.UsableThroughConfusion);
                    ca.Target = Target.Touch();
                    ca
                    .WithActionCost(0)
                    .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                        await caster.FictitiousSingleTileMove(target.Occupies);
                        Sfxs.Play(SfxName.SwordStrike);
                        await CommonSpellEffects.DealDirectDamage(spell, DiceFormula.FromText("2d4+2"), target, result, DamageKind.Bludgeoning);
                        await caster.FictitiousSingleTileMove(caster.Occupies);
                    });
                    ca.ChosenTargets.ChosenCreature = target;
                    ca.ChosenTargets.ChosenCreatures.Add(target);
                    if (ca.CanBeginToUse(ca.Owner) && (ca.Target is CreatureTarget creatureTarget && creatureTarget.IsLegalTarget(ca.Owner, target)))
                        await ca.AllExecute();
                };
                qfItem.YouAreTargetedByARoll = async (self, action, breakdown) => {
                    if (!(action.HasTrait(Trait.Attack) && action.HasTrait(Trait.Strike) && R.NextD20() >= 16)) return false;

                    self.Owner.Overhead("living cloak", Color.Lime, self.Owner + "'s living cloak shifts to intercept the attack.");
                    self.Owner.AddQEffect(new QEffect() {
                        ExpiresAt = ExpirationCondition.EphemeralAtEndOfImmediateAction,
                        BonusToDefenses = (self, action, def) => def == Defense.AC ? new Bonus(2, BonusType.Item, "Living cloak") : null
                    });
                    return true;
                };
            });
        });

        public static ItemName RingOfDeathDefiance { get; } = ModManager.RegisterNewItemIntoTheShop("RingOfDeathDefiance", itemName => {
            return new Item(itemName, Illustrations.RingOfDeathDefiance, "ring of death defiance", 3, 60,
                new Trait[] { Trait.Magical, Trait.Worn, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}This dull ring is cold to the touch, as if it exists somewhere halfway between the realm of the living and the dead.{/i}\n\n" +
            "The first time each encounter that you're reduced to 0 HP, your ring of death defiance activates to negate the damage.")
            .WithPermanentQEffectWhenWorn((qfItem, item) => {
                qfItem.YouAreDealtLethalDamage = async (self, attacker, dmg, you) => {
                    if (!item.IsUsedUp) {
                        item.UseUp();
                        self.Name += " (expended)";
                        Sfxs.Play(SfxName.ShieldSpell);
                        you.Overhead("*defied death*", Color.Black, you.Name + "'s ring of death defiance activates.");
                        return new SetToTargetNumberModification(0, "Ring of Death Defiance");
                    }
                    return null;
                };
                qfItem.Innate = true;
                qfItem.Name = "Ring of Death Defiance";
                qfItem.Description = "The first time each encounter that you're reduced to 0 HP, your ring of death defiance activates to negate the damage.";
                qfItem.EndOfCombat = async (self, won) => {
                    item.RevertUseUp();
                };
            });
        });

        public static ItemName MaskOfSkills { get; } = ModManager.RegisterNewItemIntoTheShop("MaskOfSkills", itemName => {
            return new Item(itemName, Illustrations.MaskOfSkills, "mask of skills", 3, 60,
                new Trait[] { Trait.Magical, Trait.Worn, Trait.Invested, Trait.Enchantment, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}An ornate eye mask that seems to hold the experiences of each and every wearer before you.{/i}\n\n" +
            "While wearing this mask, you a +2 item bonus to all skills.")
            .WithPermanentQEffectWhenWorn((qfItem, item) => {
                qfItem.BonusToSkills = (skill) => new Bonus(2, BonusType.Item, "Mask of skills");
            });
        });

        public static ItemName RodOfHealing { get; } = ModManager.RegisterNewItemIntoTheShop("RodOfHealing", itemName => {
            var item = new Item(itemName, Illustrations.RodOfHealing, "rod of healing", 3, 60,
                new Trait[] { Trait.Magical, Trait.Positive, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}This brilliant gem encrusted sceptre brims with healing energy.{/i}\n\n" +
            "The wielder of the rod can use it twice per day to fire a beam of healing energy.")
            ;

            item.StateCheckWhenWielded = (wielder, weapon) => {
                var uses = weapon.IsUsedUp ? 0 : weapon.ItemModifications.FirstOrDefault(mod => (string?)mod.Tag == "UsedOnceThisDay") != null ? 1 : 2;

                if (uses == 0) return;

                wielder.AddQEffect(new QEffect($"Rod of Healing Uses Remaining", "The rod of healing can be used this many more times today before its power is exhausted.", ExpirationCondition.Ephemeral, null, Illustrations.RodOfHealing) {
                    Value = uses
                });
            };

            item.ProvidesItemAction = (wielder, weapon) => {
                if (weapon.IsUsedUp) return null!;

                return new ActionPossibility(new CombatAction(wielder, Illustrations.RodOfHealing, "Vitality Beam", [Trait.Necromancy, Trait.Positive, Trait.Manipulate, Trait.Healing],
                    "{b}Area{/b} Up to 60-foot line" +
                    "\n\n{i}You fire a brilliant stream of positive energy from the rod.{/i}\n\n" +
                    $"Each living creature in the beam is healed for {(wielder.Level + 1) / 2}d8 hit points, and each undead creature suffers an equal amount of positive damage (basic Fortitude save mitigates)", Target.Line(12).WithLesserDistanceIsOkay())
                .WithActionCost(2)
                .WithSoundEffect(SfxName.Healing)
                .WithProjectileCone(IllustrationName.Heal, 15, ProjectileKind.Ray)
                .WithSavingThrow(new SavingThrow(Defense.Fortitude, wielder.ClassOrSpellDC()))
                .WithNoSaveFor((action, target) => !target.HasTrait(Trait.Undead))
                .WithEffectOnSelf(async user => {
                    if (weapon.ItemModifications.FirstOrDefault(mod => (string?)mod.Tag == "UsedOnceThisDay") == null) {
                        weapon.ItemModifications.Add(new ItemModification(ItemModificationKind.CustomPermanent) {
                            Tag = "UsedOnceThisDay",
                            ClearsAtLongRest = true
                        });
                    } else {
                        weapon.UseUp();
                    }

                })
                .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                    var spellLvl = (caster.Level + 1) / 2;

                    if (target.HasTrait(Trait.Undead)) {
                        await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, spellLvl + "d8", DamageKind.Positive);
                    } else if (target.IsLivingCreature) {
                        await target.HealAsync(spellLvl + "d8", spell);
                    }
                })
                );
            };

            return item;
        });

        public static ItemName BottomlessFlask { get; } = ModManager.RegisterNewItemIntoTheShop("BottomlessFlask", itemName => {
            var item = new Item(itemName, Illustrations.BottomlessFlask, "bottomless flask", 3, 60,
                [ Trait.Magical, Trait.Positive, Trait.Transmutation, Trait.Healing, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike ])
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}This ornate flask appears to be linked to some unseen well spring of curitive waters.{/i}\n\n" +
            "Once per encounter, the flask can be drunk from to restore an amount of hit points equal to the amount healed by a potion of healing of the drinker's level.")
            ;

            item.ProvidesItemAction = (wielder, weapon) => {
                if (weapon.IsUsedUp) return null!;

                var amount = wielder.Level < 3 ? "1d8" : wielder.Level < 5 ? "2d8+5" : wielder.Level < 11 ? "3d8+10" : "6d8+20";

                return new ActionPossibility(new CombatAction(wielder, Illustrations.BottomlessFlask, "Drink (bottomless flask)", [Trait.Necromancy, Trait.Positive, Trait.Manipulate, Trait.Healing],
                    $"Restore {amount} HP.", Target.Self().WithAdditionalRestriction(user => user.HasEffect(QEffectId.Sickened) ? "You're sickened." : user.Damage == 0 ? "already at full HP" : null))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.DrinkPotion)
                .WithEffectOnSelf(async (spell, user) => {
                    await user.HealAsync(amount, spell);
                    weapon.UseUp();
                }));
            };

            return item;
        });

        public static ItemName DuergarSkullShield { get; } = ModManager.RegisterNewItemIntoTheShop("RL_DuergarSkullShield", itemName => {
            var item = new Item(itemName, Illustrations.DuergarSkullShield, "duergar skull shield", 3, 60,
                [Trait.Magical, Trait.Enchantment, Trait.Shield, Trait.Martial, Trait.DoNotAddToCampaignShop, ModTraits.CannotHavePropertyRune, ModTraits.Roguelike])
            .WithMainTrait(Trait.SteelShield)
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Bludgeoning))
            .WithShieldProperties(6)
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}A bleak shield of deftly worked iron and obsidian, used by the standard bearer of a Duergar legion.{/i}\n\n" +
            "You have a +1 item bonus to Intimidation." +
            "\n\nWhen you raise the Duergar Skull Shield, make an intimidation check against the Will DC of all enemies within 15 feet. On a failure they become frightened, and on a critical failure they become frightened 2.")
            ;

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect("Duarger Skull Shield",
                    "When you raise the Duergar Skull Shield, make an intimidation check against the Will DC of all enemies within 15 feet. On a failure they become frightened, and on a critical failure they become frightened 2.", ExpirationCondition.Ephemeral, wielder, weapon.Illustration) {
                    ExpiresAt = ExpirationCondition.Ephemeral,
                    AfterYouTakeAction = async (self, action) => {
                        if (action.ActionId == ActionId.RaiseShield && action.Illustration == weapon.Illustration) {
                            var ca = new CombatAction(wielder, weapon.Illustration, "Duergar Skull Shield", [Trait.Emotion, Trait.Fear, Trait.Mental, Trait.Magical, Trait.Enchantment], "", Target.EnemiesOnlyEmanation(3))
                            .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Intimidation), TaggedChecks.DefenseDC(Defense.Will)))
                            .WithSoundEffect(wielder.HasTrait(Trait.Female) ? SfxName.Intimidate : SfxName.MaleIntimidate)
                            .WithProjectileCone(IllustrationName.Fear, 5, ProjectileKind.Cone)
                            .WithActionCost(0)
                            .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                                target.AddQEffect(QEffect.Frightened((int)result - 1));
                            });

                            await wielder.Battle.GameLoop.FullCast(ca);
                        }
                    },
                    BonusToSkills = (skill) => skill == Skill.Intimidation ? new Bonus(1, BonusType.Item, "Duergar skull shield") : null,
                });
            };

            return item;
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
                        if (rune.RuneProperties?.CanBeAppliedTo == null || rune.RuneProperties?.CanBeAppliedTo(rune, dagger) == null)
                            dagger.WithModificationRune(rune.ItemName);
                    }

                    var hammer = Items.CreateNew(LightHammer);
                    foreach (Item rune in item.Runes) {
                        if (rune.RuneProperties?.CanBeAppliedTo == null || rune.RuneProperties?.CanBeAppliedTo(rune, hammer) == null)
                            hammer.WithModificationRune(rune.ItemName);
                    }

                    var axe = Items.CreateNew(Hatchet);
                    foreach (Item rune in item.Runes) {
                        if (rune.RuneProperties?.CanBeAppliedTo == null || rune.RuneProperties?.CanBeAppliedTo(rune, axe) == null)
                            axe.WithModificationRune(rune.ItemName);
                    }

                    var shuriken = Items.CreateNew(Shuriken);
                    foreach (Item rune in item.Runes) {
                        if (rune.RuneProperties?.CanBeAppliedTo == null || rune.RuneProperties?.CanBeAppliedTo(rune, shuriken) == null)
                            shuriken.WithModificationRune(rune.ItemName);
                    }

                    var javelin = Items.CreateNew(Javelin);
                    foreach (Item rune in item.Runes) {
                        if (rune.RuneProperties?.CanBeAppliedTo == null || rune.RuneProperties?.CanBeAppliedTo(rune, javelin) == null)
                            shuriken.WithModificationRune(rune.ItemName);
                    }

                    qfTB.Tag = new List<Item>() { dagger, javelin, hammer, axe, shuriken };
                };

                qfTB.ProvideActionIntoPossibilitySection = (self, section) => {
                    if (section.PossibilitySectionId != PossibilitySectionId.ItemActions || qfTB.Tag == null) {
                        return null;
                    }

                    int cost = qfTB.Owner.HasFeat(FeatName.QuickDraw) ? 0 : 1;

                    SubmenuPossibility menu = new SubmenuPossibility(Illustrations.ThrowersBandolier, "Thrower's Bandolier");
                    menu.Subsections.Add(new PossibilitySection("Thrower's Bandolier"));

                    foreach (Item throwable in (qfTB.Tag as List<Item>)!) {
                        menu.Subsections[0].AddPossibility((ActionPossibility)new CombatAction(qfTB.Owner, (qfTB.Tag as List<Item>)![0].Illustration, $"Draw {throwable.Name.CapitalizeEachWord()}", new Trait[] { Trait.Manipulate, Trait.Basic },
                        $"Draw a {throwable.Name} from your thrower's bandolier." +
                        ((throwable.ItemName == CustomItems.Shuriken) ? "\n\n{b}Special{/b} While equipped, you can also thrown an unlimited number of shurikens without using an action to draw them, using the Throw Shuriken action." : ""),
                        Target.Self().WithAdditionalRestriction(you => you.HeldItems.Count == 0 || (you.HeldItems.Count == 1 && !you.HeldItems[0].TwoHanded) ? null : "free hand required"))
                        .WithActionCost(cost)
                        .WithSoundEffect(SfxName.ItemGet)
                        .WithEffectOnSelf(async user => {
                            Item item = throwable.Duplicate();
                            item.Traits.Add(Trait.EncounterEphemeral);
                            user.HeldItems.Add(item);
                        }));
                    }

                    return menu;
                };
            });
        });

        public static ItemName DeathDrinkerAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("DeathDrinkerAmulet", itemName => {
            return new Item(itemName, Illustrations.SpiritBeaconAmulet, "death drinker amulet", 2, 35,
                new Trait[] { Trait.Magical, Trait.Worn, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Necklace)
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}This eerie necklace seems to feed off necrotic energy, storing it within its amethyst gems for some unknowable purpose.{/i}\n\n" +
            "While wearing this necklace, you gain resist Negative 3")
            .WithPermanentQEffectWhenWorn((qfMoC, item) => {
                qfMoC.StateCheck = self => {
                    self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Negative, 3);
                };
            });
        });

        public static ItemName GreaterDeathDrinkerAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("GreaterDeathDrinkerAmulet", itemName => {
            return new Item(itemName, Illustrations.SpiritBeaconAmulet, "greater death drinker amulet", 4, 60,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Necklace)
            .WithItemGroup("Roguelike mode")
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
            .WithItemGroup("Roguelike mode")
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
                qfCoA.BonusToSkills = (skill) => skill == Skill.Occultism ? new Bonus(1, BonusType.Item, "Spirit beacon amulet") : null;
            });
        });

        public static ItemName DiademOfTheSpiderQueen { get; } = ModManager.RegisterNewItemIntoTheShop("DiademOfTheSpiderQueen", itemName => {
            return new Item(itemName, Illustrations.DiademOfTheSpiderQueen, "diadem of the spider queen", 17, 19400,
                [Trait.Transmutation, Trait.Apex, Trait.Invested, Trait.Magical, Trait.Worn, Trait.Unsellable, Trait.Artifact, Trait.Unique])
            .WithWornAt(Trait.Headband)
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}Legend has it that the the Demon Queen of Spider's wore this very diadem during her apothesosis into the ranks of the Demon Lords.{/i}\n\n" +
            "Your key ability score modifier is increased by +1.")
            .WithOnCreatureWhenWorn((item, wearer) => {
                var keyABS = wearer.PersistentCharacterSheet?.Calculated.FinalAbilityScores.KeyAbility ?? Ability.Charisma;
                wearer.Abilities.Set(keyABS, wearer.Abilities.Get(keyABS) + 1);
                if (keyABS == Ability.Wisdom) {
                    wearer.Perception = wearer.Perception + 1;
                    wearer.Defenses.Set(Defense.Will, wearer.Defenses.GetBaseValue(Defense.Will) + 1);
                } else if (keyABS == Ability.Constitution) {
                    wearer.Defenses.Set(Defense.Fortitude, wearer.Defenses.GetBaseValue(Defense.Fortitude) + 1);
                    wearer.MaxHP += wearer.Level;
                }

                wearer.AddQEffect(new QEffect() {
                    StartOfCombat = async (self) => {
                        wearer.Defenses.Set(Defense.Reflex, wearer.Defenses.GetBaseValue(Defense.Reflex) + 1);
                        wearer.Defenses.Set(Defense.AC, wearer.Defenses.GetBaseValue(Defense.AC) + 1);
                    }
                });
            });
        });

        public static ItemName DemonBoundRing { get; } = ModManager.RegisterNewItemIntoTheShop("DemonBoundRing", itemName => {
            return new Item(itemName, Illustrations.DemonBoundRing, "demon bound ring", 3, 60,
                new Trait[] { Trait.Magical, Trait.Worn, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}Upon slipping on this ominous looking ring, incessant whispers fill your head, whispering arcane secrets and begging you to let it out.{/i}\n\n" +
            "While wearing this ring, you gain a +1 item bonus to arcana checks, and may use the Unleash Demon {icon:ThreeActions} action once per day.")
            .WithItemAction((item, user) => {
                return new CombatAction(user, Illustrations.DemonBoundRing, "Unleash Demon", new Trait[] { Trait.Conjuration, Trait.Magical, Trait.Manipulate },
                    "{b}Frequency{/b} once per day\n\n{b}Range{/b} 30 feet\n\nYou summon a Wrecker Demon whose level is equal to your own (maximum 6).\n\n" +
                    "At the start of your turn, there's a 25% chance that the demon will slip from your control, turning against the party." +
                    "\n\nImmediately when you cast this spell and then once each turn when you Sustain this spell, you can take two actions as " +
                    "the summoner creature. If you don't Sustain this spell during a turn, the summoned creature will go away." +
                    "\n\nOnce rogue however, the demon can no longer be banished in this way and will persist even after you fall unconscious.",
                    Target.RangedEmptyTileForSummoning(6))
                .WithActionCost(3)
                .WithSoundEffect(SfxName.DeepNecromancy)
                .WithEffectOnEachTile(async (action, caster, tiles) => {
                    Creature demon = Abrikandilu.CreateAbrikandilu();
                    if (caster.Level <= 2) {
                        demon.ApplyWeakAdjustments(false, true);
                    } else if (caster.Level == 3) {
                        demon.ApplyWeakAdjustments(false);
                    }
                    if (caster.Level == 5) {
                        demon.ApplyEliteAdjustments(false);
                    } else if (caster.Level >= 6) {
                        demon.ApplyEliteAdjustments(true);
                    }
                    demon.AddQEffect(new QEffect("Bound Demon", $"This demon is bound to {caster.Name}'s will. At the start of its master's turn, there's a 25% chance that it will break free of their control and turn against the party.") {
                        Id = QEffectId.SummonedBy,
                        Source = caster,
                        StateCheck = dominateQf => {
                            if (dominateQf.Source == null)
                                return;

                            if (dominateQf.Source.HasEffect(QEffectId.Controlled))
                                dominateQf.Owner.AddQEffect(new QEffect() { Id = QEffectId.Controlled }.WithExpirationEphemeral());

                            dominateQf.Owner.OwningFaction = dominateQf.Source.OwningFaction;
                        }
                    });
                    demon.EntersInitiativeOrder = false;
                    demon.Traits.Add(Trait.Minion);
                    demon.Traits.Add(Trait.Summoned);
                    demon.Traits.Add(Trait.Conjuration);
                    QEffect sustainedeffect = new QEffect {
                        Id = QEffectId.SummonMonster,
                        CannotExpireThisTurn = true,
                        Source = caster,
                        ExpiresAt = ExpirationCondition.ExpiresAtEndOfSourcesTurn,
                        CountsAsBeneficialToSource = true,
                        StateCheck = self => {
                            if (self.Owner.HasEffect(QEffectId.Confused)) {
                                demon.AddQEffect(QEffect.Confused(false, CombatAction.CreateSimple(self.Owner, "Summoning Feedback")).WithExpirationEphemeral());
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
                                    demon.Battle.Log(demon?.ToString() + " rolls for initiative: " + demon!.Initiative);
                                }

                                demon.Battle.InitiativeOrder.Insert(index, demon);
                                demon.Overhead("*breaks free*", Color.DarkRed, $"{caster.Name}'s summoned {demon.Name} breaks free from their control!");
                                self.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        },
                        WhenExpires = delegate {
                            if (!demon.DeathScheduledForNextStateCheck) {
                                demon.Overhead("*banished*", Color.Black, demon?.ToString() + " is banished because its summoner didn't sustain the summoning spell.");
                                demon!.DeathScheduledForNextStateCheck = true;
                            }
                        },
                        WhenMonsterDies = delegate (QEffect casterQf) {
                            if (!demon.DeathScheduledForNextStateCheck) {
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
                            demon.Actions.ResetToFull();
                            await CommonSpellEffects.YourMinionActs(demon);

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
                qfCoA.BonusToSkills = (skill) => skill == Skill.Arcana ? new Bonus(1, BonusType.Item, "Demon bound ring") : null;
            });
        });

        public static ItemName GreaterDemonBoundRing { get; } = ModManager.RegisterNewItemIntoTheShop("GreaterDemonBoundRing", itemName => {
            return new Item(itemName, Illustrations.DemonBoundRing, "greater demon bound ring", 7, 360,
                new Trait[] { Trait.Magical, Trait.Worn, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}Upon slipping on this ominous looking ring, incessant whispers fill your head, whispering arcane secrets and begging you to let it out.{/i}\n\n" +
            "While wearing this ring, you gain a +1 item bonus to arcana checks, and may use the Unleash Demon {icon:ThreeActions} action once per day.")
            .WithItemAction((item, user) => {
                return new CombatAction(user, Illustrations.DemonBoundRing, "Unleash Demon", new Trait[] { Trait.Conjuration, Trait.Manipulate },
                    "{b}Frequency{/b} once per day\n\n{b}Range{/b} 30 feet\n\nYou summon a random demon bound within the ring whose level is equal to your own.\n\n" +
                    "At the start of your turn, there's a 15% chance that the demon will slip from your control, turning against the party." +
                    "\n\nImmediately when you cast this spell and then once each turn when you Sustain this spell, you can take two actions as " +
                    "the summoner creature. If you don't Sustain this spell during a turn, the summoned creature will go away." +
                    "\n\nOnce rogue however, the demon can no longer be banished in this way and will persist even after you fall unconscious.",
                    Target.RangedEmptyTileForSummoning(6))
                .WithActionCost(3)
                .WithSoundEffect(SfxName.DeepNecromancy)
                .WithEffectOnEachTile(async (action, caster, tiles) => {

                    var list = MonsterStatBlocks.MonsterExemplars.Where(pet => pet.HasTrait(Trait.Demon) && CommonEncounterFuncs.Between(pet.Level, caster.Level - 1, caster.Level + 2) && !pet.HasTrait(Trait.DemonLord) && !pet.HasTrait(Trait.NonSummonable)).ToArray();

                    if (list.Count() <= 0) {
                        caster.Overhead("*summon failed*", Color.White, $"There are no valid demons for {caster.Name} to summon.");
                        return;
                    }

                    Creature demon = MonsterStatBlocks.MonsterFactories[list[R.Next(list.Count())].Name](caster.Battle.Encounter, tiles[0]);

                    if (demon.Level - caster.Level >= 2) {
                        demon.ApplyWeakAdjustments(false, true);
                    } else if (demon.Level - caster.Level == 1) {
                        demon.ApplyWeakAdjustments(false);
                    } else if (demon.Level - caster.Level == -1) {
                        demon.ApplyEliteAdjustments();
                    } else if (demon.Level - caster.Level == -2) {
                        demon.ApplyEliteAdjustments(true);
                    }
                    demon.AddQEffect(new QEffect("Bound Demon", $"This demon is bound to {caster.Name}'s will. At the start of its master's turn, there's a 25% chance that it will break free of their control and turn against the party.") {
                        Id = QEffectId.SummonedBy,
                        Source = caster,
                        StateCheck = dominateQf => {
                            if (dominateQf.Source == null)
                                return;

                            if (dominateQf.Source.HasEffect(QEffectId.Controlled))
                                dominateQf.Owner.AddQEffect(new QEffect() { Id = QEffectId.Controlled }.WithExpirationEphemeral());

                            dominateQf.Owner.OwningFaction = dominateQf.Source.OwningFaction;
                        }
                    });
                    demon.EntersInitiativeOrder = false;
                    demon.Traits.Add(Trait.Minion);
                    demon.Traits.Add(Trait.Summoned);
                    demon.Traits.Add(Trait.Conjuration);
                    QEffect sustainedeffect = new QEffect {
                        Id = QEffectId.SummonMonster,
                        CannotExpireThisTurn = true,
                        Source = caster,
                        ExpiresAt = ExpirationCondition.ExpiresAtEndOfSourcesTurn,
                        CountsAsBeneficialToSource = true,
                        StateCheck = self => {
                            if (self.Owner.HasEffect(QEffectId.Confused)) {
                                demon.AddQEffect(QEffect.Confused(false, CombatAction.CreateSimple(self.Owner, "Summoning Feedback")).WithExpirationEphemeral());
                            }
                        },
                        StartOfYourPrimaryTurn = async (self, creature) => {
                            int roll = R.NextD20();
                            if (roll <= 3) {
                                demon.Traits.Remove(Trait.Minion);
                                self.WhenExpires = null;
                                demon.OwningFaction = demon.Battle.Enemy;
                                demon.EntersInitiativeOrder = true;
                                demon.RecalculateLandSpeedAndInitiative();
                                demon.Initiative = DiceFormula.FromText("1d20+" + demon.InitiativeBonus).Roll().Item1;
                                int index = demon.Battle.InitiativeOrder.FindLastIndex((Creature cr) => cr.Initiative > demon.Initiative) + 1;
                                if (demon.Battle.GameLoop.InitialInitiativeOrderIsSet) {
                                    demon.Battle.Log(demon?.ToString() + " rolls for initiative: " + demon?.Initiative);
                                }

                                demon!.Battle.InitiativeOrder.Insert(index, demon);
                                demon.Overhead("*breaks free*", Color.DarkRed, $"{caster.Name}'s summoned {demon.Name} breaks free from their control!");
                                self.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        },
                        WhenExpires = delegate {
                            if (!demon.DeathScheduledForNextStateCheck) {
                                demon.Overhead("*banished*", Color.Black, demon?.ToString() + " is banished because its summoner didn't sustain the summoning spell.");
                                demon!.DeathScheduledForNextStateCheck = true;
                            }
                        },
                        WhenMonsterDies = delegate (QEffect casterQf) {
                            if (!demon.DeathScheduledForNextStateCheck) {
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
                qfCoA.BonusToSkills = (skill) => skill == Skill.Arcana ? new Bonus(1, BonusType.Item, "Greater demon bound ring") : null;
            });
        });

        public static ItemName HornOfTheHunt { get; } = ModManager.RegisterNewItemIntoTheShop("Horn of the Hunt", itemName => {
            return new Item(itemName, Illustrations.HornOfTheHunt, "horn of the hunt", 3, 60,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Conjuration, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
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
                        CombatAction summon = new CombatAction(caster, Illustrations.HornOfTheHunt, "Summon Hunting Hound", [Trait.UsableEvenWhenUnconsciousOrParalyzed], "", Target.RangedEmptyTileForSummoning(100))
                        .WithActionCost(0)
                        .WithEffectOnEachTile(async (_, _, subtiles) => {
                            Creature wolf = Wolf.CreateWolf();
                            wolf.AddQEffect(CommonQEffects.CantOpenDoors());
                            wolf.AddQEffect(new QEffect("Call of the Hunt", $"This creature is compelled to attack {d.Name} and will vanish after its task is complete.", ExpirationCondition.Never, d, Illustrations.HornOfTheHunt) {
                                StateCheck = self => {
                                    if (self.Source == null || !self.Source.AliveOrUnconscious || !self.Owner.Alive) {
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
                        if (!(self.Tag as List<Creature>)!.Any(cr => cr.Alive)) {
                            self.ExpiresAt = ExpirationCondition.Immediately;
                        }
                    };

                    item.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));
                })
                ;
            }, (item, _) => !item.ItemModifications.Any(mod => mod.Kind == ItemModificationKind.UsedThisDay))
            .WithPermanentQEffectWhenWorn((qfCoA, item) => {
                qfCoA.BonusToSkills = (skill) => skill == Skill.Nature ? new Bonus(1, BonusType.Item, "Horn of the hunt") : null;
            });
        });

        public static ItemName ShifterFurs { get; } = ModManager.RegisterNewItemIntoTheShop("Shifter Furs", itemName => {
            return new Item(itemName, Illustrations.ShifterFurs, "shifter furs", 3, 60,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Transmutation, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithItemGroup("Roguelike mode")
            .WithWornAt(Trait.Cloak)
            .WithDescription("{i}This haggard fur cloak is has a musky, feral smell to it and seems to pulsate warmly... as if it were not simply a cloak, but the flesh of a living, breathing thing.{/i}\n\n" +
            "You have a +2 item bonus to Demoralize check made against animals, and gain the benefits of the Intimidating Glare feat.\n\n" +
            "Once per encounter, as a {icon:FreeAction} action, you may invoke the cloak's magic to assume a random animal form until the start of your next turn. While transformed, your weapons are replaced with natural appendages related to your new form and you cannot cast spells.")
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
                        BonusToAttackRolls = (self, action, target) => action.HasTrait(Trait.BattleformAttack) ? new Bonus(2, BonusType.Status, "Shifter furs") : null,
                        WhenExpires = self => {
                            foreach (Item obj in caster.HeldItems.ToList<Item>())
                                caster.DropItem(obj);
                            foreach (Item obj in previouslyHeldItems) {
                                if (caster.CarriedItems.Contains(obj)) {
                                    caster.CarriedItems.Remove(obj);
                                    caster.HeldItems.Add(obj);
                                }
                            }
                        },
                        EndOfCombat = async (self, victory) => {
                            if (victory != true) {
                                return;
                            }
                            if (self.WhenExpires != null)
                                self.WhenExpires(self);
                        },
                    };

                    int roll = R.Next(1, 4);
                    switch (roll) {
                        case 1:
                            transform.Illustration = IllustrationName.AnimalFormBear;
                            transform.Name = "Bear Form";
                            transform.Description = $"{user.Name} has assumed the form of a ferocious bear, capable of grappling its prey on a successful jaws attack and with a +2 item bonus to armour.";
                            transform.StateCheck = self => {
                                self.Owner.ReplacementIllustration = IllustrationName.AnimalFormBear;
                                self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d10", DamageKind.Piercing, Trait.BattleformAttack, Trait.WizardWeapon, Trait.Simple).WithAdditionalWeaponProperties(properties => {
                                    properties.WithOnTarget(async (strike, a, d, result) => {
                                        if (result >= CheckResult.Success)
                                            await Possibilities.Grapple(a, d, result);
                                    });
                                    properties.DamageDieCount = user.Level >= 4 ? 2 : 1;
                                    properties.ItemBonus = user.Level >= 2 ? 1 : 0;
                                });
                            };
                            transform.AdditionalUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.DragonClaws, "claws", "1d6", DamageKind.Slashing, Trait.Agile, Trait.BattleformAttack, Trait.WizardWeapon, Trait.Simple);
                            transform.BonusToDefenses = (self, action, defence) => {
                                if (defence == Defense.AC) {
                                    return new Bonus(2, BonusType.Item, "Natural armour");
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
                                self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d6", DamageKind.Piercing, Trait.BattleformAttack, Trait.AddsInjuryPoison, Trait.WizardWeapon, Trait.Simple).WithAdditionalWeaponProperties(properties => {
                                    properties.AdditionalDamage.Add(("1d4", DamageKind.Poison));
                                    properties.DamageDieCount = user.Level >= 4 ? 2 : 1;
                                    properties.ItemBonus = user.Level >= 2 ? 1 : 0;
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
                                self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d10", DamageKind.Piercing, Trait.BattleformAttack, Trait.Unarmed, Trait.WizardWeapon, Trait.Simple).WithAdditionalWeaponProperties(properties => {
                                    properties.DamageDieCount = user.Level >= 4 ? 2 : 1;
                                    properties.ItemBonus = user.Level >= 2 ? 1 : 0;
                                }); ;
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
                qfCoA.BonusToSkillChecks = (skill, action, d) => d != null && action.ActionId == ActionId.Demoralize && d.HasTrait(Trait.Animal) ? new Bonus(2, BonusType.Item, "Shifter furs") : null;
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
            .WithItemGroup("Roguelike mode")
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
                        return new Bonus(2, BonusType.Item, "Cloak of air");
                    }
                    return null;
                };
            });
        });

        public static ItemName BloodBondAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("Blood Bond Amulet", itemName => {
            return new Item(itemName, Illustrations.BloodBondAmulet, "blood bond amulet", 3, 40,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
            .WithWornAt(Trait.Necklace)
            .WithItemGroup("Roguelike mode")
            .WithDescription("These matching amulets link the lifeforce of those who wear them, allowing them to freely claw away the vitality of the paired wearer for themselves.\n\n" +
            "{b}Life Transfer {icon:FreeAction}.{/b} You extract the lifeforce from an ally wearing a matching amulet, dealing 2d8 damage, and healing yourself for an equivalent amount of HP.")
            .WithItemAction((item, user) => {
                return new CombatAction(user, IllustrationName.BloodVendetta, "Siphon Life", new Trait[] { Trait.Magical, Trait.Necromancy, Trait.Healing }, "You extract the lifeforce from an ally wearing a matching amulet, dealing 2d8 damage, and healing yourself for an equivalent amount of HP.", Target.RangedFriend(3)
                .WithAdditionalConditionOnTargetCreature(new FriendCreatureTargetingRequirement())
                .WithAdditionalConditionOnTargetCreature((a, d) => {
                    if (d.CarriedItems.Any(i => i.ItemName == BloodBondAmulet && i.IsWorn == true)) {
                        return Usability.Usable;
                    } else {
                        return Usability.NotUsableOnThisCreature("No matching amulet");
                    }
                }))
                .WithActionCost(0)
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

            List<ItemName> items = new List<ItemName>() { GreaterCompanionBunny, LunaRunestone, RingOfDeathDefiance, RodOfHealing, MaskOfSkills, BottomlessFlask, DuergarSkullShield, CompanionBunny, GnomishPuzzleBox, GreaterSacredSerpent, SacredSerpent, Javelin, RingOfMonsters, RunestoneOfOpportunism, RunestoneOfMirrors, CloakOfDuplicity, RunestoneOfPandemomium, SceptreOfPandemonium, GreaterDemonBoundRing, MedusaEyeChoker, SerpentineBow, GreaterScourgeOfFangs, FightingFan, Kusarigama, HookSword, Kama, Sai, Nunchaku, Shuriken, SpiderHatchling, AlicornDagger, AlicornPike, ThrowersBandolier, SpellbanePlate, SceptreOfTheSpider, DeathDrinkerAmulet, GreaterDeathDrinkerAmulet, RobesOfTheWarWizard, GreaterRobesOfTheWarWizard, WhisperMail, KrakenMail, DuelingSpear, DemonBoundRing, ShifterFurs, SmokingSword, StormHammer, ChillwindBow, Sparkcaster, HungeringBlade, SpiderChopper, WebwalkerArmour, DreadPlate, Hexshot, ProtectiveAmulet, MaskOfConsumption, FlashingRapier, Widowmaker, DolmanOfVanishing, BloodBondAmulet };

            // Roguelike Wands
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
            CreateWand(SpellId.SuddenBolt, null);
            CreateWand(SpellId.SuddenBlight, null);
            CreateWand(SpellId.SummonElemental, 3);
            CreateWand(SpellId.LooseTimesArrow, null);
            CreateWand(SpellId.Fireball, 4);
            CreateWand(SpellId.MagicMissile, 3);
            CreateWand(SpellId.MagicMissile, 4);
            CreateWand(SpellId.BoneSpray, 4);
            CreateWand(SpellId.RadiantBeam, null);
            CreateWand(SpellId.WallOfFire, null);
            CreateWand(SpellId.LightningBolt, null);
            CreateWand(SpellId.Haste, null);
            CreateWand(SpellId.Slow, null);
            CreateWand(SpellId.SuddenBolt, 4);
            CreateWand(SpellId.SuddenBlight, 4);
            CreateWand(SpellId.Geyser, null);
            CreateWand(SpellId.StagnateTime, null);
            CreateWand(SpellId.QuickenTime, null);
            CreateWand(SpellId.IncendiaryFog, null);
            // CreateWand(SpellId.Blis, null);
            CreateWand(SpellId.WyvernSting, null);
            CreateWand(SpellId.ConeOfCold, null);
            CreateWand(SpellId.CrushingDespair, null);
            CreateWand(SpellId.FlameStrike, null);
            CreateWand(SpellId.Dominate, null);
            CreateWand(SpellId.Geyser, null);
            CreateWand(SpellId.HealingWell, null);
            //CreateWand(SpellLoader.LesserDominate, null, false);

            foreach (var spell in AllSpells.All.Where(sp =>
                !sp.HasTrait(Trait.Cantrip)
                && !sp.HasTrait(Trait.Focus)
                && !sp.HasTrait(Trait.Uncommon)
                && sp.CombatActionSpell.ActionCost != Constants.ACTION_COST_REACTION
            )) {
                CreateWand(spell.SpellId, null, WandType.RELIABLE);
                if (spell.CombatActionSpell.Heightening != null || spell.HasTrait(Trait.Incapacitation)) {
                    for (int i = spell.MinimumSpellLevel + 1; i <= 10; i++) {
                        if (spell.CombatActionSpell.Heightening?.HeightensAtLevel[i] ?? false || spell.HasTrait(Trait.Incapacitation)) {
                            CreateWand(spell.SpellId, i, WandType.RELIABLE);
                        }
                    }
                }
            }

            // Item QEffects

            // Clean once per encounter item effects
            ModManager.RegisterActionOnEachCreature(creature => {
                List<Item> items = new List<Item>();

                AddEndOfEncounterRevertUseUp(creature, BottomlessFlask);
                AddEndOfEncounterRevertUseUp(creature, SceptreOfTheSpider);
                AddEndOfEncounterRevertUseUp(creature, SceptreOfPandemonium);
                AddEndOfEncounterRevertUseUp(creature, ProtectiveAmulet);


                static void AddEndOfEncounterRevertUseUp(Creature creature, ItemName itemName) {
                    var items = creature.HeldItems.Where(itm => itm.ItemName == itemName).ToList();
                    items = items.Concat(creature.CarriedItems.Where(itm => itm.ItemName == itemName).ToList()).ToList();

                    foreach (Item item in items) {
                        creature.AddQEffect(new QEffect() {
                            Tag = item,
                            EndOfCombat = async (self, won) => {
                                item.RevertUseUp();
                            }
                        });
                    }
                }
            });

            // Thrower's Bandolier logic
            ModManager.RegisterActionOnEachCreature(creature => {
                creature.AddQEffect(new QEffect() {
                    StateCheckWithVisibleChanges = async _ => {
                        Item? bandolier = creature.CarriedItems.FirstOrDefault(item => item.ItemName == ThrowersBandolier && item.IsWorn);

                        if (bandolier == null && !creature.CarriedItems.Any(item => item.ItemName == Shuriken))
                            return;

                        var uniqueShurikens = new List<(Item?, Item)>();
                        int numShurikens = 0;
                        var allShurikens = new List<(Item?, Item)>();
                        foreach (var item in creature.CarriedItems) {
                            if (item.ItemName == Shuriken)
                                allShurikens.Add((null, item));
                            foreach (var subItem in item.StoredItems) {
                                if (subItem.ItemName == Shuriken)
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

                        creature.AddQEffect(new QEffect() {
                            ExpiresAt = ExpirationCondition.Ephemeral,
                            ProvideActionIntoPossibilitySection = (self, section) => {
                                if (section.PossibilitySectionId == PossibilitySectionId.ItemActions) {
                                    var menu = new SubmenuPossibility(new SideBySideIllustration(Illustrations.Shuriken, IllustrationName.Throw), "Throw Shuriken");
                                    var subsection = new PossibilitySection($"Throw Shuriken ({(bandolier == null ? "x" + numShurikens : "unlimited")})");
                                    menu.Subsections.Add(subsection);
                                    foreach ((Item?, Item) shuriken in uniqueShurikens) {
                                        var strike = StrikeRules.CreateStrike(self.Owner, shuriken.Item2, RangeKind.Ranged, -1, true);
                                        (strike.Target as CreatureTarget)!.WithAdditionalConditionOnTargetCreature((a, d) => a.HasFreeHand ? Usability.Usable : Usability.NotUsable("no-free-hand"));
                                        strike.WithPrologueEffectOnChosenTargetsBeforeRolls(async (action, user, targets) => {
                                            if (shuriken.Item1 != null)
                                                // RunestoneRules.RecreateWithUnattachedSubitem(shuriken.Item1, shuriken.Item2, false);
                                                shuriken.Item1.StoredItems.Remove(shuriken.Item2);
                                            else
                                                self.Owner.CarriedItems.Remove(shuriken.Item2);
                                            user.AddHeldItem(shuriken.Item2);
                                        });
                                        strike.Name += " (" + shuriken.Item2.Name + ")";
                                        subsection.AddPossibility((ActionPossibility)strike);
                                    }

                                    if (bandolier != null) {
                                        var shuriken = Items.CreateNew(Shuriken);
                                        shuriken.Traits.Add(Trait.EncounterEphemeral);
                                        foreach (Item rune in bandolier.Runes) {
                                            if (rune.RuneProperties?.CanBeAppliedTo == null || rune.RuneProperties?.CanBeAppliedTo(rune, shuriken) == null)
                                                shuriken.WithModificationRune(rune.ItemName);
                                        }

                                        var strike = StrikeRules.CreateStrike(self.Owner, shuriken, RangeKind.Ranged, -1, true);
                                        (strike.Target as CreatureTarget)?.WithAdditionalConditionOnTargetCreature((a, d) => a.HasFreeHand ? Usability.Usable : Usability.NotUsable("no-free-hand"));
                                        strike.WithPrologueEffectOnChosenTargetsBeforeRolls(async (action, user, targets) => {
                                            user.AddHeldItem(shuriken);
                                        });
                                        strike.Name += " from bandolier (" + shuriken.Name + ")";
                                        subsection.AddPossibility((ActionPossibility)strike);
                                    }

                                    return menu;
                                }

                                return null;
                            }
                        });
                    }
                });
            });

            // Armour effects
            ModManager.RegisterActionOnEachCreature(creature => {
                if (creature.BaseArmor == null) {
                    return;
                }

                if (creature.BaseArmor.ItemName == SpellbanePlate) {
                    creature.AddQEffect(new QEffect("Spellbane Plate", "You have a +1 item bonus vs. all spell saving throws but cannot cast spells of your own.") {
                        BonusToDefenses = (self, action, defence) => defence != Defense.AC && action != null && action.HasTrait(Trait.Spell) ? new Bonus(1, BonusType.Item, "Spellbane plate") : null,
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
                                return new Bonus(1, BonusType.Item, "Whisper mail");
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

                            if (vas != null && !((action.ActuallySpentActions == 1 && (vas.IfOneAction is EmanationTarget || vas.IfOneAction is ConeAreaTarget)) || (action.ActuallySpentActions == 2 && (vas.IfTwoActions is EmanationTarget || vas.IfTwoActions is ConeAreaTarget))
                            || (action.ActuallySpentActions == 3 && (vas.IfThreeActions is EmanationTarget || vas.IfThreeActions is ConeAreaTarget)))) {
                                return null;
                            } else if (vas == null && varient != null && !(varient.TargetInThisVariant is EmanationTarget || varient.TargetInThisVariant is ConeAreaTarget)) {
                                return null;
                            } else if (vas == null && varient == null && !(action.Target is EmanationTarget || action.Target is ConeAreaTarget)) {
                                return null;
                            }

                            return new Bonus(2 * action.SpellLevel, BonusType.Item, "Robes of the war wizard", true);
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

                            return new Bonus(3 * action.SpellLevel, BonusType.Item, "Greater robes of the war wizard", true);
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

                if (creature.BaseArmor.ItemName == InquisitrixLeathers) {
                    creature.AddQEffect(new QEffect() {
                        BonusToSkills = skill => skill == Skill.Intimidation ? new Bonus(1, BonusType.Item, "Inquisitrix leathers") : null,
                        StartOfCombat = async self => {
                            self.Description = "{b}Trigger{/b} An enemy within 15 feet damages you. {b}Effect{/b} Your attacker suffers 1d6+" + Math.Max(1, self.Owner.Level / 2) +
                            " mental damage (basic Will save mitigates).";
                        },
                        AfterYouTakeDamage = async (self, amount, kind, action, critical) => {
                            if (action == null || action.Owner == null || action.Owner == action.Owner.Battle.Pseudocreature) {
                                return;
                            }

                            if (action.Owner.OwningFaction == self.Owner.OwningFaction) {
                                return;
                            }

                            if (self.Owner.DistanceTo(action.Owner) > 3) {
                                return;
                            }

                            if (await self.Owner.AskToUseReaction($"{action.Owner.Name} dares to strike you! Do you wish to use your iron command to deal 1d6 + {Math.Max(1, self.Owner.Level / 2)} mental damage to your attacker?")) {
                                CombatAction dummyAction = new CombatAction(self.Owner, self.Owner.Illustration, "Inquisitrix Leathers", new Trait[] { Trait.Divine, Trait.Emotion, Trait.Enchantment, Trait.Mental },
                                    $"You deal 1d6+{Math.Max(1, self.Owner.Level / 2)} mental damage to a creature that attacked you.", Target.Uncastable());

                                var result = CommonSpellEffects.RollSavingThrow(action.Owner, dummyAction, Defense.Will, self.Owner.ClassOrSpellDC());

                                await CommonSpellEffects.DealBasicDamage(dummyAction, self.Owner, action.Owner, result, DiceFormula.FromText($"1d6+{Math.Max(1, self.Owner.Level / 2)}", "Inquisitrix Leathers"), DamageKind.Mental);
                            }
                        }
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
                    creature.AddQEffect(new QEffect("Cloak of Vanishing", "This creature can attempt to hide, even in plain sight.") {
                        Id = QEffectId.HideInPlainSight,
                        BonusToSkills = skill => skill == Skill.Stealth ? new Bonus(2, BonusType.Item, "Dolman of vanishing") : null
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
                                            weapon.WeaponProperties!.AdditionalDamage[0] = ("1d6", weapon.WeaponProperties.AdditionalDamage[0].Item2);
                                            weapon.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));

                                            // Show effect
                                            user.AddQEffect(new QEffect($"{weapon.Name.CapitalizeEachWord()} Activated", "Extra elemental damage increased from 1 to 1d6.") {
                                                ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
                                                Tag = weapon,
                                                WhenExpires = (self) => {
                                                    Item weapon = self.Tag as Item;
                                                    weapon!.WeaponProperties!.AdditionalDamage[0] = ("1", weapon.WeaponProperties.AdditionalDamage[0].Item2);
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
                foreach (Item item in creature.HeldItems.Concat(creature.CarriedItems)) {
                    if (item.HasTrait(ModTraits.Wand) && item.IsUsedUp) {
                        item.Illustration = new UsedUpIllustration(item.Illustration, "used", Color.MediumPurple);
                    }
                }

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

                                        Spell spell = (Spell)item.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.CustomPermanent && mod.Tag != null && mod.Tag is Spell)?.Tag;
                                        spell = spell?.Duplicate(wandHolder, spell.SpellLevel, true);

                                        if (spell == null) {
                                            return null;
                                        }

                                        Possibility spellPossibility = Possibilities.CreateSpellPossibility(spell.CombatActionSpell);
                                        spellPossibility.PossibilitySize = PossibilitySize.Full;
                                        if (used != null && used.Tag != (object)true) {
                                            spellPossibility.Illustration = new DualIllustration(Illustrations.Overcharge, spellPossibility.Illustration);
                                            spellPossibility.Caption += " (Overcharge)";
                                        }
                                        if ((spellPossibility as SubmenuPossibility) == null) {
                                            CombatAction action = (spellPossibility as ActionPossibility)!.CombatAction;
                                            action.Owner = wandHolder;
                                            action.Item = wand;
                                            action.SpellcastingSource = wandHolder.Spellcasting?.Sources.FirstOrDefault(source => action.Traits.Contains(source.SpellcastingTradition));

                                            if (used != null && used.Tag != (object)true) {
                                                action.Illustration = new DualIllustration(Illustrations.Overcharge, action.Illustration);
                                                action.Name += " (Overcharge)";
                                                action.Description += "\n\n{b}Overcharged.{/b} Overcharging your wand to cast this spell has a 50% chance of permanantly destroying it.";
                                            }
                                        } else {
                                            SubmenuPossibility spellVars = spellPossibility as SubmenuPossibility;
                                            bool firstLoop = true;
                                            foreach (var varient in spellVars!.Subsections[0].Possibilities) {
                                                CombatAction action;
                                                if (varient is ChooseActionCostThenActionPossibility) {
                                                    action = (varient as ChooseActionCostThenActionPossibility)!.CombatAction;
                                                } else {
                                                    action = (varient as ChooseVariantThenActionPossibility)!.CombatAction;
                                                }
                                                action.Owner = wandHolder;
                                                action.Item = wand;
                                                action.SpellcastingSource = wandHolder.Spellcasting?.Sources.FirstOrDefault(source => action.Traits.Contains(source.SpellcastingTradition));

                                                if (used != null && used.Tag != (object)true) {
                                                    if (firstLoop) {
                                                        action.Illustration = new DualIllustration(Illustrations.Overcharge, action.Illustration);
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
                                                wand.Illustration = new UsedUpIllustration(wand.Illustration, "used", Color.MediumPurple);
                                            } else {
                                                used.Tag = true;
                                                (CheckResult, string) result = Checks.RollFlatCheck(10);
                                                wandHolder.Overhead(result.Item1 >= CheckResult.Success ? "Overcharge success!" : $"{wand.Name} was destoyed!", result.Item1 >= CheckResult.Success ? Color.Green : Color.Red, result.Item2, result.Item1 <= CheckResult.Failure ? $"{wand.Name} was permanantly destroyed from being overcharged!" : null);
                                                if (result.Item1 <= CheckResult.Failure) {
                                                    Sfxs.Play(SoundEffects.WandOverload, 1.2f);
                                                    wandHolder.HeldItems.Remove(wand);
                                                }
                                            }
                                        }
                                    },
                                    EndOfCombat = async (self, won) => {
                                        if (wand.HasTrait(ModTraits.ReliableWand))
                                            wand.ItemModifications.RemoveAll(mod => mod.Kind == ItemModificationKind.UsedThisDay);
                                    }
                                });
                            }
                        }
                    }
                });
            });
        }

        internal static ItemName CreateWand(SpellId spellId, int? level, WandType type = WandType.DARKSTEEL) {
            Spell baseSpell = AllSpells.TemplateSpells.GetValueOrDefault(spellId);

            if (baseSpell == null) return ItemName.None;

            if (level == null || level < baseSpell.MinimumSpellLevel) {
                level = baseSpell.MinimumSpellLevel;
            }

            ItemModification mod = new ItemModification(ItemModificationKind.CustomPermanent) {
                Tag = baseSpell.Duplicate(null, (int)level, true)
                //SpellId = spellId,
                //HeightenedToSpellLevel = baseSpell.Duplicate(null, (int)level, true).SpellLevel
            };

            List<Trait> traits = new List<Trait> { ModTraits.Wand, Trait.Magical, Trait.Unarmed, Trait.Melee, Trait.SpecificMagicWeapon, ModTraits.Roguelike };

            foreach (Trait trait in baseSpell.Traits) {
                if (new Trait[] { Trait.Divine, Trait.Occult, Trait.Arcane, Trait.Primal, Trait.Elemental }.Contains(trait)) {
                    traits.Add(trait);
                }
            }

            string prefix = "";
            string desc = "";
            int itemLevel = 0;

            if (type == WandType.DARKSTEEL) {
                traits.Add(Trait.DoNotAddToCampaignShop);
                traits.Add(ModTraits.Darksteel);
                prefix = "darksteel";
                desc = "{/i}Said to be forged using the sinister workings of Drow magi, these sinister wands function only in the sunless caverns of the Below.{i}\n\n" +
                    "Allows the wielder to cast {i}" + AllSpells.CreateModernSpellTemplate(spellId, Trait.None).ToSpellLink() + "{/i} once per day." +
                    "\n\nIn addition, you can overcharge the wand to cast this spell a second time. However, doing so has a 50% chance to permanantly destroy this item.";
                itemLevel = (int)level * 2 - 1;
            }

            if (type == WandType.RELIABLE) {
                prefix = "reliable";
                traits.Add(ModTraits.ReliableWand);
                desc = "Allows the wielder to cast {i}" + AllSpells.CreateModernSpellTemplate(spellId, Trait.None).ToSpellLink() + "{/i} once per encounter. " +
                    "\n\nIn addition, you can overcharge the wand to cast this spell a second time. However, doing so has a 50% chance to permanantly destroy this item.";
                itemLevel = ((int)level + 1) * 2 - 1;
            }

            string name = level == baseSpell.MinimumSpellLevel ? $"{prefix} wand of {baseSpell.Name.ToLower()}" : $"{prefix} wand of level {level} {baseSpell.Name.ToLower()}";
            // string desc = "Allows the wielder to cast {i}" + AllSpells.CreateModernSpellTemplate(spellId, Trait.None).ToSpellLink() + "{/i} once per day.";
            Illustration illustration = new WandIllustration(baseSpell.Illustration, Illustrations.Wand);

            return ModManager.RegisterNewItemIntoTheShop($"{(prefix == "darksteel" ? "" : prefix)}WandOf{baseSpell.Name}Lv{level}", itemName => {
                return new Item(itemName, illustration, name, itemLevel, GetWandPrice(itemLevel, type), traits.ToArray())
                .WithItemGreaterGroup(ItemGroupWands)
                .WithItemGroup($"Level {level} {prefix} wands")
                .WithDescription(desc)
                .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning))
                .WithModification(mod);
            });
        }

        public static Item MakeAnimalCompanionItem(Item item, Func<Creature, Creature> animalCompanionFactory, string companionName, string flavourText, bool advanced=false) {
            item
            .WithWornAt(ModTraits.AnimalCompanion)
            .WithItemGroup("Roguelike mode")
            .WithDescription("{i}" + flavourText + "{/i}\n\n" +
            $"If you do not already have a battle ready animal companion, you gain a unique {companionName} to fight at your side, that's otherwise functional identically to the Animal Companion class feat.\n\nIf the {companionName} dies in battle, it cannot aid the party until it has time to heal during the next long rest.")
            .WithPermanentQEffectWhenWorn((qfItem, item) => {
                qfItem.Tag = false;
                qfItem.Innate = false;
                qfItem.StartOfCombat = async self => {
                    Creature? companion = self.Owner.Battle.AllCreatures.FirstOrDefault(cr => cr.FindQEffect(QEffectId.RangersCompanion)?.Source == self.Owner);
                    if (companion != null) {
                        self.Tag = true;
                        return;
                    }
                    if (item.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay) != null) {
                        self.Owner.Overhead("no companion", Color.Green, $"The {companionName} is injured, and won't be able to fight besides the party until after their next long rest or downtime.");
                    } else {
                        self.Id = QEffectId.AnimalCompanionController;

                        Creature animalCompanion = animalCompanionFactory(qfItem.Owner);

                        animalCompanion
                        .WithEntersInitiativeOrder(false)
                        .WithProficiency(Trait.Unarmed, Proficiency.Trained)
                        .WithProficiency(Trait.UnarmoredDefense, Proficiency.Trained)
                        .WithProficiency(Trait.Barding, Proficiency.Trained)
                        .WithProficiency(Trait.Acrobatics, Proficiency.Trained)
                        .WithProficiency(Trait.Athletics, Proficiency.Trained)
                        .WithProficiency(Trait.Perception, advanced ? Proficiency.Expert : Proficiency.Trained)
                        .WithProficiency(Trait.Reflex, advanced ? Proficiency.Expert : Proficiency.Trained)
                        .WithProficiency(Trait.Fortitude, advanced ? Proficiency.Expert : Proficiency.Trained)
                        .WithProficiency(Trait.Will, advanced ? Proficiency.Expert : Proficiency.Trained);

                        if (advanced || qfItem.Owner.HasEffect(QEffectId.MatureAnimalCompanion)) {
                            animalCompanion.Abilities.Set(Ability.Strength, animalCompanion.Abilities.Strength + 1);
                            animalCompanion.Abilities.Set(Ability.Dexterity, animalCompanion.Abilities.Dexterity + 1);
                            animalCompanion.Abilities.Set(Ability.Constitution, animalCompanion.Abilities.Constitution + 1);
                            animalCompanion.Abilities.Set(Ability.Wisdom, animalCompanion.Abilities.Wisdom + 1);

                            if (animalCompanion.UnarmedStrike != null)
                                animalCompanion.UnarmedStrike.WeaponProperties!.DamageDieCount = 2;

                            foreach (QEffect strikeQf in animalCompanion.QEffects.Where(qf => qf.AdditionalUnarmedStrike != null)) {
                                strikeQf.AdditionalUnarmedStrike!.WeaponProperties!.DamageDieCount = 2;
                            }

                            Trait[] animalSkills = [Trait.Stealth, Trait.Survival, Trait.Intimidation];

                            foreach (Trait skill in animalSkills) {
                                if (animalCompanion.Proficiencies.Get(Trait.Stealth) == Proficiency.Trained)
                                    animalCompanion.WithProficiency(Trait.Acrobatics, Proficiency.Expert);
                                else
                                    animalCompanion.WithProficiency(Trait.Acrobatics, Proficiency.Trained);
                            }
                        }

                        animalCompanion.AddQEffect(new QEffect() {
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
                        var bestBarding = self.Owner.CarriedItems.FirstOrDefault(backpackItem => backpackItem.HasTrait(Trait.Barding) && backpackItem.IsWorn);
                        animalCompanion.BaseArmor = bestBarding;
                        animalCompanion.RecalculateArmor();
                        animalCompanion.Defenses.RecalculateFromProficiencies();
                        animalCompanion.Skills.RecalculateFromProficiencies();

                        Action<Creature, Creature> benefitsToCompanion = self.Owner.PersistentCharacterSheet?.Calculated.RangerBenefitsToCompanion;
                        if (benefitsToCompanion != null)
                            benefitsToCompanion(animalCompanion, self.Owner);
                        self.Owner.Battle.SpawnCreature(animalCompanion, self.Owner.OwningFaction, self.Owner.Occupies);
                    }
                };

                qfItem.StateCheck = self => {
                    if (self.Tag is true) {
                        return;
                    }

                    Creature owner = self.Owner;
                    Creature animalCompanion = Ranger.GetAnimalCompanion(owner);

                    bool flag = owner.HasEffect(QEffectId.MatureAnimalCompanion) || advanced;
                    if (animalCompanion != null && flag && GetAnimalCompanionCommandRestriction(self, animalCompanion) == null) {
                        self.Owner.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                            Id = QEffectId.YouShouldTakeYourTurnEvenUnconsciousOrParalyzed
                        });
                    }
                    if (animalCompanion == null || !animalCompanion.Actions.CanTakeActions())
                        return;

                    ActionPossibility fullCommand = new ActionPossibility(new CombatAction(owner, flag ? (Illustration)new SideBySideIllustration(animalCompanion.Illustration, (Illustration)IllustrationName.Action) : animalCompanion.Illustration, "Command your Animal Companion",
                    [Trait.Auditory], "Take 2 actions as your animal companion.\n\nYou can only command your animal companion once per turn.", (Target)Target.Self()
                    .WithAdditionalRestriction(cr => self.UsedThisTurn ? "You already commanded your animal companion this turn." : (string)null)
                    .WithAdditionalRestriction(cr => GetAnimalCompanionCommandRestriction(self, animalCompanion))) {
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
                        .WithAdditionalRestriction(cr => self.UsedThisTurn ? "You already commanded your animal companion this turn." : null))
                        .WithActionCost(0)
                        .WithEffectOnSelf((Func<Creature, Task>)(async caster => {
                            self.UsedThisTurn = true;
                            animalCompanion.AddQEffect(new QEffect(ExpirationCondition.ExpiresAtEndOfYourTurn) {
                                Id = QEffectId.MoveOnYourOwn,
                                PreventTakingAction = ca => !ca.HasTrait(Trait.Move) && !ca.HasTrait(Trait.Strike) && ca.ActionId != ActionId.EndTurn ? "You can only move or make a Strike." : null
                            });
                            await CommonSpellEffects.YourMinionActs(animalCompanion);
                        })), PossibilitySize.Half))
                    });
                };
            });

            return item;
        }

        public static string? GetAnimalCompanionCommandRestriction(QEffect qfRanger, Creature animalCompanion) {
            if (qfRanger.UsedThisTurn) return "You already commanded your animal companion this turn.";
            if (animalCompanion.HasEffect(QEffectId.Paralyzed)) return "Your animal companion is paralyzed.";
            if (animalCompanion.Actions.ActionsLeft == 0 && (animalCompanion.Actions.QuickenedForActions == null || animalCompanion.Actions.UsedQuickenedAction)) return "You animal companion has no actions it could take.";
            return null;
        }

        private static int GetWandPrice(int level, WandType type) {

            if (type == WandType.RELIABLE) {
                var mod = 0.5f;
                switch (level) {
                    case 3:
                        return (int)(60 * mod);
                    case 5:
                        return (int)(160 * mod);
                    case 7:
                        return (int)(360 * mod);
                    case 9:
                        return (int)(700 * mod);
                    case 11:
                        return (int)(1500 * mod);
                    case 13:
                        return (int)(3000 * mod);
                    case 15:
                        return (int)(6500 * mod);
                    case 17:
                        return (int)(15000 * mod);
                    case 19:
                        return (int)(40000 * mod);
                    case 21:
                        return (int)(100000 * mod);
                    default:
                        return 0;
                }
            }

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
                case 11:
                    return (int)(750);
                case 13:
                    return (int)(1500);
                case 15:
                    return (int)(6500 / 2);
                case 17:
                    return (int)(15000 / 2);
                case 19:
                    return (int)(40000 / 2);
                case 21:
                    return (int)(100000 / 2);
                default:
                    return 0;
            }
        }
    }
}