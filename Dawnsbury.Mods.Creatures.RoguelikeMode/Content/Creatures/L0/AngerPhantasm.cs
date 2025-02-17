using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class AngerPhantasm
    {
        public static Creature Create()
        {
            return new Creature(IllustrationName.GhostMage,
                "Anger Phantasm",
                [Trait.Incorporeal, Trait.Undead, Trait.Chaotic, Trait.Evil],
                0, 6, 3,
                new Defenses(15, 3, 6, 6),
                8,
                new Abilities(-5, 3, 0, 2, 0, 2),
                new Skills(intimidation: 6, stealth: 4))
            .WithCharacteristics(true, false)
            .AddQEffect(QEffect.DamageResistanceAllExcept(3, DamageKind.Force, DamageKind.Good, DamageKind.Lawful, DamageKind.Positive))
            .AddQEffect(QEffect.TraitImmunity(Trait.Emotion))
            .AddQEffect(QEffect.TraitImmunity(Trait.Fear))
            .AddQEffect(QEffect.Flying())
            .Builder
            .AddNaturalWeapon(NaturalWeaponKind.GhostHand, 8, [Trait.Finesse], "1d4+0", DamageKind.Negative, (properties) => properties.WithAdditionalDamage("1d4", DamageKind.Mental))
            .Done();
        }
    }
}
