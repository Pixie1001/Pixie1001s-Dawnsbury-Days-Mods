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
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Display;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;

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
            items.Add(Items.CreateNew(CustomItems.VipersSpit).WithModificationPlusOne());
            items.Add(Items.CreateNew(CustomItems.VipersSpit).WithModificationPlusOneStriking());
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
            items.Add(Items.CreateNew(CustomItems.RingOfMonsters));
            items.Add(Items.CreateNew(CustomItems.ChillingDemise));
            items.Add(Items.CreateNew(CustomItems.Glimmer));

            return items;
        }

        // Gold values: 3-4, ???, 1-12, 15-25, 30
        public static Item RollConsumable(Creature character, Func<int, bool> levelRange) {

            List<Item> itemList = Items.ShopItems.Where(item => levelRange(item.Level) && !item.HasTrait(Trait.DoNotAddToCampaignShop)).ToList();

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
            Feat classFeat = character.PersistentCharacterSheet?.Calculated.AllFeats.FirstOrDefault(ft => ft is ClassSelectionFeat);
            string className = "";
            if (classFeat != null) {
                className = classFeat.Name.ToLower();
            }

            List<Item> itemList = Items.ShopItems.Where(item => !item.HasTrait(Trait.DoNotAddToCampaignShop)).Concat(CreateSpecialItems()).Where(item => levelRange(item.Level) && !item.HasTrait(Trait.Consumable) && !item.HasTrait(ModTraits.Wand)).ToList();

            List<Item> weaponTable = itemList.Where(item => (item.Price >= 10 && item.HasTrait(Trait.Runestone) && !item.HasTrait(Trait.Abjuration) || (item.Name.Contains("handwraps of mighty blows") && item.Runes.Count > 0)) && levelRange(item.Level)).ToList();
            // Add extra basic scaling runes
            if (weaponTable.FirstOrDefault(item => item.ItemName == ItemName.WeaponPotencyRunestone) != null) {
                weaponTable.Add(Items.CreateNew(ItemName.WeaponPotencyRunestone));
            }
            if (weaponTable.FirstOrDefault(item => item.ItemName == ItemName.StrikingRunestone) != null) {
                weaponTable.Add(Items.CreateNew(ItemName.StrikingRunestone));
            }

            if (new string[] { "ranger", "fighter", "thaumaturge", "investigator", "inventor", "rogue", "commander", "guardian", "runesmith", "exemplar", "operative" }.Contains(className)) {
                // Ranged or melee
                if (character.Abilities.Dexterity > character.Abilities.Strength) {
                    weaponTable = weaponTable.Concat(itemList.Where(item =>
                    ((item.HasTrait(Trait.Weapon) && item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.Name.Contains("Staff Of") && (item.HasTrait(Trait.Finesse) || item.HasTrait(Trait.Ranged)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                } else if (character.Abilities.Strength > character.Abilities.Dexterity) {
                    weaponTable = weaponTable.Concat(itemList.Where(item =>
                    ((item.HasTrait(Trait.Weapon) && item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.Name.Contains("Staff Of") && !item.HasTrait(Trait.Finesse) && !item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                } else {
                    weaponTable = weaponTable.Concat(itemList.Where(item =>
                    ((item.HasTrait(Trait.Weapon) && item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.Name.Contains("Staff Of") && (item.HasTrait(Trait.Simple) || item.HasTrait(Trait.Martial)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                }
            } else if (new string[] { "gunslinger", "envoy", "soldier", "mechanic" }.Contains(className)) {
                // Full ranged
                weaponTable = weaponTable.Concat(itemList.Where(item => ((item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
            } else if (new string[] { "barbarian", "champion", "swashbuckler", "portalist", "solarian" }.Contains(className)) {
                // Full melee
                if (character.Abilities.Dexterity > character.Abilities.Strength) {
                    weaponTable = weaponTable.Concat(itemList.Where(item =>
                    ((item.HasTrait(Trait.Weapon) && item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.HasTrait(Trait.Simple) && item.HasTrait(Trait.Finesse) && !item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                } else if (character.Abilities.Strength > character.Abilities.Dexterity) {
                    weaponTable = weaponTable.Concat(itemList.Where(item =>
                    ((item.HasTrait(Trait.Weapon) && item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.HasTrait(Trait.Simple) && !item.HasTrait(Trait.Finesse) && !item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                } else {
                    weaponTable = weaponTable.Concat(itemList.Where(item => (item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.HasTrait(Trait.Simple) && !item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                }
            } else if (new string[] { "psychic", "witch", "bard", "sorcerer", "wizard", "cleric", "druid", "oracle", "necromancer", "animist", "witchwarper" }.Contains(className)) {
                // Full caster
                weaponTable = itemList.Where(item => item.Name.Contains("Staff Of") || item.HasTrait(ModTraits.CasterWeapon)).ToList();
                weaponTable = weaponTable.Concat(Items.ShopItems.Where(item => character.PersistentCharacterSheet!.Calculated.SpellTraditionsKnown.ContainsOneOf(item.Traits) && item.HasTrait(ModTraits.Wand) && item.HasTrait(ModTraits.Darksteel) && levelRange(item.Level))).ToList(); // || character.PersistentCharacterSheet.Calculated.SpellTraditionsKnown.ContainsOneOf(item.Traits))
                weaponTable = weaponTable.Concat(itemList.Where(item => item.Traits.Any(tr => tr.HumanizeLowerCase2() == "spellheart"))).ToList();
            } else if (new string[] { "monk", "shifter", }.Contains(className)) {
                // unarmed
                weaponTable = itemList.Where(item => item.Name.Contains("handwraps of mighty blows")).ToList();
            } else if (new string[] { "magus", "summoner", "technomancer",  }.Contains(className)) {
                // hybrid
                if (character.Abilities.Dexterity > character.Abilities.Strength) {
                    weaponTable = weaponTable.Concat(itemList.Where(item => ((item.HasTrait(Trait.Weapon) && item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && (item.HasTrait(Trait.Finesse) || item.HasTrait(Trait.Ranged)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                } else if (character.Abilities.Strength > character.Abilities.Dexterity) {
                    weaponTable = weaponTable.Concat(itemList.Where(item => ((item.HasTrait(Trait.Weapon) && item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && !item.HasTrait(Trait.Finesse) && !item.HasTrait(Trait.Ranged) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                } else {
                    weaponTable = weaponTable.Concat(itemList.Where(item => ((item.HasTrait(Trait.Weapon) && item.Price >= 35 && item.Runes.Count == 0) || item.HasTrait(Trait.SpecificMagicWeapon) || item.HasTrait(ModTraits.CannotHavePropertyRune)) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained).ToList()).ToList();
                }
                weaponTable = weaponTable.Concat(itemList.Where(item => item.Name.Contains("Staff Of") || item.HasTrait(ModTraits.CasterWeapon))).ToList();
                weaponTable = weaponTable.Concat(Items.ShopItems.Where(item => character.PersistentCharacterSheet!.Calculated.SpellTraditionsKnown.ContainsOneOf(item.Traits) && item.HasTrait(ModTraits.Wand) && item.HasTrait(ModTraits.Darksteel) && levelRange(item.Level))).ToList();
                weaponTable = weaponTable.Concat(itemList.Where(item => item.Traits.Any(tr => tr.HumanizeLowerCase2() == "spellheart"))).ToList();
            } else if (new string[] { "kineticist", }.Contains(className)) {
                if (!character.CarriedItems.Any(i => i.ItemName == ItemName.GateAttenuator)) {
                    weaponTable = itemList.Where(item => item.HasTrait(Trait.Kineticist)).ToList();
                } else {
                    weaponTable = new List<Item>() { RollWearable(character, levelRange) };
                }
                if (weaponTable.Count == 0) {
                    weaponTable.Add(Items.CreateNew(ItemName.WeaponPotencyRunestone));
                }
            }
            else if (new string[] { "alchemist", }.Contains(className))
            {
                weaponTable = new List<Item>() { RollWearable(character, levelRange) };
                if (weaponTable.Count == 0)
                {
                    weaponTable.Add(Items.CreateNew(ItemName.WeaponPotencyRunestone));
                }
            }

            // Add banners
            if (className == "commander") {
                weaponTable.AddRange(itemList.Where(itm => itm.Traits.Any(tr => tr.HumanizeLowerCase2() == "magical banner")));
            }

            var sfGuns = Items.ShopItems.Where(item => item.Name.ToLower().Contains("stellar canon") || item.Name.ToLower().Contains("rotolaser") || item.Name.ToLower().Contains("laser pistol") || item.Name.ToLower().Contains("flame pistol") || item.Name.ToLower().Contains("scattergun"));
            if (!new string[] { "envoy", "soldier" }.Contains(className)) {
                weaponTable.RemoveAll(item => sfGuns.Contains(item));
            }

            // Fail safe to prevent crash if list is empty
            if (weaponTable.Count == 0) {
                weaponTable.Add(Items.CreateNew(ItemName.ArmorPotencyRunestone));
            }

            return UtilityFunctions.ChooseAtRandom(weaponTable)!;
        }

        public static Item RollWearable(Creature character, Func<int, bool> levelRange) {
            //Feat classFeat = character.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft is ClassSelectionFeat);
            //string className = "";
            //if (classFeat != null) {
            //    className = classFeat.Name.ToLower();
            //}

            List<Item> itemList = Items.ShopItems.Where(item => !item.HasTrait(Trait.DoNotAddToCampaignShop)).Concat(CreateSpecialItems()).Where(item => levelRange(item.Level) && !item.HasTrait(Trait.Consumable) && item.Price >= 10 ).ToList();
            List<Item> wearableTable = itemList.Where(item => item.ItemName != ItemName.GateAttenuator && (item.HasTrait(Trait.Runestone) && item.HasTrait(Trait.Abjuration) || (item.HasTrait(Trait.Worn) && !item.Name.Contains("handwraps of mighty blows")) || (item.HasTrait(Trait.Armor) && character.Proficiencies.Get(item.Traits) >= Proficiency.Trained))).ToList();

            return wearableTable[R.Next(0, wearableTable.Count)];
        }

        public static List<ValueTuple<Item, string>?> RollEliteReward(int level, int amount=1) {
            List<(Item, string)> rewards = null;

            if (level <= 4) {
                rewards = new List<(Item, string)>() {
                    // e.g. A worn pistol etched with malevolent purple runes that seem to glow brightly in response to spellcraft.
                    new ValueTuple<Item, string>(Items.CreateNew(CustomItems.CompanionBunny), "Your adversary appears to have been attempted this keep this little guy as a pet. Perhaps you'd do a better job?"),
                    new ValueTuple<Item, string>(Items.CreateNew(CustomItems.DuergarSkullShield), "An infamous duergar skull shield, used to strike fear into their legion's enemies."),
                    new ValueTuple<Item, string>(Items.CreateNew(CustomItems.BottomlessFlask), "A sweet curitive substance seeps over the edge of this flask from some unknown reservoir."),
                    new ValueTuple<Item, string>(Items.CreateNew(CustomItems.MaskOfSkills), "This flamboyant mask seems possessed of great talent."),
                    new ValueTuple<Item, string>(Items.CreateNew(CustomItems.RodOfHealing), "A resplendent rod, brimming with positive energy."),
                    new ValueTuple<Item, string>(Items.CreateNew(CustomItems.RingOfDeathDefiance), "A strange, eerily cold ring that promises a second chance at victory."),
                };
            } else if (level <= 8) {
                rewards = new List<(Item, string)>() {
                    // e.g. A worn pistol etched with malevolent purple runes that seem to glow brightly in response to spellcraft.
                    new ValueTuple<Item, string>(Items.CreateNew(CustomItems.LunaRunestone), "A glimmering runestone, confiscated from a renegade drow follower of the Cerulean Sky."),
                    new ValueTuple<Item, string>(Items.CreateNew(CustomItems.GreaterCompanionBunny), "Your adversary appears to have been attempted this keep this little guy as a pet. Perhaps you'd do a better job?"),
                    new ValueTuple<Item, string>(Items.CreateNew(ItemName.GreaterBackfireMantle), "A protective mantle to shield you from your allies' spells."),
                    new ValueTuple<Item, string>(Items.CreateNew(ItemName.GlovesOfStoring), "An enchanted glove, used to secret away potions for emergency use."),
                    new ValueTuple<Item, string>(Items.CreateNew(ItemName.LoversGloves), "The glove's previous owners tragically had nobody they cared enough about to invoke their power."),
                    new ValueTuple<Item, string>(Items.CreateNew(CustomItems.Glimmer), "A glimmering scimitar, forged by the elves of old and said to be part of a matching set wielded by a legendary drow renegade."),
                    new ValueTuple<Item, string>(Items.CreateNew(CustomItems.ChillingDemise), "A frigid scimitar, imbued with the power of a blue dragon and said to be part of a matching set wielded by a legendary drow renegade.")
                };
            }

            // Populate highl evel rewards here
            List<ValueTuple<Item, string>?> output = [];

            if (rewards == null)
                return output;

            for (int i = 0; i < Math.Min(amount, rewards.Count); i++) {
                output.Add(UtilityFunctions.ChooseAtRandom(rewards.Where(entry => !output.Contains(entry)).ToList()));
            }

            foreach (var reward in output) {
                if (reward == default(ValueTuple<Item, string>)) continue;

                if (reward!.Value.Item1.HasTrait(ModTraits.CannotHavePropertyRune)) {
                    if (level <= 2)
                        reward.Value.Item1.WithModificationRune(ItemName.WeaponPotencyRunestone);
                    if (level <= 4)
                        reward.Value.Item1.WithModificationRune(ItemName.StrikingRunestone);
                }
            }
            

            return output;
        }

    }
}
