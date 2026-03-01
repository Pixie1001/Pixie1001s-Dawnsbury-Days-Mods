using System;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Auxiliary;
using Dawnsbury.Audio;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Text;
using Microsoft.Xna.Framework;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Campaign.Encounters.Evil_from_the_Stars;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level4
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class FeedingFrenzy : NormalEncounter {
        public FeedingFrenzy(string filename) : base("Feeding Frenzy", filename) {
            this.Map.Description = CommonEncounterFuncs.DefaultAquoticCombatDesc;
        }

        public override void ModifyCreatureSpawningIntoTheEncounter(Creature creature) {
            S4E2OnTheSeabed.AquaticCombatModify(creature);
        }

    }
}
