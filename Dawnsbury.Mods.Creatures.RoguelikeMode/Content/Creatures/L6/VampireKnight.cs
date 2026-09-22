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
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Mechanics.Zoning;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class VampireKnight {
        public static Creature Create() {
            var longsword = Items.CreateNew(ItemName.Longsword)
                .WithAdditionalWeaponProperties(wp => {
                    wp.WithAdditionalPersistentDamage("1d6", DamageKind.Bleed);
                    wp.DamageDieCount = 2;
                    wp.ItemBonus = 1;
                });
            longsword.Traits.Add(Trait.Grab);

            var monster = CommonQEffects.MakeVampire(new Creature(IllustrationName.SteamKnight, "Vampire Knight", [Trait.Undead, Trait.Lawful, Trait.Evil, ModTraits.MeleeMutator], level: 6, perception: 16, speed: 5, new Defenses(27, 17, 14, 14), hp: 65, new Abilities(5, 3, 4, 3, 3, 3), new Skills(athletics: 15, intimidation: 13, occultism: 11, acrobatics: 13)), 5, 10, (ai, options) => {
                AiFuncs.PositionalGoodness(ai.Self, options, (t, self, step, other) => !other.HasTrait(Trait.Object) && self.DistanceTo(other) <= 2, 0.5f, false);

                return null;
            })
                .WithBasicCharacteristics()
                .WithCreatureId(CreatureIds.VampireKnight)
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .WithUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Claw, "2d6", DamageKind.Slashing, [Trait.Agile, Trait.Finesse, Trait.Grab]))
                .AddHeldItem(longsword)
                // Swarming Bats
                .AddQEffect(new QEffect("Swarming Bats", "(aura) 10 feet. Allies in the aura are concealed {i}(Everyone has an extra 20% miss chance against you.){/i} by Vampire Knight's swarming bat familiars, while enemies are {r}dazzled{/r}.") {
                    SpawnsAura = qf => new MagicCircleAuraAnimation(IllustrationName.BaneCircle, Color.Black, 2),
                    StartOfCombat = async qfSelf => {
                        var z = Zone.Spawn(qfSelf, ZoneAttachment.Aura(2));
                        z.StateCheckOnEachCreatureInZone = (zSelf, target) => {
                            if (target.FriendOfAndNotSelf(qfSelf.Owner)) {
                                target.AddQEffect(Level2Spells.Blur().WithName("Obscured").WithExpirationEphemeral());
                            } else if (target.EnemyOf(qfSelf.Owner)) {
                                target.AddQEffect(QEffect.Dazzled().WithExpirationEphemeral());
                            }
                        };
                    }
                })
                .Builder
                //.AddMainAction((you) => new CombatAction(you, IllustrationName.Bat256, "Plague of Bats", [Trait.Enchantment, Trait.Manipulate, ModTraits.Vampire],
                //UtilityFunctions.CombatActionStats(range: "15 feet", save: "basic Reflex") +
                //"Target creature takes 6d6 piercing damage (basic Reflex save mitigates). On a failure they are frightened 1, and on a critical failure they are frightened 2 and drained 1 (cumulative).",
                //Target.Ranged(3))
                //.WithShortDescription("Swarm an enemy within 15 feet, dealing 6d6 piercing damage (basic Reflex save mitigates). On a failure they are frightened 1, and on a critical failure they are frightened 2 and drained 1 (cumulative).")
                //.WithSavingThrow(new SavingThrow(Defense.Reflex, you.Level + 15))
                //.WithGoodnessAgainstEnemy((t, a, d) => a.AI.ApplyAdditionalDrained(d, 1) + (a.AI.IsTargetNotWorthwhileToFrightened(d) ? 0 : d.Level) + 3.5f * 6)
                //.WithProjectileCone(IllustrationName.Bat256, 20, ProjectileKind.Cone)
                //.WithSoundEffect(SfxName.InsectDeath)
                //.WithEffectOnEachTarget(async (spell, caster, target, result) => {
                //    await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, "6d6", DamageKind.Piercing);

                //    if (result == CheckResult.Failure) {
                //        target.AddQEffect(QEffect.Frightened(1));
                //    } else if (result == CheckResult.Failure) {
                //        target.AddQEffect(QEffect.Frightened(2));
                //        CommonSpellEffects.CumulativeDrain(target, 1);
                //    }
                //}))
                .Done();

            var mistStrike = new QEffect("Mist Strike {icon:Reaction}", "When an ally within 30 feet of the Vampire Knight is damaged by a melee attack, it may use it's reaction to teleport to the damaged creature's side and strike the enemy that attacked it.");
            mistStrike.AddGrantingOfTechnical(cr => cr.FriendOfAndNotSelf(monster) && cr.DistanceTo(monster) <= 6, qfTech => {
                qfTech.AfterYouTakeDamage = async (self, amount, kind, action, critical) => {
                    if (monster.HasEffect(QEffectId.DimensionalAnchor) || action == null || !action.HasTrait(Trait.Attack) || action.Owner == null || action.Owner.Occupies == null || action.HasTrait(Trait.Ranged) || !action.IsMeleeAgainst(self.Owner)) {
                        return;
                    }

                    if (await monster.AskToUseReaction($"Use Mist Strike to teleport to {qfTech.Owner.Name}'s side and strike their attacker?")) {
                        Tile? bestTile = null;
                        int bestScore = int.MinValue;
                        foreach (Tile tile in self.Owner.Battle.Map.AllTiles.Where(t => t.IsTrulyGenuinelyFreeTo(monster) && action.Owner.Space.GetNeighbours().Contains(t) && !(!monster.HasEffect(QEffectIds.OvercameDivineRevulsion) && t.TileQEffects.Any(qf => qf.Zone?.ControllerQEffect.Id == QEffectIds.WardingOffVampire)))) {
                            int score = monster.Battle.AllCreatures.Where(cr => cr.DistanceTo(monster) <= 2 && !cr.HasTrait(Trait.Object)).Count();

                            if (score > bestScore) {
                                bestTile = tile;
                                bestScore = score;
                            }
                        }

                        if (bestTile != null) {
                            // Teleport
                            monster.Overhead("*mist strike*", Color.DimGray, $"{monster.Name} uses {{b}}Mist Strike{{/b}}.");
                            monster.TranslateTo(bestTile);
                            monster.AnimationData.ColorBlink(Color.DimGray);
                            Sfxs.Play(SfxName.PhaseBolt);

                            // Strike
                            if (monster.PrimaryWeapon != null)
                                await monster.MakeStrike(action.Owner, monster.PrimaryWeapon);
                        } else {
                            Sfxs.Play(SfxName.SpellFail);
                            monster.Actions.RefundReaction();
                        }
                    }
                };
            });
            monster.AddQEffect(mistStrike);

            return monster;
        }
    }
}