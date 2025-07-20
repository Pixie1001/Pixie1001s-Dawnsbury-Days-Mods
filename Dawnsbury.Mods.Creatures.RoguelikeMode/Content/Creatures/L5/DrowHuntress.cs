using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowHuntress {
        public static Creature Create() {
            var monster = new Creature(Illustrations.Drowhuntress, "Drow Huntress", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid, ModTraits.ArcherMutator }, 5, 15, 6, new Defenses(21, 9, 15, 12), 55,
            new Abilities(2, 5, 2, 2, 4, 2), new Skills(acrobatics: 14, stealth: 14, deception: 10, intimidation: 8))
            .WithCreatureId(CreatureIds.DrowHuntress)
            .AddQEffect(CommonQEffects.Drow())
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Ranged, Proficiency.Master)
            .AddHeldItem(Items.CreateNew(ItemName.Shortbow))
            .AddQEffect(CommonQEffects.SpiderVenomAttack(22, "shortbow"))
            .AddQEffect(new QEffect("Shadowstep {icon:Reaction}", "{b}Trigger{/b} The drow huntress is damaged by a melee attack. {b}Effect{/b} The drow huntress teleports to an unoccupied space up to 30ft away.") {
                AfterYouTakeDamage = async (self, amount, kind, action, critical) => {
                    if (action == null || !action.HasTrait(Trait.Attack) || action.Owner == null || action.Owner.Occupies == null || action.HasTrait(Trait.Ranged)) {
                        return;
                    }

                    if (await self.Owner.AskToUseReaction("Use Shadowstep to teleport up to 30ft away.")) {
                        Tile? bestTile = null;
                        int bestScore = int.MinValue;
                        foreach (Tile tile in self.Owner.Battle.Map.AllTiles.Where(t => t.IsTrulyGenuinelyFreeToEveryCreature && !t.HazardousTerrainEphemeral && t.DistanceTo(self.Owner.Occupies) <= 6)) {
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
                            self.Owner.Overhead("*shadow step*", Color.Purple, $"{self.Owner.Name} uses {{b}}Shadow Step{{/b}}.");
                            self.Owner.TranslateTo(bestTile);
                            self.Owner.AnimationData.ColorBlink(Color.DarkSlateBlue);
                            Sfxs.Play(SfxName.PhaseBolt);
                        }
                    }
                }
            });

            monster.AddQEffect(new QEffect() {
                ProvideStrikeModifier = item => {
                    if (!item.HasTrait(Trait.Bow)) return null;

                    CombatAction strike = monster.CreateStrike(item, -1, new StrikeModifiers() {
                        AdditionalBonusesToAttackRoll = [new Bonus(2, BonusType.Circumstance, "archer's aim")],
                        HuntersAim = true,
                    });

                    strike.Name = $"Archer's Aim ({item.Name})";
                    strike.Illustration = new SideBySideIllustration(strike.Illustration, IllustrationName.TargetSheet);
                    strike.ActionCost = 2;
                    strike.Description = StrikeRules.CreateBasicStrikeDescription2(strike.StrikeModifiers);
                    strike.ShortDescription = strike.ShortDescription + "; this attack ignores concealment.";
                    strike.WithGoodnessAgainstEnemy((_, a, d) => {
                        if (a.Actions.ActionsLeft == 3) {
                            return 6;
                        }
                        return 0;
                    });

                    return strike;
                }
            });

            return monster;
        }
    }
}