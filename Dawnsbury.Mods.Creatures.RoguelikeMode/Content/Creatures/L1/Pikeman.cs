using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Core.Mechanics.Treasure;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Pikeman
    {
        public static Creature Create()
        {
            var creature = new Creature(IllustrationName.OrcShaman256,
                "Pikeman",
                [Trait.Orc, Trait.Humanoid, Trait.Lawful, ModTraits.MeleeMutator],
                level: 1, perception: 10, speed: 5,
                new Defenses(16, 10, 7, 4),
                hp: 20,
                new Abilities(4, 2, 3, 1, 2, 0),
                new Skills(athletics: 6))
            .WithProficiency(Trait.Melee, (Proficiency) 4)
            .WithBasicCharacteristics()
            .AddHeldItem(new Item(IllustrationName.Halberd, "pike", [Trait.Reach, Trait.Shove, Trait.Melee, Trait.Polearm, Trait.Martial]).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)))
            .AddQEffect(QEffect.AttackOfOpportunity())
            .AddQEffect(CommonQEffects.MonsterPush());

            return creature;
        }
    }
}
