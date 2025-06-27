using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Microsoft.Xna.Framework;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Audio;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.Intelligence;
using System.Text;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class WinterWolf {
        public static Creature Create() {
            var creature = new Creature(Illustrations.WinterWolf,
                "Winter Wolf",
                [Trait.Evil, Trait.Beast, ModTraits.MeleeMutator],
                5, 14, 8,
                new Defenses(23, 13, 15, 10),
                70,
                new Abilities(6, 4, 4, 2, 3, 2),
                new Skills(acrobatics: 13, athletics: 13, deception: 11, intimidation: 11, stealth: 13, survival: 12))
            .WithCreatureId(CreatureIds.WinterWolf)
            .WithUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Jaws, "1d10", DamageKind.Piercing, [Trait.Cold, Trait.Knockdown]).WithAdditionalWeaponProperties(wp => {
                wp.AdditionalDamage.Add(("1d6", DamageKind.Cold));
            }))
            .WithTactics(Tactic.PackAttack)
            .WithCharacteristics(true, true)
            .WithProficiency(Trait.Unarmed, Proficiency.Expert)
            .AddQEffect(QEffect.DamageImmunity(DamageKind.Cold))
            .AddQEffect(QEffect.TraitImmunity(Trait.Cold))
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Fire, 5))
            .AddQEffect(QEffect.PackAttack("Winter Wolf", "1d6"))
            .AddQEffect(QEffect.MonsterKnockdown())
            .Builder
            .AddMainAction((Creature creature) => {
                return new CombatAction(creature, IllustrationName.ConeOfCold, "Breath Weapon", [Trait.Cold, Trait.Evocation, Trait.Primal], "You breathe a cloud of frost in a 15-foot cone that deals 5d8 cold damage vs. basic Reflex. You can’t use Breath Weapon again for 1d4 rounds.", Target.FifteenFootCone())
                .WithActionCost(2)
                .WithSavingThrow(new SavingThrow(Defense.Reflex, 18 + creature.Level))
                .WithSoundEffect(SfxName.RayOfFrost)
                .WithProjectileCone(IllustrationName.ConeOfCold, 20, Core.Animations.ProjectileKind.Cone)
                .WithGoodnessAgainstEnemy((_, a, d) => 5 * 4.5f)
                .WithEffectOnEachTarget(async (action, user, defender, result) => {
                    await CommonSpellEffects.DealBasicDamage(action, user, defender, result, "5d8", DamageKind.Cold);
                })
                .WithEffectOnSelf(user => {
                    user.AddQEffect(QEffect.Recharging("Breath Weapon", Dice.D4));
                });
            })
            .Done();

            QEffect effect = new QEffect("Avenging Bite {icon:Reaction}", "{b}Trigger{/b} A creature within reach of the winter wolf’s jaws attacks one of the winter wolf’s allies. " +
                        "{b}Effect{/b} Make a melee Strike against the triggering creature.") {
                Innate = true
            };

            effect.AddGrantingOfTechnical(cr => cr.FriendOf(effect.Owner), qf => {
                qf.YouAreDealtDamage = async (qfAlly, attacker, damageStuff, defender) => {
                    if (attacker == null || attacker.Occupies == null || !attacker.EnemyOf(effect.Owner) || attacker.DistanceTo(effect.Owner) > 1)
                        return null;

                    var strike = effect.Owner.CreateStrike(effect.Owner.PrimaryWeapon!).WithActionCost(0); ;

                    if (!strike.CanBeginToUse(attacker) || !(strike.Target as CreatureTarget)!.IsLegalTarget(effect.Owner, attacker)) {
                        return null;
                    }

                    if (!await effect.Owner.Battle.AskToUseReaction(effect.Owner, attacker?.Name.ToString() + " is about to attack " + defender?.Name.ToString() + ". Use Avenging Bite to strike them?"))
                        return null;

                    effect.Owner.Occupies.Overhead("avenging bite!", Color.LightBlue, effect.Owner?.ToString() + " uses avenging bite!");
                    strike.ChosenTargets = ChosenTargets.CreateSingleTarget(attacker!);
                    await strike.AllExecute();
                    return null;
                };
            });

            creature.AddQEffect(effect);

            return creature;
        }
    }
}
