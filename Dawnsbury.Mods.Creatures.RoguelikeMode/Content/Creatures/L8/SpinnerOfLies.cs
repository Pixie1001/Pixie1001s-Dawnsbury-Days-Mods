using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
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
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Notifications;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    public class SpinnerOfLies {
        private static int NumDuplicates = 7;

        public static Creature Create() {
            Item legAtk = new Item(new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.DragonClaws), "stabbing appendage", new Trait[] { Trait.Unarmed, Trait.Finesse, Trait.Agile, Trait.DeadlyD8 }).WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Piercing));

            Creature monster = new Creature(new SpiderIllustration(Illustrations.SpinnerOfLies, Illustrations.Bear4), "Spinner of Lies",
                [Trait.Chaotic, Trait.Evil, Trait.Demon, Trait.Fiend, ModTraits.Spider, ModTraits.MeleeMutator, Trait.NonSummonable, Trait.Female],
                level: 8, perception: 16, speed: 8, new Defenses(24, 12, 18, 15), 115,
            new Abilities(3, 6, 2, 6, 2, 6),
            new Skills(acrobatics: 18, stealth: 18, deception: 18, occultism: 16, religion: 12, diplomacy: 16))
            .WithCreatureId(CreatureIds.SpinnerOfLies)
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(legAtk)
            .WithSpellProficiencyBasedOnSpellAttack(16, Ability.Intelligence)
            .AddSpellcastingSource(SpellcastingKind.Innate, Trait.Bard, Ability.Intelligence, Trait.Occult).Done()
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Good, 5))
            .AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 5))
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            .AddQEffect(QEffect.WebSense())
            .AddQEffect(new QEffect("Duplicate", $"At the start of combat, the Spinner of Lies is hidden within a cadre of identical duplicates of herself, as per her {{i}}duplicate{{/i}} ability.") {
                StartOfCombat = async self => {
                    Duplicate(self.Owner);
                }
            })
            .Builder
            .AddMainAction(you => {
                return new CombatAction(you, IllustrationName.MirrorImage, "Duplicate", [Trait.Illusion, Trait.Flourish],
                    $"The Spinner of Lies splits into {NumDuplicates} copies of herself, cleansing herself of all condtions but gaining slowed 1. Only one is real, the rest will be revealed as harmless illusions upon taking damage or after the real demon is damaged. This ability can only be used once all of the Spinner of Lie's duplicates have been dispelled.",
                    Target.Self((caster, ai) => 100f).WithAdditionalRestriction(caster => caster.QEffects.Any(qf => qf.Key == "SpinnerOfLiesDuplicate") ? "Duplicates still remain" : null))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.Fear)
                .WithEffectOnSelf(async caster => {
                    Duplicate(caster);
                });
            })
            .Done();
            ApplyClass(monster, true);
            return monster;
        }

        private static void ApplyClass(Creature duplicate, bool guaranteeMelee=false) {
            var test = duplicate.Spellcasting?.PrimarySpellcastingSource.GetSpellSaveDC();

            duplicate.RemoveAllQEffects(qf => qf.Key == "SpinnerOfLiesLoadout");

            var rand = guaranteeMelee ? 0 : R.Next(0, 4);
            if (rand == 0 || rand == 1) {
                duplicate.Illustration = new SpiderIllustration(Illustrations.SpinnerOfLies, Illustrations.Bear4);
                duplicate.AddQEffect(new QEffect("Melee", "This duplicate is equipped for melee, dealing additional damage when aided by adjacent or flanking allies.", ExpirationCondition.Never, null, IllustrationName.Greataxe) {
                    Key = "SpinnerOfLiesLoadout",
                    StateCheck = self => {
                        self.Owner.AddQEffect(QEffect.PackAttack("Spinner Of Lies", "1d8").WithExpirationEphemeral());
                        self.Owner.AddQEffect(QEffect.SneakAttack("1d6").WithExpirationEphemeral());
                    }
                });
            } else if (rand == 2) {
                duplicate.Illustration = new SpiderIllustration(Illustrations.SpinnerOfLiesRanged, Illustrations.Bear4Ranged);
                duplicate.AddQEffect(new QEffect("Ranged", "This duplicate is equipped for ranged skirmishing, armed with harrying javelins that leave gushing wounds.", ExpirationCondition.Never, null, Illustrations.Javelin) {
                    Key = "SpinnerOfLiesLoadout",
                    StateCheck = self => {
                        self.Owner.ReplacementUnarmedStrike = new Item(Illustrations.Javelin, "Throw (javelin)", Trait.Knife, Trait.Ranged)
                        .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)
                        .WithRangeIncrement(6)
                        .WithAdditionalPersistentDamage("1d12", DamageKind.Bleed));
                    }
                });
            } else if (rand == 3) {
                duplicate.Illustration = new SpiderIllustration(Illustrations.SpinnerOfLiesCaster, Illustrations.Bear4Wizard);
                duplicate.AddQEffect(new QEffect("Caster", "This duplicate is equipped with a sorcerous staff, allowing it to unleash a pulse of nightmare magic around itself.", ExpirationCondition.Never, null, Illustrations.SceptreOfTheSpider) {
                    Key = "SpinnerOfLiesLoadout",
                    ProvideMainAction = self => {
                        return (ActionPossibility)new CombatAction(self.Owner, Illustrations.SceptreOfTheSpider, "Nightmare Pulse", [ Trait.Spell, Trait.SpecialAbility, Trait.Enchantment, Trait.Fear, Trait.Mental ],
                            "All enemies within a 15 foot emanation must make a Will save against the Spinner of Lies' spell DC. On a failure, they gain frightened 2. " +
                            "If they're already frightened, they instead suffer 1d12 mental damage per level of frightened. On a critical failure, they gain frightened 3 or suffer double damage instead.", Target.EnemiesOnlyEmanation(3))
                        .WithActionCost(2)
                        .WithGoodnessAgainstEnemy((_, a, d) => a.HasEffect(QEffectId.Frightened) ? a.GetQEffectValue(QEffectId.Frightened) * 5.5f : a.AI.Fear(d) / 2)
                        .WithSavingThrow(new SavingThrow(Defense.Will, self.Owner.Spellcasting?.PrimarySpellcastingSource.GetSpellSaveDC() ?? 0))
                        .WithSoundEffect(SfxName.Fear)
                        .WithProjectileCone(IllustrationName.Fear, 15, ProjectileKind.Cone)
                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                            if (result < CheckResult.Success) {
                                if (target.HasEffect(QEffectId.Frightened))
                                    await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, target.GetQEffectValue(QEffectId.Frightened) + "d12", DamageKind.Mental);
                                else
                                    target.AddQEffect(QEffect.Frightened(3 - (int)result).WithSourceAction(spell));
                            }
                        });
                    }
                });
            }
        }

        private static void Duplicate(Creature monster) {
            monster.RemoveAllQEffects(qf => qf.Illustration != null && qf.Key != "SpinnerOfLiesDuplicate");

            var duplicates = new List<Creature>() { monster };

            var freeTiles = monster.Battle.Map.AllTiles.Where(t => t.IsTrulyGenuinelyFreeToEveryCreature && t.DistanceTo(monster.Occupies) <= 6 && t.HasLineOfEffectToIgnoreLesser(monster.Occupies) != CoverKind.Blocked).ToList();
            for (int i = 0; i < NumDuplicates - 1; i++) {
                Tile tile = monster.Occupies;
                if (tile == null) return;
                if (freeTiles.Count > 0) {
                    tile = UtilityFunctions.ChooseAtRandom(freeTiles.ToArray());
                    freeTiles.Remove(tile!);
                }
                var duplicate = Create();
                ApplyClass(duplicate);
                CopiableCharacteristics.CopyFromTo(monster, duplicate);
                duplicate.TakeDamage(monster.Damage);
                duplicates.Add(duplicate);
                monster.Battle.SpawnCreature(duplicate, monster.OwningFaction, tile!);
            }

            var origional = UtilityFunctions.ChooseAtRandom(duplicates.ToArray());
            if (origional == null) return;
            foreach (Creature duplicate in duplicates.ToList()) {
                var effect = new QEffect("Duplicate",
                    "The Spinner of Lies is hiding within these duplicates. The duplicates are destroyed by upon taking damage. " +
                    "The real demon will instead reveal all other duplicates as fake when damaged. While maintaining these illusions, the Spinner of Lies and her copies are slowed 1.", ExpirationCondition.Never, null, origional.Illustration) {
                    Key = "SpinnerOfLiesDuplicate",
                    StateCheck = self => {
                        self.Owner.AddQEffect(QEffect.Slowed(1).WithExpirationEphemeral());
                    }
                };
                if (duplicate == origional) {
                    effect.AfterYouTakeDamage = async (self, amount, kind, action, crit) => {
                        if (amount > 0) {
                            foreach (Creature d in duplicates.Where(cr => cr != self.Owner)) {
                                DuplicateDeath(d);
                            }
                        }
                    };

                    Action<QEffect>? lambda = self => {
                        if (!duplicates.Any(cr => !cr.Destroyed && cr != self.Owner)) {
                            self.ExpiresAt = ExpirationCondition.Immediately;
                        }
                    };
                    effect.StateCheck += lambda;
                }

                if (duplicate != origional) {
                    effect.AfterYouTakeDamage = async (self, amount, kind, action, crit) => {
                        if (amount > 0) {
                            DuplicateDeath(self.Owner);
                        }
                    };
                }
                duplicate.AddQEffect(effect);
            }

        }

        private static void DuplicateDeath(Creature duplicate) {
            duplicate.Traits.Add(Trait.NoDeathOverhead);
            duplicate.Traits.Add(Trait.NoDeathScream);
            Sfxs.Play(SfxName.PhaseBolt);
            duplicate.Overhead("*disbelieved*", Color.Violet, duplicate.Name + " was revealed as an illusion!");
            duplicate.Battle.RemoveCreatureFromGame(duplicate);
        }
    }
}
