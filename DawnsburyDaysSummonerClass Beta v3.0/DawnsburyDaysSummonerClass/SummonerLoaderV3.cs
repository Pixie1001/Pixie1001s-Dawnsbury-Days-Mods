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
//using static Microsoft.Xna.Framework.Point;
//using static Microsoft.Xna.Framework.Vector2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Dawnsbury.Mods.Classes.Summoner.SummonerSpells;
using static Dawnsbury.Mods.Classes.Summoner.Enums;
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
//using System.Drawing;
using Microsoft.Xna.Framework.Audio;
using static System.Reflection.Metadata.BlobBuilder;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Kineticist;
using Dawnsbury.Core.Noncombat;
using Dawnsbury.Phases.Menus;
using Dawnsbury.Phases.Popups;
using Dawnsbury.Campaign.Path;

namespace Dawnsbury.Mods.Classes.Summoner {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class EidolonCreatureTargetingRequirement : CreatureTargetingRequirement {
        public QEffectId qfEidolon { get; }

        public EidolonCreatureTargetingRequirement(QEffectId qf) {
            this.qfEidolon = qf;
        }

        public override Usability Satisfied(Creature source, Creature target) {
            return target.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == this.qfEidolon && qf.Source == source)) != null ? Usability.Usable : Usability.NotUsableOnThisCreature("This ability can only be used on your eidolon.");
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class SelectFeySpells : AddToSpellRepertoireOption {

        private bool allowCantrips;

        public SelectFeySpells(string key, string name, int level, Trait classRepertoire, int maxSpellLevel, int maximumNumberOfSpells, bool allowCantrips = false) : base(key, name, level, classRepertoire, Trait.Primal, maxSpellLevel, maximumNumberOfSpells) {
            this.allowCantrips = allowCantrips;
        }

        // What spells are shown as optional
        public override bool Eligible(CalculatedCharacterSheetValues values, Spell spell) {
            Trait[] allowedTraits = new Trait[] { Trait.Illusion, Trait.Enchantment, Trait.Mental };
            List<Trait> traits = spell.Traits.ToList().Where(t => allowedTraits.Contains(t)).ToList();

            if ((!spell.HasTrait(Trait.Primal) && !(traits.Count > 0 && spell.HasTrait(Trait.Arcane))) || spell.HasTrait(Trait.SpellCannotBeChosenInCharacterBuilder) || (spell.HasTrait(Trait.Cantrip) && !allowCantrips))
                return false;
            if (allowCantrips && spell.HasTrait(Trait.Cantrip)) {
                return true;
            } else if (allowCantrips && !spell.HasTrait(Trait.Cantrip)) {
                return false;
            }
            return this.MaximumSpellLevel >= 1 && spell.MinimumSpellLevel <= this.MaximumSpellLevel && !spell.HasTrait(Trait.Cantrip);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class SummonerClassLoader {

        // Portraits
        internal static List<ModdedIllustration> portraits = new List<ModdedIllustration>();

        // SpellIDs
        internal static Dictionary<SummonerSpellId, SpellId> spells = LoadSpells();

        // Class and subclass text
        private static readonly string SummonerFlavour = "You can magically beckon a powerful being called an eidolon to your side, serving as the mortal conduit that anchors it to the world. " +
            "Whether your eidolon is a friend, a servant, or even a personal god, your connection to it marks you as extraordinary, shaping the course of your life dramatically.";
        private static readonly string SummonerCrunch =
            "{b}1. Eidolon.{/b} You have a connection with a powerful and usually otherworldly entity called an eidolon, and you can use your life force as a conduit to manifest this ephemeral entity into the mortal world. " +
            "Your bonded eidolon's nature determine your spell casting tradition, in addition to its statistics. In addition, its appearence and attacks are fully customisable.\n\n" +
            "Your eidolon begins combat already manifested, and shares your hit point pool, actions and multiple attack penalty. You can swap between controlling you or your eidolon at any time, without ending your turn.\n\n" +
            "Your eidolon benefits from the skill bonuses on any invested magical items you're wearing, and all of your fundermental armour runes.\n\n" +
            "{b}2. Invest Weapon {icon:Action}.{/b} Your eidolon's unarmed strikes also benefit from the fundermental and property runes of a single weapon you're wielding, or your handwraps of mighty blows. " +
            "At the start of combat, you automatically elect a weapon for your eidolon to use, in the following priority order: Your handwraps of mighty blows, your main-hand weapon, your off-hand weapon. If you drop an invested weapon, " +
            "your eidolon can also no longer benefit from it. You can select a new magic weapon from the items you're holding, or swap back to your handwraps, by using the Invest Weapon {icon:Action} action, under 'Other Maneuvers'." +
            "\n\n{b}3. Evolution Feat.{/b} Gain a single 1st level evolution feat. Evolution feats affect your eidolon instead of you.\n\n" +
            "{b}4. Link Spells.{/b} Your connection to your eidolon allows you to cast link spells, special spells that have been forged through your shared connection with your eidolon." +
            "You start with two such spells. The focus spell " + AllSpells.CreateModernSpellTemplate(spells[SummonerSpellId.EvolutionSurge], Enums.tSummoner).ToSpellLink() + " and the link cantrip " +
            AllSpells.CreateModernSpellTemplate(spells[SummonerSpellId.EidolonBoost], Enums.tSummoner).ToSpellLink() + "\n\n" +
            "{b}4. Spontaneous Spellcasting:{/b} You can cast spells. You can cast 1 spell per day and you can choose the spells from among the spells you know. You learn 2 spells of your choice, " +
            "but they must come from the spellcasting tradition of your eidolon. You also learn 5 cantrips — weak spells — that automatically heighten as you level up. You can cast any number of cantrips per day. " +
            "You can gain additional spell slots and spells known from leveling up and from feats. Your spellcasting ability is Charisma" +
            "\n\n{b}At higher levels:{/b}" +
            "\n{b}Level 2:{/b} Summoner feat" +
            "\n{b}Level 3:{/b} General feat, skill increase, expert perception, level 2 spells {i}(one spell slot){/i}" +
            "\n{b}Level 4:{/b} Summoner feat, level 2 spells (1 slot, one spell known)" +
            "\n{b}Level 5:{/b} Ability boosts {i}(for you and your eidolon){/i}, ancestry feat, skill increase, level 3 spells (Two spell slots, but you lose all level 1 spell slots), repick all spells known" +
            "\n{b}Level 6:{/b} Summoner Feat" +
            "\n{b}Level 7:{/b} General feat, skill increase, eidolon weapon specialization {i}(your eidolon deals 2 extra additional damage with unarmed attacks){/i}, level 4 spells (Two spell slots, but you lose all level 2 spell slots), repick all spells known" +
            "\n{b}Level 8:{/b} Summoner Feat";

        [DawnsburyDaysModMainMethod]
        public static void LoadMod() {
            AddFeats(Subclasses.LoadSubclasses());
            AddFeats(CreateFeats());
        }

        private static void AddFeats(IEnumerable<Feat> feats) {
            foreach (Feat feat in feats) {
                ModManager.AddFeat(feat);
            }
        }

        private static IEnumerable<Feat> CreateFeats() {
            string[] divineTypes = new string[] { "Angel Eidolon", "Empyreal Dragon", "Diabolic Dragon", "Azata Eidolon", "Psychopmp Eidolon", "Demon Eidolon", "Devil Eidolon" };

            //string rootLocation = new FileInfo("../").Directory.Parent.Parent.FullName;
            //Directory.GetFiles(typeof(SomeTypeInsideYourDll).Location.Fullname.Directory
            //rootLocation += "/workshop/content/2693730/3315725529/CustomMods/SummonerAssets/EidolonPortraits/";
            string rootLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            rootLocation.Substring(0, rootLocation.Length - "/DawnsburyDaysSummonerClass.dll".Length);
            string extraPath = "/SummonerAssets/EidolonPortraits/";

            // Create portrait feats
            List<string> portraitDir = Directory.GetFiles(rootLocation + extraPath + "Beast")
                .Concat(Directory.GetFiles(rootLocation + extraPath + "Construct"))
                .Concat(Directory.GetFiles(rootLocation + extraPath + "Dragon"))
                .Concat(Directory.GetFiles(rootLocation + extraPath + "Elemental"))
                .Concat(Directory.GetFiles(rootLocation + extraPath + "Humanoid"))
                .Concat(Directory.GetFiles(rootLocation + extraPath + "Outsider"))
                .Concat(Directory.GetFiles(rootLocation + extraPath + "Undead"))
                .Concat(Directory.GetFiles(rootLocation + extraPath + "ConvertedBaseGameAssets/Beast"))
                .Concat(Directory.GetFiles(rootLocation + extraPath + "ConvertedBaseGameAssets/Construct"))
                .Concat(Directory.GetFiles(rootLocation + extraPath + "ConvertedBaseGameAssets/Dragon"))
                .Concat(Directory.GetFiles(rootLocation + extraPath + "ConvertedBaseGameAssets/Elemental"))
                .Concat(Directory.GetFiles(rootLocation + extraPath + "ConvertedBaseGameAssets/Humanoid"))
                .Concat(Directory.GetFiles(rootLocation + extraPath + "ConvertedBaseGameAssets/Outsider"))
                .Concat(Directory.GetFiles(rootLocation + extraPath + "ConvertedBaseGameAssets/Undead"))
                .ToList();

            List<string> nonImages = new List<string>();
            foreach (string file in portraitDir) {
                if (!file.EndsWith(".png")) {
                    nonImages.Add(file);
                }
            }
            while (nonImages.Count > 0) {
                portraitDir.Remove(nonImages.Last());
                nonImages.Remove(nonImages.Last());
            }

            foreach (string file in portraitDir) {
                string clippedDir = file.Substring(rootLocation.Length + 1);
                //throw new Exception(clippedDir);

                portraits.Add(new ModdedIllustration(clippedDir));
            }

            List<Feat> portraitFeatList = new List<Feat>();

            foreach (ModdedIllustration portrait in portraits) {
                Trait category = Trait.None;
                string featName = DirToFeatName(portrait.Filename, out category);
                portraitFeatList.Add(new Feat(ModManager.RegisterFeatName(featName), "", "", new List<Trait>() { tPortrait, category }, null).WithIllustration(portrait));
                yield return portraitFeatList.Last();
            }

            // Create portrait category feats
            yield return new Feat(ModManager.RegisterFeatName("BeastPortraits", "Category: Beast"), "", "", new List<Trait>() { tPortraitCategory }, portraitFeatList.Where(ft => ft.HasTrait(Trait.Beast)).ToList());
            yield return new Feat(ModManager.RegisterFeatName("ConstructPortraits", "Category: Construct"), "", "", new List<Trait>() { tPortraitCategory }, portraitFeatList.Where(ft => ft.HasTrait(Trait.Construct)).ToList());
            yield return new Feat(ModManager.RegisterFeatName("DragonPortraits", "Category: Dragon"), "", "", new List<Trait>() { tPortraitCategory }, portraitFeatList.Where(ft => ft.HasTrait(Trait.Dragon)).ToList());
            yield return new Feat(ModManager.RegisterFeatName("ElementalPortraits", "Category: Elemental"), "", "", new List<Trait>() { tPortraitCategory }, portraitFeatList.Where(ft => ft.HasTrait(Trait.Elemental)).ToList());
            yield return new Feat(ModManager.RegisterFeatName("HumanoidPortraits", "Category: Humanoid"), "", "", new List<Trait>() { tPortraitCategory }, portraitFeatList.Where(ft => ft.HasTrait(Trait.Humanoid)).ToList());
            yield return new Feat(ModManager.RegisterFeatName("OutsiderPortraits", "Category: Outsider"), "", "", new List<Trait>() { tPortraitCategory }, portraitFeatList.Where(ft => ft.HasTrait(tOutsider)).ToList());
            yield return new Feat(ModManager.RegisterFeatName("UndeadPortraits", "Category: Undead"), "", "", new List<Trait>() { tPortraitCategory }, portraitFeatList.Where(ft => ft.HasTrait(Trait.Undead)).ToList());

            // Init class
            yield return new ClassSelectionFeat(classSummoner, SummonerFlavour, tSummoner,
                new EnforcedAbilityBoost(Ability.Charisma), 10, new Trait[5] { Trait.Unarmed, Trait.Simple, Trait.UnarmoredDefense, Trait.Reflex, Trait.Perception }, new Trait[2] { Trait.Fortitude, Trait.Will }, 3, SummonerCrunch, Subclasses.subclasses)
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(sheet => {
                sheet.AddFocusSpellAndFocusPoint(tSummoner, Ability.Charisma, spells[SummonerSpellId.EvolutionSurge]);
                sheet.AddSelectionOption(new SingleFeatSelectionOption("EidolonPortrait", "Eidolon Portrait", 1, ft => ft.HasTrait(tPortraitCategory)));
                sheet.AddSelectionOption((SelectionOption)new SingleFeatSelectionOption("EvolutionFeat", "Evolution Feat", 1, (Func<Feat, bool>)(ft => ft.HasTrait(tEvolution) && ft.HasTrait(tSummoner))));
                sheet.AddAtLevel(3, _ => _.SetProficiency(Trait.Perception, Proficiency.Expert));
                sheet.AddAtLevel(5, _ => _.AddSelectionOption(new MultipleFeatSelectionOption("EidolonASI-5", "Eidolon Ability Boosts", 5, ft => ft.HasTrait(tEidolonASI), 4)));
                sheet.AddAtLevel(9, _ => _.SetProficiency(Trait.Spell, Proficiency.Expert));
                sheet.AddAtLevel(9, _ => _.SetProficiency(Trait.Reflex, Proficiency.Expert));
                sheet.AddAtLevel(11, _ => _.SetProficiency(Trait.Fortitude, Proficiency.Master));
                sheet.AddAtLevel(11, _ => _.SetProficiency(Trait.Simple, Proficiency.Expert));
                sheet.AddAtLevel(11, _ => _.SetProficiency(Trait.Unarmed, Proficiency.Expert));
                sheet.AddAtLevel(11, _ => _.SetProficiency(Trait.UnarmoredDefense, Proficiency.Expert));
                sheet.AddAtLevel(15, _ => _.SetProficiency(Trait.Will, Proficiency.Master));
            }))
            .WithOnCreature((Action<Creature>)(creature => creature.Traits.Add(Trait.BasicallyNeverWantsToMakeBasicUnarmedStrike)));

            // Init eidolon ability boosts
            yield return new Feat(ftStrengthBoost, "Your eidolon grows stronger.",
                "Your eidolon increases its strength modifier by +1.\n\nIf your eidolon already has strength modifier of +4, this has no effect.", new List<Trait>() { tEidolonASI }, null).WithTag(Ability.Strength);
            //.WithPrerequisite(sheet => sheet.AllFeats.First(ft => ft.HasTrait(tEidolonArray)).Tag as Ability? != Ability.Strength, "You cannot raise an ability score above +4.");
            yield return new Feat(ftDexterityBoost, "Your eidolon grows fasters.",
                "Your eidolon increases its dexterity modifier by +1.\n\nIf your eidolon already has dexterity modifier of +4, this has no effect.", new List<Trait>() { tEidolonASI }, null).WithTag(Ability.Dexterity);
            yield return new Feat(ftConstitutionBoost, "Your eidolon becomes sturdier.",
                "Your eidolon increases its constitution modifier by +1.\n\nThis does not affect its max HP.", new List<Trait>() { tEidolonASI }, null).WithTag(Ability.Constitution); ;
            yield return new Feat(ftIntelligenceBoost, "Your eidolon grows more cunning.",
                "Your eidolon increases its intelligence modifier by +1.", new List<Trait>() { tEidolonASI }, null).WithTag(Ability.Intelligence); ;
            yield return new Feat(ftWisdomBoost, "Your eidolon's insticts grow sharper.",
                "Your eidolon increases its wisdom modifier by +1.", new List<Trait>() { tEidolonASI }, null).WithTag(Ability.Wisdom); ;
            yield return new Feat(ftCharismaBoost, "Your eidolon's presence grows.",
                "Your eidolon increases its charisma modifier by +1.", new List<Trait>() { tEidolonASI }, null).WithTag(Ability.Charisma); ;

            // Generate energy optinon feats
            DamageKind[] energyDamageTypes = new DamageKind[] { DamageKind.Acid, DamageKind.Cold, DamageKind.Electricity, DamageKind.Fire, DamageKind.Sonic, DamageKind.Positive, DamageKind.Negative };
            DamageKind[] alignmentDamageTypes = new DamageKind[] { DamageKind.Good, DamageKind.Evil, DamageKind.Lawful, DamageKind.Chaotic };

            // Energy heart
            foreach (DamageKind energy in energyDamageTypes.Concat(alignmentDamageTypes)) {
                Feat temp = new Feat(ModManager.RegisterFeatName("EnergyHeart" + energy.HumanizeTitleCase2(), "Energy Heart: " + energy.HumanizeTitleCase2()), "Your eidolon's corporeal form is infused with a particular element.", $"Your eidolon's chosen natural weapon deals {energy.HumanizeTitleCase2()} damage, and it gains {energy.HumanizeTitleCase2()} resistance equal to half your level (minimum 1)", new List<Trait>() { DamageToTrait(energy), tEnergyHeartDamage }, null);
                if (alignmentDamageTypes.Contains(energy)) {
                    temp.WithPrerequisite((sheet => {
                        if (sheet.AllFeats.FirstOrDefault(ft => divineTypes.Contains(ft.FeatName.HumanizeTitleCase2())) == null)
                            return false;
                        if (sheet.AllFeats.FirstOrDefault(ft => ft.HasTrait(DamageToTrait(energy)) && ft.HasTrait(tAlignment)) == null)
                            return false;
                        return true;
                    }), $"Your eidolon must be of {DamageToTrait(energy).HumanizeTitleCase2()} alignment, and celestial origin.");
                }
                yield return temp;
            }

            List<Feat> ewSubFeats = new List<Feat>();
            foreach (DamageKind energy in energyDamageTypes.Concat(alignmentDamageTypes)) {
                Feat temp = new Feat(ModManager.RegisterFeatName("EidolonsWrath" + energy.HumanizeTitleCase2(), "Eidolon's Wrath: " + energy.HumanizeTitleCase2()), "", $"The Eidolon's Wrath focus spell deals {energy.HumanizeTitleCase2()} damage", new List<Trait> { DamageToTrait(energy), tEidolonsWrathType }, null);
                if (alignmentDamageTypes.Contains(energy)) {
                    temp.WithPrerequisite(sheet => {
                        if (sheet.AllFeats.FirstOrDefault(ft => divineTypes.Contains(ft.FeatName.HumanizeTitleCase2())) == null)
                            return false;
                        if (sheet.AllFeats.FirstOrDefault(ft => ft.HasTrait(DamageToTrait(energy)) && ft.HasTrait(tAlignment)) == null)
                            return false;
                        return true;
                    }, $"Your eidolon must be of {DamageToTrait(energy).HumanizeTitleCase2()} alignment, and celestial origin.");
                }
                ewSubFeats.Add(temp);
            }

            // Init TrueFeats
            yield return new TrueFeat(ftAbundantSpellcasting1, 1, "Your strong connect to your eidolon grants you additional spells.",
                "You gain an extra level 1 spell slot, and learn a 1st level spell based on your spellcasting tradition:\n" +
                "• Arcane. " + AllSpells.CreateModernSpellTemplate(SpellId.MageArmor, tSummoner).ToSpellLink() +
                "\n• Divine. " + AllSpells.CreateModernSpellTemplate(SpellId.Bless, tSummoner).ToSpellLink() +
                "\n• Primal. " + AllSpells.CreateModernSpellTemplate(SpellId.Grease, tSummoner).ToSpellLink() +
                "\n• Occult. " + AllSpells.CreateModernSpellTemplate(SpellId.Fear, tSummoner).ToSpellLink() +
                "\n\nUnlike your other summoner spells, the spell you gain from this feat is not a signature spell.",
                new Trait[2] { tSummoner, Trait.Homebrew })
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                if (!values.SpellRepertoires.ContainsKey(tSummoner))
                    return;
                ++values.SpellRepertoires[tSummoner].SpellSlots[1];
            }));

