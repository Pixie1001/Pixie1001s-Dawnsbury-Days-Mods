using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.AuraAnimations;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Champion;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
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
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    public class Lyra {
        public static Creature Create(Encounter? encounter) {
            var encounterLevel = UtilityFunctions.GetEncounterLevel(encounter);
            bool highLevel = encounterLevel > 4;
            var hp = !highLevel ? 25 : 95;
            var defenses = !highLevel ? new Defenses(19, 11, 8, 8) : new Defenses(25, 16, 14, 14);
            var level = !highLevel ? 2 : 6;
            var skills = !highLevel ? new Skills(acrobatics: 10, athletics: 12, intimidation: 12, stealth: 12) : new Skills(acrobatics: 13, athletics: 15, intimidation: 15, stealth: 15);
            var abilities = !highLevel ? new Abilities(3, 2, 4, 2, 3, 4) : new Abilities(4, 3, 5, 2, 3, 5);
            var weapon = Items.CreateNew(!highLevel ? ItemName.SteelShield : ItemName.SturdyShield10).WithAdditionalWeaponProperties(wp => {
                wp.WithAdditionalDamage(!highLevel ? "1" : "1d6", DamageKind.Chaotic);
                wp.WithAdditionalDamage(!highLevel ? "1" : "1d6", DamageKind.Good);
            });
            var tail = new Item(Illustrations.SerpentileTail, "tail", [Trait.Grab, Trait.Reach, Trait.Agile, Trait.Magical, Trait.Chaotic])
                .WithWeaponProperties(new WeaponProperties((!highLevel ? 1 : 2) + "d6", DamageKind.Bludgeoning))
                .WithAdditionalWeaponProperties(wp => {
                    wp.WithAdditionalDamage(!highLevel ? "1" : "1d6", DamageKind.Chaotic);
                    wp.WithAdditionalDamage(!highLevel ? "1" : "1d6", DamageKind.Good);
                    wp.ItemBonus = !highLevel ? 0 : 1;
                });
            var spellAtk = !highLevel ? 3 : 18;
            var perception = !highLevel ? 11 : 17;

            var angel = new Creature(Illustrations.Lyra, "Azata Mediator", [Trait.Chaotic, Trait.Good, ModTraits.Azata, Trait.Celestial, Trait.Female], level, perception, 9, defenses, hp, abilities, skills)
                .WithSpawnAsGaiaFriends()
                .WithBasicCharacteristics()
                .WithProficiency(Trait.Melee, Proficiency.Expert)
                .WithCreatureId(CreatureIds.AzataMediator)
                .AddHeldItem(weapon)
                .WithUnarmedStrike(tail)
                .AddQEffect(QEffect.Flying())
                .WithFeat(FeatName.ShieldBlock)
                .WithFeat(FeatName.ShieldWarden)
                //.AddQEffect(new QEffect("Deadly Simplicity: Steel Shield", "The damage die of this weapon increases by one step.") {
                //    IncreaseItemDamageDie = (qf, weapon) => weapon.BaseItemName == ItemName.SteelShield || weapon.BaseItemName == ItemName.SturdyShield10
                //})
                .AddQEffect(QEffect.MonsterGrab())
                .AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.InspireCourage, "Inspire Friends", [Trait.Cantrip, Trait.Flourish, Trait.Divine, Trait.Emotion, Trait.Enchantment, Trait.Mental, Trait.Composition, Trait.SpellWithDuration, Trait.NoHeightening],
                        $"Lyra inspires all of her new friends within a 60ft aura, granting them a +2 bonus to attack rolls, damage rolls and saves against fear effects.",
                        Target.Self()) {
                            ShortDescription = $"Lyra inspires all of her new friends within a 60ft aura, granting them a +2 bonus to attack rolls, damage rolls and saves against fear effects."
                        }
                        .WithSoundEffect(SfxName.Harp)
                        .WithActionCost(1)
                        .WithGoodness((t, a, d) => a.Battle.AllCreatures.Where(cr => cr.FriendOf(a) && a.DistanceTo(cr) <= 12).Count() * 10f)
                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                            int effectDuration = 1;
                            int statusBonus = 2;
                            var qfInspireCourage = new QEffect($"Performance ({spell.Name})", $"You are granting a +{statusBonus} status bonus to attack rolls, damage rolls and saves against fear effects to all allies within a 60-foot aura.", ExpirationCondition.ExpiresAtStartOfSourcesTurn, caster, IllustrationName.PerformingComposition) {
                                Id = QEffectId.OngoingComposition,
                                CannotExpireThisTurn = true,
                                SpawnsAura = _ => new MagicCircleAuraAnimation(Illustrations.BaneCircleWhite, Color.Pink, 12f),
                                StateCheck = sc => {
                                    var bard = sc.Owner;
                                    foreach (var ally in bard.Battle.AllCreatures.Where(cr => cr.FriendOf(bard) && cr.DistanceTo(bard) <= 12 && !cr.IsImmuneTo(Trait.Mental)).ToList()) {
                                        var ephemeralQEffect = new QEffect(spell.Name, $"You have +{statusBonus} status bonus to attack rolls, damage rolls and saves against fear effects.", ExpirationCondition.Ephemeral, bard, IllustrationName.InspireCourage) {
                                            CountsAsABuff = true,
                                        };
                                        ephemeralQEffect.BonusToAttackRolls = (effect, action, defender) => action.HasTrait(Trait.Attack) ? new Bonus(statusBonus, BonusType.Status, spell.Name) : null;
                                        ephemeralQEffect.BonusToDamage = (effect, action, defender) => new Bonus(statusBonus, BonusType.Status, spell.Name);
                                        ephemeralQEffect.BonusToDefenses = (effect, action, defense) => defense.IsSavingThrow() && action != null && action.HasTrait(Trait.Fear) ? new Bonus(statusBonus, BonusType.Status, spell.Name) : null;
                                        ally.AddQEffect(ephemeralQEffect);
                                    }
                                }
                            }.WithExpirationAtStartOfSourcesTurn(caster, effectDuration);
                            qfInspireCourage.CountsAsABuff = true;
                            caster.AddQEffect(qfInspireCourage);
                        });
                    }
                });

            if (highLevel) {
                angel.AddQEffect(QEffect.DamageWeakness(DamageKind.Evil, 10));
                angel.AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Bard, Ability.Charisma, Trait.Divine)
                .WithSpells(
                    level1: [SpellId.Daze, SpellId.HauntingHymn],
                    level2: [SpellId.Soothe, SpellId.Soothe, SpellId.Soothe],
                    level3: [SpellId.SoundBurst]).Done()
                ;
                angel.WithSpellProficiencyBasedOnSpellAttack(spellAtk, Ability.Charisma);
            } else {
                angel.AddQEffect(QEffect.DamageWeakness(DamageKind.Evil, 5));
                angel.AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Bard, Ability.Charisma, Trait.Divine)
                .WithSpells(level1: [SpellId.Daze, SpellId.HauntingHymn, SpellId.Soothe, SpellId.Soothe]).Done()
                ;
                angel.WithSpellProficiencyBasedOnSpellAttack(spellAtk, Ability.Charisma);
            }

            if (encounterLevel == 1 || encounterLevel == 5)
                angel.ApplyWeakAdjustments(false);
            else if (encounterLevel == 3 || encounterLevel == 7)
                angel.ApplyEliteAdjustments(false);

            return angel;
        }
    }
}