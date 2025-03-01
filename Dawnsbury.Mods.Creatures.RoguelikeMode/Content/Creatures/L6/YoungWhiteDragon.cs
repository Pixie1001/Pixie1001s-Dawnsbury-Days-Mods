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
using System.Runtime.CompilerServices;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class YoungWhiteDragon
    {
        public static Creature Create()
        {
            return new Creature(Illustrations.WhiteDragon, "Young White Dragon", [Trait.Chaotic, Trait.Evil, Trait.Cold, Trait.Dragon], 6, 13, 16, new Defenses(23, 16, 14, 11), 115, new Abilities(6, 2, 4, -1, 1, 0), new Skills(acrobatics: 10, arcana: 7, athletics: 16, deception: 15, intimidation: 12, stealth: 14))
                .WithCharacteristics(false, true)
                .AddQEffect(QEffect.Flying())
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Cold))
                .AddQEffect(QEffect.TraitImmunity(Trait.Cold))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Paralyzed))
                .AddQEffect(QEffect.DamageWeakness(DamageKind.Fire, 5))
                .AddQEffect(QEffect.BreathWeapon("icey breath", Target.Cone(6), Defense.Reflex, 24, DamageKind.Cold, DiceFormula.FromText("7d6", "icey breath"), SfxName.RayOfFrost))
                .AddQEffect(QEffect.FrightfulPresence(18, 20))
                .AddQEffect(new QEffect()
                {
                    YouBeginAction = async (qfBeforeAction, action) =>
                    {
                        if (action.Item?.Name == NaturalWeapons.GetName(NaturalWeaponKind.Tail))
                        {
                            action.ProjectileKind = ProjectileKind.None;
                        }
                    }
                })
                .AddQEffect(new QEffect("Draconic Frenzy {icon:TwoActions}", "Make two claw Strikes and one tail Strike in any order.")
                {
                    ProvideMainAction = (qfMainAction) =>
                    {
                        Creature self = qfMainAction.Owner;
                        Illustration draconicFrenzyIllustration = new SideBySideIllustration(NaturalWeapons.GetIllustration(NaturalWeaponKind.Claw), NaturalWeapons.GetIllustration(NaturalWeaponKind.Tail));
                        CombatAction draconicFrenzyAction = new CombatAction(self, draconicFrenzyIllustration, "Draconic Frenzy", [], "Make two claw Strikes and one tail Strike in any order.", Target.Self()
                            .WithAdditionalRestriction((innerSelf) =>
                            {
                                if (!innerSelf.Battle.AllCreatures.Any(creature => !innerSelf.FriendOf(creature) && innerSelf.HasLineOfEffectTo(creature.Occupies) < CoverKind.Blocked && innerSelf.DistanceTo(creature.Occupies) <= 2))
                                {
                                    return "no creature in Claw range";
                                }

                                return null;
                            }))
                            .WithActionCost(2)
                            .WithEffectOnEachTarget(async (frenzy, innerSelf, target, result) =>
                            {
                                int clawStrikesRemaining = 2;
                                int tailStrikesRemaining = 1;


                                // See Flurry of Blows to better understand logic
                                for (int i = 0; i < 3; i++)
                                {
                                    await innerSelf.Battle.GameLoop.StateCheck();
                                    var possibilities = new List<Option>();

                                    string clawItemName = NaturalWeapons.GetName(NaturalWeaponKind.Claw);
                                    string tailItemName = NaturalWeapons.GetName(NaturalWeaponKind.Tail);
                                    Item? claw = innerSelf.UnarmedStrike.Name == clawItemName ? innerSelf.UnarmedStrike : innerSelf.QEffects.FirstOrDefault(qe => qe.AdditionalUnarmedStrike != null && qe.AdditionalUnarmedStrike.Name == clawItemName)?.AdditionalUnarmedStrike;
                                    Item? tail = innerSelf.UnarmedStrike.Name == tailItemName ? innerSelf.UnarmedStrike : innerSelf.QEffects.FirstOrDefault(qe => qe.AdditionalUnarmedStrike != null && qe.AdditionalUnarmedStrike.Name == tailItemName)?.AdditionalUnarmedStrike;
                                    if (claw != null && tail != null)
                                    {
                                        if (clawStrikesRemaining > 0) GameLoop.AddDirectUsageOnCreatureOptions(innerSelf.CreateStrike(claw).WithActionCost(0), possibilities, true);
                                        if (tailStrikesRemaining > 0) GameLoop.AddDirectUsageOnCreatureOptions(innerSelf.CreateStrike(tail).WithActionCost(0), possibilities, true);
                                    }

                                    if (possibilities.Count > 0)
                                    {
                                        Option chosenOption;
                                        string additionalTopText = string.Empty;
                                        if (i == 0)
                                        {
                                            possibilities.Add(new CancelOption(true));
                                            additionalTopText = " or right-click to cancel";
                                        }

                                        string clawTopText = (clawStrikesRemaining > 0) ? $"\nRemaing Claw Strikes: {clawStrikesRemaining}" : string.Empty;
                                        string tailTopText = (tailStrikesRemaining > 0) ? $"\nRemaing Tail Strikes: {tailStrikesRemaining}" : string.Empty;

                                        var requestResult = await innerSelf.Battle.SendRequest(new AdvancedRequest(innerSelf, "Choose a creature to Strike.", possibilities)
                                        {
                                            TopBarText = $"Choose a creature to Strike{additionalTopText}. ({i + 1}/3){clawTopText}{tailTopText}",
                                            TopBarIcon = draconicFrenzyIllustration
                                        });
                                        chosenOption = requestResult.ChosenOption;
                                        if (chosenOption is CancelOption)
                                        {
                                            frenzy.RevertRequested = true;
                                            return;
                                        }

                                        if (chosenOption.Text.Contains(clawItemName))
                                        {
                                            clawStrikesRemaining--;
                                        }
                                        else if (chosenOption.Text.Contains(tailItemName))
                                        {
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
                .AddQEffect(new QEffect("Draconic Momentum", "After your Strikes critical hit, you instantly recharge your breath weapon.")
                {
                    AfterYouTakeAction = async (qfAfterAction, action) =>
                    {
                        if (action.CheckResult == CheckResult.CriticalSuccess && action.HasTrait(Trait.Strike))
                        {
                            qfAfterAction.Owner.RemoveAllQEffects(qe => qe.Id == QEffectId.Recharging && (qe.Name?.ToLower().Contains("breath weapon") ?? false));
                        }
                    }
                })
                .AddQEffect(new QEffect("Freezing Blood {icon:Reaction}", "After a creature deals piercing or slashing damage to you, you can as a reaction deal 1d6 cold damage to that creature and make them slowed 1 for 1 round.")
                {
                    AfterYouTakeDamageOfKind = async (qfTakeDamage, action, damageKind) =>
                    {
                        if (action != null && action.Owner.Occupies != null && await qfTakeDamage.Owner.AskToUseReaction($"{action.Owner} spilled your blood. Use Freezing Blood to deal 1d6 cold damage and slow them?"))
                        {
                            await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("1d6", "Freezing Blood"), action.Owner, CheckResult.Success, DamageKind.Cold);
                            action.Owner.AddQEffect(QEffect.Slowed(1).WithExpirationAtEndOfOwnerTurn());
                        }
                    }
                })
                // Consider Shape Ice ??
                .Builder
                .AddNaturalWeapon(NaturalWeaponKind.Jaws, 17, [Trait.Reach, Trait.Cold], "2d8+9", DamageKind.Piercing, wp => wp.WithAdditionalDamage("1d6", DamageKind.Cold))
                .AddNaturalWeapon(NaturalWeaponKind.Claw, 17, [Trait.Reach, Trait.Agile], "2d6+9", DamageKind.Slashing)
                .AddNaturalWeapon(NaturalWeaponKind.Tail, 15, [Trait.Ranged, Trait.DoesNotProvoke], "1d8+8", DamageKind.Bludgeoning, wp => {
                    wp.WithMaximumRange(3);
                    wp.WithRangeIncrement(3);
                    wp.Sfx = SfxName.SwordStrike;
                })
                .AddMainAction((self) =>
                {
                    return new CombatAction(self, IllustrationName.Unknown, "Ground Slam", [], "All creatures without flying in a 10 foot emanation must succeed a DC 24 Reflex save or fall prone and take 2d6 bludgeoning damage. You can then Step.", Target.SelfExcludingEmanation(2))
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.DropProne)
                        .WithGoodness((t, a, d) => 2 * (a.Battle.AllCreatures.Count(creature => a.DistanceTo(creature) <= 2 && !creature.HasEffect(QEffectId.Flying) && !creature.HasEffect(QEffectId.Prone))))
                        .WithSavingThrow(new SavingThrow(Defense.Reflex, 24))
                        .WithEffectOnEachTarget(async (slam, innerSelf, target, result) =>
                        {
                            if (result <= CheckResult.Failure)
                            {
                                target.AddQEffect(QEffect.Prone());
                                await CommonSpellEffects.DealDirectDamage(slam, DiceFormula.FromText("2d6", "Grand Slam"), target, result, DamageKind.Bludgeoning);
                            }
                        })
                        .WithEffectOnSelf(async (innerSelf) =>
                        {
                            await innerSelf.StepAsync("Choose where to step.", false, true);
                        });
                })
                .Done();
        }
    }
}