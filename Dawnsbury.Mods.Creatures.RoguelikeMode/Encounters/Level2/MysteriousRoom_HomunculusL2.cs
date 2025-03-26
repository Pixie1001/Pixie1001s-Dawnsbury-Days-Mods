using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using System.Diagnostics.Metrics;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MysteriousRoom_HomunculusL2 : Level2Encounter
    {
        public MysteriousRoom_HomunculusL2(string filename) : base("Mysterious Room", filename)
        {
            // Run setup
            this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
                var masters = battle.AllCreatures.Where(creature => creature.OwningFaction.IsEnemy && creature.HasTrait(ModTraits.Drow)).ToList();
                var homunculus = battle.AllCreatures.Where(creature => creature.OwningFaction.IsEnemy && creature.BaseName == "Homunculus").ToList();

                for (int i = 0; i < masters.Count(); i++) {
                    Homunculus.AddMasterEffect(homunculus[i], masters[i]);
                }
            });
        }
    }
}