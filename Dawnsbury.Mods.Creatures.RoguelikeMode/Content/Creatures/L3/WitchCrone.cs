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
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class WitchCrone {
        public static Creature Create() {
            return new Creature(IllustrationName.SwampHag, "Agatha Agaricus", new List<Trait>() { Trait.Neutral, Trait.Evil, Trait.Human, Trait.Tiefling, Trait.Humanoid, ModTraits.Witch, Trait.Female, ModTraits.SpellcasterMutator }, 3, 4, 5, new Defenses(17, 9, 6, 12), 60,
            new Abilities(2, 2, 3, 4, 2, 0), new Skills(nature: 13, occultism: 9, intimidation: 10, religion: 9))
            .WithCreatureId(CreatureIds.WitchCrone)
            .WithProficiency(Trait.Unarmed, Proficiency.Trained)
            .WithProficiency(Trait.BattleformAttack, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Trained)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(new Item(IllustrationName.Fist, "nails", new Trait[] { Trait.Unarmed, Trait.Melee, Trait.Brawling, Trait.Finesse }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing)))
            .AddQEffect(new QEffect("Curse of Skittering Paws", "Nature itself turns against the party, calling forth swarms of critters to decend upon them so long as Agatha Agaricus lives.") {
                Tag = true,
                WhenCreatureDiesAtStateCheckAsync = async self => {
                    if (self.Owner.Battle.Encounter.Name == "Crone of the Wilds") {
                        self.Owner.Battle.Cinematics.EnterCutscene();
                        await self.Owner.Battle.Cinematics.LineAsync(self.Owner, "You fiend! This isn't the last you've seen of old Agatha, mark my words!", null);
                        self.Owner.Battle.Cinematics.ExitCutscene();
                    }
                },
                StartOfYourPrimaryTurn = async (self, owner) => {
                    if (!owner.Alive) {
                        return;
                    }

                    if (self.Tag != null && self.Tag as bool? == false) {
                        self.Tag = true;
                        return;
                    }

                    List<Tile> spawnPoints = owner.Battle.Map.AllTiles.Where(t => {
                        if (!t.IsFree || t.Kind == TileKind.Water) {
                            return false;
                        }

                        foreach (Creature pc in owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.IsPlayer)) {
                            if (pc.DistanceTo(t) < 4) {
                                return false;
                            }
                        }
                        return true;
                    }).ToList();
                    Tile spawnPt = spawnPoints[R.Next(0, spawnPoints.Count)];
                    var options = new List<Creature>() { MonsterStatBlocks.CreateGiantRat(), HuntingSpider.Create(), MonsterStatBlocks.CreateVenomousSnake(), MonsterStatBlocks.CreateWolf() };
                    Creature summon = options[R.Next(options.Count)];
                    summon.AddQEffect(new QEffect() {
                        AdditionalGoodness = (self, action, target) => {
                            if (action.HasTrait(Trait.Strike)) {
                                return 5;
                            }
                            return 0;
                        }
                    });
                    if (owner.Battle.Encounter.CharacterLevel == 1) {
                        self.Tag = false;
                    } else if (owner.Battle.Encounter.CharacterLevel < 3) {
                        summon.ApplyWeakAdjustments(false);
                    } else if (owner.Battle.Encounter.CharacterLevel == 4) {
                        summon.ApplyEliteAdjustments();
                    }
                    summon.AddQEffect(new QEffect(self.Name!, "This creature's behaviour has been altered by a powerful curse. Once broken, it will revert to its natural behaviour and flee.") {
                        Innate = false,
                        Source = owner,
                        Illustration = owner.Illustration,
                        StateCheckWithVisibleChanges = async self => {
                            if (!self.Source!.Alive) {
                                self.Owner.Occupies.Overhead($"*{self.Owner.Name} flees!*", Color.Green, $"With the curse broken, {self.Owner.Name} flees from the fight.");
                                self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                            }
                        }
                    });
                    owner.Battle.SpawnCreature(summon, owner.OwningFaction, spawnPt.X, spawnPt.Y);
                    await summon.Battle.Cinematics.PlayCutscene(async cin => {
                        cin.EnterCutscene();
                        summon.Battle.SmartCenter(summon.Occupies.X, summon.Occupies.Y);
                        Sfxs.Play(SfxName.BeastRoar, 0.75f);
                        summon.Occupies.Overhead("*Curse of Skittering Paws*", Color.White, $"{summon.Name} is drawn to aid the coven by the curse of skittering paws.");
                        await cin.WaitABit();
                        cin.ExitCutscene();
                    });
                },
            })
            .AddQEffect(new QEffect("Wild Shape", "At the start of each turn, if wounded, Agatha Agaricus takes on a new animal form, preventing her from casting spells but allowing her access to new attacks.") {
                StartOfYourPrimaryTurn = async (self, owner) => {
                    if (owner.HP > owner.MaxHP * 0.8f) {
                        return;
                    }
                    QEffect transform = new QEffect() {
                        ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn,
                        PreventTakingAction = action => action.HasTrait(Trait.Spell) ? "Cannot cast spells whilst transformed." : null
                    };

                    int roll = R.Next(1, 5);
                    switch (roll) {
                        case 1:
                            transform.Illustration = Illustrations.AnimalFormBear;
                            transform.Name = "Bear Form";
                            transform.Description = $"{self.Owner.Name} has assumed the form of a ferocious bear, capable of grappling its prey on a successful jaws attack.";
                            transform.StateCheck = self => {
                                self.Owner.ReplacementIllustration = Illustrations.AnimalFormBear;
                                self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d10", DamageKind.Piercing, Trait.BattleformAttack).WithAdditionalWeaponProperties(properties => {
                                    properties.WithOnTarget(async (strike, a, d, result) => {
                                        if (result >= CheckResult.Success)
                                            await Possibilities.Grapple(a, d, result);
                                    });
                                });
                            };
                            transform.AdditionalUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.DragonClaws, "claws", "1d6", DamageKind.Slashing, Trait.Agile, Trait.BattleformAttack);
                            transform.BonusToDefenses = (self, action, defence) => {
                                if (defence == Defense.AC) {
                                    return new Bonus(2, BonusType.Item, "Natural Armour");
                                }
                                return null;
                            };
                            // ephemeral.ProvidesArmor = new Item(IllustrationName.None, "Natural Armour", new Trait[] { Trait.UnarmoredDefense, Trait.Armor }).WithArmorProperties(new ArmorProperties(4, 0, 0, 0, 0));
                            goto case 10;
                        case 2:
                            transform.Illustration = Illustrations.AnimalFormSnake;
                            transform.Name = "Serpent Form";
                            transform.Description = $"{self.Owner.Name} has assumed the form of a venomous serpent, capable of poisoning its prey on a successful jaws attack.";
                            transform.StateCheck = self => {
                                self.Owner.ReplacementIllustration = Illustrations.AnimalFormSnake;
                                self.Owner.AddQEffect(Affliction.CreateInjuryQEffect(Affliction.CreateSnakeVenom("Snake Venom")).WithExpirationEphemeral());
                                self.Owner.AddQEffect(QEffect.Swimming().WithExpirationEphemeral());
                                self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Slashing, 2 + self.Owner.Level);
                                self.Owner.WeaknessAndResistance.AddResistance(DamageKind.Piercing, 2 + self.Owner.Level);
                                self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d6", DamageKind.Piercing, Trait.BattleformAttack, Trait.AddsInjuryPoison).WithAdditionalWeaponProperties(properties => {
                                    properties.AdditionalDamage.Add(("1d4", DamageKind.Poison));
                                });
                            };
                            transform.SetBaseSpeedTo = 8;
                            goto case 10;
                        case 3:
                            transform.Illustration = Illustrations.AnimalFormWolf;
                            transform.Name = "Wolf Form";
                            transform.Description = $"{self.Owner.Name} has assumed the form of a cunning wolf, making her cunningly adept at exploiting her foe's weaknesses.";
                            transform.StateCheck = self => {
                                self.Owner.ReplacementIllustration = Illustrations.AnimalFormWolf;
                                self.Owner.ReplacementUnarmedStrike = CommonItems.CreateNaturalWeapon(IllustrationName.Jaws, "jaws", "1d10", DamageKind.Piercing, Trait.BattleformAttack, Trait.Unarmed);
                                self.Owner.AddQEffect(QEffect.SneakAttack("1d8").WithExpirationEphemeral());
                            };
                            goto case 10;
                        case 10:
                            Sfxs.Play(SfxName.BeastRoar, 1.33f);
                            self.Owner.AddQEffect(transform);
                            break;
                        default:
                            break;
                    }
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, ModTraits.Witch, Ability.Intelligence, Trait.Primal).WithSpells(
                level1: new SpellId[] { SpellId.Heal, SpellId.PummelingRubble, SpellId.PummelingRubble },
                level2: new SpellId[] { SpellId.Barkskin }).Done();
        }
    }
}