using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.IO;
using Dawnsbury.Mods.Classes.Summoner;
using static Dawnsbury.Mods.Classes.Summoner.Enums;
using static Dawnsbury.Mods.Classes.Summoner.SummonerClassLoader;

namespace Dawnsbury.Mods.Classes.Summoner {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class ClassLogic {

        public static void GenerateActionShareQEffect(Creature summoner, Creature eidolon, bool forEidolon = false, bool archetype=false, bool forceTandemActionsMenu=false) {
            var you = forEidolon ? eidolon : summoner;
            var partner = forEidolon ? summoner : eidolon;

            var qfActionEffect = new ActionShareEffect() {
                Id = qfSharedActions,

                StateCheck = qfSelf => {
                    if (!forEidolon) {
                        if (eidolon.Battle.InitiativeOrder.Any(cr => cr == eidolon)) {
                            eidolon.Battle.InitiativeOrder.Remove(eidolon);
                        }
                    }

                    // PAST THIS POINT, INACTIVE EIDOLON NOT AFFECTED
                    if (eidolon.Destroyed) return;

                    // Reaction
                    if (you.Actions.IsReactionUsedUp == true) {
                        partner.Actions.UseUpReaction();
                    }

                    if (partner.HasEffect(QEffectId.DurationStunned)) {
                        you.AddQEffect(QEffect.Stunned().WithExpirationEphemeral());
                    }
                },
                AfterYouTakeAction = async (qf, action) => {
                    if (eidolon.Destroyed) return;

                    // Focus points
                    if (action.HasTrait(Trait.Focus)) {
                        partner.Spellcasting?.FocusPoints = you.Spellcasting?.FocusPoints ?? 0;
                    }

                    // MAP
                    if (action.Traits.Contains(Trait.Attack)) {
                        partner.Actions.AttackedThisManyTimesThisTurn = you.Actions.AttackedThisManyTimesThisTurn;
                    }
                }
            };

            if (forEidolon) {
                qfActionEffect.ProvideMainAction = qfEidolon => {
                    if (summoner.OwningFaction != qfEidolon.Owner.OwningFaction || !summoner.Actions.CanTakeActions() || qfEidolon.Owner.QEffects.FirstOrDefault(qf => qf.Id == qfActTogether) != null)
                        return null;
                    return new ActionPossibility(new CombatAction(you, summoner.Illustration, "Return Control",
                        [Trait.Basic, tSummoner], $"Switch back to controlling {summoner.Name}. All unspent actions will be retained.", Target.Self())
                    .WithActionCost(0)
                    .WithActionId(ActionId.EndTurn)
                    .WithEffectOnSelf(self => {
                        ActionShareEffect actionShare = (ActionShareEffect)qfEidolon;
                        if (actionShare == null) return;

                        // Remove act together toggle on eidolon
                        self.RemoveAllQEffects(qf => qf.Id == qfActTogetherToggle);
                        // Remove and log actions
                        actionShare.LogTurnEnd(self.Actions);
                        self.Actions.UsedQuickenedAction = true;
                        self.Actions.ActionsLeft = 0;
                        self.Actions.WishesToEndTurn = true;
                        Sfxs.Play(SfxName.EndOfTurn, 0.2f);
                    }));
                };
            }

            if (you == summoner) {
                qfActionEffect.ProvideMainAction = qfSelf => {
                    if (eidolon.OwningFaction != you.OwningFaction || !eidolon.Actions.CanTakeActions() || you.HasEffect(qfActTogether))
                        return null;

                    Possibility output = (ActionPossibility)new CombatAction(you, eidolon.Illustration, "Command your Eidolon", new Trait[] { Trait.Basic, tSummoner }, "Swap to Eidolon.", Target.Self()) {
                        ShortDescription = "Take control of your Eidolon, using your shared action pool."
                    }
                    .WithEffectOnSelf(async self => {
                        if (eidolon.HasEffect(QEffectId.Confused) && (await summoner.Battle.SendRequest(new ConfirmationRequest(summoner, "Your eidolon is confused and will use all of your shared actions without input. Are you sure you want to swap to them?", eidolon.Illustration, "Yes", "No, skip their action"))).ChosenOption is CancelOption) {
                            return;
                        }

                        await PartnerActs(you, partner);
                    })
                    .WithActionCost(0);

                    return output;
                };
                qfActionEffect.EndOfYourTurnBeneficialEffect = async (qfEndOfTurn, summoner) => {
                    eidolon?.Actions.ForgetAllTurnCounters();
                    summoner.Battle.ActiveCreature = summoner;
                };
                qfActionEffect.StartOfYourPrimaryTurn = async (qfStartOfTurn, summoner) => {
                    if (eidolon!.Destroyed || eidolon.HP <= 0) {
                        return;
                    }
                    eidolon.TurnInformation.ThisTurnIsPrimary = true;

                    // Share eidolon quickened with summoner
                    if (eidolon.Actions.QuickenedForActions != null) {
                        foreach (var rule in (List<Func<CombatAction, bool>>)eidolon.Actions.QuickenedForActions.GetType().GetField("delegates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)?.GetValue(eidolon.Actions.QuickenedForActions)!) {
                            if (summoner.Actions.QuickenedForActions == null) {
                                summoner.Actions.QuickenedForActions = new DisjunctionDelegate<CombatAction>(rule);
                            } else {
                                summoner.Actions.QuickenedForActions.Add(rule);
                            }
                        }
                        summoner.Actions.UsedQuickenedAction = eidolon.Actions.UsedQuickenedAction;
                        summoner.Actions.AnimateActionUsedTo(3, eidolon.Actions.FourthActionStyle);
                    }

                    bool quickened = summoner.Actions.QuickenedForActions != null && !summoner.Actions.UsedQuickenedAction;

                    // delegates

                    await eidolon.Battle.GameLoop.StateCheck();

                    // Handle slowed
                    QEffect? eSlowed = eidolon.QEffects.FirstOrDefault(qf => qf.Id == QEffectId.Slowed);
                    QEffect? sSlowed = summoner.QEffects.FirstOrDefault(qf => qf.Id == QEffectId.Slowed);

                    if (eSlowed != null && (sSlowed == null || sSlowed.Value < eSlowed.Value)) {
                        summoner.Actions.ActionsLeft -= eSlowed.Value;
                        for (int i = 0; i < eSlowed.Value - (quickened ? 1 : 0); i++) {
                            summoner.Actions.AnimateActionUsedTo(i, ActionDisplayStyle.Slowed);
                        }
                        if (quickened) {
                            summoner.Actions.UsedQuickenedAction = true;
                            summoner.Actions.AnimateActionUsedTo(3, ActionDisplayStyle.Slowed);
                        }
                    } else if (quickened && sSlowed != null && (eSlowed == null || eSlowed.Value < sSlowed.Value)) {
                        summoner.Actions.ActionsLeft += 1;
                        summoner.Actions.AnimateActionUsedTo(sSlowed.Value - 1, ActionDisplayStyle.Available);
                        summoner.Actions.UsedQuickenedAction = true;
                        summoner.Actions.AnimateActionUsedTo(3, ActionDisplayStyle.Slowed);
                    }

                    if (!archetype && PlayerProfile.Instance.IsBooleanOptionEnabled("Summoner_AutoUseActTogether") || !summoner.OwningFaction.IsControlledByHumanUser || summoner.HasEffect(QEffectId.Controlled)) {
                        var ca = (GenerateActTogetherAction(summoner, eidolon, summoner) as ActionPossibility)?.CombatAction;
                        if (ca != null && summoner.Actions.CanTakeActions() && !summoner.HasEffect(QEffectId.Confused) && !summoner.HasEffect(QEffectId.DurationStunned)) {
                            ca.ChosenTargets.ChosenCreature = summoner;
                            await ca.AllExecute();
                        }
                    }
                };
            }
            // Add act together
            if (!archetype || PlayerProfile.Instance.IsBooleanOptionEnabled("Summoner_ImprovedArchetype")) {
                you.AddQEffect(new QEffect() {
                    ProvideMainAction = (effect) => {
                        if (partner.Destroyed) return null;

                        if (summoner.PersistentCharacterSheet?.Calculated.AllFeats.Where(ft => ft.HasTrait(tTandem)).ToList().Count > 0 || forceTandemActionsMenu) {
                            SubmenuPossibility tandemActions = new SubmenuPossibility(illActTogether, "Tandem Actions");
                            tandemActions.Subsections.Add(new PossibilitySection("Tandem Actions"));
                            tandemActions.Subsections[0].PossibilitySectionId = psTandemActions;
                            return tandemActions;
                        }

                        return GenerateActTogetherAction(you, partner, summoner, archetype && PlayerProfile.Instance.IsBooleanOptionEnabled("Summoner_ImprovedArchetype"));

                    },
                    ProvideActionIntoPossibilitySection = (effect, section) => {
                        if (summoner.PersistentCharacterSheet?.Calculated.AllFeats.Where(ft => ft.HasTrait(tTandem)).ToList().Count == 0) {
                            return null;
                        } else if (section.PossibilitySectionId == psTandemActions) {
                            return GenerateActTogetherAction(you, partner, summoner, archetype && PlayerProfile.Instance.IsBooleanOptionEnabled("Summoner_ImprovedArchetype"));
                        }
                        return null;
                    }
                });
            }

            you.AddQEffect(qfActionEffect);
        }

        public static void GenerateHPShareQEffect(Creature summoner, Creature eidolon, bool forEidolon=false) {
            var you = forEidolon ? eidolon : summoner;
            var partner = forEidolon ? summoner : eidolon;

            you.AddQEffect(new QEffect() {
                StateCheckLayer = 1,
                StateCheck = self => {
                    HandleDrainedSharing(self.Owner, eidolon, true);
                }
            });

            var qfHPHandler = new HPShareEffect(you) {
                Id = qfSummonerBond,
                Source = partner,
                StateCheck = async qf => {
                    // PAST THIS POINT, INACTIVE EIDOLON NOT AFFECTED
                    if (eidolon.Destroyed == true) {
                        return;
                    }

                    // Handle tempHP
                    if (you.TemporaryHP < partner.TemporaryHP) {
                        you.GainTemporaryHP(partner.TemporaryHP);
                    } else if (you.TemporaryHP > partner.TemporaryHP) {
                        partner.GainTemporaryHP(you.TemporaryHP);
                    }
                },
                YouAreTargeted = async (qfHealOrHarm, action) => {
                    if (action.Name == "Command your Eidolon") {
                        return;
                    }

                    if (eidolon.Destroyed) {
                        return;
                    }

                    HPShareEffect shareHP = (HPShareEffect)you.FindQEffect(qfSummonerBond);
                    HPShareEffect partnerShareHP = (HPShareEffect)partner.FindQEffect(qfSummonerBond);

                    if (shareHP == null || partnerShareHP == null) return;

                    if (action == shareHP!.CA || action == partnerShareHP.CA || (action.Target is not AreaTarget
                            && !(action.Target is DependsOnActionsSpentTarget ap && ap.TargetFromActionCount(action.SpentActions) is AreaTarget)
                            && !(action.Target is DependsOnSpellVariantTarget sv && sv.CreateTargetFromVariant(action.ChosenVariant!) is AreaTarget))) {
                        return;
                    }

                    shareHP.LogAction(you, action, action.Owner, SummonerClassEnums.InterceptKind.TARGET);

                },
                AfterYouAreTargeted = async (qfShareHP, action) => {
                    if (action.Name == "Command your Eidolon" || eidolon.Destroyed) {
                        return;
                    }

                    await HandleHealthShare(you, partner, SummonerClassEnums.InterceptKind.TARGET, action.Name);
                },
                YouAreDealtDamage = async (qfPreHazardDamage, attacker, damageStuff, defender) => {
                    if (eidolon.Destroyed) {
                        return null;
                    }

                    // TODO: Should this check be eidolon only?
                    if (damageStuff.Power?.Name == "SummonerClass: Share HP" || (damageStuff.Power != null && damageStuff.Power.HasTrait(tTandem))) {
                        return null;
                    }

                    HPShareEffect shareHP = (HPShareEffect)you.FindQEffect(qfSummonerBond);

                    // Check if caught by target check
                    if (shareHP!.CheckForTargetLog(damageStuff.Power!, attacker)) {
                        return null;
                    }

                    shareHP.LogAction(you, damageStuff.Power, attacker, SummonerClassEnums.InterceptKind.DAMAGE);

                    return null;
                },
                AfterYouTakeDamageOfKind = async (qfPostHazardDamage, action, kind) => {
                    if (eidolon.Destroyed) {
                        return;
                    }

                    await HandleHealthShare(you, partner, SummonerClassEnums.InterceptKind.DAMAGE, action?.Name);
                },
                AfterYouAreHealed = async (self, action, amount) => {
                    if (eidolon.Destroyed) {
                        return;
                    }

                    // Check if effect is coming from self
                    if (action == null || action.Name == "SummonerClass: Share HP" || (forEidolon && action.HasTrait(tTandem))) {
                        return;
                    }

                    HPShareEffect shareHP = (HPShareEffect)you.FindQEffect(qfSummonerBond);

                    // Check if caught by target check
                    if (shareHP!.CheckForTargetLog(action, action?.Owner)) {
                        return;
                    }
                    FlatHeal(partner, DiceFormula.FromText($"{amount}", $"Eidolon Health Share ({action?.Name})"), shareHP.CA);
                },
                AfterYouAcquireEffect = async (self, nQf) => {
                    if (nQf.Id == QEffectId.Stunned) {
                        partner.AddQEffect(nQf);
                    }

                    if (you == summoner && nQf.Id == QEffectId.Unconscious) {
                        you.Battle.RemoveCreatureFromGame(eidolon);
                    }

                    if (nQf.Id == QEffectId.Drained || nQf.Id == QEffectId.MummyRot) {
                        HandleDrainedSharing(you, partner, true, true);
                    }
                },
                EndOfAnyTurn = self => {
                    HPShareEffect shareHP = (HPShareEffect)you.FindQEffect(qfSummonerBond);
                    if (shareHP == null) return;

                    shareHP.Reset();

                    if (forEidolon) {
                        // Catch unhandled hazard healing effects
                        if (partner.HP > you.HP) {
                            int healing = partner.HP - you.HP;
                            FlatHeal(you, DiceFormula.FromText($"{healing}"), shareHP.CA);
                        } else if (partner.HP < you.HP) {
                            int healing = you.HP - partner.HP;
                            FlatHeal(partner, DiceFormula.FromText($"{healing}"), shareHP.CA);
                        }
                    }
                    else if (!eidolon.Destroyed)
                        // Emergency HP correction
                        HealthShareSafetyCheck(you, partner);
                },
                WhenMonsterDies = async self => {
                    if (partner.Alive) {
                        self.Tag = true;
                    }
                },
                StateCheckWithVisibleChanges = async qfSelf => {
                    // Handle instant death effect
                    if (qfSelf.Tag is bool && partner.Alive) {
                        await CommonSpellEffects.DealDirectSplashDamage(CombatAction.CreateSimple(you, "Eidolon Health Share"), DiceFormula.FromText("999", "Eidolon Hit by Instant Death Effect"), partner, DamageKind.Untyped);
                        qfSelf.Tag = null;
                    }
                }
            };

            //if (you == summoner) {
            //    qfHPHandler.WhenMonsterDies = async self => {
            //        if (!summoner.Alive) {
            //            self.Tag = true;
            //        }
            //    };
            //}

            //if (you == eidolon) {

            //}
            you.AddQEffect(qfHPHandler);
        }

        public static void GenerateEidolonLogic(Creature eidolon, Creature summoner) {
            eidolon.AddQEffect(new QEffect() {
                PreventTakingAction = action => {
                    if (action.ActionId == ActionId.Delay) {
                        return "Your eidolon cannot take this action.";
                    }
                    return null;
                },
                PreventTargetingBy = action => {
                    if (action.SpellId == SpellId.Dominate) {
                        return "Eidolons cannot be dominated.";
                    }
                    return null;
                },
                BonusToSpellSaveDCs = qf => {
                    if (qf.Owner.Spellcasting == null) {
                        return null;
                    }

                    int sDC = summoner.Proficiencies.Get(Trait.Spell).ToNumber(summoner.ProficiencyLevel) + summoner.Spellcasting?.GetSourceByOrigin(tSummoner)?.SpellcastingAbilityModifier ?? 0;
                    int eDC = qf.Owner.Proficiencies.Get(Trait.Spell).ToNumber(qf.Owner.ProficiencyLevel) + eidolon.Spellcasting?.GetSourceByOrigin(tSummoner)?.SpellcastingAbilityModifier ?? 0;
                    return new Bonus(sDC - eDC, BonusType.Untyped, "Summoner Spellcasting DC");
                },
                BonusToAttackRolls = (qf, action, target) => {
                    if (action.HasTrait(Trait.Spell)) {
                        if (qf.Owner.Spellcasting == null) {
                            return null;
                        }

                        int sDC = summoner.Proficiencies.Get(Trait.Spell).ToNumber(summoner.ProficiencyLevel) + summoner.Spellcasting?.GetSourceByOrigin(tSummoner)?.SpellcastingAbilityModifier ?? 0;
                        int eDC = eidolon.Proficiencies.Get(Trait.Spell).ToNumber(eidolon.ProficiencyLevel) + eidolon.Spellcasting?.GetSourceByOrigin(tSummoner)?.SpellcastingAbilityModifier ?? 0;
                        return new Bonus(sDC - eDC, BonusType.Untyped, "Summoner Spellcasting Attack Bonus");
                    }
                    return null;
                },
            });
        }

        public static void GenerateSummonerActions(Creature summoner, Creature eidolon, Trait tradition) {
            // Add manifest and dismiss
            summoner.AddQEffect(new QEffect() {
                ProvideActionIntoPossibilitySection = (self, section) => {
                    if (section.PossibilitySectionId != psSummonerExtra) {
                        return null;
                    }

                    if (!eidolon.Destroyed) {
                        return new ActionPossibility(new CombatAction(self.Owner, illDismiss, "Dismiss Eidolon", [tSummoner, Trait.Concentrate, Trait.Conjuration, Trait.Manipulate, Trait.Teleportation, Trait.Basic, tradition],
                            "Dismiss your eidolon, protecting it and yourself from harm.", Target.RangedFriend(20).WithAdditionalConditionOnTargetCreature(new EidolonCreatureTargetingRequirement(qfSummonerBond)))
                        .WithEffectOnChosenTargets(async (self, targets) => {
                            self.Battle.RemoveCreatureFromGame(eidolon);
                        })
                        .WithActionCost(3))
                        .WithPossibilityGroup("Summoner");
                    }
                    return null;
                },
                ProvideMainAction = qfManifestEidolon => {
                    Creature? eidolon = GetEidolon(qfManifestEidolon.Owner);
                    QEffect actTogether = new QEffect("Recently Manifested", "Immediately take a single 1 cost action.") {
                        Illustration = IllustrationName.Haste,
                        Id = qfActTogether,
                    };
                    if (eidolon == null) {
                        return null;
                    }

                    if (eidolon.Destroyed) {
                        return new ActionPossibility(new CombatAction(qfManifestEidolon.Owner, eidolon.Illustration, "Manifest Eidolon", [tSummoner, Trait.Concentrate, Trait.Conjuration, Trait.Manipulate, Trait.Teleportation, tradition],
                            "Your eidolon appears in an open space adjacent to you, and can then take a single action.", Target.RangedEmptyTileForSummoning(1)) {
                            ShortDescription = "Your eidolon reappears in an open space adjacent to you, and can then take a single action."
                        }
                        .WithEffectOnChosenTargets(async (self, targets) => {
                            HPShareEffect shareHP = (HPShareEffect)summoner.FindQEffect(qfSummonerBond);
                            if (shareHP == null) return;

                            eidolon.Battle.InitiativeOrder.Remove(eidolon);
                            eidolon.Battle.Corpses.Remove(eidolon);
                            eidolon.Occupies = targets.ChosenTile!;
                            eidolon.RemoveAllQEffects(qf => qf.Illustration != null);
                            eidolon.AddQEffect(actTogether);
                            eidolon.Battle.SpawnCreature(eidolon, self.OwningFaction, targets.ChosenTile!);
                            eidolon.Actions.AnimateActionUsedTo(0, ActionDisplayStyle.UsedUp);
                            eidolon.Actions.AnimateActionUsedTo(1, ActionDisplayStyle.UsedUp);
                            eidolon.Actions.AnimateActionUsedTo(2, ActionDisplayStyle.UsedUp);
                            eidolon.Actions.AnimateActionUsedTo(3, ActionDisplayStyle.Invisible);
                            eidolon.Destroyed = false;
                            eidolon.DeathScheduledForNextStateCheck = false;
                            eidolon.Actions.ActionsLeft = 0;
                            eidolon.Actions.UsedQuickenedAction = true;
                            eidolon.AnimationData.ChangeSize(eidolon);
                            // Balance HP
                            if (eidolon.Damage > summoner.Damage) {
                                FlatHeal(eidolon, DiceFormula.FromText($"{eidolon.Damage - summoner.Damage}"), shareHP!.CA);
                            } else if (eidolon.Damage < summoner.Damage) {
                                await CommonSpellEffects.DealDirectSplashDamage(shareHP!.CA, DiceFormula.FromText($"{summoner.Damage - eidolon.Damage}"), eidolon, DamageKind.Untyped);
                            }
                            HandleDrainedSharing(summoner, eidolon, true);
                            HandleDrainedSharing(eidolon, summoner, false);
                            await eidolon.Battle.GameLoop.StateCheck();
                            eidolon.Destroyed = false;
                            await PartnerActs(summoner, eidolon, true, null);
                            eidolon.RemoveAllQEffects(effect => effect == actTogether);
                        })
                        .WithActionCost(qfManifestEidolon.Owner.Level < 19 ? 3 : 1))
                        .WithPossibilityGroup("Summoner");
                    }
                    return null;
                }
            });
        }
    }
}
