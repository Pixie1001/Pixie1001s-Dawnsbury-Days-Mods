using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class SudoDebugL1 : Level1Encounter
    {
        public SudoDebugL1(string filename) : base("Sudo Debug", filename)
        {
            // Run setup
            this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
                List<Creature> homunculus = battle.AllCreatures.Where(creature => creature.BaseName == "Homunculus").ToList();
                Creature master = battle.AllCreatures.First(creature => creature.BaseName == "Nuglub");
                foreach (Creature creature in homunculus)
                {
                    Homunculus.AddMasterEffect(creature, master);
                }
            });
        }
    }
}
