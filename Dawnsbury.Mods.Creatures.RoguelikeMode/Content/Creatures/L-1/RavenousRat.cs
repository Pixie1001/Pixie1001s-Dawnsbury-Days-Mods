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
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Monsters.L_1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class RavenousRat {
        public static Creature Create() {
            Creature monster = GiantRat.CreateGiantRat();
            monster.MainName = "Ravenous Rat";
            monster.Traits.Add(Trait.NonSummonable);
            monster.Traits.Add(ModTraits.MeleeMutator);
            monster.AddQEffect(QEffect.PackAttack("ravenous rat", "1d4"));
            monster.WithTactics(Tactic.PackAttack);
            monster.CreatureId = CreatureIds.RavenousRat;
            return monster;
        }

        public static Creature CreateAbyssalRat() {
            Creature monster = GiantRat.CreateGiantRat();
            monster.Level = 3;
            monster.Perception = 9;
            monster.MaxHP = 42;
            monster.Defenses = new Defenses(19, 8, 11, 7);
            monster.UnarmedStrike = new Item(IllustrationName.Jaws, "jaws", Trait.Agile, Trait.Finesse, Trait.Melee, Trait.Weapon, Trait.Unarmed)
                .WithSoundEffect(SfxName.ZombieAttack2)
                .WithWeaponProperties(new WeaponProperties("2d6", DamageKind.Piercing) { AdditionalDamage = { ("1d6", DamageKind.Acid) } });
            monster.MainName = "Abyssal Rat";
            monster.Traits.Add(Trait.Fiend);
            monster.Traits.Add(Trait.Demon);
            monster.Traits.Add(Trait.NonSummonable);
            monster.Traits.Add(ModTraits.MeleeMutator);
            monster.AddQEffect(QEffect.PackAttack("abyssal rat", "1d4"));
            monster.WithTactics(Tactic.PackAttack);
            monster.CreatureId = CreatureIds.RavenousRat;
            monster.AddQEffect(QEffect.DamageWeakness(DamageKind.Good, 3));
            monster.AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 3));
            return monster;
        }
    }
}