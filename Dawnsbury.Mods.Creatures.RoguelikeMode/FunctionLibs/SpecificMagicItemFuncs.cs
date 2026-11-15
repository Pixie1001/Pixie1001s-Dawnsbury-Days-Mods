//using Dawnsbury.Core;
//using Dawnsbury.Core.Mechanics.Enumerations;
//using Dawnsbury.Core.Mechanics.Treasure;
//using Dawnsbury.Modding;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs {
//    internal class SpecificMagicItemFuncs {

//        Item CreateScourgeOfFangs(ItemName itemName, string name, int level, int price) {
//            return Items.CreateNew(ItemName.Whip)
//                .WithBaseCharacteristics(itemName, IllustrationName.Whip, name, level, price, [Trait.Invested, Trait.Magical, Trait.Transmutation, Trait.SpecificMagicItem]);

//            ModManager.RegisterNewItemIntoTheShop("ScourgeOfFangs", itemName => {
//                Item item = new Item(itemName, IllustrationName.Whip, "scourge of fangs", 3, 60,
//                    new Trait[] { Trait.Magical, Trait.SpecificMagicWeapon, Trait.Finesse, Trait.Reach, Trait.Flail, Trait.Trip, Trait.Simple, Trait.Disarm, Trait.VersatileP, Trait.DoNotAddToCampaignShop, ModTraits.Roguelike })
//                .WithMainTrait(Trait.Whip)
//                .WithItemGreaterGroup(ItemGreaterGroup.MeleeMagicWeapons)
//                .WithItemGroup("Roguelike mode")
//                .WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Slashing) {
//                    ItemBonus = 1,
//                }
//                .WithAdditionalDamage("1d6", DamageKind.Mental))
//                .WithDescription("{i}A coiled three pronged whip favoured by drow priestesses. The weapin is constructed from interlocked copper segments that end in the mechanical heads of vicious clacking serpents, that appear possessed of a have a cruel and malevolent intelligence.{/i}\n\n" +
//                "Those that feel their bite are wrackled by incredible pain, suffering an additional 1d6 mental damage.\n\nIn addition, the serpents are eager to assist their wielder, allowing them to attack by willing the snakes to strike using their wisdom modifier.");

//                item.StateCheckWhenWielded = (wielder, weapon) => {
//                    wielder.AddQEffect(new QEffect() {
//                        ExpiresAt = ExpirationCondition.Ephemeral,
//                        BonusToAttackRolls = (self, action, d) => {
//                            int bonus = self.Owner.Abilities.Wisdom - int.Max(self.Owner.Abilities.Strength, self.Owner.Abilities.Dexterity);
//                            if (action != null && action.Item != null && action.Item == weapon && bonus > 0) {
//                                return new Bonus(bonus, BonusType.Untyped, "Wisdom");
//                            }
//                            return null;
//                        }
//                    });
//                };
//                return item;
//            });
//        }
//    }
//}
