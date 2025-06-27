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
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework.Graphics;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowNecromancer {
        public static Creature Create() {
            return new Creature(Illustrations.DrowNecromancer, "Drow Necromancer", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid, ModTraits.SpellcasterMutator }, 2, 8, 6, new Defenses(16, 5, 8, 11), 25,
            new Abilities(1, 3, 0, 4, 1, 2), new Skills(acrobatics: 10, intimidation: 11, occultism: 13, deception: 11))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;
                    AiFuncs.PositionalGoodness(creature, options, (t, crSelf, step, cr) => (step || crSelf.Occupies == t) && t.IsAdjacentTo(cr.Occupies) && cr.OwningFaction.EnemyFactionOf(crSelf.OwningFaction), -0.2f, false);
                    AiFuncs.AverageDistanceGoodness(creature, options, cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction), cr => cr.OwningFaction.AlliedFactionOf(creature.OwningFaction), -15, 5);

                    return null;
                };
            })
            .WithCreatureId(CreatureIds.DrowNecromancer)
            .WithProficiency(Trait.Melee, Proficiency.Trained)
            .WithProficiency(Trait.Occult, Proficiency.Expert)
            .WithBasicCharacteristics()
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.AnimateDead, "Empower Dead", new Trait[] { Trait.Magical, Trait.Necromancy, Trait.Manipulate, Trait.Auditory },
                        "Grant target undead ally a +1 status bonus to AC and a +4 status bonus to strike damage until the start of your next turn", Target.RangedFriend(6)
                        .WithAdditionalConditionOnTargetCreature((a, d) => d.HasTrait(Trait.Undead) ? Usability.Usable : Usability.NotUsableOnThisCreature("Target is not undead")))
                    .WithSoundEffect(SfxName.Necromancy)
                    .WithActionCost(1)
                    .WithProjectileCone(IllustrationName.AnimateDead, 7, ProjectileKind.Cone)
                    .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                        target.AddQEffect(new QEffect("Empower Dead", "This creature gains +1 status bonus to AC and a +4 status bonus to strike damage.", ExpirationCondition.ExpiresAtStartOfSourcesTurn, caster, IllustrationName.AnimateDead) {
                            BonusToDamage = (self, action, defender) => action.HasTrait(Trait.Strike) ? new Bonus(4, BonusType.Status, "Empower Dead") : null,
                            BonusToDefenses = (self, action, defence) => defence == Defense.AC ? new Bonus(1, BonusType.Status, "Empower Dead") : null,
                            Key = "Empower Dead"
                        });
                    })
                    .WithGoodness((t, attacker, defender) => {
                        if (defender.QEffects.Any(qf => qf.Name == "Empower Dead")) {
                            return int.MinValue;
                        }

                        float currScore = 100;
                        float bestScore = 100;
                        foreach (Creature pc in attacker.Battle.AllCreatures.Where(cr => cr.EnemyOf(attacker))) {
                            currScore = defender.DistanceTo(pc);
                            if (currScore < bestScore) {
                                bestScore = currScore;
                            }
                            //foreach (Creature undead in attacker.Battle.AllCreatures.Where(cr => cr.EnemyOf(pc) && cr.HasTrait(Trait.Undead) && !cr.QEffects.Any(qf => qf.Name == "Empower Dead"))) {
                            //    currScore = pc.DistanceTo(undead);
                            //    if (curr)
                            //}
                        }

                        float levelBonus = defender.Level / 2;

                        if (defender.HeldItems.Count > 0 && defender.HeldItems.MaxBy(item => item.WeaponProperties?.RangeIncrement)?.WeaponProperties?.RangeIncrement <= bestScore) {
                            return 7 + levelBonus;
                        } else if (bestScore > defender.Speed) {
                            return 3;
                        } else {
                            return 7 + levelBonus;
                        }

                        //if (attacker.Battle.AllCreatures.Where(cr => cr.))
                    })
                    ;
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Wizard, Ability.Intelligence, Trait.Occult).WithSpells(
            new SpellId[] { SpellId.GrimTendrils, SpellId.Harm, SpellId.AnimateDead, SpellId.AnimateDead, SpellId.RayOfFrost, SpellId.ChillTouch, SpellId.Shield }).Done();
        }
    }
}