using System;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MaidenOfTheLostLv3 : Level3EliteEncounter
    {
        public MaidenOfTheLostLv3(string filename) : base("Maiden of the Lost", filename, eliteRewards: new List<(Item, string)?> {
            (Items.CreateNew(CustomItems.Hexshot).WithModificationPlusOneStriking(), "A worn pistol etched with malevolent purple runes that seem to glow brightly in response to spellcraft."),
            (Items.CreateNew(CustomItems.SpiritBeaconAmulet), "An amulet in the shape of a skull... The witch seems to have been using this to lure restless spirits towards her mausoleum.")
        }) {
            // Run setup
            this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
                await Cutscenes.MaidenOfTheLost(battle);
            });
        }
    }
}
