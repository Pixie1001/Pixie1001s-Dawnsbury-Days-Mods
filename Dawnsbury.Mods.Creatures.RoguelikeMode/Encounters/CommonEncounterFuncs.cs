using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class CommonEncounterFuncs {

        private static int GetHPAdjustment(int level, bool doubleHP) {
            int val = level > 5 ? (level <= 20 ? 20 : 30) : (level <= 0 ? 5 : (level <= 2 ? 10 : 15));
            if (doubleHP)
                val *= 2;
            return val;
        }

        private static QEffectId GetAdjustmentRank(Creature creature) {
            QEffectId[] ids = new QEffectId[] { QEffectId.Inferior, QEffectId.Weak, QEffectId.Elite, QEffectId.Supreme };
            QEffect? adjustmentEffect = creature.QEffects.FirstOrDefault(qf => ids.Contains(qf.Id));
            if (adjustmentEffect == null) {
                return QEffectId.Unspecified;
            }
            return adjustmentEffect.Id;
        }

        private static void RemoveDifficultyAdjustment(Creature creature) {
            QEffectId[] ids = new QEffectId[] { QEffectId.Inferior, QEffectId.Weak, QEffectId.Elite, QEffectId.Supreme };
            QEffect? adjustmentEffect = creature.QEffects.FirstOrDefault(qf => ids.Contains(qf.Id));
            if (adjustmentEffect == null) {
                return;
            }
            int adjustmentValue = 0;
            int levelAdjustment = 0;
            int hpAdjustment = 0;
            switch (adjustmentEffect.Id) {
                case QEffectId.Inferior:
                    levelAdjustment = 2;
                    adjustmentValue = 3;
                    hpAdjustment = GetHPAdjustment(creature.Level + levelAdjustment, true);
                    break;
                case QEffectId.Weak:
                    levelAdjustment = 1;
                    adjustmentValue = 2;
                    hpAdjustment = GetHPAdjustment(creature.Level + levelAdjustment, false);
                    break;
                case QEffectId.Elite:
                    levelAdjustment = -1;
                    adjustmentValue = -2;
                    hpAdjustment -= GetHPAdjustment(creature.Level + levelAdjustment, false);
                    break;
                case QEffectId.Supreme:
                    levelAdjustment = -2;
                    adjustmentValue = -3;
                    hpAdjustment -= GetHPAdjustment(creature.Level + levelAdjustment, true);
                    break;
                default:
                    break;

            }

            creature.Level += levelAdjustment;
            creature.MaxHP += hpAdjustment;
            creature.Defenses.Set(Defense.AC, creature.Defenses.GetBaseValue(Defense.AC) + adjustmentValue);
            creature.Defenses.Set(Defense.Fortitude, creature.Defenses.GetBaseValue(Defense.Fortitude) + adjustmentValue);
            creature.Defenses.Set(Defense.Reflex, creature.Defenses.GetBaseValue(Defense.Reflex) + adjustmentValue);
            creature.Defenses.Set(Defense.Will, creature.Defenses.GetBaseValue(Defense.Will) + adjustmentValue);
            creature.Perception += adjustmentValue;
            Skill[] array = (Skill[])Enum.GetValues(typeof(Skill));
            foreach (Skill skill in array) {
                int skillVal = creature.Skills.Get(skill);
                if (creature.Skills.IsTrained(skill)) {
                    creature.Skills.Set(skill, skillVal + adjustmentValue);
                }
            }

            var split = creature.MainName.Split(" ");
            creature.MainName = "";
            for (int i = 1; i < split.Count(); i++) {
                creature.MainName += split[i] + (i == split.Count() - 1 ? "" : " ");
            }

            creature.RemoveAllQEffects(qf => qf == adjustmentEffect);
        }
        
        public static void ApplyEliteAdjustments(TBattle battle) {
            foreach (Creature enemy in battle.AllCreatures.Where(cr => cr.OwningFaction.IsEnemy && !cr.QEffects.Any(qf => qf.Id == QEffectIds.Hazard))) {
                QEffectId adj = GetAdjustmentRank(enemy);
                if (adj == QEffectId.Weak) {
                    RemoveDifficultyAdjustment(enemy);
                } else if (adj == QEffectId.Unspecified) {
                    enemy.ApplyEliteAdjustments(false);
                } else if (adj == QEffectId.Elite) {
                    if (enemy.BaseName == "Drow Arcanist") {
                        continue;
                    }
                    RemoveDifficultyAdjustment(enemy);
                    enemy.ApplyEliteAdjustments(true);
                }
            }
            AdjustEvolvableEnemies(battle, false);
        }

        public static void ApplyWeakAdjustments(TBattle battle) {
            foreach (Creature enemy in battle.AllCreatures.Where(cr => cr.OwningFaction.IsEnemy && !cr.QEffects.Any(qf => qf.Id == QEffectIds.Hazard))) {
                QEffectId adj = GetAdjustmentRank(enemy);
                if (adj == QEffectId.Elite) {
                    RemoveDifficultyAdjustment(enemy);
                } else if (adj == QEffectId.Unspecified) {
                    enemy.ApplyWeakAdjustments(false);
                } else if (adj == QEffectId.Weak) {
                    if (enemy.BaseName == "Drow Shadowcaster") {
                        continue;
                    }
                    RemoveDifficultyAdjustment(enemy);
                    enemy.ApplyWeakAdjustments(false, true);
                }
            }
            AdjustEvolvableEnemies(battle, true);
        }

        private static void AdjustEvolvableEnemies(TBattle battle, bool levelDrain) {
            List<Creature> creatures = battle.AllCreatures.Where(cr => cr.OwningFaction.IsEnemy && !cr.QEffects.Any(qf => qf.Id == QEffectIds.Hazard)).ToList();
            for (int i = 0; i < creatures.Count(); i++) {
                QEffectId adj = GetAdjustmentRank(creatures[i]);
                if (levelDrain && adj == QEffectId.Weak) {
                    if (creatures[i].BaseName == "Drow Shadowcaster") {
                        Tile pos = creatures[i].Occupies;
                        Faction faction = creatures[i].OwningFaction;
                        battle.RemoveCreatureFromGame(creatures[i]);
                        battle.SpawnCreature(CreatureList.Creatures[CreatureId.DROW_ARCANIST](battle.Encounter), faction, pos);
                    }
                } else if (!levelDrain && adj == QEffectId.Elite) {
                    if (creatures[i].BaseName == "Drow Arcanist") {
                        Tile pos = creatures[i].Occupies;
                        Faction faction = creatures[i].OwningFaction;
                        battle.RemoveCreatureFromGame(creatures[i]);
                        battle.SpawnCreature(CreatureList.Creatures[CreatureId.DROW_SHADOWCASTER](battle.Encounter), faction, pos);
                    }
                }
            }
        }

        public static int GetGoldReward(int level, ModEnums.EncounterType type) {
            // ~15 gives average recommended gold
            float gold = 10 * (level * 0.7f);
            if (type == ModEnums.EncounterType.ELITE) {
                gold *= 1.5f;
            }
            if (type == ModEnums.EncounterType.BOSS) {
                gold *= 2f;
            }

            return (int) gold;
        }

        public static void SetItemRewards(List<Item> rewards, int level, ModEnums.EncounterType type) {
            Func<int, bool> levelRange = type == EncounterType.NORMAL ? itemLevel => Between(itemLevel, level - 1, level + 1) : itemLevel => Between(itemLevel, level + 1, level + 2);

            if (LootTables.Party?.Count <= 3) {
                return;
            }

            rewards.Add(LootTables.RollConsumable(LootTables.Party[R.Next(0, LootTables.Party.Count())], levelRange));
            int bonusConsumable = R.NextD20();
            if (bonusConsumable >= 18) {
                rewards.Add(LootTables.RollConsumable(LootTables.Party[R.Next(0, LootTables.Party.Count())], levelRange));
            }
            rewards.Add(LootTables.RollWeapon(LootTables.Party[R.Next(0, LootTables.Party.Count())], levelRange));
            int bonusWearable = R.NextD20();
            if (bonusWearable >= 11) {
                rewards.Add(LootTables.RollWearable(LootTables.Party[R.Next(0, LootTables.Party.Count())], levelRange));
            }
        }

        public static async Task StandardEncounterSetup(TBattle battle, ModEnums.EncounterType type=ModEnums.EncounterType.NORMAL) {
            if (battle.CampaignState != null) {
                var treasureDemonEncounters = battle.CampaignState.Tags["TreasureDemonEncounters"].Split(", ");
                bool addTD = false;
                foreach (string index in treasureDemonEncounters) {
                    if (Int32.TryParse(index, out int result) && result != 0 && result == battle.CampaignState.UpcomingEncounterStop.Index) {
                        Faction enemyFaction = battle.AllCreatures.First(cr => cr.OwningFaction.IsEnemy).OwningFaction;
                        Tile freeTile = battle.Map.AllTiles.Where(t => t.IsFree).ToList().GetRandom();
                        Creature td = CreatureList.Creatures[ModEnums.CreatureId.TREASURE_DEMON](battle.Encounter);
                        if (battle.Encounter.CharacterLevel == 1) td.ApplyWeakAdjustments(false);
                        else if (battle.Encounter.CharacterLevel == 3) td.ApplyEliteAdjustments();
                        battle.SpawnCreature(td, enemyFaction, freeTile);
                    }
                }
            }

            //if (battle.CampaignState != null && battle.CampaignState.Tags["TreasureDemonEncounters"].Split battle.CampaignState.CurrentStopIndex ==)
        }

        //public static void SetLootReward(TBattle battle, List<Item> rewards) {
        //    Creature[] party = battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null).ToArray();
        //    rewards.Add(LootTables.RollConsumable(party[R.Next(0, party.Count())]));
        //    int bonusConsumable = R.NextD20();
        //    if (bonusConsumable >= 18) {
        //        rewards.Add(LootTables.RollConsumable(party[R.Next(0, party.Count())]));
        //    }
        //    rewards.Add(LootTables.RollWeapon(party[R.Next(0, party.Count())]));
        //    int bonusWearable = R.NextD20();
        //    if (bonusWearable >= 11) {
        //        rewards.Add(LootTables.RollWearable(party[R.Next(0, party.Count())]));
        //    }
        //}

        public static async Task StandardEncounterResolve(TBattle battle, ModEnums.EncounterType type = ModEnums.EncounterType.NORMAL) {
            //Creature[] party = battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null).ToArray();
            //battle.CampaignState.CommonLoot.Add(LootTables.RollConsumable(party[R.Next(0, party.Count())]));
            //int bonusConsumable = R.NextD20();
            //if (bonusConsumable >= 18) {
            //    battle.CampaignState.CommonLoot.Add(LootTables.RollConsumable(party[R.Next(0, party.Count())]));
            //}
            //battle.CampaignState.CommonLoot.Add(LootTables.RollWeapon(party[R.Next(0, party.Count())]));
            //int bonusWearable = R.NextD20();
            //if (bonusWearable >= 11) {
            //    battle.CampaignState.CommonLoot.Add(LootTables.RollWearable(party[R.Next(0, party.Count())]));
            //}
            await battle.EndTheGame(true, "You defeated all enemies!");
        }

        public static async Task PresentEliteRewardChoice(TBattle battle, List<(Item, string)> options) {
            if (battle.CampaignState == null || battle.CampaignState.AdventurePath == null || battle.CampaignState.AdventurePath.Id != "RoguelikeMode") {
                return;
            }
            await battle.Cinematics.NarratorLineAsync("Searching through the loot, some of your opponent's equipment is still intact.", null);
            battle.Cinematics.ExitCutscene();

            Dictionary<string, (Item, string)> itemOptions = new Dictionary<string, (Item, string)>();
            options.ForEach(item => itemOptions.Add(item.Item1.Name.CapitalizeEachWord(), (item.Item1, item.Item2)));

            string text = "Select your reward:";
            foreach (var option in itemOptions) {
                text += "\n" + "{b}" + option.Key + ".{/b} " + option.Value.Item2;
            }

            Creature looter = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsPlayer);

            ChoiceButtonOption choice = await looter.AskForChoiceAmongButtons(IllustrationName.ChestOpen, text, itemOptions.Select(o => o.Key).ToArray());

            foreach (var option in itemOptions) {
                if (option.Key == choice.Caption) {
                    battle.CampaignState.CommonLoot.Add(option.Value.Item1);
                    if (option.Value.Item1.ItemName == CustomItems.BloodBondAmulet) {
                        battle.CampaignState.CommonLoot.Add(option.Value.Item1);
                    }
                    battle.Encounter.RewardGold += option.Value.Item1.Price;
                }
            }
        }

        internal static bool Between(int value, int lower, int upper) {
            if (value >= lower && value <= upper) {
                return true;
            }
            return false;
        }

    }
}