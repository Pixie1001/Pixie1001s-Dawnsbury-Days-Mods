using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.AuraAnimations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Mechanics.Zoning;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Monsters.L3;
using Dawnsbury.Core.StatBlocks.Monsters.L5;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class VampireEnthraller {
        public static Creature Create() {
            var monster = CommonQEffects.MakeVampire(new Creature(IllustrationName.Succubus, "Vampire Enthraller", [Trait.Undead, Trait.Lawful, Trait.Evil, ModTraits.SpellcasterMutator], level: 6, perception: 16, speed: 5, new Defenses(23, 11, 14, 17), hp: 40, new Abilities(2, 4, 4, 2, 4, 6), new Skills(athletics: 13, intimidation: 15, diplomacy: 15, deception: 15, occultism: 13, arcana: 11, acrobatics: 13)), 5, 10)
                .WithBasicCharacteristics()
                .WithCreatureId(CreatureIds.VampireEnthraller)
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .WithUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Claw, "2d6", DamageKind.Slashing, [Trait.Agile, Trait.Finesse, Trait.Grab]))
                .AddQEffect(new QEffect("The Crimson Kiss", "Drained enemies suffer a -2 status penalty to Will saves aginst you.") {
                    StateCheck = qfSelf => {
                        qfSelf.Owner.Battle.AllCreatures.Where(cr => cr.EnemyOf(qfSelf.Owner)).ForEach(cr => cr.AddQEffect(new QEffect() {
                            BonusToDefenses = (qf, ca, def) => ca?.Owner == qfSelf.Owner && def == Defense.Will && qf.Owner.HasEffect(QEffectId.Drained) ? new Bonus(-2, BonusType.Status, "The crimson kiss") : null
                        }.WithExpirationEphemeral()));
                    }
                })
                .AddQEffect(new QEffect("Children of the Night", "The Vampire Enthraller enters combat with two enthralled nocturnal creatures, which it may Command {icon:Action} as a minion once per round.") {
                    StartOfCombat = async self => {
                        for (int i = 0; i < 2; i++) {
                            Creature[] nightCreatures = [GiantSpider.Create(), BloodWolf.CreateBloodWolf()];
                            var pet = CommonQEffects.GetSummonCandidateOfLevel(self.Owner, pet => nightCreatures.Any(cr => cr.CreatureId == pet.CreatureId || cr.BaseName == pet.BaseName), self.Owner.Level - 1);
                            if (pet == null) {
                                self.Owner.Overhead("*summon failed*", Color.White, $"There are no valid creatures for {self.Owner.Name} to enthrall.");
                                return;
                            }

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

                            pet.MainName = self.Owner.Name + "'s " + pet.MainName;

                            self.Owner.Battle.SpawnCreature(pet, self.Owner.OwningFaction, self.Owner.Occupies);

                            pet.AddQEffect(new QEffect("Children of the Night",
                                $"This creature will flee after {{Blue}}{self.Owner.Name}{{/Blue}} is defeated.", ExpirationCondition.Never, self.Owner, self.Owner.Illustration));
                        }
                    },
                    ProvideMainAction = self => {
                        Creature[] pets = self.Owner.Battle.AllCreatures.Where(cr => cr.FindQEffect(QEffectId.SummonedBy)?.Source == self.Owner).ToArray();

                        if (pets.Count() == 0)
                            return null;

                        return new ActionPossibility(new CombatAction(self.Owner, IllustrationName.Dominate, "Command Familiars", [Trait.Basic, Trait.DoesNotBreakStealth], "Your enthralled familiars both may take their turn. You choose what actions they take.",
                            Target.Self((cr, ai) => pets.Count() * 15).WithAdditionalRestriction(cr => {
                                if (self.UsedThisTurn) return "You already commanded your animal companions this turn.";
                                if (pets.All(pet => pet.HasEffect(QEffectId.Paralyzed))) return "Your animal companion is paralyzed.";
                                if (pets.All(pet => pet.Actions.ActionsLeft == 0 && (pet.Actions.QuickenedForActions == null || pet.Actions.UsedQuickenedAction))) return "You animal companions have no actions they could take.";
                                return null;
                            }))
                        .WithActionCost(1)
                        .WithEffectOnSelf(async (cr) => {
                            foreach (var pet in pets) {
                                await CommonSpellEffects.YourMinionActs(pet);
                            }
                            self.UsedThisTurn = true;
                        }));
                    }
                })
                .AddMonsterInnateSpellcasting(16, Trait.Occult, level1Spells: [SpellId.Command, SpellId.Command, SpellId.Command, SpellId.Command, SpellId.Command])
                .Builder
                .Done();

            return monster;
        }
    }
}