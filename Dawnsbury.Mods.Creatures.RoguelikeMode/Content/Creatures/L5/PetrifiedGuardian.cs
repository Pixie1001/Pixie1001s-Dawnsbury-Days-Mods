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
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class PetrifiedGuardian {
        public static Creature Create() {
            return new Creature(Illustrations.PetrifiedGuardian, "Petrified Guardian", [Trait.Construct, Trait.Mindless, ModTraits.MeleeMutator], 5, 10, 4, new Defenses(22, 15, 12, 12), 80, new Abilities(4, -2, 5, -5, 0, -5), new Skills(athletics: 13))
                .WithCharacteristics(false, false)
                .WithCreatureId(CreatureIds.PetrifiedGuardian)
                .WithProficiency(Trait.Weapon, Proficiency.Master)
                .AddQEffect(QEffect.TraitImmunity(Trait.Disease))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Doomed))
                .AddQEffect(QEffect.TraitImmunity(Trait.Healing))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Unconscious))
                .AddQEffect(QEffect.DamageWeakness(DamageKind.Bludgeoning, 5))
                .AddQEffect(QEffect.DamageResistance(DamageKind.Piercing, 5))
                .AddQEffect(new QEffect("Join Us", "The petrified guardians gains a +1 bonus to attack rolls made against partially petrified creatures.") {
                    BonusToAttackRolls = (self, action, target) => target != null && target.QEffects.Any(qf => qf.Key == "Partial Petrification") ? new Bonus(1, BonusType.Untyped, "Join Us") : null
                })
                .AddQEffect(QEffect.MonsterGrab())
                .AddQEffect(QEffect.AttackOfOpportunity())
                .AddHeldItem(Items.CreateNew(ItemName.Longsword).WithModificationPlusOneStriking().WithMonsterWeaponSpecialization(2))
                .Builder
                .Done();
        }
    }
}