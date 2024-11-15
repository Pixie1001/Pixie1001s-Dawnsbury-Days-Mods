using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Description;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Dawnsbury.ThirdParty.SteamApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Intrinsics.Arm;
using System.Xml;
using Dawnsbury.Core.Mechanics.Damage;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;
using System.Text;
using static Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.BarbarianFeatsDb.AnimalInstinctFeat;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Metrics;
using Microsoft.Xna.Framework.Audio;
using static System.Reflection.Metadata.BlobBuilder;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Animations.Movement;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;
using System.IO;
using System.Text.Json.Nodes;
using System.Reflection.Metadata;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Champion;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class CommonQEffects {
        public static QEffect Drow() {
            return new QEffect("Drow Resilience", "+2 status bonus vs. mental saves; +1 status bonus vs. magic") {
                BonusToDefenses = (self, action, defence) => {
                    if (action == null) {
                        return null;
                    }

                    if (action.HasTrait(Trait.Mental) && defence != Defense.AC) {
                        return new Bonus(2, BonusType.Status, self.Name);
                    }

                    if (action.SpellId != SpellId.None && defence != Defense.AC) {
                        return new Bonus(1, BonusType.Status, self.Name);
                    }

                    return null;
                }
            };
        }

        public static QEffect Stalked(Creature source) {
            return new QEffect() {
                Source = source,
                Id = QEffectIds.Stalked,
                StateCheck = self => {
                    if (self.Owner.HasEffect(QEffectId.Dying)) {
                        List<Creature> party = self.Owner.Battle.AllCreatures.Where(c => c.OwningFaction.IsHumanControlled).ToList();
                        Creature newTarget = party.OrderBy(c => c.HP / 100 * (c.Defenses.GetBaseValue(Defense.AC) * 5)).ToList().FirstOrDefault(c => !c.HasEffect(QEffectId.Dying) && !c.HasEffect(QEffectId.Unconscious));
                        if (newTarget != null) {
                            newTarget.AddQEffect(CommonQEffects.Stalked(self.Source));
                        }
                        self.ExpiresAt = ExpirationCondition.Immediately;
                    }

                    if (!self.Source.Alive) {
                        self.ExpiresAt = ExpirationCondition.Immediately;
                    }
                },
            };
        }

        public static QEffect SpiderVenomAttack(int baseDC, string weapon) {
            return new QEffect("Spider Poison", "Set Later") {
                StateCheck = async self => {
                    if (self.Description == "Set Later") {
                        self.Name += $" (DC {baseDC + self.Owner.Level})";
                        self.Description = $"Enemies damaged by {self.Owner.Name}'s {weapon} attack are afflicted by Spider Venom: {Affliction.CreateSpiderVenom().StagesDescription}";
                    }
                },
                AfterYouDealDamage = async (attacker, action, target) => {
                    if (action.Name == weapon || action.Name == $"Strike ({weapon})") {
                        Affliction poison = Affliction.CreateSpiderVenom();
                        poison.DC = baseDC + attacker.Level;
                        
                        await Affliction.ExposeToInjury(poison, attacker, target);
                    }
                },
                AdditionalGoodness = (self, action, target) => {
                    int dc = baseDC + self.Owner.Level;

                    if (action == null || !(action.Name == weapon || action.Name == $"Strike ({weapon})")) {
                        return 0f;
                    }

                    if (target != null && !target.HasEffect(QEffectId.SpiderVenom)) {
                        return 2f;
                    }

                    return 0f;
                }
            };
        }

        //public static QEffect AmuletOfAbeyance() {
        //    QEffect effect = new QEffect("Amulet of Abeyance {icon:Reaction}", "{b}Effect{/b} You or a member of your coven within 15-feet would be damaged by an attack.");
        //    effect.AddGrantingOfTechnical(cr => cr.HasTrait(Traits.Witch), qf => {
        //        qf.YouAreDealtDamage = async (self, a, damage, d) => {
        //            if (effect.Owner.DistanceTo(d) > 3) {
        //                return null;
        //            }
                    
        //            if (effect.UseReaction()) {
        //                return new ReduceDamageModification(3 + effect.Owner.Level, "Amulet of Abeyance");
        //            }
        //            return null;
        //        };
        //    });

        //    return effect;
        //}

        public static QEffect WebAttack(int baseDC) {
            return new QEffect() {
                Tag = false,
                ProvideMainAction = self => {
                    return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.Web, "Shoot Web", new Trait[] { Trait.Unarmed, Trait.Ranged },
                        "{b}Range.{/b} 30-feet\n\nOn a hit, the target is immobilized by a web trap, sticking them to the nearest surface. They must use the Escape action to free themselves.",
                        Target.Ranged(6)) {
                        ShortDescription = "On a hit, the target is immobilized by a web trap, until they use the Escape action to free themselves."
                    }
                    .WithProjectileCone(IllustrationName.Web, 5, ProjectileKind.Cone)
                    .WithSoundEffect(SfxName.AeroBlade)
                    .WithActionCost(1)
                    .WithActiveRollSpecification(new ActiveRollSpecification(Checks.Attack(new Item(IllustrationName.Web, "Web", new Trait[] { Trait.Attack, Trait.Unarmed, Trait.Finesse, Trait.Ranged })), Checks.DefenseDC(Defense.AC)))
                    .WithGoodnessAgainstEnemy((targeting, attacker, defender) => {

                        if (defender.QEffects.FirstOrDefault(qf => qf.Name.StartsWith("Webbed (")) != null || self.UsedThisTurn) {
                            return int.MinValue;
                        }

                        if (defender.HasEffect(QEffectId.Immobilized) || defender.HasEffect(QEffectId.Grabbed) || defender.HasEffect(QEffectId.Restrained)) {
                            return 0.1f;
                        }

                        float score = 2.5f;

                        // Dramatic opening attack bonus
                        if ((bool)self.Tag == false) {
                            score += 10f;
                        }

                        // Bonus based on target isolated
                        List<Creature> adjacentAllies = defender.Battle.AllCreatures.Where(cr => cr.OwningFaction == attacker.OwningFaction && cr.IsAdjacentTo(defender)).ToList();
                        if (adjacentAllies.Count > 0) {
                            score += 2.5f; //+ (adjacentAllies[0].QEffects.FirstOrDefault(qf => qf.Name == "Sneak Attack") != null ? 3.5f : 0f);
                        }
                        return score;
                    })
                    .WithEffectOnSelf(user => {
                        self.Tag = true;
                        self.UsedThisTurn = true;
                    })
                    .WithEffectOnEachTarget(async (spell, caster, target, checkResult) => {
                        if (checkResult >= CheckResult.Success) {
                            QEffect webbed = new QEffect($"Webbed (DC {baseDC + caster.Level})", "You cannot use any action with the move trait, until you break free of the webs.") {
                                Id = QEffectId.Immobilized,
                                Source = caster,
                                PreventTakingAction = (CombatAction ca) => (!ca.HasTrait(Trait.Move)) ? null : "You're immobilized.",
                                Illustration = IllustrationName.Web,
                                Tag = 14 + caster.Level,
                                ProvideContextualAction = self => {
                                    CombatAction combatAction = new CombatAction(self.Owner, (Illustration)IllustrationName.Escape, "Escape from " + caster?.ToString() + "'s webs.", new Trait[] {
                                        Trait.Attack, Trait.AttackDoesNotTargetAC }, $"Make an unarmed attack, Acrobatics check or Athletics check against the escape DC ({baseDC + caster.Level}) of the webs.",
                                        (Target)Target.Self((Func<Creature, AI, float>)((_, ai) => ai.EscapeFrom(caster)))) {
                                        ActionId = ActionId.Escape
                                    };

                                    ActiveRollSpecification activeRollSpecification = (new ActiveRollSpecification[] {
                                        new ActiveRollSpecification(Checks.Attack(Item.Fist()), Checks.FlatDC(baseDC + caster.Level)),
                                        new ActiveRollSpecification(Checks.SkillCheck(Skill.Athletics), Checks.FlatDC(baseDC + caster.Level)),
                                        new ActiveRollSpecification(Checks.SkillCheck(Skill.Acrobatics), Checks.FlatDC(baseDC + caster.Level))
                                    }).MaxBy(roll => roll.DetermineBonus(combatAction, self.Owner, null).TotalNumber);

                                    return (ActionPossibility)combatAction
                                    .WithActiveRollSpecification(activeRollSpecification)
                                    .WithSoundEffect(combatAction.Owner.HasTrait(Trait.Female) ? SfxName.TripFemale : combatAction.Owner.HasTrait(Trait.Male) ? SfxName.TripMale : SfxName.BeastRoar)
                                    .WithEffectOnEachTarget((Delegates.EffectOnEachTarget)(async (spell, a, d, cr) => {
                                        switch (cr) {
                                            case CheckResult.CriticalFailure:
                                                a.AddQEffect(new QEffect("Cannot escape", "You can't Escape until your next turn.", ExpirationCondition.ExpiresAtStartOfYourTurn, a) {
                                                    PreventTakingAction = (Func<CombatAction, string>)(ca => !ca.Name.StartsWith("Escape") ? (string)null : "You already tried to escape and rolled a critical failure.")
                                                });
                                                break;
                                            case CheckResult.Success:
                                                self.ExpiresAt = ExpirationCondition.Immediately;
                                                break;
                                            case CheckResult.CriticalSuccess:
                                                self.ExpiresAt = ExpirationCondition.Immediately;
                                                int num = await self.Owner.StrideAsync("You escape and you may Stride 5 feet.", maximumFiveFeet: true, allowPass: true) ? 1 : 0;
                                                break;
                                        }
                                    }));
                                }
                            };
                            target.AddQEffect(webbed);
                        }
                    })
                    ;
                }
            };
        }

        public static QEffect CruelTaskmistress(string damage) {
            QEffect effect = new QEffect("Cruel Taskmistress {icon:Reaction}", $"After a non-mindless ally within reach fails a strike attack, you may strike them for {damage} mental damage in order to allow them to reroll it with a +1 bonus, taking the best of their two rolls.") {
                Innate = true,
            };

            effect.AddGrantingOfTechnical(cr => cr.OwningFaction != effect.Owner.OwningFaction, qf => {
                qf.YouAreTargetedByARoll = async (self, action, breakdown) => {
                    if (breakdown.CheckResult > CheckResult.Failure) {
                        return false;
                    }
                    
                    if (effect.Owner.Alive == false) {
                        return false;
                    }

                    Item? whip = effect.Owner.HeldItems.FirstOrDefault(item => item.ItemName == CustomItems.ScourgeOfFangs);

                    if (whip == null) {
                        return false;
                    }

                    if (action == null || !action.HasTrait(Trait.Strike) || action.Owner == effect.Owner || action.Owner.OwningFaction != effect.Owner.OwningFaction || action.Owner.DistanceTo(effect.Owner) > 2 || effect.Owner.HasLineOfEffectTo(action.Owner.Occupies) == CoverKind.Blocked || action.Owner.Traits.Contains(Trait.Mindless)) {
                        return false;
                    }

                    if (action.Owner.HP + action.Owner.TemporaryHP <= DiceFormula.FromText(damage).ExpectedValue * 2) {
                        return false;
                    }

                    if (effect.UseReaction()) {
                        effect.Owner.Occupies.Overhead("*cruel taskmistress*", Color.Green,
                                effect.Owner.Name + " uses {b}Cruel Taskmistress{/b} to punish " + action.Owner.Name + " for their failure.",
                                effect.Owner.Name + " uses {b}Cruel Taskmistress{/b}",
                                effect.Owner.Name + " uses {b}Cruel Taskmistress{/b} on " + action.Owner.Name + " as a reaction {icon:Reaction}.\n\nThis inflcits " + damage + " {b}mental{/b} damage on the target, but allows them to reroll their attack with a +1 bonus and take the best result.");
                        //CombatAction attack = effect.Owner.CreateStrike(whip).AllExecute();
                        //await CommonSpellEffects.DealDirectDamage(effect.Owner.CreateStrike(whip), DiceFormula.FromText("1d4"), action.Owner, CheckResult.Success, DamageKind.Mental);

                        Item fakeWhip = new Item(CustomItems.ScourgeOfFangs, IllustrationName.Whip, "Scourge of Fangs", 3, 0, new Trait[] { Trait.Flail }).WithWeaponProperties(new WeaponProperties(damage, DamageKind.Mental) {
                            AdditionalFlatBonus = -effect.Owner.Abilities.Strength
                        });

                        StrikeModifiers strikeMod = new StrikeModifiers() {
                            CalculatedTrueDamageFormula = DiceFormula.FromText("1d6", "Cruel Taskmistress")
                        };

                        CombatAction attack = effect.Owner.CreateStrike(fakeWhip, -1, strikeMod);
                        attack.WithActiveRollSpecification(null);
                        attack.Target = new CreatureTarget(RangeKind.Melee, new CreatureTargetingRequirement[] { }, (_1, _2, _3) => int.MinValue);
                        attack.ChosenTargets = ChosenTargets.CreateSingleTarget(action.Owner);
                        await attack.AllExecute();

                        int newValue = R.NextD20();
                        if (newValue > breakdown.D20Roll) {
                            action.Owner.Occupies.Overhead("", Color.Black,
                                $"{action.Owner.Name} rerolls their attack and takes the new result: {breakdown.D20Roll} > {newValue}",
                                $"{action.Owner.Name} rerolls their attack: {breakdown.D20Roll} > {newValue}",
                                $"{action.Owner.Name} rerolls their attack: {breakdown.D20Roll} > {newValue}.\n\nThey have taken the new higher value.");
                            breakdown.GetType().GetField("<D20Roll>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).SetValue(breakdown, newValue);
                            breakdown.GetType().GetField("<FirstD20Roll>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).SetValue(breakdown, newValue);
                        } else {
                            action.Owner.Occupies.Overhead("", Color.Black,
                                $"{action.Owner.Name} rerolls their attack and takes the origional result: {breakdown.D20Roll} > {newValue}",
                                $"{action.Owner.Name} rerolls their attack: {breakdown.D20Roll} > {newValue}",
                                $"{action.Owner.Name} rerolls their attack: {breakdown.D20Roll} > {newValue}.\n\nThey have taken the origional higher value.");
                        }
                        Sfxs.Play(SfxName.SnakeHiss);
                        action.Owner.AddQEffect(new QEffect() {
                            BonusToAttackRolls = (self, action, target) => new Bonus(1, BonusType.Untyped, "Cruel Taskmistress"),
                            ExpiresAt = ExpirationCondition.Ephemeral,
                        });
                        return true;
                    }

                    return false;
                };
            });

            return effect;
        }

        public static QEffect RetributiveStrike(int reduction, Func<Creature, bool> filter, string targetDesc, bool step) {

            QEffect effect = new QEffect("Retributive Strike {icon:Reaction}", "{b}Trigger{/b} An enemy damages " + targetDesc + " and both are within 15 feet of you. " +
                "{b}Effect{/b} The ally gains resistance " + reduction + " to all damage against the triggering attack. If the foe is within reach, make a melee Strike against it." +
                (step ? " If the target is out of range, you can step to put the foe within your reach." : "")) {
                Innate = true,
            };

            effect.AddGrantingOfTechnical(filter, qf => {
                qf.YouAreDealtDamage = async (qfAlly, attacker, damageStuff, defender) => {
                    if (attacker == null || attacker.Occupies == null || !attacker.EnemyOf(effect.Owner) || attacker.DistanceTo(effect.Owner) > 3)
                        return (DamageModification)null;
                    if (!await effect.Owner.Battle.AskToUseReaction(effect.Owner, attacker?.ToString() + " is about to deal " + damageStuff.Amount.ToString() + " damage to " + defender?.ToString() + ". Use your champion's reaction to prevent " + reduction.ToString() + " of that damage?"))
                        return (DamageModification)null;

                    List<Tile> validStepTiles = effect.Owner.Battle.Map.AllTiles.Where(t => t.IsAdjacentTo(effect.Owner.Occupies) && t.DistanceTo(attacker.Occupies) < 3 && attacker.HasLineOfEffectTo(attacker.Occupies) < CoverKind.Blocked).ToList();

                    effect.Owner.Occupies.Overhead("retributive strike!", Color.Orange, effect.Owner?.ToString() + " uses retributive strike!");
                    effect.Owner.AddQEffect(new QEffect(ExpirationCondition.Never) {
                        StateCheckWithVisibleChanges = async qfStrikeBack => {
                            qfStrikeBack.StateCheckWithVisibleChanges = null;
                            qfStrikeBack.ExpiresAt = ExpirationCondition.Immediately;
                            Item championMainWeapon = effect.Owner.PrimaryWeapon;
                            if (championMainWeapon == null || !championMainWeapon.HasTrait(Trait.Melee))
                                return;
                            CombatAction meleeStrike = effect.Owner.CreateStrike(championMainWeapon).WithActionCost(0);
                            CreatureTarget meleeStrikeTarget = (CreatureTarget)meleeStrike.Target;
                            if ((bool)meleeStrike.CanBeginToUse(effect.Owner) && (bool)meleeStrikeTarget.IsLegalTarget(effect.Owner, attacker)) {
                                meleeStrike.ChosenTargets = ChosenTargets.CreateSingleTarget(attacker);
                                int num4 = await meleeStrike.AllExecute() ? 1 : 0;
                            } else if (step && effect.Owner.DistanceTo(attacker) > 2 && validStepTiles.Count > 0) {
                                Tile bestTile = validStepTiles.OrderBy(t => t.HasLineOfEffectTo(attacker.Occupies)).ToArray()[0];
                                await effect.Owner.SingleTileMove(bestTile, null);
                                if (meleeStrikeTarget.IsLegalTarget(effect.Owner, attacker)) {
                                    meleeStrike.ChosenTargets = new ChosenTargets() {
                                        ChosenCreature = attacker,
                                        ChosenCreatures = {
                                            attacker
                                        }
                                    };
                                    int num5 = await meleeStrike.AllExecute() ? 1 : 0;
                                }
                            }
                            meleeStrike = (CombatAction)null;
                            meleeStrikeTarget = (CreatureTarget)null;
                        }
                    });
                    return (DamageModification)new ReduceDamageModification(reduction, "Retributive Strike");
                };
            });
            return effect;
        }

        public static QEffect DrowBloodBond() {
            return new QEffect("Blood Sworn Guardian", "This creature is sworn to the drow priestesses through demonic blood magic, protecting it from negative energy and allowing members of the drow clergy to siphon their life force at will.") {
                Id = QEffectIds.BloodBond,
            };
        }

        public static QEffect DrowClergy() {
            return new QEffect() {
                Id = QEffectIds.DrowClergy,
                ProvideContextualAction = self => {
                    if (self.Owner.HP > self.Owner.TrueMaximumHP * 0.33) {
                        return null;
                    }

                    return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.VampiricExsanguination, "Activate Blood Bond", new Trait[] { Trait.Magical, Trait.Divine, Trait.Healing, Trait.Necromancy },
                        "You extract the lifeforce from a bonded serf, dealing 2d6 bleed damage, and healing yourself for twice as much HP.", Target.RangedFriend(3).WithAdditionalConditionOnTargetCreature(new FriendCreatureTargetingRequirement()).WithAdditionalConditionOnTargetCreature((a, d) => !d.HasEffect(QEffectIds.BloodBond) ? Usability.NotUsableOnThisCreature("No blood bond") : Usability.Usable))
                    .WithActionCost(1)
                    .WithSoundEffect(SfxName.ElementalBlastWater)
                    .WithProjectileCone(IllustrationName.VampiricExsanguination, 7, ProjectileKind.Ray)
                    .WithEffectOnEachTarget(async (spell, caster, target, checkResult) => {
                        int prevHP = target.HP;
                        await CommonSpellEffects.DealDirectDamage(spell, DiceFormula.FromText("2d6", "Activate Blood Bond"), target, CheckResult.Success, DamageKind.Bleed);
                        int healAmount = (prevHP - target.HP) * 2;
                        await caster.HealAsync(DiceFormula.FromText(healAmount.ToString(), "Activate Blood Bond"), spell);
                    })
                    .WithGoodness((targeting, a, d) => {
                        return 15f + (d.HP / d.MaxHP * 4);
                    })
                    ;
                }
            };
        }

        public static QEffect MiniBoss() {
            QEffect effect = new QEffect("Powerful Adversary", "The first time this creature acts each round, it only drops down two places in intitive order. In addition it has significantly increased HP for its level.") {
                StartOfCombat = async self => {
                    self.Owner.MaxHP = self.Owner.MaxHP * 2;
                    self.Owner.AddQEffect(new QEffect("Extra Turn", "This creature will only move down two spaces in inititve order after this turn.") {
                        Illustration = IllustrationName.Haste,
                        Id = QEffectIds.ExtraTurn
                    });
                },
            };

            effect.AddGrantingOfTechnical(cr => effect.Owner.Battle.InitiativeOrder.Contains(cr), qf => {
                qf.StartOfYourTurn = async (tmp, owner) => {
                    List<Creature> initOrder = effect.Owner.Battle.InitiativeOrder;

                    if (initOrder.IndexOf(effect.Owner) != initOrder.IndexOf(effect.Owner.Battle.ActiveCreature) - 1) {
                        if (!(effect.Owner == initOrder.Last() && effect.Owner.Battle.ActiveCreature == initOrder.First())) {
                            return;
                        }
                    }

                    if (!effect.Owner.HasEffect(QEffectIds.ExtraTurn)) {
                        effect.Owner.AddQEffect(new QEffect("Extra Turn", "This creature will only move down two spaces in initiative order after this turn.") {
                            Illustration = IllustrationName.Haste,
                            Id = QEffectIds.ExtraTurn
                        });
                        return;
                    }

                    int pos = initOrder.IndexOf(effect.Owner);
                    initOrder.Remove(effect.Owner);

                    int newPos = 0;
                    if (pos == initOrder.Count - 1) {
                        newPos = 1;
                    } else if (pos == initOrder.Count - 2) {
                        newPos = 0;
                    } else {
                        newPos = pos + 2;
                    }

                    List<Creature> newOrder = new List<Creature>();

                    bool added = false;
                    for (int i = 0; i < initOrder.Count; i++) {
                        if (i == newPos && added == false) {
                            newOrder.Add(effect.Owner);
                            added = true;
                            i--;
                        } else {
                            newOrder.Add(initOrder[i]);
                        }
                    }

                    effect.Owner.Battle.InitiativeOrder = newOrder;
                    effect.Owner.RemoveAllQEffects(qf => qf.Id == QEffectIds.ExtraTurn);
                };
            });

            return effect;
            //StateCheckWithVisibleChanges = async (self) => {
            //    if (self.Owner.Battle.ActiveCreature == null || self.UsedThisTurn) {
            //        return;
            //    }

            //    List<Creature> initOrder = self.Owner.Battle.InitiativeOrder;

            //    if (initOrder.IndexOf(self.Owner) != initOrder.IndexOf(self.Owner.Battle.ActiveCreature) - 1) {
            //        if (!(self.Owner == initOrder.Last() && self.Owner.Battle.ActiveCreature == initOrder.First())) {
            //            self.UsedThisTurn = true;
            //            return;
            //        }
            //    }

            //    if (!self.Owner.HasEffect(QEffectIds.ExtraTurn)) {
            //        self.Owner.AddQEffect(new QEffect("Extra Turn", "This creature will only move down two spaces in initiative order after this turn.") {
            //            Illustration = IllustrationName.Haste,
            //            Id = QEffectIds.ExtraTurn
            //        });
            //        self.UsedThisTurn = true;
            //        return;
            //    }

            //    int pos = initOrder.IndexOf(self.Owner);
            //    initOrder.Remove(self.Owner);

            //    // Uses cases
            //    // - 2 creatures
            //    // - 3 creatures

            //    int newPos = 0;
            //    if (pos == initOrder.Count - 1) {
            //        newPos = 1;
            //    } else if (pos == initOrder.Count - 2) {
            //        newPos = 0;
            //    } else {
            //        newPos = pos + 2;
            //    }

            //    List<Creature> newOrder = new List<Creature>();

            //    bool added = false;
            //    for (int i = 0; i < initOrder.Count; i++) {
            //        if (i == newPos && added == false) {
            //            newOrder.Add(self.Owner);
            //            added = true;
            //            i--;
            //        } else {
            //            newOrder.Add(initOrder[i]);
            //        }
            //    }

            //    self.Owner.Battle.InitiativeOrder = newOrder;
            //    self.Owner.RemoveAllQEffects(qf => qf.Id == QEffectIds.ExtraTurn);
            //    self.UsedThisTurn = true;
            //}
        }
    }
}
