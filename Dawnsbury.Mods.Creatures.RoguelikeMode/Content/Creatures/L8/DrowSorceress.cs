using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
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
using Microsoft.Xna.Framework;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Animations.AuraAnimations;
using Dawnsbury.Core.Mechanics.Zoning;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.Mechanics.Rules;
using System;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    public class DrowSorceress {
        public static Creature Create() {

            Creature monster = new Creature(Illustrations.DrowSorceress, "Drow Sorceress", [Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid, Trait.Female, ModTraits.Drow, ModTraits.SpellcasterMutator],
               level: 8, perception: 16, speed: 6, new Defenses(26, fort: 13, reflex: 16, will: 19), hp: 100,
            new Abilities(1, 4, 2, 4, 3, 6), new Skills(religion: 12, occultism: 18, arcana: 18, acrobatics: 16, intimidation: 16))
            .WithAIModification(ai => {
                //ai.OverrideDecision = (self, options) => {
                //    Creature monster = self.Self;

                //    AiFuncs.PositionalGoodness(monster, options, (pos, you, step, cr) => you.DistanceTo(cr) <= 6 && cr.FriendOf(you) && !cr.HasTrait(Trait.Celestial) && (cr.HasTrait(Trait.Animal) || cr.HasTrait(Trait.Beast)), 0.2f, false);

                //    return null;
                //};
            })
            .WithCreatureId(CreatureIds.DrowSorceress)
            .AddQEffect(CommonQEffects.Drow())
            .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Master)
            .WithBasicCharacteristics()
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Sorcerer, Ability.Charisma, Trait.Divine).WithSpells(
                level1: [SpellId.Command, SpellId.Command, SpellId.ElectricArc, SpellId.RayOfFrost],
                level2: [SpellId.HideousLaughter, SpellId.Blur, SpellId.MirrorImage],
                level3: [SpellId.VampiricTouch, SpellLoader.AgonisingDespair, SpellId.Slow],
                level4: [SpellId.ImpendingDoom, SpellId.SuddenBlight, SpellId.Sleep]
                ).Done()
            .AddQEffect(new QEffect("Shadowstep {icon:Reaction}", "{b}Trigger{/b} You are damaged by a melee attack. {b}Effect{/b} You teleport to an unoccupied space up to 30ft away.") {
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
            })
            .Builder
            .AddManufacturedWeapon(ItemName.Staff, 15, [], "2d8+1")
            .Done()
            ;

            return monster;
        }
    }

}

