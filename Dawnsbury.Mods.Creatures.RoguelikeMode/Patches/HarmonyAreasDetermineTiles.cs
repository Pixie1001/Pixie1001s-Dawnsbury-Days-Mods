using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawnsbury.Core;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Phases.Menus;
using Dawnsbury.Phases.Menus.StoryMode;
using HarmonyLib;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using System.Data;
using System.Runtime.CompilerServices;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using static Dawnsbury.Core.Mechanics.Rules.RunestoneRules;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Phases.Ingame;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Dawnsbury.Display;
using Dawnsbury.Audio;
using Dawnsbury.Phases.Popups;
using System.Reflection;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Tiles;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Patches {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    [HarmonyPatch]
    internal class HarmonyAreasDetermineTiles {

        public static MethodBase TargetMethod() {
            // use normal reflection or helper methods in <AccessTools> to find the method/constructor
            // you want to patch and return its MethodInfo/ConstructorInfo
            //
            return Type.GetType("Dawnsbury.Core.Mechanics.Targeting.Areas, Dawnsbury Days")
                    .GetMethod("DetermineTiles", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, new[] {
                        typeof(CloseAreaTarget),
                        typeof(Tile),
                        typeof(Vector2)
                    });
        }

       // TODO: Fix up so riptide will work
        [HarmonyPostfix]
        //[HarmonyPatch("Areas", "DetermineTiles", new Type[] { typeof(CloseAreaTarget), typeof(Tile), typeof(Vector2) })]
        private static void AreasDetermineTilesPatch(ref AreaSelection __result, CloseAreaTarget closeAreaTarget, Tile originTile, Vector2 targetPoint) {
            if (closeAreaTarget.OwnerAction.Owner.CreatureId == CreatureIds.MerfolkBrineBlade && closeAreaTarget is LineAreaTarget) {
                Creature user = closeAreaTarget.OwnerAction.Owner;
                List<Tile> tiles = __result.TargetedTiles.ToList().OrderBy(t => t.DistanceTo(user.Occupies)).ToList();
                List<Tile> keptTiles = new List<Tile>();
                int i = 0;
                for (; i < tiles.Count; i++) {
                    keptTiles.Add(tiles[i]);
                    if (tiles[i].PrimaryOccupant is Creature) {
                        break;
                    }
                }

                for (; i < tiles.Count; i++) {
                    __result.ExcludedTiles.Add(tiles[i]);
                }

                // Add included tiles
                __result.TargetedTiles.Clear();
                foreach (Tile tile in keptTiles) {
                    __result.TargetedTiles.Add(tile);
                }
            }
        }

    }
}
