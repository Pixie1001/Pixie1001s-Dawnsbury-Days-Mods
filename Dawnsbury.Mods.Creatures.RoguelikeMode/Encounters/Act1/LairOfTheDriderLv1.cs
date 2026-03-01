using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act1 {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class LairOfTheDriderLv1 : EliteEncounter {

        public LairOfTheDriderLv1(string filename) : base("Lair of the Drider", filename, eliteRewards: new List<(Item, string)?> {
            (Items.CreateNew(CustomItems.SpideryHalberd), "This cruel halberd's jagged edge drips with poison and the strange spinneret like contraption upon it looks promising"),
            (Items.CreateNew(CustomItems.SpiderHatchling), "A dazed looking spiderling, ready to be imprinted upon a new master.")
        }) { }
    }
}