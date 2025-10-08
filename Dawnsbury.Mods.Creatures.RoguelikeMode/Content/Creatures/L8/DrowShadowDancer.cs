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
    public class DrowShadowDancer {
        public static Creature Create() {

            Creature monster = new Creature(Illustrations.DrowShadowdancer, "Drow Shadowdancer", [Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid, Trait.MetalArmor, Trait.Female, ModTraits.Drow, ModTraits.MeleeMutator],
               level: 8, perception: 17, speed: 7, new Defenses(26, fort: 13, reflex: 19, will: 16), hp: 135,
            new Abilities(4, 6, 2, 3, 3, 4), new Skills(religion: 16, athletics: 16, intimidation: 16, acrobatics: 16))
            .WithAIModification(ai => {
                //ai.OverrideDecision = (self, options) => {
                //    Creature monster = self.Self;

                //    AiFuncs.PositionalGoodness(monster, options, (pos, you, step, cr) => you.DistanceTo(cr) <= 6 && cr.FriendOf(you) && !cr.HasTrait(Trait.Celestial) && (cr.HasTrait(Trait.Animal) || cr.HasTrait(Trait.Beast)), 0.2f, false);

                //    return null;
                //};
            })
            .WithCreatureId(CreatureIds.DrowShadowdancer)
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(CommonQEffects.DrowClergy())
            .AddQEffect(CommonQEffects.PreyUpon())
            .AddQEffect(new QEffect("Critical Specialisation (Sword)", "") {
                YouHaveCriticalSpecialization = (self, weapon, combatAction, defender) => weapon.HasTrait(Trait.Sword)
            })
            .AddQEffect(QEffect.SneakAttack("2d6"))
            .WithProficiency(Trait.Weapon, Proficiency.Master)
            .WithBasicCharacteristics()
            //.AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Champion, Ability.Charisma, Trait.Divine).WithSpells(
            //    level1: [SpellId.TrueStrike, SpellId.TrueStrike, SpellId.TrueStrike],
            //    //level2: [SpellId.SuddenBolt, SpellId.Bless, SpellId.Heal],
            //    level4: [SpellId.Harm, SpellId.Harm, SpellId.Harm]
            //    ).Done()
            .AddQEffect(new QEffect("Shadow Strike {icon:Reaction}", "{b}Trigger{/b} An enemy damages you. {b}Effect{/b} You may make immediately teleport to the target, and strike them.") {
                YouAreDealtDamage = async (self, attacker, dmg, you) => {
                    if (attacker.Occupies == null || !attacker.Occupies.Neighbours.TilesPlusSelf.Any(t => t.IsTrulyGenuinelyFreeTo(self.Owner))) {
                        return null;
                    }

                    if (await self.Owner.AskToUseReaction($"{attacker.Name} is attempting to attack you in melee. Would you like to retaliate?") == false) return null;

                    Tile? targetTile = attacker.Occupies.GetShuntoffTileIfNecessary(self.Owner);

                    if (targetTile != null) {
                        self.Owner.Overhead("*shadow step*", Color.Purple);
                        self.Owner.TranslateTo(targetTile);
                        self.Owner.AnimationData.ColorBlink(Color.DarkSlateBlue);
                        Sfxs.Play(SfxName.PhaseBolt);
                    }

                    CombatAction strike = self.Owner.CreateStrike(self.Owner.PrimaryWeapon!, 0).WithActionCost(0);
                    strike.ChosenTargets = ChosenTargets.CreateSingleTarget(attacker);

                    int map = self.Owner.Actions.AttackedThisManyTimesThisTurn;

                    if ((bool)strike.CanBeginToUse(self.Owner) && (strike.Target as CreatureTarget)!.IsLegalTarget(self.Owner, attacker).CanBeUsed) {
                        await strike.AllExecute();
                        self.Owner.Actions.AttackedThisManyTimesThisTurn = map;
                    } else {
                        you.Overhead("Strike failed", Color.White, you.Name + "'s Shadow Strike failed.");
                    }

                    return null;
                }
            })
            .Builder
            .AddManufacturedWeapon(ItemName.ElvenCurveBlade, 20, [Trait.Evil, Trait.Magical, Trait.Chaotic], "2d8+4", wp => {
                wp.AdditionalDamage.Add(("1d8", DamageKind.Negative));
            })
            .AddMainAction(you => {
                return new CombatAction(you, IllustrationName.ShadowProjectile, "Shadow Abduction", [Trait.Divine, Trait.Concentrate, Trait.Flourish], "Teleport target creature within 30 feet up to 30 feet away, and then immediately teleport next to them.", Target.Ranged(6))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.Necromancy)
                .WithGoodnessAgainstEnemy((_, a, d) => 20f)
                .WithEffectOnSelf(async (action, caster) => {

                })
                .WithEffectOnEachTarget(async (action, user, target, result) => {
                    Tile? bestTile = null;
                    int bestScore = int.MinValue;
                    foreach (Tile tile in user.Battle.Map.AllTiles.Where(t => t.IsTrulyGenuinelyFreeToEveryCreature && t.Neighbours.TilesPlusSelf.Any(n => t != n && n.IsTrulyGenuinelyFreeTo(user)) && !t.HazardousTerrainEphemeral && t.DistanceTo(user.Occupies) <= 6)) {
                        int score = 0;
                        var enemies = user.Battle.AllCreatures.Where(cr => !cr.FriendOf(user));
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
                        target.Overhead("*abducted*", Color.Purple, $"{target.Name} was abducted.");
                        target.TranslateTo(bestTile);
                        target.AnimationData.ColorBlink(Color.DarkSlateBlue);
                        Sfxs.Play(SfxName.PhaseBolt);
                    } else {
                        action.RevertRequested = true;
                        user.AddQEffect(new QEffect() {
                            AdditionalGoodness = (self, ca, victim) => ca.Name == action.Name && victim == target ? int.MinValue : 0f
                        });
                        return;
                    }

                    Tile? targetTile = target.Occupies.GetShuntoffTileIfNecessary(user);

                    if (targetTile != null) {
                        user.Overhead("*shadow step*", Color.Purple);
                        user.TranslateTo(targetTile);
                        user.AnimationData.ColorBlink(Color.DarkSlateBlue);
                        Sfxs.Play(SfxName.PhaseBolt);
                    } else {
                        Sfxs.Play(SfxName.SpellFail);
                    }
                })
                ;
            })
            .Done()
            ;

            return monster;
        }
    }

}

