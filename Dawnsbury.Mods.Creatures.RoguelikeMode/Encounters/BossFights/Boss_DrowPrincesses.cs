using System;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;


namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Boss_DrowPrincesses : BossFightEncounter {
        public Boss_DrowPrincesses(string filename) : base("Drow Princesses", filename) {
            // Run setup
            //this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
            //    await Cutscenes.WitchCoven(battle);
            //});
        }
    }
}
