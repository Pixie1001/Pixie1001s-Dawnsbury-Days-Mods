using System;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class BossFightEncounter : Encounter {

        public BossFightEncounter(string name, string filename, List<Item>? rewards = null) : base(name, filename, rewards, 0) {
            this.RewardGold = CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.BOSS);
            // Handle boss relics here if I extend the game to level 8

            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                await CommonEncounterFuncs.StandardEncounterSetup(battle);
            });

            // Run cleanup
            this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {
                await CommonEncounterFuncs.StandardEncounterResolve(battle);
            });
        }

    }
}
