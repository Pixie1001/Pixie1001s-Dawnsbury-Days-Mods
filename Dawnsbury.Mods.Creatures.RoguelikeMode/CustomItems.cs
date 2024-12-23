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
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;
using System.IO;
using System.Buffers.Text;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class CustomItems {

        //public static List<ItemName> items = new List<ItemName>();

        public static ItemName ScourgeOfFangs { get; } = ModManager.RegisterNewItemIntoTheShop("ScourgeOfFangs", itemName => {
            Item item = new Item(itemName, IllustrationName.Whip, "scourge of fangs", 3, 100,
                new Trait[] { Trait.Magical, Trait.Finesse, Trait.Reach, Trait.Flail, Trait.Trip, Trait.Simple, Trait.Disarm, Trait.VersatileP, Trait.DoNotAddToShop, Traits.LegendaryItem })
            .WithMainTrait(Trait.Whip)
            .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Slashing) {
                ItemBonus = 1,
            }
            .WithAdditionalDamage("1d6", DamageKind.Mental))
            .WithDescription("A coiled three pronged whip favoured by drow priestesses. The weapin is constructed from interlocked copper segments that end in the mechanical heads of vicious clacking serpents, that appear possessed of a have a cruel and malevolent intelligence. " +
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

        public static ItemName ProtectiveAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("ProtectiveAmulet", itemName => {
            Item item = new Item(itemName, Illustrations.ProtectiveAmulet, "protective amulet", 4, 90, new Trait[] { Trait.Magical, Trait.DoNotAddToShop })
            .WithDescription("An eerie fetish, thrumming with protective magic bestowed by foul and unknowable beings. Though it's intended user has perished, some small measure of the amulet's origional power can still be invoked by holding the amulet aloft.\n\n" +
            "{b}Protective Amulet {icon:Reaction}{/b}.\n\n{b}Trigger{/b} You or an ally within 15-feet would be damaged by an attack.\n{b}Effect{/b} Reduce the damage by an amount equal to your level.");
            
            item.StateCheckWhenWielded = (wielder, weapon) => {
                if (wielder.HasTrait(Traits.Witch)) {
                    QEffect effect = new QEffect("Protective Amulet {icon:Reaction}", "{b}Trigger{/b} You or a member of your coven within 15-feet would be damaged by an attack. {b}Effect{/b} Reduce the damage by an amount equal to 3 + your level.");
                    effect.ExpiresAt = ExpirationCondition.Ephemeral;
                    effect.AddGrantingOfTechnical(cr => cr.HasTrait(Traits.Witch), qf => {
                        qf.YouAreDealtDamage = async (self, a, damage, d) => {
                            if (effect.Owner.DistanceTo(d) > 3) {
                                return null;
                            }

                            if (effect.UseReaction()) {
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
                    QEffect effect = new QEffect("Protective Amulet {icon:Reaction}", "{b}Trigger{/b} You or an ally within 15-feet would be damaged by an attack. {b}Effect{/b} Reduce the damage by an amount equal to your level.");
                    effect.ExpiresAt = ExpirationCondition.Ephemeral;
                    effect.Tag = weapon;
                    effect.EndOfCombat = async (self, won) => {
                        ItemModification? used = weapon.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay);
                        if (used != null) {
                            weapon.ItemModifications.Remove(used);
                        }
                    };
                    effect.AddGrantingOfTechnical(cr => cr.OwningFaction.AlliedFactionOf(effect.Owner.OwningFaction), qf => {
                        qf.YouAreDealtDamage = async (self, a, damage, d) => {
                            if (weapon.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay) != null || effect.Owner.DistanceTo(d) > 3) {
                                return null;
                            }

                            if (await effect.Owner.AskToUseReaction((damage.Power != null ? "{b}" + a.Name + "{/b} uses {b}" + damage.Power.Name + "{/b} on " + "{b}" + qf.Owner.Name + "{/b}" :
                                "{b}" + qf.Owner.Name + "{/b} has been hit") + " for " + damage.Amount + $" damage, which provokes the protective powers of your Protective Amulet.\nUse your reaction reduce the damage by {effect.Owner.Level}?")) {
                                item.WithModification(new ItemModification(ItemModificationKind.UsedThisDay));
                                effect.Owner.Occupies.Overhead("*uses protective amulet*", Color.Black, $"{effect.Owner.Name} holds up their protective amulet to shield {qf.Owner.Name} from harm.");
                                qf.Owner.Occupies.Overhead($"*{effect.Owner.Level} damage negated*", Color.Black);
                                Sfxs.Play(SfxName.Abjuration, 1f);
                                return new ReduceDamageModification(effect.Owner.Level, "Protective Amulet");
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
                new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.VersatileB, Trait.FatalD8, Trait.Reload1, Trait.Crossbow, Trait.Simple, Trait.DoNotAddToShop, Traits.CasterWeapon, Traits.CannotHavePropertyRune })
            .WithMainTrait(Traits.Hexshot)
            .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Piercing).WithRangeIncrement(8))
            .WithDescription("This worn pistol is etched with malevolent purple runes that seem to glow brightly in response to spellcraft, loading the weapon's strange inscribed ammunition with power.\n\n" +
            "After the wielder expends a spell slot, the Hexshot becomes charged, gaining a +2 status bonus to hit, and dealing additional force damage equal to twice the level of the slot used.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect("Hexshot", "After expending a spell slot or using a scroll, your Hexshot becomes charged, gaining a +2 status bonus to hit, and dealing additional force damage equal to twice the level of the slot used.") {
                    ExpiresAt = ExpirationCondition.Ephemeral,
                    AfterYouTakeAction = async (self, spell) => {
                        if (spell != null && (((spell.HasTrait(Trait.Spell) || spell.HasTrait(Trait.Spellstrike)) && !spell.HasTrait(Trait.SustainASpell) && !spell.HasTrait(Trait.Cantrip) && !spell.HasTrait(Trait.Focus)) || (spell.Name.StartsWith("Channel Smite")))) {
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
            return new Item(itemName, new WandIllustration(IllustrationName.ElementFire, IllustrationName.Longsword), "smoking sword", 3, 25,
                new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.Martial, Trait.Sword, Trait.Fire, Trait.VersatileP, Trait.DoNotAddToShop, Traits.CannotHavePropertyRune, Traits.BoostedWeapon
            })
            .WithMainTrait(Trait.Longsword)
            .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing) {
            AdditionalDamageFormula = "1",
                AdditionalDamageKind = DamageKind.Fire
            })
                .WithDescription("Smoke constantly belches from this longsword. Any hit with this sword deals 1 extra fire damage." +
                "You can use a special action while holding the sword to command the blade's edges to light on fire.\n\n{b}Activate {icon:Action}.{/b} concentrate; {b}Effect.{/b} Until the end" +
                " of your turn, the sword deals 1d6 extra fire damage instead of just 1. After you use this action, you can't use it again until the end of the encounter.");
            });

        public static ItemName StormHammer { get; } = ModManager.RegisterNewItemIntoTheShop("Storm Hammer", itemName => {
            return new Item(itemName, new DualIllustration(IllustrationName.ElementAir, IllustrationName.Warhammer), "storm hammer", 3, 25,
                new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.Shove, Trait.Martial, Trait.Hammer, Trait.Electricity, Trait.DoNotAddToShop, Traits.CannotHavePropertyRune, Traits.BoostedWeapon })
            .WithMainTrait(Trait.Warhammer)
            .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Bludgeoning) {
                AdditionalDamageFormula = "1",
                AdditionalDamageKind = DamageKind.Electricity
            })
            .WithDescription("Sparks of crackling electricity arc from this warhammer, and the head thrums with distant thunder. Any hit with this hammer deals 1 extra electricity damage." +
            "You can use a special action while holding the hammer to transform the sparks into lightning bolts.\n\n{b}Activate {icon:Action}.{/b} concentrate; {b}Effect.{/b} Until the end" +
            " of your turn, the hammer deals 1d6 extra electricity damage instead of just 1. After you use this action, you can't use it again until the end of the encounter.");
        });

        public static ItemName ChillwindBow { get; } = ModManager.RegisterNewItemIntoTheShop("Chillwind Bow", itemName => {
            return new Item(itemName, Illustrations.ChillwindBow, "chillwind bow", 3, 25,
                new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.OneHandPlus, Trait.DeadlyD10, Trait.Bow, Trait.Martial, Trait.RogueWeapon, Trait.ElvenWeapon, Trait.Cold, Trait.DoNotAddToShop, Traits.CannotHavePropertyRune, Traits.BoostedWeapon })
            .WithMainTrait(Trait.Shortbow)
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing) {
                AdditionalDamageFormula = "1",
                AdditionalDamageKind = DamageKind.Cold
            }
            .WithRangeIncrement(12))
            .WithDescription("The yew of this bow is cold to the touch, and its arrows pool with fog as they're nocked. Any hit with this bow deals 1 extra cold damage." +
            "You can use a special action while holding the bow to coat the bow in fridgid icy scales.\n\n{b}Activate {icon:Action}.{/b} concentrate; {b}Effect.{/b} Until the end" +
            " of your turn, the bow deals 1d6 extra cold damage instead of just 1. After you use this action, you can't use it again until the end of the encounter.");
        });

        public static ItemName Sparkcaster { get; } = ModManager.RegisterNewItemIntoTheShop("Sparkcaster", itemName => {
            Item item = new Item(itemName, new DualIllustration(IllustrationName.ElementAir, IllustrationName.HeavyCrossbow), "sparkcaster", 3, 25,
                new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.Reload2, Trait.Simple, Trait.Bow, Trait.TwoHanded, Trait.Crossbow, Trait.WizardWeapon, Trait.Electricity, Trait.DoNotAddToShop, Traits.CasterWeapon, Traits.CannotHavePropertyRune })
            .WithMainTrait(Trait.HeavyCrossbow)
            .WithWeaponProperties(new WeaponProperties("1d10", DamageKind.Piercing) {
                AdditionalDamageFormula = "1",
                AdditionalDamageKind = DamageKind.Electricity
            }
            .WithRangeIncrement(24))
            .WithDescription("Sparks of crackling electricity arc from this crossbow. Any hit with this crossbow deals 1 extra electricity damage." +
            "You can use a special action while holding the crossbow to fire a crackling bolt of lightning in a great arc.\n\n{b}Activate {icon:Action}.{/b} concentrate, manipulate; {b}Effect.{/b} Each creature in a 30-foot line " +
            "suffers 2d6 electricity damage, mitigated by a basic Reflex save. After you use this action, you can't use it again until the end of the encounter.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                    ProvideStrikeModifierAsPossibility = item => {
                        if (item != weapon || item.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay) != null || wielder.PersistentCharacterSheet == null || weapon.EphemeralItemProperties.NeedsReload) {
                            return null;
                        }

                        CombatAction action = new CombatAction(wielder, weapon.Illustration, $"Activate {weapon.Name.CapitalizeEachWord()}", new Trait[] { Trait.Concentrate, Trait.Manipulate, Trait.Magical, Trait.Electricity, Trait.Evocation },
                            "{b}Range{/b} 30-foot line\n{b}Saving Throw{/b} Basic Reflex save\n\nEach creature in the line suffers 2d6 electricity damage, mitigated by a basic Reflex saving throw against the wielder's class or spell save DC.", Target.ThirtyFootLine()) {
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

        public static ItemName SpiderChopper { get; } = ModManager.RegisterNewItemIntoTheShop("Spider Chopper", itemName => {
            Item item = new Item(itemName, Illustrations.SpiderChopper, "spider chopper", 3, 25,
                               new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.Sweep, Trait.Martial, Trait.Axe, Trait.DoNotAddToShop, Traits.CannotHavePropertyRune })
            .WithMainTrait(Trait.BattleAxe)
            .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing))
            .WithDescription("An obsidian cleaver, erodated down to a brutal jagged edge by acidic spittle. The weapon seems to shifting and throb when in the presence of spiders.\n\n" +
            "The Spider Chopper deals +1d6 damage slashing damage to spiders.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                    AddExtraKindedDamageOnStrike = (strike, d) => {
                        if (strike == null || strike.Item != weapon) {
                            return null;
                        }

                        if (d.HasTrait(Traits.Spider) || d.Name.ToLower().Contains("spider")) {
                            return new KindedDamage(DiceFormula.FromText("1d4"), DamageKind.Slashing);
                        }
                        return null;
                    }
                });
            };

            return item;
        });

        public static ItemName Widowmaker { get; } = ModManager.RegisterNewItemIntoTheShop("Widowmaker", itemName => {
            Item item = new Item(itemName, Illustrations.Widowmaker, "widowmaker", 3, 25,
                               new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.Agile, Trait.Finesse, Trait.Thrown10Feet, Trait.VersatileS, Trait.Simple, Trait.Knife, Trait.DoNotAddToShop, Traits.CannotHavePropertyRune })
            .WithMainTrait(Trait.Dagger)
            .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Piercing))
            .WithDescription("A wicked looking dagger, with a small hollow at the tip of the blade, from which a steady supply of deadly poison drips.\n\nAttacks made against flat footed creatures using this dagger expose them to spider venom.");

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
                               new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.DeadlyD8, Trait.Disarm, Trait.Finesse, Trait.Martial, Trait.Sword, Trait.DoNotAddToShop, Traits.CannotHavePropertyRune })
            .WithMainTrait(Trait.Rapier)
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing))
            .WithDescription("A brilliant sparkling rapier, that causes the light to bend around its blade in strange prismatic patterns."+
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
                new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.VersatileP, Trait.TwoHanded, Trait.Martial, Trait.Sword, Trait.DoNotAddToShop, Traits.CannotHavePropertyRune })
            .WithMainTrait(Trait.Greatsword)
            .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing))
            .WithDescription("A sinister greatsword made from cruel black steel and an inhospitable grip dotted by jagged spines. No matter how many times the blade is cleaned, it continues to ooze forth a constant trickle of blood.\n\n" +
            "The hungering blade causes its wielder to suffer 1d8 persistent bleed damage at the start of each turn, but deals an additional 1d4 negative damage on a hit. In addition, after felling an enemy with the blade, the wielder gains 5 temporary hit points.");

            item.StateCheckWhenWielded = (wielder, weapon) => {
                wielder.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                    StartOfYourTurn = async (self, owner) => {
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
                new Trait[] { Trait.Magical, Trait.LightArmor, Trait.Leather, Trait.DoNotAddToShop })
            .WithArmorProperties(new ArmorProperties(2, 3, -1, 0, 12))
            .WithDescription("Flexible jetblack armour of elven make, sewn from some exotic underdark silk.\n\nThe wearer of this armour may pass through webs unhindered.");
        });

        public static ItemName DreadPlate { get; } = ModManager.RegisterNewItemIntoTheShop("Dread Plate", itemName => {
            return new Item(itemName, Illustrations.DreadPlate, "dread plate", 3, 80,
                new Trait[] { Trait.Magical, Trait.HeavyArmor, Trait.Bulwark, Trait.Plate, Trait.DoNotAddToShop })
            .WithArmorProperties(new ArmorProperties(6, 0, -3, -2, 18))
            .WithDescription("This cold, black steel suit of plate armour radiates a spiteful presence, feeding off its wearer's own lifeforce to strike at any who would dare mar it.\n\nWhile wearing this cursed armour, you take an additional 1d4 negative damage when damaged by an adjacent creature, but deal 1d6 negative damage in return.");
        });

        public static ItemName DolmanOfVanishing { get; } = ModManager.RegisterNewItemIntoTheShop("Dolman of Vanishing", itemName => {
            return new Item(itemName, Illustrations.DolmanOfVanishing, "dolman of vanishing", 2, 35,
                new Trait[] { Trait.Magical, Trait.Armor, Trait.UnarmoredDefense, Trait.Cloth, Trait.DoNotAddToShop })
            .WithArmorProperties(new ArmorProperties(0, 5, 0, 0, 0))
            .WithDescription("A skyblue robe of gossmer, that seems to evade the beholder's full attention span, no matter how hard they try to focus on it.\n\nThe wearer of this cloak gains a +2 item bonus to stealth and can hide in plain sight..");
        });

        public static ItemName MaskOfConsumption { get; } = ModManager.RegisterNewItemIntoTheShop("Mask of Consumption", itemName => {
            return new Item(itemName, Illustrations.MaskOfConsumption, "mask of consumption", 2, 50,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToShop })
            .WithWornAt(Trait.Mask)
            .WithDescription("This cursed mask fills the wearer with a ghoulish, ravenous hunger for living flesh, alongside a terrible wasting pallor.\n\n" +
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
                        if (strike != null && strike.Owner != null && strike.HasTrait(Trait.Strike) && strike.Item != null && strike.Item.Name == strike.Owner.UnarmedStrike.Name && self.Owner.IsLivingCreature) {
                            await strike.Owner.HealAsync(DiceFormula.FromText($"{damage}", "Mask of Consumption"), strike);
                        }
                    };
                });
            });
        });

        public static ItemName CloakOfAir { get; } = ModManager.RegisterNewItemIntoTheShop("Cloak of Air", itemName => {
            return new Item(itemName, Illustrations.CloakOfAir, "cloak of air", 2, 50,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Evocation, Trait.Elemental, Trait.Air, Trait.DoNotAddToShop })
            .WithWornAt(Trait.Cloak)
            .WithDescription("This swooshing cloak seem to be perpetually billowing in the wind.\n\nThe wear of this cloak may dramatically swoosh it about, to send a blade of air at their enemies for 1d8 damage, 20ft range. In addition, Kineticists wearing this cloak deal +2 damage with their air impulses.")
            .WithItemAction((item, user) => {
                Item weapon = new Item(IllustrationName.AerialBoomerang256, "Slicing Wind", new Trait[] { Trait.Ranged, Trait.Air, Trait.Magical, Trait.Unarmed }).WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing) {
                    VfxStyle = new VfxStyle(5, ProjectileKind.Cone, IllustrationName.AerialBoomerang256),
                    Sfx = SfxName.AeroBlade
                }.WithRangeIncrement(4));
                return user.CreateStrike(weapon);
                //return (ActionPossibility)new CombatAction(user, IllustrationName.AerialBoomerang256, "Cutting Wind", new Trait[] { Trait.Air, Trait.Evocation, Trait.Ranged, Trait.Magical }, Target.RangedCreature(5));
            }, (_, _) => true )
            .WithPermanentQEffectWhenWorn((qfCoA, item) => {
                qfCoA.Innate = true;
                qfCoA.Name = "Cloak of Air";
                qfCoA.Description = "+2 damage to Air impulses.";
                qfCoA.BonusToDamage = (self, action, target) => {
                    if (action != null && action.HasTrait(Trait.Impulse) && action.HasTrait(Trait.Air)) {
                        return new Bonus(2, BonusType.Item, "Coak of Air");
                    }
                    return null;
                };
            });
        });

        public static ItemName BloodBondAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("Blood Bond Amulet", itemName => {
            return new Item(itemName, Illustrations.BloodBondAmulet, "blood bond amulet", 3, 40,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToShop })
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
                    int healAmount = (prevHP - target.HP);
                    await caster.HealAsync(DiceFormula.FromText(healAmount.ToString(), "Activate Blood Bond"), spell);
                })
                ;
            }, (_, _) => true);
        });

        internal static void LoadItems() {

            //items.Add("wand of fireball", CreateWand(SpellId.Fireball, 3));
            //items.Add("wand of bless", CreateWand(SpellId.Fireball, 3));
            //items.Add("wand of bless", CreateWand(SpellId.Fireball, 3));

            List<ItemName> items = new List<ItemName>() { SmokingSword, StormHammer, ChillwindBow, Sparkcaster, HungeringBlade, SpiderChopper, WebwalkerArmour, DreadPlate, Hexshot, ProtectiveAmulet, MaskOfConsumption, FlashingRapier, Widowmaker, DolmanOfVanishing, BloodBondAmulet };

            // Wands
            CreateWand(SpellId.Fireball, null);
            CreateWand(SpellId.Bless, null);
            CreateWand(SpellId.Boneshaker, null);
            CreateWand(SpellId.Fear, null);
            CreateWand(SpellId.Fear, 3);
            CreateWand(SpellId.Blur, null);
            CreateWand(SpellId.MirrorImage, null);
            CreateWand(SpellId.TrueStrike, null);
            CreateWand(SpellId.AcidArrow, null);
            CreateWand(SpellId.Barkskin, null);
            CreateWand(SpellId.MageArmor, null);
            CreateWand(SpellId.Bane, null);
            CreateWand(SpellId.Grease, null);
            CreateWand(SpellId.MagicWeapon, null);
            CreateWand(SpellId.ShockingGrasp, 3);

            // Item QEffects

            // Armour effects
            ModManager.RegisterActionOnEachCreature(creature => {
                if (creature.BaseArmor == null) {
                    return;
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
                            if (weapon.HasTrait(Traits.BoostedWeapon)) {
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
                                            "Until the end of your turn, this weapon deals 1d6 extra elemental damage instead of just 1.\n\nAfter you use this action, you can't do so again for the rest of the encounter.", Target.Self())
                                        .WithActionCost(1)
                                        .WithSoundEffect(SfxName.Abjuration)
                                        .WithEffectOnSelf(user => {
                                            // Effect
                                            weapon.WeaponProperties.AdditionalDamageFormula = "1d6";
                                            weapon.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));

                                            // Show effect
                                            user.AddQEffect(new QEffect($"{weapon.Name.CapitalizeEachWord()} Activated", "Extra elemental damage increased from 1 to 1d6.") {
                                                ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
                                                Tag = weapon,
                                                WhenExpires = (self) => {
                                                    Item weapon = self.Tag as Item;
                                                    weapon.WeaponProperties.AdditionalDamageFormula = "1";
                                                }
                                            });
                                            // Run end of combat cleanup
                                            user.AddQEffect(new QEffect() {
                                                Tag = weapon,
                                                EndOfCombat = async (self, won) => {
                                                    weapon.WeaponProperties.AdditionalDamageFormula = "1";
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
                                if (wand.HasTrait(Traits.Wand)) {
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

                                            CombatAction action = spell.CombatActionSpell;
                                            action.Owner = wandHolder;
                                            action.Item = wand;
                                            action.SpellcastingSource = wandHolder.Spellcasting.Sources.FirstOrDefault(source => action.Traits.Contains(source.SpellcastingTradition));

                                            if (used != null && used.Tag != (object)true) {
                                                action.Illustration = new ScrollIllustration(IllustrationName.Broken, action.Illustration);
                                                action.Name += " (Overcharge)";
                                                action.Description += "\n\n{b}Overcharged.{/b} Overcharging your wand to cast this spell has a 50% chance of permanantly destroying it.";
                                            }

                                            return (ActionPossibility)action;
                                        },
                                        AfterYouTakeAction = async (self, action) => {
                                            if (action.SpellId != SpellId.None && action.Item != null && action.Item == wand) {
                                                ItemModification? used = wand.ItemModifications.FirstOrDefault(mod => mod.Kind == ItemModificationKind.UsedThisDay);

                                                if (used == null) {
                                                    wand.ItemModifications.Add(new ItemModification(ItemModificationKind.UsedThisDay));
                                                } else {
                                                    used.Tag = true;
                                                    (CheckResult, string) result = Checks.RollFlatCheck(10);
                                                    wandHolder.Occupies.Overhead(result.Item1 >= CheckResult.Success ? "Overcharge success!" : $"{wand.Name} was destoyed!", result.Item1 >= CheckResult.Success ? Color.Green : Color.Red, result.Item2);
                                                    if (result.Item1 <= CheckResult.Failure) {
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
            Spell baseSpell = AllSpells.TemplateSpells.GetValueOrDefault<SpellId, Spell>(spellId);

            if (level == null || level < baseSpell.MinimumSpellLevel) {
                level = baseSpell.MinimumSpellLevel;
            }

            ItemModification mod = new ItemModification(ItemModificationKind.CustomPermanent) {
                Tag = baseSpell.Duplicate(null, (int)level, true)
            };

            List<Trait> traits = new List<Trait> { Traits.Wand, Trait.Magical, Trait.Unarmed, Trait.Melee, Trait.SpecificMagicWeapon };

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