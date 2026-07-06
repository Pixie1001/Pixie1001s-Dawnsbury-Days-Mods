using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Kineticist;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Archetypes;
using Dawnsbury.Core.CharacterBuilder.Selections.Selected;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.StatBlocks.Monsters.L_1;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using Dawnsbury.ThirdParty.SteamApi;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Feats {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class FeatLoader {
        public static FeatName NightmareDomain { get; } = ModManager.RegisterFeatName("Nightmares");

        internal static void LoadFeats() {
            AddFeats(CreateFeats());
            AddFeats(DrowHeritage.CreateFeats());
            AddFeats(RatMonarch.CreateFeats());
            AddFeats(MonkFeats.CreateFeats());
        }

        private static void AddFeats(IEnumerable<Feat> feats) {
            foreach (Feat feat in feats) {
                ModManager.AddFeat(feat);
            }
        }

        private static IEnumerable<Feat> CreateFeats() {
            //List<Trait> classTraits = new List<Trait>();
            //AllFeats.All.ForEach(ft => {
            //    if (ft is ClassSelectionFeat classFeat) {
            //        classTraits.Add(classFeat.ClassTrait);
            //    }
            //});

            var nightmareDomain = ClericClassFeatures.CreateDomain(NightmareDomain, "You fill minds with horror and dread.", SpellLoader.WakingNightmare, SpellLoader.SharedNightmare);
            nightmareDomain.Traits.Add(ModTraits.Roguelike);
            ClericClassFeatures.AllDomainFeats.Add(nightmareDomain);
            CreateAdvancedDomainFeat(nightmareDomain);

            //AllFeats.All.Find(ft => ft.FeatName == FeatName.AdvancedDomain)?.Subfeats?.Add(CreateAdvancedDomainFeat(Trait.Cleric, nightmareDomain));
            //AllFeats.All.Find(ft => ft.FeatName == FeatName.AdvancedDeitysDomain)?.Subfeats?.Add(CreateAdvancedDomainFeat(Trait.Champion, nightmareDomain));
            //AllFeats.All.Find(ft => ft.FeatName == FeatName.DomainFluency)?.Subfeats?.Add(CreateAdvancedDomainFeat(Trait.Oracle, nightmareDomain));
            yield return nightmareDomain;

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

            AllFeats.All.Find(ft => ft.FeatName == FeatName.Cleric)?.Subfeats?.Add(echidnaDiety);
            yield return echidnaDiety;
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

        internal static void CreateAdvancedDomainFeat(Feat domainFeat) {
            ValueTuple<Trait, FeatName>[] domainAccessFeats = [(Trait.Cleric, FeatName.AdvancedDomain), (Trait.Champion, FeatName.AdvancedDeitysDomain), (Trait.Oracle, FeatName.DomainFluency)];
            var name = domainFeat.FeatName.HumanizeTitleCase2();
            var advancedSpell = (SpellId)domainFeat.Tag!;
            //var spell = AllSpells.All.Concat((List<SpellId>)typeof(ModManager).GetProperty("NewSpells", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).GetValue(null)).FirstOrDefault(spell => spell.SpellId == advancedSpell);
            foreach (var pair in domainAccessFeats) {
                var spell = AllSpells.CreateModernSpellTemplate(advancedSpell, pair.Item1);
                var feat = new Feat(ModManager.RegisterFeatName("RL_AdvancedDomain:" + pair.Item1.HumanizeTitleCase2() + ":" + name, name + ": " + spell.Name), "Your studies or prayers have unlocked deeper secrets of the " + name.ToLower() + " domain.",
                    $"You learn the {pair.Item1.HumanizeTitleCase2().ToLower()} focus spell " + AllSpells.CreateSpellLink(advancedSpell, pair.Item1) + ", and you gain 1 focus point, up to a maximum 3.", [ModTraits.Roguelike], null)
                .WithIllustration(spell.Illustration)
                .WithRulesBlockForSpell(advancedSpell, pair.Item1)
                .WithPrerequisite(values => values.HasFeat(domainFeat.FeatName), "You must have the " + name + " domain.")
                .WithOnSheet(sheet => {
                    if (sheet.Sheet.Class?.ClassTrait == pair.Item1) {
                        sheet.AddFocusSpellAndFocusPoint(pair.Item1, Ability.Wisdom, advancedSpell);
                    } else if (sheet.Sheet.Class?.ClassTrait == Trait.Champion) {
                        sheet.AddFocusSpellAndFocusPoint(pair.Item1, Ability.Charisma, advancedSpell);
                    } else if (sheet.Sheet.Class?.ClassTrait == Trait.Oracle) {
                        sheet.AddFocusSpellAndFocusPoint(pair.Item1, Ability.Charisma, advancedSpell);
                    }
                });
                AllFeats.All.Find(ft => ft.FeatName == pair.Item2)?.Subfeats?.Add(feat);
                ModManager.AddFeat(feat);
            }
        }
    }
}