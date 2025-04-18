using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class GrantMonsterMutator {

        public static (string, Func<Tile, Encounter?, TileQEffect>) Create() {
            return ("MonsterMutator", (tile, encounter) => new TileQEffect(tile) {
                StateCheck = self => {
                    string level = "1";
                    if ((CampaignState.Instance != null && CampaignState.Instance.Tags.TryGetValue("corruption level", out level) && level != "1") || (CampaignState.Instance == null && !PlayerProfile.Instance.IsBooleanOptionEnabled("RL_Corruption2"))) {
                        self.ExpiresAt = ExpirationCondition.Immediately;
                        return;
                    }

                    if (self.Owner.PrimaryOccupant?.Battle.RoundNumber <= 1) {
                        MonsterMutatorTable.RollForMutator(self.Owner.PrimaryOccupant);
                        // self.Owner.PrimaryOccupant.AddQEffect();
                        //self.Owner.PrimaryOccupant.MainName = "Modified " + self.Owner.PrimaryOccupant.MainName;
                        self.ExpiresAt = ExpirationCondition.Immediately;
                    }
                }
            });
        }
    }
}