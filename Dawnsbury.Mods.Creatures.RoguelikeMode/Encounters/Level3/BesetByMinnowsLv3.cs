using Dawnsbury.Core.Creatures;
using Dawnsbury.Campaign.Encounters.Evil_from_the_Stars;
using Dawnsbury.Campaign.Encounters;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class BesetByMinnowsLv3 : Encounter
    {
        public BesetByMinnowsLv3(string filename) : base("Beset by Minnows", filename, null, 0)
        {

            this.CharacterLevel = 3;
            this.RewardGold = CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.NORMAL);
            if (Rewards.Count == 0)
            {
                CommonEncounterFuncs.SetItemRewards(Rewards, 3, EncounterType.NORMAL);
            }

            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                await CommonEncounterFuncs.StandardEncounterSetup(battle);
            });

            // Run cleanup
            this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {
                await CommonEncounterFuncs.StandardEncounterResolve(battle);
            });
        }

        public override void ModifyCreatureSpawningIntoTheEncounter(Creature creature)
        {
            S4E2OnTheSeabed.AquaticCombatModify(creature);
        }
    }
}
