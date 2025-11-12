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
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.StatBlocks;
using System.Collections.Generic;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Core.Creatures.Parts;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act2 {

    internal class CultistPatrol2 : NormalEncounter {
        public CultistPatrol2(string filename) : base("Echidnadite Patrol", filename) {

        }
    }

}
