using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawnsbury.Display;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using FMOD;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Core.CharacterBuilder.Selections.Selected;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Core.Roller;
using Microsoft.Xna.Framework;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.AuraAnimations;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Display.Illustrations;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.StatBlocks.Monsters.L5;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Tables {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class MonsterMutatorTable {
        private static List<MonsterArchetype> mutators = new List<MonsterArchetype>();

        public static bool RollForMutator(Creature creature) {
            int seed = CampaignState.Instance != null && CampaignState.Instance.Tags.TryGetValue("seed", out string result) ? Int32.TryParse(result, out int r2) ? r2 : R.Next(1000) : R.Next(1000);
            seed += CampaignState.Instance?.CurrentStopIndex != null ? CampaignState.Instance.CurrentStopIndex : 0;
            seed += creature.Battle.AllCreatures.Where(cr => cr.OwningFaction == cr.Battle.Enemy).ToList().FindIndex(cr => cr == creature);

            Random rand = new Random(seed);

            List<MonsterArchetype> viableResults = GetFilteredList(creature);

            if (viableResults.Count <= 0)
                return false;

            int num = rand.Next(0, viableResults.Count);

            viableResults[num].Apply(creature);
            return true;
        }

        public static List<MonsterArchetype> GetFilteredList(Creature creature) {
            return mutators.Where(m => m.CheckIfValid(creature)).ToList();
        }

        public static void LoadMutators() {
            mutators.Clear();

            mutators.Add(new MonsterArchetype("Vampiric", "This creature heals for an amount of damage equal to that which it deals on a melee strike.", [ModTraits.MeleeMutator], (mutator, creature) => {
                QEffect effect = new QEffect() {
                    AdditionalGoodness = (self, action, target) => {
                        if (action != null && action.HasTrait(Trait.Strike) && action.HasTrait(Trait.Melee) && target.IsLivingCreature && self.Owner.Damage >= 0) {
                            return 5;
                        }
                        return 0;
                    }
                };
                effect.AddGrantingOfTechnical(cr => cr.IsLivingCreature, qfTechnical => {
                    qfTechnical.AfterYouTakeAmountOfDamageOfKind = async (self, strike, damage, kind) => {
                        if (strike != null && strike.Owner == creature && strike.HasTrait(Trait.Strike) && strike.HasTrait(Trait.Melee) && self.Owner.IsLivingCreature) {
                            await creature.HealAsync(DiceFormula.FromText($"{damage}", "Vampiric"), strike);
                        }
                    };
                });

                creature.AddQEffect(effect);
            }));

            mutators.Add(new MonsterArchetype("Analytical", "Enemy creatures damaged by this creature gain the 'analysed' condition, causing them to take 1d6 additional damage when damaged by an ally of this creature.", [ModTraits.UniversalMutator], (mutator, creature) => {
                QEffect effect = new QEffect() {
                    AfterYouDealDamage = async (you, action, target) => {
                        if (target?.Occupies == null) {
                            return;
                        }

                        target.AddQEffect(new QEffect("Analysed", $"You suffer an additional 1d6 precision damage from attacks made by enemy creatures other than {action.Owner.Name}.", ExpirationCondition.CountsDownAtStartOfSourcesTurn, you, IllustrationName.HuntPrey) {
                            Value = 1
                        });
                    }
                };
                effect.AddGrantingOfTechnical(cr => cr.FriendOfAndNotSelf(effect.Owner), qfTechnical => {
                    qfTechnical.AfterYouDealDamage = async (you, action, target) => {
                        if (target.EnemyOf(you) && target.QEffects.Any(qf => qf.Name == "Analysed" && qf.Source != you)) {
                            await CommonSpellEffects.DealDirectSplashDamage(CombatAction.CreateSimple(you.Battle.Pseudocreature, "Analysed", Trait.PrecisionDamage), DiceFormula.FromText("1d6", "Analysed Bonus Damage"), target, DamageKind.Untyped);
                        }
                    };
                    qfTechnical.AdditionalGoodness = (self, action, target) => {
                        if (target.EnemyOf(self.Owner) && target.QEffects.Any(qf => qf.Name == "Analysed" && qf.Source != self.Owner)) {
                            return 3.5f;
                        }
                        return 0;
                    };
                });

                creature.AddQEffect(effect);
            }));

            mutators.Add(new MonsterArchetype("Berserking",
                "This creature is consumed by a reckless, berserkers rage, granting them additional temporary hit points, a +4 bonus to damage and a +5 bonus to speed, at the expense of a -2 penalty to their AC and Will save DC. They cannot use Concentrate actions.",
                [ModTraits.MeleeMutator], (mutator, creature) => {
                    QEffect effect = new QEffect() {
                        BonusToDamage = (self, action, target) => !action.HasTrait(Trait.Spell) ? new Bonus(4, BonusType.Untyped, "Berserking") : null,
                        BonusToDefenses = (self, action, def) => def == Defense.AC || def == Defense.Will ? new Bonus(-2, BonusType.Untyped, "Berserking") : null,
                        BonusToAllSpeeds = (self) => new Bonus(1, BonusType.Untyped, "Berserking"),
                        PreventTakingAction = action => action.HasTrait(Trait.Concentrate) && !action.HasTrait(Trait.Rage) ? "Cannot use concentrate actions" : null,
                    };
                    creature.GainTemporaryHP(creature.Level * 2);
                    creature.AddQEffect(effect);
                }));

            mutators.Add(new MonsterArchetype("Mirrored", "This creature is obscured by three illusory images, swirling about their space, as per the {i}mirror image{/i} spell.", [ModTraits.UniversalMutator], (mutator, creature) => {
                var effect = Level2Spells.CreateMirrorImageEffect(creature);
                creature.AddQEffect(effect);
            }));

            mutators.Add(new MonsterArchetype("Volatile", "", [ModTraits.MeleeMutator], (mutator, creature) => {
                int dc = SkillChallengeTables.GetDCByLevel(creature.Level) + 2;
                string dmg = (1 + Math.Max(1, creature.Level / 2)) + "d6";
                mutator.description = $"This creature explodes on death, dealing {dmg} force damage vs. a basic Reflex save (DC {dc}) against each creature within 10 feet of them.";
                creature.AddQEffect(new QEffect("Volatile", $"This creature explodes on death, dealing {dmg} force damage vs. a basic Reflex save (DC {dc}) against each creature within 10 feet of them.") {
                    Illustration = IllustrationName.Fireball,
                    Innate = false,
                    YouAreDealtDamageEvent = async (self, dmgEvent) => {
                        Tile location = self.Owner.Occupies;

                        CombatAction explosion = new CombatAction(self.Owner, IllustrationName.Fireball, "Volatile Explosion", [Trait.Force, Trait.UsableEvenWhenUnconsciousOrParalyzed], "", Target.SelfExcludingEmanation(2))
                        .WithActionCost(0)
                        .WithSoundEffect(SfxName.Fireball)
                        .WithSavingThrow(new SavingThrow(Defense.Reflex, dc))
                        .WithProjectileCone(IllustrationName.AcidSplash, 15, ProjectileKind.Cone)
                        .WithEffectOnEachTarget(async (spell, a, d, r) => {
                            await CommonSpellEffects.DealBasicDamage(spell, a, d, r, DiceFormula.FromText(dmg, "Volatile Explosion"), DamageKind.Force);
                        });
                        self.Owner.Battle.AllCreatures.Where(cr => cr != self.Owner && cr.DistanceTo(location) <= 2 && cr.HasLineOfEffectTo(location) < CoverKind.Blocked).ForEach(cr => explosion.ChosenTargets.ChosenCreatures.Add(cr));
                        self.Owner.Battle.Map.AllTiles.Where(t => t.DistanceTo(location) <= 2 && t.DistanceTo(location) > 0).ForEach(t => explosion.ChosenTargets.ChosenTiles.Add(t));
                        self.UsedThisTurn = true;
                        await explosion.AllExecute();
                        self.ExpiresAt = ExpirationCondition.Immediately;
                    }
                });
            }));

            mutators.Add(new MonsterArchetype("Deflecting", "This creature is master at deflecting attacks in melee, gaining a +2 bonus to AC against melee attacks.", [ModTraits.UniversalMutator], (mutator, creature) => {
                QEffect effect = new QEffect() {
                    BonusToDefenses = (self, action, def) => action != null && action.HasTrait(Trait.Melee) && def == Defense.AC ? new Bonus(2, BonusType.Untyped, "Deflecting") : null
                };

                creature.AddQEffect(effect);
            }));

            mutators.Add(new MonsterArchetype("Galeward", "An aegis of wind protects this creature from projectiles, granting them a +2 bonus Reflex, and to AC against ranged attacks.", [ModTraits.UniversalMutator], (mutator, creature) => {
                QEffect effect = new QEffect() {
                    BonusToDefenses = (self, action, def) => def == Defense.Reflex || (action != null && action.HasTrait(Trait.Ranged) && def == Defense.AC) ? new Bonus(2, BonusType.Untyped, "Galeward") : null
                };

                creature.AddQEffect(effect);
            }));

            mutators.Add(new MonsterArchetype("Impervious", "", [ModTraits.UniversalMutator], (mutation, creature) => {
                QEffect effect = new QEffect() {
                    StateCheckLayer = 1,
                    StateCheck = (self) => {
                        mutation.description = $"This creature cannot be harmed by mere physical attacks, granting them resistence {3 + self.Owner.Level} against bludgeoning, piercing and slashing damage.";

                        DamageKind[] resList = [DamageKind.Bludgeoning, DamageKind.Slashing, DamageKind.Piercing];

                        foreach (DamageKind dmgKind in resList) {
                            Resistance? weakness = self.Owner.WeaknessAndResistance.Weaknesses.MaxBy(res => res.DamageKind == dmgKind ? res.Value : 0);
                            int weakVal = weakness?.DamageKind == dmgKind ? weakness.Value : 0;

                            if (weakness != null && weakVal > 0)
                                self.Owner.WeaknessAndResistance.Weaknesses.Remove(weakness);

                            if (self.Owner.Level + 3 > weakVal)
                                self.Owner.WeaknessAndResistance.AddResistance(dmgKind, self.Owner.Level + 3 - weakVal);
                            else
                                self.Owner.WeaknessAndResistance.AddWeakness(dmgKind, weakVal - (self.Owner.Level + 3));
                        }
                    }
                };

                creature.AddQEffect(effect);
            }));

            mutators.Add(new MonsterArchetype("Trollblood", "The blood of trolls runs through this creature's vein, granting them natural regeneration.", [ModTraits.UniversalMutator], (mutator, creature) => {
                creature.AddQEffect(QEffect.Regeneration(Math.Max(5, creature.Level * 5), [DamageKind.Acid, DamageKind.Fire], [], true));
            }));

            mutators.Add(new MonsterArchetype("Eternal", "Foul necromancy sustains this creature, granting them regeneration and undead status.", [ModTraits.UniversalMutator], (mutator, creature) => {
                creature.Traits.Add(Trait.Undead);
                creature.AddQEffect(QEffect.DamageImmunity(DamageKind.Negative));
                creature.AddQEffect(QEffect.DamageImmunity(DamageKind.Bleed));
                creature.AddQEffect(QEffect.DamageImmunity(DamageKind.Poison));
                creature.AddQEffect(QEffect.TraitImmunity(Trait.Poison));
                creature.AddQEffect(QEffect.TraitImmunity(Trait.Death));
                creature.AddQEffect(QEffect.TraitImmunity(Trait.Disease));
                creature.AddQEffect(QEffect.ImmunityToCondition(QEffectId.Paralyzed));
                creature.AddQEffect(QEffect.ImmunityToCondition(QEffectId.Unconscious));

                creature.AddQEffect(QEffect.Regeneration(Math.Max(5, creature.Level * 5), [DamageKind.Good, DamageKind.Positive], [], true));
            }));

            mutators.Add(new MonsterArchetype("Rat Blessed", "This creature commands a swarm of ravenous rats. Killing them will disperse their swarm.", [ModTraits.UniversalMutator], (mutator, creature) => {
                QEffect effect = new QEffect() {
                    WhenMonsterDies = self => {
                        foreach (var rat in self.Owner.Battle.AllCreatures.Where(cr => cr.QEffects.Any(qf => qf.Name == "Rat Swarm Familiar" && qf.Source == self.Owner)).ToList()) {
                            self.Owner.Battle.RemoveCreatureFromGame(rat);
                        }
                    }
                };

                // TODO: Make a proper formula for this
                int numRats = Math.Max(2, Math.Min(creature.Level, 5));
                int ratLevel = Math.Max(-2, Math.Min(creature.Level - 3, 1));

                for (int i = 0; i < numRats; i++) {
                    var rat = CreatureList.Creatures[CreatureIds.RavenousRat](creature.Battle.Encounter);
                    rat.AddQEffect(new QEffect("Rat Swarm Familiar", $"Killing {creature.Name} will cause this creature to flee the encounter.", ExpirationCondition.Never, creature, IllustrationName.GiantRat256));
                    rat.MainName = "Rat Familiar";
                    if (ratLevel == -2) rat.ApplyWeakAdjustments(false);
                    else if (ratLevel == 0) rat.ApplyEliteAdjustments();
                    else if (ratLevel == 1) rat.ApplyEliteAdjustments(true);
                    rat.MainName = creature.Name + "'s " + rat.MainName;
                    creature.Battle.SpawnCreature(rat, creature.OwningFaction, creature.Occupies);
                }

                creature.AddQEffect(effect);
            }));

            mutators.Add(new MonsterArchetype("Mooncursed", "This creature has been infected by a werecreature, allowing them to assume their true monstrous form when reduced below half HP. Their new form shakes off all conditions, but begins combat at half HP.",
                [ModTraits.UniversalMutator], (mutator, creature) => {
                QEffect effect = new QEffect() {
                    WhenCreatureDiesAtStateCheckAsync = async self => {
                        if (self.Owner.Damage >= self.Owner.MaxHP) {
                            Tile pos = self.Owner.Occupies;
                            self.Owner.Battle.RemoveCreatureFromGame(self.Owner);

                            var list = MonsterStatBlocks.MonsterExemplars.Where(pet => (pet.HasTrait(Trait.Animal) || pet.HasTrait(Trait.Beast)) && CommonEncounterFuncs.Between(pet.Level, self.Owner.Level - 1, self.Owner.Level + 2) && !pet.HasTrait(Trait.Celestial) && !pet.HasTrait(Trait.NonSummonable)).ToArray();
                            int rand = R.Next(0, list.Count());

                            if (list.Count() == 0)
                                return;

                            Creature newForm = MonsterStatBlocks.MonsterFactories[list[rand].Name](self.Owner.Battle.Encounter, self.Owner.Occupies);

                            if (newForm.Level - self.Owner.Level >= 3) {
                                newForm.ApplyWeakAdjustments(false, true);
                            } else if (newForm.Level - self.Owner.Level == 2) {
                                newForm.ApplyWeakAdjustments(false);
                            } else if (newForm.Level - self.Owner.Level == 0) {
                                newForm.ApplyEliteAdjustments();
                            } else if (newForm.Level - self.Owner.Level == -1) {
                                newForm.ApplyEliteAdjustments(true);
                            }
                            Sfxs.Play(SfxName.BeastRoar);
                            self.Owner.Battle.SpawnCreature(newForm, self.Owner.OwningFaction, pos);
                            await CommonSpellEffects.DealDirectSplashDamage(null, DiceFormula.FromText((newForm.MaxHP / 2).ToString(), "Mooncursed"), newForm, DamageKind.Untyped);
                            self.Owner.Overhead("mooncursed", Color.Aqua, $"{self.Owner.Name}'s curse activates, causing them to assume the form of a{("aeiouAEIOU".IndexOf(newForm.Name[0]) >= 0 ? "n" : "")} {newForm.Name}!");
                        }
                    }
                };
                creature.AddQEffect(effect);
            }));

            mutators.Add(new MonsterArchetype("Studious", "This creature is capable of casting powerful high level spells.", [ModTraits.SpellcasterMutator], (mutator, creature) => {
                QEffect effect = new QEffect();
                creature.AddQEffect(effect);
                switch (creature.CreatureId) {
                    case var v when v.Equals(CreatureIds.DrowArcanist):
                        creature.Spellcasting?.PrimarySpellcastingSource.WithSpells([SpellId.AcidArrow], 2);
                        break;
                    case var v when v.Equals(CreatureIds.DrowShadowcaster):
                        creature.Spellcasting?.PrimarySpellcastingSource.WithSpells([SpellId.Fear], 3);
                        break;
                    case var v when v.Equals(CreatureIds.DrowNecromancer):
                        creature.Spellcasting?.PrimarySpellcastingSource.WithSpells([SpellId.AnimateDead], 3);
                        break;
                    case var v when v.Equals(CreatureIds.DrowPriestess):
                        creature.Spellcasting?.PrimarySpellcastingSource.WithSpells([SpellId.SuddenBlight], 3);
                        break;
                    case var v when v.Equals(CreatureIds.MerfolkBrineBlade):
                        creature.Spellcasting?.PrimarySpellcastingSource.WithSpells([SpellId.BrinyBolt], 2);
                        break;
                    case var v when v.Equals(CreatureIds.DevotedCultist):
                        creature.Spellcasting?.PrimarySpellcastingSource.WithSpells([SpellId.BrinyBolt], 1);
                        break;
                    case var v when v.Equals(CreatureIds.DrowInquisitrix):
                        creature.Spellcasting?.PrimarySpellcastingSource.WithSpells([SpellId.BoneSpray], 2);
                        break;
                    case var v when v.Equals(CreatureIds.EchidnaditeBroodNurse):
                        creature.Spellcasting?.PrimarySpellcastingSource.WithSpells([SpellId.BoneSpray], 2);
                        break;
                    case var v when v.Equals(CreatureIds.EchidnaditePriestess):
                        creature.Spellcasting?.PrimarySpellcastingSource.WithSpells([SpellId.BoneSpray], 2);
                        break;
                    case var v when v.Equals(CreatureIds.NightmareWeaver):
                        creature.Spellcasting?.PrimarySpellcastingSource.WithSpells([SpellId.BoneSpray], 2);
                        break;
                }
            }));

            mutators.Add(GenerateElementalArchetype("Stormheart", DamageKind.Electricity, Trait.Electricity));
            mutators.Add(GenerateElementalArchetype("Pyreheart", DamageKind.Fire, Trait.Fire, DamageKind.Cold));
            mutators.Add(GenerateElementalArchetype("Steelheart", DamageKind.Slashing, Trait.Metal, DamageKind.Electricity));
            mutators.Add(GenerateElementalArchetype("Terraheart", DamageKind.Bludgeoning, Trait.Earth));
            mutators.Add(GenerateElementalArchetype("Venomheart", DamageKind.Poison, Trait.Poison));
            mutators.Add(GenerateElementalArchetype("Windheart", DamageKind.Slashing, Trait.Air));
            mutators.Add(GenerateElementalArchetype("Coldheart", DamageKind.Cold, Trait.Cold, DamageKind.Fire));
        }

        private static MonsterArchetype GenerateElementalArchetype(string name, DamageKind element, Trait elementTrait, DamageKind weakness = DamageKind.Untyped) {
            Color colour = Color.White;
            switch (elementTrait) {
                case Trait.Fire:
                    colour = Color.OrangeRed;
                    break;
                case Trait.Earth:
                    colour = Color.Sienna;
                    break;
                case Trait.Air:
                    colour = Color.Beige;
                    break;
                case Trait.Electricity:
                    colour = Color.DeepSkyBlue;
                    break;
                case Trait.Metal:
                    colour = Color.DarkSlateGray;
                    break;
                case Trait.Poison:
                    colour = Color.Olive;
                    break;
                case Trait.Cold:
                    colour = Color.PaleTurquoise;
                    break;
            }

            bool physical = element == DamageKind.Bludgeoning || element == DamageKind.Slashing || element == DamageKind.Piercing ? true : false;
            bool hasWeakness = weakness != DamageKind.Untyped;

            return new MonsterArchetype(name, "", [ModTraits.UniversalMutator], (mutator, creature) => {
                creature.AnimationData.AddAuraAnimation(new MagicCircleAuraAnimation(Illustrations.KinestistCircleWhite, colour, 2));
                creature.AnimationData.AuraAnimations.Last().MaximumOpacity = 0.7f;

                mutator.description = $"This creature is empowered by the element of {elementTrait.HumanizeTitleCase2()}," +
                    $" gaining immunity to {elementTrait.HumanizeTitleCase2()} and {(physical ? "resistance 5 to" : "")} {element.HumanizeTitleCase2()} damage{(hasWeakness ? $", as well as weakness 5 to {weakness.HumanizeTitleCase2()} damage" : "")}." +
                    $" It also gains an aura that deals 1d6+{creature.Level} {element.HumanizeTitleCase2()} " +
                    $"damage (Basic fort vs. DC {SkillChallengeTables.GetDCByLevel(creature.Level) + 2}) to enemy creatures who end their turn within 10ft of it. In addition, their spells deal +2 {element.HumanizeTitleCase2()} damage.";

                QEffect effect = new QEffect() {
                    StateCheck = self => {
                        if (hasWeakness) {
                            self.Owner.WeaknessAndResistance.AddWeakness(weakness, 5);
                        }

                        if (physical) {
                            self.Owner.WeaknessAndResistance.AddResistance(element, 5);
                            return;
                        }
                        self.Owner.WeaknessAndResistance.AddImmunity(element);
                    },
                    ImmuneToTrait = elementTrait,
                    AfterYouDealDamage = async (caster, action, target) => {
                        if (action.HasTrait(Trait.Spell)) {
                            await CommonSpellEffects.DealDirectSplashDamage(CombatAction.CreateSimple(caster, action.Name), DiceFormula.FromText("2", name), target, element);
                        }
                    }
                };
                effect.AddGrantingOfTechnical(cr => cr.EnemyOf(creature) && !cr.IsImmuneTo(elementTrait), qfTechnical => {
                    qfTechnical.EndOfYourTurnBeneficialEffect = async (self, you) => {
                        if (you.DistanceTo(effect.Owner) <= 2) {
                            var ca = CombatAction.CreateSimple(creature, name + " aura");
                            var result = CommonSpellEffects.RollSavingThrow(you, ca, Defense.Fortitude, SkillChallengeTables.GetDCByLevel(creature.Level) + 2);
                            await CommonSpellEffects.DealBasicDamage(ca, creature, you, result, DiceFormula.FromText($"1d6+{creature.Level}", ca.Name), element);
                        }
                    };
                });
                creature.AddQEffect(effect);
            });
        }
    }
}
