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
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using System.Reflection.Metadata;
using static Dawnsbury.Delegates;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class HuntingShark {
        public static Creature Create() {

            Item jaws = NaturalWeapons.Create(NaturalWeaponKind.Jaws, "1d8", DamageKind.Piercing);
            //.WithAdditionalWeaponProperties(p => {
            //    p.WithOnTarget(async (action, user, defender, result) => {
            //        if (result >= CheckResult.Success) {
            //            await CommonAbilityEffects.Grapple(user, defender);
            //        }
            //    });
            //});

            return new Creature(IllustrationName.AnimalFormShark, "Hunting Shark", new List<Trait>() { Trait.Neutral, Trait.Animal, Trait.Aquatic }, 2, 5, 7, new Defenses(17, 11, 8, 5), 40,
            new Abilities(5, 3, 3, 0, 2, 2), new Skills(acrobatics: 6, athletics: 8))
            .WithCharacteristics(false, true)
            .WithProficiency(Trait.Unarmed, Proficiency.Expert)
            .WithUnarmedStrike(jaws)
            //.AddQEffect(new QEffect("Latching Bite", "Creatures hit by the Hunting Shark's jaws attack are grappled.") {
            //    Id = QEffectId.Swimming
            //})
            .AddQEffect(new QEffect() {
                Id = QEffectId.Swimming
            })
            .AddQEffect(new QEffect("Blood Frenzy", "The scent of blood drives the hunting shark into a frenzy, granting it a +2 status bonus to attack and damage rolls against bleeding creatures.") {
                BonusToAttackRolls = (self, action, defender) => defender?.FindQEffect(QEffectId.PersistentDamage)?.Key == $"PersistentDamage:{DamageKind.Bleed.ToStringOrTechnical()}" ? new Bonus(2, BonusType.Status, "Blood Frenzy") : null,
                BonusToDamage = (self, action, defender) => defender.FindQEffect(QEffectId.PersistentDamage)?.Key == $"PersistentDamage:{DamageKind.Bleed.ToStringOrTechnical()}" ? new Bonus(2, BonusType.Status, "Blood Frenzy") : null,
                AdditionalGoodness = (self, action, defender) => {
                    if (defender.FindQEffect(QEffectId.PersistentDamage)?.Name == $"PersistentDamage:{DamageKind.Bleed.ToStringOrTechnical()}") {
                        return 4f;
                    }
                    return 0f;
                }
            });
        }
    }
}