using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
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
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Intrinsics.Arm;
using System.Xml;
using Dawnsbury.Core.Mechanics.Damage;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;
using System.Text;
using static Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.BarbarianFeatsDb.AnimalInstinctFeat;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Metrics;
using Microsoft.Xna.Framework.Audio;
using static System.Reflection.Metadata.BlobBuilder;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Animations.Movement;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using System.IO;
using static HarmonyLib.Code;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Core.CharacterBuilder.Selections.Selected;
using System.Xml.Linq;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class FeatLoader {

        public static FeatName RatMonarchDedication { get; } = ModManager.RegisterFeatName("Rat Monarch Dedication");
        public static FeatName PlagueRats { get; } = ModManager.RegisterFeatName("Plague Rats");
        public static FeatName SwarmLord { get; } = ModManager.RegisterFeatName("Swarm Lord");
        public static FeatName PowerOfTheRatFiend { get; } = ModManager.RegisterFeatName("Power of the Rat Fiend");
        public static FeatName NightmareDomain { get; } = ModManager.RegisterFeatName("Nightmares");

        internal static void LoadFeats() {
            AddFeats(CreateFeats());
        }

        private static void AddFeats(IEnumerable<Feat> feats) {
            foreach (Feat feat in feats) {
                ModManager.AddFeat(feat);
            }
        }

        private static IEnumerable<Feat> CreateFeats() {
            List<Trait> classTraits = new List<Trait>();
            AllFeats.All.ForEach(ft => {
                if (ft is ClassSelectionFeat classFeat) {
                    classTraits.Add(classFeat.ClassTrait);
                }
            });

            var nightmareDomain = ClericClassFeatures.CreateDomain(NightmareDomain, "You fill minds with horror and dread.", SpellLoader.WakingNightmare, SpellLoader.SharedNightmare);
            nightmareDomain.Traits.Add(ModTraits.Roguelike);
            ClericClassFeatures.AllDomainFeats.Add(nightmareDomain);

            //AllFeats.All.Find(ft => ft.FeatName == FeatName.AdvancedDomain).Subfeats.Add(CreateAdvancedDomainFeat(Trait.Cleric, nightmareDomain));
            //AllFeats.All.Find(ft => ft.FeatName == FeatName.AdvancedDeitysDomain).Subfeats.Add(CreateAdvancedDomainFeat(Trait.Champion, nightmareDomain));
            //AllFeats.All.Find(ft => ft.FeatName == FeatName.DomainFluency).Subfeats.Add(CreateAdvancedDomainFeat(Trait.Oracle, nightmareDomain));
            yield return nightmareDomain;

            yield return CreateAdvancedDomainFeat(Trait.Cleric, nightmareDomain);
            yield return CreateAdvancedDomainFeat(Trait.Champion, nightmareDomain);
            yield return CreateAdvancedDomainFeat(Trait.Oracle, nightmareDomain);

            var echidnaDiety = new DeitySelectionFeat(ModManager.RegisterFeatName("Diety: The Echidna"),
                "While the other deities busied themselves crafting the sky, peoples and land of Our Point of Light, The The Echidna refused to collaborate with their efforts, instead crafting all manner of unusual and grotesque" +
                " creations to populate the plane. Whilst The The Echidna's followers insists that her creation are just as valuable as any other creature, with many unique and" +
                " exciting characteristics that exemplify their goddess' passion and creativity, their often violent and predatory nature is a topic of great controversy that makes The The Echidna a particularly unpopular deity." +
                "\n\nDespite these differences, The The Echidna has always been quick to offer succour to outcasts and pariahs, insisting that all creatures should be celebrated for their differences, regardless of their heritage or" +
                " unsightly ailments. She is also often worshipped as a goddess of fertility, or invoked by scholars to seek twisted wisdom from nightmares.",
                "{b}• Edicts{/b} Find beauty and cuteness in The The Echidna's cherished creations, bring power to outcasts and the downtrodden, reveal the corruption and flaws in those with the hubris to who claim perfection.\n" +
                "{b}• Anathema{/b} Ostracise another for a deforminity or mental illness, contribute to the extinction of endangered monstrous species.",
                [NineCornerAlignment.ChaoticEvil, NineCornerAlignment.ChaoticNeutral, NineCornerAlignment.ChaoticGood, NineCornerAlignment.NeutralEvil],
                [FeatName.HealingFont, FeatName.HarmfulFont], [FeatName.DomainFamily, FeatName.DomainMight, NightmareDomain], ItemName.Ranseur, [SpellId.MagicFang, SpellId.SummonAnimal, SpellId.AnimalForm, SpellId.Confusion], Skill.Intimidation);

            echidnaDiety.Traits.Add(ModTraits.Roguelike);

            AllFeats.All.Find(ft => ft.FeatName == FeatName.Cleric).Subfeats.Add(echidnaDiety);
            yield return echidnaDiety;

            yield return new Feat(PowerOfTheRatFiend, null, "", [], null);

            yield return new TrueFeat(RatMonarchDedication, 2, "The Rat Monarch lords over their flock of rodents, that emerge from the most neglected corners of the abyss to fulfill their master's will.",
                "You gain the following actions, allowing to summon forth and direct a swarm of vicious rats:\n\n" +
                "{b}Call Rats {icon:TwoActions}.{/b}\n" +
                "{b}Range{/b} self\n\nSpawn 2 friendly rat familiars into the nearest unoccupied space available to you." +
                "Your familiars have the base statistics of a Giant Rat, but their level is equal to your own -2, adjusting their defences and attack bonuses and increasing their max HP by 3 per level.\n\n" +
                "{b}Command Swarm {icon:Action}.{/b}\n" +
                "{b}Range{/b} 30 feet\n{b}Target{/b} 1 enemy creature\n\n" +
                "You mark the target creature with a demonic curse, attracting your rat familiars to them. While marked, your familiars gain a +1 status bonus to attack, and a +2 damage bonus against the target and are more likely to attack them.",
                new Trait[] { ModTraits.Archetype, ModTraits.Dedication, ModTraits.Event, ModTraits.Roguelike }.Concat(classTraits).ToArray(), null)
            .WithOnCreature((sheet, creature) => {
                creature.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        var action = (ActionPossibility)new CombatAction(self.Owner, IllustrationName.SummonAnimal, "Call Rats", new Trait[] { Trait.Conjuration, Trait.Manipulate },
                            "{b}Range{/b} self\n\nSpawn 2 friendly rat familiars into the nearest unoccupied space available to you.\n\n" +
                            "Your familiars have the base statistics of a Giant Rat, but their level is equal to your own -2, adjusting their defences and attack bonuses and increasing their max HP by 3 per level.", Target.Self()) {
                            ShortDescription = "Summoner 3 familirs into nearby unoccupied spaces."
                        }
                        .WithActionCost(2)
                        .WithSoundEffect(SfxName.ScratchFlesh)
                        .WithEffectOnSelf(async (action, caster) => {
                            for (int i = 0; i <= 1; i++) {
                                SpawnRatFamiliar(self.Owner);
                            }
                        });
                        action.PossibilityGroup = "Rat Monarch";
                        action.PossibilitySize = PossibilitySize.Half;
                        return action;
                    },
                });

                creature.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        var action = (ActionPossibility)new CombatAction(self.Owner, IllustrationName.Command, "Command Swarm", new Trait[] { Trait.Manipulate },
                            "{b}Range{/b} 30 feet\n{b}Target{/b} 1 enemy creature\n\n" +
                            "You mark the target creature with a demonic curse, attracting your rat familiars to them. While marked, your familiars gain a +1 status bonus to attack, and a +2 damage bonus against the target and are more likely to attack them.", Target.Ranged(6)) {
                            ShortDescription = "Command your rat familiars to attack the target creature, granting them a +1 status bonus to attack rolls, and a +2 bonus to damage against them until you move the mark."
                        }
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.ScratchFlesh)
                        .WithEffectOnEachTarget(async (action, caster, target, result) => {
                            caster.Battle.AllCreatures.ForEach(cr => cr.RemoveAllQEffects(qf => qf.Id == QEffectIds.CommandSwarm && qf.Source == caster));
                            QEffect effect = new QEffect("Marked for the Swarm",
                                $"{caster.Name}'s rat familiars are drawn to you, causing them to deal +2 damage and gain a +1 bonus to attack rolls.",
                                ExpirationCondition.Never, caster, IllustrationName.GiantRat256) { Id = QEffectIds.CommandSwarm };
                            effect.AddGrantingOfTechnical(cr => cr.QEffects.Any(qf => qf.Id == QEffectIds.RatFamiliar && qf.Source == caster), qfMark => {
                                qfMark.AdditionalGoodness = (self, action, d) => d.QEffects.Any(qf => qf.Id == QEffectIds.CommandSwarm && qf.Source == caster) ? 20f : -5f;
                                qfMark.BonusToAttackRolls = (self, action, d) => d != null && d.QEffects.Any(qf => qf.Id == QEffectIds.CommandSwarm && qf.Source == caster) ? new Bonus(1, BonusType.Status, "Marked for the Swarm", true) : null;
                                qfMark.BonusToDamage = (self, action, d) => action.HasTrait(Trait.Strike) && d.QEffects.Any(qf => qf.Id == QEffectIds.CommandSwarm && qf.Source == caster) ? new Bonus(2, BonusType.Untyped, "Marked for the Swarm", true) : null;
                            });
                            target.AddQEffect(effect);
                        });
                        action.PossibilityGroup = "Rat Monarch";
                        action.PossibilitySize = PossibilitySize.Half;
                        return action;
                    },
                });
            })
            .WithPrerequisite(sheet => {
                //if (CampaignState.Instance?.AdventurePath?.Name == "Roguelike Mode" && CampaignState.Instance.Heroes.First(h => h.CharacterSheet == sheet.Sheet).LongTermEffects.Effects.Any(lte => lte.Id.ToStringOrTechnical() == "Power of the Rat Fiend")) {
                if (sheet.Sheet.SelectedFeats.TryGetValue("Power of the Rat Fiend", out SelectedChoice val)) {
                    return true;
                } else {
                    return false;
                }
            }, "This archetype can only be taken by one who has stolen the power of the Rat Fiend");

            yield return new TrueFeat(PlagueRats, 4, "Your rats grow larger and more vicious, carrying a wasting otherwordly plague.",
                "Your rat familiars gain +5 HP and inflict Rat Plague on a successful jaws attack, an affliction with a DC equal to the higher of your class or spell DC.\n\n{b}Rat Plague{/b}\n{b}Stage 1{/b} 1d6 poison damage and enfeebled 1; {b}Stage 2{/b} 2d6 poison damage and enfeebled 2; {b}Stage 3{/b} 3d6 poison damage and enfeebled 2.",
                new Trait[] { ModTraits.Archetype, ModTraits.Event, ModTraits.Roguelike }.Concat(classTraits).ToArray(), null)
            .WithPrerequisite(sheet => sheet.HasFeat(RatMonarchDedication) ? true : false, "Requires the Rat Monarch Dedication");

            yield return new TrueFeat(SwarmLord, 4, "You and your rats fight as one.",
                "When you hit a creature with an attack roll, each adjacent rat familiar may use its reaction to make a jaws attack against them. Attacks triggered by ranged attacks suffer a -2 penalty.",
                new Trait[] { ModTraits.Archetype, ModTraits.Event, ModTraits.Roguelike }.Concat(classTraits).ToArray(), null)
            .WithOnCreature((sheet, creature) => {
                creature.AddQEffect(new QEffect("Swarm Lord", "When you hit a creature with an attack roll, each adjacent rat familiar may use its reaction to make a jaws attack against them. Attacks triggered by ranged attacks suffer a -2 penalty.") {
                    AfterYouTakeActionAgainstTarget = async (self, action, target, result) => {
                        if (!action.HasTrait(Trait.Attack) || result <= CheckResult.Failure) {
                            return;
                        }
                        bool ranged = !self.Owner.IsAdjacentTo(target) && (action.Target is not CreatureTarget || action.Target is CreatureTarget ct && ct.RangeKind == RangeKind.Ranged);

                        foreach (Creature rat in self.Owner.Battle.AllCreatures.Where(cr => cr.QEffects.Any(qf => qf.Id == QEffectIds.RatFamiliar && qf.Source == self.Owner) && cr.IsAdjacentTo(target))) {
                            if (!await rat.AskToUseReaction("Would you like to attack the target of your master's attack?")) {
                                return;
                            }
                            
                            StrikeModifiers mod = new StrikeModifiers();
                            if (ranged) {
                                mod.AdditionalBonusesToAttackRoll = new List<Bonus>() { new Bonus(-2, BonusType.Untyped, "Ranged Trigger") };
                            }
                            CombatAction ca = rat.CreateStrike(rat.UnarmedStrike, 0, mod);
                            ca.ChosenTargets.ChosenCreature = target;
                            ca.ChosenTargets.ChosenCreatures.Add(target);
                            await ca.AllExecute();
                        }
                    }
                });
            })
            .WithPrerequisite(sheet => sheet.HasFeat(RatMonarchDedication) ? true : false, "Requires the Rat Monarch Dedication");
        }

        //internal static Feat CreateAdvancedDomainFeat(Trait forClass, Feat domainFeat) {
        //    var name = domainFeat.FeatName.HumanizeTitleCase2();
        //    var advancedSpell = (SpellId)domainFeat.Tag!;
        //    //var spell = AllSpells.All.Concat((List<SpellId>)typeof(ModManager).GetProperty("NewSpells", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null)).FirstOrDefault(spell => spell.SpellId == advancedSpell);
        //    var spell = AllSpells.CreateModernSpellTemplate(advancedSpell, forClass);
        //    return new Feat(ModManager.RegisterFeatName("AdvancedDomain:" + forClass.HumanizeTitleCase2() + ":" + name, name + ": " + spell.Name), "Your studies or prayers have unlocked deeper secrets of the " + name.ToLower() + " domain.",
        //        $"You learn the {forClass.HumanizeTitleCase2().ToLower()} focus spell " + AllSpells.CreateSpellLink(advancedSpell, forClass) + ", and you gain 1 focus point, up to a maximum 3.", [], null)
        //    .WithIllustration(spell.Illustration)
        //    .WithRulesBlockForSpell(advancedSpell, forClass)
        //    .WithPrerequisite(values => values.HasFeat(domainFeat.FeatName), "You must have the " + name + " domain.")
        //    .WithOnSheet(sheet => {
        //        if (sheet.Sheet.Class?.ClassTrait == Trait.Cleric) {
        //            sheet.AddFocusSpellAndFocusPoint(Trait.Cleric, Ability.Wisdom, advancedSpell);
        //        } else if (sheet.Sheet.Class?.ClassTrait == Trait.Champion) {
        //            sheet.AddFocusSpellAndFocusPoint(Trait.Champion, Ability.Charisma, advancedSpell);
        //        } else if (sheet.Sheet.Class?.ClassTrait == Trait.Oracle) {
        //            sheet.AddFocusSpellAndFocusPoint(Trait.Oracle, Ability.Charisma, advancedSpell);
        //        }
        //    });
        //}

        internal static Feat CreateAdvancedDomainFeat(Trait forClass, Feat domainFeat) {
            var name = domainFeat.FeatName.HumanizeTitleCase2();
            var advancedSpell = (SpellId)domainFeat.Tag!;
            //var spell = AllSpells.All.Concat((List<SpellId>)typeof(ModManager).GetProperty("NewSpells", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null)).FirstOrDefault(spell => spell.SpellId == advancedSpell);
            var spell = AllSpells.CreateModernSpellTemplate(advancedSpell, forClass);
            return new TrueFeat(ModManager.RegisterFeatName("AdvancedDomain:" + forClass.HumanizeTitleCase2() + ":" + name, name + ": " + spell.Name), 8, "Your studies or prayers have unlocked deeper secrets of the " + name.ToLower() + " domain.",
                $"You learn the {forClass.HumanizeTitleCase2().ToLower()} focus spell " + AllSpells.CreateSpellLink(advancedSpell, forClass) + ", and you gain 1 focus point, up to a maximum 3.", [forClass, ModTraits.Roguelike], null)
            .WithIllustration(spell.Illustration)
            .WithRulesBlockForSpell(advancedSpell, forClass)
            .WithPrerequisite(values => values.HasFeat(domainFeat.FeatName), "You must have the " + name + " domain.")
            .WithOnSheet(sheet => {
                if (sheet.Sheet.Class?.ClassTrait == Trait.Cleric) {
                    sheet.AddFocusSpellAndFocusPoint(Trait.Cleric, Ability.Wisdom, advancedSpell);
                } else if (sheet.Sheet.Class?.ClassTrait == Trait.Champion) {
                    sheet.AddFocusSpellAndFocusPoint(Trait.Champion, Ability.Charisma, advancedSpell);
                } else if (sheet.Sheet.Class?.ClassTrait == Trait.Oracle) {
                    sheet.AddFocusSpellAndFocusPoint(Trait.Oracle, Ability.Charisma, advancedSpell);
                }
            });
        }

        internal static void SpawnRatFamiliar(Creature master) {
            Creature rat = MonsterStatBlocks.CreateGiantRat();
            rat.MainName = master.MainName + "'s Rat Familiar";
            rat.Level = master.Level - 2;
            rat.MaxHP += (master.Level - 1) * 3;
            rat.Defenses.Set(Defense.AC, rat.Defenses.GetBaseValue(Defense.AC) + master.Level - 1);
            rat.Defenses.Set(Defense.Fortitude, rat.Defenses.GetBaseValue(Defense.Fortitude) + master.Level - 1);
            rat.Defenses.Set(Defense.Reflex, rat.Defenses.GetBaseValue(Defense.Reflex) + master.Level - 1);
            rat.Defenses.Set(Defense.Will, rat.Defenses.GetBaseValue(Defense.Will) + master.Level - 1);
            rat.Initiative += master.Level - 1;
            rat.ProficiencyLevel += master.Level - 1;
            rat.AddQEffect(new QEffect("Familiar", "This creature is permanantly slowed 1.") { Id = QEffectId.Slowed, Value = 1 });
            rat.AddQEffect(CommonQEffects.CantOpenDoors());
            rat.AddQEffect(new QEffect() {
                Id = QEffectIds.RatFamiliar,
                Source = master,
                StateCheck = self => {
                    if (self.Owner.HP == 0) {
                        self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                    }
                }
            });
            // Check for super rat feat here
            if (master.HasFeat(PlagueRats)) {
                rat.MaxHP += 5;
                rat.AddQEffect(CommonQEffects.RatPlagueAttack(master, "jaws"));
            }
            master.Battle.SpawnCreature(rat, master.Battle.GaiaFriends, master.Occupies);
        }
    }
}