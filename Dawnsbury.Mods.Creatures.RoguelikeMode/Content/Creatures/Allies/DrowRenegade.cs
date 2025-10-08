using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
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
    public class DrowRenegade {
        public static Creature Create(Encounter? encounter) {
            bool highLevel = UtilityFunctions.GetEncounterLevel(encounter) > 4;
            var hp = !highLevel ? 25 : 95;
            var defenses = !highLevel ? new Defenses(16, 10, 7, 7) : new Defenses(22, 15, 12, 12);
            var level = !highLevel ? 1 : 5;
            var skills = !highLevel ? new Skills(deception: 7, athletics: 9) : new Skills(deception: 11, athletics: 13);
            var abilities = !highLevel ? new Abilities(4, 2, 3, 1, 1, 2) : new Abilities(5, 3, 4, 1, 1, 3);
            var weapon = Items.CreateNew(ItemName.Greatsword);
            var dmgDice = !highLevel ? 3 : 6;
            var perception = !highLevel ? 7 : 12;

            if (highLevel) {
                weapon.WeaponProperties!.DamageDieCount = 2;
                weapon.WeaponProperties!.ItemBonus = 2;
            }

            return new Creature(Illustrations.DrowRenegade, "Drow Renegade", new List<Trait>() { Trait.Good, Trait.Elf, Trait.Humanoid, Trait.Female, ModTraits.Drow }, level, perception, 5, defenses, hp, abilities, skills)
                .WithSpawnAsGaiaFriends()
                .WithBasicCharacteristics()
                .WithProficiency(Trait.Melee, Proficiency.Expert)
                .AddHeldItem(weapon)
                .AddQEffect(QEffect.AttackOfOpportunity())
                .AddQEffect(CommonQEffects.Drow())
                .AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.Moonbeam, "Crescent Moon Strike", new Trait[] { Trait.Magical, Trait.Divine, Trait.Silver },
                        $"Deal {dmgDice}d6 fire damage (basic Reflex save) to each enemy creature within a 25ft cone. On a critical failure, targets are dazzled for 1 round. The Drow Renegade cannot use this attack again for 1d4 rounds.",
                        Target.Cone(5).WithIncludeOnlyIf((area, cr) => cr.OwningFaction.IsEnemy)) {
                            ShortDescription = $"Deal {dmgDice}d6 fire damage (basic Reflex save) to each enemy creature within a 25ft cone. On a critical failure, targets are dazzled for 1 round. The Drow Renegade cannot use this attack again for 1d4 rounds."
                        }
                        .WithSavingThrow(new SavingThrow(Defense.Reflex, cr => cr != null ? cr.Level + cr.Abilities.Charisma + 4 + 10 : 10))
                        .WithActionCost(2)
                        .WithProjectileCone(IllustrationName.Moonbeam, 15, ProjectileKind.Cone)
                        .WithSoundEffect(SfxName.DivineLance)
                        .WithEffectOnEachTarget(async (spell, user, defender, result) => {
                            await CommonSpellEffects.DealBasicDamage(spell, user, defender, result, DiceFormula.FromText(dmgDice + "d6", "Crescent Moon Strike"), DamageKind.Fire);
                            if (result == CheckResult.CriticalFailure) {
                                defender.AddQEffect(QEffect.Dazzled().WithExpirationAtStartOfSourcesTurn(user, 1));
                            }
                        })
                        .WithEffectOnSelf(user => {
                            user.AddQEffect(QEffect.Recharging("Crescent Moon Strike"));
                        })
                        .WithGoodnessAgainstEnemy((cone, a, d) => {
                            return 3.5f * dmgDice + (d.QEffects.FirstOrDefault(qf => qf.Name == "Dazzled" || qf.Id == QEffectId.Blinded) == null ? 2f : 0f);
                        })
                        ;
                    }
                });
        }
    }
}