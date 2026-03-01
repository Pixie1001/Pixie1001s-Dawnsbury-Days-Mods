using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act1
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class CroneOfTheWildsLv1 : EliteEncounter
    {
        public CroneOfTheWildsLv1(string filename) : base("Crone of the Wilds", filename, eliteRewards: new List<(Item, string)?> {
            (Items.CreateNew(CustomItems.ShifterFurs), "A mangy fur cloak, still touched by a linger of Agatha's shapeshifting power."),
            (Items.CreateNew(CustomItems.HornOfTheHunt), "An bone hunting horn, worn around the neck, hewn in the shape of a snarling wolf, that might be blown to summon forth a pack of hunting wolves.")
        }) {
            // Run setup
            this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
                await Cutscenes.CroneOfTheWilds(battle);
            });
        }
    }
}
