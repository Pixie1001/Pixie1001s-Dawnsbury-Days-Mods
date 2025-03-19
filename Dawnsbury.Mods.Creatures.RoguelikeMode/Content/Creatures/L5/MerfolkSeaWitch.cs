using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using System.Threading;
using System;
using static System.Collections.Specialized.BitVector32;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.StatBlocks;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class MerfolkSeaWitch {
        public static Creature Create() {
            Creature monster = new Creature(Illustrations.MerfolkSeaWitch, "Merfolk Sea Witch", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Merfolk, Trait.Humanoid, Trait.Aquatic }, 5, 13, 6, new Defenses(21, 9, 12, 15), 60,
                new Abilities(2, 2, 3, 2, 5, 4), new Skills(intimidation: 13, nature: 13, occultism: 13))
                .WithCreatureId(CreatureIds.MerfolkSeaWitch)
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .WithProficiency(Trait.Spell, Proficiency.Expert)
                .WithBasicCharacteristics()
                .AddHeldItem(Items.CreateNew(ItemName.Trident))
                .AddQEffect(CommonQEffects.UnderwaterMarauder())
                .AddQEffect(CommonQEffects.OceanFlight())
                .AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        if (self.Owner.Battle.AllCreatures.Any(cr => cr.BaseName == "Heart of the Storm")) {
                            return null;
                        }

                        int dc = 17 + self.Owner.Level;
                        int radius = 3;

                        return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.TidalHands, "Conjure Maelstrom", new Trait[] { Trait.Evocation },
                        $"The Merfolk Sea Witch conjures forth a raging maelstrom to tear her enemies apart.",
                        new TileTarget(delegate (Creature caster, Tile tile) {
                            if (tile.IsTrulyGenuinelyFreeToEveryCreature) {
                                Tile occupies = caster.Occupies;
                                if (occupies != null && occupies.DistanceTo(tile) <= 4) {
                                    return (int)caster.Occupies.HasLineOfEffectToIgnoreLesser(tile) < 4;
                                }
                            }
                            return false;
                        },
                            (caster, tile) => {
                                int score = 0;
                                foreach (Creature pm in caster.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsPlayer)) {
                                    score -= tile.DistanceTo(pm.Occupies);
                                }

                                return 100 + score;
                            }
                        ))
                        .WithActionCost(2)
                        //.WithSoundEffect(SfxName.TidalSurge)
                        .WithProjectileCone(IllustrationName.TidalHands, 15, ProjectileKind.Cone)
                        .WithEffectOnEachTile(async (action, caster, tiles) => {
                            var hots = HeartOfTheStorm.Create(radius, dc);
                            caster.Battle.SpawnCreature(hots, caster.Battle.Gaia, tiles[0]);
                            foreach (Tile tile in self.Owner.Battle.Map.AllTiles.Where(t => t.DistanceTo(hots.Occupies) <= radius)) {
                                tile.QEffects.Add(CommonQEffects.Maelstrom(dc, tile, hots));
                            }
                            caster.AddQEffect(new QEffect("Channeling Maelstrom", "The Merfolk Sea Witch is channeling a destructive maelstrom, which she can move up to 10-feet each round, and will remain until she's defeated.") { Innate = false, Illustration = IllustrationName.TidalHands });
                            await caster.Battle.Cinematics.PlayCutscene(async cin => {
                                cin.EnterCutscene();
                                caster.Battle.SmartCenter(hots.Occupies.X, hots.Occupies.Y);
                                Sfxs.Play(SfxName.TidalSurge);
                                await cin.WaitABit();
                                await cin.WaitABit();
                                cin.ExitCutscene();
                            });
                        })
                        ;
                    }
                })
                .AddQEffect(new QEffect() {
                    StartOfCombat = async self => {
                        self.Owner.AddQEffect(new QEffect() {
                            Id = QEffectId.Slowed,
                            Value = 1,
                            ExpiresAt = ExpirationCondition.ExpiresAtEndOfAnyTurn
                        });
                        await self.Owner.Battle.GameLoop.Turn(self.Owner, false);
                    },
                    ProvideMainAction = self => {
                        if (!self.Owner.Battle.AllCreatures.Any(cr => cr.BaseName == "Heart of the Storm")) {
                            return null;
                        }

                        int dc = 17 + self.Owner.Level;
                        int radius = 3;

                        var hots = self.Owner.Battle.AllCreatures.First(cr => cr.BaseName == "Heart of the Storm");

                        int oldScore = 100;
                        foreach (Creature pm in self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsPlayer)) {
                            if (hots.DistanceTo(pm) <= radius) {
                                oldScore += 5;
                            }
                            oldScore -= hots.DistanceTo(pm);
                        }

                        return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.TidalHands, "Direct Maelstrom", new Trait[] { Trait.Evocation, Trait.Manipulate, Trait.Flourish },
                        $"The Merfolk Sea Witch moves her maelstrom up to 10ft.",
                        new TileTarget(delegate (Creature caster, Tile tile) {
                            if (tile.IsTrulyGenuinelyFreeToEveryCreature && tile.DistanceTo(self.Owner.Battle.AllCreatures.First(cr => cr.BaseName == "Heart of the Storm").Occupies) <= 2) {
                                Tile occupies = caster.Occupies;
                                if (occupies != null && occupies.DistanceTo(tile) <= 6) {
                                    return (int)caster.Occupies.HasLineOfEffectToIgnoreLesser(tile) < 4;
                                }
                            }
                            return false;
                        }, (caster, tile) => {
                                // Get new score
                                int nScore = 100;
                                foreach (Creature pm in caster.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsPlayer)) {
                                    if (tile.DistanceTo(pm.Occupies) <= radius) {
                                        nScore += 5;
                                    }
                                    nScore -= tile.DistanceTo(pm.Occupies);
                                }

                                if (nScore > oldScore) {
                                    nScore += 10;
                                }

                                return nScore - oldScore;
                            }
                        ))
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.TidalSurge)
                        .WithEffectOnEachTile(async (action, caster, tiles) => {
                            var hots = self.Owner.Battle.AllCreatures.First(cr => cr.BaseName == "Heart of the Storm");
                            foreach (Tile tile in self.Owner.Battle.Map.AllTiles) {
                                tile.QEffects.RemoveAll(qf => qf.TileQEffectId == QEffectIds.Maelstrom);
                            }

                            await hots.MoveTo(tiles[0], action, new MovementStyle() {
                                ForcedMovement = true,
                                IgnoresUnevenTerrain = true,
                                Insubstantial = true,
                                ShortestPath = true,
                                MaximumSquares = 1000
                            });

                            foreach (Tile tile in self.Owner.Battle.Map.AllTiles.Where(t => t.DistanceTo(hots.Occupies) <= radius)) {
                                tile.QEffects.Add(CommonQEffects.Maelstrom(dc, tile, hots));
                            }
                        })
                        ;
                    }
                })
                .AddSpellcastingSource(SpellcastingKind.Prepared, ModTraits.Witch, Ability.Wisdom, Trait.Primal).WithSpells(
                [SpellId.RayOfFrost, SpellId.Shield, SpellId.NeedleDarts, SpellId.Heal, SpellId.Fear, SpellId.Fear],
                [SpellId.BrinyBolt, SpellId.BrinyBolt]).Done()
                ;

            return monster;
        }
    }
}

