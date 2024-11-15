using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters {

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

            // Inferor/ supreme double HP md
            // 

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

            creature.RemoveAllQEffects(qf => qf == adjustmentEffect);
        }
        
        public static void ApplyEliteAdjustments(TBattle battle) {
            //for (int i = 0; i < battle.AllCreatures.Count; i++) {
            //    if (!battle.AllCreatures[i].OwningFaction.IsEnemy) {
            //        continue;
            //    }
            //    QEffectId adj = GetAdjustmentRank(battle.AllCreatures[i]);
            //    if (adj == QEffectId.Weak) {
            //        RemoveDifficultyAdjustment(battle.AllCreatures[i]);
            //    } else if (adj == QEffectId.Unspecified) {
            //        battle.AllCreatures[i].ApplyEliteAdjustments(false);
            //    } else if (adj == QEffectId.Elite) {
            //        RemoveDifficultyAdjustment(battle.AllCreatures[i]);
            //        if (battle.AllCreatures[i].BaseName == "Drow Arcanist") {
            //            battle.AllCreatures[i] = CreatureList.Creatures[ModEnums.CreatureId.DROW_SHADOWCASTER](battle.Encounter);
            //        }
            //        battle.AllCreatures[i].ApplyEliteAdjustments(true);
            //    }

            //}

            // TODO: Figure out how to dynamically replace high level arcanists with shadowcasters.

            foreach (Creature enemy in battle.AllCreatures.Where(cr => cr.OwningFaction.IsEnemy)) {
                QEffectId adj = GetAdjustmentRank(enemy);
                if (adj == QEffectId.Weak) {
                    RemoveDifficultyAdjustment(enemy);
                } else if (adj == QEffectId.Unspecified) {
                    enemy.ApplyEliteAdjustments(false);
                } else if (adj == QEffectId.Elite) {
                    RemoveDifficultyAdjustment(enemy);
                    enemy.ApplyEliteAdjustments(true);
                }
            }
        }

        public static void ApplyWeakAdjustments(TBattle battle) {
            foreach (Creature enemy in battle.AllCreatures.Where(cr => cr.OwningFaction.IsEnemy)) {
                QEffectId adj = GetAdjustmentRank(enemy);
                if (adj == QEffectId.Elite) {
                    RemoveDifficultyAdjustment(enemy);
                } else if (adj == QEffectId.Unspecified) {
                    enemy.ApplyWeakAdjustments(false);
                } else if (adj == QEffectId.Weak) {
                    RemoveDifficultyAdjustment(enemy);
                    enemy.ApplyWeakAdjustments(true);
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

        public static async Task StandardEncounterSetup(TBattle battle, ModEnums.EncounterType type=ModEnums.EncounterType.NORMAL) {
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
            Creature[] party = battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null).ToArray();
            battle.CampaignState.CommonLoot.Add(LootTables.RollConsumable(party[R.Next(0, party.Count())]));
            int bonusConsumable = R.NextD20();
            if (bonusConsumable >= 18) {
                battle.CampaignState.CommonLoot.Add(LootTables.RollConsumable(party[R.Next(0, party.Count())]));
            }
            battle.CampaignState.CommonLoot.Add(LootTables.RollWeapon(party[R.Next(0, party.Count())]));
            int bonusWearable = R.NextD20();
            if (bonusWearable >= 11) {
                battle.CampaignState.CommonLoot.Add(LootTables.RollWearable(party[R.Next(0, party.Count())]));
            }
            await battle.EndTheGame(true, "You defeated all enemies!");
        }

        public static async Task PresentEliteRewardChoice(TBattle battle) {
            if (battle.CampaignState == null || battle.CampaignState.AdventurePath == null || battle.CampaignState.AdventurePath.Id != "RoguelikeMode") {
                return;
            }

            battle.Cinematics.ExitCutscene();

            // TODO: Hook up loot tables
            Item option1 = Items.CreateNew(ItemName.NecklaceOfFireballsI);
            Item option2 = Items.CreateNew(ItemName.BarbariansGloves);
            Item option3 = Items.CreateNew(ItemName.StrikingRunestone);

            Dictionary<string, (Item, string)> itemOptions = new Dictionary<string, (Item, string)>();

            // TODO: Replace this with a loot table look up thing
            itemOptions.Add(option1.Name, (option1, "A necklace with flickering rubies that can be thrown to produce fiery explosions. (DC 21; 4d6 x2, 6d6 x1)"));
            itemOptions.Add(option2.Name, (option2, "These might gloves grant the wearer a +1 bonus to athletics, and can be activated {icon:TwoActions} to remove the fatigued condition and heal the wearer for 3d8 HP."));
            itemOptions.Add(option3.Name, (option3, "A powerful rune that upgrades the damage of a weapon."));

            string text = "Select your reward:";
            foreach (var option in itemOptions) {
                text += "\n" + "{b}" + option.Key + ".{/b} " + option.Value.Item2;
            }

            Creature looter = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsHumanControlled);

            ChoiceButtonOption choice = await looter.AskForChoiceAmongButtons(IllustrationName.GoldPouch, text, itemOptions.Select(o => o.Key).ToArray());

            foreach (var option in itemOptions) {
                if (option.Key == choice.Caption) {
                    battle.CampaignState.CommonLoot.Add(option.Value.Item1);
                }
            }
        }

    }
}