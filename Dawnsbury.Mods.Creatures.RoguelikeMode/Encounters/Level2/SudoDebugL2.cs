using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class SudoDebugL2 : Level2Encounter
    {
        public SudoDebugL2(string filename) : base("Sudo Debug", filename)
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
