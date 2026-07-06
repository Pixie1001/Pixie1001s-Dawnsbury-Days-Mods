using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury;
using Dawnsbury.Audio;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display;
using Dawnsbury.Display.Text;
using Dawnsbury.Modding;
using Microsoft.Xna.Framework;
using static Dawnsbury.Mods.Classes.Summoner.SummonerClassLoader;
using Dawnsbury.Core.Mechanics.Squeezing;

namespace Dawnsbury.Mods.Classes.Summoner {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class SummonerSpells {

        public enum SummonerSpellId {
            EvolutionSurge,
            EidolonBoost,
            ReinforceEidolon,
            ExtendBoost,
            LifelinkSurge,
            EidolonsWrath
        }

        public static Dictionary<SummonerSpellId, SpellId> LoadSpells() {
            Dictionary<SummonerSpellId, SpellId> spellList = new Dictionary<SummonerSpellId, SpellId>();

            spellList.Add(SummonerSpellId.EvolutionSurge, ModManager.RegisterNewSpell("EvolutionSurge", 1, (spellId, spellcaster, spellLevel, inCombat, spellInformation) => {
                return Spells.CreateModern(Enums.illEvolutionSurge, "Evolution Surge", new[] { Enums.tSummoner, Trait.Focus, Trait.Morph, Trait.Transmutation, Trait.Uncommon },
                        "You flood your eidolon with power, creating a temporary evolution in your eidolon's capabilities.",
                        "{b}Duration{/b} Until end of encounter.\n\nYour eidolon gains one the following adeptations for the rest of the encounter:\n\n• Your eidolon gains a swim speed.\n• Your eidolom gains a +20-foot status bonus to its speed." +
                        $"{(spellLevel >= 3 ? "\n• Your eidolon gains reach on all of its attacks." : "")}",
                        Target.DependsOnSpellVariant(varient => {
                            var baseTarget = Target.RangedFriend(20).WithAdditionalConditionOnTargetCreature(new EidolonCreatureTargetingRequirement(Enums.qfSummonerBond));
                            if (varient.Id == "Large") baseTarget.WithAdditionalConditionOnTargetCreature((a, d) => d.Space.SizeCategory >= 2 ? Usability.NotUsable("Your eidolon is already large or larger.") : Usability.Usable);
                            if (varient.Id == "Huge") baseTarget.WithAdditionalConditionOnTargetCreature((a, d) => d.Space.SizeCategory >= 3 ? Usability.NotUsable("Your eidolon is already huge or larger.") : Usability.Usable);
                            if (varient.Id == "Flight") baseTarget.WithAdditionalConditionOnTargetCreature((a, d) => d.QEffects.Any(qf => qf.Id == QEffectId.Flying && qf.Innate) ? Usability.NotUsable("Your eidolon can already fly.") : Usability.Usable);
                            if (varient.Id == "Amphibious") baseTarget.WithAdditionalConditionOnTargetCreature((a, d) => d.HasTrait(Trait.Aquatic) || d.HasEffect(QEffectId.Swimming) ? Usability.NotUsable("Your eidolon is already aquotic.") : Usability.Usable);

                            return baseTarget;
                        }),
                        spellLevel, null)
                    .WithSoundEffect(SfxName.Abjuration)
                    .WithHeightenedAtSpecificLevel(spellLevel, 3, inCombat, "Add the following option:\n• Your eidolon gains reach on all of its attacks.")
                    .WithVariants(new SpellVariant[] {
                        new SpellVariant("Amphibious", "Amphibious Evolution Surge", IllustrationName.ElementWater),
                        new SpellVariant("Agility", "Agility Evolution Surge", IllustrationName.FleetStep),
                        new SpellVariant("Large", "Enlarge Evolution Surge (Large)", IllustrationName.EnlargeCompanion),
                        new SpellVariant("Huge", "Enlarge Evolution Surge (Huge)", IllustrationName.EnlargeCompanion),
                        new SpellVariant("Flight", "Winged Evolution Surge", IllustrationName.Fly),
                    }.Where(v => !(spellLevel < 3 && v.Id == "Large") && !(spellLevel < 5 && (v.Id == "Huge" || v.Id == "Flight"))).ToArray())
                    .WithCreateVariantDescription((_, variant) => {
                        string text = "Until the end of the encounter, your eidolon ";
                        if (variant!.Id == "Amphibious") {
                            return text + "gains a swim speed.";
                        } else if (variant.Id == "Agility") {
                            return text + "gains a +20 feet status bonus to its speed.";
                        } else if (variant.Id == "Large") {
                            return text + "becomes Large, increasing its base reach to 10 feet.";
                        } else if (variant.Id == "Huge") {
                            return text + "becomes Huge, increasing its base reach to 15 feet.";
                        } else if (variant.Id == "Flight") {
                            return text + "gains a fly Speed equal to its Speed.";
                        }
                        return text;
                    })
                    .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                        SpellVariant variant = spell.ChosenVariant;
                        if (variant!.Id == "Amphibious") {
                            target.AddQEffect(new QEffect(
                            variant.Name, "Your eidolon gains a +20ft status bonus to its speed.",
                            ExpirationCondition.Never, caster, variant.Illustration) {
                                CountsAsABuff = true,
                                Id = QEffectId.Swimming
                            });
                        } else if (variant.Id == "Agility") {
                            target.AddQEffect(new QEffect(
                            variant.Name, "Your eidolon gains a +20 feet status bonus to its speed.",
                            ExpirationCondition.Never, caster, variant.Illustration) {
                                CountsAsABuff = true,
                                BonusToAllSpeeds = _ => new Bonus(4, BonusType.Status, "Evolution Surge")
                            });
                        } else if (variant.Id == "Large") {
                            if (await SizeChangeRules.EnlargeCreature(caster, target, Size.Large, IllustrationName.EnlargeCompanion, variant.Name,
                                    $"Your size is increased to Large.") is { } form) {
                                form.CountsAsABuff = true;
                                target.AddQEffect(new QEffect() {
                                    StateCheckWithVisibleChanges = async self => {
                                        if (!self.Owner.QEffects.Any(qf => qf == form) || !self.Owner.AliveOrUnconscious) {
                                            self.Owner.RemoveAllQEffects(qf => qf == form);
                                            self.ExpiresAt = ExpirationCondition.Immediately;
                                        }
                                    }
                                });
                            } else {
                                target.Overhead("nowhere to grow", Color.Red, $"{target.ToColoredBoldedName()} cannot be enlarged because there is no space to fit your eidolon's enlarged form.");
                                spell.RevertRequested = true;
                            }
                        } else if (variant.Id == "Huge") {
                            if (await SizeChangeRules.EnlargeCreature(caster, target, Size.Huge, IllustrationName.EnlargeCompanion, variant.Name,
                                    $"Your size is increased to Huge.") is { } form) {
                                form.CountsAsABuff = true;
                                target.AddQEffect(new QEffect() {
                                    StateCheckWithVisibleChanges = async self => {
                                        if (!self.Owner.QEffects.Any(qf => qf == form) || !self.Owner.AliveOrUnconscious) {
                                            form.ExpiresAt = ExpirationCondition.Immediately;
                                            self.ExpiresAt = ExpirationCondition.Immediately;
                                            if (self.Owner.QEffects.Any(qff => qff.Traits.Contains(Trait.SizeChangingEffect) && qff != form))
                                                return;
                                            await self.Owner.Space.GrowTo(self.Owner.Space.StandardSize);
                                        }
                                    }
                                });
                            } else {
                                target.Overhead("nowhere to grow", Color.Red, $"{target.ToColoredBoldedName()} cannot be enlarged because there is no space to fit your eidolon's enlarged form.");
                                spell.RevertRequested = true;
                            }
                        } else if (variant.Id == "Flight") {
                            target.AddQEffect(new QEffect(
                            variant.Name, "Your eidolon gains a fly speed.",
                            ExpirationCondition.Never, caster, variant.Illustration) {
                                CountsAsABuff = true,
                                Id = QEffectId.Flying
                            });
                        }
                    });
            }));

