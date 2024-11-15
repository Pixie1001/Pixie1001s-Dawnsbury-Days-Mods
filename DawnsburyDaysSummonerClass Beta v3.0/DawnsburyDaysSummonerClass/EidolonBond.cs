using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury;
using Dawnsbury.Audio;
using Dawnsbury.Core;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
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
using Dawnsbury.Core.Mechanics.Damage;
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
using Dawnsbury.Mods.Classes.Summoner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dawnsbury.Mods.Classes.Summoner.SummonerSpells;
using static Dawnsbury.Mods.Classes.Summoner.SummonerClassLoader;
using static Dawnsbury.Mods.Classes.Summoner.Enums;
using Dawnsbury.Modding;
using Dawnsbury.Core.Mechanics;

namespace Dawnsbury.Mods.Classes.Summoner {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class EidolonBond : Feat {
        public List<Trait> eidolonTraits = new List<Trait>();
        public string ActionText { get; set; } = "";
        public string AbilityText { get; set; } = "";
        public Action<Creature, Creature>? ClassFeatures { get; set; }

        public EidolonBond WithActionText(string text) {
            ActionText = text;
            return this;
        }

        public EidolonBond WithAbilityText(string text) {
            AbilityText = text;
            return this;
        }

        /// <summary>
        /// Adds logic for 1st and 7th level class features, using EIDOLON and SUMMONER.
        /// </summary>
        public EidolonBond WithClassFeatures(Action<Creature, Creature> features) {
            ClassFeatures = features;
            return this;
        }

        public EidolonBond(FeatName featName, string flavourText, string rulesText, Trait spellList, List<FeatName> skills, Func<Feat, bool> alignmentOptions, List<Trait> traits, List<Trait> eidolonTraits, List<Feat> subfeats) : base(featName, flavourText, rulesText, traits.Concat(new List<Trait>() { Enums.tSummonerSubclass }).ToList(), subfeats) {
            Init(spellList, skills, alignmentOptions, eidolonTraits);
        }

        public EidolonBond(FeatName featName, string flavourText, string rulesText, Trait spellList, List<FeatName> skills, Func<Feat, bool> alignmentOptions, List<Trait> eidolonTraits) : base(featName, flavourText, rulesText, new List<Trait>() { Enums.tSummonerSubclass }, null) {
            Init(spellList, skills, alignmentOptions, eidolonTraits);
        }

