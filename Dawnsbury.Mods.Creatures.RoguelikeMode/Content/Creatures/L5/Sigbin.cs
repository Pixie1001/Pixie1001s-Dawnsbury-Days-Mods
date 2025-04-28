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
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.AuraAnimations;
using System;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Sigbin {
        public static Creature Create() {
            var creature = new Creature(IllustrationName.BloodWolf256,
                "Winter Wolf",
                [Trait.Evil, Trait.Beast, ModTraits.MeleeMutator],
                5, 12, 5,
                new Defenses(21, 13, 16, 8),
                75,
                new Abilities(1, 5, 4, -1, 1, 2),
                new Skills(acrobatics: 12, athletics: 10, stealth: 10))
            .WithCreatureId(CreatureIds.WinterWolf)
            .WithUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Claw, "2d4", DamageKind.Slashing, [Trait.Finesse, Trait.Agile, Trait.RogueWeapon])
                .WithMonsterWeaponSpecialization(4))
            .WithUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Fang, "2d6", DamageKind.Piercing, [Trait.Finesse, Trait.RogueWeapon])
                .WithMonsterWeaponSpecialization(4))
            .WithUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Tail, "2d4", DamageKind.Bludgeoning, [Trait.Finesse, Trait.Agile, Trait.Reach])
                .WithMonsterWeaponSpecialization(4)
                .WithAdditionalWeaponProperties(wp => {
                    wp.WithOnTarget(async (action, a, d, r) => {
                        if (r >= CheckResult.Success)
                            d.AddQEffect(QEffect.Prone());
                    });
                }))
            .WithCharacteristics(true, true)
            .WithProficiency(Trait.Unarmed, Proficiency.Trained)
            .WithProficiency(Trait.RogueWeapon, Proficiency.Expert)
            .AddQEffect(new QEffect("Stench", "(aura, olfactory) 30 feet. A creature that starts their turn within the emanation must attempt a DC 14 + the Sigbin's level Fortitude save." +
            " On a failure, the creature is sickened 1, or sickened 2 on a critical failure. While within the aura, the creature takes a –2 circumstance penalty to saves" +
            " to recover from the sickened condition. A creature that succeeds at its save is immune to all sigbins' stenches for the rest of the encounter.") {
                StateCheck = self => {
                    var blessOrigin = new QEffect("Stench", QEffect.NoDescription, ExpirationCondition.Never, self.Owner, IllustrationName.None);
                    blessOrigin.StateCheckWithVisibleChanges = async qfStench => {
                        foreach (var stenchTarget in qfStench.Owner.Battle.AllCreatures.Where(cr =>
                                     cr.DistanceTo(qfStench.Owner) <= 6
                                     && !cr.HasEffect(QEffectId.OutOfCombat)
                                     && !cr.HasTrait(Trait.Object))) {
                            if (stenchTarget.QEffects.Any(qf => qf.ImmuneToTrait == Trait.Olfactory)) continue;
                            if (stenchTarget.HasEffect(QEffectIds.SigbinStenchImmunity)) continue;

                            stenchTarget.AddQEffect(new QEffect("Sigbin Stench", "You take a -2 circumstance penalty to saves made to recover from the sickened conditon.", ExpirationCondition.Ephemeral, qfStench.Owner, self.Owner.Illustration) {
                                Key = "SigbinStench",
                                BonusToDefenses = (_, action, def) => action?.ActionId == ActionId.Retch ? new Bonus(-2, BonusType.Circumstance, "Sigbin Stench") : null,
                                EndOfYourTurnDetrimentalEffect = async (_, you) => {
                                    var result = CommonSpellEffects.RollSavingThrow(you, CombatAction.CreateSimple(self.Owner, "Sigbin Stench"), Defense.Fortitude, 14 + self.Owner.Level);
                                    if (result == CheckResult.CriticalFailure)
                                        you.AddQEffect(QEffect.Sickened(2, 14 + self.Owner.Level));
                                    if (result == CheckResult.Failure)
                                        you.AddQEffect(QEffect.Sickened(1, 14 + self.Owner.Level));
                                    if (result >= CheckResult.Success)
                                        you.AddQEffect(new QEffect("Sigbin Stench Immunity", "Immune to Sigbin Stench for the rest of the encounter",
                                            ExpirationCondition.Never, null, new SameSizeDualIllustration(IllustrationName.Sickened, IllustrationName.RedWarning)) {Id = QEffectIds.SigbinStenchImmunity});
                                }
                            });
                        }
                    };
                }
            })
            .AddQEffect(new QEffect("Powerful Tail", "A successful strike made using the Sigbin's tail strike knocks the target prone."))
            ;

            creature.AnimationData.AddAuraAnimation(new MagicCircleAuraAnimation(IllustrationName.KineticistAuraCircle, Color.GreenYellow, 6));

            return creature;
        }
    }
}
