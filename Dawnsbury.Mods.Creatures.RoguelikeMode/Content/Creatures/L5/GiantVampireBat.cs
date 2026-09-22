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
    public static class GiantVampireBat {
        public static Creature Create() {
            var creature = new Creature(Illustrations.Unicorn,
                "Giant Vampire Bat",
                [Trait.Neutral, Trait.Animal, Trait.Large, ModTraits.MeleeMutator],
                5, 15, 6,
                new Defenses(22, 15, 12, 11),
                75,
                new Abilities(5, 3, 4, -4, 4, -2),
                new Skills(acrobatics: 12, athletics: 12, stealth: 12))
            .WithCreatureId(CreatureIds.GiantVampireBat)
            .WithUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Fang, "2d6", DamageKind.Piercing, []).WithAdditionalWeaponProperties(wp => {
                wp.WithAdditionalPersistentDamage("1d6", DamageKind.Bleed);
                wp.OverrideReach = 1;
            }))
            .WithAdditionalUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Wing, "2d6", DamageKind.Bludgeoning, [Trait.Agile]).WithAdditionalWeaponProperties(wp => {
                wp.WithAdditionalPersistentDamage("1d6", DamageKind.Bleed);
                wp.OverrideReach = 1;
            }))
            .WithCharacteristics(false, true)
            .WithProficiency(Trait.Unarmed, Proficiency.Expert)
            .AddQEffect(QEffect.Flying())
            .AddQEffect(QEffect.Tremorsense(8).WithName("Echolocation"))
            .AddQEffect(CommonQEffects.WingThrash("wing"))
            .Builder
            .Done();

            return creature;
        }
    }
}