            spellList.Add(SummonerSpellId.EidolonBoost, ModManager.RegisterNewSpell("EidolonBoost", 1, (spellId, spellcaster, spellLevel, inCombat, spellInformation) => {
                return Spells.CreateModern(Enums.illEidolonBoost, "Eidolon Boost", new[] { Enums.tSummoner, Trait.Cantrip, Trait.Evocation, Trait.Uncommon },
                        "{b}Duration{/b} 1 round\n\nYou focus deeply on the link between you and your eidolon and boost the power of its attacks.",
                        "Your eidolon gains a +2 status bonus to damage rolls with its strikes.\n\n{b}Special.{/b} If your eidolon's Strikes deal more than one weapon damage die, the status bonus increases to 2 per weapon damage die, to a maximum of +8 with four weapon damage dice.",
                        Target.RangedFriend(20).WithAdditionalConditionOnTargetCreature((CreatureTargetingRequirement)new EidolonCreatureTargetingRequirement(Enums.qfSummonerBond)), spellLevel, null)
                    .WithSoundEffect(SfxName.Abjuration)
                    .WithEffectOnEachTarget((Delegates.EffectOnEachTarget)(async (spell, caster, target, result) => {
                        target.RemoveAllQEffects(qf => qf.Name == "Reinforce Eidolon");
                        QEffect buff = new QEffect("Eidolon Boost", "+2 status bonus to damage per damage die to strikes.") {
                            Key = "Eidolon Boost",
                            Source = caster,
                            CountsAsABuff = true,
                            Illustration = Enums.illEidolonBoost,
                            BonusToDamage = (qf, action, target) => {
                                if (!action.HasTrait(Trait.Strike)) {
                                    return null;
                                }
                                int dice = action.TrueDamageFormula!.ToString()[0] - '0';

                                return new Bonus(dice * 2, BonusType.Status, "Eidolon Boost");
                            },
                            ExpiresAt = ExpirationCondition.ExpiresAtStartOfSourcesTurn,
                        };
                        QEffect extender = caster.QEffects.FirstOrDefault(qf => qf.Id == Enums.qfExtendBoostExtender);
                        if (extender != null) {
                            buff.ExpiresAt = ExpirationCondition.CountsDownAtStartOfSourcesTurn;
                            buff.Value = extender.Value;
                            extender.ExpiresAt = ExpirationCondition.Immediately;
                        }
                        target.AddQEffect(buff);
                        if (caster.HasFeat(Enums.ftBoostSummons)) {
                            List<Creature> summons = caster.Battle.AllCreatures.Where(c => target.DistanceTo(c) <= 12 && c.QEffects.FirstOrDefault(qf => qf.Id == QEffectId.SummonedBy && qf.Source == caster) != null).ToList();
                            foreach (Creature creature in summons) {
                                creature.RemoveAllQEffects(qf => qf.Name == "Reinforce Eidolon");
                                QEffect buffCopy = new QEffect("Eidolon Boost", "+2 status bonus to damage per damage die to strikes.") {
                                    Key = "Eidolon Boost",
                                    Source = caster,
                                    CountsAsABuff = true,
                                    Illustration = Enums.illEidolonBoost,
                                    BonusToDamage = (qf, action, target) => {
                                        if (!action.HasTrait(Trait.Strike)) {
                                            return null;
                                        }
                                        int dice = action.TrueDamageFormula!.ToString()[0] - '0';

                                        return new Bonus(dice * 2, BonusType.Status, "Eidolon Boost");
                                    },
                                    ExpiresAt = buff.ExpiresAt,
                                    Value = buff.Value
                                };
                                creature.AddQEffect(buffCopy);
                            }
                        }
                    })).WithActionCost(1);
            }));

