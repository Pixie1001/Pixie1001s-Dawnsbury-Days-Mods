using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dawnsbury.Campaign.Encounters;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act1
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class HatcheryLv1 : NormalEncounter {
        public HatcheryLv1(string filename) : base("Hatchery", filename) {

            // Run setup
            this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
                await Cutscenes.HatcheryCutscene(battle);
            });

            this.AddTrigger(TriggerName.InitiativeCountZero, async battle => {
                await Cutscenes.HatcheryCutscene2(battle);
            });
        }

    }
}
