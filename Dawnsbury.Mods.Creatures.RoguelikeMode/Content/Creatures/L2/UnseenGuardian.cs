using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class UnseenGuardian {
        public static Creature Create() {
            return new Creature(Illustrations.UnseenGuardian, "Unseen Guardian", new List<Trait>() { Trait.Lawful, Trait.Elemental, Trait.Air }, 2, 6, 8, new Defenses(16, 5, 11, 7), 30, new Abilities(2, 4, 3, 1, 3, 1), new Skills(stealth: 2))
                .WithAIModification(ai => {
                    ai.IsDemonHorizonwalker = true;
                    ai.OverrideDecision = (self, options) => {
                        Creature creature = self.Self;

                        if (creature.HasEffect(QEffectIds.Lurking)) {
                            return options.Where(opt => opt.OptionKind == OptionKind.MoveHere && opt.Text == "Sneak" && opt is TileOption).ToList().GetRandom();
                        }

                        QEffectId[] rootEffects = new QEffectId[] { QEffectId.Grabbed, QEffectId.Grappled, QEffectId.Restrained, QEffectId.Immobilized };

                        return creature.Actions.ActionsLeft == 1 && !creature.QEffects.Any(qf => rootEffects.Contains(qf.Id)) && creature.Battle.AllCreatures.All(enemy => !enemy.EnemyOf(creature) || creature.DetectionStatus.EnemiesYouAreHiddenFrom.Contains(enemy)) && !creature.DetectionStatus.Undetected ? options.Where(opt => opt.OptionKind == OptionKind.MoveHere && opt.Text == "Sneak" && opt is TileOption).ToList().GetRandom() : null;
                    };
                })
                .WithProficiency(Trait.Weapon, Proficiency.Trained)
                .AddQEffect(new QEffect() {
                    BonusToInitiative = self => new Bonus(-30, BonusType.Untyped, "Patient Guardian")
                })
                .AddQEffect(new QEffect("Obliviating Aura", "The unseen guardian feels slippery and elusive in its victim's minds, making it easy for them to lose track of its postion. It gains a +20 bonus to checks made to sneak or hide and can hide in plain sight.") {
                    Id = QEffectId.HideInPlainSight,
                    Innate = true,
                    Illustration = IllustrationName.Blur,
                    BonusToSkillChecks = (skill, action, target) => {
                        if (action.Name == "Sneak" || action.Name == "Hide") {
                            return new Bonus(20, BonusType.Status, "Indistinct Form");
                        }
                        return null;
                    },
                    StartOfCombat = async self => {
                        List<Creature> party = self.Owner.Battle.AllCreatures.Where(c => c.OwningFaction.IsHumanControlled).ToList();
                        Creature target = party.OrderBy(c => c.HP / 100 * c.Defenses.GetBaseValue(Defense.AC) * 5).ToList()[0];

                        // TODO: Set so that lurking ends after taking their bonus turn
                        self.Owner.AddQEffect(new QEffect {
                            Id = QEffectIds.Lurking,
                            PreventTakingAction = action => action.ActionId != ActionId.Sneak ? "Stalking prey, cannot act." : null,
                            BonusToSkillChecks = (skill, action, target) => {
                                if (skill == Skill.Stealth && action.Name == "Sneak") {
                                    return new Bonus(30, BonusType.Status, "Lurking");
                                }
                                return null;
                            },
                            ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
                        });

                        foreach (Creature player in party) {
                            self.Owner.DetectionStatus.HiddenTo.Add(player);
                        }
                        self.Owner.DetectionStatus.Undetected = true;
                        target.AddQEffect(CommonQEffects.Stalked(self.Owner));

                        self.Owner.AddQEffect(new QEffect() {
                            Id = QEffectId.Slowed,
                            Value = 1
                        });
                        await self.Owner.Battle.GameLoop.Turn(self.Owner, false);
                        self.Owner.RemoveAllQEffects(qf => qf.Id == QEffectId.Slowed);
                    }
                })
                .AddQEffect(new QEffect("Seek Vulnerability", "The Unseen Guardian's obliviating aura quickly falls apart as soon as a creatre's attention begins to settle on it, distrupting the magic. Successful seek attempts against a detected Unseen Guardian instead fully reveal it to all of the seeker's allies.") {
                    Innate = true,
                    AfterYouAreTargeted = async (self, action) => {
                        action.ChosenTargets.CheckResults.TryGetValue(self.Owner, out var result);
                        //if (action.ActionId == ActionId.Seek && result == CheckResult.Success) {
                        //    self.Owner.DetectionStatus.HiddenTo.Remove(action.Owner);
                        //    self.Owner.DetectionStatus.RecalculateIsHiddenToAnEnemy();
                        //} else if (action.ActionId == ActionId.Seek && result == CheckResult.CriticalSuccess) {
                        //    self.Owner.DetectionStatus.HiddenTo.Clear();
                        //    self.Owner.DetectionStatus.RecalculateIsHiddenToAnEnemy();
                        //}
                        if (action.ActionId == ActionId.Seek && result >= CheckResult.Success && !self.Owner.DetectionStatus.EnemiesYouAreHiddenFrom.Contains(action.Owner)) {
                            self.Owner.DetectionStatus.HiddenTo.Clear();
                            self.Owner.DetectionStatus.RecalculateIsHiddenToAnEnemy();
                        }
                    }
                })
                .AddQEffect(new QEffect() {
                    Innate = false,
                    StateCheck = self => {
                        if (self.Owner.Battle.RoundNumber == 4) {
                            Creature creature = self.Owner;
                            creature.RemoveAllQEffects(qf => qf.Name == "Obliviating Aura" || qf.Name == "Seek Vulnerability");
                            creature.DetectionStatus.HiddenTo.Clear();
                            creature.DetectionStatus.Undetected = false;
                            creature.DetectionStatus.RecalculateIsHiddenToAnEnemy();
                            Sfxs.Play(SfxName.BeastRoar);
                            creature.AnimationData.ColorBlink(Color.Red);
                            creature.AI.IsDemonHorizonwalker = false;
                            creature.AI.OverrideDecision = null;

                            creature.AddQEffect(new QEffect("Enraged",
                                "The Unseen Guardian's powers of concealment have began to wane after prolonged scruniny, driving them to instead brutalise its enemies to maintain the secret of its existence.",
                                ExpirationCondition.Never,
                                creature, IllustrationName.Rage) {
                                BonusToAttackRolls = (qfAtk, action, _) => action.HasTrait(Trait.Strike) ? new Bonus(2, BonusType.Status, "Enraged", true) : null,
                                BonusToDefenses = (qfDef, action, defence) => defence == Defense.AC ? new Bonus(2, BonusType.Status, "Enraged", true) : null,
                                BonusToDamage = (qfDmg, action, _) => action.HasTrait(Trait.Strike) ? new Bonus(2, BonusType.Status, "Enraged", true) : null
                            });
                            self.ExpiresAt = ExpirationCondition.Immediately;
                        }
                    }
                })
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Bleed))
                .AddQEffect(QEffect.DamageImmunity(DamageKind.Poison))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Paralyzed))
                .AddQEffect(QEffect.Flying())
                .AddQEffect(QEffect.SneakAttack("1d8"))
                .WithUnarmedStrike(CommonItems.CreateNaturalWeapon(IllustrationName.Fist, "Fists", "2d4", DamageKind.Bludgeoning, new Trait[] { Trait.Unarmed, Trait.Magical, Trait.Finesse, Trait.Melee, Trait.Agile }))
                .WithAdditionalUnarmedStrike(new Item(IllustrationName.FourWinds, "Slicing Wind", new Trait[] { Trait.Ranged, Trait.Air, Trait.Magical }).WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Slashing) {
                    VfxStyle = new VfxStyle(5, ProjectileKind.Cone, IllustrationName.FourWinds),
                    Sfx = SfxName.AeroBlade
                }.WithRangeIncrement(4)));
        }
    }
}
