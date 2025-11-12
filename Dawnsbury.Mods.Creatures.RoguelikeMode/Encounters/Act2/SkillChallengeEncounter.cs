using System;
using System.Collections.Generic;
using System.Collections;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class SkillChallengeEncounter : Encounter
    {

        public SkillChallengeEncounter(string filename) : base("Skill Challenge", filename, null, 0) {
            this.CharacterLevel = CampaignState.Instance?.CurrentLevel ?? this.Map.Level;

            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                if (battle.CampaignState == null) {
                    await battle.EndTheGame(true, "Sorry. Skill challenges can only be played out in campaign mode.");
                    return;
                }

                await SkillChallengeTables.chosenEvents[battle.CampaignState.UpcomingEncounterStop.Index].Run(this.CharacterLevel, battle);
                await battle.EndTheGame(true, "Skill challenge completed!\nThe party pushes onwards...");
            });
        }

    }
}
