using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
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
using Dawnsbury.Core.Tiles;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class MerfolkBrineblade {
        public static Creature Create() {
            Creature monster = new Creature(IllustrationName.DarkPoet256, "Merfolk Brineblade", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Merfolk, Trait.Humanoid, Trait.Aquatic }, 2, 8, 6, new Defenses(17, 5, 8, 11), 25,
                new Abilities(4, 3, 1, 0, 2, 1), new Skills(acrobatics: 6, intimidation: 8, nature: 7, occultism: 7))
                .WithAIModification(ai => {
                    ai.OverrideDecision = (self, options) => {
                        Creature monster = self.Self;

                        AiFuncs.PositionalGoodness(monster, options, (pos, you, step, them) => {
                            if (monster.Battle.AllCreatures.Any(cr => cr.EnemyOf(you) && cr.Threatens(you.Occupies)) && step == false) {
                                return false;
                            }

                            int nearbyOpponents = you.Battle.AllCreatures.Where(cr => (cr.OwningFaction.IsPlayer || cr.OwningFaction.IsGaiaFriends) && cr.Alive && cr.DistanceTo(you) <= 3).Count();
                            if (nearbyOpponents <= 1) {
                                return true;
                            }
                            return false;
                        }, 10);

                        AiFuncs.PositionalGoodness(monster, options, (pos, you, step, them) => {
                            if (monster.Battle.AllCreatures.Any(cr => cr.EnemyOf(you) && cr.Threatens(you.Occupies)) && step == false) {
                                return false;
                            }

                            int nearbyOpponents = you.Battle.AllCreatures.Where(cr => (cr.OwningFaction.IsPlayer || cr.OwningFaction.IsGaiaFriends) && cr.DistanceTo(you) <= 6).Count();
                            if (nearbyOpponents <= 1) {
                                return true;
                            }
                            return false;
                        }, 7);

                        return null;
                    };
                })
                .WithCreatureId(CreatureIds.MerfolkBrineBlade)
                .WithProficiency(Trait.Simple, Proficiency.Expert)
                .WithProficiency(Trait.Unarmed, Proficiency.Trained)
                .WithProficiency(Trait.Spell, Proficiency.Trained)
                .WithBasicCharacteristics()
                .AddHeldItem(Items.CreateNew(ItemName.Dagger))
                .AddQEffect(CommonQEffects.UnderwaterMarauder())
                .AddQEffect(QEffect.AttackOfOpportunity())
                .AddQEffect(CommonQEffects.OceanFlight())
                .AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        int dc = 17 + self.Owner.Level;

                        return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.TidalHands, "Riptide", new Trait[] { Trait.Evocation, Trait.Magical, Trait.Manipulate, Trait.Flourish },
                            $"Range 30ft range; DC {dc} vs. Fort; On a failure the target is pulled into an adjacent space. On a critical failure, they're also immobilized until the start of the Merfolk Brineblade's next turn.", Target.Line(6)
                            .WithIncludeOnlyIf((t, d) => !d.FriendOf(t.OwnerAction.Owner)))
                        .WithGoodnessAgainstEnemy((targeting, a, d) => {
                            int nearbyOpponents = a.Battle.AllCreatures.Where(cr => (cr.OwningFaction.IsPlayer || cr.OwningFaction.IsGaiaFriends) && cr.Alive && cr.DistanceTo(a) <= 3).Count();
                            if (nearbyOpponents == 0) {
                                return 100f - d.Defenses.GetBaseValue(Defense.Fortitude);
                            }
                            return 0;
                        })
                        .WithActionCost(1)
                        .WithSavingThrow(new SavingThrow(Defense.Fortitude, dc))
                        .WithSoundEffect(SfxName.TidalSurge)
                        .WithProjectileCone(IllustrationName.TidalHands, 7, ProjectileKind.Cone)
                        .WithEffectOnEachTarget(async (action, caster, defender, result) => {
                            if (action.ChosenTargets.ChosenCreatures.MinBy(cr => cr.DistanceTo(caster)) != defender) {
                                return;
                            }

                            if (result <= CheckResult.Failure) {
                                await caster.PullCreature(defender);
                            }

                            if (result == CheckResult.CriticalFailure) {
                                defender.AddQEffect(QEffect.Immobilized().WithExpirationAtStartOfSourcesTurn(caster, 0));
                            }

                            //action.ChosenTargets.ChosenCreatures.RemoveAll(cr => cr != defender);
                        })
                        ;
                    }
                })
                //.AddQEffect(new QEffect() {
                //    ProvideMainAction = self => {
                //        int dc = 17 + self.Owner.Level;

                //        return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.TidalHands, "Riptide", new Trait[] { Trait.Evocation, Trait.Magical, Trait.Manipulate, Trait.Flourish },
                //            $"DC {dc} vs. Fort; On a failure the target is pulled into an adjacent space. On a critical failure, they're also immobilized until the start of the Merfolk Brineblade's next turn.", Target.Ranged(6, (targeting, a, d) => {
                //                int nearbyOpponents = a.Battle.AllCreatures.Where(cr => (cr.OwningFaction.IsPlayer || cr.OwningFaction.IsGaiaFriends) && cr.DistanceTo(a) <= 3).Count();
                //                if (nearbyOpponents == 0) {
                //                    return 100f - d.Defenses.GetBaseValue(Defense.Fortitude);
                //                }
                //                return int.MinValue;
                //            })
                //        .WithAdditionalConditionOnTargetCreature((a, d) => {
                //            if (a.HasLineOfEffectTo(d.Occupies) == CoverKind.None) {
                //                return Usability.Usable;
                //            }
                //            return Usability.NotUsableOnThisCreature("Path obstructed");
                //        }))
                //        .WithActionCost(1)
                //        .WithSavingThrow(new SavingThrow(Defense.Fortitude, dc))
                //        .WithSoundEffect(SfxName.TidalSurge)
                //        .WithProjectileCone(IllustrationName.TidalHands, 7, ProjectileKind.Cone)
                //        .WithEffectOnEachTarget(async (action, caster, defender, result) => {
                //            if (result <= CheckResult.Failure) {
                //                QEffect incorporeal = new QEffect() { Id = QEffectId.Incorporeal };
                //                defender.AddQEffect(incorporeal);
                //                await caster.PullCreature(defender);
                //                defender.RemoveAllQEffects(qf => qf == incorporeal);
                //            }

                //            if (result == CheckResult.CriticalFailure) {
                //                defender.AddQEffect(QEffect.Immobilized().WithExpirationAtStartOfSourcesTurn(caster, 0));
                //            }
                //        })
                //        ;
                //    }
                //})
                .AddQEffect(new QEffect("Blade in the Depths", "Creatures without any allies within 10 feet of them suffer an additional 1d8 cold damage, unless they're flanking you.") {
                    AddExtraKindedDamageOnStrike = (action, d) => {
                        if (!action.HasTrait(Trait.Strike)) {
                            return null;
                        }

                        if (UtilityFunctions.IsFlanking(d, action.Owner)) {
                            return null;
                        }

                        int closeAllies = action.Owner.Battle.AllCreatures.Where(cr => cr != d && (cr.OwningFaction.IsPlayer || cr.OwningFaction.IsGaiaFriends) && cr.Alive && cr.DistanceTo(d) <= 2).Count();
                        if (closeAllies == 0) {
                            return new KindedDamage(DiceFormula.FromText("1d8", "Blade in the Depths"), DamageKind.Cold);
                        }
                        return null;
                    },
                    AdditionalGoodness = (self, action, defender) => {
                        if (UtilityFunctions.IsFlanking(defender, self.Owner)) {
                            return 0;
                        }

                        int closeAllies = self.Owner.Battle.AllCreatures.Where(cr => cr != defender && (cr.OwningFaction.IsPlayer || cr.OwningFaction.IsGaiaFriends) && cr.Alive && cr.DistanceTo(defender) <= 2).Count();
                        if (closeAllies == 0) {
                            return 4.5f;
                        }
                        return 0;
                    }
                })
                .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Magus, Ability.Wisdom, Trait.Primal).WithSpells(
                new SpellId[] { SpellId.RayOfFrost, SpellId.Shield }).Done()
                ;

            return monster;
        }

        private static bool IsPullObstructed(Tile originTile, Creature defender) {
            Vector2 origin = new Vector2(originTile.X + 0.5f, originTile.Y + 0.5f);
            Vector2 target = new Vector2(defender.Occupies.X + 0.5f, defender.Occupies.Y + 0.5f);
            Vector2 mag = target - origin;

            //float distance = mag.Length() * 8;

            for (int i = 0; i < mag.Length() * 8; ++i) {
                Point currPoint = (origin + (mag / (mag.Length() * 8))).ToPoint();
                Tile currTile = defender.Battle.Map.GetTile(currPoint.X, currPoint.Y);

                if (currTile != null && currTile != originTile && currTile != defender.Occupies) {
                    if (currTile.AlwaysBlocksMovement) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}