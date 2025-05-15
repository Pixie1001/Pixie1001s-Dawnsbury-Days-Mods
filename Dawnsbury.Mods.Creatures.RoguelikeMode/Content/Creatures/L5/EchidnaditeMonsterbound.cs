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
using System.Threading;
using System;
using static System.Collections.Specialized.BitVector32;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Campaign.Path;
using Microsoft.Xna.Framework;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Display.Text;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class EchidnaditeMonsterBound {
        public static Creature Create() {
            Creature monster = new Creature(Illustrations.EMonsterbound, "Echidnadite Monsterbound", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Human, Trait.Humanoid, ModTraits.MeleeMutator }, 5, 12, 5, new Defenses(21, 12, 12, 12), 72,
                new Abilities(2, 4, 3, 0, 2, 2), new Skills(nature: 13, intimidation: 12))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (pos, you, step, cr) => you.DistanceTo(cr) <= 2 && (CommonQEffects.IsMonsterAlly(you, cr) || cr.QEffects.Any(qf => qf.Key == "Marked for the Hunt")), 0.5f, false);
                    //AiFuncs.PositionalGoodness(monster, options, (pos, you, step, cr) => you.DistanceTo(cr) <= 2 && CommonQEffects.IsMonsterAlly(you, cr), 0.3f, false);
                    //AiFuncs.PositionalGoodness(monster, options, (pos, you, step, cr) => you.DistanceTo(cr) <= 2 && cr.QEffects.Any(qf => qf.Key == "Marked for the Hunt"), 0.3f, false);

                    return null;
                };
            })
            .WithCreatureId(CreatureIds.EchidnaditeMonsterBound)
            .AddQEffect(CommonQEffects.BlessedOfEchidna())
            .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Trained)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(ItemName.Whip).WithModificationPlusOneStriking())
            .AddQEffect(CommonQEffects.MothersProtection())
            .AddQEffect(new QEffect("Kill Command", "Enemies damaged by Echidnadite Monsterbound's attacks gain the 'Marked for the Hunt' condition, giving them a -2 penalty to all defence DCs again beast and animal creatures.") {
                AfterYouDealDamage = async (attacker, action, defender) => {
                    if (!action.HasTrait(Trait.Strike)) {
                        return;
                    }

                    defender.AddQEffect(new QEffect("Marked for the Hunt", "You suffer a -2 penalty to AC and saves DCs against beasts and animals.", ExpirationCondition.ExpiresAtStartOfSourcesTurn, attacker, IllustrationName.HuntPrey) {
                        Key = "Marked for the Hunt",
                        BonusToDefenses = (self, action, def) => action?.Owner.Occupies != null && !action.Owner.HasTrait(Trait.Celestial) && (action.Owner.HasTrait(Trait.Animal) || action.Owner.HasTrait(Trait.Beast)) ? new Bonus(-2, BonusType.Untyped, "Marked for the Hunt") : null,
                    });
                },
                AdditionalGoodness = (self, action, target) => {
                    if (action == null || !action.HasTrait(Trait.Strike) || !self.Owner.Battle.AllCreatures.Any(cr => cr.FriendOf(cr) && (cr.HasTrait(Trait.Animal) || cr.HasTrait(Trait.Beast)))) {
                        return 0f;
                    }

                    if (target.QEffects.Any(qf => qf.Key == "Marked for the Hunt")) {
                        return 0f;
                    }

                    return 6f;
                }
            })
            .AddQEffect(new QEffect("Monstrous Companion", "The Echidnadite Monsterbound has a Monstrous companion which they can command as a minion once per turn as a free {icon:FreeAction} action.") {
                StartOfCombat = async self => {
                    var list = MonsterStatBlocks.MonsterExemplars.Where(pet => (pet.HasTrait(Trait.Animal) || pet.HasTrait(Trait.Beast)) && CommonEncounterFuncs.Between(pet.Level, self.Owner.Level - 2, self.Owner.Level + 1) && !pet.HasTrait(Trait.Celestial) && !pet.HasTrait(Trait.NonSummonable)).ToArray();

                    int seed = CampaignState.Instance != null && CampaignState.Instance.Tags.TryGetValue("seed", out string result) ? Int32.TryParse(result, out int r2) ? r2 : R.Next(1000) : R.Next(1000);
                    seed += CampaignState.Instance?.CurrentStopIndex != null ? CampaignState.Instance.CurrentStopIndex : 0;

                    Random rand = new Random(seed);

                    if (list.Count() <= 0) {
                        self.Owner.Occupies.Overhead("*summon failed*", Color.White, $"There are no valid monsters for {self.Owner.Name} to be bonded to.");
                        return;
                    }

                    Creature pet = MonsterStatBlocks.MonsterFactories[list[rand.Next(0, list.Count())].Name](self.Owner.Battle.Encounter, self.Owner.Occupies);
                    pet.InitiativeControlledBy = self.Owner;
                    pet.WithEntersInitiativeOrder(false);
                    pet.Traits.Add(Trait.Minion);
                    pet.AddQEffect(new QEffect() {
                        Id = QEffectId.SummonedBy,
                        Source = self.Owner,
                        StateCheck = dominateQf => {
                            if (dominateQf.Source == null)
                                return;

                            if (dominateQf.Source.HasEffect(QEffectId.Controlled))
                                dominateQf.Owner.AddQEffect(new QEffect() { Id = QEffectId.Controlled }.WithExpirationEphemeral());

                            dominateQf.Owner.OwningFaction = dominateQf.Source.OwningFaction;

                            if (!dominateQf.Source.Alive) {
                                dominateQf.Owner.Battle.RemoveCreatureFromGame(dominateQf.Owner);
                            }
                        }
                    });
                    if (pet.Level - self.Owner.Level >= 2) {
                        pet.ApplyWeakAdjustments(false, true);
                    } else if (pet.Level - self.Owner.Level == 1) {
                        pet.ApplyWeakAdjustments(false);
                    } else if (pet.Level - self.Owner.Level == -1) {
                        pet.ApplyEliteAdjustments();
                    } else if (pet.Level - self.Owner.Level == -2) {
                        pet.ApplyEliteAdjustments(true);
                    }

                    pet.MainName = self.Owner.Name + "'s " + pet.MainName;

                    pet.AddQEffect(new QEffect("Monstrous Companion",
                        $"This monster will flee when it's master {{Blue}}{self.Owner.Name}{{/Blue}} is defeated.", ExpirationCondition.Never, self.Owner, self.Owner.Illustration));

                    self.Owner.Battle.SpawnCreature(pet, self.Owner.OwningFaction, self.Owner.Occupies);
                },
                ProvideMainAction = self => {
                    Creature? pet = self.Owner.Battle.AllCreatures.FirstOrDefault(cr => cr.FindQEffect(QEffectId.SummonedBy)?.Source == self.Owner);

                    if (pet == null)
                        return null;

                    return new ActionPossibility(new CombatAction(self.Owner, pet.Illustration, "Command " + pet.BaseName, [Trait.Basic, Trait.DoesNotBreakStealth], "Your bonded monster takes its turn. You choose what actions it takes.",
                        Target.Self((cr, ai) => cr.Actions.ActionsLeft == 0 ? 1f : AIConstants.NEVER).WithAdditionalRestriction(cr => CustomItems.GetAnimalCompanionCommandRestriction(self, pet)))
                    .WithActionCost(0)
                    .WithEffectOnSelf(async (cr) => {
                        await CommonSpellEffects.YourMinionActs(pet);
                        self.UsedThisTurn = true;
                    }));
                }
            })
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    if (self.Owner.HasEffect(QEffectId.UsedMonsterOncePerEncounterAbility))
                        return null;

                    int spellLevel = (self.Owner.Level + 1) / 2;

                    var menu = new SubmenuPossibility(IllustrationName.HealCompanion, "Heal Companion");
                    var section = new PossibilitySection("Heal Companion");
                    menu.Subsections.Add(section);

                    section.AddPossibility((ActionPossibility)new CombatAction(self.Owner, IllustrationName.HealCompanion, "Heal Companion", [Trait.Healing, Trait.Necromancy, Trait.Positive],
                        "{b}Frequency{/b} Once per encounter\n{b}Range{/b} Touch\n{b}Target{/b} Your monstrous companion\n\n{i}You harness positive energy to heal your animal companion's wounds.{/i}\n\n" +
                        $"You restore {S.HeightenedVariable(spellLevel, 1)}d10 Hit Points to your animal companion.",
                        Target.AdjacentFriend()
                        .WithAdditionalConditionOnTargetCreature((a, d) => d.QEffects.Any(qf => qf.Id == QEffectId.SummonedBy && qf.Source == a) ? Usability.Usable : Usability.NotUsableOnThisCreature("not-animal-companion"))
                        .WithAdditionalConditionOnTargetCreature((a, d) => d.Damage != 0 ? Usability.NotUsableOnThisCreature("healthy") : Usability.Usable))
                    .WithSoundEffect(SfxName.Healing)
                    .WithActionCost(1)
                    .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                        await target.HealAsync(spellLevel + "d10", spell);
                    })
                    .WithEffectOnSelf(caster => {
                        caster.AddQEffect(new QEffect() { Id = QEffectId.UsedMonsterOncePerEncounterAbility });
                    })
                    .WithShortDescription($"Once per encounter, the Echidnadite Monsterbound can heal its monstrous companion for {spellLevel}d10 hit points. If they spend two actions, it gains a range of 30 feet and heals an addition {8 * spellLevel} HP.")
                    .WithGoodness((t, a, d) => {
                         if (d.Damage == 0)
                             return AIConstants.NEVER;

                         return Math.Min(spellLevel * 5.5f, d.Damage);
                     }));

                    section.AddPossibility((ActionPossibility)new CombatAction(self.Owner, IllustrationName.HealCompanion, "Heal Companion", [Trait.Healing, Trait.Necromancy, Trait.Positive, Trait.Basic],
                        "{b}Frequency{/b} Once per encounter\n{b}Range{/b} 30 feet\n{b}Target{/b} Your monstrous companion\n\n{i}You harness positive energy to heal your animal companion's wounds.{/i}\n\n" +
                        $"You restore {S.HeightenedVariable(spellLevel, 1)}d10 + {S.HeightenedVariable(8 * spellLevel, 8)} Hit Points to your animal companion.",
                        Target.RangedFriend(6)
                        .WithAdditionalConditionOnTargetCreature((a, d) => d.QEffects.Any(qf => qf.Id == QEffectId.SummonedBy && qf.Source == a) ? Usability.Usable : Usability.NotUsableOnThisCreature("not-animal-companion"))
                        .WithAdditionalConditionOnTargetCreature((a, d) => d.Damage != 0 ? Usability.NotUsableOnThisCreature("healthy") : Usability.Usable))
                    .WithSoundEffect(SfxName.Healing)
                    .WithActionCost(2)
                    .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                        await target.HealAsync(spellLevel + "d10" + "+" + (8 * spellLevel), spell);
                    })
                    .WithEffectOnSelf(caster => {
                        caster.AddQEffect(new QEffect() { Id = QEffectId.UsedMonsterOncePerEncounterAbility });
                    })
                    .WithGoodness((t, a, d) => {
                        if (d.Damage == 0)
                            return AIConstants.NEVER;

                        return Math.Min(spellLevel * 5.5f + 8 * spellLevel, d.Damage);
                    }));

                    return menu;
                }
            });
            return monster;
        }

        private static float HealCOmpanionGoodness(int spellLevel, Creature a, Creature d) {
            if (d.Damage == 0)
                return AIConstants.NEVER;

            return Math.Min(spellLevel * 5.5f, d.Damage);
        }
    }
}

