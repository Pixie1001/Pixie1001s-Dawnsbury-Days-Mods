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
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Display.Text;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Hydra {
        public static Creature Create() { 

            var creature = new Creature(Illustrations.Hydra,
                "Hydra",
                [Trait.Beast, Trait.NonSummonable, ModTraits.MeleeMutator],
                level: 6, perception: 17, speed: 6,
                new Defenses(23, 15, 12, 10),
                hp: 15,
                new Abilities(7, 4, 5, -3, 2, -1),
                new Skills(athletics: 17, stealth: 12))
            .WithCreatureId(CreatureIds.Hydra)
            .WithCharacteristics(false, true)
            .AddQEffect(QEffect.AllAroundVision())
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Slashing, 5))
            .AddQEffect(HydraHeads(5))
            .AddQEffect(new QEffect() {
                YouAcquireQEffect = (self, newQf) => {
                    int adj = 0;
                    if (newQf.Id == QEffectId.Inferior)
                        adj = -2;
                    if (newQf.Id == QEffectId.Weak)
                        adj = -1;
                    if (newQf.Id == QEffectId.Elite)
                        adj = 1;
                    if (newQf.Id == QEffectId.Supreme)
                        adj = 2;

                    if (adj != 0) {
                        self.Owner.FindQEffect(QEffectIds.HydraHeads)!.Value += adj;
                        self.Owner.MaxHP = 15;
                    }

                    return newQf;
                }
            })
            .AddQEffect(new QEffect($"Head Regrowth", $"The Hydra fully heals at the end of any given creature's turn. " +
            $"The Hydra's HP represents one of its many heads. When it would be reduced to 0 HP, it instead heals to full health, and one of its heads becomes a stump.\n" +
            $"A hydra can regrow a severed head using Hydra Regeneration. A creature can prevent this regrowth by dealing acid or fire damage to the stump, cauterizing it." +
            $" Single-target acid or fire effects can be redirected to cauterise a stump instead of dealing damage, but effects that deal splash damage or affect areas covering the hydra’s whole space automatically cauterize all stumps. If the attack that severs a head deals any acid or fire damage, the stump is cauterized instantly. If all of its heads are cauterized, the hydra dies.") {
                Tag = false,
                StateCheck = self => {
                    if (self.Owner.GetQEffectValue(QEffectIds.HydraStumps) + self.Owner.GetQEffectValue(QEffectIds.HydraHeads) <= 0) {
                        self.Owner.RemoveAllQEffects(qf => qf.Id == QEffectId.RegenerationPreventsDeath);
                        self.Owner.Die();
                    }
                },
                YouAreDealtDamageEvent = async (self, dmg) => {
                    if (self.Owner.GetQEffectValue(QEffectIds.HydraStumps) <= 0 || dmg.CombatAction == null || !dmg.KindedDamages.Any(kd => kd.DamageKind == DamageKind.Acid || kd.DamageKind == DamageKind.Fire)) return;

                    // Ask if player wants to instead cauterise a stump
                    if (dmg.CombatAction.Target.IsAreaTarget
                    || (dmg.CombatAction.Item?.WeaponProperties?.AdditionalSplashDamageFormula != null
                    && (dmg.CombatAction.Item?.WeaponProperties?.AdditionalSplashDamageKind == DamageKind.Fire || dmg.CombatAction.Item?.WeaponProperties?.AdditionalSplashDamageKind == DamageKind.Acid))) {
                        self.Owner.RemoveAllQEffects(qf => qf.Id == QEffectIds.HydraStumps);
                    } else if (dmg.CombatAction.Owner.Occupies != null) {
                        if (self.Owner.GetQEffectValue(QEffectIds.HydraHeads) == 0)
                            goto cauterise;

                        // Ask player if they want to target a stump instead
                        if (await dmg.CombatAction.Owner.AskForConfirmation(self.Owner.Illustration, "Your single target attack deals acid or fire damage. Would you like to forgo damaging the Hydra's active head in favour of cauterising one of its stumps?", "Yes", "No")) {
                            goto cauterise;
                        }

                        cauterise:
                            dmg.KindedDamages.ForEach(kd => kd.ResolvedDamage = 0);
                            if (self.Owner.GetQEffectValue(QEffectIds.HydraStumps) > 1)
                                self.Owner.FindQEffect(QEffectIds.HydraStumps)!.Value--;
                            else
                                self.Owner.RemoveAllQEffects(qf => qf.Id == QEffectIds.HydraStumps);
                            self.Owner.Overhead("cauterised!", Color.DarkRed, $"One of the hydra's neck stumps has been cauterised!");
                            Sfxs.Play(SfxName.AcidSplash);
                            self.StateCheck!(self);
                    }
                },
                YouAreDealtLethalDamage = async (self, attacker, dmg, defender) => {
                    if (self.Owner.GetQEffectValue(QEffectIds.HydraHeads) >= 1) {
                        self.Owner.FindQEffect(QEffectIds.HydraHeads)!.Value -= 1;
                    }

                    var hasHeadsRemaining = self.Owner.GetQEffectValue(QEffectIds.HydraStumps) + self.Owner.GetQEffectValue(QEffectIds.HydraHeads) > 0;
                    self.Tag = hasHeadsRemaining;

                    if (hasHeadsRemaining)
                        return new SetToTargetNumberModification(defender.HP - 1, "Head Regrowth");
                    else
                        return null;
                },
                AfterYouTakeIncomingDamageEventEvenZero = async (self, dmg) => {
                    if ((self.Tag as bool?) == true) {

                        // Handle head management
                        if (dmg.KindedDamages.Any(kd => kd.DamageKind == DamageKind.Acid || kd.DamageKind == DamageKind.Fire)) {
                            self.Owner.Overhead("cauterised!", Color.DarkRed, $"The hydra's head is cauterised as its destroyed, preventing a new stump from forming.");
                            Sfxs.Play(SfxName.AcidSplash);
                            self.StateCheck!(self);
                            return;
                        }
                        self.Owner.Overhead("head destroyed!", Color.LightSkyBlue, $"One of the hydra's heads has been reduced to a stump.");
                        Sfxs.Play(SfxName.BeastDeath);
                        if (self.Owner.GetQEffectValue(QEffectIds.HydraStumps) == 0)
                            self.Owner.AddQEffect(HydraStumps());
                        else
                            self.Owner.FindQEffect(QEffectIds.HydraStumps)!.Value += 1;

                        if (self.Owner.GetQEffectValue(QEffectIds.HydraHeads) == 0 && self.Owner.FindQEffect(QEffectIds.HydraHeads) != null) {
                            self.Owner.RemoveAllQEffects(qf => qf.Id == QEffectIds.HydraHeads);
                            self.Owner.AddQEffect(QEffect.Unconscious());
                        } else {
                            self.Owner.Heal("15", null);
                        }
                    }
                    self.Tag = false;
                }
            })
            .AddQEffect(new QEffect("Hydra Regeneration", "The Hydra attempts to regrow its uncauterised head stumps at the start of its turn, by making a DC 25 Fortitude save. On a success, one of its stumps regrows into two new heads. On a critical success, it instead regrows two stumps in this way." +
            "\nWhile the hydra has no heads, it is paralyzed, but still continues to attempt to regenerate itself, until all of its stumps are cauterised.") {
                Id = QEffectId.RegenerationPreventsDeath,
                StartOfYourPrimaryTurn = async (self, you) => {
                    // TODO: Add overheads and sfx here to make it clear heads regrew

                    var stumpQf = you.FindQEffect(QEffectIds.HydraStumps);
                    if (stumpQf == null || stumpQf.Value == 0) return;

                    var result = CommonSpellEffects.RollSavingThrow(you, CombatAction.CreateSimple(you, "Hydra Regeneration"), Defense.Fortitude, 25);

                    if (result >= CheckResult.Success) {
                        bool crit = result == CheckResult.CriticalSuccess && stumpQf.Value >= 2;

                        Sfxs.Play(SfxName.NaturalHealing);
                        self.Owner.Overhead("heads regenerate", Color.LightGreen, $"{(crit ? 2 : 1)} of the hydra's severed head stumps regrow into {(crit ? 4 : 2)} new heads.");

                        if (stumpQf.Value <= (crit ? 2 : 1))
                            self.Owner.RemoveAllQEffects(qf => qf.Id == QEffectIds.HydraStumps || qf.Id == QEffectIds.HydraStumps);
                        else
                            stumpQf.Value -= (crit ? 2 : 1);

                        var headsQf = you.FindQEffect(QEffectIds.HydraHeads);
                        if (headsQf == null) {
                            headsQf = HydraHeads(crit ? 4 : 2);
                            you.AddQEffect(headsQf);
                            self.Owner.Heal("15", null);
                        } else {
                            headsQf.Value += crit ? 4 : 2;
                        }
                    }
                },
                EndOfAnyTurn = self => {
                    if (self.Owner.GetQEffectValue(QEffectIds.HydraHeads) > 0)
                        self.Owner.Heal("15", null);
                }
            })
            .AddQEffect(new QEffect("Multiple Opportunities", "A hydra gains an extra reaction per round for each of its heads beyond the first. Whenever one of the hydra’s heads is severed, the hydra loses 1 of its extra reactions per round.") {
                Tag = 0,
                StartOfYourPrimaryTurn = async (self, you) => {
                    self.Tag = you.GetQEffectValue(QEffectIds.HydraHeads);
                },
                StartOfCombat = async (self) => {
                    await self.StartOfYourPrimaryTurn!(self, self.Owner);
                }
            })
            .AddQEffect(new("Attack of Opportunity{icon:Reaction}", "When a creature leaves a square within your reach, makes a ranged attack or uses a move or manipulate action, you can Strike it for free. On a critical hit, you also disrupt the manipulate action.") {
                Id = QEffectId.AttackOfOpportunity,
                WhenProvoked = async delegate (QEffect attackOfOpportunityQEffect, CombatAction provokingAction) {
                    var user = attackOfOpportunityQEffect.Owner;
                    var target = provokingAction.Owner;
                    if ((await OfferAndMakeReactiveStrike(user, target, "{b}" + target.Name + "{/b} uses {b}" + provokingAction.Name + "{/b} which provokes.\nUse your reaction to make an attack of opportunity?", "*attack of opportunity*", 1, provokingAction)).GetValueOrDefault() == CheckResult.CriticalSuccess && provokingAction.HasTrait(Trait.Manipulate)) {
                        provokingAction.Disrupted = true;
                    }
                }
            })
            .Builder
            // TODO: Add special actions
            .AddMainAction(creature => {
                if (creature.UnarmedStrike == null) return null;

                int map = creature.Actions.AttackedThisManyTimesThisTurn;
                return new CombatAction(creature, new SideBySideIllustration(IllustrationName.Jaws, new SideBySideIllustration(IllustrationName.Jaws, IllustrationName.Jaws)),
                    "Storm of Jaws", [],
                    "The hydra makes a number of Strikes up to its number of heads, each against a different target. These attacks count toward the hydra’s multiple attack penalty, " +
                    "but the multiple attack penalty doesn’t increase until after the hydra makes all its attacks.",
                    Target.MultipleCreatureTargets(Target.Reach(creature.UnarmedStrike), Target.Reach(creature.UnarmedStrike), Target.Reach(creature.UnarmedStrike))
                    .WithMustBeDistinct().WithMinimumTargets(1)
                    .WithOverriddenOverallGoodness((t, hm) => {
                        var targets = hm.Battle.AllCreatures.Where(cr => cr.Alive && cr.EnemyOf(hm) && hm.HasLineOfEffectTo(cr.Occupies) <= CoverKind.Greater && hm.DistanceTo(cr) <= 2).Count();

                        if (targets < 2) return int.MinValue;
                        else return Math.Min(targets, hm.GetQEffectValue(QEffectIds.HydraHeads)) * creature.CreateStrike(creature.UnarmedStrike)?.TrueDamageFormula?.ExpectedValue ?? 14;
                    })
                )
                .WithActionCost(2)
                .WithEffectOnEachTarget(async (action, attacker, defender, result) => {
                    await attacker.MakeStrike(defender, attacker.UnarmedStrike, map);
                });
            })
            .AddMainAction(creature => {
                if (creature.UnarmedStrike == null) return null;

                return new CombatAction(creature, new SideBySideIllustration(IllustrationName.Jaws, new SideBySideIllustration(IllustrationName.Jaws, IllustrationName.Jaws)),
                    "Focused Assault", [],
                    "he hydra attacks a single target with its heads, overwhelming its foe with multiple attacks and leaving almost nowhere to dodge. The hydra Strikes with its fangs. " +
                    "On a successful attack, the hydra deals damage from its fangs Strike to the target, plus an additional 1d6 damage for every head it has beyond the first. Even on a failed attack, " +
                    "the hydra deals the damage from one fangs Strike to the target creature, though it still misses completely on a critical failure. This counts toward the hydra’s multiple " +
                    "attack penalty as a number of attacks equal to the number of heads the hydra has.",
                    Target.Reach(creature.UnarmedStrike))
                .WithActionCost(2)
                .WithGoodnessAgainstEnemy((_, a, d) => a.GetQEffectValue(QEffectIds.HydraHeads) * 3.5f + (creature.CreateStrike(creature.UnarmedStrike)?.TrueDamageFormula?.ExpectedValue ?? 14))
                .WithEffectOnEachTarget(async (action, attacker, defender, result) => {
                    var strike = attacker.CreateStrike(attacker.UnarmedStrike).WithActionCost(0);
                    strike.WithEffectOnEachTarget(async (action, a, d, result) => {
                        if (result == CheckResult.Failure) {
                            await CommonSpellEffects.DealDirectDamage(action, a.CreateStrike(a.UnarmedStrike).TrueDamageFormula ?? DiceFormula.FromText("2d6+7", "Focused Assault"), d, result, DamageKind.Piercing);
                        } else if (result >= CheckResult.Success) {
                            await CommonSpellEffects.DealDirectDamage(action, DiceFormula.FromText(a.GetQEffectValue(QEffectIds.HydraHeads) + "d6", "Focused Assault"), d, result, DamageKind.Piercing);
                        }
                        a.Actions.AttackedThisManyTimesThisTurn += a.GetQEffectValue(QEffectIds.HydraHeads) - 1;
                    });
                    await attacker.MakeStrike(strike, defender);
                });
            })
            .AddNaturalWeapon("fangs", IllustrationName.Jaws, 16, [Trait.Reach], "2d6+7", DamageKind.Piercing)
            .Done();

            return creature;
        }

        public static async Task<CheckResult?> OfferAndMakeReactiveStrike(Creature attacker, Creature target, string question, string overhead, int numberOfStrikes, CombatAction provokingAction) {

            int numReactions = (int?)attacker.QEffects.FirstOrDefault((effect) => effect.Name == "Multiple Opportunities")?.Tag ?? 0;

            if (attacker.QEffects.FirstOrDefault(qf => qf.Key == "Action Tracker")?.Tag == provokingAction) {
                return null;
            }

            if (numReactions > 0) {
                var item = attacker.UnarmedStrike;

                CombatAction strike = attacker.CreateStrike(item, 0).WithActionCost(0);
                strike.Traits.Add(Trait.AttackOfOpportunity);
                CreatureTarget creatureTarget = (CreatureTarget)strike.Target;
                CheckResult? bestCheckResult = null;
                if ((bool)strike.CanBeginToUse(attacker) && creatureTarget.IsLegalTarget(attacker, target).CanBeUsed && (numReactions > 1 || await attacker.Battle.AskToUseReaction(attacker, question))) {
                    if (attacker.QEffects.FirstOrDefault(qf => qf.Name == "Multiple Opportunities") != null)
                        attacker.QEffects.FirstOrDefault(qf => qf.Name == "Multiple Opportunities")!.Tag = numReactions - 1;
                    int map = attacker.Actions.AttackedThisManyTimesThisTurn;
                    attacker.Overhead(overhead, Color.White);

                    attacker.RemoveAllQEffects(qf => qf.Key == "Action Tracker");

                    attacker.AddQEffect(new QEffect() {
                        Tag = provokingAction,
                        Key = "Action Tracker"
                    });

                    for (int i = 0; i < numberOfStrikes; i++) {
                        CheckResult checkResult = await attacker.MakeStrike(strike, target);
                        if (!bestCheckResult.HasValue) {
                            bestCheckResult = checkResult;
                        } else if (checkResult > bestCheckResult) {
                            bestCheckResult = checkResult;
                        }
                    }

                    attacker.Actions.AttackedThisManyTimesThisTurn = map;
                }

                return bestCheckResult;
            }

            return null;
        }

        private static QEffect HydraStumps() {
            return new QEffect("Stumps", "...", ExpirationCondition.Never, null, IllustrationName.BloodVendetta) {
                Value = 1,
                Id = QEffectIds.HydraStumps,
                Key = "RL_Hydra Stumps"
            };
        }

        private static QEffect HydraHeads(int value) {
            return new QEffect("Heads", "...", ExpirationCondition.Never, null, IllustrationName.Jaws) {
                Value = value,
                Id = QEffectIds.HydraHeads,
                Key = "RL_Hydra Heads"
            };
        }
    }
}
