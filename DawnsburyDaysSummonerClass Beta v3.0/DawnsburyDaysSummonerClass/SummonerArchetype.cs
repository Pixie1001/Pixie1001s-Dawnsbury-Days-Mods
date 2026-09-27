using Dawnsbury;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Archetypes;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Archetypes.Multiclass;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Zoning;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.StatBlocks.MonsterBuilder;
using Dawnsbury.Core.StatBlocks.Monsters.L9;
using Dawnsbury.Display;
using Dawnsbury.Display.Text;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using System.Diagnostics;
using static Dawnsbury.Mods.Classes.Summoner.Enums;
using static Dawnsbury.Mods.Classes.Summoner.SummonerSpells;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dawnsbury.Mods.Classes.Summoner {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class SummonerArchetype {
        public static IEnumerable<Feat> CreateFeats() {
            var summonerArchetype = ArchetypeFeats.CreateMulticlassDedication(tSummoner, "You've formed a bond with an eidolon, an entity that manifests in a physical body only through its link to your life force. Your bond may be tenuous, but that doesn't make your connection any less special.", PlayerProfile.Instance.IsBooleanOptionEnabled("Summoner_ImprovedArchetype") ?
                @"You gain an eidolon as well as the Manifest Eidolon activity. Due to your tenuous link, you can only use Act Together once per encounter and cannot take the Tandem Movement feat.

Your eidolon is trained in unarmed attacks and unarmored defense, and shares your proficiency rank for Perception, saving throws, and skill checks. Choose an eidolon type. You gain it's initial ability and become trained in your eidolon's listed skills. For each of those skills that you are already trained in, you become trained in a different skill of your choice."
:
                @"You gain an eidolon as well as the Manifest Eidolon activity. Due to your tenuous link, you can't gain or use tandem actions.

Your eidolon is trained in unarmed attacks and unarmored defense, and shares your proficiency rank for Perception, saving throws, and skill checks. Choose an eidolon type. You become trained in your eidolon's listed skills. For each of those skills that you are already trained in, you become trained in a different skill of your choice.

Your eidolon's initial ability scores are reduced. It starts with a 16 in any ability score listed at 18 for its eidolon array. It otherwise gains the statistics listed for an eidolon of that type.

At 5th level, the eidolon's ability score that was reduced to 16 increases to 18, before applying ability boosts. At levels 5, 10, 15, and 20, your eidolon also gets four ability boosts, which follow the same rules as yours.", Subclasses.subclasses)
                .WithDemandsAbility14(Ability.Charisma)
            .WithOnSheet(sheet => {
                sheet.AddSelectionOptionRightNow(new SingleFeatSelectionOption("EidolonPortrait", "Eidolon Portrait", 1, ft => ft.HasTrait(tPortraitCategory)));
                sheet.AddSelectionOptionRightNow(new SingleFeatSelectionOption("EidolonSecondaryWeapon", "Eidolon Secondary Natural Weapon", 1, ft => ft.HasTrait(tSecondaryAttackType)));
                sheet.AddSelectionOptionRightNow(new SingleFeatSelectionOption("EidolonPrimaryWeapon", "Eidolon Primary Natural Weapon", 1, ft => ft.HasTrait(tPrimaryAttackType)));
                sheet.AddSelectionOptionRightNow(new SingleFeatSelectionOption("EidolonPrimaryWeaponStats", "Eidolon Primary Weapon Stats", 1, ft => ft.HasTrait(tPrimaryAttackStats)));

                var placeholderName = $"{(sheet.Name ?? "")}'s Eidolon";
                sheet.AddSelectionOptionRightNow(new FreeTextSelectionOption("EidolonNickname", "Eidolon name", -1,
                    $"You can name your eidolon.\n\nIf you don't choose a name, it will be called {{b}}{placeholderName}{{/b}}.",
                    placeholderName,
                    (v, sName) => {
                        v.Tags["EidolonNickname"] = sName;
                    }).WithIsOptional());
            });

            yield return summonerArchetype;

            // Archetype feats
            var featSuffix = "Synergy";
            string basicFeatDisplayName = "Basic " + featSuffix;
            FeatName basicFeatNameTechnical = ModManager.RegisterFeatName("MulticlassBasicFeat" + tSummoner.ToStringOrTechnical(), basicFeatDisplayName);
            yield return new TrueFeat(basicFeatNameTechnical, 4, null, "You gain a 1st- or 2nd-level " + tSummoner.HumanizeLowerCase2() + " feat.", Array.Empty<Trait>()).WithAvailableAsArchetypeFeat(tSummoner).WithOnSheet(delegate (CalculatedCharacterSheetValues values) {
                values.AddSelectionOption(new SingleFeatSelectionOption(basicFeatNameTechnical.ToStringOrTechnical() + "Feat", basicFeatDisplayName + " feat", -1, (Feat ft) => ft is TrueFeat trueFeat && ft.HasTrait(tSummoner) && !ft.HasTrait(tTandem) && trueFeat.Level <= 2));
            });
            string advancedFeatDisplayName = "Advanced " + featSuffix;
            FeatName advancedFeatNameTechnical = ModManager.RegisterFeatName("MulticlassAdvancedFeat" + tSummoner.ToStringOrTechnical(), advancedFeatDisplayName);

            foreach (var feat in CreateSpellcastingFeats(summonerArchetype.FeatName)) yield return feat;
            yield return CreateExpertSpellcastingBenefitsFeat()
                .WithPrerequisite((values) => {
                    values.SpellRepertoires.TryGetValue(tSummoner, out var repertoire);
                    Trait spellList = repertoire!.SpellList;
                    Trait spellSkill = Trait.None;
                    if (spellList == Trait.Arcane) spellSkill = Trait.Arcana;
                    else if (spellList == Trait.Divine) spellSkill = Trait.Religion;
                    else if (spellList == Trait.Primal) spellSkill = Trait.Nature;
                    else if (spellList == Trait.Occult) spellSkill = Trait.Occultism;
                    if (spellSkill == Trait.None) return false;
                    if (values.GetProficiency(spellSkill) >= Proficiency.Master) return true;
                    return false;
                }, "You must be a master in the skill associated with your eidolon's magical tradition.");
            yield return CreateMasterSpellcastingBenefitsFeat()
                .WithPrerequisite((values) => {
                    values.SpellRepertoires.TryGetValue(tSummoner, out var repertoire);
                    Trait spellList = repertoire!.SpellList;
                    Trait spellSkill = Trait.None;
                    if (spellList == Trait.Arcane) spellSkill = Trait.Arcana;
                    else if (spellList == Trait.Divine) spellSkill = Trait.Religion;
                    else if (spellList == Trait.Primal) spellSkill = Trait.Nature;
                    else if (spellList == Trait.Occult) spellSkill = Trait.Occultism;
                    if (spellSkill == Trait.None) return false;
                    if (values.GetProficiency(spellSkill) >= Proficiency.Legendary) return true;
                    return false;
                }, "You must be a legendary in the skill associated with your eidolon's magical tradition.");

            yield return new TrueFeat(advancedFeatNameTechnical, 6, null, "You gain one " + tSummoner.HumanizeLowerCase2() + $" feat.\r\n\r\nFor the purpose of meeting its prerequisites, your {tSummoner.HumanizeLowerCase2()} level is equal to half your character level:\r\n• If you take this feat at level 6, you can only take a level 1 or level 2 {tSummoner.HumanizeLowerCase2()} feat.\r\n• If you take this feat at level 8, you can only take a level 1, level 2 or level 4 {tSummoner.HumanizeLowerCase2()} feat.", Array.Empty<Trait>()).WithAvailableAsArchetypeFeat(tSummoner).WithMultipleSelection().WithPrerequisite(basicFeatNameTechnical, basicFeatDisplayName)
            .WithOnSheet(delegate (CalculatedCharacterSheetValues values) {
                values.AddSelectionOption(new SingleFeatSelectionOption(advancedFeatNameTechnical.ToStringOrTechnical() + "Feat", advancedFeatDisplayName + " feat", -1, (Feat ft, CalculatedCharacterSheetValues val) => ft is TrueFeat trueFeat && ft.HasTrait(tSummoner) && !ft.HasTrait(tTandem) && trueFeat.Level <= val.CurrentLevel / 2));
            });

            yield return new TrueFeat(ftInitialEidolonAbility, 4, "Your link to your eidolon becomes stronger, granting it a new ability.", "Your eidolon gains the initial ability for an eidolon of its type.", [])
                .WithAvailableAsArchetypeFeat(tSummoner)
            .WithOnSheet(sheet => {
                if (sheet.HasFeat(scFeyEidolon)) {
                    sheet.AddFeat(AllFeats.GetFeatByFeatName(ftMagicalUnderstudy), null);
                }
            });

            var ts = ArchetypeFeats.DuplicateFeatAsArchetypeFeat(ftTandemStrike, tSummoner, 6)
                .WithPrerequisite(ft => PlayerProfile.Instance.IsBooleanOptionEnabled("Summoner_ImprovedArchetype"), "You must enable 'Summoner: Improved Multiclass Archetype' in settings to take this feat.");
            ts.Traits.Add(Trait.Rebalanced);
            yield return ts;

            yield return new TrueFeat(ftExpertCombatEidolon, 12, "Your eidolon advances its capabilities in combat.", "Your eidolon becomes an expert in unarmed attacks. If you are an expert in unarmored defense, your eidolon also becomes an expert in unarmored defense. If you have weapon specialization, your eidolon also gains weapon specialization.", [])
                .WithAvailableAsArchetypeFeat(tSummoner);

            TrueFeat everVigilentFormLowLevel = CommonFeatTemplates.CreateDuplicateFeat(ftEverVigilantSenses, ModManager.RegisterFeatName(ftEverVigilantSenses.ToStringOrTechnical() + "ForArchetype" + tSummoner.ToStringOrTechnical(), AllFeats.GetFeatByFeatName(ftEverVigilantSenses).BaseName), 12);
            everVigilentFormLowLevel.Traits.RemoveAll((Trait trait) => trait.GetTraitProperties().IsClassTrait);
            everVigilentFormLowLevel.Prerequisites.RemoveAll((Prerequisite prereq) => prereq is ClassPrerequisite);
            yield return everVigilentFormLowLevel;

            yield return new TrueFeat(ModManager.RegisterFeatName("Summoner_SignatureSynergy", "Signature Synergy"), 14, "Your eidolon gains an evolution integral to its form, and it comes more easily than your other synergies.", "You gain either Ever-Vigilant Senses or Hulking Size evolution feat.", [])
                .WithAvailableAsArchetypeFeat(tSummoner)
                .WithPrerequisite(advancedFeatNameTechnical, advancedFeatDisplayName)
            .WithOnSheet(delegate (CalculatedCharacterSheetValues values) {
                values.AddSelectionOption(new SingleFeatSelectionOption("Summoner_SignatureSynergyFeat", "Signature Synergy feat", -1, (ft, val) => ft.FeatName == ftHulkingSize || ft == everVigilentFormLowLevel));
            });
        }

        public static IEnumerable<Feat> CreateSpellcastingFeats(FeatName dedicationFeat) {
            var basicBenefits = CreateBasicSpellcastingBenefitsFeat();
            yield return basicBenefits;
            yield return MulticlassArchetypeFeats.CreateBreadthFeat(tSummoner, Trait.Spontaneous, "Eidolon Bond", basicBenefits);
            yield return MulticlassArchetypeFeats.CreateEldritchTricksterFeat(tSummoner, dedicationFeat);
        }

        public static Feat CreateBasicSpellcastingBenefitsFeat() {
            return new TrueFeat(ModManager.RegisterFeatName("Basic" + tSummoner.ToStringOrTechnical() + "Spellcasting", "Basic " + tSummoner.HumanizeTitleCase2() + " Spellcasting"), 4, null, "You gain a level 1 spell slot and learn a level 1 spell.\n\nAt level 6, you automatically also gain a level 2 spell slot and learn a level 2 spell; and you can designate one spell as your signature spell.\n\nAt level 8, you automatically also gain a level 3 spell slot and learn a level 3 spell.", [Trait.Rebalanced], null)
            .WithAvailableAsArchetypeFeat(tSummoner)
            .WithOnSheet(values => {
                if (values.SpellRepertoires.TryGetValue(tSummoner, out var repertoire)) {
                    repertoire.SpellSlots[1]++;
                    if (values.HasFeat(scArsonDemonEidolon)) {
                        values.AddSelectionOption(new SelectArsonSpells(tSummoner.ToStringOrTechnical() + "SpellKnownBasic1",
                            "Level 1 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 1, 1));
                        values.AddAtLevel(6, val => {
                            values.AddSelectionOption(new SelectArsonSpells(tSummoner.ToStringOrTechnical() + "SpellKnownBasic2",
                                "Level 2 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 2, 1));
                            values.AddSelectionOption(new SignatureSpellSelectionOption(tSummoner.ToStringOrTechnical() + "ArchetypeSignatureSpell2",
                                tSummoner.HumanizeTitleCase2() + " signature spell", -1, 2, tSummoner));
                        });
                        values.AddAtLevel(8, val => {
                            values.AddSelectionOption(new AddToSpellRepertoireOption(tSummoner.ToStringOrTechnical() + "SpellKnownBasic3",
                                "Level 3 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, repertoire.SpellList, 3, 1));
                        });
                    } else if (values.HasFeat(scFeyEidolon) && values.HasFeat(ftInitialEidolonAbility)) {
                        values.AddSelectionOption(new SelectFeySpells(tSummoner.ToStringOrTechnical() + "SpellKnownBasic1",
                            "Level 1 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 1, 1));
                        values.AddAtLevel(6, val => {
                            values.AddSelectionOption(new SelectFeySpells(tSummoner.ToStringOrTechnical() + "SpellKnownBasic2",
                                "Level 2 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 2, 1));
                            values.AddSelectionOption(new SignatureSpellSelectionOption(tSummoner.ToStringOrTechnical() + "ArchetypeSignatureSpell2",
                                tSummoner.HumanizeTitleCase2() + " signature spell", -1, 2, tSummoner));
                        });
                        values.AddAtLevel(8, val => {
                            values.AddSelectionOption(new AddToSpellRepertoireOption(tSummoner.ToStringOrTechnical() + "SpellKnownBasic3",
                                "Level 3 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, repertoire.SpellList, 3, 1));
                        });
                    } else {
                        values.AddSelectionOption(new AddToSpellRepertoireOption(tSummoner.ToStringOrTechnical() + "SpellKnownBasic1",
                            "Level 1 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, repertoire.SpellList, 1, 1));
                        values.AddAtLevel(6, val => {
                            values.AddSelectionOption(new AddToSpellRepertoireOption(tSummoner.ToStringOrTechnical() + "SpellKnownBasic2",
                                "Level 2 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, repertoire.SpellList, 2, 1));
                            values.AddSelectionOption(new SignatureSpellSelectionOption(tSummoner.ToStringOrTechnical() + "ArchetypeSignatureSpell2",
                                tSummoner.HumanizeTitleCase2() + " signature spell", -1, 2, tSummoner));
                        });
                        values.AddAtLevel(8, val => {
                            values.AddSelectionOption(new AddToSpellRepertoireOption(tSummoner.ToStringOrTechnical() + "SpellKnownBasic3",
                                "Level 3 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, repertoire.SpellList, 3, 1));
                        });
                    }
                    if (values.Sheet.MaximumLevel >= 6) {
                        repertoire.SpellSlots[2]++;
                    }

                    if (values.Sheet.MaximumLevel >= 8) {
                        repertoire.SpellSlots[3]++;
                    }
                }
            });
        }

        private static Feat CreateExpertSpellcastingBenefitsFeat() {
            return new TrueFeat(ModManager.RegisterFeatName("Expert" + tSummoner.ToStringOrTechnical() + "Spellcasting", "Expert " + tSummoner.HumanizeTitleCase2() + " Spellcasting"), 12, null,
                "You become an expert in spell attacks and spell save DCs. You gain a level 4 spell slot.\n\nAt level 14, you automatically gain also a level 5 spell slot.\n\nAt level 16, you automatically gain also a level 6 spell slot.", [Trait.Rebalanced], null)
            .WithAvailableAsArchetypeFeat(tSummoner)
            .WithPrerequisite((values) => values.AllFeats.Any((Feat ft) => ft.ToTechnicalName() == "Basic" + tSummoner.ToStringOrTechnical() + "Spellcasting"), "You must have the Basic " + tSummoner.HumanizeTitleCase2() + " Spellcasting feat.")
            .WithOnSheet(values => {
                values.SetProficiency(Trait.Spell, Proficiency.Expert);
                if (values.SpellRepertoires.TryGetValue(tSummoner, out var repertoire)) {
                    repertoire.SpellSlots[4]++;
                    if (values.HasFeat(scArsonDemonEidolon)) {
                        values.AddSelectionOption(new SelectArsonSpells(tSummoner.ToStringOrTechnical() + "SpellKnownExpert5",
                            "Level 4 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 4, 1));
                        values.AddAtLevel(14, val => {
                            values.AddSelectionOption(new SelectArsonSpells(tSummoner.ToStringOrTechnical() + "SpellKnownExpert6",
                                "Level 5 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 5, 1));
                            values.AddSelectionOption(new SignatureSpellSelectionOption(tSummoner.ToStringOrTechnical() + "ArchetypeSignatureSpell5",
                                tSummoner.HumanizeTitleCase2() + " signature spell", -1, 5, tSummoner));
                        });
                        values.AddAtLevel(16, val => {
                            values.AddSelectionOption(new SelectArsonSpells(tSummoner.ToStringOrTechnical() + "SpellKnownExpert7",
                                "Level 6 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 6, 1));
                        });
                    } else if (values.HasFeat(scFeyEidolon) && values.HasFeat(ftInitialEidolonAbility)) {
                        values.AddSelectionOption(new SelectFeySpells(tSummoner.ToStringOrTechnical() + "SpellKnownExpert5",
                            "Level 4 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 4, 1));
                        values.AddAtLevel(14, val => {
                            values.AddSelectionOption(new SelectFeySpells(tSummoner.ToStringOrTechnical() + "SpellKnownExpert6",
                                "Level 5 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 5, 1));
                            values.AddSelectionOption(new SignatureSpellSelectionOption(tSummoner.ToStringOrTechnical() + "ArchetypeSignatureSpell5",
                                tSummoner.HumanizeTitleCase2() + " signature spell", -1, 5, tSummoner));
                        });
                        values.AddAtLevel(16, val => {
                            values.AddSelectionOption(new SelectFeySpells(tSummoner.ToStringOrTechnical() + "SpellKnownExpert7",
                                "Level 6 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 6, 1));
                        });
                    } else {
                        values.AddSelectionOption(new AddToSpellRepertoireOption(tSummoner.ToStringOrTechnical() + "SpellKnownExpert5",
                            "Level 4 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, repertoire.SpellList, 4, 1));
                        values.AddAtLevel(14, val => {
                            values.AddSelectionOption(new AddToSpellRepertoireOption(tSummoner.ToStringOrTechnical() + "SpellKnownExpert6",
                                "Level 5 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, repertoire.SpellList, 5, 1));
                            values.AddSelectionOption(new SignatureSpellSelectionOption(tSummoner.ToStringOrTechnical() + "ArchetypeSignatureSpell5",
                                tSummoner.HumanizeTitleCase2() + " signature spell", -1, 5, tSummoner));
                        });
                        values.AddAtLevel(16, val => {
                            values.AddSelectionOption(new AddToSpellRepertoireOption(tSummoner.ToStringOrTechnical() + "SpellKnownExpert7",
                                "Level 6 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, repertoire.SpellList, 6, 1));
                        });
                    }
                    if (values.Sheet.MaximumLevel >= 14) {
                        repertoire.SpellSlots[5]++;
                    }

                    if (values.Sheet.MaximumLevel >= 16) {
                        repertoire.SpellSlots[6]++;
                    }
                }
            });
        }

        private static Feat CreateMasterSpellcastingBenefitsFeat() {
            return new TrueFeat(ModManager.RegisterFeatName("Master" + tSummoner.ToStringOrTechnical() + "Spellcasting", "Master " + tSummoner.HumanizeTitleCase2() + " Spellcasting"), 18, null,
                "You become a master in spell attacks and spell save DCs. You gain a level 7 spell slot and learn a level 7 spell; and you can designate one more spell as your signature spell.\n\nAt level 20, you automatically also gain a level 8 spell slot and learn a level 8 spell.", [Trait.Rebalanced], null)
            .WithAvailableAsArchetypeFeat(tSummoner)
            .WithPrerequisite((values) => values.AllFeats.Any((Feat ft) => ft.ToTechnicalName() == "Basic" + tSummoner.ToStringOrTechnical() + "Spellcasting"), "You must have the Basic " + tSummoner.HumanizeTitleCase2() + " Spellcasting feat.")
            .WithOnSheet(values => {
                values.SetProficiency(Trait.Spell, Proficiency.Master);
                if (values.SpellRepertoires.TryGetValue(tSummoner, out var repertoire)) {
                    repertoire.SpellSlots[7]++;
                    if (values.HasFeat(scArsonDemonEidolon)) {
                        values.AddSelectionOption(new SelectArsonSpells(tSummoner.ToStringOrTechnical() + "SpellKnownMaster7", "Level 7 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 7, 1));
                        values.AddSelectionOption(new SignatureSpellSelectionOption(tSummoner.ToStringOrTechnical() + "ArchetypeSignatureSpell7",
                            tSummoner.HumanizeTitleCase2() + " signature spell", -1, 7, tSummoner));
                        values.AddAtLevel(20, val => {
                            values.AddSelectionOption(new SelectArsonSpells(tSummoner.ToStringOrTechnical() + "SpellKnownMaster8",
                                "Level 8 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 8, 1));
                        });
                    } else if (values.HasFeat(scFeyEidolon) && values.HasFeat(ftInitialEidolonAbility)) {
                        values.AddSelectionOption(new SelectFeySpells(tSummoner.ToStringOrTechnical() + "SpellKnownMaster7", "Level 7 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 7, 1));
                        values.AddSelectionOption(new SignatureSpellSelectionOption(tSummoner.ToStringOrTechnical() + "ArchetypeSignatureSpell7",
                            tSummoner.HumanizeTitleCase2() + " signature spell", -1, 7, tSummoner));
                        values.AddAtLevel(20, val => {
                            values.AddSelectionOption(new SelectFeySpells(tSummoner.ToStringOrTechnical() + "SpellKnownMaster8",
                                "Level 8 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, 8, 1));
                        });
                    } else {
                        values.AddSelectionOption(new AddToSpellRepertoireOption(tSummoner.ToStringOrTechnical() + "SpellKnownMaster7", "Level 7 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, repertoire.SpellList, 7, 1));
                        values.AddSelectionOption(new SignatureSpellSelectionOption(tSummoner.ToStringOrTechnical() + "ArchetypeSignatureSpell7",
                            tSummoner.HumanizeTitleCase2() + " signature spell", -1, 7, tSummoner));
                        values.AddAtLevel(20, val =>
                        {
                            values.AddSelectionOption(new AddToSpellRepertoireOption(tSummoner.ToStringOrTechnical() + "SpellKnownMaster8",
                                "Level 8 " + tSummoner.HumanizeLowerCase2() + " spell", -1, tSummoner, repertoire.SpellList, 8, 1));
                        });
                    }
                    if (values.Sheet.MaximumLevel >= 20) {
                        repertoire.SpellSlots[8]++;
                    }
                }
            });
        }
    }
}
