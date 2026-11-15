using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Tiles;
using Dawnsbury.IO;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Tiles {
    public static class ScalingLock {
        public static (string, CustomTileKind, Func<Tile, Encounter?, TileQEffect>) Create() {
            return ("ScalingLock", CustomTileKind.CreatureModifier, (tile, encounter) => new TileQEffect(tile) {
                TileQEffectId = QEffectIds.ScalingLock
            });
        }

    }
}
