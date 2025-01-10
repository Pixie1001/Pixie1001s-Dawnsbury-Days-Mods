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
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowSniper {
        public static Creature Create() {
            return new Creature(Illustrations.DrowSniper, "Drow Sniper", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid }, 1, 10, 6, new Defenses(15, 4, 10, 7), 18,
            new Abilities(-1, 4, 1, 1, 2, 2), new Skills(acrobatics: 7, stealth: 7, deception: 7, intimidation: 5))
            .AddQEffect(CommonQEffects.Drow())
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Melee, Proficiency.Trained)
            .WithProficiency(Trait.Ranged, Proficiency.Master)
            .AddHeldItem(Items.CreateNew(ItemName.Longbow))
            .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "longbow"));
        }
    }
}