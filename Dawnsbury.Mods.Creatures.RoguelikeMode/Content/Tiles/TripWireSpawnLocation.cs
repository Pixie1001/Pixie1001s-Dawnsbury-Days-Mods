using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Traps;
using Dawnsbury.Core.Tiles;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class TripWireSpawnLocation {

        private const int DC = 16;

        public static (string, Func<Tile, Encounter?, TileQEffect>) Create() {
            return ("Trip Wire Spawn Location", (tile, encounter) => {
                var trap = CommonTraps.CreateBasicTrap(tile, "Trip Wire", Illustrations.TripWire, DC, $"When a creature enters this tile, a rock falls from the ceiling dealing 3d6 bludgeoning damage vs. a basic Reflex save. On a crital failure, the victim is knocked prone. An adjacent creature can {{icon:TwoActions}} disable the trap with a DC {DC} Thievery check.");
                Task OnTrigger(Creature enterer) => TriggerEffect(enterer);
                CommonTraps.AddDoNotStepHere(trap);
                CommonTraps.AddWhenCreatureEnters(trap, OnTrigger, (enterer) => !enterer.HasEffect(QEffectId.Flying));
                CommonTraps.AddDisableDeviceOption(trap, DC - 2, OnTrigger);

                trap.AfterDamageIsDealtHereAsync = async dmg => { };

                trap.StateCheck += self => {
                    if (self.AfterDamageIsDealtHereAsync == null) return;
                    int seed = CampaignState.Instance != null && CampaignState.Instance.Tags.TryGetValue("seed", out string result) ? Int32.TryParse(result, out int r2) ? r2 : R.Next(1000) : R.Next(1000);
                    seed += CampaignState.Instance?.CurrentStopIndex != null ? CampaignState.Instance.CurrentStopIndex : 0;
                    seed += self.Owner.Battle.Map.AllTiles.IndexOf(self.Owner);

                    Random rand = new Random(seed);

                    if (rand.Next(0, 3) == 0) {
                        self.AfterDamageIsDealtHereAsync = null;
                    } else {
                        self.ExpiresAt = ExpirationCondition.Immediately;
                    }

                };

                return trap;
            });
        }

        private static async Task TriggerEffect(Creature cr) {
            // cr.Occupies.Overhead("*trap triggered*", Color.White, $"Trip Wire triggered by {cr.Name}!");
            Sfxs.Play(SfxName.ElementalBlastEarth);
            var trap = Creature.CreateSimpleCreature("Trip Wire");
            trap.Battle = cr.Battle;
            trap.OwningFaction = cr.Battle.Enemy;
            var ca = CombatAction.CreateSimple(trap, "Falling Rocks", []);
            var result = CommonSpellEffects.RollSavingThrow(cr, ca, Defense.Reflex, DC);
            if (result == CheckResult.CriticalFailure) {
                cr.AddQEffect(QEffect.Prone());
            }

            await CommonSpellEffects.DealBasicDamage(ca, trap, cr, result, DiceFormula.FromText("3d6", "Falling Rocks"), DamageKind.Bludgeoning);
        }
    }
}