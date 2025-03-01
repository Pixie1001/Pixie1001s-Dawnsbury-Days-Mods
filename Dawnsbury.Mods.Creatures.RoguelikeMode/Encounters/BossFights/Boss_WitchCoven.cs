using System;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;


namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Boss_WitchCoven : BossFightEncounter {
        public Boss_WitchCoven(string filename) : base("Witch Coven", filename) {
            // Run setup
            this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
                await Cutscenes.WitchCoven(battle);
            });
        }
    }
}
