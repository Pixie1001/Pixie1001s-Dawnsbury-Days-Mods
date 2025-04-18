using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Bodyguard
    {
        public static Creature Create()
        {
            var creature = new Creature(IllustrationName.OrcBrute256,
                "Bodyguard",
                [Trait.Orc, Trait.Humanoid, Trait.Lawful, ModTraits.MeleeMutator],
                0, 9, 5,
                new Defenses(16, 9, 3, 6),
                18,
                new Abilities(3, 0, 3, 0, 2, 0),
                new Skills(athletics: 6, intimidation: 4))
            .WithBasicCharacteristics()
            .AddQEffect(CommonQEffects.MonsterKnockdown())
            .AddHeldItem(new Item(ItemName.SteelShield, IllustrationName.SteelShield, "Steel Shield", 0, 0, Trait.Shield, Trait.Martial).WithMainTrait(Trait.SteelShield).WithShieldProperties(5));

            creature.UnarmedStrike = null;

            UtilityFunctions.AddNaturalWeapon(creature, "flail", IllustrationName.Flail, 7, [Trait.Disarm, Trait.Sweep, Trait.Trip], "1d6+2", DamageKind.Bludgeoning);

            return creature;
        }
    }
}
