using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.AuraAnimations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Mechanics.Zoning;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class VampireBloodStalker {
        public static Creature Create() {
            var hatchet = Items.CreateNew(CustomItems.Hatchet).WithAdditionalWeaponProperties(wp => {
                wp.WithAdditionalPersistentDamage("1d6", DamageKind.Bleed);
                wp.DamageDieCount = 2;
                wp.ItemBonus = 1;
            });
            hatchet.Traits.Add(Trait.Returning);
            hatchet.Traits.Add(Trait.Grab);

            var monster = CommonQEffects.MakeVampire(new Creature(IllustrationName.Babau, "Vampire Blood Stalker", [Trait.Undead, Trait.Lawful, Trait.Evil, ModTraits.MeleeMutator], level: 5, perception: 18, speed: 7, new Defenses(21, 9, 15, 12), hp: 65, new Abilities(5, 5, 3, 3, 5, 2), new Skills(athletics: 14, intimidation: 11, nature: 11, acrobatics: 15)), 5, 10, (ai, options) => {
                if (ai.Self.PrimaryWeapon == null) return null;

                var weakest = ai.Self.Battle.AllCreatures.Where(cr => cr.HasTrait(Trait.Mindless) && cr.EnemyOf(ai.Self)).MinBy(cr => cr.HP * (2 - (ai.Self.GetProficiency(ai.Self.PrimaryWeapon) + 21 - cr.Defenses.GetBaseValue(Defense.AC)) / 20));
                AiFuncs.PriorityTarget(ai.Self, options, (a, d) => d == weakest, 3, true);

                return null;
            })
                .WithBasicCharacteristics()
                .WithCreatureId(CreatureIds.VampireBloodStalker)
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .WithUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Claw, "2d6", DamageKind.Slashing, [Trait.Agile, Trait.Finesse, Trait.Grab]))
                .AddHeldItem(hatchet)
                .AddQEffect(new QEffect("Misty Escape {icon:Reaction}", "{b}Trigger{/b} The vampire blood stalker is damaged by a melee attack. {b}Effect{/b} The vampire blood stalker teleports to an unoccupied space up to 30 feet away, taking any creature it has grabbed with it.") {
                    AfterYouTakeDamage = async (self, amount, kind, action, critical) => {
                        if (self.Owner.HasEffect(QEffectId.DimensionalAnchor) || action == null || !action.HasTrait(Trait.Attack) || action.Owner == null || action.Owner.Occupies == null || action.HasTrait(Trait.Ranged) || !action.IsMeleeAgainst(self.Owner)) {
                            return;
                        }

                        if (await self.Owner.AskToUseReaction("Use Misty Escape to teleport up to 30 feet away.")) {
                            Tile? bestTile = null;
                            int bestScore = int.MinValue;
                            foreach (Tile tile in self.Owner.Battle.Map.AllTiles.Where(t => t.IsTrulyGenuinelyFreeToEveryCreature && !t.HazardousTerrainEphemeral && t.DistanceTo(self.Owner.Occupies) <= 6 && !(self.Owner.HeldItems.Any(itm => itm.HasTrait(Trait.Grapplee)) && !t.Neighbours.TilesPlusSelf.Any(n => n.IsTrulyGenuinelyFreeToEveryCreature && n != t)))) {
                                int score = 0;
                                var enemies = self.Owner.Battle.AllCreatures.Where(cr => !cr.FriendOf(self.Owner));
                                foreach (Creature enemy in enemies) {
                                    score += enemy.DistanceTo(tile);
                                }
                                score /= enemies.Count();

                                if (score > bestScore) {
                                    bestTile = tile;
                                    bestScore = score;
                                }
                            }

                            if (bestTile != null) {
                                self.Owner.Overhead("*misty escape*", Color.DimGray, $"{self.Owner.Name} uses {{b}}Misty Escape{{/b}}.");
                                self.Owner.TranslateTo(bestTile);
                                self.Owner.AnimationData.ColorBlink(Color.DimGray);
                                var grapplee = self.Owner.HeldItems.FirstOrDefault(itm => itm.HasTrait(Trait.Grapplee))?.Grapplee;
                                if (grapplee != null) {
                                    grapplee.TranslateTo(UtilityFunctions.ChooseAtRandom(bestTile.Neighbours.TilesPlusSelf.Where(n => n.IsTrulyGenuinelyFreeToEveryCreature && n != bestTile).ToList())!);
                                    grapplee.AnimationData.ColorBlink(Color.DimGray);
                                }
                                self.Owner.TranslateTo(bestTile);
                                self.Owner.AnimationData.ColorBlink(Color.DimGray);
                                Sfxs.Play(SfxName.PhaseBolt);
                            }
                        }
                    },
                    ProvideActionIntoPossibilitySection = (qfSelf, section) => {
                        if (section.PossibilitySectionId != PossibilitySectionId.InvisibleActions || !qfSelf.Owner.HeldItems.Contains(hatchet)) {
                            return null;
                        }

                        return new ActionPossibility(StrikeRules.CreateStrike(qfSelf.Owner, hatchet, RangeKind.Ranged, -1, true)
                            .WithGoodnessAgainstEnemy((t, a, d) => (StrikeRules.CreateStrike(qfSelf.Owner, hatchet, RangeKind.Melee, -1).Target as CreatureTarget)?.CreatureGoodness(t, a, d) ?? 0f));
                    }
                })
                .Builder
                .Done();

            return monster;
        }
    }
}