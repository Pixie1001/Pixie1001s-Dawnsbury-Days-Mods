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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class LootTables {

        private static List<Item> CreateSpecialItems() {

            List<Item> items = new List<Item>();

            items.Add(Items.CreateNew(CustomItems.SmokingSword).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.StormHammer).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.HungeringBlade).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.ChillwindBow).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.SpiderChopper).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.Sparkcaster).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.ScourgeOfFangs));
            items.Add(Items.CreateNew(CustomItems.WebwalkerArmour));
            items.Add(Items.CreateNew(CustomItems.DreadPlate));

            return items;
        }

        // Gold values: 3-4, ???, 1-12, 15-25, 30
        public static Item RollConsumable(Creature character) {
            int lvl = character.Level;

            List<Item> itemList = Items.ShopItems.Where(item => lvl - 1 <= item.Level && item.Level <= lvl + 1).ToList();

            List<Item> general = itemList.Where(item => item.HasTrait(Trait.Potion) || item.HasTrait(Trait.Elixir) || item.HasTrait(Trait.Elixir) && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList();

            if (character.Spellcasting != null) {
                SpellcastingKind kind = character.Spellcasting.PrimarySpellcastingSource.Kind;
                Trait tradition = character.Spellcasting.PrimarySpellcastingSource.SpellcastingTradition;

                if (kind == SpellcastingKind.Innate || kind == SpellcastingKind.Spontaneous) {
                    List<Item> scrolls = itemList.Where(item => item.HasTrait(Trait.Scroll) && item.HasTrait(tradition) && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList();
                    general = general.Concat(scrolls).ToList();
                }
            }

            if (character.Proficiencies.Get(Trait.Martial) >= Proficiency.Trained || character.Proficiencies.Get(Trait.Bomb) >= Proficiency.Trained) {
                List<Item> bombs = itemList.Where(item => item.HasTrait(Trait.Bomb) && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList();
                general = general.Concat(bombs).ToList();
            }

            return general[R.Next(0, general.Count)];
        }

        public static Item RollWeapon(Creature character) {
            int lvl = character.Level;
            Feat classFeat = character.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft is ClassSelectionFeat);
            string className = "";
            if (classFeat != null) {
                className = classFeat.Name.ToLower();
            }

            List<Item> itemList = Items.ShopItems.Concat(CreateSpecialItems()).ToList().Where(item => lvl - 1 <= item.Level && item.Level <= lvl + 1 && !item.HasTrait(Trait.Consumable) && !item.HasTrait(Traits.Wand)).ToList();

            List<Item> weaponTable = itemList.Where(item => (item.HasTrait(Trait.Runestone) && !item.HasTrait(Trait.Abjuration)) || item.Name.Contains("handwraps of mighty blows") && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList();
            // Add extra basic scaling runes
            if (weaponTable.FirstOrDefault(item => item.ItemName == ItemName.WeaponPotencyRunestone) != null) {
                weaponTable.Add(Items.CreateNew(ItemName.WeaponPotencyRunestone));
            }
            if (weaponTable.FirstOrDefault(item => item.ItemName == ItemName.StrikingRunestone) != null) {
                weaponTable.Add(Items.CreateNew(ItemName.StrikingRunestone));
            }

            if (new string[] { "ranger", "fighter", "thaumaturge", "investigator", "inventor", "rogue", "commander", "guardian", "portalist" }.Contains(className)) {
                // Ranged or melee
                if (character.Abilities.Dexterity > character.Abilities.Strength) {
                    weaponTable = weaponTable.Concat(itemList.Where(item => item.HasTrait(Trait.SpecificMagicWeapon) && !item.Name.Contains("Staff Of") && (item.HasTrait(Trait.Finesse) || item.HasTrait(Trait.Ranged)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList()).ToList();
                } else if (character.Abilities.Strength > character.Abilities.Dexterity) {
                    weaponTable = weaponTable.Concat(itemList.Where(item => item.HasTrait(Trait.SpecificMagicWeapon) && !item.Name.Contains("Staff Of") && (!item.HasTrait(Trait.Finesse) && !item.HasTrait(Trait.Ranged)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList()).ToList();
                } else {
                    weaponTable = weaponTable.Concat(itemList.Where(item => item.HasTrait(Trait.SpecificMagicWeapon) && !item.Name.Contains("Staff Of") && (item.HasTrait(Trait.Simple) || item.HasTrait(Trait.Martial)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList()).ToList();
                }
            } else if (new string[] { "gunslinger", "envoy", "soldier" }.Contains(className)) {
                // Full ranged
                weaponTable = weaponTable.Concat(itemList.Where(item => item.HasTrait(Trait.SpecificMagicWeapon) && item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
            } else if (new string[] { "barbarian", "champion", "swashbuckler" }.Contains(className)) {
                // Full melee
                if (character.Abilities.Dexterity > character.Abilities.Strength) {
                    weaponTable = weaponTable.Concat(itemList.Where(item => item.HasTrait(Trait.SpecificMagicWeapon) && !item.HasTrait(Trait.Simple) && (item.HasTrait(Trait.Finesse) && !item.HasTrait(Trait.Ranged)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList()).ToList();
                } else if (character.Abilities.Strength > character.Abilities.Dexterity) {
                    weaponTable = weaponTable.Concat(itemList.Where(item => item.HasTrait(Trait.SpecificMagicWeapon) && !item.HasTrait(Trait.Simple) && (!item.HasTrait(Trait.Finesse) && !item.HasTrait(Trait.Ranged)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList()).ToList();
                } else {
                    weaponTable = weaponTable.Concat(itemList.Where(item => item.HasTrait(Trait.SpecificMagicWeapon) && !item.HasTrait(Trait.Simple) && !item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList()).ToList();
                }
            } else if (new string[] { "psychic", "witch", "bard", "sorcerer", "wizard", "cleric", "druid", "oracle" }.Contains(className)) {
                // Full caster
                weaponTable = itemList.Where(item => item.Name.Contains("Staff Of") || (item.HasTrait(Traits.CasterWeapon) || character.PersistentCharacterSheet.Calculated.SpellTraditionsKnown.ContainsOneOf(item.Traits)) && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList();
            } else if (new string[] { "monk", "shifter", }.Contains(className)) {
                // unarmed
                weaponTable = itemList.Where(item => item.Name.Contains("handwraps of mighty blows") && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList();
            } else if (new string[] { "magus", "summoner", }.Contains(className)) {
                // hybrid
                if (character.Abilities.Dexterity > character.Abilities.Strength) {
                    weaponTable = weaponTable.Concat(itemList.Where(item => item.HasTrait(Trait.SpecificMagicWeapon) && (item.HasTrait(Trait.Finesse) || item.HasTrait(Trait.Ranged)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList()).ToList();
                } else if (character.Abilities.Strength > character.Abilities.Dexterity) {
                    weaponTable = weaponTable.Concat(itemList.Where(item => item.HasTrait(Trait.SpecificMagicWeapon) && (!item.HasTrait(Trait.Finesse) && !item.HasTrait(Trait.Ranged)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList()).ToList();
                } else {
                    weaponTable = weaponTable.Concat(itemList.Where(item => item.HasTrait(Trait.SpecificMagicWeapon) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList()).ToList();
                }
                weaponTable = weaponTable.Concat(itemList.Where(item => item.Name.Contains("Staff Of") || (item.HasTrait(Traits.CasterWeapon) || character.PersistentCharacterSheet.Calculated.SpellTraditionsKnown.ContainsOneOf(item.Traits)) && (lvl - 1 <= item.Level && item.Level <= lvl + 1))).ToList();
            } else if (new string[] { "kineticist", }.Contains(className)) {
                if (character.CarriedItems.Where(i => i.ItemName == ItemName.GateAttenuator).Count() == 0) {
                    weaponTable = itemList.Where(item => item.HasTrait(Trait.Kineticist) && (lvl - 1 <= item.Level && item.Level <= lvl + 1)).ToList();
                } else {
                    weaponTable = new List<Item>() { RollWearable(character) };
                }
                if (weaponTable.Count == 0) {
                    weaponTable.Add(Items.CreateNew(ItemName.WeaponPotencyRunestone));
                }
            }

            return weaponTable[R.Next(0, weaponTable.Count)];
        }

        public static Item RollWearable(Creature character) {
            int lvl = character.Level;
            List<Item> itemList = Items.ShopItems.Where(item => lvl - 1 <= item.Level && item.Level <= lvl + 1 && !item.HasTrait(Trait.Consumable) && item.Price >= 10).ToList();
            List<Item> wearableTable = itemList.Where(item => (item.HasTrait(Trait.Runestone) && item.HasTrait(Trait.Abjuration)) || item.HasTrait(Trait.Worn) && !item.Name.Contains("handwraps of mighty blows") || item.HasTrait(Trait.Armor)).ToList();

            return wearableTable[R.Next(0, wearableTable.Count)];
        }

    }
}
