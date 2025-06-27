using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Pikeman
    {
        public static Creature Create()
        {
            var creature = new Creature(IllustrationName.OrcShaman256,
                "Pikeman",
                [Trait.Orc, Trait.Humanoid, Trait.Lawful, Trait.NoPhysicalUnarmedAttack, ModTraits.MeleeMutator],
                1, 10, 5,
                new Defenses(16, 10, 7, 4),
                20,
                new Abilities(4, 2, 3, 1, 2, 0),
                new Skills(athletics: 6))
            .WithBasicCharacteristics()
            .AddQEffect(QEffect.AttackOfOpportunity())
            .AddQEffect(CommonQEffects.MonsterPush());

            UtilityFunctions.AddNaturalWeapon(creature, "pike", IllustrationName.Halberd, 9, [Trait.Reach, Trait.Shove], "1d6+3", DamageKind.Piercing);

            return creature;
        }
    }
}