            spellList.Add(SummonerSpellId.ReinforceEidolon, ModManager.RegisterNewSpell("ReinforceEidolon", 1, (spellId, spellcaster, spellLevel, inCombat, spellInformation) => {
                return Spells.CreateModern(Enums.illReinforceEidolon, "Reinforce Eidolon", new[] { Enums.tSummoner, Trait.Cantrip, Trait.Abjuration, Trait.Uncommon },
                        "{b}Duration{/b} 1 round\n\nYou focus deeply on the link between you and your eidolon and reinforce your eidolon's defenses.",
                        "Your eidolon gains a +1 status bonus to AC and saving throws, plus resistance to all damage equal to half the spell's level.\n\n{b}Special.{/b} Your eidolon can benefit from either boost eidolon or reinforce eidolon, but not both; if you cast one of these spells during the other's duration, the newer spell replaces the older one.",
                        Target.RangedFriend(20).WithAdditionalConditionOnTargetCreature((CreatureTargetingRequirement)new EidolonCreatureTargetingRequirement(Enums.qfSummonerBond)), spellLevel, null)
                    .WithSoundEffect(SfxName.Abjuration)
                    .WithEffectOnEachTarget((Delegates.EffectOnEachTarget)(async (spell, caster, target, result) => {
                        target.RemoveAllQEffects(qfActTogether => qfActTogether.Name == "Eidolon Boost");
                        QEffect buff = new QEffect("Reinforce Eidolon", "+1 status bonus to AC and all saves." + (spellLevel > 1 ? " Plus resist " + spellLevel / 2 + " to all damage." : "")) {
                            Key = "Reinforce Eidolon",
                            Source = caster,
                            CountsAsABuff = true,
                            Illustration = Enums.illReinforceEidolon,
                            BonusToDefenses = (qf, action, target) => {
                                return new Bonus(1, BonusType.Status, "Reinforce Eidolon");
                            },
                            StateCheck = qfResistance => {
                                qfResistance.Owner.WeaknessAndResistance.AddResistanceAllExcept(Math.Max(1, spellLevel / 2), false, new DamageKind[] { });
                            },
                            ExpiresAt = ExpirationCondition.ExpiresAtStartOfSourcesTurn,
                        };
                        QEffect extender = caster.QEffects.FirstOrDefault(qf => qf.Id == Enums.qfExtendBoostExtender);
                        if (extender != null) {
                            buff.ExpiresAt = ExpirationCondition.CountsDownAtStartOfSourcesTurn;
                            buff.Value = extender.Value;
                            extender.ExpiresAt = ExpirationCondition.Immediately;
                        }
                        target.AddQEffect(buff);
                        if (caster.HasFeat(Enums.ftBoostSummons)) {
                            List<Creature> summons = caster.Battle.AllCreatures.Where(c => target.DistanceTo(c) <= 12 && c.QEffects.FirstOrDefault(qf => qf.Id == QEffectId.SummonedBy && qf.Source == caster) != null).ToList();
                            foreach (Creature creature in summons) {
                                creature.RemoveAllQEffects(qf => qf.Name == "Boost Eidolon");
                                QEffect buffCopy = new QEffect("Reinforce Eidolon", "+1 status bonus to AC and all saves." + (spellLevel > 1 ? " Plus resist " + spellLevel / 2 + " to all damage." : "")) {
                                    Key = "Reinforce Eidolon",
                                    Source = caster,
                                    CountsAsABuff = true,
                                    Illustration = Enums.illReinforceEidolon,
                                    BonusToDefenses = buff.BonusToDefenses,
                                    StateCheck = buff.StateCheck,
                                    ExpiresAt = buff.ExpiresAt,
                                    Value = buff.Value
                                };
                                creature.AddQEffect(buffCopy);
                            }
                        }
                    })).WithActionCost(1);
            }));

