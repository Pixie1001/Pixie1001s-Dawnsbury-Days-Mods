using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dawnsbury.Campaign.Encounters;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act1
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class AbandonedTempleLv1 : NormalEncounter
    {
        public AbandonedTempleLv1(string filename) : base("Abandoned Temple", filename) {

            // Run setup
            this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
                await Cutscenes.AbandonedTempleCutscene(battle);
            });
        }
    }
}
