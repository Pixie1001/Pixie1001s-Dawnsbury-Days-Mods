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
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2 {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowAssassin {
        public static Creature Create() {
            return new Creature(Illustrations.DrowAssassin, "Drow Assassin", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid }, 1, 7, 6, new Defenses(18, 4, 10, 7), 18, new Abilities(-1, 4, 1, 2, 2, 1), new Skills(stealth: 10, acrobatics: 7))
                .WithAIModification(ai => {
                    ai.OverrideDecision = (self, options) => {

                        Creature creature = self.Self;

                        if (creature.HasEffect(QEffectIds.Lurking)) {
                            Creature stalkTarget = creature.Battle.AllCreatures.FirstOrDefault(c => c.QEffects.FirstOrDefault(qf => qf.Id == QEffectIds.Stalked && qf.Source == creature) != null);

                            if (stalkTarget != null) {
                                var path = Pathfinding.GetPath(creature, stalkTarget.Occupies, creature.Battle, new PathfindingDescription() {
                                    Squares = 100,
                                    Style = new MovementStyle() {
                                        ForcedMovement = false,
                                        MaximumSquares = 100,
                                        IgnoresUnevenTerrain = false,
                                        PermitsStep = true,
                                        Shifting = false
                                    }
                                });

                                if (path != null && path.Count > 0 && creature.Speed > 0) {
                                    return options.Where(opt => opt.OptionKind == OptionKind.MoveHere).ToList().ConvertAll<TileOption>(opt => (TileOption)opt).MinBy(opt => opt.Tile.DistanceTo(path[Math.Min(creature.Speed - 1, path.Count - 1)]));

                                }
                            }
                        }

                        return null;
                    };
                })
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .AddQEffect(new QEffect() {
                    Id = QEffectId.SwiftSneak,
                    BonusToInitiative = self => new Bonus(-20, BonusType.Untyped, "Patient Stalker")
                })
                .AddQEffect(new QEffect("Shadowsilk Cloak", "Target can always attempt to sneak or hide, even when unobstructed.") {
                    Id = QEffectId.HideInPlainSight,
                    Innate = true,
                    StartOfCombat = async self => {
                        List<Creature> party = self.Owner.Battle.AllCreatures.Where(c => c.PersistentCharacterSheet != null).ToList();
                        Creature target = party.GetRandom();
                        //Creature target = party.OrderBy(c => c.HP / 100 * c.Defenses.GetBaseValue(Defense.AC) * 5).ToList()[0];

                        self.Owner.AddQEffect(new QEffect {
                            Id = QEffectIds.Lurking,
                            PreventTakingAction = action => action.ActionId != ActionId.Sneak ? "Stalking prey, cannot act." : null,
                            BonusToSkillChecks = (skill, action, target) => {
                                if (skill == Skill.Stealth && action.Name == "Sneak") {
                                    return new Bonus(7, BonusType.Status, "Lurking");
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
                        self.Owner.RemoveAllQEffects(qf => qf.Id == QEffectId.Slowed || qf.Id == QEffectIds.Lurking);
                    }

                })
                .AddQEffect(new QEffect("Nimble Dodge {icon:Reaction}", "{b}Trigger{/b} The drow assassin is hit or critically hit by an attack. {b}Effect{/b} The drow assassin gains a +2 bonus to their Armor Class against the triggering attack.") {
                    YouAreTargetedByARoll = async (self, action, result) => {
                        if ((result.CheckResult == CheckResult.Success || result.CheckResult == CheckResult.CriticalSuccess) && result.ThresholdToDowngrade <= 2) {
                            if (await self.Owner.AskToUseReaction("Use Nimble Dodge to gain a +2 bonus to AC?")) {
                                self.Owner.AddQEffect(new QEffect() {
                                    ExpiresAt = ExpirationCondition.Ephemeral,
                                    BonusToDefenses = (self, action, defence) => defence == Defense.AC ? new Bonus(2, BonusType.Untyped, "Nimble Dodge") : null
                                });
                                return true;
                            }
                        }
                        return false;
                    }
                })
                .AddQEffect(new QEffect("Prey Upon", "Creatures without any allies within 10 feet of them are considered flat-footed against the drow assassin.") {
                    StateCheck = self => {
                        foreach (Creature enemy in self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsPlayer || cr.OwningFaction.IsGaiaFriends)) {
                            int closeAllies = self.Owner.Battle.AllCreatures.Where(cr => cr != enemy && (cr.OwningFaction.IsPlayer || cr.OwningFaction.IsGaiaFriends) && cr.DistanceTo(enemy) <= 2).Count();
                            if (closeAllies == 0) {
                                enemy.AddQEffect(new QEffect() {
                                    Source = self.Owner,
                                    ExpiresAt = ExpirationCondition.Ephemeral,
                                    IsFlatFootedTo = (qfFlatFooted, attacker, action) => attacker == qfFlatFooted.Source ? "prey upon" : null
                                });
                            }
                        }
                    }
                })
                .AddQEffect(new QEffect() {
                    AdditionalGoodness = (self, action, target) => {
                        if (target.QEffects.FirstOrDefault(qf => qf.Id == QEffectIds.Stalked && qf.Source == self.Owner) != null) {
                            return 30f;
                        }
                        return -10f;
                        //return 0f;
                    }
                })
                .AddQEffect(CommonQEffects.Drow())
                .AddQEffect(QEffect.SneakAttack("2d6"))
                .WithBasicCharacteristics()
                .WithProficiency(Trait.Dagger, Proficiency.Expert)
                .AddHeldItem(Items.CreateNew(ItemName.Dagger))
                .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "dagger"));
        }
    }
}