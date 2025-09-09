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
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class Drider {
        public static Creature Create() {
            return new Creature(Illustrations.Drider, "Drider", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Aberration, ModTraits.Spider, Trait.Female, ModTraits.MeleeMutator }, 3, 6, 6, new Defenses(17, 9, 7, 6), 56,
            new Abilities(5, 3, 3, 1, 3, 2), new Skills(athletics: 10, intimidation: 8))
            .WithCreatureId(CreatureIds.Drider)
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Ranged, Proficiency.Expert)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(ItemName.Halberd))
            .WithUnarmedStrike(new Item(IllustrationName.Jaws, "fangs", new Trait[] { Trait.Melee, Trait.Finesse, Trait.Unarmed, Trait.Brawling }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)))
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(QEffect.AttackOfOpportunity())
            .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "fangs")) // Change to drider venom?
            .AddQEffect(CommonQEffects.WebAttack(16))
            .AddQEffect(CommonQEffects.MiniBoss())
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            .AddQEffect(QEffect.WebSense());
        }
    }
}