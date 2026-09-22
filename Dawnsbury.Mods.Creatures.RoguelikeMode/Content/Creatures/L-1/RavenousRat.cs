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
using Dawnsbury.Display.Illustrations;
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
            Creature monster = new Creature(IllustrationName.GiantRat256, "Abyssal Rat", [Trait.Small, Trait.Animal, Trait.Chaotic, Trait.Evil], 3, 9, 6, new Defenses(19, 8, 11, 7), 42, new Abilities(2, 4, 3, -4, 1, -3), new Skills(acrobatics: 10, athletics: 6, thievery: 10))
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .WithTactics(Tactic.Mindless)
            .WithCharacteristics(speaksCommon: false, hasASkeleton: true)
            .With(delegate (Creature cr) {
                cr.Characteristics.DeathSoundEffect = SfxName.InsectDeath;
            })
            .WithUnarmedStrike(new Item(IllustrationName.Jaws, "jaws", Trait.Agile, Trait.Finesse, Trait.Melee, Trait.Weapon, Trait.Unarmed)
                .WithSoundEffect(SfxName.ZombieAttack2)
                .WithWeaponProperties(new WeaponProperties("2d6", DamageKind.Piercing) { AdditionalDamage = { ("1d6", DamageKind.Acid) } }));
            monster.Traits.Add(Trait.Fiend);
            monster.Traits.Add(Trait.Demon);
            monster.Traits.Add(Trait.NonSummonable);
            monster.Traits.Add(ModTraits.MeleeMutator);
            monster.AddQEffect(QEffect.PackAttack("abyssal rat", "1d4"));
            monster.WithTactics(Tactic.PackAttack);
            //monster.CreatureId = CreatureIds.RavenousRat;
            monster.AddQEffect(QEffect.DamageWeakness(DamageKind.Good, 3));
            monster.AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 3));
            return monster;
        }
    }
}