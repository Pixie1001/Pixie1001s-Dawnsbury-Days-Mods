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
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class EchidnaditeBroodguard {
        public static Creature Create() {
            Creature monster = new Creature(Illustrations.EBroodGuard, "Echidnadite Brood Guard", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Human, Trait.Humanoid, ModTraits.MeleeMutator }, 6, 12, 5, new Defenses(24, 17, 11, 14), 120,
                new Abilities(4, 2, 4, 0, 2, 2), new Skills(athletics: 17, nature: 13))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (pos, you, step, cr) => you.DistanceTo(cr) <= 3 && cr.FriendOf(you) && !cr.HasTrait(Trait.Celestial) && (cr.HasTrait(Trait.Animal) || cr.HasTrait(Trait.Beast)), 0.5f, false);

                    return null;
                };
            })
            .WithCreatureId(CreatureIds.EchidnaditeBroodGuard)
            .WithProficiency(Trait.Weapon, Proficiency.Master)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(ItemName.Ranseur).WithModificationPlusOneStriking())
            .AddQEffect(CommonQEffects.BlessedOfEchidna())
            .AddQEffect(new QEffect("Vengeance", "If there are no monstrous allies left in the battle for the Echidna Brood Guard to protect, it enters a Vengeful Rage, gaining a +4 bonus to damage at the expense of a -2 penalty to AC.") {
                StateCheck = self => {
                    if (!self.Owner.Battle.AllCreatures.Any(cr => CommonQEffects.IsMonsterAlly(self.Owner, cr))) {
                        self.Owner.AddQEffect(new QEffect("Vengeful Rage", "+4 damage, -2 AC and cannot use concentrate actions.", ExpirationCondition.Ephemeral, self.Owner, IllustrationName.Rage) {
                            BonusToDamage = (thisQf, action, target) => !action.HasTrait(Trait.Spell) ? new Bonus(4, BonusType.Untyped, thisQf.Name!) : null,
                            BonusToDefenses = (thisQf, action, def) => def == Defense.AC ? new Bonus(-2, BonusType.Untyped, thisQf.Name!) : null,
                            PreventTakingAction = action => action.HasTrait(Trait.Concentrate) && !action.HasTrait(Trait.Rage) ? "Cannot use concentrate actions" : null,
                        });
                    }
                }
            })
            .AddQEffect(new QEffect("Monstrous Assault", "The Echidna Brood Guard deals an additional 1d6 damage against enemies that are adjacent to a monstrous ally.") {
                YouDealDamageWithStrike = (self, action, diceFormula, defender) => {
                    if (action.HasTrait(Trait.Strike) && defender.Battle.AllCreatures.Any(cr => cr.IsAdjacentTo(defender) && CommonQEffects.IsMonsterAlly(self.Owner, cr))) {
                        defender.Overhead("monstrous assault!", Color.Gainsboro);
                        return diceFormula.Add(DiceFormula.FromText("1d6", "Monstrous Assault"));
                    }
                    return diceFormula;
                }
            })
            .AddQEffect(new QEffect("Combat Disarm", "When your Strike hits, you can spend an action to attempt to disarm your opponent.") {
                ProvideMainAction = self => {
                    Creature monster = self.Owner;
                    IEnumerable<Creature> source = from cr in monster.Battle.AllCreatures.Where(delegate (Creature cr) {
                        CombatAction combatAction = monster.Actions.ActionHistoryThisTurn.LastOrDefault();
                        return combatAction != null && combatAction.CheckResult >= CheckResult.Success && combatAction.HasTrait(Trait.Disarm) && combatAction.ChosenTargets.ChosenCreature == cr;
                    }) select cr;
                    return new SubmenuPossibility(IllustrationName.Disarm, "Disarm") {
                        Subsections = {
                            new PossibilitySection("Disarm") {
                                Possibilities = source.Select((Func<Creature, Possibility>)((lt) =>
                                    (ActionPossibility) new CombatAction(monster, IllustrationName.Disarm, "Disarm " + lt.Name, [Trait.Melee, Trait.AttackDoesNotIncreaseMultipleAttackPenalty, Trait.Attack, Trait.Basic],
                                    "Disarm the target.",
                                    Target.ReachWithWeaponOfTrait(Trait.Disarm)
                                        .WithAdditionalConditionOnTargetCreature((a, d) => (d != lt) ? Usability.CommonReasons.TargetIsNotPossibleForComplexReason : Usability.Usable)
                                        .WithAdditionalConditionOnTargetCreature((a, d) => (a.HasFreeHand || a.WieldsItem(Trait.Disarm) ? Usability.Usable : Usability.CommonReasons.NoFreeHandForManeuver))
                                        .WithAdditionalConditionOnTargetCreature((a, d) => d.HeldItems.Any(hi => !hi.HasTrait(Trait.Grapplee) && !hi.HasTrait(Trait.Shield) && d.GetProficiency(hi) > d.Level) ? Usability.Usable : Usability.CommonReasons.TargetHasNoWeapon)
                                    )
                                    .WithGoodness((t, a, d) => AIConstants.ALWAYS)
                                    .WithItem(monster.PrimaryWeapon!)
                                    .WithActionCost(1)
                                    .WithActiveRollSpecification(new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Athletics), TaggedChecks.DefenseDC(Defense.Reflex)))
                                    .WithEffectOnEachTarget(async delegate(CombatAction ca, Creature a, Creature d, CheckResult cr) {
                                        await CommonAbilityEffects.Disarm(ca, a, d, cr);
                                }))).ToList()
                            }
                        }
                    };
                }
            })
            .AddQEffect(new QEffect("Powerful Disarm", "When you succeed but not critically succeed at a Disarm check, the penalty lasts until the end of the target's turn, not until the start of their turn.") { Id = QEffectId.PowerfulDisarm })
            ;

            monster.AddQEffectAtPriority(CommonQEffects.RetributiveStrike(2, cr => CommonQEffects.IsMonsterAlly(monster, cr), "a monstrous ally", true), true);

            return monster;
        }
    }
}