        private void Init(Trait spellList, List<FeatName> skills, Func<Feat, bool> alignmentOptions, List<Trait> eidolonTraits) {
            this.OnSheet = (Action<CalculatedCharacterSheetValues>)(sheet => {
                this.eidolonTraits = eidolonTraits;
                Feat[] alignments = AllFeats.All.Where(alignmentOptions).ToArray();
                if (alignments.Count() == 1) {
                    sheet.GrantFeat(alignments[0].FeatName);
                } else {
                    sheet.AddSelectionOption((SelectionOption)new SingleFeatSelectionOption("EidolonAlignment", "Eidolon Alignment", 1, alignmentOptions));
                }
                sheet.AddSelectionOption(new SingleFeatSelectionOption("EidolonPrimaryWeaponStats", "Eidolon Primary Weapon Stats", 1, (Func<Feat, bool>)(ft => ft.HasTrait(Enums.tPrimaryAttackStats))));
                sheet.AddSelectionOption(new SingleFeatSelectionOption("EidolonPrimaryWeapon", "Eidolon Primary Natural Weapon", 1, (Func<Feat, bool>)(ft => ft.HasTrait(Enums.tPrimaryAttackType))));
                sheet.AddSelectionOption(new SingleFeatSelectionOption("EidolonSecondaryWeapon", "Eidolon Secondary Natural Weapon", 1, (Func<Feat, bool>)(ft => ft.HasTrait(Enums.tSecondaryAttackType))));
                sheet.SpellTraditionsKnown.Add(spellList);
                sheet.SpellRepertoires.Add(Enums.tSummoner, new SpellRepertoire(Ability.Charisma, spellList));
                sheet.SetProficiency(Trait.Spell, Proficiency.Trained);
                foreach (FeatName skill in skills) {
                    sheet.GrantFeat(skill);
                }
                if (this.FeatName == Enums.scFeyEidolon)
                    sheet.AddFeat(AllFeats.All.FirstOrDefault(ft => ft.FeatName == Enums.ftMagicalUnderstudy), null);
                SpellRepertoire repertoire = sheet.SpellRepertoires[Enums.tSummoner];
                if (this.FeatName != Enums.scFeyEidolon) {
                    sheet.AddSelectionOption((SelectionOption)new AddToSpellRepertoireOption("SummonerCantrips", "Cantrips", 1, Enums.tSummoner, spellList, 0, 5));
                    sheet.AddSelectionOption((SelectionOption)new AddToSpellRepertoireOption("SummonerSpells1", "Level 1 spells", 1, Enums.tSummoner, spellList, 1, 2));
                    sheet.AddSelectionOption((SelectionOption)new AddToSpellRepertoireOption("SummonerSpells2", "Level 1 spell", 2, Enums.tSummoner, spellList, 1, 1));
                    sheet.AddSelectionOption((SelectionOption)new AddToSpellRepertoireOption("SummonerSpells3", "Level 2 spell", 3, Enums.tSummoner, spellList, 2, 1));
                    sheet.AddSelectionOption((SelectionOption)new AddToSpellRepertoireOption("SummonerSpells4", "Level 2 spell", 4, Enums.tSummoner, spellList, 2, 1));
                } else {
                    sheet.AddSelectionOption((SelectionOption)new SelectFeySpells("SummonerCantrips", "Cantrips", 1, Enums.tSummoner, 0, 5, true));
                    sheet.AddSelectionOption((SelectionOption)new SelectFeySpells("SummonerSpells1", "Level 1 spells", 1, Enums.tSummoner, 1, 2));
                    sheet.AddSelectionOption((SelectionOption)new SelectFeySpells("SummonerSpells2", "Level 1 spell", 2, Enums.tSummoner, 1, 1));
                    sheet.AddSelectionOption((SelectionOption)new SelectFeySpells("SummonerSpells3", "Level 2 spells", 3, Enums.tSummoner, 2, 1));
                    sheet.AddSelectionOption((SelectionOption)new SelectFeySpells("SummonerSpells4", "Level 2 spell", 4, Enums.tSummoner, 2, 1));
                }

                repertoire.SpellSlots[1] = 1;
                sheet.AddAtLevel(2, (Action<CalculatedCharacterSheetValues>)(_ => ++repertoire.SpellSlots[1]));
                sheet.AddAtLevel(3, (Action<CalculatedCharacterSheetValues>)(_ => ++repertoire.SpellSlots[2]));
                sheet.AddAtLevel(4, (Action<CalculatedCharacterSheetValues>)(_ => ++repertoire.SpellSlots[2]));

                if (this.FeatName == Enums.scFeyEidolon)
                    sheet.AddAtLevel(7, sheet => sheet.AddFeat(AllFeats.All.FirstOrDefault(ft => ft.FeatName == Enums.ftMagicalAdept), null));

                for (int index = 5; index <= 17; index += 2) {
                    int thisLevel = index;
                    sheet.AddAtLevel(thisLevel, (Action<CalculatedCharacterSheetValues>)(values => {
                        int num = (thisLevel + 1) / 2;
                        int removedLevel = num - 2;
                        values.SpellRepertoires[Enums.tSummoner].SpellSlots[removedLevel]--;
                        values.SpellRepertoires[Enums.tSummoner].SpellSlots[removedLevel]--;
                        values.SpellRepertoires[Enums.tSummoner].SpellSlots[num]++;
                        values.SpellRepertoires[Enums.tSummoner].SpellSlots[num]++;

                        repertoire.SpellsKnown.RemoveAll(spell => spell.HasTrait(Trait.Focus) == false && spell.HasTrait(Trait.Cantrip) == false);

                        int tradition = (int)spellList;
                        int maximumSpellLevel = num;
                        AddToSpellRepertoireOption repertoireOption1;
                        AddToSpellRepertoireOption repertoireOption2;
                        if (this.FeatName != Enums.scFeyEidolon) {
                            repertoireOption1 = new AddToSpellRepertoireOption($"SummonerSpells{sheet.CurrentLevel}-1", $"Level {num - 1} spell replacements", thisLevel, Enums.tSummoner, spellList, maximumSpellLevel - 1, 3);
                            repertoireOption2 = new AddToSpellRepertoireOption($"SummonerSpells{sheet.CurrentLevel}-2", $"Level {num} spell replacements", thisLevel, Enums.tSummoner, spellList, maximumSpellLevel, 2);
                        } else {
                            repertoireOption1 = new SelectFeySpells($"SummonerSpells{sheet.CurrentLevel}-1", $"Level {num - 1} spell replacements", thisLevel, Enums.tSummoner, maximumSpellLevel - 1, 3);
                            repertoireOption2 = new SelectFeySpells($"SummonerSpells{sheet.CurrentLevel}-2", $"Level {num} spell replacements", thisLevel, Enums.tSummoner, maximumSpellLevel, 2);
                        }
                        values.AddSelectionOption((SelectionOption)repertoireOption1);
                        values.AddSelectionOption((SelectionOption)repertoireOption2);

                    }));
                }
                repertoire.SpellsKnown.Add(AllSpells.CreateModernSpellTemplate(SummonerClassLoader.spells[SummonerSpellId.EidolonBoost], Enums.tSummoner, sheet.MaximumSpellLevel));
            });
            this.OnCreature = ((sheet, creature) => {
                // Signature-afy spells
                SpellRepertoire repertoire = sheet.SpellRepertoires[Enums.tSummoner];
                List<Spell> spells = repertoire.SpellsKnown.Where(spell => spell.HasTrait(Trait.Cantrip) == false).ToList();

                if (spells.Count() > 5) {
                    return;
                }

                for (int i = 0; i < spells.Count(); i++) {
                    for (int spellLvl = spells[i].MinimumSpellLevel; spellLvl < 10; spellLvl++) {
                        if (spells.FirstOrDefault(s => s.SpellId == spells[i].SpellId && s.SpellLevel == spellLvl) == null) {
                            repertoire.SpellsKnown.Add(AllSpells.CreateModernSpellTemplate(spells[i].SpellId, Enums.tSummoner, spellLvl));
                        }
                    }
                }
                if (sheet.HasFeat(Enums.ftAbundantSpellcasting1)) {
                    Spell spell = TraditionToSpell(sheet.SpellRepertoires[Enums.tSummoner].SpellList, 1);
                    if (repertoire.SpellsKnown.FirstOrDefault(s => s.SpellId == spell.SpellId && s.SpellLevel == spell.SpellLevel) == null) {
                        repertoire.SpellsKnown.Add(spell);
                    }
                }
                if (sheet.HasFeat(Enums.ftAbundantSpellcasting4)) {
                    Spell spell = TraditionToSpell(sheet.SpellRepertoires[Enums.tSummoner].SpellList, 2);
                    var test = repertoire.SpellsKnown;
                    if (repertoire.SpellsKnown.FirstOrDefault(s => s.SpellId == spell.SpellId && s.SpellLevel == spell.SpellLevel) == null) {
                        repertoire.SpellsKnown.Add(spell);
                    }
                }
            });
        }
    }
}