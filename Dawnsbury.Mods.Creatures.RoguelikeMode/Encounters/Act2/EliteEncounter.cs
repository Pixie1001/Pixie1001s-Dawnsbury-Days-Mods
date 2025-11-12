using System;
using System.Collections.Generic;
using System.Collections;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class EliteEncounter : Encounter {

        internal List<(Item, string)?>? EliteRewards = null;

        public EliteEncounter(string name, string filename, List<(Item, string)?>? eliteRewards = null, List<Item>? rewards = null) : base(name, filename, rewards, 0) {
            this.CharacterLevel = CampaignState.Instance?.CurrentLevel ?? this.Map.Level;
            this.RewardGold = CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.ELITE);
            EliteRewards = eliteRewards;
            if (eliteRewards == null && Rewards.Count == 0) {
                CommonEncounterFuncs.SetItemRewards(Rewards, CharacterLevel, EncounterType.ELITE);
            }

            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                await Setup(battle);
            });

            // Run cleanup
            this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {
                await Cleanup(battle);
            });
        }

        internal async Task Setup(TBattle battle) {
            if (this.CharacterLevel == 3 || this.CharacterLevel == 7)
                CommonEncounterFuncs.ApplyEliteAdjustments(battle);
            else if (this.CharacterLevel == 1 || this.CharacterLevel == 5)
                CommonEncounterFuncs.ApplyWeakAdjustments(battle);
            await CommonEncounterFuncs.StandardEncounterSetup(battle);
        }

        internal async Task Cleanup(TBattle battle) {
            if (EliteRewards == null) {
                EliteRewards = LootTables.RollEliteReward(this.CharacterLevel, 2);
            } else if (EliteRewards.Count == 1) {
                EliteRewards = EliteRewards.Concat(LootTables.RollEliteReward(this.CharacterLevel)).ToList();
            }
            await CommonEncounterFuncs.PresentEliteRewardChoice(battle, EliteRewards);
            await CommonEncounterFuncs.StandardEncounterResolve(battle);
        }

    }
}
