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
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using System.IO;
using System.Buffers.Text;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using FMOD;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class CustomItems {

        //public static List<ItemName> items = new List<ItemName>();

        public static ItemName DuelingSpear { get; } = ModManager.RegisterNewItemIntoTheShop("DuelingSpear", itemName => new Item(itemName, Illustrations.DuelingSpear, "dueling spear", 0, 2, Trait.Disarm, Trait.Finesse, Trait.Uncommon, Trait.VersatileS, Trait.TwoHanded, Trait.Spear, Trait.Martial)
        .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Piercing)));

        public static ItemName ScourgeOfFangs { get; } = ModManager.RegisterNewItemIntoTheShop("ScourgeOfFangs", itemName => {
            Item item = new Item(itemName, IllustrationName.Whip, "scourge of fangs", 3, 100,
                new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.Finesse, Trait.Reach, Trait.Flail, Trait.Trip, Trait.Simple, Trait.Disarm, Trait.VersatileP, Trait.DoNotAddToShop })
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



        public static ItemName ProtectiveAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("ProtectiveAmulet", itemName => {
            Item item = new Item(itemName, Illustrations.ProtectiveAmulet, "protective amulet", 3, 60, new Trait[] { Trait.Magical, Trait.DoNotAddToShop })
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
                new Trait[] { Trait.Magical, Trait.VersatileB, Trait.FatalD8, Trait.Reload1, Trait.Crossbow, Trait.Simple, Trait.DoNotAddToShop, ModTraits.CasterWeapon, ModTraits.CannotHavePropertyRune })
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
                new Trait[] { Trait.Magical, Trait.Martial, Trait.Sword, Trait.Fire, Trait.VersatileP, Trait.DoNotAddToShop, ModTraits.CannotHavePropertyRune, ModTraits.BoostedWeapon
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
                new Trait[] { Trait.Magical, Trait.Shove, Trait.Martial, Trait.Hammer, Trait.Electricity, Trait.DoNotAddToShop, ModTraits.CannotHavePropertyRune, ModTraits.BoostedWeapon })
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
                new Trait[] { Trait.Magical, Trait.OneHandPlus, Trait.DeadlyD10, Trait.Bow, Trait.Martial, Trait.RogueWeapon, Trait.ElvenWeapon, Trait.Cold, Trait.DoNotAddToShop, ModTraits.CannotHavePropertyRune, ModTraits.BoostedWeapon })
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
                new Trait[] { Trait.Magical, Trait.Reload2, Trait.Simple, Trait.Bow, Trait.TwoHanded, Trait.Crossbow, Trait.WizardWeapon, Trait.Electricity, Trait.DoNotAddToShop, ModTraits.CasterWeapon, ModTraits.CannotHavePropertyRune })
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
                               new Trait[] { Trait.Magical, Trait.Sweep, Trait.Martial, Trait.Axe, Trait.DoNotAddToShop, ModTraits.CannotHavePropertyRune })
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
                               new Trait[] { Trait.Magical, Trait.Agile, Trait.Finesse, Trait.Thrown10Feet, Trait.VersatileS, Trait.Simple, Trait.Knife, Trait.DoNotAddToShop, ModTraits.CannotHavePropertyRune })
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
                               new Trait[] { Trait.Magical, Trait.DeadlyD8, Trait.Disarm, Trait.Finesse, Trait.Martial, Trait.Sword, Trait.DoNotAddToShop, ModTraits.CannotHavePropertyRune })
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
                new Trait[] { Trait.Magical, Trait.VersatileP, Trait.TwoHanded, Trait.Martial, Trait.Sword, Trait.DoNotAddToShop, ModTraits.CannotHavePropertyRune })
            .WithMainTrait(Trait.Greatsword)
            .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing))
            .WithDescription("{i}A sinister greatsword made from cruel black steel and an inhospitable grip dotted by jagged spines. No matter how many times the blade is cleaned, it continues to ooze forth a constant trickle of blood.{/i}\n\n" +
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
            .WithDescription("{i}Flexible jetblack armour of elven make, sewn from some exotic underdark silk.{/i}\n\nThe wearer of this armour may pass through webs unhindered.");
        });

        public static ItemName DreadPlate { get; } = ModManager.RegisterNewItemIntoTheShop("Dread Plate", itemName => {
            return new Item(itemName, Illustrations.DreadPlate, "dread plate", 3, 80,
                new Trait[] { Trait.Magical, Trait.HeavyArmor, Trait.Bulwark, Trait.Plate, Trait.DoNotAddToShop })
            .WithArmorProperties(new ArmorProperties(6, 0, -3, -2, 18))
            .WithDescription("{i}This cold, black steel suit of plate armour radiates a spiteful presence, feeding off its wearer's own lifeforce to strike at any who would dare mar it.{/i}\n\nWhile wearing this cursed armour, you take an additional 1d4 negative damage when damaged by an adjacent creature, but deal 1d6 negative damage in return.");
        });

        public static ItemName DolmanOfVanishing { get; } = ModManager.RegisterNewItemIntoTheShop("Dolman of Vanishing", itemName => {
            return new Item(itemName, Illustrations.DolmanOfVanishing, "dolman of vanishing", 3, 60,
                new Trait[] { Trait.Magical, Trait.Armor, Trait.UnarmoredDefense, Trait.Cloth, Trait.DoNotAddToShop })
            .WithArmorProperties(new ArmorProperties(0, 5, 0, 0, 0))
            .WithDescription("{i}A skyblue robe of gossmer, that seems to evade the beholder's full attention span, no matter how hard they try to focus on it.{/i}\n\nThe wearer of this cloak gains a +2 item bonus to stealth and can hide in plain sight.");
        });

        public static ItemName MaskOfConsumption { get; } = ModManager.RegisterNewItemIntoTheShop("Mask of Consumption", itemName => {
            return new Item(itemName, Illustrations.MaskOfConsumption, "mask of consumption", 2, 30,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToShop })
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
                        if (strike != null && strike.Owner != null && strike.HasTrait(Trait.Strike) && strike.Item != null && strike.Item.Name == strike.Owner.UnarmedStrike.Name && self.Owner.IsLivingCreature) {
                            await strike.Owner.HealAsync(DiceFormula.FromText($"{damage}", "Mask of Consumption"), strike);
                        }
                    };
                });
            });
        });

        public static ItemName SpiritBeaconAmulet { get; } = ModManager.RegisterNewItemIntoTheShop("Spirit Beacon Amulet", itemName => {
            return new Item(itemName, Illustrations.SpiritBeaconAmulet, "spirit beacon amulet", 3, 60,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToShop })
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
                new Trait[] { Trait.Magical, Trait.Worn, Trait.Invested, Trait.Necromancy, Trait.DoNotAddToShop })
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
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Conjuration, Trait.DoNotAddToShop })
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
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Transmutation, Trait.DoNotAddToShop })
            .WithWornAt(Trait.Cloak)
            .WithDescription("{i}This haggard fur cloak is has a musky, feral smell to it and seems to pulsate warmly... as if it were not simply a cloak, but the flesh of a living, breathing thing.{/i}\n\n" +
            "You have a +2 item bonus to Demoralize check made against animals, and gain the benefits of the Intimidating Glare feat.\n\n" +
            "Once per day, as a {icon:FreeAction} action, you may invoke the cloak's magic to assume a random animal form until the start of your next turn.")
            .WithItemAction((item, user) => {
                if (user.FindQEffect(QEffectIds.ShifterFurs) != null) {
                    return null;
                }

                return new CombatAction(user, Illustrations.ShifterFurs, "Activate Shifter Furs", new Trait[] { Trait.Transmutation, Trait.Magical }, "{b}Frequency{/b} once per day\n\n{b}Target{/b} Self\n\nYou assume the form of a random enhanced animal form until the start of your next turn.", Target.Self())
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
                            transform.Description += " Whilst transformed, they cannot cast spells.";
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
            });
        });

        public static ItemName CloakOfAir { get; } = ModManager.RegisterNewItemIntoTheShop("Cloak of Air", itemName => {
            return new Item(itemName, Illustrations.CloakOfAir, "cloak of air", 3, 60,
                new Trait[] { Trait.Magical, Trait.Invested, Trait.Evocation, Trait.Elemental, Trait.Air, Trait.DoNotAddToShop })
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

            List<ItemName> items = new List<ItemName>() { DuelingSpear, DemonBoundRing, ShifterFurs, SmokingSword, StormHammer, ChillwindBow, Sparkcaster, HungeringBlade, SpiderChopper, WebwalkerArmour, DreadPlate, Hexshot, ProtectiveAmulet, MaskOfConsumption, FlashingRapier, Widowmaker, DolmanOfVanishing, BloodBondAmulet };

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
            CreateWand(SpellId.MageArmor, null);
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
                                                        action.Description += "\n\n{b}Overcharged.{/b} Overcharging your wand to cast this spell has a 50% chance of permanantly destroying it.";
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

            List<Trait> traits = new List<Trait> { ModTraits.Wand, Trait.Magical, Trait.Unarmed, Trait.Melee, Trait.SpecificMagicWeapon };

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