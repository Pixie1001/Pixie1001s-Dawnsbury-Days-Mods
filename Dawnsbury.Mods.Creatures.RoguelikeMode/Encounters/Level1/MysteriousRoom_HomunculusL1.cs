using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MysteriousRoom_HomunculusL1 : Level1Encounter
    {
        public MysteriousRoom_HomunculusL1(string filename) : base("Mysterious Room", filename)
        {
            // Run setup
            this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
                Creature? master = battle.AllCreatures.FirstOrDefault(creature => creature.OwningFaction.IsEnemy && creature.BaseName != "Homunculus");
                foreach (Creature homunculus in battle.AllCreatures.Where(creature => creature.BaseName == "Homunculus"))
                {
                    if (master != null) Homunculus.AddMasterEffect(homunculus, master);
                }
            });
        }
    }
}