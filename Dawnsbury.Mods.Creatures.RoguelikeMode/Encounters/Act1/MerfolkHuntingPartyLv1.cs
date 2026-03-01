using System;
using System.Collections.Generic;
using System.Collections;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Campaign.Encounters.Evil_from_the_Stars;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act1
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MerfolkHuntingPartyLv1 : NormalEncounter
    {
        public MerfolkHuntingPartyLv1(string filename) : base("Merfolk Hunting Party", filename) {
            this.Map.Description = CommonEncounterFuncs.DefaultAquoticCombatDesc;
        }

        public override void ModifyCreatureSpawningIntoTheEncounter(Creature creature) {
            S4E2OnTheSeabed.AquaticCombatModify(creature);
        }
    }
}
