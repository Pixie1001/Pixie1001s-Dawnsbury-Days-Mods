using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
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
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.Mechanics.Rules;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    public class DrowChampion {
        public static Creature Create() {

            Creature monster = new Creature(Illustrations.DrowChampion, "Drow Champion", [Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid, Trait.Female, ModTraits.Drow, ModTraits.MeleeMutator],
               level: 8, perception: 16, speed: 4, new Defenses(27, fort: 19, reflex: 16, will: 16), hp: 150,
            new Abilities(6, 3, 4, 2, 3, 4), new Skills(religion: 16, athletics: 19, intimidation: 16))
            .WithAIModification(ai => {
                //ai.OverrideDecision = (self, options) => {
                //    Creature monster = self.Self;

                //    AiFuncs.PositionalGoodness(monster, options, (pos, you, step, cr) => you.DistanceTo(cr) <= 6 && cr.FriendOf(you) && !cr.HasTrait(Trait.Celestial) && (cr.HasTrait(Trait.Animal) || cr.HasTrait(Trait.Beast)), 0.2f, false);

                //    return null;
                //};
            })
            .WithCreatureId(CreatureIds.DrowChampion)
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(CommonQEffects.DrowClergy())
            .AddQEffect(QEffect.AttackOfOpportunity())
            .AddQEffect(new QEffect("Critical Specialisation (Flail)", "") {
                YouHaveCriticalSpecialization = (self, weapon, combatAction, defender) => weapon.HasTrait(Trait.Flail)
            })
            .WithProficiency(Trait.Weapon, Proficiency.Master)
            .WithBasicCharacteristics()
            //.AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Cleric, Ability.Wisdom, Trait.Divine).WithSpells(
            //    // level1: [SpellId.TrueStrike, SpellId.TrueStrike, SpellId.TrueStrike],
            //    //level2: [SpellId.SuddenBolt, SpellId.Bless, SpellId.Heal],
            //    level3: [SpellId.SuddenBolt, SpellId.Fear],
            //    level4: [SpellLoader.VomitSwarm, SpellId.SuddenBolt]
            //    ).Done()
            .AddQEffect(new QEffect("Contemptious Retaliation {icon:Reaction}", "{b}Trigger{/b} An enemy within 5 feet attacks you. {b}Effect{/b} You may make a strike against the attacker.") {
                YouAreTargeted = async (self, action) => {
                    if (action.Owner?.Occupies == null || !action.HasTrait(Trait.Attack) || action.Owner.DistanceTo(self.Owner) > 1) {
                        return;
                    }

                    CombatAction strike = self.Owner.CreateStrike(self.Owner.PrimaryWeapon!, 0).WithActionCost(0);
                    strike.ChosenTargets = ChosenTargets.CreateSingleTarget(action.Owner);

                    int map = self.Owner.Actions.AttackedThisManyTimesThisTurn;

                    if ((bool)strike.CanBeginToUse(self.Owner) && (strike.Target as CreatureTarget)!.IsLegalTarget(self.Owner, action.Owner).CanBeUsed && await self.Owner.AskToUseReaction($"{action.Owner.Name} is attempting to attack you in melee. Would you like to retaliate?")) {
                        if (strike.CanBeginToUse(action.Owner)) {
                            await strike.AllExecute();
                            self.Owner.Actions.AttackedThisManyTimesThisTurn = map;
                        }

                    }
                }
            })
            .AddQEffect(new QEffect("Demonic Aegis", "While raising its shield, this creature gains a +2 circumstance bonus against ranged attacks and reflex saves.") {
                BonusToDefenses = (self, action, def) => self.Owner.HasEffect(QEffectId.RaisingAShield) && action != null && ((action.HasTrait(Trait.Attack) && action.HasTrait(Trait.Ranged) && def == Defense.AC) || def == Defense.Reflex) ? new Bonus(2, BonusType.Circumstance, "Demonic aegis", true) : null
            })
            .Builder
            .AddManufacturedWeapon(ItemName.Flail, 19, [Trait.Evil, Trait.Magical, Trait.Chaotic], "3d6+6", wp => {
                wp.AdditionalDamage.Add(("1d6", DamageKind.Negative));
            })
            .AddMainAction(you => {
                return new CombatAction(you, IllustrationName.Bane, "Dark Chant", [Trait.Divine, Trait.Concentrate, Trait.Flourish], "...", Target.Self((user, ai) => user.GetQEffectValue(QEffectIds.DemonicPower) < 6 ? 20f : int.MinValue))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.Necromancy)
                .WithEffectOnSelf(async (action, caster) => {
                    var stacks = caster.GetQEffectValue(QEffectIds.DemonicPower);

                    if (stacks > 0)
                        caster.FindQEffect(QEffectIds.DemonicPower)!.Value += 1;
                    else
                        caster.AddQEffect(new QEffect("Demonic Power", caster.Name + " is channeling the demon power of the Demon Queen of Spiders, growing stronger the longer she chants." +
                            "\n • 2. +1 status bonus to attack." +
                            "\n • 3. Gains a 5-foot demonic aura." +
                            "\n • 4. +2 status bonus all saves." +
                            "\n • 5. +5-feet to demonic aura size." +
                            "\n • 6. +10 status bonus to speed and +4 status bonus to strike damage.", ExpirationCondition.Never, you, you.Illustration) {
                            Id = QEffectIds.DemonicPower,
                            Value = 1,
                            BonusToAllSpeeds = (self) => self.Value >= 6 ? new Bonus(2, BonusType.Status, "Demonic power", true) : null,
                            BonusToDefenses = (self, action, def) => self.Value >= 4 && def != Defense.AC ? new Bonus(2, BonusType.Status, "Demonic power", true) : null,
                            BonusToAttackRolls = (self, action, target) => self.Value >= 2 ? new Bonus(1, BonusType.Status, "Demonic power", true) : null,
                            BonusToDamage = (self, action, target) => action.HasTrait(Trait.Strike) && self.Value >= 6 ? new Bonus(4, BonusType.Status, "Demonic power", true) : null,
                            StateCheck = self => {
                                if (self.Value < 3) return;

                                var effect = new QEffect("Demonic Aura",
                                    $"(aura, olfactory) {(self.Value >= 5 ? 10 : 5)} feet. Enemy creatures that end their turn within the aura suffer 2d8 negative damage, mitigated by a basic DC {23 - 8 + caster.Level} Will save.",
                                    ExpirationCondition.Ephemeral, caster, IllustrationName.Bane);
                                self.Owner.AddQEffect(effect);

                                var z = Zone.Spawn(effect, ZoneAttachment.Aura(self.Value >= 5 ? 2 : 1));
                                z.AfterCreatureEndsItsTurnHere = async cr => {
                                    if (cr.FriendOf(caster)) return;

                                    var ca = CombatAction.CreateSimple(caster, "Demonic Aura", Trait.Evil, Trait.Divine, Trait.Demon);
                                    var result = CommonSpellEffects.RollSavingThrow(cr, ca, Defense.Fortitude, 23 - 8 + caster.Level);
                                    await CommonSpellEffects.DealBasicDamage(ca, caster, cr, result, "2d8", DamageKind.Negative);
                                };
                            }
                        });

                    if (stacks == 2) {
                        caster.AnimationData.AddAuraAnimation(new MagicCircleAuraAnimation(Illustrations.BaneCircleWhite, Color.Purple, 1));
                    } else if (stacks == 4) {
                        caster.AnimationData.AuraAnimations.FirstOrDefault(aura => aura.Color == Color.Purple)?.MoveTo(2);
                    }


                })
                ;
            })
            .Done()
            .AddHeldItem(Items.CreateNew(ItemName.SteelShield))
            ;

            return monster;
        }
    }

}

