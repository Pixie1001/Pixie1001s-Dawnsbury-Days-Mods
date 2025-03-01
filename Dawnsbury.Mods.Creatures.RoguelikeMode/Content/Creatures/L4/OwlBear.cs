using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using FMOD;
using static Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.BarbarianFeatsDb.AnimalInstinctFeat;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using static System.Net.Mime.MediaTypeNames;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class OwlBear
    {
        private static string BloodcurdlingScreechDescription = "Each creature in an 80-foot emanation must attempt a DC 20 Will save.Regardless of the result, creatures are temporarily immune for 1 minute.\n\n{b}Critical Success{/b} The creature is unaffected.\n{b}Success{/b} The creature is frightened 1.\n{b}Failure{/b} The creature is frightened 2.\n{b}Critical Failure{/b} The creature is fleeing for 1 round and frightened 3.";

        public static Creature Create()
        {
            QEffect GnawEffect = new QEffect();
            GnawEffect.ProvideStrikeModifier = (item) =>
            {
                if (item.Name == NaturalWeapons.GetName(NaturalWeaponKind.Beak))
                {
                    Creature self = GnawEffect.Owner;
                    CombatAction gnawAction = self.CreateStrike(item);
                    gnawAction.Name = "Gnaw";
                    gnawAction.Description = StrikeRules.CreateBasicStrikeDescription3(gnawAction.StrikeModifiers, additionalAttackRollText: "If the Strike hits, the target must attempt a DC 22 Will save.", additionalAftertext: "Depending on the Will saving throw:\n{b}Critical Success{/b} The target is unaffected.\n{b}Success{/b} The target is sickened 1\n{b}Failure{/b} The target is sickened 1 and slowed 1 as long as it remains sickened.");
                    gnawAction.ActionCost = 1;

                    ((CreatureTarget)gnawAction.Target).WithAdditionalConditionOnTargetCreature((a, d) => d.QEffects.Any(qe => (qe.Id == QEffectId.Grappled || qe.Id == QEffectId.Grabbed || qe.Id == QEffectId.Restrained) && qe.Source != null && qe.Source == a) ? Usability.Usable : Usability.NotUsableOnThisCreature("no creature is grabbed"));
                    gnawAction.StrikeModifiers.OnEachTarget = async (a, d, result) =>
                    {
                        if (result >= CheckResult.Success)
                        {
                            CheckResult savingThrow = CommonSpellEffects.RollSavingThrow(d, gnawAction, Defense.Will, 22);
                            if (savingThrow <= CheckResult.Success)
                            {
                                QEffect sickedEffect = QEffect.Sickened(1, 22);
                                if (savingThrow <= CheckResult.Failure)
                                {
                                    QEffect slowedEffect = QEffect.Slowed(1);
                                    sickedEffect.WhenExpires = (qfExpires) => slowedEffect.ExpiresAt = ExpirationCondition.Immediately;
                                    slowedEffect.Description += " (This expires if you are no longer Slowed)";
                                    d.AddQEffect(slowedEffect);
                                }
                                d.AddQEffect(sickedEffect);
                            }
                        }
                    };
                    return gnawAction;
                }

                return null;
            };
            return new Creature(Illustrations.Owlbear, "Owl Bear", [Trait.Animal], 4, 14, 5, new Defenses(21, 13, 7, 11), 70, new Abilities(6, 1, 5, -4, 3, 0), new Skills(acrobatics: 7, athletics: 14, intimidation: 10))
                .WithAIModification(ai =>
                {
                    ai.OverrideDecision = (self, options) =>
                    {
                        Creature monster = self.Self;

                        if (self.Self.QEffects.Any(qf => qf.Key == "temporaryBloodCurdling"))
                        {
                            //AiFuncs.PositionalGoodness(monster, options, (pos, you, step, them) => pos.DistanceTo(them.Occupies) <= 2 && pos.HasLineOfEffectToIgnoreLesser(them.Occupies) <= CoverKind.Standard, 4, false);
                            //return options.MinBy(opt => opt.AiUsefulness.DistanceFromClosestEnemy);

                            Creature target = monster.Battle.AllCreatures.MinBy(c => c.OwningFaction.EnemyFactionOf(monster.OwningFaction) ? c.DistanceTo(monster) : 1000);

                            if (target != null) {
                                var path = Pathfinding.GetPath(monster, target.Occupies, monster.Battle, new PathfindingDescription() {
                                    Squares = 100,
                                    Style = new MovementStyle() {
                                        ForcedMovement = false,
                                        MaximumSquares = 100,
                                        IgnoresUnevenTerrain = false,
                                        PermitsStep = true,
                                        Shifting = false
                                    }
                                });

                                if (path != null && path.Count > 0 && monster.Speed > 0) {
                                    return options.Where(opt => opt.OptionKind == OptionKind.MoveHere).ToList().ConvertAll<TileOption>(opt => (TileOption)opt).MinBy(opt => opt.Tile.DistanceTo(path[Math.Min(monster.Speed - 1, path.Count - 1)]));

                                }
                            }

                        }

                        return null;
                    };
                })
                .WithCharacteristics(false, true)
                .AddQEffect(QEffect.MonsterGrab())
                .AddQEffect(GnawEffect)
                .Builder
                .AddNaturalWeapon(NaturalWeaponKind.Talon, 14, [Trait.Reach, Trait.Agile, Trait.Grab], "1d10+6", DamageKind.Piercing)
                .AddNaturalWeapon(NaturalWeaponKind.Beak, 14, [Trait.Reach], "1d12+6", DamageKind.Piercing)
                .AddMainAction((self) =>
                {
                    return GetBloodcurdlingScreechAction(self);
                })
                .AddMainAction((self) =>
                {
                    return new CombatAction(self, IllustrationName.Demoralize, "Screeching Advance", [Trait.Auditory, Trait.Emotion, Trait.Fear, Trait.Mental], "Stride twice while using Bloodcurdling Screech. (Any creature in the range during the movement will be affected)", Target.Self())
                    .WithActionCost(2)
                    .WithGoodness((t, a, d) => a.PersistentUsedUpResources.UsedUpActions.Contains("Screeching Advance") ? AIConstants.NEVER : AIConstants.ALWAYS)
                    .WithEffectOnSelf(async (action, innerSelf) =>
                    {
                        innerSelf.PersistentUsedUpResources.UsedUpActions.Add("Screeching Advance");
                        QEffect temporaryBloodCurdling = new QEffect()
                        {
                            Key = "temporaryBloodCurdling",
                            StateCheck = async (qfStateCheck) =>
                            {
                                Creature owlBear = qfStateCheck.Owner;
                                foreach (Creature newTarget in owlBear.Battle.AllCreatures.Where(creature => creature != owlBear && !creature.HasEffect(QEffectIds.BloodcurdlingScreechImmunity) && owlBear.DistanceTo(creature.Occupies) <= 16))
                                {
                                    BloodcurdlingScreechAgainstCreature(owlBear, newTarget);
                                }
                            }
                        };

                        
                        innerSelf.AddQEffect(temporaryBloodCurdling);
                        if (await innerSelf.StrideAsync("Choose where to Stride with Screeching Advance. (1/2)", allowCancel: true, allowPass: true))
                        {
                            await innerSelf.StrideAsync("Choose where to Stride with Screeching Advance. (2/2)", allowCancel: false, allowPass: true);
                        }
                        else
                        {
                            action.RevertRequested = true;
                        }

                        temporaryBloodCurdling.ExpiresAt = ExpirationCondition.Immediately;
                    });
                })
                .Done();
        }

        private static void BloodcurdlingScreechAgainstCreature(Creature self, Creature creature)
        {
            if (!creature.HasEffect(QEffectIds.BloodcurdlingScreechImmunity) && !creature.HasTrait(Trait.Indestructible))
            {
                CombatAction bloodcurlingScreech = GetBloodcurdlingScreechAction(self);
                CheckResult result = CommonSpellEffects.RollSavingThrow(creature, bloodcurlingScreech, Defense.Will, 20);

                creature.AddQEffect(new QEffect("Immunity to Bloodcurdling Screech", "You are no longer affected by Bloodcurdling Screen.")
                {
                    Source = self,
                    ExpiresAt = ExpirationCondition.Never,
                    Id = QEffectIds.BloodcurdlingScreechImmunity,
                    Illustration = new SameSizeDualIllustration(Illustrations.StatusBackdrop, IllustrationName.Demoralize),
                    //Value = 10
                });

                if (result <= CheckResult.CriticalSuccess)
                {
                    int frightenedValue = (result <= CheckResult.Failure) ? ((result == CheckResult.CriticalFailure) ? 3 : 2) : 1;
                    creature.AddQEffect(QEffect.Frightened(frightenedValue));
                    if (result == CheckResult.CriticalFailure)
                    {
                        creature.AddQEffect(QEffect.FleeingAllDanger().WithExpirationAtEndOfOwnerTurn());
                    }
                }
            }
        }

        private static CombatAction GetBloodcurdlingScreechAction(Creature self)
        {
            return new CombatAction(self, IllustrationName.Demoralize, "Bloodcurdling Screech", [Trait.Auditory, Trait.Emotion, Trait.Fear, Trait.Mental],
                "Each creature in an 80-foot emanation must attempt a DC 20 Will save. Regardless of the result, creatures are temporarily immune for 1 minute.\n\n{b}Critical Success{/b} The creature is unaffected.\n{b}Success{/b} The creature is frightened 1.\n{b}Failure{/b} The creature is frightened 2.\n{b}Critical Failure{/b} The creature is fleeing for 1 round and frightened 3.", Target.SelfExcludingEmanation(16)) {
                ShortDescription = "The Owlbear forces each enemy within an 80-foot radius to make a DC 20 will save, as per the {i}Fear{/i} spell. They are then immune until the end of the encounter."
            }
                .WithActionCost(1)
                .WithGoodness((t, a, d) => a.Battle.AllCreatures.Any(cr => cr.DistanceTo(a) <= 16 && !a.FriendOf(cr) && !cr.HasEffect(QEffectIds.BloodcurdlingScreechImmunity)) ? AIConstants.EXTREMELY_PREFERRED : AIConstants.NEVER)
                .WithEffectOnEachTarget(async (bloodcurdlingScreech, attacker, defender, result) =>
                {
                    BloodcurdlingScreechAgainstCreature(attacker, defender);
                });
        }
    }
}