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
    public class AnimatedStatue
    {
        public static Creature Create()
        {
            return new Creature(Illustrations.AnimatedStatue, "Animated Statue", [Trait.Construct, Trait.Earth, Trait.Mindless], 3, 9, 4, new Defenses(19, 12, 5, 5), 35, new Abilities(4, -2, 5, -5, 0, -5), new Skills(athletics: 11))
                .WithCharacteristics(false, true)
                .AddQEffect(QEffect.TraitImmunity(Trait.Disease))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Doomed))
                .AddQEffect(QEffect.TraitImmunity(Trait.Healing))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Unconscious))
                .WithHardness(6)
                .AddQEffect(QEffect.ArmorBreak(4))
                .AddQEffect(QEffect.MonsterGrab())
                .Builder
                .AddNaturalWeapon(NaturalWeaponKind.Fist, 11, [Trait.Magical, Trait.Grab], "1d8+6", DamageKind.Bludgeoning)
                .Done();
        }
    }
}