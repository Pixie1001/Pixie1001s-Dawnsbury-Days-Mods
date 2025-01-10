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
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class WitchMaiden {
        public static Creature Create() {
            return new Creature(IllustrationName.SuccubusShapeshifted, "Harriet Hex", new List<Trait>() { Trait.Neutral, Trait.Evil, Trait.Human, Trait.Tiefling, Trait.Humanoid, ModTraits.Witch, Trait.Female }, 3, 6, 5, new Defenses(17, 6, 9, 12), 50,
            new Abilities(0, 4, 3, 4, 2, 0), new Skills(nature: 10, occultism: 10, intimidation: 9, arcana: 14))
            .WithProficiency(Trait.Unarmed, Proficiency.Trained)
            .WithProficiency(Trait.Crossbow, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(CustomItems.Hexshot))
            .WithUnarmedStrike(new Item(IllustrationName.Fist, "nails", new Trait[] { Trait.Unarmed, Trait.Melee, Trait.Brawling, Trait.Finesse }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing)))
            .AddQEffect(new QEffect("Curse of Agony", $"The party are wracked by terrible pain, which will not abate so long as the caster lives.") {
                StateCheckWithVisibleChanges = async self => {
                    if (!self.Owner.Alive) {
                        return;
                    }

                    string curseDmg = "1d8";
                    if (self.Owner.Level == 2) {
                        curseDmg = "1d4";
                    } else if (self.Owner.Level == 3) {
                        curseDmg = "1d8";
                    } else if (self.Owner.Level == 4) {
                        curseDmg = "1d10";
                    }

                    List<Creature> party = self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(self.Owner.OwningFaction)).ToList();
                    party.ForEach(cr => {
                        cr.AddQEffect(new QEffect("Curse of Agony", $"You suffer {curseDmg} mental damage at the start of each turn so long as {self.Owner.Name} lives.") {
                            ExpiresAt = ExpirationCondition.Ephemeral,
                            Innate = false,
                            Source = self.Owner,
                            Illustration = self.Owner.Illustration,
                            StartOfYourTurn = async (qfCurse, victim) => {
                                if (victim.Traits.Any(t => t.HumanizeTitleCase2() == "Eidolon")) {
                                    QEffect bond = victim.QEffects.FirstOrDefault(qf => qf.Id.HumanizeTitleCase2() == "Summoner_Shared HP");
                                    CombatAction action = new CombatAction(self.Owner, self.Illustration, "Curse of Agony", new Trait[] { Trait.Curse, Trait.Mental, Trait.Arcane }, "", Target.Emanation(100).WithIncludeOnlyIf((area, target) => {
                                        return target == victim || target == bond.Source;
                                    }))
                                    .WithEffectOnEachTarget(async (spell, user, d, result) => {
                                        await CommonSpellEffects.DealDirectDamage(spell, DiceFormula.FromText($"{curseDmg}", "Curse of Agony"), d, CheckResult.Success, DamageKind.Mental);
                                    });
                                    action.ChosenTargets.ChosenCreatures.Add(victim);
                                    action.ChosenTargets.ChosenCreatures.Add(bond.Source);
                                    await action.AllExecute();
                                    return;
                                } else if (victim.Traits.Any(t => t.HumanizeTitleCase2() == "Summoner") && victim.Battle.AllCreatures.Any(cr => cr.Traits.Any(t => t.HumanizeTitleCase2() == "Eidolon") && cr.QEffects.FirstOrDefault(qf => qf.Id.HumanizeTitleCase2() == "Summoner_Shared HP").Source == victim)) {
                                    return;
                                }
                                CombatAction action2 = new CombatAction(self.Owner, self.Illustration, "Curse of Agony", new Trait[] { Trait.Curse, Trait.Mental, Trait.Arcane }, "", Target.Uncastable());
                                await CommonSpellEffects.DealDirectDamage(action2, DiceFormula.FromText($"{curseDmg}", "Curse of Agony"), victim, CheckResult.Success, DamageKind.Mental);
                            }
                        });
                    });
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, ModTraits.Witch, Ability.Intelligence, Trait.Arcane).WithSpells(
                level1: new SpellId[] { SpellId.ChillTouch, SpellId.TrueStrike, SpellId.FlourishingFlora, SpellId.FlourishingFlora },
                level2: new SpellId[] { SpellId.TrueStrike, SpellId.TrueStrike }).Done();
        }
    }
}