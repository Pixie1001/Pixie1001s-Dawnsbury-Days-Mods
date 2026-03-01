using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Campaign.Encounters.Evil_from_the_Stars;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act1
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class ChosenOfTheKrakenLv1 : NormalEncounter
    {
        public ChosenOfTheKrakenLv1(string filename) : base("Chosen of the Kraken", filename) {
            this.Map.Description = CommonEncounterFuncs.DefaultAquoticCombatDesc;
        }

        public override void ModifyCreatureSpawningIntoTheEncounter(Creature creature) {
            S4E2OnTheSeabed.AquaticCombatModify(creature);
        }
    }
}
