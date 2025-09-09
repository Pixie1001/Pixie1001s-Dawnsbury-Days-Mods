using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Audio;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Intelligence;
using Microsoft.Xna.Framework;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.Animations.AuraAnimations;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class RalknarTheRude
    {
        public static Creature Create()
        {
            return new Creature(IllustrationName.OrcWarchief256,
                "Ralknar the Rude",
                [Trait.Orc, Trait.Humanoid, Trait.Chaotic, Trait.Evil],
                5, 15, 6,
                new Defenses(21, 15, 12, 9),
                90,
                new Abilities(5, 3, 4, 2, 2, 5),
                new Skills(athletics: 12, intimidation: 13))
            .WithBasicCharacteristics()
            .WithIsNamedMonster()
            .AddQEffect(new QEffect("Angry Soul", "Ralknar's anger builds whenever he is damaged.")
            {
                YouAreDealtDamage = async (QEffect _, Creature _, DamageStuff _, Creature defender) =>
                {
                    var preventUnleashFury = defender.QEffects.FirstOrDefault((effect) => effect.Name == "PreventFury");

                    if (preventUnleashFury != null)
                    {
                        return null;
                    }

                    var anger = defender.QEffects.FirstOrDefault((effect) => effect.Name == "Anger");

                    if (anger == null)
                    {
                        defender.AddQEffect(new("Anger", "Ralknar's anger is at 1.", ExpirationCondition.Never, defender, IllustrationName.Rage)
                        {
                            Value = 1
                        });
                    }
                    else
                    {
                        anger.Value++;
                        anger.Description = $"Ralknar's anger is at {anger.Value}.";
                    }

                    defender.Overhead("Ralknar's anger is building!", Color.Red, "Ralknar's anger is building!");

                    return null;
                }
            })
            .Builder
            .AddMainAction((creature) =>
            {
                return new CombatAction(creature, IllustrationName.Fear, "Battle Cry", [Trait.Auditory, Trait.Emotion, Trait.Fear, Trait.Mental], "You let out a battle cry, shaking your enemies' confidence. Each creature in an 60-foot emanation must attempt a DC 20 Will save. Regardless of the result, creatures are temporarily immune for 1 minute.\n\n{b}Critical Success{/b} The creature is unaffected.\n{b}Success{/b} The creature is frightened 1.\n{b}Failure{/b} The creature is frightened 2.\n{b}Critical Failure{/b} The creature is fleeing for 1 round and frightened 3.", Target.SelfExcludingEmanation(12))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.Intimidate)
                .WithSavingThrow(new(Defense.Will, 19))
                .WithGoodness((_, _, _) => AIConstants.ALWAYS)
                .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                {
                    if (result == CheckResult.Success)
                    {
                        target.AddQEffect(QEffect.Frightened(1).WithSourceAction(action));
                    }
                    else if (result == CheckResult.Failure)
                    {
                        target.AddQEffect(QEffect.Frightened(2).WithSourceAction(action));
                    }
                    else if (result == CheckResult.CriticalFailure)
                    {
                        target.AddQEffect(QEffect.Fleeing(user).WithExpirationAtStartOfSourcesTurn(user, 0));
                        target.AddQEffect(QEffect.Frightened(3).WithSourceAction(action));
                    }
                })
                .WithEffectOnSelf((user) =>
                {
                    user.AddQEffect(new()
                    {
                        PreventTakingAction = (CombatAction action) => action.Name == "Battle Cry" ? "You can only battle cry once each combat." : null
                    });
                });
            })
            .AddMainAction((creature) =>
            {
                return new CombatAction(creature, IllustrationName.Rage, "Unleash Fury", [Trait.Evocation], "You unleash your fury.", Target.SelfExcludingEmanation(6).WithAdditionalRequirementOnCaster((user) => user.HP <= user.MaxHPMinusDrained / 2 ? Usability.Usable : Usability.NotUsable("You must have no more than half your maximum hot points remaining to unleash your fury.")))
                .WithActionCost(3)
                .WithSoundEffect(SfxName.Intimidate)
                .WithSavingThrow(new(Defense.Fortitude, 19))
                .WithGoodness((_, _, _) => AIConstants.ALWAYS)
                .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult result) =>
                {
                    var angerValue = 0;

                    var anger = user.QEffects.FirstOrDefault((effect) => effect.Name == "Anger");

                    if (anger != null)
                    {
                        angerValue = 1 + (anger.Value / 6);
                    }

                    if (angerValue > 4)
                    {
                        angerValue = 4;
                    }

                    await CommonSpellEffects.DealBasicDamage(action, user, target, result, $"{2 * angerValue}d6", DamageKind.Mental);
                })
                .WithEffectOnSelf((user) =>
                {
                    var angerValue = 0;

                    var anger = user.QEffects.FirstOrDefault((effect) => effect.Name == "Anger");

                    if (anger != null)
                    {
                        angerValue = anger.Value / 5;
                    }

                    if (angerValue > 4)
                    {
                        angerValue = 4;
                    }

                    for (var i = 0; i <= angerValue; i++)
                    {
                        user.Battle.SpawnCreature(AngerPhantasm.Create(), user.OwningFaction, user.Occupies);
                    }

                    user.AddQEffect(new()
                    {
                        Name = "PreventFury",
                        PreventTakingAction = (CombatAction action) => action.Name == "Unleash Fury" ? "You can only unleash your fury once each combat." : null,
                    });

                    user.RemoveAllQEffects((effect) => effect.Name == "Anger");

                    user.AddQEffect(new("Dominating Presence", "Conscious enemies that end their turn within 5 feet of you take 2d4 mental damage, with a basic Will save.", ExpirationCondition.Never, user, IllustrationName.Dominate)
                    {
                        StateCheck = (effect) =>
                        {
                            var ralknar = effect.Owner;

                            foreach (Creature creature in ralknar.Battle.AllCreatures)
                            {
                                if (creature.HP > 0 && creature.BaseName != "RalknarTheRude" && creature.IsAdjacentTo(ralknar) && creature.QEffects.FirstOrDefault((qEffect) => qEffect.Name == "In Ralknar's Presence") == null)
                                {
                                    creature.AddQEffect(new("In Ralknar's Presence", "At the end of your turn, you take 2d4 mental damage, with a basic Will save.", ExpirationCondition.Ephemeral, ralknar, IllustrationName.Dominate)
                                    {
                                        EndOfYourTurnDetrimentalEffect = async (QEffect boilingEffect, Creature effectOwner) =>
                                        {
                                            await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("2d4"), effectOwner, CommonSpellEffects.RollSavingThrow(effectOwner, new(ralknar, IllustrationName.Dominate, "Dominating Presence", [Trait.Mental, Trait.Emotion], "", Target.Touch()), Defense.Will, 19), DamageKind.Mental);
                                        }
                                    });
                                }
                            }
                        }
                    });

                    user.AnimationData.AddAuraAnimation(new MagicCircleAuraAnimation(IllustrationName.BaneCircle, Color.Maroon, 0.85f));
                });
            })
            .AddManufacturedWeapon(ItemName.Greataxe, 13, [Trait.Sweep], "2d12+7", null, null)
            .Done();
        }
    }
}
