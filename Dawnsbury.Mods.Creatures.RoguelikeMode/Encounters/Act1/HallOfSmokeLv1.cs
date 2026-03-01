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
    internal class HallOfSmokeLv1 : EliteEncounter {

        public HallOfSmokeLv1(string filename) : base("Hall of Smoke", filename, eliteRewards: new List<(Item, string)?> {
            (Items.CreateNew(CustomItems.CloakOfAir), "A gently billowing cloak that might be used to bend and direct the air itself against one's enemies."),
            (Items.CreateNew(CustomItems.DolmanOfVanishing), "A skyblue robe of gossmer, that seems to evade the beholder's full attention span, no matter how hard they try to focus on it, allowing the wear to hide in even plain sight.")
        }) { }
    }
}