﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
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
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using System.IO;
using System.Text.Json.Nodes;
using System.Reflection.Metadata;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Champion;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using static Dawnsbury.Delegates;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class CommonQEffects {
        public static QEffect Drow() {
            return new QEffect("Drow Resilience", "+2 status bonus vs. mental saves; +1 status bonus vs. magic") {
                BonusToDefenses = (self, action, defence) => {
                    if (action == null) {
                        return null;
                    }

                    if (action.HasTrait(Trait.Mental) && !(action.SpellId != SpellId.None && action.Owner.HeldItems.Any(item => item.ItemName == CustomItems.StaffOfSpellPenetration)) && defence != Defense.AC) {
                        return new Bonus(2, BonusType.Status, self.Name!);
                    }

                    if (action.SpellId != SpellId.None && !action.Owner.HasEffect(QEffectId.SpellPenetration) && defence != Defense.AC) {
                        return new Bonus(1, BonusType.Status, self.Name!);
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
                        List<Creature> party = self.Owner.Battle.AllCreatures.Where(c => c.OwningFaction.IsPlayer).ToList();
                        Creature newTarget = party.OrderBy(c => c.HP / 100 * c.Defenses.GetBaseValue(Defense.AC) * 5).ToList().FirstOrDefault(c => c.Alive);
                        if (newTarget != null) {
                            newTarget.AddQEffect(Stalked(self.Source!));
                        }
                        self.ExpiresAt = ExpirationCondition.Immediately;
                    }

                    if (!self.Source!.Alive) {
                        self.ExpiresAt = ExpirationCondition.Immediately;
                    }
                },
            };
        }

        public static QEffect Hazard() {
            QEffect effect = new QEffect("Hazard", "This is a potentially hazardous terrain feature that may be attacked, but does not need to be destroyed in order to complete the encounter.") {
                Id = QEffectIds.Hazard,
                StateCheck = self => {
                    if (!self.Owner.Battle.AllCreatures.Any(cr => cr.OwningFaction.IsEnemy && !cr.QEffects.Any(qf => qf.Id == QEffectIds.Hazard))) {
                        self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                    }
                }
            };
            effect.AddGrantingOfTechnical(cr => cr.OwningFaction.IsEnemy, qfDeathCheck => {
                qfDeathCheck.WhenCreatureDiesAtStateCheckAsync = async self => {
                    // If there are only living hazard left, loop through all hazard and destroy them
                    if (!self.Owner.Battle.AllCreatures.Any(cr => cr.OwningFaction.IsEnemy && cr.Alive && !cr.QEffects.Any(qf => qf.Id == QEffectIds.Hazard))) {
                        Creature[] hazards = new Creature[self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsEnemy).Count()];
                        self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsEnemy).ToList().CopyTo(hazards);
                        foreach (Creature hazard in hazards) {
                            self.Owner.Battle.RemoveCreatureFromGame(hazard);
                        }
                    }
                };
            });
            return effect;
        }

        //public static QEffect BlocksLoS() {
        //    return new QEffect() {
        //        StateCheck = self => {
        //            if (self.Owner.Alive && self.Owner.Occupies.Kind != TileKind.BlocksMovementAndLineOfEffect) {
        //                self.Tag = self.Owner.Occupies.Kind;
        //                self.Owner.Occupies.Kind = TileKind.Rock;
        //                self.Owner.Occupies.AlwaysBlocksLineOfEffect = true;
        //            } else if (!self.Owner.Alive && self.Owner.Occupies.Kind == TileKind.BlocksMovementAndLineOfEffect) {
        //                self.Owner.Occupies.Kind = (TileKind)self.Tag;
        //            }
        //        }
        //    };
        //}

        public static QEffect SpiderVenomAttack(int baseDC, string weapon) {
            return new QEffect("Spider Poison", "Set Later") {
                StateCheck = async self => {
                    if (self.Description == "Set Later") {
                        self.Name += $" (DC {baseDC + self.Owner.Level})";
                        self.Description = $"Enemies damaged by {self.Owner.Name}'s {weapon} attack are afflicted by Spider Venom: " + "{i}" + $"{Affliction.CreateGiantSpiderVenom(baseDC + self.Owner.Level).StagesDescription}" + "{/i}";
                    }
                },
                AfterYouDealDamage = async (attacker, action, target) => {
                    if (action.Name.Contains($" ({weapon})")) {
                        Affliction poison = Affliction.CreateGiantSpiderVenom(baseDC + attacker.Level);
                        poison.DC = baseDC + attacker.Level;

                        await Affliction.ExposeToInjury(poison, attacker, target);
                    }
                },
                AdditionalGoodness = (self, action, target) => {
                    int dc = baseDC + self.Owner.Level;

                    if (action == null || !(action.Name == weapon || action.HasTrait(Trait.Strike))) {
                        return 0f;
                    }

                    if (target != null && !target.HasEffect(QEffectId.SpiderVenom)) {
                        return 2f;
                    }

                    return 0f;
                }
            };
        }

        public static QEffect ShadowWebSicknessAttack(int baseDC, string weapon) {
            Affliction sws = new Affliction(QEffectIds.ShadowWebSickness, "Shadow Web Sickness", 0,
                "{b}Stage 1{/b} frightened 1; {b}Stage 2{/b} frightened 1 and dazzled; {b}Stage 3{/b} frightened 1 and blinded", 3, stage => null, qf => {
                    qf.Owner.AddQEffect(QEffect.Frightened(1).WithExpirationEphemeral());

                    if (qf.Value == 2)
                        qf.Owner.AddQEffect(QEffect.Dazzled().WithExpirationEphemeral());

                    if (qf.Value == 3)
                        qf.Owner.AddQEffect(QEffect.Blinded().WithExpirationEphemeral());

                });

            return new QEffect("Shadow Web Sickness", "Set Later") {
                StateCheck = async self => {
                    if (self.Description == "Set Later") {
                        self.Name += $" (DC {baseDC + self.Owner.Level})";
                        self.Description = $"Enemies damaged by {self.Owner.Name}'s {weapon} attack are afflicted by Shadow Web Sickness: " + "{i}" + $"{sws.StagesDescription}" + "{/i}";
                    }
                },
                AfterYouDealDamage = async (attacker, action, target) => {
                    if (action.Name.Contains($" ({weapon})")) {
                        Affliction poison = sws;
                        poison.DC = baseDC + attacker.Level;

                        await Affliction.ExposeToInjury(poison, attacker, target);
                    }
                },
                AdditionalGoodness = (self, action, target) => {
                    int dc = baseDC + self.Owner.Level;

                    if (action == null || !(action.Name == weapon || action.HasTrait(Trait.Strike))) {
                        return 0f;
                    }

                    if (target != null && !target.HasEffect(QEffectIds.ShadowWebSickness)) {
                        float val = action.Owner.Level * 2.5f;
                        if (action.Owner.AI.IsTargetNotWorthwhileToFrightened(target))
                            val =- 1 * action.Owner.Level;
                        if (target.HasEffect(QEffectId.Dazzled) || target.IsImmuneTo(Trait.Visual))
                            val = -1 * action.Owner.Level;
                        if (target.HasEffect(QEffectId.Blinded) || target.IsImmuneTo(Trait.Visual))
                            val = -1 * action.Owner.Level;
                        return Math.Max(0, val);
                    }

                    return 0f;
                }
            };
        }

        public static QEffect SerpentVenomAttack(int baseDC, string? weapon) {
            Affliction serpentVenom = new Affliction(QEffectIds.SerpentVenom, "Serpent Venom", 0,
                "{b}Stage 1{/b} 1d6 poison damage and enfeebled 1; {b}Stage 2{/b} 2d6 poison damage and enfeebled 2", 2, stage => stage + "d6", qfVenom => qfVenom.Owner.AddQEffect(QEffect.Enfeebled(qfVenom.Value).WithExpirationEphemeral()));

            return new QEffect("Serpent Poison", "Set Later") {
                StateCheck = async self => {
                    if (self.Description == "Set Later") {
                        self.Name += $" (DC {baseDC + self.Owner.Level})";
                        self.Description = $"Enemies damaged by the {self.Owner.Name}{(weapon == null ? "" : $"'s {weapon} attack")} are afflicted by Serpent Venom: " + "{i}" + $"{serpentVenom.StagesDescription}" + "{/i}";
                    }
                },
                AfterYouDealDamage = async (attacker, action, target) => {
                    if (action.Name.Contains($" ({weapon})")) {
                        Affliction poison = serpentVenom;
                        poison.DC = baseDC + attacker.Level;

                        await Affliction.ExposeToInjury(poison, attacker, target);
                    }
                },
                AdditionalGoodness = (self, action, target) => {
                    int dc = baseDC + self.Owner.Level;

                    if (weapon != null && !(action.Name == weapon || action.HasTrait(Trait.Strike))) {
                        return 0f;
                    }

                    if (target != null && !target.HasEffect(QEffectIds.SerpentVenom)) {
                        return 4f;
                    }

                    return 0f;
                }
            };
        }

        public static QEffect CantOpenDoors() {
            return new QEffect() {
                PreventTakingAction = (action) => action.ActionId == ActionId.OpenADoor ? "No opposable thumbs." : null
            };
        }

        public static QEffect AbyssalRotAttack(int baseDC, string dmg, string weapon) {
            Affliction abyssalRot = new Affliction(QEffectIds.AbyssalRot, "Abyssal Rot", 0,
                "The drained condition from Abyssal rot is cumulative, to a maximum of drained 4; {b}Stage 1{/b} " + dmg + " negative damage; {b}Stage 2{/b} " + dmg + " negative damage and drained 1; {b}Stage 3{/b} " + dmg + " negative damage and drained 2", 3, stage => null, null);
            abyssalRot.EnterStage = async (self, action) => {
                await CommonSpellEffects.DealDirectDamage(action, DiceFormula.FromText(dmg), self.Owner, CheckResult.Failure, DamageKind.Negative);
                if (self.Value >= 2) {
                    int stacks = 1;
                    if (self.Value >= 3) {
                        stacks += 1;
                    }
                    CommonSpellEffects.CumulativeDrain(self.Owner, stacks);
                }
            };

            return new QEffect("Abyssal Rot", "Set Later") {
                StateCheck = async self => {
                    if (self.Description == "Set Later") {
                        self.Name += $" (DC {baseDC + self.Owner.Level})";
                        self.Description = $"Enemies damaged by {self.Owner.Name}'s {weapon} attack are afflicted by Abyssal Rot: " + "{i}" + $"{abyssalRot.StagesDescription}" + "{/i}";
                    }
                },
                AfterYouDealDamage = async (attacker, action, target) => {
                    if (action.Name.Contains($" ({weapon})")) {
                        Affliction poison = abyssalRot;
                        poison.DC = baseDC + attacker.Level;

                        await Affliction.ExposeToInjury(poison, attacker, target);
                    }
                },
                AdditionalGoodness = (self, action, target) => {
                    if (action == null || !(action.Name == weapon || action.HasTrait(Trait.Strike))) {
                        return 0f;
                    }

                    if (target != null && !target.HasEffect(QEffectIds.AbyssalRot)) {
                        return target.Level + DiceFormula.FromText(dmg).ExpectedValue;
                    }

                    return 0f;
                }
            };
        }

        public static QEffect RatPlagueAttack(Creature master, string weapon) {
            Affliction ratPlague = new Affliction(QEffectIds.RatPlague, "Rat Plague", master.ClassOrSpellDC(),
                "{b}Stage 1{/b} 1d6 poison damage and enfeebled 1; {b}Stage 2{/b} 2d6 poison damage and enfeebled 2; {b}Stage 3{/b} 3d6 poison damage and enfeebled 2", 3, stage => $"{stage}d6",
                qf => {
                    if (qf.Value == 1) {
                        qf.Owner.AddQEffect(QEffect.Enfeebled(1).WithExpirationEphemeral());
                    } else {
                        qf.Owner.AddQEffect(QEffect.Enfeebled(2).WithExpirationEphemeral());
                    }
                });

            return new QEffect("Plague Rat", "Set Later") {
                StateCheck = async self => {
                    if (self.Description == "Set Later") {
                        self.Name += $" (DC {master.ClassOrSpellDC()})";
                        self.Description = $"Enemies damaged by {self.Owner.Name}'s {weapon} attack are afflicted by Rat Plague: " + "{i}" + $"{ratPlague.StagesDescription}" + "{/i}";
                    }
                },
                AfterYouDealDamage = async (attacker, action, target) => {
                    if (action.Name.Contains($" ({weapon})")) {
                        Affliction poison = ratPlague;
                        poison.DC = master.ClassOrSpellDC();

                        await Affliction.ExposeToInjury(poison, attacker, target);
                    }
                },
                AdditionalGoodness = (self, action, target) => {
                    if (action == null || !(action.Name == weapon || action.HasTrait(Trait.Strike))) {
                        return 0f;
                    }

                    if (target != null && !target.HasEffect(QEffectIds.RatPlague)) {
                        return 4.5f;
                    }

                    return 0f;
                }
            };
        }

        //public static QEffect MonsterKnockdown()
        //{
        //    return new QEffect("Knockdown", "When your Strike hits, you can spend an action to trip without a trip check.", ExpirationCondition.Never, null, IllustrationName.None)
        //    {
        //        Innate = true,
        //        ProvideMainAction = delegate (QEffect qfGrab)
        //        {
        //            Creature monster = qfGrab.Owner;
        //            IEnumerable<Creature> source = monster.Battle.AllCreatures.Where(delegate (Creature cr)
        //            {
        //                CombatAction combatAction2 = monster.Actions.ActionHistoryThisTurn.LastOrDefault()!;
        //                return (combatAction2 != null && combatAction2.CheckResult >= CheckResult.Success && combatAction2.HasTrait(Trait.Trip) && combatAction2.ChosenTargets.ChosenCreature == cr);
        //            });

        //            return new SubmenuPossibility(IllustrationName.Trip, "Knockdown")
        //            {
        //                Subsections =
        //            {
        //                new PossibilitySection("Knockdown")
        //                {
        //                    Possibilities = source.Select((Func<Creature, Possibility>)delegate(Creature lt)
        //                    {
        //                        CombatAction combatAction = new CombatAction(monster, IllustrationName.Trip, "Trip " + lt.Name, [Trait.Melee], "Trip the target.", Target.ReachWithAnyWeapon((t, a, d) => (!d.HasEffect(QEffectId.Unconscious) && !d.HasEffect(QEffectId.Prone)) ? AIConstants.ALWAYS : AIConstants.NEVER)
        //                            .WithAdditionalConditionOnTargetCreature((Creature a, Creature d) => (d != lt) ? Usability.CommonReasons.TargetIsNotPossibleForComplexReason : Usability.Usable)).WithEffectOnEachTarget(async delegate(CombatAction ca, Creature a, Creature d, CheckResult cr)
        //                        {
        //                            d.AddQEffect(QEffect.Prone());
        //                        });
        //                        return new ActionPossibility(combatAction);
        //                    }).ToList()
        //                }
        //            }
        //            };
        //        }
        //    };
        //}

        public static QEffect MonsterPush()
        {
            return new QEffect("Push", "When your Strike hits, you can spend an action to shove without a shove check.", ExpirationCondition.Never, null, IllustrationName.None)
            {
                Innate = true,
                ProvideMainAction = delegate (QEffect qfGrab)
                {
                    Creature monster = qfGrab.Owner;
                    IEnumerable<Creature> source = monster.Battle.AllCreatures.Where(delegate (Creature cr)
                    {
                        CombatAction combatAction2 = monster.Actions.ActionHistoryThisTurn.LastOrDefault()!;
                        return (combatAction2 != null && combatAction2.CheckResult >= CheckResult.Success && combatAction2.HasTrait(Trait.Shove) && combatAction2.ChosenTargets.ChosenCreature == cr);
                    });

                    return new SubmenuPossibility(IllustrationName.Shove, "Push")
                    {
                        Subsections =
                    {
                        new PossibilitySection("Push")
                        {
                            Possibilities = source.Select((Func<Creature, Possibility>)delegate(Creature lt)
                            {
                                CombatAction combatAction = new CombatAction(monster, IllustrationName.Shove, "Shove" + lt.Name, [Trait.Melee], "Shove the target.", Target.ReachWithWeaponOfTrait(Trait.Shove)
                                    .WithInnerGoodness((t, a, d) => (a.Actions.ActionsLeft == 1 && PushTileIsFree(a, d) && !d.HasEffect(QEffectId.Unconscious)) ? AIConstants.ALWAYS : AIConstants.NEVER)
                                    .WithAdditionalConditionOnTargetCreature((Creature a, Creature d) => (d != lt) ? Usability.CommonReasons.TargetIsNotPossibleForComplexReason : Usability.Usable))
                                .WithEffectOnEachTarget(async delegate(CombatAction ca, Creature a, Creature d, CheckResult cr)
                                {
                                    await monster.PushCreature(d, 1);
                                });
                                return new ActionPossibility(combatAction);
                            }).ToList()
                        }
                    }
                    };
                }
            };
        }

        private static bool PushTileIsFree(Creature user, Creature target)
        {
            var point = new Point(target.Occupies.X + Math.Sign(target.Occupies.X - user.Occupies.X), target.Occupies.Y + Math.Sign(target.Occupies.Y - user.Occupies.Y));
            var tile = user.Battle.Map.GetTile(point.X, point.Y);

            if (tile == null || !tile.LooksFreeTo(user))
            {
                return false;
            }

            return true;
        }

        public static QEffect PreyUpon() {
            return new QEffect("Prey Upon", "Creatures without any allies within 10 feet of them are considered flat-footed against you, unless they're also flanking you.") {
                StateCheck = self => {
                    foreach (Creature enemy in self.Owner.Battle.AllCreatures.Where(cr => cr.Alive && (cr.OwningFaction.IsPlayer || cr.OwningFaction.IsGaiaFriends))) {
                        if (UtilityFunctions.IsFlanking(enemy, self.Owner)) {
                            continue;
                        }
                        
                        //var test = self.Owner.QEffects.Where(qf => qf.Id == QEffectId.FlankedBy);
                        //var test2 = self.Owner.QEffects.Where(qf => qf.Id == QEffectId.FlankedBy && enemy.PrimaryWeapon != null && qf.IsFlatFootedTo(qf, enemy, enemy.CreateStrike(enemy.PrimaryWeapon)) == "flanked");

                        //if (self.Owner.QEffects.Any(qf => qf.Id == QEffectId.FlankedBy && enemy.PrimaryWeapon != null && qf.IsFlatFootedTo(qf, enemy, enemy.CreateStrike(enemy.PrimaryWeapon)) == "flanked")) {
                        //    continue;
                        //}

                        int closeAllies = self.Owner.Battle.AllCreatures.Where(cr => cr != enemy && (cr.OwningFaction.IsPlayer || cr.OwningFaction.IsGaiaFriends) && cr.Alive && cr.DistanceTo(enemy) <= 2).Count();
                        if (closeAllies == 0) {
                            enemy.AddQEffect(new QEffect() {
                                Source = self.Owner,
                                ExpiresAt = ExpirationCondition.Ephemeral,
                                IsFlatFootedTo = (qfFlatFooted, attacker, action) => attacker == qfFlatFooted.Source ? "prey upon" : null
                            });
                        }
                    }
                },
                AdditionalGoodness = (self, action, defender) => {
                    if (UtilityFunctions.IsFlanking(defender, self.Owner)) {
                        return 0;
                    }

                    if (defender.IsFlatFootedTo(self.Owner, action)) {
                        return 0;
                    }
                    int closeAllies = self.Owner.Battle.AllCreatures.Where(cr => cr != defender && (cr.OwningFaction.IsPlayer || cr.OwningFaction.IsGaiaFriends) && cr.Alive && cr.DistanceTo(defender) <= 2).Count();
                    if (closeAllies == 0) {
                        return 4;
                    }
                    return 0;
                }
            };
        }

        public static QEffect WebAttack(int baseDC) {
            return new QEffect() {
                Tag = false,
                ProvideMainAction = self => {
                    return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.Web, "Shoot Web", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Attack },
                        "{b}Range.{/b} 30-feet\n\nOn a hit, the target is immobilized by a web trap, sticking them to the nearest surface. They must use the Escape (DC " + (int)(baseDC + self.Owner.Level) + ") action to free themselves.",
                        Target.Ranged(6)) {
                        ShortDescription = "On a hit, the target is immobilized by a web trap, until they use the Escape (DC " + (int)(baseDC + self.Owner.Level) + ") action to free themselves."
                    }
                    .WithProjectileCone(IllustrationName.Web, 5, ProjectileKind.Cone)
                    .WithSoundEffect(SfxName.AeroBlade)
                    .WithActionCost(1)
                    .WithActiveRollSpecification(new ActiveRollSpecification(Checks.Attack(new Item(IllustrationName.Web, "Web", [Trait.Attack, Trait.Unarmed, Trait.Finesse, Trait.Ranged])), Checks.DefenseDC(Defense.AC)))
                    .WithGoodnessAgainstEnemy((targeting, attacker, defender) => {

                        if (defender.QEffects.FirstOrDefault(qf => qf.Name != null && qf.Name.StartsWith("Webbed (")) != null || self.UsedThisTurn) {
                            return int.MinValue;
                        }

                        if (defender.HasEffect(QEffectId.Immobilized) || defender.HasEffect(QEffectId.Grabbed) || defender.HasEffect(QEffectId.Restrained)) {
                            return 0.1f;
                        }

                        float score = 2.5f;

                        // Dramatic opening attack bonus
                        if ((bool?)self.Tag == false) {
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
                                PreventTakingAction = (ca) => !ca.HasTrait(Trait.Move) ? null : "You're immobilized.",
                                Illustration = IllustrationName.Web,
                                Tag = 14 + caster.Level,
                                ProvideContextualAction = self => {
                                    CombatAction combatAction = new CombatAction(self.Owner, (Illustration)IllustrationName.Escape, "Escape from " + caster?.ToString() + "'s webs.", new Trait[] {
                                        Trait.Attack, Trait.AttackDoesNotTargetAC }, $"Make an unarmed attack, Acrobatics check or Athletics check against the escape DC ({baseDC + caster!.Level}) of the webs.",
                                        Target.Self((_, ai) => ai.EscapeFrom(caster))) {
                                        ActionId = ActionId.Escape
                                    };

                                    ActiveRollSpecification activeRollSpecification = (new ActiveRollSpecification[] {
                                        new ActiveRollSpecification(Checks.Attack(Item.Fist()), Checks.FlatDC(baseDC + caster.Level)),
                                        new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Athletics), Checks.FlatDC(baseDC + caster.Level)),
                                        new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill.Acrobatics), Checks.FlatDC(baseDC + caster.Level))
                                    }).MaxBy(roll => roll.DetermineBonus(combatAction, self.Owner, null).TotalNumber);

                                    return (ActionPossibility)combatAction
                                    .WithActiveRollSpecification(activeRollSpecification)
                                    .WithSoundEffect(combatAction.Owner.HasTrait(Trait.Female) ? SfxName.TripFemale : combatAction.Owner.HasTrait(Trait.Male) ? SfxName.TripMale : SfxName.BeastRoar)
                                    .WithEffectOnEachTarget(async (spell, a, d, cr) => {
                                        switch (cr) {
                                            case CheckResult.CriticalFailure:
                                                a.AddQEffect(new QEffect("Cannot escape", "You can't Escape until your next turn.", ExpirationCondition.ExpiresAtStartOfYourTurn, a) {
                                                    PreventTakingAction = ca => !ca.Name.StartsWith("Escape") ? null : "You already tried to escape and rolled a critical failure."
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
                                    });
                                }
                            };
                            target.AddQEffect(webbed);
                        }
                    })
                    ;
                }
            };
        }

        public static QEffect SlipAway() {
            return new QEffect("Slip Away {icon:Reaction}", "{b}Trigger{/b} You're damaged by an attack. {b}Effect{/b} You make a free step action and gains +1 AC until the end of their attacker's turn.") {
                AfterYouTakeDamage = async (self, amount, kind, action, critical) => {
                    if (action == null || !action.HasTrait(Trait.Attack) || action.Owner == null || action.Owner.Occupies == null || !action.Owner.IsAdjacentTo(self.Owner)) {
                        return;
                    }

                    if (await self.Owner.AskToUseReaction("Use Slip Away to step and gain +1 AC until end of the current turn?")) {
                        self.Owner.AddQEffect(new QEffect("Slip Away", "+1 circumstance bonus to AC.") {
                            Illustration = IllustrationName.Shield,
                            BonusToDefenses = (self, action, defence) => defence == Defense.AC ? new Bonus(1, BonusType.Circumstance, "Slip Away") : null,
                            ExpiresAt = ExpirationCondition.ExpiresAtEndOfAnyTurn
                        });
                        await self.Owner.StepAsync("Choose tile for Slip Away");
                    }
                }
            };
        }

        public static QEffect UnderwaterMarauder() {
            return new QEffect("Underwater Marauder", "You are not flat-footed while underwater, and don't take the usual penalties for using a bludgeoning or slashing melee weapon in water.") {
                YouAcquireQEffect = (self, newEffect) => {
                    if (newEffect.Id == QEffectId.AquaticCombat && newEffect.Name != "Aquatic Combat (underwater marauder)") {
                        return new QEffect("Aquatic Combat (underwater marauder)", "You can't cast fire spells (but fire impulses still work).\nYou can't use slashing or bludgeoning ranged attacks.\nWeapon ranged attacks have their range increments halved.\nYou have resistance 5 to acid and fire.") {
                            Id = QEffectId.AquaticCombat,
                            DoNotShowUpOverhead = self.Owner.HasTrait(Trait.Aquatic),
                            Illustration = IllustrationName.ElementWater,
                            Innate = false,
                            StateCheck = (Action<QEffect>)(qfAquaticCombat => {
                                qfAquaticCombat.Owner.AddQEffect(QEffect.DamageResistance(DamageKind.Acid, 5).WithExpirationEphemeral());
                                qfAquaticCombat.Owner.AddQEffect(QEffect.DamageResistance(DamageKind.Fire, 5).WithExpirationEphemeral());
                                if (qfAquaticCombat.Owner.HasTrait(Trait.Aquatic) || qfAquaticCombat.Owner.HasEffect(QEffectId.Swimming))
                                    return;
                                qfAquaticCombat.Owner.AddQEffect(new QEffect(ExpirationCondition.Ephemeral) {
                                    Id = QEffectId.CountsAllTerrainAsDifficultTerrain
                                });
                            }),
                            PreventTakingAction = (Func<CombatAction, string>)(action => {
                                if (action.HasTrait(Trait.Impulse))
                                    return null!;
                                if (action.HasTrait(Trait.Fire))
                                    return "You can't use fire actions underwater.";
                                return action.HasTrait(Trait.Ranged) && action.HasTrait(Trait.Attack) && IsSlashingOrBludgeoning(action) ? "You can't use slashing or bludgeoning ranged attacks underwater." : null!;
                            })
                        };
                    }
                    return newEffect;
                }
            };
        }

        public static QEffect OceanFlight() {
            return new QEffect() {
                Id = QEffectId.Swimming,
                StateCheck = self => {
                    if (self.Owner.FindQEffect(QEffectId.AquaticCombat) != null) {
                        self.Owner.AddQEffect(QEffect.Flying().WithExpirationEphemeral());
                    }
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

                    if (await effect.Owner.AskToUseReaction($"Your ally {action.Owner.Name} has failed the spider queen with their incompetence. Would you like discipline them, dealing 1d6 slashing damage to allow them to reroll their attack with a +1 bonus?")) {
                        effect.Owner.Overhead("*cruel taskmistress*", Color.Green,
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

                        CombatAction attack = effect.Owner.CreateStrike(fakeWhip, -1, strikeMod).WithActionCost(0);
                        attack.WithActiveRollSpecification(null);
                        attack.Target = new CreatureTarget(RangeKind.Melee, new CreatureTargetingRequirement[] { }, (_1, _2, _3) => int.MinValue);
                        attack.ChosenTargets = ChosenTargets.CreateSingleTarget(action.Owner);
                        await attack.AllExecute();

                        int newValue = R.NextD20();
                        if (newValue > breakdown.D20Roll) {
                            action.Owner.Overhead("", Color.Black,
                                $"{action.Owner.Name} rerolls their attack and takes the new result: {breakdown.D20Roll} > {newValue}",
                                $"{action.Owner.Name} rerolls their attack: {breakdown.D20Roll} > {newValue}",
                                $"{action.Owner.Name} rerolls their attack: {breakdown.D20Roll} > {newValue}.\n\nThey have taken the new higher value.");
                            //breakdown.GetType().GetField("<D20Roll>k__BackingField", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).SetValue(breakdown, newValue);
                            //breakdown.GetType().GetField("<FirstD20Roll>k__BackingField", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).SetValue(breakdown, newValue);
                            breakdown.D20Roll = newValue;
                            breakdown.FirstD20Roll = newValue;
                        } else {
                            action.Owner.Overhead("", Color.Black,
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

        public static QEffect RetributiveStrike(int baseReduction, Func<Creature, bool> filter, string targetDesc, bool step) {

            QEffect effect = new QEffect("Retributive Strike {icon:Reaction}", "...") {
                Innate = true,
                StartOfCombat = async self => {
                    self.Description = "{b}Trigger{/b} An enemy damages " + targetDesc + " and both are within 15 feet of you. " +
                        "{b}Effect{/b} The ally gains resistance " + (baseReduction + self.Owner.Level) + " to all damage against the triggering attack. If the foe is within reach, make a melee Strike against it." +
                        (step ? " If the target is out of range, you can step to put the foe within your reach." : "");
                }
            };

            effect.AddGrantingOfTechnical(filter, qf => {
                qf.YouAreDealtDamage = async (qfAlly, attacker, damageStuff, defender) => {
                    if (attacker == null || attacker.Occupies == null || !attacker.EnemyOf(effect.Owner) || attacker.DistanceTo(effect.Owner) > 3 || effect.Owner.DistanceTo(defender) > 3 || effect.Owner?.PrimaryWeapon == null)
                        return null;

                    if (!effect.Owner.CreateStrike(effect.Owner.PrimaryWeapon).CanBeginToUse(attacker)) {
                        return null;
                    }

                    if (!await effect.Owner.Battle.AskToUseReaction(effect.Owner, attacker.ToString() + " is about to deal " + damageStuff.Amount.ToString() + " damage to " + defender?.ToString() + ". Use your champion's reaction to prevent " + (baseReduction + effect.Owner.Level).ToString() + " of that damage?"))
                        return null;

                    List<Tile> validStepTiles = effect.Owner.Battle.Map.AllTiles.Where(t => t.IsFree && t.IsAdjacentTo(effect.Owner.Occupies) && t.DistanceTo(attacker.Occupies) < 3 && attacker.HasLineOfEffectTo(attacker.Occupies) < CoverKind.Blocked).ToList();

                    effect.Owner.Overhead("retributive strike!", Color.Orange, effect.Owner?.ToString() + " uses retributive strike!");
                    effect.Owner!.AddQEffect(new QEffect(ExpirationCondition.Never) {
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
                                Tile bestTile = validStepTiles.OrderBy(t => t.HasLineOfEffectToIgnoreLesser(attacker.Occupies)).ToArray()[0];
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
                            meleeStrike = null;
                            meleeStrikeTarget = null;
                        }
                    });
                    return new ReduceDamageModification(baseReduction + effect.Owner.Level, "Retributive Strike");
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
                    if (self.Owner.HP > self.Owner.MaxHP * 0.33) {
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
                        return 15f + d.HP / d.MaxHP * 4;
                    })
                    ;
                }
            };
        }

        public static QEffect MothersProtection() {
            return new QEffect("Mother's Protection {icon:Reaction}", "Upon taking damage, Echidna protects this creature by transfering the damage dealt to a healthier friendly beast or animal within 10 feet.") {
                YouAreDealtDamage = async (self, attacker, damageStuff, defender) => {
                    Creature? protector = defender.Battle.AllCreatures.Where(cr => cr.DistanceTo(defender) <= 2 && IsMonsterAlly(self.Owner, cr)).MaxBy(cr => cr.HP);

                    if (protector == null || damageStuff.Amount == 0 || (protector.HP < defender.HP && damageStuff.Amount < defender.HP) || damageStuff.Amount >= protector.HP) {
                        return null;
                    }

                    if (await defender.AskToUseReaction($"Use your reaction to have {protector.Name} take {damageStuff.Amount} damage instead of you?")) {
                        Sfxs.Play(SfxName.Abjuration);
                        defender.Overhead("mother's protection", Color.Crimson, $"{defender.Name } used Mother's Protection to transfer their wounds to {protector.Name}.");
                        await CommonSpellEffects.DealDirectDamage(CombatAction.CreateSimple(defender.Battle.Pseudocreature, "Mother's Protection"), DiceFormula.FromText(damageStuff.Amount.ToString()), protector, CheckResult.Failure, DamageKind.Untyped);

                        return new ReduceDamageModification(damageStuff.Amount, $"Protected by {protector.Name}");
                    }

                    return null;
                }
            };
        }

        public static QEffect BlessedOfEchidna() {
            return new QEffect("Blessed of the Echidna", "You automatically critically succeed against all save effects from allied animals and beasts.") {
                Id = QEffectIds.BlessedOfEchidna,
                AfterYouMakeSavingThrow = (self, action, breakdown) => {
                    if (IsMonsterAlly(self.Owner, action.Owner))
                        breakdown.CheckResult = CheckResult.CriticalSuccess;
                }
            };
        }

        public static TileQEffect Maelstrom(int dc, Tile owner, Creature source) {
            return new TileQEffect(owner) {
                TileQEffectId = QEffectIds.Maelstrom,
                AfterCreatureEntersHere = async creature => {
                    if (!(creature.OwningFaction.IsPlayer || creature.OwningFaction.IsGaiaFriends)) {
                        return;
                    }

                    CombatAction ca = new CombatAction(source, IllustrationName.TidalHands, "Maelstrom", [Trait.Water, Trait.Evocation], "", Target.Ranged(100))
                    .WithActionCost(0)
                    .WithSavingThrow(new SavingThrow(Defense.Fortitude, dc))
                    .WithSoundEffect(SfxName.ElementalBlastWater)
                    .WithEffectOnEachTarget(async (spell, user, d, result) => {
                        await CommonSpellEffects.DealBasicDamage(spell, user, d, result, DiceFormula.FromText($"1d8", "Maelstrom"), DamageKind.Bludgeoning);
                    });

                    ca.ChosenTargets.ChosenCreatures.Add(creature);
                    ca.ChosenTargets.ChosenCreature = creature;
                    await ca.AllExecute();
                },
                TransformsTileIntoHazardousTerrain = true
            };
        }

        /// <summary>
        /// Creature with this qeffect should be counted as two creatures for the purpose of encounter balancing. You can use it to create a boss monster with appropriate stats, without also giving them a frustrating amount of AC.
        /// </summary>
        public static QEffect MiniBoss() {
            QEffect effect = new QEffect("Powerful Adversary", "The first time this creature acts each round while above half HP, it only drops down two places in the intitive order. In addition it has significantly increased HP for its level.") {
                StartOfCombat = async self => {
                    self.Owner.MaxHP = self.Owner.MaxHP * 2;
                    self.Owner.AddQEffect(new QEffect("Extra Turn", "This creature will only move down two spaces in inititve order after this turn.") {
                        Innate = false,
                        Illustration = IllustrationName.Haste,
                        Id = QEffectIds.ExtraTurn,
                        StateCheck = self => {
                            if (self.Owner.Damage >= self.Owner.MaxHP * 0.5f) {
                                self.Illustration = null;
                                self.Name = null;
                                self.Description = null;
                            } else {
                                self.Illustration = IllustrationName.Haste;
                                self.Name = "Extra Turn";
                                self.Description = "This creature will only move down two spaces in inititve order after this turn.";
                            }
                        }
                    });
                },
            };

            effect.AddGrantingOfTechnical(cr => effect.Owner.Battle.InitiativeOrder.Contains(cr), qf => {
                qf.StartOfYourPrimaryTurn = async (tmp, owner) => {
                    List<Creature> initOrder = effect.Owner.Battle.InitiativeOrder.Where(cr => cr.AliveOrUnconscious).ToList();

                    if (effect.Owner.Battle.ActiveCreature == null) return;

                    // Skip if boss isn't going on your next turn
                    if (initOrder.IndexOf(effect.Owner) != initOrder.IndexOf(effect.Owner.Battle.ActiveCreature) - 1) {
                        // Stop if you aren't last in the turn order AND the boss goes first (wrap around check)
                        if (!(effect.Owner == initOrder.Last() && effect.Owner.Battle.ActiveCreature == initOrder.First()) || owner.Battle.RoundNumber == 1) {
                            return;
                        }
                    }

                    // Skip if under half HP
                    if (effect.Owner.Damage >= effect.Owner.MaxHP * 0.5f) {
                        return;
                    }

                    // Skip if boss doesn't have the extra turn buff
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
                    if (pos == initOrder.Count + 1) {
                        newPos = 2;
                    } else if (pos == initOrder.Count) {
                        newPos = 1;
                    } else if (pos == initOrder.Count - 1) {
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

                    if (!newOrder.Contains(effect.Owner)) {
                        newOrder.Add(effect.Owner);
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

        public static bool IsMonsterAlly(Creature user, Creature? otherCreature) {
            if (otherCreature == null)
                return false;

            return otherCreature.FriendOf(user) && !otherCreature.HasTrait(Trait.Celestial) && (otherCreature.HasTrait(Trait.Beast) || otherCreature.HasTrait(Trait.Animal) || otherCreature.HasTrait(ModTraits.Monstrous));
        }

        private static bool IsSlashingOrBludgeoning(CombatAction action) {
            Item obj1 = action.Item;
            DamageKind? damageKind1;
            int num1;
            if (obj1 == null) {
                num1 = 0;
            } else {
                damageKind1 = obj1.WeaponProperties?.DamageKind;
                DamageKind damageKind2 = DamageKind.Slashing;
                num1 = damageKind1.GetValueOrDefault() == damageKind2 & damageKind1.HasValue ? 1 : 0;
            }
            if (num1 == 0) {
                Item obj2 = action.Item;
                int num2;
                if (obj2 == null) {
                    num2 = 0;
                } else {
                    damageKind1 = obj2.WeaponProperties?.DamageKind;
                    DamageKind damageKind3 = DamageKind.Bludgeoning;
                    num2 = damageKind1.GetValueOrDefault() == damageKind3 & damageKind1.HasValue ? 1 : 0;
                }
                if (num2 == 0)
                    return false;
            }
            Item obj3 = action.Item;
            return (obj3 != null ? (obj3.HasTrait(Trait.VersatileP) ? 1 : 0) : 0) == 0;
        }
    }
}
