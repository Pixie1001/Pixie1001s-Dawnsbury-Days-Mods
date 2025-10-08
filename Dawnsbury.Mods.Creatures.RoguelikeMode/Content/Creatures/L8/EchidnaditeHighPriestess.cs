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
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.Mechanics.Rules;
using System.Threading.Tasks.Dataflow;
using Dawnsbury.Core.StatBlocks;
using Microsoft.Xna.Framework.Input;
using Dawnsbury.Core.Animations.AnimationTypes;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    public class EchidnaditeHighPriestess {
        public static Creature Create() {
            var numJavelins = 3;;

            Creature monster = new Creature(Illustrations.EHighPriestess, "Echidnadite High Priestess", [Trait.Chaotic, Trait.Evil, Trait.Human, Trait.Humanoid, Trait.Female, ModTraits.SpellcasterMutator],
               level: 8, perception: 16, speed: 5, new Defenses(24, fort: 19, reflex: 16, will: 16), hp: 170,
            new Abilities(5, 5, 6, 1, 7, 4), new Skills(religion: 16, nature: 16, athletics: 13, diplomacy: 12))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (pos, you, step, cr) => you.DistanceTo(cr) <= 6 && cr.FriendOf(you) && !cr.HasTrait(Trait.Celestial) && (cr.HasTrait(Trait.Animal) || cr.HasTrait(Trait.Beast)), 0.2f, false);

                    return null;
                };
            })
            .AddHeldItem(CreateJavelin())
            .WithCreatureId(CreatureIds.EchidnaditeHighPriestess)
            .WithProficiency(Trait.Weapon, Proficiency.Master)
            .WithBasicCharacteristics()
            .AddQEffect(CommonQEffects.BlessedOfEchidna())
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Cleric, Ability.Wisdom, Trait.Divine).WithSpells(
                // level1: [SpellId.TrueStrike, SpellId.TrueStrike, SpellId.TrueStrike],
                //level2: [SpellId.SuddenBolt, SpellId.Bless, SpellId.Heal],
                level3: [SpellId.SuddenBolt, SpellId.Fear],
                level4: [SpellLoader.VomitSwarm, SpellId.SuddenBolt]
                ).Done()
            .AddQEffect(new QEffect() {
                ProvideActionIntoPossibilitySection = (self, section) => {
                    if (section.PossibilitySectionId != PossibilitySectionId.InvisibleActions) return null;
                    // TODO: Hook this up to the transformation
                    return (ActionPossibility) new CombatAction(self.Owner, new SideBySideIllustration(IllustrationName.PickUp, Illustrations.Javelin), "Draw javelin", [Trait.Basic, Trait.Manipulate], "",
                        Target.Self((user, ai) => !user.HasEffect(QEffectIds.MaenadForm) && !user.HeldItems.Any(itm => itm.ItemName == CustomItems.Javelin) ? 15f : 0f)
                        .WithAdditionalRestriction(user => user.HasFreeHand ? user.CarriedItems.Any(itm => itm.ItemName == CustomItems.Javelin) ? null : "out of javelins" : "no free hand"))
                    .WithActionCost(0)
                    .WithSoundEffect(SfxName.PickUpLight)
                    .WithEffectOnSelf(async (action, user) => {
                        var javelin = user.CarriedItems.FirstOrDefault(itm => itm.ItemName == CustomItems.Javelin);
                        if (javelin == null) return;
                        user.CarriedItems.Remove(javelin);
                        user.AddHeldItem(javelin);
                    });
                }
            })
            .AddQEffect(new QEffect() {
                ProvideActionIntoPossibilitySection = (self, section) => {
                    if (section.PossibilitySectionId != PossibilitySectionId.InvisibleActions || !self.Owner.HeldItems.Any(itm => itm.ItemName == CustomItems.Javelin)) return null;
                    var ca = (ActionPossibility) StrikeRules.CreateStrike(self.Owner, self.Owner.HeldItems.FirstOrDefault(itm => itm.ItemName == CustomItems.Javelin)!, RangeKind.Ranged, -1, true)
                    .WithGoodnessAgainstEnemy((targeting, a, d) => {
                        return targeting.OwnerAction?.StrikeModifiers.CalculatedTrueDamageFormula?.ExpectedValue ?? 0f;
                    });
                    ca.CombatAction.Traits.Add(Trait.Basic);
                    return ca;
                }
            })
            .AddQEffect(new QEffect("Javelins", "The Echidnadite High Priestess has this many more javelins in their inventory.", ExpirationCondition.Never, null, Illustrations.Javelin) {
                Value = numJavelins + 1,
                AfterYouTakeAction = async (self, ca) => {
                    var javelins = self.Owner.CarriedItems.Where(itm => itm.ItemName == CustomItems.Javelin).Count();
                    self.Value = javelins;
                    if (javelins <= 0)
                        self.ExpiresAt = ExpirationCondition.Immediately;
                }
            })
            .AddQEffect(new QEffect("Monstrous Form", "When the Echidnadite High Priestess runs out of javelins or is reduced to half health, she takes on a monstrous battle form.") {
                AfterYouTakeDamage = async (self, _, _, _, _) => {
                    if (self.Owner.Damage > self.Owner.MaxHPMinusDrained / 2) {
                        await Transform(self.Owner);
                        self.ExpiresAt = ExpirationCondition.Immediately;
                    }
                },
                AfterYouTakeAction = async (self, ca) => {
                    if (!self.Owner.QEffects.Any(qf => qf.Name == "Javelins") || self.Owner.QEffects.FirstOrDefault(qf => qf.Name == "Javelins")?.Value == 0) {
                        await Transform(self.Owner);
                        self.ExpiresAt = ExpirationCondition.Immediately;
                    }
                }
            })
            .Builder
            //.AddManufacturedWeapon(ItemName.BattleAxe, 18, [Trait.Sweep], "2d8+5")
            .Done()
            ;

            var aura = new MagicCircleAuraAnimation(IllustrationName.AngelicHaloCircleWhite, Color.RosyBrown, 6);
            aura.DecreaseOpacityAsSizeIncreases = true;

            // Monstrous Domain
            var effect = new QEffect("Monstrous Domain", "(aura, divine, transmutation) 30 feet. All allied monsters within this aura gain a +4 bonus to strike damage.") {
                SpawnsAura = self => aura,
            };

            effect.AddGrantingOfTechnical(cr => CommonQEffects.IsMonsterAlly(monster, cr), qfTech => {
                qfTech.AdditionalGoodness = (self, action, target) => action.HasTrait(Trait.Strike) && self.Owner.DistanceTo(monster) < 6 ? 4 : 0f;
            });

            monster.AddQEffect(effect);

            var z = Zone.Spawn(effect, ZoneAttachment.Aura(6));

            z.StateCheckOnEachCreatureInZone = (self, enterer) => {
                if (CommonQEffects.IsMonsterAlly(self.ControllerQEffect.Owner, enterer)) {
                    enterer.AddQEffect(new QEffect("Monstrous Domain", $"You Strikes deal +4 bonus damage.", ExpirationCondition.Ephemeral, null, self.ControllerQEffect.Owner.Illustration) {
                        BonusToDamage = (self, action, target) => action.HasTrait(Trait.Strike) ? new Bonus(4, BonusType.Untyped, "Monstrous Domain") : null,
                    });
                }
            };

            for (int i = 0; i <= numJavelins; i++)
                monster.CarriedItems.Add(CreateJavelin());

            return monster;
        }

        private static Item CreateJavelin() {
            var item = Items.CreateNew(CustomItems.Javelin);
            item.WeaponProperties!.DamageDieCount = 2;
            item.WeaponProperties!.ItemBonus = 1;
            item.WeaponProperties!.WithAdditionalPersistentDamage("1d6", DamageKind.Bleed);
            item.Traits.Add(Trait.HandEphemeral);
            item.Name = "+1 striking wounding javelin";
            return item;
        }

        private static async Task Transform(Creature monster) {
            Sfxs.Play(SfxName.BeastRoar);
            monster.AnimationData.ColorBlink(Color.DarkRed);
            monster.Illustration = IllustrationName.Babau;
            monster.HeldItems.Clear();
            monster.Builder
            .AddNaturalWeapon(NaturalWeaponKind.Claw, 20, [Trait.Sweep], "2d12+8", DamageKind.Slashing)
            .AddNaturalWeapon("tail", Illustrations.SerpentileTail, 20, [Trait.Agile, Trait.Sweep], "2d8+8", DamageKind.Bludgeoning)
            .Done();
            monster.AddQEffect(new QEffect("Monstrous Form", "...", ExpirationCondition.Never, monster, monster.Illustration) {
                Id = QEffectIds.MaenadForm,
                AfterYouDealDamageOfKind = async (attacker, action, kind, defender) => {
                    if (action.HasTrait(Trait.Strike) && kind == DamageKind.Slashing) {
                        foreach (Creature ally in monster.Battle.AllCreatures.Where(cr => CommonQEffects.IsMonsterAlly(monster, cr) && monster.DistanceTo(cr) <= 2 && monster.HasLineOfEffectTo(cr) != CoverKind.Blocked)) {
                            ally.AddQEffect(QEffect.Quickened(ca => true).WithExpirationOneRoundOrRestOfTheEncounter(monster, false));
                        }
                    }
                }
            });

            monster.Battle.Cinematics.EnterCutscene();
            await monster.Battle.Cinematics.WaitABit();
            var db1 = DeepBeast.Create().ApplyWeakAdjustments(false, monster.HasEffect(QEffectId.Weak));
            var db2 = DeepBeast.Create().ApplyWeakAdjustments(false, monster.HasEffect(QEffectId.Weak));

            monster.Battle.SpawnCreature(db1, monster.OwningFaction, monster.Occupies);
            db1.AnimationData.ColorBlinkFast(Color.Tan);

            Sfxs.Play(SfxName.Stoneskin);
            await CommonAnimations.CreateConeAnimation(monster.Battle, db1.Occupies.ToCenterVector(), db1.Occupies.Neighbours.TilesPlusSelf.ToList(), 10, ProjectileKind.Cone, IllustrationName.ElementEarth);

            await monster.Battle.Cinematics.WaitABit();

            monster.Battle.SpawnCreature(db2, monster.OwningFaction, monster.Occupies);
            db2.AnimationData.ColorBlinkFast(Color.Tan);

            Sfxs.Play(SfxName.Stoneskin);
            await CommonAnimations.CreateConeAnimation(monster.Battle, db2.Occupies.ToCenterVector(), db2.Occupies.Neighbours.TilesPlusSelf.ToList(), 10, ProjectileKind.Cone, IllustrationName.ElementEarth);

            monster.Battle.Cinematics.ExitCutscene();
        }
    }

}

