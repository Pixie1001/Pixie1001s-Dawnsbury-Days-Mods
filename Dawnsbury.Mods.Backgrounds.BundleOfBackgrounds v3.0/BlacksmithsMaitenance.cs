using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Enumerations;
using System;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Backgrounds.BundleOfBackgrounds {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class BlacksmithsMaitenance {

        public static RuneKind Service = ModManager.RegisterEnumMember<RuneKind>("BoB_Service");

        public static ItemName iBlacksmithsMaintenance = ModManager.RegisterNewItemIntoTheShop("iBlacksmithsMaintenance", itemName => {
            return new Item(itemName, BoBAssets.imgs[BoBAssets.ImageId.BLACKSMITH_MAINTENANCE], "Blacksmith's Maintenance", 0, 0, Trait.Unsellable, Trait.DoNotAddToShop)
            .WithRuneProperties(new RuneProperties("{i}{Gray}well-maintained{/Gray}{/i}", Service,
            "A true smith always keeps their weapons lethally well maintained between battles.",
            "The first hit made with this weapon each encounter deals 1 additional damage of the weapon's primary damage type.\n\n" +
            "{b}Special.{/b} This attachment should only be placed on items in the blacksmith's sheet, else it will be removed after combat. If it disappears, you can respawn it by opening the character's sheet.", wpn => {
                var additionalDamage = ("1", wpn.WeaponProperties!.DamageKind);
                wpn.WeaponProperties!.WithOnTarget(async (action, user, target, result) => {
                    if (result >= CheckResult.Success)
                        wpn.WeaponProperties.AdditionalDamage.Remove(additionalDamage);
                });

                wpn.WeaponProperties.AdditionalDamage.Add(additionalDamage);
            })
            .WithCanBeAppliedTo((rune, weapon) => weapon.WeaponProperties == null ? "Can only be applied to a weapon." : null));
        });

        // Code mostly copied from @SudoProgramming and @Junabell's implementations
        public static void HandleBM() {

            ItemName[] items = [iBlacksmithsMaintenance];

            ModManager.RegisterActionOnEachCharacterSheet(sheet => {
                Action<CalculatedCharacterSheetValues> endCalc = sheet => {
                    var inventories = new Dictionary<int, Inventory>(sheet.Sheet.InventoriesByLevel);
                    inventories.TryAdd(0, sheet.Sheet.CampaignInventory);

                    foreach (var (inventoryLevel, inventory) in inventories) {
                        List<Item?> allItems = [inventory.LeftHand, inventory.RightHand, inventory.Armor, .. inventory.Backpack];

                        List<Item> items = [.. allItems.Where(item => item != null && item.ItemName == iBlacksmithsMaintenance),
                                    .. allItems.Where(item => item != null && item.Runes.Any(rune => rune.ItemName == iBlacksmithsMaintenance))
                                               .Select(item => item?.Runes.Where(rune => rune.ItemName == iBlacksmithsMaintenance).FirstOrDefault())];

                        var owner = sheet.Name;
                        var test = inventory.CanBackpackFit(null, 1);

                        if (inventory.CanBackpackFit(null, 1) && sheet.HasFeat(LoaderV3.ftBlacksmith) && items.Count == 0) {
                            Item newItem = Items.CreateNew(iBlacksmithsMaintenance);
                            AddItem(inventory, newItem);
                        } else if (sheet.HasFeat(LoaderV3.ftBlacksmith) && items.Count > 1) {
                            foreach (Item item in items.ToList()) {
                                if (items.Count > 1)
                                    RemoveItem(inventory, item);
                            }
                        } else if (!sheet.HasFeat(LoaderV3.ftBlacksmith) && items.Count >= 1) {
                                foreach (Item item in items.ToList()) {
                                    RemoveItem(inventory, item);
                                }
                            }
                        }
                };

                sheet.Calculated.AtEndOfRecalculation += endCalc;
            });
        }

        private static void RemoveItem(Inventory inventory, Item item) {
            if (inventory.Backpack.Remove(item)) {

            } else if (inventory.LeftHand?.Runes.Contains(item) ?? false) {
                inventory.LeftHand = RunestoneRules.RecreateWithUnattachedSubitem(inventory.LeftHand, item, true);
            } else if (inventory.RightHand?.Runes.Contains(item) ?? false) {
                inventory.RightHand = RunestoneRules.RecreateWithUnattachedSubitem(inventory.RightHand, item, true);
            } else if (inventory.Armor?.Runes.Contains(item) ?? false) {
                inventory.Armor = RunestoneRules.RecreateWithUnattachedSubitem(inventory.Armor, item, true);
            } else {
                var index = inventory.Backpack.FindIndex(item => item?.Runes.Contains(item) ?? false);
                if (index >= 0) {
                    inventory.Backpack[index] = RunestoneRules.RecreateWithUnattachedSubitem(inventory.Backpack[index]!, item, true);
                }
            }
        }

        private static void AddItem(Inventory inventory, Item newItem) {
            if (inventory.CanBackpackFit(newItem, 0)) {
                inventory.AddAtEndOfBackpack(newItem);
            }
        }

    }
}
