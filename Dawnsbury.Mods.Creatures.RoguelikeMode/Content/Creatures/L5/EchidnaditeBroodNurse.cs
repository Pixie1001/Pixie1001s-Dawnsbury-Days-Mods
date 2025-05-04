using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using System.Threading;
using System;
using static System.Collections.Specialized.BitVector32;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Campaign.Path;
using Microsoft.Xna.Framework;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.Intelligence;
using System.Collections.Generic;
using Dawnsbury.Core.Mechanics.Rules;
using System.Reflection.Metadata;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class EchidnaditeBroodNurse {
        public static Creature Create() {
            Creature monster = new Creature(Illustrations.EBroodNurse, "Echidnadite Brood Nurse", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Human, Trait.Humanoid, ModTraits.SpellcasterMutator }, 5, 12, 5, new Defenses(21, 15, 9, 12), 55,
                new Abilities(3, 2, 4, 2, 5, 3), new Skills(nature: 13, medicine: 15))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (pos, you, step, cr) => you.DistanceTo(cr) <= 2 && cr.FriendOf(you) && !cr.HasTrait(Trait.Celestial) && (cr.HasTrait(Trait.Animal) || cr.HasTrait(Trait.Beast)), 0.5f, false);

                    return null;
                };
            })
            .WithCreatureId(CreatureIds.EchidnaditeBroodNurse)
            .WithProficiency(Trait.Weapon, Proficiency.Master)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .AddHeldItem(Items.CreateNew(ItemName.Dagger).WithModificationPlusOneStriking())
            .WithBasicCharacteristics()
            .AddQEffect(CommonQEffects.MothersProtection())
            .AddQEffect(CommonQEffects.BlessedOfEchidna())
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Cleric, Ability.Wisdom, Trait.Divine).WithSpells(
                [SpellId.Guidance, SpellId.RayOfEnfeeblement, SpellId.RayOfEnfeeblement, SpellId.RayOfEnfeeblement],
                [SpellId.Heal, SpellId.Heal, SpellId.Heal],
                [SpellId.BoneSpray]).Done()
            .Builder
            .AddMainAction(you => new CombatAction(you, IllustrationName.HealersTools, "Battle Veterinarianism", [Trait.Manipulate, Trait.Healing, Trait.Flourish],
                "Make a DC 20 medicine check. On a success, target adjacent monster is healed for 2d8 + 10 HP, or 4d8 + 10 HP on a critical success.",
                Target.AdjacentFriend().WithAdditionalConditionOnTargetCreature((a, d) => CommonQEffects.IsMonsterAlly(a, d) ? Usability.Usable : Usability.NotUsableOnThisCreature("not a monster")))
            .WithActionCost(1)
            .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Medicine), Checks.FlatDC(20)))
            .WithGoodness((_, a, d) => a.AI.Heal(d, 18))
            .WithEffectOnEachTarget(async (action, user, target, result) => {
                if (result >= CheckResult.Success)
                    await target.HealAsync((result == CheckResult.CriticalSuccess ? 4 : 2) + "d8+10", action);
            })
            )
            .Done()
            ;

            var passive = new QEffect("Monster Veterinarian", "After healing a monster, they regain an additional +10 HP, and gain the Divine Hide condition, granting them a +2 bonus to AC until the start of your next turn.") {
                AdditionalGoodness = (self, action, target) => {
                    if (action.HasTrait(Trait.Healing) && CommonQEffects.IsMonsterAlly(self.Owner, target) && target.Damage >= 20) {
                        return 15f;
                    }

                    return 0;
                }
            };
            passive.AddGrantingOfTechnical(cr => CommonQEffects.IsMonsterAlly(monster, cr), qfTech => {
                qfTech.AfterYouAreHealed = async (self, action, amount) => {
                    if (action?.Owner?.CreatureId == CreatureIds.EchidnaditeBroodNurse && action.Name != "Brood Nurse") {
                        self.Owner.AddQEffect(new QEffect("Divine Hide", "You gain a +2 bonus to AC.", ExpirationCondition.ExpiresAtStartOfSourcesTurn, action.Owner, IllustrationName.MagicHide) {
                            BonusToDefenses = (thisQf, _, def) => def == Defense.AC ? new Bonus(2, BonusType.Untyped, thisQf.Name) : null,
                            Key = "Divine Hide"
                        });

                        await self.Owner.HealAsync(DiceFormula.FromText("10", "Brood Nurse"), CombatAction.CreateSimple(action.Owner, "Brood Nurse"));
                    }
                };
            });
            monster.AddQEffect(passive);
            return monster;
        }
    }
}

