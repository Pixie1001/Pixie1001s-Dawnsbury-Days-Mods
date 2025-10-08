using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Campaign.Path;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class NormalEncounter : Encounter
    {

        public NormalEncounter(string name, string filename, List<Item>? rewards= null) : base(name, filename, rewards, 0)
        {
            this.CharacterLevel = CampaignState.Instance?.CurrentLevel ?? this.Map.Level;
            this.RewardGold = CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.NORMAL);
            if (Rewards.Count == 0) {
                CommonEncounterFuncs.SetItemRewards(Rewards, CharacterLevel, EncounterType.NORMAL);
            }

            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                if (this.CharacterLevel == 3 || this.CharacterLevel == 7)
                    CommonEncounterFuncs.ApplyEliteAdjustments(battle);
                else if (this.CharacterLevel == 1 || this.CharacterLevel == 5)
                    CommonEncounterFuncs.ApplyWeakAdjustments(battle);
                await CommonEncounterFuncs.StandardEncounterSetup(battle);
            });

            // Run cleanup
            this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {
                await CommonEncounterFuncs.StandardEncounterResolve(battle);
            });
        }

    }
}
