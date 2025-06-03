using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.AuraAnimations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Kineticist;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using System;
using System.Xml.Linq;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowHighPriestess {
        private const int AURA_SIZE = 3;

        public static Creature Create() {
            Creature monster = new Creature(Illustrations.DrowPriestess, "Drow High Priestess",
                [Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid, Trait.Female, ModTraits.Drow, ModTraits.SpellcasterMutator],
                7, 12, 8, new Defenses(24, 12, 15, 18), 115,
            new Abilities(5, 4, 3, 2, 4, 4),
            new Skills(athletics: 17, acrobatics: 16, deception: 16, religion: 16, occultism: 12, intimidation: 16))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;
                    AiFuncs.PositionalGoodness(monster, options, (pos, you, step, them) => them.HasTrait(Trait.Demon) && them.FriendOf(you) && pos.DistanceTo(them.Occupies) <= AURA_SIZE, 1f, false);

                    return null;
                };
            })
            .WithCreatureId(CreatureIds.DrowHighPriestess)
            .WithProficiency(Trait.Melee, Proficiency.Master)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .AddHeldItem(Items.CreateNew(CustomItems.GreaterScourgeOfFangs))
            .WithBasicCharacteristics()
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(CommonQEffects.DrowClergy())
            .AddQEffect(QEffect.SneakAttack("1d6"))
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Cleric, Ability.Wisdom, Trait.Divine).WithSpells(
                level1: [SpellId.Shield, SpellId.Fear, SpellId.Fear, SpellId.Fear],
                level2: [SpellId.BloodVendetta, SpellId.BloodVendetta, SpellId.Blur],
                level3: [SpellId.BloodVendetta, SpellId.Harm, SpellId.Harm],
                level4: [SpellId.SuddenBlight, SpellId.SuddenBlight]).Done()
            .Builder
            .AddMainAction(you => {
                return new CombatAction(you, IllustrationName.ProfaneCircle, "Abyssal Summoning", [Trait.Evil, Trait.Conjuration, Trait.Summon], "The drow high priestess conjures worth a demonic ally to serve her. She can't use Abyssal Summoning again for 1d4 rounds.", Target.RangedEmptyTileForSummoning(3))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.Summoning)
                .WithEffectOnChosenTargets(async (spell, caster, targets) => {
                    caster.AddQEffect(QEffect.Recharging("Abyssal Summoning", Dice.D4));

                    var list = MonsterStatBlocks.MonsterExemplars.Where(pet => pet.HasTrait(Trait.Demon) && CommonEncounterFuncs.Between(pet.Level, caster.Level - 4, caster.Level - 1) && !pet.HasTrait(Trait.NonSummonable)).ToArray();

                    Random rand = new Random();

                    if (list.Count() <= 0) {
                        caster.Occupies.Overhead("*summon failed*", Color.White, $"There are no valid demons for {caster.Name} to summon.");
                        return;
                    }

                    Creature demon = MonsterStatBlocks.MonsterFactories[list[rand.Next(0, list.Count())].Name](caster.Battle.Encounter, targets.ChosenTile!);

                    if (demon.Level - caster.Level >= 0) {
                        demon.ApplyWeakAdjustments(false, true);
                    } else if (demon.Level - caster.Level == -1) {
                        demon.ApplyWeakAdjustments(false);
                    } else if (demon.Level - caster.Level == -3) {
                        demon.ApplyEliteAdjustments();
                    } else if (demon.Level - caster.Level == -4) {
                        demon.ApplyEliteAdjustments(true);
                    }

                    caster.Battle.SpawnCreature(demon, caster.OwningFaction, targets.ChosenTile!);
                    demon.AnimationData.ColorBlink(Color.Purple);
                })
                ;
            })
            .Done();

            var aura = new QEffect("Profane Aura", $"(aura, evil) {AURA_SIZE * 5} feet. Demons within the aura gain a +1 status bonus to attack and AC, and a +4 bonus to damage.") {

            };

            aura.AddGrantingOfTechnical(cr => cr.HasTrait(Trait.Demon) && cr.FriendOfAndNotSelf(monster) && cr.DistanceTo(monster) <= AURA_SIZE, qfTech => {
                qfTech.BonusToAttackRolls = (self, action, defender) => new Bonus(1, BonusType.Status, "profane aura", true);
                qfTech.BonusToDamage = (self, action, defender) => new Bonus(4, BonusType.Untyped, "profane aura", true);
                qfTech.BonusToDefenses = (self, action, def) => def == Defense.AC ? new Bonus(1, BonusType.Status, "profane aura", true) : null;
                qfTech.Name = "Profane Aura";
                qfTech.Description = "You gain a +1 status bonus to attack and AC, and a +4 bonus to damage.";
                qfTech.Illustration = IllustrationName.ProfaneCircle;
            });

            monster.AddQEffectAtPriority(aura, true);
            var animation = new MagicCircleAuraAnimation(Illustrations.BaneCircleWhite, Color.PaleVioletRed, AURA_SIZE);
            animation.DecreaseOpacityAsSizeIncreases = true;
            monster.AnimationData.AddAuraAnimation(animation);

            return monster;
        }

        private static ValueTuple<int, bool> Test() {
            return (1, true);
        }
    }
}
