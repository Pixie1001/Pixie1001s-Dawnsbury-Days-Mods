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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Tables {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class LootTables {

        public static List<Creature> Party = new List<Creature>();

        public static void GenerateParty(CampaignState campaign) {
            Party.Clear();
            foreach (AdventurePathHero hero in campaign.Heroes) {
                Party.Add(hero.CharacterSheet.ToCreature(1));
            }
        }

        private static List<Item> CreateSpecialItems() {

            List<Item> items = new List<Item>();

            items.Add(Items.CreateNew(CustomItems.SmokingSword).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.StormHammer).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.HungeringBlade).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.ChillwindBow).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.SpiderChopper).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.Sparkcaster).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.FlashingRapier).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.Widowmaker).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.HungeringBlade).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.SmokingSword).WithModificationPlusOneStriking());
            items.Add(Items.CreateNew(CustomItems.StormHammer).WithModificationPlusOneStriking());
            items.Add(Items.CreateNew(CustomItems.HungeringBlade).WithModificationPlusOneStriking());
            items.Add(Items.CreateNew(CustomItems.ChillwindBow).WithModificationPlusOneStriking());
            items.Add(Items.CreateNew(CustomItems.SpiderChopper).WithModificationPlusOneStriking());
            items.Add(Items.CreateNew(CustomItems.Sparkcaster).WithModificationPlusOneStriking());
            items.Add(Items.CreateNew(CustomItems.FlashingRapier).WithModificationPlusOneStriking());
            items.Add(Items.CreateNew(CustomItems.Widowmaker).WithModificationPlusOneStriking());
            items.Add(Items.CreateNew(CustomItems.HungeringBlade).WithModificationPlusOneStriking());
            items.Add(Items.CreateNew(CustomItems.MaskOfConsumption));
            items.Add(Items.CreateNew(CustomItems.WebwalkerArmour));
            items.Add(Items.CreateNew(CustomItems.DreadPlate));
            items.Add(Items.CreateNew(CustomItems.KrakenMail));
            items.Add(Items.CreateNew(CustomItems.WhisperMail));
            items.Add(Items.CreateNew(CustomItems.RobesOfTheWarWizard));
            items.Add(Items.CreateNew(CustomItems.GreaterRobesOfTheWarWizard));
            items.Add(Items.CreateNew(CustomItems.SceptreOfTheSpider));
            items.Add(Items.CreateNew(CustomItems.DeathDrinkerAmulet));
            items.Add(Items.CreateNew(CustomItems.GreaterDeathDrinkerAmulet));
            items.Add(Items.CreateNew(CustomItems.SpellbanePlate));
            items.Add(Items.CreateNew(CustomItems.ThrowersBandolier).WithModificationRune(ItemName.WeaponPotencyRunestone));
            items.Add(Items.CreateNew(CustomItems.ThrowersBandolier).WithModificationRune(ItemName.WeaponPotencyRunestone).WithModificationRune(ItemName.StrikingRunestone));

            return items;
        }

        // Gold values: 3-4, ???, 1-12, 15-25, 30
        public static Item RollConsumable(Creature character, Func<int, bool> levelRange) {

            List<Item> itemList = Items.ShopItems.Where(item => levelRange(item.Level)).ToList();

            List<Item> general = itemList.Where(item => (item.HasTrait(Trait.Potion) || item.HasTrait(Trait.Elixir)) && levelRange(item.Level)).ToList();
            general = general.Concat(itemList.Where(item => item.HasTrait(Trait.Potion) && item.HasTrait(Trait.Healing) && levelRange(item.Level))).ToList();

            if (character.Spellcasting != null) {
                SpellcastingKind kind = character.Spellcasting.PrimarySpellcastingSource.Kind;
                Trait tradition = character.Spellcasting.PrimarySpellcastingSource.SpellcastingTradition;

                if (kind == SpellcastingKind.Innate || kind == SpellcastingKind.Spontaneous) {
                    List<Item> scrolls = itemList.Where(item => item.HasTrait(Trait.Scroll) && item.HasTrait(tradition) && levelRange(item.Level)).ToList();
                    general = general.Concat(scrolls).ToList();
                }
            }

            if (character.Proficiencies.Get(Trait.Martial) >= Proficiency.Trained || character.Proficiencies.Get(Trait.Bomb) >= Proficiency.Trained) {
                List<Item> bombs = itemList.Where(item => item.HasTrait(Trait.Bomb) && levelRange(item.Level)).ToList();
                general = general.Concat(bombs).ToList();
            }

            return general[R.Next(0, general.Count)];
        }

        public static Item RollScroll(int minLevel, int maxLevel, Func<Item, bool> filter) {

            List<Item> scrolls = Items.ShopItems.Where(item => item.HasTrait(Trait.Scroll) && item.Level >= minLevel && item.Level <= maxLevel).ToList();

            return scrolls[R.Next(0, scrolls.Count)];
        }

        // TODO: Modify this to take a lambda funct to specify the level range
        public static Item RollWeapon(Creature character, Func<int, bool> levelRange) {
            Feat classFeat = character.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft is ClassSelectionFeat);
            string className = "";
            if (classFeat != null) {
                className = classFeat.Name.ToLower();
            }

            List<Item> itemList = Items.ShopItems.Concat(CreateSpecialItems()).Where(item => levelRange(item.Level) && !item.HasTrait(Trait.Consumable) && !item.HasTrait(ModTraits.Wand)).ToList();

            List<Item> weaponTable = itemList.Where(item => (item.HasTrait(Trait.Runestone) && !item.HasTrait(Trait.Abjuration) || item.Name.Contains("handwraps of mighty blows")) && levelRange(item.Level)).ToList();
            // Add extra basic scaling runes
            if (weaponTable.FirstOrDefault(item => item.ItemName == ItemName.WeaponPotencyRunestone) != null) {
                weaponTable.Add(Items.CreateNew(ItemName.WeaponPotencyRunestone));
            }
            if (weaponTable.FirstOrDefault(item => item.ItemName == ItemName.StrikingRunestone) != null) {
                weaponTable.Add(Items.CreateNew(ItemName.StrikingRunestone));
            }

            if (new string[] { "ranger", "fighter", "thaumaturge", "investigator", "inventor", "rogue", "commander", "guardian" }.Contains(className)) {
                // Ranged or melee
                if (character.Abilities.Dexterity > character.Abilities.Strength) {
                    weaponTable = weaponTable.Concat(itemList.Where(item =>
                    ((item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.Name.Contains("Staff Of") && (item.HasTrait(Trait.Finesse) || item.HasTrait(Trait.Ranged)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                } else if (character.Abilities.Strength > character.Abilities.Dexterity) {
                    weaponTable = weaponTable.Concat(itemList.Where(item =>
                    ((item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.Name.Contains("Staff Of") && !item.HasTrait(Trait.Finesse) && !item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                } else {
                    weaponTable = weaponTable.Concat(itemList.Where(item =>
                    ((item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.Name.Contains("Staff Of") && (item.HasTrait(Trait.Simple) || item.HasTrait(Trait.Martial)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                }
            } else if (new string[] { "gunslinger", "envoy", "soldier" }.Contains(className)) {
                // Full ranged
                weaponTable = weaponTable.Concat(itemList.Where(item => ((item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
            } else if (new string[] { "barbarian", "champion", "swashbuckler", "portalist" }.Contains(className)) {
                // Full melee
                if (character.Abilities.Dexterity > character.Abilities.Strength) {
                    weaponTable = weaponTable.Concat(itemList.Where(item =>
                    ((item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.HasTrait(Trait.Simple) && item.HasTrait(Trait.Finesse) && !item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                } else if (character.Abilities.Strength > character.Abilities.Dexterity) {
                    weaponTable = weaponTable.Concat(itemList.Where(item =>
                    ((item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.HasTrait(Trait.Simple) && !item.HasTrait(Trait.Finesse) && !item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                } else {
                    weaponTable = weaponTable.Concat(itemList.Where(item => (item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.HasTrait(Trait.Simple) && !item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                }
            } else if (new string[] { "psychic", "witch", "bard", "sorcerer", "wizard", "cleric", "druid", "oracle" }.Contains(className)) {
                // Full caster
                weaponTable = itemList.Where(item => item.Name.Contains("Staff Of") || item.HasTrait(ModTraits.CasterWeapon)).ToList();
                weaponTable = weaponTable.Concat(Items.ShopItems.Where(item => character.PersistentCharacterSheet.Calculated.SpellTraditionsKnown.ContainsOneOf(item.Traits) && item.HasTrait(ModTraits.Wand) && levelRange(item.Level))).ToList(); // || character.PersistentCharacterSheet.Calculated.SpellTraditionsKnown.ContainsOneOf(item.Traits))
            } else if (new string[] { "monk", "shifter", }.Contains(className)) {
                // unarmed
                weaponTable = itemList.Where(item => item.Name.Contains("handwraps of mighty blows")).ToList();
            } else if (new string[] { "magus", "summoner", }.Contains(className)) {
                // hybrid
                if (character.Abilities.Dexterity > character.Abilities.Strength) {
                    weaponTable = weaponTable.Concat(itemList.Where(item => ((item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && (item.HasTrait(Trait.Finesse) || item.HasTrait(Trait.Ranged)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                } else if (character.Abilities.Strength > character.Abilities.Dexterity) {
                    weaponTable = weaponTable.Concat(itemList.Where(item => ((item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.HasTrait(Trait.Finesse) && !item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                } else {
                    weaponTable = weaponTable.Concat(itemList.Where(item => ((item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                }
                weaponTable = weaponTable.Concat(itemList.Where(item => item.Name.Contains("Staff Of") || item.HasTrait(ModTraits.CasterWeapon))).ToList();
                weaponTable = weaponTable.Concat(Items.ShopItems.Where(item => character.PersistentCharacterSheet.Calculated.SpellTraditionsKnown.ContainsOneOf(item.Traits) && item.HasTrait(ModTraits.Wand) && levelRange(item.Level))).ToList();
            } else if (new string[] { "kineticist", }.Contains(className)) {
                if (character.CarriedItems.Where(i => i.ItemName == ItemName.GateAttenuator).Count() == 0) {
                    weaponTable = itemList.Where(item => item.HasTrait(Trait.Kineticist)).ToList();
                } else {
                    weaponTable = new List<Item>() { RollWearable(character, levelRange) };
                }
                if (weaponTable.Count == 0) {
                    weaponTable.Add(Items.CreateNew(ItemName.WeaponPotencyRunestone));
                }
            }

            var sfGuns = Items.ShopItems.Where(item => item.Name.Contains("stellar canon") || item.Name.Contains("rotolaser") || item.Name.Contains("laser pistol") || item.Name.Contains("flame pistol") || item.Name.Contains("scattergun"));
            if (!new string[] { "envoy", "soldier" }.Contains(className)) {
                weaponTable.RemoveAll(item => sfGuns.Contains(item));
            }

            return weaponTable[R.Next(0, weaponTable.Count)];
        }

        public static Item RollWearable(Creature character, Func<int, bool> levelRange) {
            //Feat classFeat = character.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft is ClassSelectionFeat);
            //string className = "";
            //if (classFeat != null) {
            //    className = classFeat.Name.ToLower();
            //}

            List<Item> itemList = Items.ShopItems.Concat(CreateSpecialItems()).Where(item => levelRange(item.Level) && !item.HasTrait(Trait.Consumable) && item.Price >= 10).ToList();
            List<Item> wearableTable = itemList.Where(item => item.ItemName != ItemName.GateAttenuator && (item.HasTrait(Trait.Runestone) && item.HasTrait(Trait.Abjuration) || (item.HasTrait(Trait.Worn) && !item.Name.Contains("handwraps of mighty blows")) || (item.HasTrait(Trait.Armor) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained))).ToList();

            return wearableTable[R.Next(0, wearableTable.Count)];
        }

    }
}
