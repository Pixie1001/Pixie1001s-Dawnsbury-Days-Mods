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
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowRenegade {
        public static Creature Create() {
            return new Creature(Illustrations.DrowRenegade, "Drow Renegade", new List<Trait>() { Trait.Good, Trait.Elf, Trait.Humanoid, Trait.Female, ModTraits.Drow }, 1, 7, 5, new Defenses(16, 10, 7, 7), 25, new Abilities(4, 2, 3, 1, 1, 2), new Skills(deception: 7, athletics: 9)) {
                SpawnAsFriends = true
            }
                .WithBasicCharacteristics()
                .WithProficiency(Trait.Melee, Proficiency.Expert)
                .AddHeldItem(Items.CreateNew(ItemName.Greatsword))
                .AddQEffect(QEffect.AttackOfOpportunity())
                .AddQEffect(CommonQEffects.Drow())
                .AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.Moonbeam, "Crescent Moon Strike", new Trait[] { Trait.Magical, Trait.Divine }, "...", Target.Cone(5).WithIncludeOnlyIf((area, cr) => cr.OwningFaction.IsEnemy)) {
                            ShortDescription = "Deal 3d6 fire damage (basic Reflex save) to each enemy creature within a 25ft cone. On a critical failure, targets are dazzled for 1 round. The Drow Renegade cannot use this attack again for 1d4 rounds."
                        }
                        .WithSavingThrow(new SavingThrow(Defense.Reflex, cr => cr.Level + cr.Abilities.Charisma + 4 + 10))
                        .WithActionCost(2)
                        .WithProjectileCone(IllustrationName.Moonbeam, 15, ProjectileKind.Cone)
                        .WithSoundEffect(SfxName.DivineLance)
                        .WithEffectOnEachTarget(async (spell, user, defender, result) => {
                            await CommonSpellEffects.DealBasicDamage(spell, user, defender, result, DiceFormula.FromText("3d6", "Crescent Moon Strike"), DamageKind.Fire);
                            if (result == CheckResult.CriticalFailure) {
                                defender.AddQEffect(QEffect.Dazzled().WithExpirationAtStartOfSourcesTurn(user, 1));
                            }
                        })
                        .WithEffectOnSelf(user => {
                            user.AddQEffect(QEffect.Recharging("Crescent Moon Strike"));
                            //user.AddQEffect(QEffect.CannotUseForXRound("Crescent Moon Strike", user, DiceFormula.FromText("1d4").Roll().Item1 + 1));
                        })
                        .WithGoodnessAgainstEnemy((cone, a, d) => {
                            return 3.5f * 3 + (d.QEffects.FirstOrDefault(qf => qf.Name == "Dazzled" || qf.Id == QEffectId.Blinded) == null ? 2f : 0f);
                        })
                        ;
                    }
                });
        }
    }
}