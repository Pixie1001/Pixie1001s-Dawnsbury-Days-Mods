using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Core.Animations.Movement;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Level1EliteEncounter : Encounter
    {
        public Level1EliteEncounter(string name, string filename, List<(Item, string)?>? eliteRewards = null, List<Item>? rewards = null) : base(name, filename, rewards, 0) {
            this.CharacterLevel = 1;
            this.RewardGold = CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.ELITE);
            //if (eliteRewards == null && Rewards.Count == 0) {
            //    CommonEncounterFuncs.SetItemRewards(Rewards, CharacterLevel, EncounterType.ELITE);
            //}

            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                CommonEncounterFuncs.ApplyWeakAdjustments(battle);
                await CommonEncounterFuncs.StandardEncounterSetup(battle);
            });

            // Run cleanup
            this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {
                if (eliteRewards == null) {
                    eliteRewards = [LootTables.RollEliteReward(this.CharacterLevel), LootTables.RollEliteReward(this.CharacterLevel)];
                } else if (eliteRewards.Count == 1) {
                    eliteRewards.Add(LootTables.RollEliteReward(this.CharacterLevel));
                }
                await CommonEncounterFuncs.PresentEliteRewardChoice(battle, eliteRewards);
                await CommonEncounterFuncs.StandardEncounterResolve(battle);
            });
        }

    }
}
