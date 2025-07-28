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
    public static class AcidTrapSpawnLocation {

        private const int DC = 16;

        public static (string, Func<Tile, Encounter?, TileQEffect>) Create() {
            return ("Trip Wire Spawn Location", (tile, encounter) => {
                var trap = CommonTraps.CreateBasicTrap(tile, "Acidic Spray Trap", IllustrationName.TrapOfAcidicSpray, 20, "When a creature steps on the pressure plate, the trap casts a 3rd-level {i}acid arrow{/i} on the creature (spell attack +12; 3d8 acid damage). An adjacent creature can {icon:TwoActions} disable the trap with a DC 18 Thievery check.");

                async Task SendAcidArrowAgainst(Creature enterer) {
                    var caster = Creature.CreateSimpleCreature(trap.Name!).AddMonsterInnateSpellcasting(12, Trait.Arcane, level3Spells: [SpellId.AcidArrow]);
                    caster.OwningFaction = enterer.Battle.Gaia;
                    caster.Battle = enterer.Battle;
                    caster.Occupies = tile;
                    var cc = AllSpells.CreateSpellInCombat(SpellId.AcidArrow, caster, 3, Trait.Innate).WithActionCost(0);
                    cc.Traits.Add(Trait.DoesNotProvoke);
                    await enterer.Battle.GameLoop.FullCast(cc, ChosenTargets.CreateSingleTarget(enterer));
                }

                CommonTraps.AddDoNotStepHere(trap);
                CommonTraps.AddWhenCreatureEnters(trap, SendAcidArrowAgainst, (enterer) => !enterer.HasEffect(QEffectId.Flying));
                CommonTraps.AddDisableDeviceOption(trap, 18, SendAcidArrowAgainst);

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
    }
}