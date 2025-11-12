using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using System.Runtime.CompilerServices;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class NormalEncounter : Encounter
    {

        internal bool SetLoot = false;

        public NormalEncounter(string name, string filename, List<Item>? rewards= null) : base(name, filename, rewards, 0)
        {
            this.CharacterLevel = CampaignState.Instance?.CurrentLevel ?? this.Map.Level;
            this.RewardGold = 0;

            if (rewards != null)
                SetLoot = true;

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
            this.RewardGold = CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.NORMAL);
            if (Rewards.Count == 0) {
                CommonEncounterFuncs.SetItemRewards(Rewards, CharacterLevel, EncounterType.NORMAL);
            }

            if (this.CharacterLevel == 3 || this.CharacterLevel == 7)
                CommonEncounterFuncs.ApplyEliteAdjustments(battle);
            else if (this.CharacterLevel == 1 || this.CharacterLevel == 5)
                CommonEncounterFuncs.ApplyWeakAdjustments(battle);
            await CommonEncounterFuncs.StandardEncounterSetup(battle);
        }

        internal async Task Cleanup(TBattle battle) {
            CommonEncounterFuncs.PostFightLoot(battle, this.CharacterLevel, this.SetLoot);
            await CommonEncounterFuncs.StandardEncounterResolve(battle);
        }
    }
}
