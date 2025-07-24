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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    public static class DeepBeast {
        public static Creature Create() {
            var creature = new Creature(Illustrations.DeepBeast,
                "Deep Beast",
                [Trait.Animal, Trait.Aberration, ModTraits.MeleeMutator],
                5, 12, 7,
                new Defenses(22, 14, 10, 10),
                73,
                new Abilities(5, 1, 5, -4, 1, -1),
                new Skills(athletics: 14, stealth: 14))
            .WithCreatureId(CreatureIds.CavernBeast)
            .WithCharacteristics(false, true)
            .AddQEffect(QEffect.SneakAttack("1d6"))
            .AddQEffect(QEffect.MonsterGrab())
            .AddQEffect(QEffect.TraitImmunity(Trait.Visual))
            .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Blinded))
            .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Dazzled))
            .AddQEffect(new QEffect("Blind Sight", "") { Id = QEffectId.SeeInvisibility })
            .AddQEffect(new QEffect("Mauler", "This creature gains a +3 circumstance bonus to damage rolls against creatures it has grabbed.") {
                BonusToDamage = (self, action, d) => action.HasTrait(Trait.Strike) && d.QEffects.Any(qf => qf.Id == QEffectId.Grappled && qf.Source == self.Owner) ? new Bonus(3, BonusType.Circumstance, "Mauler") : null
            })
            .Builder
            .AddNaturalWeapon(NaturalWeaponKind.Jaws, 15, [], "2d8+7", DamageKind.Piercing)
            .AddNaturalWeapon(NaturalWeaponKind.Claw, 15, [Trait.Agile, Trait.Grab], "2d6+7", DamageKind.Slashing)
            .Done();

            return creature;
        }
    }
}