            spellList.Add(SummonerSpellId.LifelinkSurge, ModManager.RegisterNewSpell("LifelinkSurgeSpell", 2, (spellId, spellcaster, spellLevel, inCombat, spellInformation) => {
                return Spells.CreateModern(Enums.illLifeLink, "Lifelink Surge", new[] { Enums.tSummoner, Trait.Focus, Trait.Healing, Trait.Positive, Trait.Necromancy, Trait.Uncommon },
                        "You make a quick gesture, tracing the link between yourself and your eidolon and drawing on your connection to slowly strengthen your shared life force.",
                        $"Your eidolon gains fast healing {S.HeightenedVariable(spellLevel * 2, 4)} for 4 rounds, which causes it to heal {S.HeightenedVariable(spellLevel * 2, 4)} HP at the start of each of its turns.",
                        Target.RangedFriend(20).WithAdditionalConditionOnTargetCreature((CreatureTargetingRequirement)new EidolonCreatureTargetingRequirement(Enums.qfSummonerBond)), spellLevel, null)
                    .WithSoundEffect(SfxName.Healing)
                    .WithHeighteningNumerical(spellLevel, 2, inCombat, 1, "The fast healing increases by 2.")
                    .WithEffectOnEachTarget((Delegates.EffectOnEachTarget)(async (spell, caster, target, result) => {
                        target.AddQEffect(new QEffect("Lifelink Boost", $"You gain Fast Healing {spellLevel * 2}.") {
                            Value = 4,
                            Source = caster,
                            Illustration = Enums.illLifeLink,
                            Key = "LifeLinkSurge",
                            StartOfYourPrimaryTurn = (async (qf, self) => {
                                await self.HealAsync($"{spellLevel * 2}", spell);
                            }),
                            ExpiresAt = ExpirationCondition.CountsDownAtStartOfSourcesTurn,
                        });
                    })).WithActionCost(1);
            }));

