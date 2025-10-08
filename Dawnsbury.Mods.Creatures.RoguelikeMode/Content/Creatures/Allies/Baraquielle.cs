using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
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
using System.Runtime.CompilerServices;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    public class Baraquielle {
        public static Creature Create(Encounter? encounter) {
            var encounterLevel = UtilityFunctions.GetEncounterLevel(encounter);
            bool highLevel = encounterLevel > 4;
            var hp = !highLevel ? 25 : 95;
            var defenses = !highLevel ? new Defenses(19, 11, 8, 8) : new Defenses(25, 16, 14, 14);
            var level = !highLevel ? 2 : 6;
            var skills = !highLevel ? new Skills(acrobatics: 10, athletics: 12, intimidation: 12, stealth: 12) : new Skills(acrobatics: 13, athletics: 15, intimidation: 15, stealth: 15);
            var abilities = !highLevel ? new Abilities(4, 2, 3, 2, 3, 3) : new Abilities(5, 2, 4, 2, 4, 4);
            var weapon = Items.CreateNew(ItemName.Warhammer);
            var spellAtk = !highLevel ? 3 : 18;
            var perception = !highLevel ? 11 : 17;

            if (highLevel) {
                weapon = weapon.WithModificationRune(ItemName.FlamingRunestone);
                weapon.WeaponProperties!.DamageDieCount = 2;
                weapon.WeaponProperties!.ItemBonus = 2;
            }

            var angel = new Creature(Illustrations.Baraquielle, "Baraquielle", [Trait.Lawful, Trait.Good, Trait.Angel, Trait.Celestial, Trait.MetalArmor, Trait.Female, Trait.NoDeathOverhead, Trait.NoDeathScream], level, perception, 7, defenses, hp, abilities, skills)
                .WithSpawnAsGaiaFriends()
                .WithCreatureId(CreatureIds.Baraquielle)
                .WithBasicCharacteristics()
                .WithProficiency(Trait.Melee, Proficiency.Expert)
                .WithLargeIllustration(IllustrationName.BaraquielleLarge)
                .AddHeldItem(weapon)
                .AddHeldItem(Items.CreateNew(!highLevel ? ItemName.SteelShield : ItemName.SturdyShield10))
                .AddQEffect(QEffect.AttackOfOpportunity())
                .AddQEffect(QEffect.Flying())
                .AddQEffect(new QEffect() {
                    PreventDeathDueToDyingAsync = async (self, dying) => {
                        if (self.Owner.Battle.Encounter.Name == "Defend the Reliquary") {
                            self.Owner.Battle.Cinematics.EnterCutscene();
                            await self.Owner.Battle.Cinematics.LineAsync(self.Owner, "I grow weak! You must continue the fight without me, heroes. The reliquary must not be destroyed!");
                            self.Owner.Battle.Cinematics.ExitCutscene();
                        }
                        Sfxs.Play(SfxName.PhaseBolt);
                        self.Owner.AnimationData.ColorBlinkFast(Color.Yellow);
                        self.Owner.Overhead("*retreats*", Color.Black, self.Owner.Name + " planes shifts away to safety.");
                        return false;
                    }
                })
                .WithFeat(FeatName.ShieldBlock)
                .WithFeat(FeatName.ReactiveShield)
                .WithFeat(FeatName.AggressiveBlock)
                .WithSpellProficiencyBasedOnSpellAttack(spellAtk, Ability.Charisma)
                .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Champion, Ability.Charisma, Trait.Divine)
                .WithSpells(level1: [SpellId.TrueStrike, SpellId.TrueStrike, SpellId.TrueStrike],
                    level3: highLevel ? [SpellId.SearingLight] : []).Done()
                ;

            if (highLevel) {
                angel.AddQEffect(new QEffect() {
                    YouHaveCriticalSpecialization = (self, weapon, combatAction, defender) => weapon.HasTrait(Trait.Warhammer)
                });
                angel.AddQEffect(QEffect.DamageWeakness(DamageKind.Evil, 10));
                angel.AddQEffect(new QEffect($"Phantasmal Doorknob", $"On a critical hit with the warhammer the target is dazzled until the end of its next turn.") {
                    AfterYouTakeActionAgainstTarget = async (effect, action, defender, checkResult) => {
                        if (checkResult >= CheckResult.CriticalSuccess && action.Item?.ItemName == ItemName.Warhammer) {
                            var qf = QEffect.Dazzled();
                            qf.WithExpirationAtEndOfOwnerTurn();
                            qf.CannotExpireThisTurn = true;
                            defender.AddQEffect(qf);
                        }
                    }
                });
            } else {
                angel.AddQEffect(QEffect.DamageWeakness(DamageKind.Evil, 5));
            }

            if (encounterLevel == 1 || encounterLevel == 5)
                angel.ApplyWeakAdjustments(false);
            else if (encounterLevel == 3 || encounterLevel == 7)
                angel.ApplyEliteAdjustments(false);

            return angel;
        }
    }
}