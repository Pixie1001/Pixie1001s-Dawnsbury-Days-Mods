using Dawnsbury.Core.Creatures;
using Dawnsbury.Campaign.Encounters.Evil_from_the_Stars;
using Dawnsbury.Campaign.Encounters;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act1
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class BesetByMinnowsLv3 : NormalEncounter
    {
        public BesetByMinnowsLv3(string filename) : base("Beset by Minnows", filename, null)
        {
            this.Map.Description = CommonEncounterFuncs.DefaultAquoticCombatDesc;
        }

        public override void ModifyCreatureSpawningIntoTheEncounter(Creature creature)
        {
            S4E2OnTheSeabed.AquaticCombatModify(creature);
        }
    }
}