            spellList.Add(SummonerSpellId.ExtendBoost, ModManager.RegisterNewSpell("ExtendBoostSpell", 1, (spellId, spellcaster, spellLevel, inCombat, spellInformation) => {
                return Spells.CreateModern(Enums.illExtendBoost, "Extend Boost", new[] { Enums.tSummoner, Trait.Focus, Trait.Metamagic, Trait.Divination, Trait.Uncommon },
                        "You focus on the intricacies of the magic binding you to your eidolon to extend the duration of your boost eidolon or reinforce eidolon spell.",
                        "If your next action is to cast boost eidolon or reinforce eidolon, attempt a skill check with the skill associated with the tradition of magic you gain from your eidolon (such as Nature for a primal eidolon) vs. a standard-difficulty DC of your level. The effect depends on the result of your check.\n\n{b}Critical success{/b} The spell lasts 4 rounds.\n{b}Success{/b} The spell lasts 3 rounds.\n{b}Failure{/b} The spell lasts 1 round, but you don't spend the Focus Point for casting this spell.",
                        Target.Self(), spellLevel, null)
                    .WithSoundEffect(SfxName.MinorAbjuration)
                    .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                        target.AddQEffect(new QEffect("Extend Boost Toggled", "The duration of the next Boost Eidolon or Reinforce Eidolon cantrip you cast will be extended.") {
                            Illustration = Enums.illExtendBoost,
                            YouBeginAction = async (qf, action) => {
                                if (spellcaster == null) return;
                                if (action.SpellId == spellList[SummonerSpellId.EidolonBoost] || action.SpellId == spellList[SummonerSpellId.ReinforceEidolon]) {
                                    CheckResult result = CommonSpellEffects.RollCheck("Extend Boost Check", new ActiveRollSpecification(TaggedChecks.SkillCheck(SpellTraditionToSkill(qf.Owner.PersistentCharacterSheet!.Calculated.SpellRepertoires[Enums.tSummoner].SpellList)), Checks.FlatDC(Checks.LevelBasedDC(qf.Owner.Level))), qf.Owner, qf.Owner);
                                    int duration = 0;
                                    if (result == CheckResult.Failure) {
                                        spellcaster.Spellcasting?.FocusPoints += 1;
                                        GetEidolon(spellcaster)?.Spellcasting?.FocusPoints += 1;
                                        spellcaster.Overhead("Focus point refunded", Color.Green);
                                    } else if (result == CheckResult.Success) {
                                        duration = 3;
                                    } else if (result == CheckResult.CriticalSuccess) {
                                        duration = 4;
                                    }

                                    if (duration > 0) {
                                        target.AddQEffect(new QEffect() {
                                            Id = Enums.qfExtendBoostExtender,
                                            Value = duration
                                        });
                                    }
                                } else {
                                    spellcaster.Spellcasting?.FocusPoints += 1;
                                    GetEidolon(spellcaster)?.Spellcasting?.FocusPoints += 1;
                                    spellcaster.Overhead("Focus point refunded", Color.Green);
                                }
                                qf.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        });

                    }).WithActionCost(0);
            }));

            spellList.Add(SummonerSpellId.EidolonsWrath, ModManager.RegisterNewSpell("EidolonsWrath", 3, (spellId, spellcaster, spellLevel, inCombat, spellInformation) => {
                return Spells.CreateModern(IllustrationName.DivineWrath, "Eidolon's Wrath", new[] { Enums.tSummoner, Trait.Focus, Trait.Evocation, Trait.Uncommon },
                        "",
                        $"Your eidolon releases a powerful energy attack that deals {S.HeightenedVariable(spellLevel * 2 - 1, 5)}d6 " +
                        (spellcaster != null && spellcaster.HasEffect(Enums.qfEidolonsWrath) ? HumanizeDamageKind((DamageKind)spellcaster.FindQEffect(Enums.qfEidolonsWrath)?.Tag!) + " damage" : "damage of the type chosen when you took the Eidolon's Wrath feat") + ".",
                        new EmanationTarget(4, false), spellLevel, SpellSavingThrow.Basic(Defense.Reflex))
                .WithNoSaveFor((a, c) => c.Destroyed ? true : true )    
                .WithSoundEffect(SfxName.Fireball)
                .WithHeighteningOfDamageEveryLevel(spellLevel, 3, inCombat, "2d6")
                .WithEffectOnEachTarget(async (spell, caster, target, _) => {
                    if (caster == null || spellcaster == null || spellcaster.HasEffect(Enums.qfEidolonsWrath) == false) {
                        return;
                    }
                    CheckResult result = await CommonSpellEffects.RollSavingThrowAsync(target, spell, Defense.Fortitude, GetSummoner(caster)!.ClassOrSpellDC());
                    DamageKind dk = (DamageKind)caster.FindQEffect(Enums.qfEidolonsWrath)?.Tag!;
                    await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, DiceFormula.FromText($"{2 * spellLevel - 1}d6"), dk);
                })
                .WithActionCost(2);
            }));

            // Add new spells HERE


            return spellList;
        }

        internal static Skill SpellTraditionToSkill(Trait tradition) {
            switch (tradition) {
                case Trait.Divine:
                    return Skill.Religion;
                case Trait.Arcane:
                    return Skill.Arcana;
                case Trait.Primal:
                    return Skill.Nature;
                case Trait.Occult:
                    return Skill.Occultism;
                default:
                    return Skill.Society;
            }
        }

        internal static Spell TraditionToSpell(Trait tradition, int spellLevel) {
            if (spellLevel == 1) {
                switch (tradition) {
                    case Trait.Arcane:
                        return AllSpells.CreateModernSpellTemplate(SpellId.MageArmor, Enums.tSummoner, spellLevel);
                    case Trait.Divine:
                        return AllSpells.CreateModernSpellTemplate(SpellId.Bless, Enums.tSummoner, spellLevel);
                    case Trait.Primal:
                        return AllSpells.CreateModernSpellTemplate(SpellId.Grease, Enums.tSummoner, spellLevel);
                    case Trait.Occult:
                        return AllSpells.CreateModernSpellTemplate(SpellId.Fear, Enums.tSummoner, spellLevel);
                    default:
                        throw new Exception("Summoner Class Mod: Invalid spell casting tradition");
                }
            }
            switch (tradition) {
                case Trait.Arcane:
                    return AllSpells.CreateModernSpellTemplate(SpellId.Blur, Enums.tSummoner, spellLevel);
                case Trait.Divine:
                    return AllSpells.CreateModernSpellTemplate(SpellId.BloodVendetta, Enums.tSummoner, spellLevel);
                case Trait.Primal:
                    return AllSpells.CreateModernSpellTemplate(SpellId.Barkskin, Enums.tSummoner, spellLevel);
                case Trait.Occult:
                    return AllSpells.CreateModernSpellTemplate(SpellId.HideousLaughter, Enums.tSummoner, spellLevel);
                default:
                    throw new Exception("Summoner Class Mod: Invalid spell casting tradition");
            }
        }

        internal static string HumanizeDamageKind(DamageKind damageKind) {
            return damageKind.HumanizeTitleCase2();
        }

    }
}
