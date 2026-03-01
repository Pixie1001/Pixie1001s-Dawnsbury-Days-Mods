using System;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act1
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MotherOfThePoolLv1 : EliteEncounter
    {
        public MotherOfThePoolLv1(string filename) : base("Mother of the Pool", filename, eliteRewards: new List<(Item, string)?> {
            (Items.CreateNew(CustomItems.DemonBoundRing), "A sinister ring carved with the face of a gruesome looking fiend. Invoke its power at your own risk."),
            (Items.CreateNew(CustomItems.ProtectiveAmulet), "An eerie fetish intended to be held in the off hand, thrumming with protective magic bestowed by foul and unknowable beings, that might be held aloft to protect others from harm.")
        }) {
            // Run setup
            this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
                await Cutscenes.MotherOfThePool(battle);
            });
        }
    }
}
