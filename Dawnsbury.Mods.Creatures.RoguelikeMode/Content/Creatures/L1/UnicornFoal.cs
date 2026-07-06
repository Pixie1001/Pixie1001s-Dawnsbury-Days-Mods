using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class UnicornFoal {
        public static Creature Create(Encounter? encounter) {
            var encounterLevel = UtilityFunctions.GetEncounterLevel(encounter);
            bool highLevel = encounterLevel > 4;
            var hp = !highLevel ? 10 : 95;
            var defenses = !highLevel ? new Defenses(17, 8, 6, 9) : new Defenses(23, 14, 12, 15);
            var level = !highLevel ? 1 : 5;
            var skills = !highLevel ? new Skills(athletics: 8, acrobatics: 8, stealth: 9) : new Skills(athletics: 14, acrobatics: 14, stealth: 15);
            var abilities = !highLevel ? new Abilities(3, 3, 3, 0, 3, 4) : new Abilities(4, 4, 3, 0, 4, 5);
            var weapon = new Item(IllustrationName.Horn, "horn", new Trait[] { Trait.Melee, Trait.GhostTouch, Trait.Unarmed, Trait.Brawling })
                .WithWeaponProperties(new WeaponProperties($"{(!highLevel ? 1 : 2)}d10", DamageKind.Piercing))
                .WithAdditionalWeaponProperties(wp => {
                    wp.WithAdditionalDamage(!highLevel ? "1d4" : "1d6", DamageKind.Good);
                    wp.ItemBonus = !highLevel ? 0 : 1;
                });
            var perception = !highLevel ? 8 : 11;

            var unicorn = new Creature(Illustrations.Unicorn, "Unicorn, Foal", new List<Trait>() { Trait.Chaotic, Trait.Good, Trait.Beast, Trait.Fey }, level, perception, perception, defenses, hp,
            abilities, skills)
            .WithSpawnAsGaiaFriends()
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    if (self.Self.QEffects.Any(qf => qf.Key == "Powerful Charge")) {
                        foreach (Option opt in options.Where(o => !(o.AiUsefulness.ObjectiveAction != null && o.AiUsefulness.ObjectiveAction.Action.HasTrait(Trait.Strike)))) {
                            opt.AiUsefulness.MainActionUsefulness = 0;
                        }
                    }

                    return null;
                };
            })
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(weapon)
            .AddQEffect(new QEffect("Unicorn Miracle", "The unicorn Foal regains all of its spell slots after each encounter."))
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    return (ActionPossibility)new CombatAction(self.Owner, IllustrationName.Haste, "Powerful Charge", new Trait[] { Trait.Move },
                        $"Stride up to twice your speed in a direct line, then strike. If you moved at least 20-feet, the strike deals +{(!highLevel ? 1 : 2)}d6 damage." +
                        "\n\nThis movement will not path around hazards or attacks of opportunity.",
                        Target.Self((user, ai) => {
                            if (user.HasEffect(QEffectIds.CannotChargeThisTurn)) return 0f;

                            if (!user.Battle.AllCreatures.Any(cr => cr.EnemyOf(user) && cr.Threatens(user.Occupies)) && user.Battle.AllCreatures.Any(cr => cr.EnemyOf(user) && !cr.DetectionStatus.IsUndetectedTo(user) && user.HasLineOfEffectTo(cr.Occupies) <= CoverKind.Lesser && user.DistanceTo(cr) <= user.Speed * (user.HasEffect(QEffectId.AquaticCombat) ? 0.75f : 1.5f) && user.DistanceTo(cr) > 4)) {
                                return 15f;
                            }
                            return 0f;
                        })) {
                        ShortDescription = "Stride up to twice your speed, then strike. If you travelled at least 20-feet and only in a straight line, the strike deals +1d6 damage."
                    }
                    .WithActionCost(2)
                    .WithSoundEffect(SfxName.Footsteps)
                    .WithEffectOnSelf(async (action, self) => {
                        self.AddQEffect(new QEffect() {
                            Key = "Powerful Charge",
                            AdditionalGoodness = (self, action, d) => d.OwningFaction.EnemyFactionOf(self.Owner.OwningFaction) ? 100f : 0f
                        });

                        MovementStyle movementStyle = new MovementStyle() {
                            MaximumSquares = self.Speed * 2,
                            ShortestPath = false,
                            PermitsStep = false,
                            IgnoresUnevenTerrain = false,
                        };

                        Tile startingTile = self.Occupies;
                        Tile? destTile = await UtilityFunctions.GetChargeTiles(self, movementStyle, 4, "Choose where to Stride with Powerful Charge or right-click to cancel", IllustrationName.Haste);

                        if (destTile == null) {
                            action.RevertRequested = true;
                            self.AddQEffect(new QEffect() { Id = QEffectIds.CannotChargeThisTurn }.WithExpirationAtEndOfThisTurn());
                        } else {
                            movementStyle.Shifting = self.HasEffect(QEffectId.Mobility) && destTile.InIteration.RequiresProvokingAttackOfOpportunity;
                            await self.MoveTo(destTile, action, movementStyle);
                            QEffect? chargeBonus = null;
                            if (self.DistanceTo(startingTile) >= 4) {
                                self.AddQEffect(chargeBonus = new QEffect("Charge Bonus", $"+{(!highLevel ? 1 : 2)}d6 damage on your next strike action.") {
                                    AddExtraStrikeDamage = (action, user) => {
                                        return (DiceFormula.FromText($"{(!highLevel ? 1 : 2)}d6", "Powerful Charge"), DamageKind.Piercing);
                                    },
                                    Illustration = IllustrationName.Horn,
                                });
                            }

                            self.RemoveAllQEffects(qf => qf.Key == "Powerful Charge");

                            await CommonCombatActions.StrikeAdjacentCreature(self);
                            if (chargeBonus != null) {
                                chargeBonus.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        }
                    });
                },
            });

            if (!highLevel) {
                unicorn.AddSpellcastingSource(SpellcastingKind.Innate, Trait.Fey, Ability.Wisdom, Trait.Primal).WithSpells(
                level1: new SpellId[] { SpellId.Heal }, level3: new SpellId[] { SpellId.NeutralizePoison }).Done();
            } else {
                unicorn.AddSpellcastingSource(SpellcastingKind.Innate, Trait.Fey, Ability.Wisdom, Trait.Primal).WithSpells(
                level3: new SpellId[] { SpellId.NeutralizePoison, SpellId.Heal }).Done();
            }
            return unicorn;
        }
    }
}