            yield return new TrueFeat(ftAbundantSpellcasting4, 4, "Your strong connect to your eidolon grants you additional spells.",
                "You gain an extra level 2 spell slot, and learn a 2nd level spell based on your spellcasting tradition:\n" +
                "• Arcane. " + AllSpells.CreateModernSpellTemplate(SpellId.Blur, tSummoner).ToSpellLink() +
                "\n• Divine. " + AllSpells.CreateModernSpellTemplate(SpellId.BloodVendetta, tSummoner).ToSpellLink() +
                "\n• Primal. " + AllSpells.CreateModernSpellTemplate(SpellId.Barkskin, tSummoner).ToSpellLink() +
                "\n• Occult. " + AllSpells.CreateModernSpellTemplate(SpellId.HideousLaughter, tSummoner).ToSpellLink() +
                "\n\nUnlike your other summoner spells, the spell you gain from this feat is not a signature spell.",
                new Trait[2] { tSummoner, Trait.Homebrew })
            .WithOnSheet((Action<CalculatedCharacterSheetValues>)(values => {
                if (!values.SpellRepertoires.ContainsKey(tSummoner))
                    return;
                ++values.SpellRepertoires[tSummoner].SpellSlots[2];
            }));

            yield return new EvolutionFeat(ftAirbornForm, 1, "Your eidolon can take to the skies, either via great wings, a blimp like appendage or levitation.",
                "Your eidolon can fly. It gains a fly Speed equal to its Speed.", new Trait[] { Trait.Homebrew, tSummoner }, e => e.AddQEffect(new QEffect("Airborn Form", "Your eidolon ignores difficult terrain and and can fly over lava and water.") { Id = QEffectId.Flying }), null);

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Advanced Weaponry"), 1, "Your eidolon's attack evolves.", "Choose one of your eidolon's starting melee unarmed attacks. " +
                "It gains one of the following traits, chosen when you gain the feat: disarm, grapple, shove, trip, or versatile piercing or slashing.", new Trait[] { tSummoner }, e => e.AddQEffect(new QEffect {
                    StartOfCombat = (async (qf) => {
                        string atkType = GetSummoner(qf.Owner).PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault((Func<Feat, bool>)(ft => ft.HasTrait(tAdvancedWeaponryAtkType))).Name;
                        Item naturalWeapon;
                        if (atkType == "Primary Unarmed Attack")
                            naturalWeapon = qf.Owner.UnarmedStrike;
                        else
                            naturalWeapon = qf.Owner.QEffects.FirstOrDefault(qf => qf.AdditionalUnarmedStrike != null && qf.AdditionalUnarmedStrike.WeaponProperties.Melee).AdditionalUnarmedStrike;

                        List<Feat> test = GetSummoner(qf.Owner).PersistentCharacterSheet.Calculated.AllFeats;
                        string traitName = GetSummoner(qf.Owner).PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault((Func<Feat, bool>)(ft => ft.HasTrait(tAdvancedWeaponryAtkTrait))).Name;
                        Trait trait;
                        switch (traitName) {
                            case "Disarm":
                                trait = Trait.Disarm;
                                break;
                            case "Grapple":
                                trait = tGrapple;
                                break;
                            case "Shove":
                                trait = Trait.Shove;
                                break;
                            case "Trip":
                                trait = Trait.Trip;
                                break;
                            case "Versatile Piercing":
                                trait = Trait.VersatileP;
                                break;
                            case "Versatile Slashing":
                                trait = Trait.VersatileS;
                                break;
                            case "Versatile Bludgeoning":
                                trait = Trait.VersatileB;
                                break;
                            default:
                                trait = Trait.None;
                                break;
                        }

                        if (naturalWeapon.HasTrait(trait)) {
                            return;
                        }

                        naturalWeapon.Traits.Add(trait);
                    })
                }), new List<Feat> {
                new Feat(ModManager.RegisterFeatName("AW_PrimaryUnarmedAttack", "Primary Unarmed Attack"), "", "This evolution will apply to your eidolon's primary natural weapon attack.", new List<Trait>() { tAdvancedWeaponryAtkType }, null)
                .WithOnSheet(sheet => {
                    sheet.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("AdvancedWeaponryTrait", "Eidolon Advanced Weaponry Trait", sheet.CurrentLevel, (Func<Feat, bool>)(ft => ft.HasTrait(tAdvancedWeaponryAtkTrait))));
                }),
                new Feat(ModManager.RegisterFeatName("AW_SecondaryUnarmedAttack", "Secondary Unarmed Attack"), "", "This evolution will apply to your eidolon's secondary natural weapon attack.", new List<Trait>() { tAdvancedWeaponryAtkType }, null)
                .WithOnSheet(sheet => {
                    sheet.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("AdvancedWeaponryTrait", "Eidolon Advanced Weaponry Trait", sheet.CurrentLevel, (Func<Feat, bool>)(ft => ft.HasTrait(tAdvancedWeaponryAtkTrait))));
                })
            });

            yield return new TrueFeat(ModManager.RegisterFeatName("LifelinkSurgeFeat", "Lifelink Surge"), 4, "", "You learn the lifelink surge link spell. Increase the number of Focus Points in your focus pool by 1.", new Trait[] { tSummoner }, null).WithOnSheet(sheet => {
                sheet.AddFocusSpellAndFocusPoint(tSummoner, Ability.Charisma, spells[SummonerSpellId.LifelinkSurge]);
            })
            .WithRulesBlockForSpell(spells[SummonerSpellId.LifelinkSurge], tSummoner);

            yield return new TrueFeat(ModManager.RegisterFeatName("ExtendBoostFeat", "Extend Boost"), 1, "You can increase the duration of your eidolon's boosts.", "You learn the extend boost link spell. Increase the number of Focus Points in your focus pool by 1.",
                new Trait[] { tSummoner }, null).WithOnSheet(sheet => {
                    sheet.AddFocusSpellAndFocusPoint(tSummoner, Ability.Charisma, spells[SummonerSpellId.ExtendBoost]);
                })
            .WithRulesBlockForSpell(spells[SummonerSpellId.ExtendBoost], tSummoner);

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Alacritous Action"), 2, "Your eidolon moves more quickly.", "Your eidolon gains a +10-foot status bonus to its Speed.", new Trait[] { tSummoner }, e => e.AddQEffect(new QEffect {
                BonusToAllSpeeds = (qf => {
                    return new Bonus(2, BonusType.Status, "Alacritous Action");
                })
            }), null);

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Tandem Movement {icon:FreeAction}"), 4, "You and your eidolon move together.", "You and your eidolon gain the Tandem Movement action. After toggling on this action, your next action must be to stride. " +
                "Then, your bonded partner gains an immediate turn where they can do the same.", new Trait[] { tSummoner, tTandem }, e => e.AddQEffect(new QEffect {
                    ProvideActionIntoPossibilitySection = (qf, section) => {
                        if (section.Name == "Tandem Actions") {
                            return GenerateTandemMovementAction(qf.Owner, GetSummoner(qf.Owner), GetSummoner(qf.Owner));
                        }
                        return null;
                    }
                }), null)
            .WithOnCreature((sheet, self) => {
                self.AddQEffect(new QEffect {
                    ProvideActionIntoPossibilitySection = (qf, section) => {
                        Creature eidolon = GetEidolon(qf.Owner);
                        if (eidolon != null) {
                            if (section.Name == "Tandem Actions") {
                                return GenerateTandemMovementAction(qf.Owner, GetSummoner(qf.Owner), GetSummoner(qf.Owner));
                            }
                        }
                        return null;
                    }
                });
            })
            .WithIllustration(illTandemMovement);

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Tandem Strike {icon:FreeAction}"), 6, "You and your eidolon strike together.",
                "You and your eidolon each attack, seamlessly targeting the same foe without interfering with each other's movements. Your eidolon makes a melee Strike, " +
                "and then you make a melee Strike against the same creature. Both attacks count toward your multiple attack penalty, but the penalty doesn't increase until " +
                "after both attacks have been made.", new Trait[] { tSummoner, tTandem }, e => e.AddQEffect(new QEffect {
                    ProvideActionIntoPossibilitySection = (qf, section) => {
                        if (section.Name == "Tandem Actions") {
                            return GenerateTandemStrikeAction(qf.Owner, GetSummoner(qf.Owner), GetSummoner(qf.Owner));
                        }
                        return null;
                    }
                }), null)
            .WithOnCreature((sheet, self) => {
                self.AddQEffect(new QEffect {
                    ProvideActionIntoPossibilitySection = (qf, section) => {
                        Creature eidolon = GetEidolon(qf.Owner);
                        if (eidolon != null) {
                            if (section.Name == "Tandem Actions") {
                                return GenerateTandemStrikeAction(qf.Owner, GetSummoner(qf.Owner), GetSummoner(qf.Owner));
                            }
                        }
                        return null;
                    }
                });
            })
            .WithIllustration(illTandemStrike);

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Eidolon's Wrath {icon:TwoActions}"), 6, "Your eidolon gains the ability to expel its planar essence in a surge of destructive energy.",
                "Your eidolon releases a powerful energy attack that deals 5d6 damage of the type you chose when you took the Eidolon's Wrath feat, with a basic Reflex save.",
                new Trait[] { tSummoner }, e => {
                    Feat dmgTypeFeat = GetSummoner(e).PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft.HasTrait(tEidolonsWrathType));
                    if (dmgTypeFeat != null) {
                        e.AddQEffect(new QEffect() { Id = qfEidolonsWrath, Tag = TraitToDamage(dmgTypeFeat.Traits[0]) });
                        e.Spellcasting.PrimarySpellcastingSource.FocusSpells.Add(AllSpells.CreateSpellInCombat(spells[SummonerSpellId.EidolonsWrath], e, 3, tSummoner));
                    }
                }, ewSubFeats)
            .WithOnSheet(sheet => {
                if (sheet.FocusPointCount < 3) {
                    sheet.FocusPointCount += 1;
                }
            })
            .WithRulesBlockForSpell(spells[SummonerSpellId.EidolonsWrath], tSummoner)
            .WithIllustration(IllustrationName.DivineWrath);

            // Generate spell selection feats
            List<Spell> allSpells = AllSpells.All.Where(sp => (sp.HasTrait(Trait.Cantrip) || sp.SpellLevel <= 2) && !sp.HasTrait(Trait.Focus) && !sp.HasTrait(Trait.Uncommon) && new Trait[] { Trait.Arcane, Trait.Occult, Trait.Primal, Trait.Divine }.ContainsOneOf(sp.Traits)).ToList();

            List<SpellId> moddedSpells = (List<SpellId>)typeof(ModManager).GetProperty("NewSpells", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);
            List<Spell> moddedSpells2 = new List<Spell>();
            foreach (SpellId spell in (List<SpellId>)moddedSpells) {
                moddedSpells2.Add(AllSpells.CreateModernSpellTemplate(spell, tSummoner));
            }
            moddedSpells2 = moddedSpells2.Where(sp => (sp.HasTrait(Trait.Cantrip) || sp.SpellLevel <= 2) && (!sp.HasTrait(Trait.Focus) && !sp.HasTrait(Trait.Uncommon) && new Trait[] { Trait.Arcane, Trait.Occult, Trait.Primal, Trait.Divine }.ContainsOneOf(sp.Traits))).ToList();
            allSpells = allSpells.Concat(moddedSpells2).ToList();

            foreach (Spell spell in allSpells) {
                List<Trait> traits = new List<Trait>() { tEidolonSpellFeat };

                if (AllFeats.All.FirstOrDefault(ft => ft.Name == $"EidolonSpellGainFeat({spell.Name}-Cantrip)") != null) {
                    continue;
                }

                // TODO: When DLC launches, change this to spell.SpellId.ToStringOrTechnical().
                if (spell.HasTrait(Trait.Cantrip)) {
                    yield return new EvolutionFeat(ModManager.RegisterFeatName($"EidolonSpellGainFeat({spell.Name}-Cantrip)", spell.Name), spell.MinimumSpellLevel, "", AllSpells.CreateModernSpell(spell.SpellId, null, 1, false, spell.CombatActionSpell.SpellInformation).CombatActionSpell.Description, spell.Traits.ToList().Concat(traits).ToArray(), e => {
                        e.Spellcasting.PrimarySpellcastingSource.WithSpells([spell.SpellId], (e.Level + 1) / 2);
                    }, null)
                    .WithIllustration(spell.Illustration);
                }

                if (AllFeats.All.FirstOrDefault(ft => ft.Name == $"EidolonSpellGainFeat({spell.Name}-1)") != null) {
                    continue;
                }

                if (spell.MinimumSpellLevel <= 1 && !spell.HasTrait(Trait.Cantrip)) {
                    traits.Add(tEidolonSpellLvl1);
                    yield return new EvolutionFeat(ModManager.RegisterFeatName($"EidolonSpellGainFeat({spell.Name}-1)", spell.Name), spell.MinimumSpellLevel, "", AllSpells.CreateModernSpell(spell.SpellId, null, 1, false, spell.CombatActionSpell.SpellInformation).CombatActionSpell.Description, spell.Traits.ToList().Concat(traits).ToArray(), e => {
                        e.Spellcasting.PrimarySpellcastingSource.WithSpells(new SpellId[] { spell.SpellId });
                    }, null)
                    .WithIllustration(spell.Illustration);
                }
                traits.Remove(tEidolonSpellLvl1);

                if (AllFeats.All.FirstOrDefault(ft => ft.Name == $"EidolonSpellGainFeat({spell.Name}-2)") != null) {
                    continue;
                }

                if (spell.MinimumSpellLevel <= 2 && !spell.HasTrait(Trait.Cantrip)) {
                    traits.Add(tEidolonSpellLvl2);
                    yield return new EvolutionFeat(ModManager.RegisterFeatName($"EidolonSpellGainFeat({spell.Name}-2)", spell.Name), spell.MinimumSpellLevel, "", AllSpells.CreateModernSpell(spell.SpellId, null, 1, false, spell.CombatActionSpell.SpellInformation).CombatActionSpell.Description, spell.Traits.ToList().Concat(traits).ToArray(), e => {
                        e.Spellcasting.PrimarySpellcastingSource.WithSpells(null, new SpellId[] { spell.SpellId });
                    }, null)
                    .WithIllustration(spell.Illustration);
                }
                traits.Remove(tEidolonSpellLvl2);
            }

            yield return new EvolutionFeat(ftMagicalUnderstudy, 2, "Your eidolon evolves to cast spells.",
                "Your eidolon gains the Cast a Spell activity and learns two cantrips of its tradition, which it can cast as innate spells.\n\nYour eidolon's spell DC and attack moddifier for these spells is equal to yours.",
                new Trait[] { tSummoner }, (Action<Creature>)null, null)
            .WithOnSheet(sheet => {
                if (sheet.HasFeat(scFeyEidolon)) {
                    sheet.AddSelectionOptionRightNow(new MultipleFeatSelectionOption("EidolonCantrip", "Eidolon Cantrips", -1, ft => ft.HasTrait(tEidolonSpellFeat) && ft.HasTrait(Trait.Cantrip) &&
                    (ft.HasTrait(sheet.SpellRepertoires[tSummoner].SpellList) || (ft.HasTrait(Trait.Arcane) && (ft.HasTrait(Trait.Enchantment) || ft.HasTrait(Trait.Illusion) || ft.HasTrait(Trait.Mental)))), 2));
                } else {
                    sheet.AddSelectionOptionRightNow(new MultipleFeatSelectionOption("EidolonCantrip", "Eidolon Cantrips", -1, ft => ft.HasTrait(tEidolonSpellFeat) && ft.HasTrait(Trait.Cantrip) && ft.HasTrait(sheet.SpellRepertoires[tSummoner].SpellList), 2));
                }
            });

            yield return new EvolutionFeat(ftMagicalAdept, 8, "Your eidolon gains more magic.",
                "Choose one 2nd-level spell and one 1st-level spell of your eidolon's tradition. Your eidolon can cast them each once per day as innate spells.",
                new Trait[] { tSummoner }, (Action<Creature>)null, null)
            .WithOnSheet(sheet => {
                if (sheet.HasFeat(scFeyEidolon)) {
                    sheet.AddSelectionOptionRightNow(new MultipleFeatSelectionOption("Eidolon2ndLevelSpell", "Level 2 Eidolon Spell", -1, ft => ft.HasTrait(tEidolonSpellFeat) &&
                    ft.HasTrait(tEidolonSpellLvl2) && (ft.HasTrait(sheet.SpellRepertoires[tSummoner].SpellList) || (ft.HasTrait(Trait.Arcane) && (ft.HasTrait(Trait.Enchantment) || ft.HasTrait(Trait.Illusion) || ft.HasTrait(Trait.Mental)))), 1));
                    sheet.AddSelectionOptionRightNow(new MultipleFeatSelectionOption("Eidolon1stLevelSpell", "Level 1 Eidolon Spell", -1, ft => ft.HasTrait(tEidolonSpellFeat) &&
                    ft.HasTrait(tEidolonSpellLvl1) && (ft.HasTrait(sheet.SpellRepertoires[tSummoner].SpellList) || (ft.HasTrait(Trait.Arcane) && (ft.HasTrait(Trait.Enchantment) || ft.HasTrait(Trait.Illusion) || ft.HasTrait(Trait.Mental)))), 1));
                } else {
                    sheet.AddSelectionOptionRightNow(new MultipleFeatSelectionOption("Eidolon2ndLevelSpell", "Level 2 Eidolon Spell", -1, ft => ft.HasTrait(tEidolonSpellFeat) && ft.HasTrait(tEidolonSpellLvl2) && ft.HasTrait(sheet.SpellRepertoires[tSummoner].SpellList), 1));
                    sheet.AddSelectionOptionRightNow(new MultipleFeatSelectionOption("Eidolon1stLevelSpell", "Level 1 Eidolon Spell", -1, ft => ft.HasTrait(tEidolonSpellFeat) && ft.HasTrait(tEidolonSpellLvl1) && ft.HasTrait(sheet.SpellRepertoires[tSummoner].SpellList), 1));
                }
            });

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Eidolon's Opportunity {icon:Reaction}"), 6,
                "Your eidolon makes a melee Strike against the triggering creature.", "If the attack is a critical hit and the trigger was a manipulate action, " +
                "your eidolon disrupts that action. This Strike doesn't count toward your multiple attack penalty, and your multiple attack penalty doesn't apply to this Strike.",
                new Trait[] { tSummoner }, e => e.AddQEffect(QEffect.AttackOfOpportunity("Eidolon's Opportunity", "Can make attacks of opportunity, and disrupt actions on a critical hit.", null, false)), null);

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Constricting Hold {icon:Action}"), 8,
                "Your eidolon develops a long serpentine appendage, or a powerful choking grip, perfect for constricting the life out of its victims.", "Your eidolon constricts the creature, dealing bludgeoning damage equal to your eidolon's level plus its Strength modifier, with a basic Fortitude save against your spell DC.",
                new Trait[] { tSummoner }, e => e.AddQEffect(new QEffect("Constricting Hold", "Your eidolon can crush grabbed opponents.") {
                    ProvideContextualAction = qf => {
                        List<Creature> grappledCreatures = qf.Owner.Battle.AllCreatures.Where(c => c.OwningFaction != e.OwningFaction && c.HasEffect(QEffectId.Grappled) && c.FindQEffect(QEffectId.Grappled).Source == e).ToList();
                        if (grappledCreatures.Count > 0) {
                            int saveDC = GetSummoner(e).ClassOrSpellDC();
                            return (Possibility)(ActionPossibility)new CombatAction(e, illConstrictingHold, "Constricting Hold", new Trait[] { },
                                "{b}Target{/b} 1 creature currently grappled or restrained by your eidolon\n{b}Saving throw{/b} basic Fortitude\n\nDeal level + Strength modifier bludgeoning damage (basic Fortitude save mitigates) to the grappled creature.",
                                (Target) Target.Distance(1).WithAdditionalConditionOnTargetCreature(new GrappledCreatureOnlyCreatureTargetingRequirement()))
                            .WithActionCost(1)
                            .WithSoundEffect(SfxName.Boneshaker)
                            .WithSavingThrow(new SavingThrow(Defense.Fortitude, (_ => saveDC)))
                            .WithEffectOnChosenTargets(async (action, self, targets) => {
                                await CommonSpellEffects.DealBasicDamage(action, self, targets.ChosenCreature, action.CheckResult, DiceFormula.FromText($"{e.Level + e.Abilities.Strength}"), DamageKind.Bludgeoning);
                            });
                        }
                        return null;
                    },
                }), null)
                .WithIllustration(illConstrictingHold);

            yield return new TrueFeat(ftBoostSummons, 8, "Augmenting your eidolon extends to creatures you summon.",
                $"When you cast {AllSpells.CreateSpellLink(spells[SummonerSpellId.EidolonBoost], tSummoner)} or {AllSpells.CreateSpellLink(spells[SummonerSpellId.ReinforceEidolon], tSummoner)}, " +
                "in addition to your eidolon, it also targets your summoned creatures within 60 feet.", new Trait[] { tSummoner }, null)
            .WithIllustration(illReinforceEidolon);

            yield return new TrueFeat(ModManager.RegisterFeatName("Master Summoner"), 6, "You've become particularly adept at calling upon the aid of lesser beings, in addition to your eidolon.", "You gain an additional slot of your spell level, that can only be used to cast summon spells.", new Trait[] { tSummoner }, null)
            .WithOnSheet(sheet => {
                sheet.SpellRepertoires[tSummoner].SpellSlots[sheet.MaximumSpellLevel]++;
            })
            .WithOnCreature((sheet, self) => {
                self.AddQEffect(new QEffect("Master Summoner", "You gain an additional max level spell slot that can only be used to cast summon spells.") {
                    StartOfCombat = async qf => {
                        if (qf.Owner.PersistentUsedUpResources.UsedUpActions.Contains("Master Summoner")) {
                            qf.Name = "Master Summoner (Expended)";
                        }
                        return;
                    },
                    AfterYouTakeAction = async (qf, action) => {
                        if (action.SpellId == SpellId.None) {
                            return;
                        }

                        if (!action.Name.StartsWith("Summon ") && action.Name != "Animate Dead") {
                            return;
                        }

                        if (action.SpellLevel != action.Owner.PersistentCharacterSheet.Calculated.MaximumSpellLevel) {
                            return;
                        }

                        qf.Owner.PersistentUsedUpResources.UsedUpActions.Add("Master Summoner");
                        qf.Name = "Master Summoner (Expended)";
                    },
                    PreventTakingAction = action => {
                        if (action.Owner.PersistentUsedUpResources.UsedUpActions.Contains("Master Summoner")) {
                            return null;
                        }

                        if (action.SpellId == SpellId.None) {
                            return null;
                        }

                        if (action.Name.StartsWith("Summon ") || action.Name == "Animate Dead") {
                            return null;
                        }

                        if (action.SpellLevel != action.Owner.PersistentCharacterSheet.Calculated.MaximumSpellLevel) {
                            return null;
                        }

                        if (action.Owner.Spellcasting.PrimarySpellcastingSource.SpontaneousSpellSlots[action.Owner.PersistentCharacterSheet.Calculated.MaximumSpellLevel] != 1) {
                            return null;
                        }

                        return "This spell slot can only be used to cast summoning spells.";
                    },
                });
            });

            yield return new TrueFeat(ModManager.RegisterFeatName("Ostentatious Arrival {icon:FreeAction}"), 6, "Your summons manifest in an explosive wave of destructive energy.",
                "If the next action you take is to Manifest your Eidolon as a three-action activity, or to Cast a three-action summoning Spell, the creature appears in an explosion. " +
                "All creatures in a 10-foot emanation around the creature you summoned or manifested take 1d4 fire damage per spell level for a summoning spell, or 1d4 damage per 2 levels for Manifesting your Eidolon. " +
                "If your eidolon has an elemental trait, they deal that damage type instead.",
                new Trait[] { tSummoner, Trait.Concentrate, Trait.Metamagic, Trait.Manipulate }, null)
            .WithOnSheet(sheet => {
                sheet.SpellRepertoires[tSummoner].SpellSlots[sheet.MaximumSpellLevel]++;
            })
            .WithIllustration(illOstentatiousArrival)
            .WithOnCreature((sheet, self) => {
                self.AddQEffect(new QEffect() {
                    ProvideMainAction = qf => {
                        Possibility output = null;
                        if (qf.Owner.HasEffect(qfOstentatiousArrival)) {
                            output = (ActionPossibility)new CombatAction(qf.Owner, illOstentatiousArrival, "Cancel Ostentatious Arrival", new Trait[] { tSummoner }, "Cancel ostentatious arrival, to cast a non-explosive summoning spell instead.", Target.Self())
                            .WithEffectOnSelf(self => self.RemoveAllQEffects(effect => effect.Id == qfOstentatiousArrival))
                            .WithSoundEffect(SfxName.Button)
                            .WithActionCost(0);

                            output.WithPossibilityGroup("Summoner");
                            return output;
                        }

                        output = (ActionPossibility)new CombatAction(qf.Owner, illOstentatiousArrival, "Ostentatious Arrival", new Trait[] { tSummoner, Trait.Concentrate, Trait.Metamagic, Trait.Manipulate },
                            "If the next action you take is to Manifest your Eidolon as a three-action activity, or to Cast a three-action summoning Spell, the creature appears in an explosion. " +
                            "All creatures in a 10-foot emanation around the creature you summoned or manifested take 1d4 fire damage per spell level for a summoning spell, or 1d4 damage per 2 levels for Manifesting your Eidolon. " +
                            "If your eidolon has an elemental trait, they deal that damage type instead.", Target.Self())
                        .WithSoundEffect(SfxName.Abjuration)
                        .WithActionCost(0)
                        .WithEffectOnSelf(self => {
                            self.AddQEffect(new QEffect("Ostentatious Arrival Toggled", "Your next summoning spell will create a 10-foot burst, dealing 1d4 fire damage to all creatures caught inside.") {
                                Id = qfOstentatiousArrival,
                                PreventTakingAction = action => {
                                    if (action.Name == "Cancel Ostentatious Arrival" || action.Name == "Manifest Eidolon" || action.Name.StartsWith("Summon ") || action.Name == "Animate Dead") {
                                        return null;
                                    }

                                    return "Ostentatious Arrival can only be used with summon spells, or the manifest eidolon action.";
                                },
                                YouBeginAction = async (qf, action) => {
                                    // Check if summon spell
                                    if ((action.SpellId == SpellId.None || !(action.Name.StartsWith("Summon ") || action.Name == "Animate Dead")) && action.Name != "Manifest Eidolon") {
                                        return;
                                    }

                                    Tile target = action.ChosenTargets.ChosenTile;
                                    if (target != null) {
                                        string damage = "";
                                        DamageKind type = DamageKind.Fire;
                                        Trait? element = action.Traits.FirstOrDefault(t => TraitToDamage(t) != DamageKind.Untyped);
                                        if (action.Name == "Manifest Eidolon") {
                                            damage = $"{(qf.Owner.Level + 1) / 2}d4";
                                            Feat energyHeart = qf.Owner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft.HasTrait(tEnergyHeartDamage));
                                            if (energyHeart != null) {
                                                type = TraitToDamage(energyHeart.Traits[0]);
                                            } else if (element != null) {
                                                type = TraitToDamage((Trait)element);
                                            }
                                        } else {
                                            damage = $"{action.SpellLevel}d4";
                                        }
                                        Sfxs.Play(SfxName.Fireball, 0.5f);
                                        BurstAreaTarget burst = new BurstAreaTarget(0, 2);
                                        await CommonAnimations.CreateConeAnimation(qf.Owner.Battle, target.ToCenterVector(), DetermineTilesCopy(qf.Owner, burst, target.ToCenterVector()).TargetedTiles.ToList(), 20, ProjectileKind.Cone, IllustrationName.Fireball);
                                        // Target
                                        foreach (Creature creature in qf.Owner.Battle.AllCreatures) {
                                            if (creature.DistanceTo(target) <= 2) {
                                                List<QEffect> effects = creature.QEffects.ToList();
                                                for (int i = 0; i < effects.Count; i++) {
                                                    if (effects[i].YouAreTargeted != null) {
                                                        await effects[i].YouAreTargeted(effects[i], action);
                                                    }
                                                }
                                            }
                                        }
                                        // Damage
                                        foreach (Creature creature in qf.Owner.Battle.AllCreatures) {
                                            if (creature.DistanceTo(target) <= 2) {
                                                await CommonSpellEffects.DealDirectDamage(action, DiceFormula.FromText(damage), creature, CheckResult.Success, type);
                                            }
                                        }
                                        // Resolve target
                                        foreach (Creature creature in qf.Owner.Battle.AllCreatures) {
                                            if (creature.DistanceTo(target) <= 2) {
                                                List<QEffect> effects = creature.QEffects.ToList();
                                                for (int i = 0; i < effects.Count; i++) {
                                                    if (effects[i].AfterYouAreTargeted != null) {
                                                        await effects[i].AfterYouAreTargeted(effects[i], action);
                                                    }
                                                }
                                            }
                                        }
                                        qf.ExpiresAt = ExpirationCondition.Immediately;
                                    }
                                }
                            });
                        });

                        output.WithPossibilityGroup("Summoner");
                        return output;
                    },
                    AfterYouTakeAction = async (qf, action) => {
                        if (action.SpellId == SpellId.None) {
                            return;
                        }

                        if (!action.Name.StartsWith("Summon ") && action.Name != "Animate Dead") {
                            return;
                        }

                        if (action.SpellLevel != action.Owner.PersistentCharacterSheet.Calculated.MaximumSpellLevel) {
                            return;
                        }

                        qf.Owner.PersistentUsedUpResources.UsedUpActions.Add("Master Summoner");
                        qf.Name = "Master Summoner (Expended)";
                    },
                    PreventTakingAction = action => {
                        if (action.Owner.PersistentUsedUpResources.UsedUpActions.Contains("Master Summoner")) {
                            return null;
                        }

                        if (action.SpellId == SpellId.None) {
                            return null;
                        }

                        if (action.Name.StartsWith("Summon ") || action.Name == "Animate Dead") {
                            return null;
                        }

                        if (action.SpellLevel != action.Owner.PersistentCharacterSheet.Calculated.MaximumSpellLevel) {
                            return null;
                        }

                        if (action.Owner.Spellcasting.PrimarySpellcastingSource.SpontaneousSpellSlots[action.Owner.PersistentCharacterSheet.Calculated.MaximumSpellLevel] != 1) {
                            return null;
                        }

                        return "This spell slot can only be used to cast summoning spells.";
                    },
                });
            });

            yield return new TrueFeat(ModManager.RegisterFeatName("Reinforce Eidolon"), 2, "You buffer your eidolon.", "You gain the reinforce eidolon link cantrip.", new Trait[] { tSummoner }, null)
            .WithOnSheet(sheet => {
                if (!sheet.SpellRepertoires.ContainsKey(tSummoner)) {
                    return;
                }
                sheet.SpellRepertoires[tSummoner].SpellsKnown.Add(AllSpells.CreateModernSpellTemplate(spells[SummonerSpellId.ReinforceEidolon], tSummoner, sheet.MaximumSpellLevel));
            })
            .WithRulesBlockForSpell(spells[SummonerSpellId.ReinforceEidolon], tSummoner);

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Energy Heart"), 1, "Your eidolon's heart beats with energy.",
                "Choose an energy damage type other than force. One of your eidolon's unarmed attacks changes its damage type to the chosen type, and it gains resistance to that type equal to half your level (minimum 1).",
                new Trait[] { tSummoner }, e => e.AddQEffect(new QEffect {
                StartOfCombat = (async (qf) => {
                    DamageKind kind = TraitToDamage(GetSummoner(qf.Owner).PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft.HasTrait(tEnergyHeartDamage)).Traits[0]);
                    qf.Owner.WeaknessAndResistance.AddResistance(kind, Math.Max(1, (qf.Owner.Level / 2)));
                })
            }), new List<Feat> {
                new Feat(ModManager.RegisterFeatName("EH_PrimaryUnarmedAttack", "Primary Unarmed Attack"), "", "This evolution will change the damage type of your eidolon's primary natural weapon attack.", new List<Trait>() { tEnergyHeartWeapon }, null).WithOnSheet(sheet => {
                sheet.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("EnergyHeartType", "Energy Heart Type", sheet.CurrentLevel, (Func<Feat, bool>)(ft => ft.HasTrait(tEnergyHeartDamage))));
            }),
                new Feat(ModManager.RegisterFeatName("EH_SecondaryUnarmedAttack", "Secondary Unarmed Attack"), "", "This evolution will change the damage type of your eidolon's secondary natural weapon attack.", new List<Trait>() { tEnergyHeartWeapon }, null).WithOnSheet(sheet => {
                sheet.AddSelectionOptionRightNow((SelectionOption)new SingleFeatSelectionOption("EnergyHeartType", "Energy Heart Type", sheet.CurrentLevel, (Func<Feat, bool>)(ft => ft.HasTrait(tEnergyHeartDamage))));
            })
            });

            yield return new EvolutionFeat(ModManager.RegisterFeatName("Bloodletting Claws"), 4,
                "Your eidolon inflicts bleeding wounds on a telling blow.",
                "If your eidolon critically hits with a melee unarmed Strike that deals slashing or piercing damage, its target takes 1d6 persistent bleed damage. " +
                "Your eidolon gains an item bonus to this bleed damage equal to the unarmed attack's item bonus to attack rolls.", new Trait[] { tSummoner }, e => e.AddQEffect(new QEffect {
                AfterYouDealDamageOfKind = (async (self, action, damageType, target) => {
                    if (!action.HasTrait(Trait.Strike) || !action.HasTrait(Trait.Unarmed)) {
                        return;
                    }

                    int bonus = self.UnarmedStrike.WeaponProperties.ItemBonus;

                    if ((damageType == DamageKind.Slashing || damageType == DamageKind.Piercing) && action.CheckResult == CheckResult.CriticalSuccess) {
                        target.AddQEffect(QEffect.PersistentDamage("1d6" + (bonus > 0 ? $"+{bonus}" : ""), DamageKind.Bleed));
                    }
                })
            }), null);

            yield return new EvolutionFeat(ModManager.RegisterFeatName("RangedCombatant", "Ranged Combatant"), 2, "Spines, flame jets, and holy blasts are just some of the ways your eidolon might strike from a distance.",
                "Your eidolon gains a ranged unarmed attack with a range increment of 30 feet that deals 1d4 damage and has the magical and propulsive traits." +
                " When you select this feat, choose a damage type: acid, bludgeoning, cold, electricity, fire, negative, piercing, positive, or slashing." +
                " If your eidolon is a celestial, fiend, or monitor with an alignment other than true neutral, you can choose a damage type in its alignment.", new Trait[] { tSummoner }, null, new List<Feat> {
                new EvolutionFeat(ModManager.RegisterFeatName("Acid_RangedCombatant", "Acid"), 1, "", "Your eidolon's ranged attack deals acid damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.AcidArrow, "Acid Spit", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Acid) {
                        Sfx = SfxName.AcidSplash,
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.AcidArrow)
                    }.WithRangeIncrement(6)
                )}), null),
                new EvolutionFeat(ModManager.RegisterFeatName("Bludgeoning_RangedCombatant", "Bludgeoning"), 1, "", "Your eidolon's ranged attack deals bludgeoning damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.TelekineticProjectile, "Telekinesis", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Bludgeoning) {
                        Sfx = SfxName.PhaseBolt,
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.TelekineticProjectile)
                    }.WithRangeIncrement(6)
                )}), null),
                new EvolutionFeat(ModManager.RegisterFeatName("Cold_RangedCombatant", "Cold"), 1, "", "Your eidolon's ranged attack deals cold damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.RayOfFrost, "Chill", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Cold) {
                        Sfx = SfxName.RayOfFrost,
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.RayOfFrost)
                    }.WithRangeIncrement(6)
                )}), null),
                new EvolutionFeat(ModManager.RegisterFeatName("Electricity_RangedCombatant", "Electricity"), 1, "", "Your eidolon's ranged attack deals electricity damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.ElectricArc, "Zap", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Electricity) {
                        Sfx = SfxName.ElectricArc,
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.ElectricArc)
                    }.WithRangeIncrement(6)
                )}), null),
                new EvolutionFeat(ModManager.RegisterFeatName("Fire_RangedCombatant", "Fire"), 1, "", "Your eidolon's ranged attack deals fire damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.ProduceFlame, "Scorch", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Fire) {
                        Sfx = SfxName.FireRay,
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.ProduceFlame)
                    }.WithRangeIncrement(6)
                )}), null),
                new EvolutionFeat(ModManager.RegisterFeatName("Negative_RangedCombatant", "Negative"), 1, "", "Your eidolon's ranged attack deals negative damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.ChillTouch, "Wilt", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Negative) {
                        Sfx = SfxName.ChillTouch,
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.ChillTouch)
                    }.WithRangeIncrement(6)
                )}), null),
                new EvolutionFeat(ModManager.RegisterFeatName("Piercing_RangedCombatant", "Piercing"), 1, "", "Your eidolon's ranged attack deals piercing damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.MagneticPinions, "Shoot", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Piercing) {
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.ArrowProjectile)
                    }.WithRangeIncrement(6)
                )}), null),
                new EvolutionFeat(ModManager.RegisterFeatName("Positive_RangedCombatant", "Positive"), 1, "", "Your eidolon's ranged attack deals positive damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.DisruptUndead, "Smite", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Positive) {
                        Sfx = SfxName.DivineLance,
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.DisruptUndead)
                    }.WithRangeIncrement(6)
                )}), null),
                new EvolutionFeat(ModManager.RegisterFeatName("Slashing_RangedCombatant", "Slashing"), 1, "", "Your eidolon's ranged attack deals slashing damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.AerialBoomerang256, "Razor Wind", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Slashing) {
                        Sfx = SfxName.AeroBlade,
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.AerialBoomerang256)
                    }.WithRangeIncrement(6)
                )}), null),
                new EvolutionFeat(ModManager.RegisterFeatName("Good_RangedCombatant", "Good"), 1, "", "Your eidolon's ranged attack deals good damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.DivineLance, "Rebuke", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Good) {
                        Sfx = SfxName.DivineLance,
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.DivineLance)
                    }.WithRangeIncrement(6)
                )}), null).WithPrerequisite((sheet => {
                    if (sheet.AllFeats.FirstOrDefault(ft => divineTypes.Contains(ft.FeatName.HumanizeTitleCase2())) == null)
                       return false;
                    if (sheet.AllFeats.FirstOrDefault(ft => ft.HasTrait(Trait.Good) && ft.HasTrait(tAlignment)) == null)
                       return false;
                    return true;
                }), "Your eidolon must be of good alignment, and celestial origin."),
                new EvolutionFeat(ModManager.RegisterFeatName("Evil_RangedCombatant", "Evil"), 1, "", "Your eidolon's ranged attack deals evil damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.DivineLance, "Rebuke", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Evil) {
                        Sfx = SfxName.DivineLance,
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.DivineLance)
                    }.WithRangeIncrement(6)
                )}), null).WithPrerequisite((sheet => {
                    if (sheet.AllFeats.FirstOrDefault(ft => divineTypes.Contains(ft.FeatName.HumanizeTitleCase2())) == null)
                       return false;
                    if (sheet.AllFeats.FirstOrDefault(ft => ft.HasTrait(Trait.Evil) && ft.HasTrait(tAlignment)) == null)
                       return false;
                    return true;
                }), "Your eidolon must be of evil alignment, and celestial origin."),
                new EvolutionFeat(ModManager.RegisterFeatName("Chaotic_RangedCombatant", "Chaotic"), 1, "", "Your eidolon's ranged attack deals chaos damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.DivineLance, "Rebuke", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Chaotic) {
                        Sfx = SfxName.DivineLance,
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.DivineLance)
                    }.WithRangeIncrement(6)
                )}), null).WithPrerequisite((sheet => {
                    if (sheet.AllFeats.FirstOrDefault(ft => divineTypes.Contains(ft.FeatName.HumanizeTitleCase2())) == null)
                       return false;
                    if (sheet.AllFeats.FirstOrDefault(ft => ft.HasTrait(Trait.Chaotic) && ft.HasTrait(tAlignment)) == null)
                       return false;
                    return true;
                }), "Your eidolon must be of chaotic alignment, and celestial origin."),
                new EvolutionFeat(ModManager.RegisterFeatName("Lawful_RangedCombatant", "Lawful"), 1, "", "Your eidolon's ranged attack deals law damage.", new Trait[] {}, e => e.AddQEffect(new QEffect {
                    AdditionalUnarmedStrike = new Item(IllustrationName.DivineLance, "Rebuke", new Trait[] { Trait.Unarmed, Trait.Ranged, Trait.Magical, Trait.Propulsive }).WithWeaponProperties(new WeaponProperties("1d4", DamageKind.Lawful) {
                        Sfx = SfxName.DivineLance,
                        VfxStyle = new VfxStyle(1, ProjectileKind.Arrow, IllustrationName.DivineLance)
                    }.WithRangeIncrement(6)
                )}), null).WithPrerequisite((sheet => {
                    if (sheet.AllFeats.FirstOrDefault(ft => divineTypes.Contains(ft.FeatName.HumanizeTitleCase2())) == null)
                       return false;
                    if (sheet.AllFeats.FirstOrDefault(ft => ft.HasTrait(Trait.Lawful) && ft.HasTrait(tAlignment)) == null)
                       return false;
                    return true;
                }), "Your eidolon must be of lawful alignment, and celestial origin.")
            });

            yield return new Feat(ModManager.RegisterFeatName("Disarm"), "Your eidolon's natural weapon is especially adept at prying away their foe's weapons.", "{b}" + Trait.Disarm.HumanizeTitleCase2() + "{/b} " + Trait.Disarm.GetTraitProperties().RulesText, new List<Trait>() { tAdvancedWeaponryAtkTrait }, null);
            yield return new Feat(ModManager.RegisterFeatName("Grapple"), "Your eidolon's natural weapon is especially adept at ensaring their foes.", "{b}" + tGrapple.HumanizeTitleCase2() + "{/b} " + tGrapple.GetTraitProperties().RulesText, new List<Trait>() { tAdvancedWeaponryAtkTrait }, null);
            yield return new Feat(ModManager.RegisterFeatName("Shove"), "Your eidolon's natural weapon is especially adept at shoving away their foes.", "{b}" + Trait.Shove.HumanizeTitleCase2() + "{/b} " + Trait.Shove.GetTraitProperties().RulesText, new List<Trait>() { tAdvancedWeaponryAtkTrait }, null);
            yield return new Feat(ModManager.RegisterFeatName("Trip"), "Your eidolon's natural weapon is especially adept at topplin their enemies.", "{b}" + Trait.Trip.HumanizeTitleCase2() + "{/b} " + Trait.Trip.GetTraitProperties().RulesText, new List<Trait>() { tAdvancedWeaponryAtkTrait }, null);
            yield return new Feat(ModManager.RegisterFeatName("Versatile Piercing"), "Your eidolon's natural weapon has a deadly piercing appendage.", "{b}" + Trait.VersatileP.HumanizeTitleCase2() + "{/b} " + Trait.VersatileP.GetTraitProperties().RulesText, new List<Trait>() { tAdvancedWeaponryAtkTrait }, null);
            yield return new Feat(ModManager.RegisterFeatName("Versatile Slashing"), "Your eidolon's natural weapon has a sharp slashing edge.", "{b}" + Trait.VersatileS.HumanizeTitleCase2() + "{/b} " + Trait.VersatileS.GetTraitProperties().RulesText, new List<Trait>() { tAdvancedWeaponryAtkTrait }, null);
            yield return new Feat(ModManager.RegisterFeatName("Versatile Bludgeoning"), "Your eidolon's natural weapon has a heavy, crushing weight.", "{b}" + Trait.VersatileB.HumanizeTitleCase2() + "{/b} " + Trait.VersatileB.GetTraitProperties().RulesText, new List<Trait>() { tAdvancedWeaponryAtkTrait }, null);

            // Init Natural Attack Options
            yield return new Feat(ftPSword, "Your eidolon wields a sword, or possess a natural blade-like appendage.", "Your eidolon's primary attack deals slashing damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Longsword);
            yield return new Feat(ftPPolearm, "Your eidolon wields a spear or lance, or possess a natural spear-like appendage.", "Your eidolon's primary attack deals piercing damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Spear);
            yield return new Feat(ftPMace, "Your eidolon wields a mace, or possess a natural mace-like appendage.", "Your eidolon's primary attack deals bludgeoning damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Warhammer);
            yield return new Feat(ftPWing, "Your eidolon knocks its enemies aside with a pair of powerful wings.", "Your eidolon's primary attack deals bludgeoning damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Wing);
            yield return new Feat(ftPKick, "Your eidolon possesses a powerful kick.", "Your eidolon's primary attack deals bludgeoning damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.BootsOfElvenkind);
            yield return new Feat(ftPClaw, "Your eidolon possesses razor sharp claws.", "Your eidolon's primary attack deals slashing damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.DragonClaws);
            yield return new Feat(ftPJaws, "Your eidolon possesses powerful bite attack.", "Your eidolon's primary attack deals piercing damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Jaws);
            yield return new Feat(ftPFist, "Your eidolon tears or pummels its enemies apart with its bare hands.", "Your eidolon's primary attack deals bludgeoning damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Fist);
            yield return new Feat(ftPTendril, "Your eidolon possesses crushing tendrils.", "Your eidolon's primary attack deals bludgeoning damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Tentacle);
            yield return new Feat(ftPHorn, "Your eidolon possesses vicious horns to gore its enemies.", "Your eidolon's primary attack deals piercing damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Horn);
            yield return new Feat(ftPTail, "Your eidolon possesses a deadly stinging tail.", "Your eidolon's primary attack deals piercing damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Tail);
            yield return new Feat(ftPMermaidTail, "Your eidolon possesses a powerful tail.", "Your eidolon's primary attack deals bludgeoning damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(Enums.illMermaidTail);
            yield return new Feat(ftPSpiderLeg, "Your eidolon possesses lethal spidery appendages.", "Your eidolon's primary attack deals piercing damage.", new List<Trait> { tPrimaryAttackType, Trait.Strike }, null).WithIllustration(Enums.illStabbingAppendage);

            yield return new Feat(ftSWing, "Your eidolon knocks its enemies aside with a pair of powerful wings.", "Your eidolon's secondary attack deals 1d6 bludgeoning damage with the agile and finesse traits." +
                "\n\n{b}" + Trait.Agile.GetTraitProperties().HumanizedName + "{/b} " + Trait.Agile.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Finesse.GetTraitProperties().HumanizedName + "{/b} " + Trait.Finesse.GetTraitProperties().RulesText,
                new List<Trait> { tSecondaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Wing);
            yield return new Feat(ftSKick, "Your eidolon possesses a powerful kick.", "Your eidolon's secondary attack deals 1d6 bludgeoning damage with the agile and finesse traits." +
                "\n\n{b}" + Trait.Agile.GetTraitProperties().HumanizedName + "{/b} " + Trait.Agile.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Finesse.GetTraitProperties().HumanizedName + "{/b} " + Trait.Finesse.GetTraitProperties().RulesText,
                new List<Trait> { tSecondaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.BootsOfElvenkind);
            yield return new Feat(ftSClaw, "Your eidolon possesses razor sharp claws.", "Your eidolon's secondary attack deals 1d6 slashing damage with the agile and finesse traits." +
                "\n\n{b}" + Trait.Agile.GetTraitProperties().HumanizedName + "{/b} " + Trait.Agile.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Finesse.GetTraitProperties().HumanizedName + "{/b} " + Trait.Finesse.GetTraitProperties().RulesText,
                new List<Trait> { tSecondaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.DragonClaws);
            yield return new Feat(ftSJaws, "Your eidolon possesses powerful bite attack.", "Your eidolon's secondary attack deals 1d6 piercing damage with the agile and finesse traits." +
                "\n\n{b}" + Trait.Agile.GetTraitProperties().HumanizedName + "{/b} " + Trait.Agile.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Finesse.GetTraitProperties().HumanizedName + "{/b} " + Trait.Finesse.GetTraitProperties().RulesText,
                new List<Trait> { tSecondaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Jaws);
            yield return new Feat(ftSFist, "Your eidolon tears or pummels its enemies apart with its bare hands.", "Your eidolon's secondary attack deals 1d6 bludgeoning damage with the agile and finesse traits." +
                "\n\n{b}" + Trait.Agile.GetTraitProperties().HumanizedName + "{/b} " + Trait.Agile.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Finesse.GetTraitProperties().HumanizedName + "{/b} " + Trait.Finesse.GetTraitProperties().RulesText,
                new List<Trait> { tSecondaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Fist);
            yield return new Feat(ftSTendril, "Your eidolon possesses crushing tendrils.", "Your eidolon's secondary attack deals 1d6 bludgeoning damage with the agile and finesse traits." +
                "\n\n{b}" + Trait.Agile.GetTraitProperties().HumanizedName + "{/b} " + Trait.Agile.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Finesse.GetTraitProperties().HumanizedName + "{/b} " + Trait.Finesse.GetTraitProperties().RulesText,
                new List<Trait> { tSecondaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Tentacle);
            yield return new Feat(ftSHorn, "Your eidolon possesses vicious horns to gore its enemies.", "Your eidolon's secondary attack deals 1d6 piercing damage with the agile and finesse traits." +
                "\n\n{b}" + Trait.Agile.GetTraitProperties().HumanizedName + "{/b} " + Trait.Agile.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Finesse.GetTraitProperties().HumanizedName + "{/b} " + Trait.Finesse.GetTraitProperties().RulesText,
                new List<Trait> { tSecondaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Horn);
            yield return new Feat(ftSTail, "Your eidolon possesses deadly stinging tail.", "Your eidolon's secondary attack deals 1d6 piercing damage with the agile and finesse traits." +
                "\n\n{b}" + Trait.Agile.GetTraitProperties().HumanizedName + "{/b} " + Trait.Agile.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Finesse.GetTraitProperties().HumanizedName + "{/b} " + Trait.Finesse.GetTraitProperties().RulesText,
                new List<Trait> { tSecondaryAttackType, Trait.Strike }, null).WithIllustration(IllustrationName.Tail);
            yield return new Feat(ftSMermaidTail, "Your eidolon possesses a powerful tail.", "Your eidolon's secondary attack deals 1d6 bludgeoning damage with the agile and finesse traits." +
                "\n\n{b}" + Trait.Agile.GetTraitProperties().HumanizedName + "{/b} " + Trait.Agile.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Finesse.GetTraitProperties().HumanizedName + "{/b} " + Trait.Finesse.GetTraitProperties().RulesText,
                new List<Trait> { tSecondaryAttackType, Trait.Strike }, null).WithIllustration(Enums.illMermaidTail);
            yield return new Feat(ftSSpiderLeg, "Your eidolon possesses lethal spidery appendages.", "Your eidolon's secondary attack deals 1d6 piercing damage with the agile and finesse traits." +
                "\n\n{b}" + Trait.Agile.GetTraitProperties().HumanizedName + "{/b} " + Trait.Agile.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Finesse.GetTraitProperties().HumanizedName + "{/b} " + Trait.Finesse.GetTraitProperties().RulesText,
                new List<Trait> { tSecondaryAttackType, Trait.Strike }, null).WithIllustration(Enums.illStabbingAppendage);

            // Init Primary Weapon Properties
            yield return new Feat(ftPSPowerful, "Your eidolon possesses great strength, allowing it to easily bully and subdue its enemies.", "Your eidolon's primary deals 1d8 damage and has the disarm, nonlethal, shove and trip traits.\n\nAthletics checks made using a weapon with a maneouvre trait benefit your eidolon's item bonus." +
                "\n\n{b}" + Trait.Disarm.GetTraitProperties().HumanizedName + "{/b} " + Trait.Disarm.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Shove.GetTraitProperties().HumanizedName + "{/b} " + Trait.Shove.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Trip.GetTraitProperties().HumanizedName + "{/b} " + Trait.Trip.GetTraitProperties().RulesText,
                new List<Trait> { tPrimaryAttackStats, Trait.Strike, Trait.Disarm, Trait.Nonlethal, Trait.Shove, Trait.Trip }, null);
            yield return new Feat(ftPSFatal, "Your eidolon waits patiently for the perfect opportunity before closing in on its foes.", "Your eidolon's primary attack deals 1d6 damage and has the fatal d10 traits." +
                "\n\n{b}" + Trait.FatalD10.GetTraitProperties().HumanizedName + "{/b} " + Trait.FatalD10.GetTraitProperties().RulesText,
                new List<Trait> { tPrimaryAttackStats, Trait.Strike, Trait.FatalD10 }, null);
            yield return new Feat(ftPSUnstoppable, "Your eidolon's attacks pick up speed and momentum as it fights.", "Your eidolon's primary attack deals 1d6 damage and has the forceful and sweep traits." +
                "\n\n{b}" + Trait.Forceful.GetTraitProperties().HumanizedName + "{/b} " + Trait.Forceful.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Sweep.GetTraitProperties().HumanizedName + "{/b} " + Trait.Sweep.GetTraitProperties().RulesText,
                new List<Trait> { tPrimaryAttackStats, Trait.Strike, Trait.Forceful, Trait.Sweep }, null);
            yield return new Feat(ftPSGraceful, "Your eidolon possesses dexterous and opportunisitic natural weapons.", "Your eidolon's primary attack deals 1d6 damage and has the deadly d8 and finesse traits." +
                "\n\n{b}" + Trait.DeadlyD8.GetTraitProperties().HumanizedName + "{/b} "+ Trait.DeadlyD8.GetTraitProperties().RulesText +
                "\n{b}" + Trait.Finesse.GetTraitProperties().HumanizedName + "{/b} " + Trait.Finesse.GetTraitProperties().RulesText,
                new List<Trait> { tPrimaryAttackStats, Trait.Strike, Trait.DeadlyD8, Trait.Finesse }, null);

            // Init Eidolon Alignments
            yield return new Feat(ftALawfulGood, "Your eidolon's alignment is lawful good.", "", new List<Trait> { tAlignment, Trait.Lawful, Trait.Good }, null);
            yield return new Feat(ftAGood, "Your eidolon's alignment is good.", " ", new List<Trait> { tAlignment, Trait.Good }, null);
            yield return new Feat(ftAChaoticGood, "Your eidolon's alignment is chaotic good.", " ", new List<Trait> { tAlignment, Trait.Chaotic, Trait.Good }, null);
            yield return new Feat(ftALawful, "Your eidolon's alignment is lawful.", " ", new List<Trait> { tAlignment, Trait.Lawful }, null);
            yield return new Feat(ftANeutral, "Your eidolon's alignment is true neutral.", "  ", new List<Trait> { tAlignment, Trait.Neutral }, null);
            yield return new Feat(ftAChaotic, "Your eidolon's alignment is chaotic.", " ", new List<Trait> { tAlignment, Trait.Chaotic }, null);
            yield return new Feat(ftALawfulEvil, "Your eidolon's alignment is lawful evil.", "  ", new List<Trait> { tAlignment, Trait.Lawful, Trait.Evil }, null);
            yield return new Feat(ftAEvil, "Your eidolon's alignment is evil.", "", new List<Trait> { tAlignment, Trait.Evil }, null);
            yield return new Feat(ftAChaoticEvil, "Your eidolon's alignment is chaotic evil.", "  ", new List<Trait> { tAlignment, Trait.Chaotic, Trait.Evil }, null);

        }

        public static Creature? GetSummoner(Creature eidolon) {
            return eidolon.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond))?.Source;
        }

        public static Creature? GetEidolon(Creature summoner) {
            return summoner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond))?.Source;
        }

        public static DamageKind TraitToDamage(Trait trait) {
            switch (trait) {
                case Trait.Acid:
                    return DamageKind.Acid;
                case Trait.Cold:
                    return DamageKind.Cold;
                case Trait.Electricity:
                    return DamageKind.Electricity;
                case Trait.Fire:
                    return DamageKind.Fire;
                case Trait.Sonic:
                    return DamageKind.Sonic;
                case Trait.Positive:
                    return DamageKind.Positive;
                case Trait.Negative:
                    return DamageKind.Negative;
                case Trait.Good:
                    return DamageKind.Good;
                case Trait.Evil:
                    return DamageKind.Evil;
                case Trait.Chaotic:
                    return DamageKind.Chaotic;
                case Trait.Lawful:
                    return DamageKind.Lawful;
                case Trait.VersatileP:
                    return DamageKind.Piercing;
                default:
                    return DamageKind.Untyped;
            }
        }

        public static Trait DamageToTrait(DamageKind type) {
            switch (type) {
                case DamageKind.Acid:
                    return Trait.Acid;
                case DamageKind.Cold:
                    return Trait.Cold;
                case DamageKind.Electricity:
                    return Trait.Electricity;
                case DamageKind.Fire:
                    return Trait.Fire;
                case DamageKind.Sonic:
                    return Trait.Sonic;
                case DamageKind.Positive:
                    return Trait.Positive;
                case DamageKind.Negative:
                    return Trait.Negative;
                case DamageKind.Good:
                    return Trait.Good;
                case DamageKind.Evil:
                    return Trait.Evil;
                case DamageKind.Chaotic:
                    return Trait.Chaotic;
                case DamageKind.Lawful:
                    return Trait.Lawful;
                default:
                    return Trait.None;
            }
        }

        internal static string Int2Mod(int num) {
            if (num >= 0) {
                return $"+{num}";
            }
            return $"-{num - num * 2}";
        }

        public class EvolutionFeat : TrueFeat {
            public Action<Creature>? EffectOnEidolon { get; private set; }
            public EvolutionFeat(FeatName featName, int level, string flavourText, string rulesText, Trait[] traits, Action<Creature> effect, List<Feat>? subfeats) : base(featName, level, flavourText, rulesText, new Trait[] { tEvolution }.Concat(traits).ToArray(), subfeats) {
                EffectOnEidolon = effect;
            }

        }

        private static string PrintEidolonStatBlock(FeatName bond, string? abilityText, string? actionText, int[] abilityScores, int ac, int dexCap) {
            string general =
                "{b}Perception{/b} " + Int2Mod((abilityScores[4] + 3)) +
                "\n{b}Skills{/b} Shares all your skill proficiancies\n" +
                "\nStr " + Int2Mod(abilityScores[0]) + " Dex " + Int2Mod(abilityScores[1]) + " Con " + Int2Mod(abilityScores[2]) + " Int " + Int2Mod(abilityScores[3]) + " Wis " + Int2Mod(abilityScores[4]) + " Cha " + Int2Mod(abilityScores[5]) + "\n" +
                "\n{b}{DarkRed}DEFENSE{/b}{/}\n" +
                "{b}AC{/b} " + (10 + ac + Math.Min(abilityScores[1], dexCap)) + "; {b}Fort{/b} " + Int2Mod((5 + abilityScores[2])) + ", {b}Ref{/b} " + Int2Mod((3 + abilityScores[1])) + ", {b}Will{/b} " + Int2Mod((4 + abilityScores[4])) +
                "\n{b}HP{/b} Share's your HP pool";

            if (bond == scDevilEidolonBarrister || bond == scDevilEidolonLegionnaire) {
                general += "\n{b}Resistances{/b} fire 1; {b}Weaknesses{/b} good 1";
            }

            general +=
                "\n\n{b}{DarkRed}OFFENSE{/b}{/}\n" +
                "{b}Speed{/b} 25 feet\n";

            string actions =
                "{b}Strke (Primary){/b} {icon:Action} " + Int2Mod((abilityScores[0] + 3)) + " [variable] 1dx" + (abilityScores[0] >= 0 ? " +" : " ") + abilityScores[0] + " variable damage\n" +
                "{b}Strke (Secondary){/b} {icon:Action} " + Int2Mod((Math.Max(abilityScores[0], abilityScores[1]) + 3)) + " [agile] " + Int2Mod(abilityScores[0]) + " variable damage\n";

            if (actionText != null)
                actions += actionText;

            actions += "{b}Act Together{/b} {icon:FreeAction} Your eidolon's next action grants you an immediate bonus tandem turn, where you can make a single action.\n";

            string abilities =
                "\n{b}{DarkRed}ABILITIES{/b}{/}\n" +
                "{b}Eidolon Bond.{/b} You and your eidolon share your actions and multiple attack penalty. Each round, you can use any of your actions (including reactions and free actions) for yourself or your eidolon. Your eidolon gains all of your skill proficiancies and uses your spell attack and save DC for its special abilities.";

            if (abilityText != null) 
                abilities += abilityText;

            return general + actions + abilities;
        }


        internal static Feat CreateEidolonFeat(FeatName featName, string flavorText, string? abilityText, string? actionText, int[] abilityScores, int ac, int dexCap) {
            return new Feat(featName, flavorText, "Your eidolon has the following characteristics at level 1:\n\n" + PrintEidolonStatBlock(featName, abilityText, actionText, abilityScores, ac, dexCap), new List<Trait>() { tEidolonArray }, (List<Feat>)null)
            //.WithOnSheet(sheet => {
            //    if (abilityScores[0] == 4) {
            //        sheet.AddAtLevel(5, _ => _.AddSelectionOption(new MultipleFeatSelectionOption("EidolonStrASI-5", "Eidolon Ability Boosts", 5, ft => ft.HasTrait(tEidolonASI) && !(abilityScores[0] == 4 && ft.Tag as Ability? == Ability.Strength) && !(abilityScores[1] == 4 && ft.Tag as Ability? == Ability.Dexterity), 4)));
            //    } else {
            //        sheet.AddAtLevel(5, _ => _.AddSelectionOption(new MultipleFeatSelectionOption("EidolonASI-5", "Eidolon Ability Boosts", 5, ft => ft.HasTrait(tEidolonASI) && !(abilityScores[0] == 4 && ft.Tag as Ability? == Ability.Strength) && !(abilityScores[1] == 4 && ft.Tag as Ability? == Ability.Dexterity), 4)));
            //    }
            //})
            .WithOnCreature((Action<CalculatedCharacterSheetValues, Creature>)((sheet, summoner) => summoner
            .AddQEffect(new ActionShareEffect() {
                Id = qfSharedActions,
            })
            // TODO: Bookmark: Summoner act together
            .AddQEffect(new QEffect() {
                ProvideMainAction = (effect) => {
                    if (summoner.PersistentCharacterSheet.Calculated.AllFeats.Where(ft => ft.HasTrait(tTandem)).ToList().Count > 0) {
                        SubmenuPossibility tandemActions = new SubmenuPossibility(illActTogether, "Tandem Actions");
                        tandemActions.Subsections.Add(new PossibilitySection("Tandem Actions"));
                        tandemActions.Subsections[0].PossibilitySectionId = psTandemActions;
                        return tandemActions;
                    }

                    return GenerateActTogetherAction(effect.Owner, GetEidolon(effect.Owner), summoner);

                },
                ProvideActionIntoPossibilitySection = (effect, section) => {
                    if (summoner.PersistentCharacterSheet.Calculated.AllFeats.Where(ft => ft.HasTrait(tTandem)).ToList().Count == 0) {
                        return null;
                    } else if (section.PossibilitySectionId == psTandemActions) {
                        return GenerateActTogetherAction(effect.Owner, GetEidolon(effect.Owner), summoner);
                    }
                    return null;
                },
            })
            .AddQEffect(new QEffect("Eidolon", "This character can summon and command an Eidolon.") {
                StartOfCombat = (Func<QEffect, Task>)(async qfSummonerTechnical => {
                    Creature eidolon = CreateEidolon(featName, abilityScores, ac, dexCap, summoner);
                    eidolon.MainName = qfSummonerTechnical.Owner.Name + "'s " + eidolon.MainName;

                    InvestedWeaponLogic.MagicItemLogic(summoner, eidolon);

                    summoner.Battle.SpawnCreature(eidolon, summoner.OwningFaction, summoner.Occupies);

                    // Handle evolution feat effects
                    for (int i = 0; i < eidolon.QEffects.Count; i++) {
                        if (eidolon.QEffects[i].StartOfCombat != null)
                            await eidolon.QEffects[i].StartOfCombat(eidolon.QEffects[i]);
                    }
                }),
                StartOfYourPrimaryTurn = async (qfStartOfTurn, summoner) => {
                    Creature eidolon = GetEidolon(summoner);

                    if (eidolon.Destroyed || eidolon.HP <= 0) {
                        return;
                    }
                    eidolon.TurnInformation.ThisTurnIsPrimary = true;
                    await (Task)eidolon.Battle.GameLoop.GetType().GetMethod("Step1_Upkeep", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(eidolon.Battle.GameLoop, new object[] { eidolon });
                    eidolon.Battle.GameLoop.GetType().GetMethod("Step2_Refresh", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(eidolon.Battle.GameLoop, new object[] { eidolon });
                    await (Task)eidolon.Battle.GameLoop.GetType().GetMethod("Step3_StartOfTurn", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(eidolon.Battle.GameLoop, new object[] { eidolon });

                    // Share eidolon quickened with summoner
                    if (eidolon.Actions.QuickenedForActions != null) {
                        foreach (var rule in (List<Func<CombatAction, bool>>)eidolon.Actions.QuickenedForActions.GetType().GetField("delegates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).GetValue(eidolon.Actions.QuickenedForActions)) {
                            if (summoner.Actions.QuickenedForActions == null) {
                                summoner.Actions.QuickenedForActions = new DisjunctionDelegate<CombatAction>(rule);
                            } else {
                                summoner.Actions.QuickenedForActions.Add(rule);
                            }
                        }
                        summoner.Actions.UsedQuickenedAction = eidolon.Actions.UsedQuickenedAction;
                        summoner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(summoner.Actions, new object[] { 3, eidolon.Actions.FourthActionStyle });
                    }

                    bool quickened = summoner.Actions.QuickenedForActions != null && !summoner.Actions.UsedQuickenedAction;

                    // delegates

                    await eidolon.Battle.GameLoop.StateCheck();

                    // Handle slowed
                    QEffect? eSlowed = eidolon.QEffects.FirstOrDefault(qf => qf.Id == QEffectId.Slowed);
                    QEffect? sSlowed = summoner.QEffects.FirstOrDefault(qf => qf.Id == QEffectId.Slowed);

                    if (eSlowed != null && (sSlowed == null || sSlowed.Value < eSlowed.Value)) {
                        summoner.Actions.ActionsLeft -= eSlowed.Value;
                        for (int i = 0; i < eSlowed.Value - (quickened ? 1 : 0); i++) {
                            summoner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(summoner.Actions, new object[] { i, ActionDisplayStyle.Slowed });
                        }
                        if (quickened) {
                            summoner.Actions.UsedQuickenedAction = true;
                            summoner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(summoner.Actions, new object[] { 3, ActionDisplayStyle.Slowed });
                        }
                    } else if (quickened && sSlowed != null && (eSlowed == null || eSlowed.Value < sSlowed.Value)) {
                        summoner.Actions.ActionsLeft += 1;
                        summoner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(summoner.Actions, new object[] { sSlowed.Value-1, ActionDisplayStyle.Available });
                        summoner.Actions.UsedQuickenedAction = true;
                        summoner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(summoner.Actions, new object[] { 3, ActionDisplayStyle.Slowed });
                    }
                },
                StateCheckWithVisibleChanges = (async qf => {
                    Creature eidolon = GetEidolon(qf.Owner);
                    if (eidolon == null) {
                        return;
                    }

                    // Handle drained mirror addition
                    QEffect drained = qf.Owner.FindQEffect(QEffectId.Drained);
                    if (drained != null) {
                        eidolon.AddQEffect(new QEffect() {
                            Id = qfDrainedMirror,
                            Key = "DrainedMirror",
                            StateCheck = drained.StateCheck,
                            Value = drained.Value,
                        });
                    } else if (qf.Owner.HasEffect(qfDrainedMirror) && eidolon.HasEffect(QEffectId.Drained) == false) {
                        qf.Owner.RemoveAllQEffects(effect => effect.Id == qfDrainedMirror);
                    }

                    QEffect mummyrot = qf.Owner.FindQEffect(QEffectId.MummyRot);
                    if (mummyrot != null) {
                        eidolon.AddQEffect(new QEffect() {
                            Id = qfMummyRotMirror,
                            Key = "MummyRotMirror",
                            StateCheck = mummyrot.StateCheck,
                            Value = mummyrot.Value,
                        });
                    } else if (qf.Owner.HasEffect(qfMummyRotMirror) && eidolon.HasEffect(QEffectId.MummyRot) == false) {
                        qf.Owner.RemoveAllQEffects(effect => effect.Id == qfMummyRotMirror);
                    }

                    // PAST THIS POINT, INACTIVE EIDOLON NOT AFFECTED
                    if (eidolon.Destroyed == true) {
                        return;
                    }

                    // Reaction
                    if (qf.Owner.Actions.IsReactionUsedUp == true) {
                        eidolon.Actions.UseUpReaction();
                    }

                    // Handle AoO
                    HPShareEffect shareHP = (HPShareEffect)qf.Owner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond));
                    if (shareHP.Logs.Any(log => !log.Processed && log.Type == SummonerClassEnums.InterceptKind.TARGET)) {
                        await HandleHealthShare(eidolon, qf.Owner, SummonerClassEnums.InterceptKind.TARGET);
                    }

                    HPShareEffect eidolonShareHP = (HPShareEffect)eidolon.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond));
                    if (eidolonShareHP.Logs.Any(log => !log.Processed && log.Type == SummonerClassEnums.InterceptKind.TARGET)) {
                        await HandleHealthShare(qf.Owner, eidolon, SummonerClassEnums.InterceptKind.TARGET);
                    }

                    // Handle tempHP
                    if (qf.Owner.TemporaryHP < eidolon.TemporaryHP) {
                        qf.Owner.GainTemporaryHP(eidolon.TemporaryHP);
                    } else if (qf.Owner.TemporaryHP > eidolon.TemporaryHP) {
                        eidolon.GainTemporaryHP(qf.Owner.TemporaryHP);
                    }
                }),
                EndOfYourTurnBeneficialEffect = (Func<QEffect, Creature, Task>)(async (qfEndOfTurn, summoner) => {
                    Creature eidolon = GetEidolon(summoner);

                    // Handle stunned
                    QEffect? eStunned = eidolon.QEffects.FirstOrDefault(qf => qf.Id == QEffectId.Stunned);
                    QEffect? sStunned = summoner.QEffects.FirstOrDefault(qf => qf.Id == QEffectId.Stunned);

                    if (eStunned != null && (sStunned == null || sStunned.Value < eStunned.Value)) {
                        summoner.AddQEffect(QEffect.Stunned(eStunned.Value));
                    }

                    eidolon.Actions.ForgetAllTurnCounters();
                    summoner.Battle.ActiveCreature = eidolon;
                    List<QEffect> sustainEffects = new List<QEffect>();
                    foreach (Creature creature in qfEndOfTurn.Owner.Battle.AllCreatures) {
                        sustainEffects = sustainEffects.Concat(creature.QEffects.Where(qf => qf.CannotExpireThisTurn == true)).ToList();
                    }
                    await eidolon.Battle.GameLoop.EndOfTurn(eidolon);
                    foreach (QEffect effect in sustainEffects) {
                        effect.CannotExpireThisTurn = true;
                    }
                    summoner.Battle.ActiveCreature = summoner;
                }),
                ProvideMainAction = (Func<QEffect, Possibility>)(qfSummoner => {
                    Creature? eidolon = GetEidolon(qfSummoner.Owner);
                    if (eidolon == null || eidolon.OwningFaction != qfSummoner.Owner.OwningFaction || !eidolon.Actions.CanTakeActions() || qfSummoner.Owner.QEffects.FirstOrDefault(qf => qf.Id == qfActTogether) != null)
                        return (Possibility)null;

                    Possibility output = (Possibility)(ActionPossibility)new CombatAction(qfSummoner.Owner, eidolon.Illustration, "Command your Eidolon", new Trait[] { Trait.Basic, tSummoner }, "Swap to Eidolon.", (Target)Target.Self()) {
                        ShortDescription = "Take control of your Eidolon, using your shared action pool."
                    }
                    .WithEffectOnSelf((Func<Creature, Task>)(async self => {
                        if (GetEidolon(summoner)?.FindQEffect(QEffectId.Confused) != null && (await summoner.Battle.SendRequest(new ConfirmationRequest(summoner, "Your eidolon is confused and will use all of your shared actions without input. Are you sure you want to swap to them?", GetEidolon(summoner)?.Illustration, "Yes", "No, skip their action"))).ChosenOption is CancelOption) {
                            return;
                        }

                        await PartnerActs(summoner, eidolon);
                    }))
                    .WithActionCost(0);

                    //output.WithPossibilityGroup("Summoner");
                    return output;
                }),
                YouAreTargeted = (Func<QEffect, CombatAction, Task>)(async (qfHealOrHarm, action) => {
                    if (action.Name == "Command your Eidolon") {
                        return;
                    }

                    if (GetEidolon(qfHealOrHarm.Owner) == null || GetEidolon(qfHealOrHarm.Owner).Destroyed) {
                        return;
                    }

                    HPShareEffect shareHP = (HPShareEffect)qfHealOrHarm.Owner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond && qf.Source == GetEidolon(qfHealOrHarm.Owner)));
                    HPShareEffect eidolonShareHP = (HPShareEffect)GetEidolon(qfHealOrHarm.Owner).QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond && qf.Source == qfHealOrHarm.Owner));

                    if (action == shareHP.CA || action == eidolonShareHP.CA) {
                        return;
                    }

                    shareHP.LogAction(qfHealOrHarm.Owner, action, action.Owner, SummonerClassEnums.InterceptKind.TARGET);
                }),
                AfterYouAreTargeted = (Func<QEffect, CombatAction, Task>)(async (qfShareHP, action) => {
                    if (action.Name == "Command your Eidolon") {
                        return;
                    }

                    if (GetEidolon(qfShareHP.Owner) == null || GetEidolon(qfShareHP.Owner).Destroyed) {
                        return;
                    }

                    Creature summoner = qfShareHP.Owner;
                    Creature eidolon = GetEidolon(summoner);

                    await HandleHealthShare(summoner, eidolon, SummonerClassEnums.InterceptKind.TARGET, action.Name);
                }),
                EndOfAnyTurn = self => {
                    HPShareEffect shareHP = (HPShareEffect)self.Owner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond));
                    if (shareHP != null) {
                        shareHP.Reset();
                    }
                    // Handle healing
                    if (GetEidolon(self.Owner) != null && GetEidolon(self.Owner).Destroyed == false)
                        HealthShareSafetyCheck(self.Owner, GetEidolon(self.Owner));
                },
                YouAreDealtDamage = async (qfPreHazardDamage, attacker, damageStuff, defender) => {
                    if (GetEidolon(qfPreHazardDamage.Owner) == null || GetEidolon(qfPreHazardDamage.Owner).Destroyed) {
                        return null;
                    }

                    // Check if effect is coming from self
                    if (damageStuff.Power.Name == "SummonerClass: Share HP") {
                        return null;
                    }

                    HPShareEffect shareHP = (HPShareEffect)qfPreHazardDamage.Owner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond));

                    // Check if caught by target check
                    if (shareHP.CheckForTargetLog(damageStuff.Power, attacker)) {
                        return null;
                    }

                    shareHP.LogAction(qfPreHazardDamage.Owner, damageStuff.Power, attacker, SummonerClassEnums.InterceptKind.DAMAGE);
                    return null;
                },
                AfterYouTakeDamageOfKind = (async (qfPostHazardDamage, action, kind) => {
                    if (GetEidolon(qfPostHazardDamage.Owner) == null || GetEidolon(qfPostHazardDamage.Owner).Destroyed) {
                        return;
                    }

                    // Check if effect is coming from self
                    if (action.Name == "SummonerClass: Share HP") {
                        return;
                    }

                    Creature summoner = qfPostHazardDamage.Owner;
                    Creature eidolon = GetEidolon(summoner);

                    await HandleHealthShare(summoner, eidolon, SummonerClassEnums.InterceptKind.DAMAGE, action.Name);
                }),
                AfterYouAreHealed = async (self, action, amount) => {
                    if (GetEidolon(self.Owner) == null || GetEidolon(self.Owner).Destroyed) {
                        return;
                    }

                    // Check if effect is coming from self
                    if (action != null && action.Name == "SummonerClass: Share HP") {
                        return;
                    }

                    Creature summoner = self.Owner;
                    Creature eidolon = GetEidolon(summoner);

                    HPShareEffect shareHP = (HPShareEffect)self.Owner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond));

                    // Check if caught by target check
                    if (shareHP.CheckForTargetLog(action, action.Owner)) {
                        return;
                    }

                    eidolon.Heal(DiceFormula.FromText($"{amount}", $"Eidolon Health Share ({action.Name})"), action);

                    //await HandleHealthShare(summoner, eidolon, SummonerClassEnums.InterceptKind.DAMAGE, action.Name);
                },
                AfterYouTakeAction = (Func<QEffect, CombatAction, Task>)(async (qf, action) => {
                    Creature summoner = qf.Owner;
                    Creature eidolon = GetEidolon(summoner);
                    //eidolon.Actions.UseUpActions(action.ActionCost, )

                    if (eidolon == null) {
                        return;
                    }

                    // Focus points
                    if (action.HasTrait(Trait.Focus)) {
                        eidolon.Spellcasting.FocusPoints = summoner.Spellcasting.FocusPoints;
                    }

                    // MAP
                    if (action.Traits.Contains(Trait.Attack)) {
                        eidolon.Actions.AttackedThisManyTimesThisTurn = summoner.Actions.AttackedThisManyTimesThisTurn;
                    }
                })
            })
            .AddQEffect(InvestedWeaponLogic.SummonerInvestedWeaponQEffect())
            .AddQEffect(new QEffect() {
                ProvideActionIntoPossibilitySection = (self, section) => {
                    Creature? eidolon = GetEidolon(self.Owner);
                    if (eidolon == null || section.PossibilitySectionId != psSummonerExtra) {
                        return null;
                    }
                    Trait spellList = sheet.SpellRepertoires[tSummoner].SpellList;

                    if (!eidolon.Destroyed) {
                        Possibility output = (Possibility)(ActionPossibility)new CombatAction(self.Owner, illDismiss, "Dismiss Eidolon", new Trait[] {
                            tSummoner, Trait.Concentrate, Trait.Conjuration, Trait.Manipulate, Trait.Teleportation, spellList
                        },
                            "Dismiss your eidolon, protecting it and yourself from harm.", Target.RangedFriend(20).WithAdditionalConditionOnTargetCreature((CreatureTargetingRequirement)new EidolonCreatureTargetingRequirement(qfSummonerBond)))
                        .WithEffectOnChosenTargets((Func<Creature, ChosenTargets, Task>)(async (self, targets) => {
                            self.Battle.RemoveCreatureFromGame(eidolon);
                        }))
                        .WithActionCost(3);

                        output.WithPossibilityGroup("Summoner");
                        return output;
                    }
                    return null;
                },
                ProvideMainAction = (qfManifestEidolon => {
                    Creature? eidolon = GetEidolon(qfManifestEidolon.Owner);
                    QEffect actTogether = new QEffect("Recently Manifested", "Immediately take a single 1 cost action.") {
                        Illustration = IllustrationName.Haste,
                        Id = qfActTogether,
                    };
                    if (eidolon == null) {
                        return null;
                    }

                    Trait spellList = sheet.SpellRepertoires[tSummoner].SpellList;

                    if (eidolon.Destroyed) {
                        Possibility output = (Possibility)(ActionPossibility)new CombatAction(qfManifestEidolon.Owner, eidolon.Illustration, "Manifest Eidolon", new Trait[] {
                                tSummoner, Trait.Concentrate, Trait.Conjuration, Trait.Manipulate, Trait.Teleportation, spellList
                        },
                            "Your eidolon appears in an open space adjacent to you, and can then take a single action.", (Target)Target.RangedEmptyTileForSummoning(1)) {
                            ShortDescription = "Your eidolon reappears in an open space adjacent to you, and can then take a single action."
                        }
                        .WithEffectOnChosenTargets((Func<Creature, ChosenTargets, Task>)(async (self, targets) => {
                            eidolon.Battle.InitiativeOrder.Remove(eidolon);
                            eidolon.Battle.Corpses.Remove(eidolon);
                            eidolon.Occupies = targets.ChosenTile;
                            eidolon.RemoveAllQEffects(qf => qf.Illustration != null);
                            eidolon.AddQEffect(actTogether);
                            eidolon.Battle.SpawnCreature(eidolon, self.OwningFaction, targets.ChosenTile);
                            eidolon.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(eidolon.Actions, new object[] { 0, ActionDisplayStyle.UsedUp });
                            eidolon.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(eidolon.Actions, new object[] { 1, ActionDisplayStyle.UsedUp });
                            eidolon.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(eidolon.Actions, new object[] { 2, ActionDisplayStyle.UsedUp });
                            eidolon.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(eidolon.Actions, new object[] { 3, ActionDisplayStyle.Invisible });
                            eidolon.Destroyed = false;
                            eidolon.Actions.ActionsLeft = 0;
                            eidolon.Actions.UsedQuickenedAction = true;
                            // Balance HP
                            HPShareEffect shareHP = (HPShareEffect)summoner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond));
                            if (eidolon.HP < summoner.HP) {
                                eidolon.Heal($"{summoner.HP - eidolon.HP}", shareHP.CA);
                            } else if (eidolon.HP > summoner.HP) {
                                await CommonSpellEffects.DealDirectSplashDamage(shareHP.CA, DiceFormula.FromText($"{eidolon.HP - summoner.HP}"), eidolon, DamageKind.Untyped);
                            }
                            await eidolon.Battle.GameLoop.StateCheck();
                            await PartnerActs(summoner, eidolon, true, null);
                            eidolon.RemoveAllQEffects(effect => effect == actTogether);
                        }))
                        .WithActionCost(3);

                        output.WithPossibilityGroup("Summoner");
                        return output;
                    }
                    return null;
                }),
            })
            ));
        }

        //++combatActionExecution.user.Actions.AttackedThisManyTimesThisTurn

        private static Creature CreateEidolon(FeatName featName, int[] abilityScores, int ac, int dexCap, Creature summoner) {
            Creature eidolon = CreateEidolonBase("Eidolon", summoner, abilityScores, ac, dexCap);

            // Link to summoner
            eidolon.AddQEffect(new HPShareEffect(eidolon) {
                Id = qfSummonerBond,
                Source = summoner
            });
            summoner.AddQEffect(new HPShareEffect(summoner) {
                Id = qfSummonerBond,
                Source = eidolon
            });

            // Add spellcasting
            SpellcastingSource spellSource = eidolon.AddSpellcastingSource(SpellcastingKind.Innate, tSummoner, Ability.Charisma, summoner.PersistentCharacterSheet.Calculated.SpellRepertoires[tSummoner].SpellList);
            eidolon.Spellcasting.FocusPointsMaximum = summoner.Spellcasting.FocusPointsMaximum;
            eidolon.Spellcasting.FocusPoints = summoner.Spellcasting.FocusPointsMaximum;

            // Add skill profs
            List<KeyValuePair<Trait, Proficiency>> skillProfs = summoner.Proficiencies.AllProficiencies.ToList()
                //.Where(t => t.Key != Trait.Spell && t.Key != tSummoner && t.Key != Trait.Simple && t.Key != Trait.Martial && t.Key != Trait.Unarmed && t.Key != Trait.UnarmoredDefense).ToList();
                .Where(t => Skills.TraitToSkill(t.Key) != null).ToList();
            foreach (KeyValuePair<Trait, Proficiency> skill in skillProfs) {
                eidolon.WithProficiency(skill.Key, skill.Value);
            }

            // Generate natural weapon attacks
            Feat pAttack = summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault((Func<Feat, bool>)(ft => ft.HasTrait(tPrimaryAttackType)));
            Feat sAttack = summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault((Func<Feat, bool>)(ft => ft.HasTrait(tSecondaryAttackType)));
            Feat pStatsFeat = summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault((Func<Feat, bool>)(ft => ft.HasTrait(tPrimaryAttackStats)));
            List<Trait> pStats = new List<Trait>() { Trait.Unarmed };
            for (int i = 2; i < pStatsFeat.Traits.Count; i++) {
                if (pStatsFeat.Traits[i] != Trait.Mod) {
                    pStats.Add(pStatsFeat.Traits[i]);
                }
            }
            List<Trait> sStats = new List<Trait>() { Trait.Unarmed, Trait.Finesse, Trait.Agile };

            DamageKind primaryDamageType;
            if (summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft.HasTrait(tEnergyHeartWeapon)) != null &&
                summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft.HasTrait(tEnergyHeartWeapon)).Name == "Primary Unarmed Attack") {
                primaryDamageType = TraitToDamage(summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft.HasTrait(tEnergyHeartDamage)).Traits[0]);
                pStats.Add(DamageToTrait(primaryDamageType));
            }  else if (new FeatName[] { ftPMace, ftPWing, ftPKick, ftPFist, ftPTendril, ftPMermaidTail }.Contains(pAttack.FeatName)) {
                primaryDamageType = DamageKind.Bludgeoning;
            } else if (new FeatName[] { ftPPolearm, ftPHorn, ftPTail, ftPSpiderLeg }.Contains(pAttack.FeatName)) {
                primaryDamageType = DamageKind.Piercing;
            } else {
                primaryDamageType = DamageKind.Slashing;
            }

            DamageKind secondaryDamageType;
            if (summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft.HasTrait(tEnergyHeartWeapon)) != null &&
                summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft.HasTrait(tEnergyHeartWeapon)).Name == "Secondary Unarmed Attack") {
                secondaryDamageType = TraitToDamage(summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft.HasTrait(tEnergyHeartDamage)).Traits[0]);
                sStats.Add(DamageToTrait(secondaryDamageType));
            } else if (new FeatName[] { ftSWing, ftSKick, ftSFist, ftSTendril, ftPMermaidTail }.Contains(sAttack.FeatName)) {
                secondaryDamageType = DamageKind.Bludgeoning;
            } else if (new FeatName[] { ftSHorn, ftSTail, ftPSpiderLeg }.Contains(sAttack.FeatName)) {
                secondaryDamageType = DamageKind.Piercing;
            } else {
                secondaryDamageType = DamageKind.Slashing;
            }

            string damage = "1d6";
            if (pStatsFeat.FeatName == ftPSPowerful) {
                damage = "1d8";
            }

            Illustration pIcon = pAttack.Illustration;
            Illustration sIcon = sAttack.Illustration;

            eidolon.WithUnarmedStrike(new Item(pIcon, pAttack.Name.ToLower(), pStats.ToArray()).WithWeaponProperties(new WeaponProperties(damage, primaryDamageType)));
            eidolon.WithAdditionalUnarmedStrike(new Item(sIcon, sAttack.Name.ToLower(), sStats.ToArray()).WithWeaponProperties(new WeaponProperties("1d6", secondaryDamageType)));

            var evoFeats = summoner.PersistentCharacterSheet.Calculated.AllFeats.Where(ft => ft.HasTrait(tEvolution)).ToArray();
            evoFeats = Array.ConvertAll(evoFeats, ft => (EvolutionFeat)ft);

            eidolon.AddQEffect(new QEffect() {
                ProvideMainAction = (effect) => {
                    if (summoner.PersistentCharacterSheet.Calculated.AllFeats.Where(ft => ft.HasTrait(tTandem)).ToList().Count > 0) {
                        SubmenuPossibility tandemActions = new SubmenuPossibility(illActTogether, "Tandem Actions");
                        tandemActions.Subsections.Add(new PossibilitySection("Tandem Actions"));
                        tandemActions.Subsections[0].PossibilitySectionId = psTandemActions;
                        return tandemActions;
                    }

                    return GenerateActTogetherAction(effect.Owner, summoner, summoner);

                },
                ProvideActionIntoPossibilitySection = (qfActTogether, section) => {
                    if (summoner.PersistentCharacterSheet.Calculated.AllFeats.Where(ft => ft.HasTrait(tTandem)).ToList().Count == 0) {
                        return null;
                    } else if (section.PossibilitySectionId == psTandemActions) {
                        return GenerateActTogetherAction(qfActTogether.Owner, summoner, summoner);
                    }
                    return null;
                },
            })
            .AddQEffect(new QEffect() {
                ProvideMainAction = (Func<QEffect, Possibility>)(qfEidolon => {
                    Creature? summoner = GetSummoner(qfEidolon.Owner);
                    if (summoner == null || summoner.OwningFaction != qfEidolon.Owner.OwningFaction || !summoner.Actions.CanTakeActions() || qfEidolon.Owner.QEffects.FirstOrDefault(qf => qf.Id == qfActTogether) != null)
                        return (Possibility)null;
                    Possibility output = (Possibility)(ActionPossibility)new CombatAction(qfEidolon.Owner, summoner.Illustration, "Return Control",
                        new Trait[] { Trait.Basic, tSummoner }, $"Switch back to controlling {summoner.Name}. All unspent actions will be retained.", (Target)Target.Self())
                    .WithActionCost(0)
                    .WithActionId(ActionId.EndTurn)
                    .WithEffectOnSelf((Action<Creature>)(self => {
                        // Remove act together toggle on eidolon
                        self.RemoveAllQEffects(qf => qf.Id == qfActTogetherToggle);
                        // Remove and log actions
                        ActionShareEffect actionShare = (ActionShareEffect)self.QEffects.FirstOrDefault(qf => qf.Id == qfSharedActions);
                        actionShare.LogTurnEnd(self.Actions);
                        self.Actions.UsedQuickenedAction = true;
                        self.Actions.ActionsLeft = 0;
                        self.Actions.WishesToEndTurn = true;
                        Sfxs.Play(SfxName.EndOfTurn, 0.2f);
                    }));

                    //output.WithPossibilityGroup("Summoner");
                    return output;
                })
            });

            // Add subclasses
            EidolonBond bond = summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft is EidolonBond) as EidolonBond;
            if (bond.ClassFeatures != null) {
                bond.ClassFeatures(eidolon, summoner);
            }

            // Add Evolution feats
            foreach (EvolutionFeat feat in evoFeats) {
                if (feat.EffectOnEidolon != null) {
                    feat.EffectOnEidolon.Invoke(eidolon);
                }
            }

            // Handle hard coded natural attack adjustments
            if (bond.Name == "Psychopomp Eidolon") {
                eidolon.UnarmedStrike.Traits.Add(Trait.GhostTouch);
                foreach (QEffect effect in eidolon.QEffects.Where(qf => qf.AdditionalUnarmedStrike != null)) {
                    effect.AdditionalUnarmedStrike.Traits.Add(Trait.GhostTouch);
                }
            }

            if (bond.Name == "Elemental Eidolon") {
                if (bond.eidolonTraits.Contains(Trait.Air)) {
                    eidolon.UnarmedStrike.Traits.Add(Trait.Air);
                    foreach (QEffect effect in eidolon.QEffects.Where(qf => qf.AdditionalUnarmedStrike != null)) {
                        effect.AdditionalUnarmedStrike.Traits.Add(Trait.Air);
                    }
                }

                if (bond.eidolonTraits.Contains(Trait.Earth)) {
                    eidolon.UnarmedStrike.Traits.Add(Trait.Earth);
                    foreach (QEffect effect in eidolon.QEffects.Where(qf => qf.AdditionalUnarmedStrike != null)) {
                        effect.AdditionalUnarmedStrike.Traits.Add(Trait.Earth);
                    }
                }

                if (bond.eidolonTraits.Contains(Trait.Fire) && !summoner.HasEffect(QEffectId.AquaticCombat)) {
                    eidolon.UnarmedStrike.Traits.Add(Trait.Fire);
                    foreach (QEffect effect in eidolon.QEffects.Where(qf => qf.AdditionalUnarmedStrike != null)) {
                        effect.AdditionalUnarmedStrike.Traits.Add(Trait.Fire);
                    }
                }

                if (bond.eidolonTraits.Contains(Trait.Metal)) {
                    eidolon.UnarmedStrike.Traits.Add(Trait.Metal);
                    foreach (QEffect effect in eidolon.QEffects.Where(qf => qf.AdditionalUnarmedStrike != null)) {
                        effect.AdditionalUnarmedStrike.Traits.Add(Trait.Metal);
                    }
                }

                if (bond.eidolonTraits.Contains(Trait.Water)) {
                    eidolon.UnarmedStrike.Traits.Add(Trait.Water);
                    foreach (QEffect effect in eidolon.QEffects.Where(qf => qf.AdditionalUnarmedStrike != null)) {
                        effect.AdditionalUnarmedStrike.Traits.Add(Trait.Water);
                    }
                }

                if (bond.eidolonTraits.Contains(Trait.Wood)) {
                    eidolon.UnarmedStrike.Traits.Add(Trait.Wood);
                    foreach (QEffect effect in eidolon.QEffects.Where(qf => qf.AdditionalUnarmedStrike != null)) {
                        effect.AdditionalUnarmedStrike.Traits.Add(Trait.Wood);
                    }
                }
            }

            if (eidolon.Level >= 7) {
                eidolon.AddQEffect(new QEffect("Weapon Specialization", "+2 weapon damage.") {
                    BonusToDamage = ((self, action, target) => {
                        if (action.Item == null)
                            return (Bonus)null;
                        Proficiency proficiency = action.Owner.Proficiencies.Get(action.Item.Traits);
                        return proficiency >= Proficiency.Expert ? new Bonus(proficiency == Proficiency.Expert ? 2 : (proficiency == Proficiency.Master ? 3 : (proficiency == Proficiency.Legendary ? 4 : 0)), BonusType.Untyped, "Weapon specialization") : (Bonus)null;
                    })
                });
            }

            //List<Item> wornItems = summoner.CarriedItems.Where(item => item.IsWorn == true && item.HasTrait(Trait.Invested) && item.PermanentQEffectActionWhenWorn != null).ToList<Item>();


            eidolon.PostConstructorInitialization(TBattle.Pseudobattle);
            return eidolon;
        }

        private static Creature CreateEidolonBase(string name, Creature summoner, int[] abilityScores, int ac, int dexCap) {
            int strength = abilityScores[0] + (summoner.PersistentCharacterSheet.Calculated.HasFeat(ftStrengthBoost) && abilityScores[0] < 4 ? 1 : 0);
            int dexterity = abilityScores[1] + (summoner.PersistentCharacterSheet.Calculated.HasFeat(ftDexterityBoost) && abilityScores[1] < 4 ? 1 : 0);
            int constitution = abilityScores[2] + (summoner.PersistentCharacterSheet.Calculated.HasFeat(ftConstitutionBoost) ? 1 : 0);
            int intelligence = abilityScores[3] + (summoner.PersistentCharacterSheet.Calculated.HasFeat(ftIntelligenceBoost) ? 1 : 0);
            int wisdom = abilityScores[4] + (summoner.PersistentCharacterSheet.Calculated.HasFeat(ftWisdomBoost) ? 1 : 0);
            int charisma = abilityScores[5] + (summoner.PersistentCharacterSheet.Calculated.HasFeat(ftCharismaBoost) ? 1 : 0);
            int level = summoner.Level;
            int trained = 2 + level;
            int expert = trained + 2;
            int master = expert + 2;

            Abilities abilities = new Abilities(strength, dexterity, constitution, intelligence, wisdom, charisma);
            Illustration illustration1 = summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft.HasTrait(tPortrait)).Illustration;
            string name1 = name;
            List<Trait> alignment = summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault((Func<Feat, bool>)(ft => ft.HasTrait(tAlignment))).Traits;
            EidolonBond subclass = (EidolonBond) summoner.PersistentCharacterSheet.Calculated.AllFeats.FirstOrDefault(ft => ft.HasTrait(tSummonerSubclass));
            List<Trait> traits = new List<Trait>();
            for (int i = 1; i < alignment.Count; i++) {
                if (alignment[i] != Trait.Mod)
                    traits.Add(alignment[i]);
            }
            traits = traits.Concat(subclass.eidolonTraits).ToList();
            traits.Add(tEidolon);
            int perception = wisdom + (int)summoner.Proficiencies.Get(Trait.Perception) + level;
            int speed1 = 5;
            Defenses defenses = new Defenses(10 + ac + (dexterity < dexCap ? dexterity : dexCap) + (level >= 11 ? expert : trained), constitution + (level >= 11 ? master : expert), dexterity + (level >= 9 ? expert : trained), wisdom + (level >= 15 ? master : expert));
            int hp = summoner.MaxHP;
            //summoner.Skills.IsTrained
            Trait[] skillTraits = new Trait[] { Trait.Acrobatics, Trait.Arcana, Trait.Athletics, Trait.Crafting, Trait.Deception, Trait.Diplomacy, Trait.Intimidation,
                Trait.Medicine, Trait.Nature, Trait.Occultism, Trait.Performance, Trait.Religion, Trait.Society, Trait.Stealth, Trait.Survival, Trait.Thievery };
            Skills skills = new Skills();
            foreach (Trait trait in skillTraits) {
                int prof = (int)summoner.Proficiencies.Get(trait);
                Skill? skill = (Skill)Skills.TraitToSkill(trait);
                if (skill != null && prof >= 2) {
                    skills.Set((Skill)skill, prof + level + abilities.Get(Skills.GetSkillAbility((Skill)skill)));
                }
            }

            return new Creature(illustration1, name1, (IList<Trait>)traits, level, perception, speed1, defenses, hp, abilities, skills)
                .WithProficiency(Trait.Unarmed, (level >= 5 ? (level >= 13 ? Proficiency.Master : Proficiency.Expert) : Proficiency.Trained))
                .WithProficiency(Trait.Spell, level < 9 ? Proficiency.Trained : level < 17 ? Proficiency.Master : Proficiency.Master )
                .WithProficiency(Trait.UnarmoredDefense, (level >= 11 ? (level >= 19 ? Proficiency.Master : Proficiency.Expert) : Proficiency.Trained))
                .WithEntersInitiativeOrder(false)
                //.WithSpellProficiencyBasedOnSpellAttack(summoner.ClassOrSpellDC() - 10, abilities1.Strength >= abilities1.Dexterity ? Ability.Strength : Ability.Dexterity)
                .AddQEffect(new ActionShareEffect() {
                    Id = qfSharedActions,
                })
                .AddQEffect(InvestedWeaponLogic.EidolonInvestedWeaponQEffect())
                .AddQEffect(new QEffect("Eidolon Bond", "You and your eidolon share your actions and multiple attack penalty. Each round, you can use any of your actions (including reactions and free actions) for yourself or your eidolon. " +
                "Your eidolon gains all of your skill proficiancies and uses your spell attack and save DC for its special abilities.") {
                    //YouAcquireQEffect = (self, qfNew) => {
                    //    if (qfNew.Id == QEffectId.Quickened) {
                    //        summoner.AddQEffect(qfNew);
                    //        return null;
                    //    }
                    //    return qfNew;
                    //},
                    PreventTakingAction = action => {
                        if (action.ActionId == ActionId.Delay) {
                            return "Your eidolon cannot take this action.";
                        }
                        return null;
                    },
                    PreventTargetingBy = action => {
                        if (action.SpellId == SpellId.Dominate) {
                            return "Eidolons cannot be dominated.";
                        }
                        return null;
                    },
                    StateCheckWithVisibleChanges = (async qf => {
                        Creature summoner = GetSummoner(qf.Owner);

                        // Handle drained mirror addition
                        QEffect drained = qf.Owner.FindQEffect(QEffectId.Drained);
                        if (drained != null) {
                            summoner.AddQEffect(new QEffect() {
                                Id = qfDrainedMirror,
                                Key = "DrainedMirror",
                                StateCheck = drained.StateCheck,
                                Value = drained.Value,
                            });
                        } else if (qf.Owner.HasEffect(qfDrainedMirror) && summoner.HasEffect(QEffectId.Drained) == false) {
                            qf.Owner.RemoveAllQEffects(effect => effect.Id == qfDrainedMirror);
                        }

                        QEffect mummyrot = qf.Owner.FindQEffect(QEffectId.MummyRot);
                        if (mummyrot != null) {
                            summoner.AddQEffect(new QEffect() {
                                Id = qfMummyRotMirror,
                                Key = "MummyRotMirror",
                                StateCheck = mummyrot.StateCheck,
                                Value = mummyrot.Value,
                            });
                        } else if (qf.Owner.HasEffect(qfMummyRotMirror) && summoner.HasEffect(QEffectId.MummyRot) == false) {
                            qf.Owner.RemoveAllQEffects(effect => effect.Id == qfMummyRotMirror);
                        }

                        // PAST THIS POINT, INACTIVE EIDOLON NOT AFFECTED
                        if (qf.Owner.Destroyed == true) {
                            return;
                        }

                        if (qf.Owner.HP <= 0) {
                            qf.Owner.Battle.RemoveCreatureFromGame(qf.Owner);
                        }

                        if (summoner.OwningFaction != qf.Owner.OwningFaction) {
                            qf.Owner.OwningFaction = summoner.OwningFaction;
                        }

                        // Reaction
                        if (qf.Owner.Actions.IsReactionUsedUp == true) {
                            summoner.Actions.UseUpReaction();
                        }

                        // Handle AoO
                        HPShareEffect shareHP = (HPShareEffect)qf.Owner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond));
                        if (shareHP.Logs.Any(log => !log.Processed && log.Type == SummonerClassEnums.InterceptKind.TARGET)) {
                            await HandleHealthShare(summoner, qf.Owner, SummonerClassEnums.InterceptKind.TARGET);
                        }

                        HPShareEffect summonerShareHP = (HPShareEffect)summoner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond));
                        if (summonerShareHP.Logs.Any(log => !log.Processed && log.Type == SummonerClassEnums.InterceptKind.TARGET)) {
                            await HandleHealthShare(qf.Owner, summoner, SummonerClassEnums.InterceptKind.TARGET);
                        }

                        // Handle tempHP
                        if (qf.Owner.TemporaryHP < summoner.TemporaryHP) {
                            qf.Owner.GainTemporaryHP(summoner.TemporaryHP);
                        } else if (qf.Owner.TemporaryHP > summoner.TemporaryHP) {
                            summoner.GainTemporaryHP(qf.Owner.TemporaryHP);
                        }

                        // TODO: Remove debug logs
                        //summoner.Occupies.Overhead("", Color.White, "{b}Log: {/b}" + $"SUMMONER HP: {summoner.HP}");
                        //qf.Owner.Occupies.Overhead("", Color.White, "{b}Log: {/b}" + $"EIDOLON HP: {qf.Owner.HP}");

                        // Handle emergency HP correction
                        //HealthShareSafetyCheck(qf.Owner, summoner);
                    }),
                    YouAreTargeted = (Func<QEffect, CombatAction, Task>)(async (qfHealOrHarm, action) => {
                        HPShareEffect shareHP = (HPShareEffect)qfHealOrHarm.Owner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond));
                        HPShareEffect summonerShareHP = (HPShareEffect)summoner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond && qf.Source == qfHealOrHarm.Owner));

                        if (action == shareHP.CA || action == summonerShareHP.CA) {
                            return;
                        }

                        shareHP.LogAction(qfHealOrHarm.Owner, action, action.Owner, SummonerClassEnums.InterceptKind.TARGET);
                    }),
                    BonusToSpellSaveDCs = qf => {
                        if (qf.Owner.Spellcasting == null) {
                            return null;
                        }

                        Creature summoner = GetSummoner(qf.Owner);

                        int sDC = summoner.Proficiencies.Get(Trait.Spell).ToNumber(summoner.ProficiencyLevel) + summoner.Spellcasting.PrimarySpellcastingSource.SpellcastingAbilityModifier;
                        int eDC = qf.Owner.Proficiencies.Get(Trait.Spell).ToNumber(qf.Owner.ProficiencyLevel) + qf.Owner.Spellcasting.PrimarySpellcastingSource.SpellcastingAbilityModifier;
                        return new Bonus(sDC - eDC, BonusType.Untyped, "Summoner Spellcasting DC");
                    },
                    BonusToAttackRolls = (qf, action, target) => {
                        if (action.HasTrait(Trait.Spell)) {
                            if (qf.Owner.Spellcasting == null) {
                                return null;
                            }

                            Creature summoner = GetSummoner(qf.Owner);

                            int sDC = summoner.Proficiencies.Get(Trait.Spell).ToNumber(summoner.ProficiencyLevel) + summoner.Spellcasting.PrimarySpellcastingSource.SpellcastingAbilityModifier;
                            int eDC = qf.Owner.Proficiencies.Get(Trait.Spell).ToNumber(qf.Owner.ProficiencyLevel) + qf.Owner.Spellcasting.PrimarySpellcastingSource.SpellcastingAbilityModifier;
                            return new Bonus(sDC - eDC, BonusType.Untyped, "Summoner Spellcasting Attack Bonus");
                        }
                        return null;
                    },
                    AfterYouAreTargeted = (Func<QEffect, CombatAction, Task>)(async (qfShareHP, action) => {
                        Creature summoner = GetSummoner(qfShareHP.Owner);
                        Creature eidolon = qfShareHP.Owner;

                        await HandleHealthShare(eidolon, summoner, SummonerClassEnums.InterceptKind.TARGET);
                    }),
                    EndOfAnyTurn = (Action<QEffect>)(qfEndOfTurn => {
                        HPShareEffect shareHP = (HPShareEffect)qfEndOfTurn.Owner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond));
                        if (shareHP != null) {
                            shareHP.Reset();
                        }

                        Creature summoner = GetSummoner(qfEndOfTurn.Owner);
                        Creature eidolon = qfEndOfTurn.Owner;

                        // Catch unhandled hazard healing effects
                        if (summoner.HP > eidolon.HP) {
                            int healing = summoner.HP - eidolon.HP;
                            eidolon.Heal($"{healing}", shareHP.CA);
                        } else if (summoner.HP < eidolon.HP) {
                            int healing = eidolon.HP - summoner.HP;
                            summoner.Heal($"{healing}", shareHP.CA);
                        }
                    }),
                    YouAreDealtDamage = (Func<QEffect, Creature, DamageStuff, Creature, Task<DamageModification?>>)(async (qfPreHazardDamage, attacker, damageStuff, defender) => {
                        // Check if effect is coming from self
                        if (damageStuff.Power.Name == "SummonerClass: Share HP") {
                            return null;
                        }

                        HPShareEffect shareHP = (HPShareEffect)qfPreHazardDamage.Owner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond));

                        // Check if caught by target check
                        if (shareHP.CheckForTargetLog(damageStuff.Power, attacker)) {
                            return null;
                        }

                        shareHP.LogAction(qfPreHazardDamage.Owner, damageStuff.Power, attacker, SummonerClassEnums.InterceptKind.DAMAGE);
                        return null;

                        //if (attacker == qfPreHazardDamage.Owner.Battle.Pseudocreature) {
                        //    qfPreHazardDamage.Owner.Battle.Log("{b}HAZARD DAMAGE LOGGED{/b}");
                        //}
                        //return null;
                    }),
                    AfterYouTakeDamageOfKind = (async (qfPostHazardDamage, action, kind) => {
                        Creature summoner = GetSummoner(qfPostHazardDamage.Owner);
                        Creature eidolon = qfPostHazardDamage.Owner;

                        await HandleHealthShare(eidolon, summoner, SummonerClassEnums.InterceptKind.DAMAGE);
                    }),
                    AfterYouAreHealed = async (self, action, amount) => {
                        if (self.Owner.Destroyed) {
                            return;
                        }

                        // Check if effect is coming from self
                        if (action != null && action.Name == "SummonerClass: Share HP") {
                            return;
                        }

                        Creature eidolon = self.Owner;
                        Creature summoner = GetSummoner(eidolon);

                        HPShareEffect shareHP = (HPShareEffect)self.Owner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond));

                        // Check if caught by target check
                        if (shareHP.CheckForTargetLog(action, action.Owner)) {
                            return;
                        }

                        summoner.Heal(DiceFormula.FromText($"{amount}", $"Eidolon Health Share ({action.Name})"), action);
                    },
                    AfterYouTakeAction = (Func<QEffect, CombatAction, Task>)(async (qf, action) => {
                        Creature eidolon = qf.Owner;

                        // Focus points
                        if (action.HasTrait(Trait.Focus)) {
                            summoner.Spellcasting.FocusPoints = eidolon.Spellcasting.FocusPoints;
                        }

                        // MAP
                        if (summoner != null && action.Traits.Contains(Trait.Attack)) {
                            summoner.Actions.AttackedThisManyTimesThisTurn = eidolon.Actions.AttackedThisManyTimesThisTurn;
                        }
                    })
                });
        }

        private async static Task PartnerActs(Creature self, Creature partner) {
            await PartnerActs(self, partner, false, null);
        }

        private async static Task PartnerActs(Creature self, Creature partner, bool tandem, Func<CombatAction, string?>? limitations) {
            QEffect limitationEffect = null;
            if (limitations != null) {
                limitationEffect = new QEffect() {
                    PreventTakingAction = limitations
                };

                partner.AddQEffect(limitationEffect);
            }

            self.RemoveAllQEffects(qf => qf.Id == qfActTogetherToggle);

            if (partner.OwningFaction.IsEnemy)
                await Task.Delay(500);
            Creature oldActiveCreature = partner.Battle.ActiveCreature;
            await partner.Battle.GameLoop.StateCheck();
            partner.Battle.ActiveCreature = partner;
            Action<Tile> centerIfNotVisible = partner.Battle.SmartCenterIfNotVisible;
            if (centerIfNotVisible != null)
                centerIfNotVisible(partner.Occupies);
            await partner.Battle.GameLoop.StateCheck();
            //Set remaining actions for partner
            bool quickenedActionState = partner.Actions.UsedQuickenedAction;
            ActionDisplayStyle[] actionDisplays = new ActionDisplayStyle[4] { partner.Actions.FirstActionStyle, partner.Actions.SecondActionStyle, partner.Actions.ThirdActionStyle, partner.Actions.FourthActionStyle };
            int partnerActionsLeft = partner.Actions.ActionsLeft;
            if (tandem) {
                partner.Actions.ActionsLeft = 1;
                partner.Actions.UsedQuickenedAction = true;
                partner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(partner.Actions, new object[] { 0, ActionDisplayStyle.Summoned });
                partner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(partner.Actions, new object[] { 1, ActionDisplayStyle.Summoned });
                partner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(partner.Actions, new object[] { 2, ActionDisplayStyle.Available });
                partner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(partner.Actions, new object[] { 3, ActionDisplayStyle.Invisible });
            } else {
                partner.Actions.ActionsLeft = self.Actions.ActionsLeft;
                partner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(partner.Actions, new object[] { 0, self.Actions.FirstActionStyle });
                partner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(partner.Actions, new object[] { 1, self.Actions.SecondActionStyle });
                partner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(partner.Actions, new object[] { 2, self.Actions.ThirdActionStyle });

                //partner.Actions.UseUpActions(partner.Actions.ActionsLeft - self.Actions.ActionsLeft, ActionDisplayStyle.UsedUp);
                partner.Actions.QuickenedForActions = self.Actions.QuickenedForActions;
                partner.Actions.UsedQuickenedAction = self.Actions.UsedQuickenedAction;
                partner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(partner.Actions, new object[] { 3, self.Actions.FourthActionStyle });
            }
            if (partner.OwningFaction.IsPlayer)
                Sfxs.Play(SfxName.StartOfTurn, 0.2f);
            await partner.Battle.GameLoop.StateCheck();
            // Process partner's turn
            // Handle tandem attack
            Creature? tandemAttackTarget = partner.QEffects.FirstOrDefault(qf => qf.Id == qfActTogether) != null ? (Creature) partner.QEffects.FirstOrDefault(qf => qf.Id == qfActTogether).Tag : null;
            if (tandemAttackTarget != null) {
                List<Option> options = new List<Option>();
                List<Item> weapons = partner.HeldItems.Where(item => item.WeaponProperties != null).ToList();
                List<QEffect> additionalAttackEffects = partner.QEffects.Where(qf => qf.AdditionalUnarmedStrike != null).ToList();
                List<Item> additionalAttacks = new List<Item>();
                foreach (QEffect qf in additionalAttackEffects) {
                    additionalAttacks.Add(qf.AdditionalUnarmedStrike);
                }
                List<Item> strikes = new List<Item>().Concat(additionalAttacks).Concat(weapons).ToList();
                strikes.Add(partner.UnarmedStrike);
                foreach (Item obj in strikes) {
                    CombatAction strike = partner.CreateStrike(obj, partner.Actions.AttackedThisManyTimesThisTurn - 1);
                    strike.WithActionCost(0);
                    CreatureTarget targeting = (CreatureTarget) strike.Target;
                    if ((bool)targeting.IsLegalTarget(partner, tandemAttackTarget)) {
                        Option option = Option.ChooseCreature(strike.Name, tandemAttackTarget, async delegate {
                            await partner.Battle.GameLoop.FullCast(strike, new ChosenTargets {
                                ChosenCreature = tandemAttackTarget,
                                ChosenCreatures = { tandemAttackTarget }
                            });
                        }, -2.14748365E+09f).WithIllustration(strike.Illustration);
                        string text = strike.TooltipCreator?.Invoke(strike, tandemAttackTarget, 0);
                        if (text != null) {
                            option.WithTooltip(text);
                        } else if (strike.ActiveRollSpecification != null) {
                            option.WithTooltip(CombatActionExecution.BreakdownAttack(strike, tandemAttackTarget).TooltipDescription);
                        } else if (strike.SavingThrow != null && (strike.ExcludeTargetFromSavingThrow == null || !strike.ExcludeTargetFromSavingThrow(strike, tandemAttackTarget))) {
                            option.WithTooltip(CombatActionExecution.BreakdownSavingThrow(strike, tandemAttackTarget, strike.SavingThrow).TooltipDescription);
                        } else {
                            option.WithTooltip(strike.Description);
                        }
                        option.NoConfirmation = true;
                        options.Add(option);
                    }
                }
                if (options.Count > 0) {
                    Option chosenOption;
                    if (options.Count >= 2) {
                        options.Add((Option)new CancelOption(true));
                        chosenOption = (await partner.Battle.SendRequest(new AdvancedRequest(partner, "Choose a creature to Strike.", options) {
                            TopBarText = $"Choose a creature to Strike or right-click to cancel.",
                            TopBarIcon = illTandemStrike
                        })).ChosenOption;
                    } else
                        chosenOption = options[0];

                    if (chosenOption is CancelOption) {
                        return;
                    }
                    await chosenOption.Action();
                }
            } else {
                // Run regular turn
                await (Task)partner.Battle.GameLoop.GetType().GetMethod("Step4_MainPhase", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .Invoke(partner.Battle.GameLoop, new object[] { partner });
            }
            // Reset partner's actions
            if (tandem) {
                // Reset actions
                partner.Actions.ActionsLeft = partnerActionsLeft;
                partner.Actions.UsedQuickenedAction = quickenedActionState;
                for (int i = 0; i < actionDisplays.Count(); i++) {
                    partner.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(partner.Actions, new object[] { i, actionDisplays[i] });
                }
            } else {
                ActionShareEffect actionTracker = (ActionShareEffect)partner.QEffects.FirstOrDefault(qf => qf.Id == qfSharedActions);
                if (actionTracker.ResetRequired) {
                    partner.Actions.ActionsLeft = actionTracker.ActionTally;
                    partner.Actions.UsedQuickenedAction = actionTracker.UsedQuickenedAction;
                    actionTracker.Clear();
                }
                // Update own actions
                self.Actions.UseUpActions(self.Actions.ActionsLeft - partner.Actions.ActionsLeft, ActionDisplayStyle.UsedUp);
                self.Actions.UsedQuickenedAction = partner.Actions.UsedQuickenedAction;
                self.Actions.GetType().GetMethod("AnimateActionUsedTo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(self.Actions, new object[] { 3, partner.Actions.FourthActionStyle });
            }
            await partner.Battle.GameLoop.StateCheck();
            partner.Actions.WishesToEndTurn = false;
            await partner.Battle.GameLoop.StateCheck();
            partner.Battle.ActiveCreature = oldActiveCreature;
            if (limitations != null) {
                partner.RemoveAllQEffects(qf => qf == limitationEffect);
            }
            oldActiveCreature = (Creature)null;
        }

        private static void HealthShareSafetyCheck(Creature self, Creature partner) {
            if (self.MaxHP - self.Damage < partner.MaxHP - partner.Damage) {
                self.Heal(DiceFormula.FromText($"{(partner.MaxHP - partner.Damage) - (self.MaxHP - self.Damage)}", "Eidolon Health Share (failsafe)"), null);
            } else if (self.MaxHP - self.Damage > partner.MaxHP - partner.Damage) {
                partner.Heal(DiceFormula.FromText($"{(self.MaxHP - self.Damage) - (partner.MaxHP - partner.Damage)}", "Eidolon Health Share (failsafe))"), null);
            }
        }

        private async static Task HandleHealthShare(Creature self, Creature partner, SummonerClassEnums.InterceptKind interceptKind, string? actionName = null) {
            HPShareEffect selfShareHP = (HPShareEffect)self.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond && qf.Source == partner));
            HPShareEffect partnerShareHP = (HPShareEffect)partner.QEffects.FirstOrDefault<QEffect>((Func<QEffect, bool>)(qf => qf.Id == qfSummonerBond && qf.Source == self));

            List<HPShareLogEntry> list = selfShareHP.Logs.Where(l => l.Type == interceptKind && !l.Processed && l.LoggedAction != selfShareHP.CA && l.LoggedAction != partnerShareHP.CA).ToList();

            foreach (HPShareLogEntry log in list) {
                int totalHPSelf = self.HP + self.TemporaryHP;
                int totalHPPartner = partner.HP + partner.TemporaryHP;
                HPShareLogEntry? partnerLog = null;
                SummonerClassEnums.EffectKind aoeCheck = log.CompareEffects(partnerShareHP, out partnerLog);

                if (aoeCheck != SummonerClassEnums.EffectKind.NONE) {
                    // Same effect
                    if (aoeCheck == SummonerClassEnums.EffectKind.HARM) {
                        if (totalHPPartner < totalHPSelf) {
                            int damage = totalHPSelf - totalHPPartner;
                            await CommonSpellEffects.DealDirectSplashDamage(partnerShareHP.CA, DiceFormula.FromText($"{damage}", $"Eidolon Health Share ({log.LoggedAction.Name})"), self, DamageKind.Untyped);
                        } else if (totalHPPartner > totalHPSelf) {
                            int damage = totalHPPartner - totalHPSelf;
                            await CommonSpellEffects.DealDirectSplashDamage(selfShareHP.CA, DiceFormula.FromText($"{damage}", $"Eidolon Health Share ({log.LoggedAction.Name})"), partner, DamageKind.Untyped);
                        }
                    } else if (aoeCheck == SummonerClassEnums.EffectKind.HEAL) {
                        if (partner.HP < self.HP) {
                            int healing = self.HP - partner.HP;
                            partner.Heal(DiceFormula.FromText($"{healing}", $"Eidolon Health Share ({log.LoggedAction.Name})"), selfShareHP.CA);
                        } else if (partner.HP > self.HP) {
                            int healing = partner.HP - self.HP;
                            self.Heal(DiceFormula.FromText($"{healing}", $"Eidolon Health Share ({log.LoggedAction.Name})"), partnerShareHP.CA);
                        }
                    } else if (aoeCheck == SummonerClassEnums.EffectKind.HEAL_HARM) {
                        if (partnerLog.Processed) {
                            log.Processed = true;
                            continue;
                        }
                        partnerLog.Processed = true;

                        int healing = self.HP - log.HP;
                        partner.Heal(DiceFormula.FromText($"{healing}", $"Eidolon Health Share ({log.LoggedAction.Name})"), selfShareHP.CA);

                        int damage = (partnerLog.HP + partnerLog.TempHP) - totalHPPartner;
                        await CommonSpellEffects.DealDirectSplashDamage(partnerShareHP.CA, DiceFormula.FromText($"{damage}", $"Eidolon Health Share ({log.LoggedAction.Name})"), self, DamageKind.Untyped);

                        selfShareHP.UpdateLogs(damage, log);
                        partnerShareHP.UpdateLogs(-healing, log);
                    } else if (aoeCheck == SummonerClassEnums.EffectKind.HARM_HEAL) {
                        if (partnerLog.Processed) {
                            log.Processed = true;
                            continue;
                        }
                        partnerLog.Processed = true;

                        int damage = (log.HP + log.TempHP) - totalHPSelf;
                        await CommonSpellEffects.DealDirectSplashDamage(selfShareHP.CA, DiceFormula.FromText($"{damage}", $"Eidolon Health Share ({log.LoggedAction.Name})"), partner, DamageKind.Untyped);

                        int healing = partner.HP - partnerLog.HP;
                        self.Heal(DiceFormula.FromText($"{healing}", $"Eidolon Health Share ({log.LoggedAction.Name})"), partnerShareHP.CA);

                        selfShareHP.UpdateLogs(-healing, log);
                        partnerShareHP.UpdateLogs(damage, log);
                    }
                } else {
                    // Invividual effect
                    if (log.HealOrHarm(self) == SummonerClassEnums.EffectKind.HARM) {
                        //int damage = totalHPPartner - totalHPSelf;
                        int damage = (log.HP + log.TempHP) - totalHPSelf;
                        await CommonSpellEffects.DealDirectSplashDamage(selfShareHP.CA, DiceFormula.FromText($"{damage}", $"Eidolon Health Share ({log.LoggedAction.Name})"), partner, DamageKind.Untyped);
                        selfShareHP.UpdateLogs(damage, log);
                    } else if (log.HealOrHarm(self) == SummonerClassEnums.EffectKind.HEAL) {
                        int healing = self.HP - log.HP;
                        partner.Heal(DiceFormula.FromText($"{healing}", $"Eidolon Health Share ({log.LoggedAction.Name})"), selfShareHP.CA);
                        selfShareHP.UpdateLogs(-healing, log);
                    }
                }
                log.Processed = true;
            }
            selfShareHP.Clean();

                //selfShareHP.SoftReset();
        }

        private static Possibility? GenerateTandemStrikeAction(Creature self, Creature partner, Creature summoner) {
            if (partner == null || !partner.Actions.CanTakeActions() || self.QEffects.FirstOrDefault(qf => qf.Id == qfActTogether) != null)
                return (Possibility)null;
            if (self.QEffects.FirstOrDefault(qf => qf.Id == qfActTogetherToggle) != null) {
                Possibility output = (Possibility)(ActionPossibility)new CombatAction(self, illTandemStrike, "Cancel Tandem Strike",
                new Trait[] { tSummoner, tTandem }, $"Cancel tandem strike toggle.", (Target)Target.Self())
                .WithActionCost(0).WithEffectOnSelf((Action<Creature>)(self => {
                    // Remove toggle from self
                    self.RemoveAllQEffects(qf => qf.Id == qfActTogetherToggle);
                }));

                //output.WithPossibilityGroup("Tandem Actions");
                return output;
            } else {
                Possibility tandemStrike = (Possibility)(ActionPossibility)new CombatAction(self, illTandemStrike, "Enable Tandem Strike",
                new Trait[] { tSummoner, tTandem },
                "{b}Frequency: {/b} once per round\n\n" + (self == summoner ? "Your" : "Your eidolon's") + " next strike action grants " + (self == summoner ? "your eidolon" : "you") + " an immediate bonus tandem turn, where " + (self == summoner ? "they" : "you") + " they can make a single strike action.",
                (Target)Target.Self()) {
                    ShortDescription = (self == summoner ? "Your" : "Your eidolon's") + " next strike action grants " + (self == summoner ? "your eidolon" : "you") + " an immediate bonus tandem turn, where " + (self == summoner ? "they" : "you") + " they can make a single strike action."
                }
                    .WithActionCost(0)
                    .WithEffectOnSelf((Action<Creature>)(self => {
                        // Give toggle qf to self
                        self.AddQEffect(new QEffect("Tandem Strike Toggled", "Your next strike action cost will also grant a free strike action to your bonded partner.") {
                            Id = qfActTogetherToggle,
                            ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
                            Illustration = illTandemStrike,
                            PreventTakingAction = (a => {
                                if (!a.HasTrait(Trait.Strike) && a.Name != "Cancel Tandem Strike") {
                                    return "Tandem strike can only be activated by the strike action.";
                                }
                                return null;
                            }),
                            AfterYouTakeAction = (Func<QEffect, CombatAction, Task>)(async (qf, action) => {
                                if (action.HasTrait(Trait.Strike)) {
                                    self.RemoveAllQEffects(qf => qf.Id == qfActTogetherToggle);

                                    if (GetEidolon(summoner)?.FindQEffect(QEffectId.Confused) != null && (await summoner.Battle.SendRequest(new ConfirmationRequest(summoner, "Your eidolon is confused will use their tandem turn to attack the nearest creature. Are you sure you want to swap to them?", GetEidolon(summoner)?.Illustration, "Yes", "No, skip their action"))).ChosenOption is CancelOption) {
                                        return;
                                    }

                                    self.AddQEffect(new QEffect {
                                        PreventTakingAction = action => action.Name == "Enable Tandem Strike" ? "Tandem strike already used this round" : null,
                                        ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn
                                    });
                                    partner.AddQEffect(new QEffect {
                                        PreventTakingAction = action => action.Name == "Enable Tandem Strike" ? "Tandem strike already used this round" : null,
                                        ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn
                                    });
                                    QEffect actTogether = new QEffect("Tandem Strike", "Immediately take a single strike action.") {
                                        Illustration = IllustrationName.Haste,
                                        Id = qfActTogether,
                                    };
                                    partner.AddQEffect(actTogether);
                                    actTogether.Tag = action.ChosenTargets.ChosenCreature;
                                    await PartnerActs(self, partner, true, (a => {
                                        if (!a.HasTrait(Trait.Strike)) {
                                            return "Only the strike action is allowed during a tandem strike turn.";
                                        }
                                        return null;
                                    }));
                                    partner.RemoveAllQEffects(effect => effect == actTogether);
                                }
                            })
                        });
                    }));
                //tandemStrike.WithPossibilityGroup("Tandem Actions");
                return tandemStrike;
            }
        }

        private static Possibility? GenerateTandemMovementAction(Creature self, Creature partner, Creature summoner) {
            if (partner == null || !partner.Actions.CanTakeActions() || self.QEffects.FirstOrDefault(qf => qf.Id == qfActTogether) != null)
                return (Possibility)null;
            if (self.QEffects.FirstOrDefault(qf => qf.Id == qfActTogetherToggle) != null) {
                Possibility output = (Possibility)(ActionPossibility)new CombatAction(self, illTandemMovement, "Cancel Tandem Movement",
                new Trait[] { tSummoner, tTandem }, $"Cancel tandem movement toggle.", (Target)Target.Self())
                    .WithActionCost(0).WithEffectOnSelf((Action<Creature>)(self => {
                    // Remove toggle from self
                    self.RemoveAllQEffects(qf => qf.Id == qfActTogetherToggle);
                }));

                //output.WithPossibilityGroup("Tandem Actions");
                return output;
            } else {
                Possibility tandemMove = (Possibility)(ActionPossibility)new CombatAction(self, illTandemMovement, "Enable Tandem Movement",
                new Trait[] { tSummoner, tTandem },
                "{b}Frequency: {/b} once per round\n\n" + (self == summoner ? "Your" : "Your eidolon's") + " next stride action grants " + (self == summoner ? "your eidolon" : "you") + " an immediate bonus tandem turn, where " + (self == summoner ? "they" : "you") + " they can make a single stride action.",
                (Target)Target.Self()) {
                    ShortDescription = (self == summoner ? "Your" : "Your eidolon's") + " next stride action grants " + (self == summoner ? "your eidolon" : "you") + " an immediate bonus tandem turn, where " + (self == summoner ? "they" : "you") + " they can make a single stride action."
                }
                    .WithActionCost(0)
                    .WithEffectOnSelf((Action<Creature>)(self => {
                        // Give toggle qf to self
                        self.AddQEffect(new QEffect("Tandem Movement Toggled", "Your next stride action cost will also grant a free stride action to your bonded partner.") {
                            Id = qfActTogetherToggle,
                            ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
                            Illustration = illTandemMovement,
                            PreventTakingAction = (a => {
                                if (a.ActionId != ActionId.Stride && a.Name != "Cancel Tandem Movement") {
                                    return "Tandem movement can only be activated by the stride action.";
                                }
                                return null;
                            }),
                            AfterYouTakeAction = (Func<QEffect, CombatAction, Task>)(async (qf, action) => {
                                if (action.ActionId == ActionId.Stride) {
                                    self.RemoveAllQEffects(qf => qf.Id == qfActTogetherToggle);

                                    if (GetEidolon(summoner)?.FindQEffect(QEffectId.Confused) != null && (await summoner.Battle.SendRequest(new ConfirmationRequest(summoner, "Your eidolon is confused and will move randomly. Are you sure you want to swap to them?", GetEidolon(summoner)?.Illustration, "Yes", "No, skip their action"))).ChosenOption is CancelOption) {
                                        return;
                                    }

                                    self.AddQEffect(new QEffect {
                                        PreventTakingAction = action => action.Name == "Enable Tandem Movement" ? "Tandem movement already used this round" : null,
                                        ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn
                                    });
                                    partner.AddQEffect(new QEffect {
                                        PreventTakingAction = action => action.Name == "Enable Tandem Movement" ? "Tandem movement already used this round" : null,
                                        ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn
                                    });
                                    QEffect actTogether = new QEffect("Tandem Movement", "Immediately take a single stride action.") {
                                        Illustration = IllustrationName.Haste,
                                        Id = qfActTogether,
                                    };
                                    partner.AddQEffect(actTogether);
                                    await PartnerActs(self, partner, true, (a => {
                                        if (a.ActionId != ActionId.Stride) {
                                            return "Only the stride action is allowed during a tandem movement turn.";
                                        }
                                        return null;
                                    }));
                                    partner.RemoveAllQEffects(effect => effect == actTogether);
                                }
                            })
                        });
                    }));
                    //tandemMove.WithPossibilityGroup("Tandem Actions");
                    return tandemMove;
            }
        }

        private static Possibility? GenerateActTogetherAction(Creature self, Creature partner, Creature summoner) {
            if (partner == null || !partner.Actions.CanTakeActions() || self.QEffects.FirstOrDefault(qf => qf.Id == qfActTogether) != null)
                return (Possibility)null;
            if (self.QEffects.FirstOrDefault(qf => qf.Id == qfActTogetherToggle) != null) {
                Possibility output = (Possibility)(ActionPossibility)new CombatAction(self, illActTogether, "Cancel Act Together",
                new Trait[] { tSummoner, tTandem }, $"Cancel act together toggle.", (Target)Target.Self())
                .WithActionCost(0).WithEffectOnSelf((Action<Creature>)(self => {
                    // Remove toggle from self
                    self.RemoveAllQEffects(qf => qf.Id == qfActTogetherToggle);
                }));

                //output.WithPossibilityGroup("Tandem Actions");
                return output;
            } else {
                Possibility actTogether = (Possibility)(ActionPossibility)new CombatAction(self, illActTogether, "Enable Act Together",
                new Trait[] { tSummoner, tTandem },
                "{b}Frequency: {/b} once per round\n\n" + (self == summoner ? "Your" : "Your eidolon's") + " next action grants " + (self == summoner ? "your eidolon" : "you") + " an immediate bonus tandem turn, where " + (self == summoner ? "they" : "you") + " they can make a single action.",
                (Target)Target.Self()) {
                    ShortDescription = (self == summoner ? "Your" : "Your eidolon's") + " next action grants " + (self == summoner ? "your eidolon" : "you") + " an immediate bonus tandem turn, where " + (self == summoner ? "they" : "you") + " they can make a single action."
                }
                    .WithActionCost(0)
                    .WithEffectOnSelf((Action<Creature>)(self => {
                    // Give toggle qf to self
                    self.AddQEffect(new QEffect("Act Together Toggled", "Your next action of 1+ cost will also grant a single quickened action to your bonded partner.") {
                        Id = qfActTogetherToggle,
                        ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
                        Illustration = illActTogetherStatus,
                        AfterYouTakeAction = (Func<QEffect, CombatAction, Task>)(async (qf, action) => {
                            if (action.ActuallySpentActions > 0) {
                                self.RemoveAllQEffects(qf => qf.Id == qfActTogetherToggle);

                                if (GetEidolon(summoner)?.FindQEffect(QEffectId.Confused) != null && (await summoner.Battle.SendRequest(new ConfirmationRequest(summoner, "Your eidolon is confused will use their tandem turn to attack the nearest creature. Are you sure you want to swap to them?", GetEidolon(summoner).Illustration, "Yes", "No, skip their action"))).ChosenOption is CancelOption) {
                                    return;
                                }

                                self.AddQEffect(new QEffect {
                                    PreventTakingAction = action => action.Name == "Enable Act Together" ? "Act together already used this round" : null,
                                    ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn
                                });
                                partner.AddQEffect(new QEffect {
                                    PreventTakingAction = action => action.Name == "Enable Act Together" ? "Act together already used this round" : null,
                                    ExpiresAt = ExpirationCondition.ExpiresAtStartOfYourTurn
                                });
                                QEffect actTogether = new QEffect("Act Together", "Immediately take a single 1 cost action.") {
                                    Illustration = IllustrationName.Haste,
                                    Id = qfActTogether,
                                };
                                partner.AddQEffect(actTogether);

                                await PartnerActs(self, partner, true, null);
                                partner.RemoveAllQEffects(effect => effect == actTogether);
                            }
                        })
                    });
                }));
                //actTogether.WithPossibilityGroup("Tandem Actions");
                return actTogether;
            }
        }

        
        private static string DirToFeatName(string text, out Trait category) {
            category = Trait.None;
            
            // Trim directions
            if (text.Contains("SummonerAssets/EidolonPortraits/ConvertedBaseGameAssets/"))
                text = text.Substring(32 + 24);
            else
                text = text.Substring(32);

            // Trim category
            if (text.StartsWith("Beast")) {
                category = Trait.Beast;
                text = text.Substring("Beast".Length + 1);
            } else if (text.StartsWith("Construct")) {
                category = Trait.Construct;
                text = text.Substring("Construct".Length + 1);
            } else if (text.StartsWith("Dragon")) {
                category = Trait.Dragon;
                text = text.Substring("Dragon".Length + 1);
            } else if (text.StartsWith("Elemental")) {
                category = Trait.Elemental;
                text = text.Substring("Elemental".Length + 1);
            } else if (text.StartsWith("Humanoid")) {
                category = Trait.Humanoid;
                text = text.Substring("Humanoid".Length + 1);
            } else if (text.StartsWith("Outsider")) {
                category = tOutsider;
                text = text.Substring("Outsider".Length + 1);
            } else if (text.StartsWith("Undead")) {
                category = Trait.Undead;
                text = text.Substring("Undead".Length + 1);
            }

            // From Github User Binary Worrier: https://stackoverflow.com/a/272929
            if (string.IsNullOrWhiteSpace(text))
                return "";
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++) {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(text[i]);
            }
            string output = newText.ToString();
            // This part was added on by me
            if (output.EndsWith(".png")) {
                output = output.Substring(0, output.Length - 4);
            }
            if (output.EndsWith("256")) {
                output = output.Substring(0, output.Length - 3);
            }
            return output;
        }

        internal static async Task<Tile> GetChargeTiles(Creature self, MovementStyle movementStyle, int minimumDistance, string msg, Illustration img) {
            Tile startingPos = self.Occupies;
            Vector2 pos = self.Occupies.ToCenterVector();
            List<Option> options = new List<Option>();
            Dictionary<Option, Tile> pairs = new Dictionary<Option, Tile>();

            PathfindingDescription pathfindingDescription = new PathfindingDescription() {
                Squares = movementStyle.MaximumSquares,
                Style = movementStyle
            };

            IList<Tile>? tiles = Pathfinding.Floodfill(self, self.Battle, pathfindingDescription);

            if (tiles == null) {
                self.Occupies.Overhead("cannot move", Color.White);
                return null;
            }

            movementStyle.MaximumSquares *= 5;
            movementStyle.ShortestPath = true;

            // Compile destination tiles
            foreach (Tile tile in tiles) {
                // Check if valid tile
                if (!tile.LooksFreeTo(self)) {
                    continue;
                }

                // Handle minimum distance
                if (self.DistanceTo(tile) < minimumDistance) {
                    continue;
                }

                // Handle LoS
                if (new CoverKind[] { CoverKind.Greater, CoverKind.Blocked, CoverKind.Standard }.Contains(self.HasLineOfEffectTo(tile))) {
                    continue;
                }

                // Add tile as option
                CombatAction movement = new CombatAction(self, img, "Beast's Charge", new Trait[] { Trait.Move }, "", Target.Tile((cr, t) => t.LooksFreeTo(cr), (cr, t) => (float)int.MinValue)
                    .WithPathfindingGuidelines((cr => pathfindingDescription))
                )
                .WithActionCost(0)
                ;
                options.Add(movement.CreateUseOptionOn(tile));
                pairs.Add(options.Last(), tile);
            }

            // Adds a Cancel Option
            options.Add(new CancelOption(true));

            // Prompts the user for their desired tile and returns it or null
            Option selectedOption = (await self.Battle.SendRequest(new AdvancedRequest(self, msg, options) {
                IsMainTurn = false,
                IsStandardMovementRequest = true,
                TopBarIcon = img,
                TopBarText = msg
            })).ChosenOption;

            if (selectedOption != null) {
                if (selectedOption is CancelOption cancel) {
                    return null;
                }

                return pairs[selectedOption];
            }

            return null;
        }

        private static AreaSelection? DetermineTilesCopy(Creature caster, BurstAreaTarget burstAreaTarget, Vector2 burstOrigin, bool ignoreBurstOriginLoS = false) {
            Vector2 vector2 = burstOrigin;
            Microsoft.Xna.Framework.Point point = new Microsoft.Xna.Framework.Point((int) caster.Occupies.X, (int)caster.Occupies.X);
            Coverlines coverlines = caster.Battle.Map.Coverlines;
            bool flag1 = true;
            for (int targetCorner = 0; targetCorner < 4; ++targetCorner) {
                Point corner = Coverlines.CreateCorner(point.X, point.Y, targetCorner);
                if (!coverlines.GetCorner(corner.X, corner.Y, (int)burstOrigin.X, (int)burstOrigin.Y)) {
                    flag1 = false;
                    break;
                }
            }
            if (flag1 & ignoreBurstOriginLoS)
                return (AreaSelection)null;
            AreaSelection tiles = new AreaSelection();
            foreach (Tile allTile in caster.Battle.Map.AllTiles) {
                Vector2 centerVector = allTile.ToCenterVector();
                if ((double)DistanceBetweenCenters(vector2, centerVector) <= (double)burstAreaTarget.Radius) {
                    bool flag2 = false;
                    for (int targetCorner = 0; targetCorner < 4; ++targetCorner) {
                        Microsoft.Xna.Framework.Point corner = Coverlines.CreateCorner(allTile.X, allTile.Y, targetCorner);
                        if (!coverlines.GetCorner((int)burstOrigin.X, (int)burstOrigin.Y, corner.X, corner.Y)) {
                            if (!allTile.AlwaysBlocksLineOfEffect) {
                                flag2 = true;
                                break;
                            }
                            break;
                        }
                    }
                    if (flag2)
                        tiles.TargetedTiles.Add(allTile);
                    else
                        tiles.ExcludedTiles.Add(allTile);
                }
            }
            return tiles;
        }

        private static float DistanceBetweenCenters(Vector2 pointOne, Vector2 pointTwo) {
            float num = Math.Abs(pointOne.X - pointTwo.X);
            float num2 = Math.Abs(pointOne.Y - pointTwo.Y);
            if (num >= num2) {
                return num + num2 / 2f;
            }

            return num2 + num / 2f;
        }
    }
}

//Yes, indeed.  To prevent an action from showing up on the stat block, add the trait Trait.Basic to its list of trait. Not too intuitive, sorry.