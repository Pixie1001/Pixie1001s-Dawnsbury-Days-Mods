using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class YoungRedDragon {
        public static Creature Create() {
            return new Creature(Illustrations.RedDragon, "Young Red Dragon", [Trait.Chaotic, Trait.Evil, Trait.Fire, Trait.Dragon],
                10, 20, 24, new Defenses(30, 21, 18, 19), 210,
                new Abilities(6, 1, 4, 1, 2, 3), new Skills(acrobatics: 15, arcana: 17, athletics: 22, deception: 19, diplomacy: 17, intimidation: 21, stealth: 17))
                .WithCreatureId(CreatureIds.YoungRedDragon)
                .WithCharacteristics(true, true)
                .AddQEffect(QEffect.Flying())
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Fire))
                .AddQEffect(QEffect.TraitImmunity(Trait.Fire))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Paralyzed))
                .AddQEffect(QEffect.DamageWeakness(DamageKind.Cold, 10))
                .AddQEffect(QEffect.BreathWeapon("fiery breath", Target.Cone(8), Defense.Reflex, 30, DamageKind.Fire, DiceFormula.FromText("11d6", "fiery breath"), SfxName.FireRay))
                .AddQEffect(QEffect.FrightfulPresence(18, 27))
                .AddQEffect(new QEffect() {
                    StartOfCombat = async self => {
                        var effect = QEffect.Recharging("Breath Weapon");
                        effect.Value = 1;
                        self.Owner.AddQEffect(effect);
                    },
                    YouBeginAction = async (qfBeforeAction, action) => {
                        if (action.Item?.Name == NaturalWeapons.GetName(NaturalWeaponKind.Tail)) {
                            action.ProjectileKind = ProjectileKind.None;
                        }
                    }
                })
                .AddQEffect(new QEffect("Draconic Frenzy {icon:TwoActions}", "Make two claw Strikes and one tail Strike in any order.") {
                    ProvideMainAction = (qfMainAction) => {
                        Creature self = qfMainAction.Owner;
                        Illustration draconicFrenzyIllustration = new SideBySideIllustration(NaturalWeapons.GetIllustration(NaturalWeaponKind.Claw), NaturalWeapons.GetIllustration(NaturalWeaponKind.Tail));
                        CombatAction draconicFrenzyAction = new CombatAction(self, draconicFrenzyIllustration, "Draconic Frenzy", [], "Make two claw Strikes and one tail Strike in any order.", Target.Self()
                            .WithAdditionalRestriction((innerSelf) => {
                                if (!innerSelf.Battle.AllCreatures.Any(creature => !innerSelf.FriendOf(creature) && innerSelf.HasLineOfEffectTo(creature.Occupies) < CoverKind.Blocked && innerSelf.DistanceTo(creature.Occupies) <= 2)) {
                                    return "no creature in Claw range";
                                }

                                return null;
                            }))
                            .WithActionCost(2)
                            .WithEffectOnEachTarget(async (frenzy, innerSelf, target, result) => {
                                int clawStrikesRemaining = 2;
                                int tailStrikesRemaining = 1;


                                // See Flurry of Blows to better understand logic
                                for (int i = 0; i < 3; i++) {
                                    await innerSelf.Battle.GameLoop.StateCheck();
                                    var possibilities = new List<Option>();

                                    string clawItemName = NaturalWeapons.GetName(NaturalWeaponKind.Claw);
                                    string tailItemName = NaturalWeapons.GetName(NaturalWeaponKind.Tail);
                                    Item? claw = innerSelf.UnarmedStrike.Name == clawItemName ? innerSelf.UnarmedStrike : innerSelf.QEffects.FirstOrDefault(qe => qe.AdditionalUnarmedStrike != null && qe.AdditionalUnarmedStrike.Name == clawItemName)?.AdditionalUnarmedStrike;
                                    Item? tail = innerSelf.UnarmedStrike.Name == tailItemName ? innerSelf.UnarmedStrike : innerSelf.QEffects.FirstOrDefault(qe => qe.AdditionalUnarmedStrike != null && qe.AdditionalUnarmedStrike.Name == tailItemName)?.AdditionalUnarmedStrike;
                                    if (claw != null && tail != null) {
                                        if (clawStrikesRemaining > 0) GameLoop.AddDirectUsageOnCreatureOptions(innerSelf.CreateStrike(claw).WithActionCost(0), possibilities, true);
                                        if (tailStrikesRemaining > 0) GameLoop.AddDirectUsageOnCreatureOptions(innerSelf.CreateStrike(tail).WithActionCost(0), possibilities, true);
                                    }

                                    if (possibilities.Count > 0) {
                                        Option chosenOption;
                                        string additionalTopText = string.Empty;
                                        if (i == 0) {
                                            possibilities.Add(new CancelOption(true));
                                            additionalTopText = " or right-click to cancel";
                                        }

                                        string clawTopText = (clawStrikesRemaining > 0) ? $"\nRemaing Claw Strikes: {clawStrikesRemaining}" : string.Empty;
                                        string tailTopText = (tailStrikesRemaining > 0) ? $"\nRemaing Tail Strikes: {tailStrikesRemaining}" : string.Empty;

                                        var requestResult = await innerSelf.Battle.SendRequest(new AdvancedRequest(innerSelf, "Choose a creature to Strike.", possibilities) {
                                            TopBarText = $"Choose a creature to Strike{additionalTopText}. ({i + 1}/3){clawTopText}{tailTopText}",
                                            TopBarIcon = draconicFrenzyIllustration
                                        });
                                        chosenOption = requestResult.ChosenOption;
                                        if (chosenOption is CancelOption) {
                                            frenzy.RevertRequested = true;
                                            return;
                                        }

                                        if (chosenOption.Text.Contains(clawItemName)) {
                                            clawStrikesRemaining--;
                                        } else if (chosenOption.Text.Contains(tailItemName)) {
                                            tailStrikesRemaining--;
                                        }

                                        await chosenOption.Action();
                                    }
                                }
                            });

                        ActionPossibility draconicFrenzyPossibility = new ActionPossibility(draconicFrenzyAction);
                        return draconicFrenzyPossibility;
                    }
                })
                .AddQEffect(new QEffect("Draconic Momentum", "After your Strikes critical hit, you instantly recharge your breath weapon.") {
                    AfterYouTakeAction = async (qfAfterAction, action) => {
                        if (action.CheckResult == CheckResult.CriticalSuccess && action.HasTrait(Trait.Strike)) {
                            qfAfterAction.Owner.RemoveAllQEffects(qe => qe.Id == QEffectId.Recharging && (qe.Name?.ToLower().Contains("breath weapon") ?? false));
                        }
                    }
                })
                .AddQEffect(QEffect.AttackOfOpportunity())
                .Builder
                .AddNaturalWeapon(NaturalWeaponKind.Jaws, 23, [Trait.Reach, Trait.Cold], "2d12+12", DamageKind.Piercing, wp => wp.WithAdditionalDamage("2d6", DamageKind.Fire))
                .AddNaturalWeapon(NaturalWeaponKind.Claw, 23, [Trait.Agile], "2d10+12", DamageKind.Slashing)
                .AddNaturalWeapon(NaturalWeaponKind.Tail, 21, [Trait.Ranged, Trait.DoesNotProvoke], "2d12+10", DamageKind.Slashing, wp => {
                    wp.WithMaximumRange(3);
                    wp.WithRangeIncrement(3);
                    wp.Sfx = SfxName.SwordStrike;
                })
                .AddNaturalWeapon("Wing", IllustrationName.Wing, 21, [Trait.Reach, Trait.Agile], "1d10+10", DamageKind.Slashing)
                .Done();
        }
    }
}