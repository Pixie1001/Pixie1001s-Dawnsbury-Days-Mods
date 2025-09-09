using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Level3Encounter : Encounter
    {

        public Level3Encounter(string name, string filename, List<Item>? rewards = null) : base(name, filename, rewards, 0) {
            this.CharacterLevel = 3;
            this.RewardGold = CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.NORMAL);
            if (Rewards.Count == 0) {
                CommonEncounterFuncs.SetItemRewards(Rewards, 3, EncounterType.NORMAL);
            }

            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                CommonEncounterFuncs.ApplyEliteAdjustments(battle);
                await CommonEncounterFuncs.StandardEncounterSetup(battle);
            });

            // Run cleanup
            this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {
                await CommonEncounterFuncs.StandardEncounterResolve(battle);
            });
        }

    }
}
