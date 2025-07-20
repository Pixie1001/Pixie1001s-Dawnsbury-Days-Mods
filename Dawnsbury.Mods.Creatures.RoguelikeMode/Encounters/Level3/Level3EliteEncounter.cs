using System;
using System.Collections.Generic;
using System.Collections;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Level3EliteEncounter : Encounter
    {

        public Level3EliteEncounter(string name, string filename, List<(Item, string)>? eliteRewards = null, List<Item>? rewards=null) : base(name, filename, rewards, 0) {
            this.CharacterLevel = 3;
            this.RewardGold = CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.ELITE);
            if (eliteRewards == null && Rewards.Count == 0) {
                CommonEncounterFuncs.SetItemRewards(Rewards, CharacterLevel, EncounterType.ELITE);
            }

            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                CommonEncounterFuncs.ApplyEliteAdjustments(battle);
                await CommonEncounterFuncs.StandardEncounterSetup(battle);
            });

            // Run cleanup
            this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {
                if (eliteRewards != null) {
                    await CommonEncounterFuncs.PresentEliteRewardChoice(battle, eliteRewards);
                }
                await CommonEncounterFuncs.StandardEncounterResolve(battle);
            });
        }

    }
}
