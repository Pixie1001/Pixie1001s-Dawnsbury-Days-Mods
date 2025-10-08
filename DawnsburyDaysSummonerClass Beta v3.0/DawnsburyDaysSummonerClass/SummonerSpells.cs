using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Description;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Dawnsbury.ThirdParty.SteamApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Dawnsbury.Mods.Classes.Summoner.SummonerClassLoader;

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



            //.WithVariants(MonsterStatBlocks.MonsterExemplars.Where(animal => animal.HasTrait(Trait.Elemental) && animal.Level <= elementalLevel)
            //    .Select(animal => new SpellVariant(animal.Name, "Summon " + animal.Name + " (level " + animal.Level.ToString() + ")", animal.Illustration)).ToArray())
            //    .WithCreateVariantDescription((Func<int, SpellVariant, string>)((_, variant) => RulesBlock.CreateCreatureDescription(MonsterStatBlocks.MonsterExemplarsByName[variant.Id]))).WithEffectOnChosenTargets((Delegates.EffectOnChosenTargets)(async (spell, caster, targets) => await CommonSpellEffects.SummonMonster(spell, caster, targets.ChosenTile)));


            spellList.Add(SummonerSpellId.EvolutionSurge, ModManager.RegisterNewSpell("EvolutionSurge", 1, (spellId, spellcaster, spellLevel, inCombat, spellInformation) => {
                return Spells.CreateModern(Enums.illEvolutionSurge, "Evolution Surge", new[] { Enums.tSummoner, Trait.Focus, Trait.Morph, Trait.Transmutation, Trait.Uncommon },
                        "You flood your eidolon with power, creating a temporary evolution in your eidolon's capabilities.",
                        "{b}Duration{/b} Until end of encounter.\n\nYour eidolon gains one the following adeptations for the rest of the encounter:\n\n• Your eidolon gains a swim speed.\n• Your eidolom gains a +20-foot status bonus to its speed." +
                        $"{(spellLevel >= 3 ? "\n• Your eidolon gains reach on all of its attacks." : "")}",
                        Target.RangedFriend(20).WithAdditionalConditionOnTargetCreature((CreatureTargetingRequirement)new EidolonCreatureTargetingRequirement(Enums.qfSummonerBond)), spellLevel, null)
                    .WithSoundEffect(SfxName.Abjuration)
                    .WithHeightenedAtSpecificLevel(spellLevel, 3, inCombat, "Add the following option:\n• Your eidolon gains reach on all of its attacks.")
                    .WithVariants(new SpellVariant[] {
                        new SpellVariant("Amphibious", "Amphibious Evolution Surge", (Illustration) IllustrationName.ElementWater),
                        new SpellVariant("Agility", "Agility Evolution Surge", (Illustration) IllustrationName.FleetStep),
                        new SpellVariant("Enlarge", "Enlarge Evolution Surge", (Illustration) IllustrationName.SummonAnimal),
                    }.Where(v => (spellLevel < 3 && v.Id != "Enlarge") || spellLevel >= 3).ToArray())
                    .WithCreateVariantDescription((_, variant) => {
                        string text = "Until the end of the encounter, your eidolon ";
                        if (variant.Id == "Amphibious") {
                            return text + "gains a swim speed.";
                        } else if (variant.Id == "Agility") {
                            return text + "gains a +20ft status bonus to its speed.";
                        } else if (variant.Id == "Enlarge") {
                            return text + "gains reach on all its attacks.";
                        }
                        return text;
                    })
                    .WithEffectOnEachTarget((Delegates.EffectOnEachTarget)(async (spell, caster, target, result) => {
                        SpellVariant variant = spell.ChosenVariant;
                        if (variant.Id == "Amphibious") {
                            target.AddQEffect(new QEffect(
                            variant.Name, "Your eidolon gains a +20ft status bonus to its speed.",
                            ExpirationCondition.Never, caster, variant.Illustration) {
                                CountsAsABuff = true,
                                Id = QEffectId.Swimming
                            });
                        } else if (variant.Id == "Agility") {
                            target.AddQEffect(new QEffect(
                            variant.Name, "Your eidolon gains a +20ft status bonus to its speed.",
                            ExpirationCondition.Never, caster, variant.Illustration) {
                                CountsAsABuff = true,
                                BonusToAllSpeeds = ((Func<QEffect, Bonus>)(_ => new Bonus(4, BonusType.Status, "Evolution Surge")))
                            });
                        } else if (variant.Id == "Enlarge") {
                            target.AddQEffect(new QEffect(
                            variant.Name, "Your eidolon gains reach on all its attacks.",
                            ExpirationCondition.Never, caster, variant.Illustration) {
                                CountsAsABuff = true,
                                Tag = false,
                                StateCheckWithVisibleChanges = async self => {
                                    bool triggered = (bool) self.Tag;
                                    if (!triggered) {
                                        self.Tag = true;
                                        self.Owner.UnarmedStrike.Traits.Add(Trait.Reach);
                                        foreach (QEffect attack in self.Owner.QEffects.Where(qf => qf.AdditionalUnarmedStrike != null && qf.AdditionalUnarmedStrike.WeaponProperties.Melee)) {
                                            attack.AdditionalUnarmedStrike.Traits.Add(Trait.Reach);
                                        }
                                    }
                                },
                                WhenExpires = self => {
                                    self.Owner.UnarmedStrike.Traits.Remove(Trait.Reach);
                                    foreach (QEffect attack in self.Owner.QEffects.Where(qf => qf.AdditionalUnarmedStrike != null && qf.AdditionalUnarmedStrike.WeaponProperties.Melee)) {
                                        attack.AdditionalUnarmedStrike.Traits.Remove(Trait.Reach);
                                    }
                                }
                            });
                        }
                    }));
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
                                int dice = action.TrueDamageFormula.ToString()[0] - '0';

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
                                        int dice = action.TrueDamageFormula.ToString()[0] - '0';

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
                    .WithEffectOnEachTarget((Delegates.EffectOnEachTarget)(async (spell, caster, target, result) => {
                        target.AddQEffect(new QEffect("Extend Boost Toggled", "The duration of the next Boost Eidolon or Reinforce Eidolon cantrip you cast will be extended.") {
                            Illustration = Enums.illExtendBoost,
                            YouBeginAction = (async (qf, action) => {
                                if (action.SpellId != null && (action.SpellId == spellList[SummonerSpellId.EidolonBoost] || action.SpellId == spellList[SummonerSpellId.ReinforceEidolon])) {
                                    CheckResult result = CommonSpellEffects.RollCheck("Extend Boost Check", new ActiveRollSpecification(Checks.SkillCheck(SpellTraditionToSkill(qf.Owner.PersistentCharacterSheet.Calculated.SpellRepertoires[Enums.tSummoner].SpellList)), Checks.FlatDC(GetDCByLevel(qf.Owner.Level))), qf.Owner, qf.Owner);
                                    int duration = 0;
                                    if (result == CheckResult.Failure) {
                                        spellcaster.Spellcasting.FocusPoints += 1;
                                        GetEidolon(spellcaster).Spellcasting.FocusPoints += 1;
                                        spellcaster.Occupies.Overhead("Focus point refunded", Color.Green);
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
                                    spellcaster.Spellcasting.FocusPoints += 1;
                                    GetEidolon(spellcaster).Spellcasting.FocusPoints += 1;
                                    spellcaster.Occupies.Overhead("Focus point refunded", Color.Green);
                                }
                                qf.ExpiresAt = ExpirationCondition.Immediately;
                            })
                        });

                    })).WithActionCost(0);
            }));

            spellList.Add(SummonerSpellId.EidolonsWrath, ModManager.RegisterNewSpell("EidolonsWrath", 3, (spellId, spellcaster, spellLevel, inCombat, spellInformation) => {
                return Spells.CreateModern(IllustrationName.DivineWrath, "Eidolon's Wrath", new[] { Enums.tSummoner, Trait.Focus, Trait.Evocation, Trait.Uncommon },
                        "",
                        $"Your eidolon releases a powerful energy attack that deals {S.HeightenedVariable(spellLevel * 2 - 1, 5)}d6 " +
                        (spellcaster != null && spellcaster.HasEffect(Enums.qfEidolonsWrath) ? HumanizeDamageKind((DamageKind)spellcaster.FindQEffect(Enums.qfEidolonsWrath).Tag) + " damage" : "damage of the type chosen when you took the Eidolon's Wrath feat") + ".",
                        new EmanationTarget(4, false), spellLevel, SpellSavingThrow.Basic(Defense.Reflex))
                .WithNoSaveFor((a, c) => c.Destroyed ? true : true )    
                .WithSoundEffect(SfxName.Fireball)
                .WithHeighteningOfDamageEveryLevel(spellLevel, 3, inCombat, "2d6")
                .WithEffectOnEachTarget((Delegates.EffectOnEachTarget)(async (spell, caster, target, _) => {
                    if (caster == null || spellcaster.HasEffect(Enums.qfEidolonsWrath) == false) {
                        return;
                    }
                    CheckResult result = CommonSpellEffects.RollSavingThrow(target, spell, Defense.Fortitude, GetSummoner(caster).ClassOrSpellDC());
                    DamageKind dk = (DamageKind)caster.FindQEffect(Enums.qfEidolonsWrath).Tag;
                    await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, DiceFormula.FromText($"{2 * spellLevel - 1}d6"), dk);
                }))
                .WithActionCost(2);
            }));

            // Add new spells HERE


            return spellList;
        }

        internal static Skill SpellTraditionToSkill(Trait tradition) {
            switch (tradition) {
                case Trait.Divine:
                    return Skill.Religion;
                    break;
                case Trait.Arcane:
                    return Skill.Arcana;
                    break;
                case Trait.Primal:
                    return Skill.Nature;
                    break;
                case Trait.Occult:
                    return Skill.Occultism;
                    break;
                default:
                    return Skill.Society;
            }
        }

        internal static Spell TraditionToSpell(Trait tradition, int spellLevel) {
            if (spellLevel == 1) {
                switch (tradition) {
                    case Trait.Arcane:
                        return AllSpells.CreateModernSpellTemplate(SpellId.MageArmor, Enums.tSummoner, spellLevel);
                        break;
                    case Trait.Divine:
                        return AllSpells.CreateModernSpellTemplate(SpellId.Bless, Enums.tSummoner, spellLevel);
                        break;
                    case Trait.Primal:
                        return AllSpells.CreateModernSpellTemplate(SpellId.Grease, Enums.tSummoner, spellLevel);
                        break;
                    case Trait.Occult:
                        return AllSpells.CreateModernSpellTemplate(SpellId.Fear, Enums.tSummoner, spellLevel);
                        break;
                    default:
                        throw new Exception("Summoner Class Mod: Invalid spell casting tradition");
                        return null;
                }
            }
            switch (tradition) {
                case Trait.Arcane:
                    return AllSpells.CreateModernSpellTemplate(SpellId.Blur, Enums.tSummoner, spellLevel);
                    break;
                case Trait.Divine:
                    return AllSpells.CreateModernSpellTemplate(SpellId.BloodVendetta, Enums.tSummoner, spellLevel);
                    break;
                case Trait.Primal:
                    return AllSpells.CreateModernSpellTemplate(SpellId.Barkskin, Enums.tSummoner, spellLevel);
                    break;
                case Trait.Occult:
                    return AllSpells.CreateModernSpellTemplate(SpellId.HideousLaughter, Enums.tSummoner, spellLevel);
                    break;
                default:
                    throw new Exception("Summoner Class Mod: Invalid spell casting tradition");
                    return null;
            }
        }

        internal static string HumanizeDamageKind(DamageKind damageKind) {
            return damageKind.HumanizeTitleCase2();
        }

        internal static int GetDCByLevel(int level) {
            switch (level) {
                case 0:
                    return 14;
                    break;
                case 1:
                    return 15;
                    break;
                case 2:
                    return 16;
                    break;
                case 3:
                    return 18;
                    break;
                case 4:
                    return 19;
                    break;
                case 5:
                    return 20;
                    break;
                case 6:
                    return 22;
                    break;
                case 7:
                    return 23;
                    break;
                case 8:
                    return 24;
                    break;
                default:
                    throw new Exception("ERROR: Invalid player level.");
                    return 30;
            }
        }
    }
}
