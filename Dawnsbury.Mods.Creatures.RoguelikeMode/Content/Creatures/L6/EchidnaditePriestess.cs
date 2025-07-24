using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Animations.AuraAnimations;
using Dawnsbury.Core.Mechanics.Zoning;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    public class EchidnaditePriestess {
        public static Creature Create() {
            Creature monster = new Creature(Illustrations.EHighPriestess, "Echidnadite Priestess", [Trait.Chaotic, Trait.Evil, Trait.Human, Trait.Humanoid, ModTraits.SpellcasterMutator],
               level: 6, perception: 14, speed: 5, new Defenses(23, fort: 11, reflex: 14, will: 17), hp: 60,
            new Abilities(5, 2, 4, 0, 7, 3), new Skills(religion: 16, nature: 16, athletics: 13, diplomacy: 12))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (pos, you, step, cr) => you.DistanceTo(cr) <= 3 && cr.FriendOf(you) && !cr.HasTrait(Trait.Celestial) && (cr.HasTrait(Trait.Animal) || cr.HasTrait(Trait.Beast)), 0.5f, false);

                    return null;
                };
            })
            .WithCreatureId(CreatureIds.EchidnaditePriestess)
            .WithProficiency(Trait.Weapon, Proficiency.Master)
            .WithBasicCharacteristics()
            .AddQEffect(CommonQEffects.BlessedOfEchidna())
            .AddQEffect(CommonQEffects.SerpentVenomAttack(18, "serpent familiar"))
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Cleric, Ability.Wisdom, Trait.Divine).WithSpells(
                level1: [SpellId.Guidance, SpellId.Harm, SpellId.Fear, SpellId.Harm],
                level2: [SpellId.Heal, SpellId.Heal, SpellLoader.VomitSwarm],
                level3: [SpellLoader.VomitSwarm, SpellLoader.VomitSwarm, SpellId.Heal]
                //level4: [SpellLoader.SummonMonster]
                ).Done()
            .Builder
            .AddNaturalWeapon("serpent familiar", IllustrationName.Constrict, 15, [Trait.Reach, Trait.AddsInjuryPoison], "2d4+5", DamageKind.Piercing, wp => wp.WithAdditionalDamage("1d6", DamageKind.Poison))
            .AddMainAction(you => new CombatAction(you, new SideBySideIllustration(IllustrationName.Bless, IllustrationName.Jaws), "Monstrous Gift", [Trait.Flourish, Trait.Divine, Trait.Transmutation], "Target echidnadite cultist within 30 feet gains a random monstrous gift.",
            Target.RangedFriend(6)
                .WithAdditionalConditionOnTargetCreature((a, d) => a == d ? Usability.NotUsableOnThisCreature("cannot-target-self") : Usability.Usable)
                .WithAdditionalConditionOnTargetCreature((a, d) => !d.HasEffect(QEffectIds.BlessedOfEchidna) ? Usability.NotUsableOnThisCreature("not-a-humanoid") : Usability.Usable))
            .WithGoodness((_, a, d) => AIConstants.ALWAYS)
            .WithActionCost(0)
            .WithSoundEffect(SfxName.ScratchFlesh)
            .WithProjectileCone(IllustrationName.Bless, 5, ProjectileKind.Cone)
            .WithEffectOnEachTarget(async (action, user, target, _) => {
                var mutation = new QEffect().WithExpirationEphemeral();

                var effect = new QEffect("Monstrous Gift ", "You count as a monster and ", ExpirationCondition.ExpiresAtStartOfSourcesTurn, user, IllustrationName.MagicHide) {
                    Id = QEffectIds.MonstrousGift
                };

                if (!target.HasTrait(ModTraits.Monstrous)) {
                    target.Traits.Add(ModTraits.Monstrous);
                    effect.WhenExpires = self => self.Owner.Traits.Remove(ModTraits.Monstrous);
                }

                var rand = 0;
                // 0 - 3

                // Denotes creatures that cannot use new attacks
                if (target.CreatureId == CreatureIds.EchidnaditeWombCultist)
                    rand = R.Next(2); 
                else
                    rand = R.Next(4);
                switch (rand) {
                    case 0:
                        effect.Name += "(Monstrous Hide)";
                        effect.Description += "gain a +2 bonus to AC.";
                        effect.Illustration = IllustrationName.MagicHide;
                        effect.BonusToDefenses = (self, action, defence) => defence == Defense.AC ? new Bonus(2, BonusType.Untyped, "Monstrous Hide") : null;
                        break;
                    case 1:
                        target.Actions.AttackedThisManyTimesThisTurn = 0;
                        effect.EndOfYourTurnBeneficialEffect = async (self, you) => { you.Actions.AttackedThisManyTimesThisTurn = 0; };
                        effect.Name += "(Striking Tail)";
                        effect.Description += "you may make a striking tail strike attack as a free action {icon:FreeAction} against creatures within reach that attack you.";
                        effect.Illustration = IllustrationName.Tail;
                        effect.AdditionalUnarmedStrike = new Item(IllustrationName.Tail, "striking tail", [Trait.Agile, Trait.Finesse, Trait.BattleformAttack, Trait.DeadlyD8])
                            .WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Piercing));
                        effect.BattleformMinimumStrikeModifier = 9 + user.Level; ;
                        effect.YouAreTargeted = async (self, action) => {
                            if (!action.HasTrait(Trait.Attack) || action.Owner.DistanceTo(self.Owner) > 1) {
                                return;
                            }

                            CombatAction strike = self.Owner.CreateStrike(self.AdditionalUnarmedStrike ?? self.Owner.UnarmedStrike, 0).WithActionCost(0);
                            strike.ChosenTargets = ChosenTargets.CreateSingleTarget(action.Owner);

                            if ((bool)strike.CanBeginToUse(self.Owner) && (strike.Target as CreatureTarget)!.IsLegalTarget(self.Owner, action.Owner)
                                .CanBeUsed && await self.Owner.AskForConfirmation(IllustrationName.Tail, $"{action.Owner.Name} is attempting to attack you in melee. Would you like to retaliate with striking tail as a {{icon:FreeAction}}?", "Yes")) {
                                if (strike.CanBeginToUse(action.Owner)) {
                                    await strike.AllExecute();
                                }

                            }
                        };
                        break;
                    case 2:
                        effect.Name += "(Spidery Appendages)";
                        effect.Description += "gain a powerful spidery appendage melee attack that can grab enemies.";
                        effect.Illustration = new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.DragonClaws);
                        effect.AdditionalGoodness = (self, action, target) => action.Item == self.AdditionalUnarmedStrike ? 5f : 0f;
                        effect.AdditionalUnarmedStrike = new Item(new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.DragonClaws), "spidery appendage", [Trait.Grab, Trait.Agile, Trait.Finesse, Trait.DeadlyD6, Trait.BattleformAttack])
                            .WithWeaponProperties(new WeaponProperties("2d10", DamageKind.Piercing));
                        effect.BattleformMinimumStrikeModifier = 9 + user.Level; ;
                        effect.StateCheck = self => {
                            self.Owner.AddQEffect(QEffect.MonsterGrab(true).WithExpirationEphemeral());
                        };
                        break;
                    case 3:
                        effect.Name += "(Quills)";
                        effect.Description += "gain a ranged quills attacks.";
                        effect.Illustration = Illustrations.Quills;
                        effect.BattleformMinimumStrikeModifier = 9 + user.Level;
                        effect.AdditionalGoodness = (self, action, target) => action.Item == self.AdditionalUnarmedStrike ? 12f : 0f;
                        effect.AdditionalUnarmedStrike = new Item(Illustrations.Quills, "quills", [Trait.VersatileS, Trait.BattleformAttack])
                            .WithWeaponProperties(new WeaponProperties("2d8", DamageKind.Piercing)
                            .WithRangeIncrement(6)
                            .WithAdditionalPersistentDamage("1d10", DamageKind.Bleed));
                        break;
                }

                target.AddQEffect(effect);
            })
            )
            .Done()
            ;

            var aura = new MagicCircleAuraAnimation(IllustrationName.AngelicHaloCircleWhite, Color.RosyBrown, 3);
            aura.DecreaseOpacityAsSizeIncreases = true;

            // Monstrous Domain
            var effect = new QEffect("Monstrous Domain", "(aura, divine, transmutation) 15 feet. All allied monsters within this aura gain a +4 bonus to strike damage.") {
                SpawnsAura = self => aura,
            };

            effect.AddGrantingOfTechnical(cr => CommonQEffects.IsMonsterAlly(monster, cr), qfTech => {
                qfTech.AdditionalGoodness = (self, action, target) => action.HasTrait(Trait.Strike) && self.Owner.DistanceTo(monster) < 3 ? 4 : 0f;
            });

            monster.AddQEffect(effect);

            var z = Zone.Spawn(effect, ZoneAttachment.Aura(3));

            z.StateCheckOnEachCreatureInZone = (self, enterer) => {
                if (CommonQEffects.IsMonsterAlly(self.ControllerQEffect.Owner, enterer)) {
                    enterer.AddQEffect(new QEffect("Monstrous Domain", $"You Strikes deal +4 bonus damage.", ExpirationCondition.Ephemeral, null, self.ControllerQEffect.Owner.Illustration) {
                        BonusToDamage = (self, action, target) => action.HasTrait(Trait.Strike) ? new Bonus(4, BonusType.Untyped, "Monstrous Domain") : null,
                    });
                }
            };

            return monster;
        }
    }
}

