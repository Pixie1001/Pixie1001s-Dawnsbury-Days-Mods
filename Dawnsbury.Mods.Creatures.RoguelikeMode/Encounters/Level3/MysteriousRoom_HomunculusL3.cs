using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MysteriousRoom_HomunculusL3 : Level3Encounter
    {
        public MysteriousRoom_HomunculusL3(string filename) : base("Mysterious Room", filename)
        {
            // Run setup
            this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
                var masters = battle.AllCreatures.Where(creature => creature.OwningFaction.IsEnemy && creature.HasTrait(ModTraits.Drow)).ToList();
                var homunculus = battle.AllCreatures.Where(creature => creature.OwningFaction.IsEnemy && creature.CreatureId == CreatureIds.Homunculus).ToList();

                for (int i = 0; i < masters.Count(); i++) {
                    Homunculus.AddMasterEffect(homunculus[i], masters[i]);
                }
            });
        }
    }
}