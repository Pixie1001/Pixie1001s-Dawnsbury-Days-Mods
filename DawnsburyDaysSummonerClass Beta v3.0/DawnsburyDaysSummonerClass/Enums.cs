using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawnsbury.Mods.Classes.Summoner {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class Enums {
        // Traits
        internal static Trait tSummoner = ModManager.RegisterTrait("SummonerTrait", new TraitProperties("Summoner", true) { IsClassTrait = true });
        internal static Trait tEvolution = ModManager.RegisterTrait("EvolutionTrait", new TraitProperties("Evolution", true));
        internal static Trait tTandem = ModManager.RegisterTrait("TandemTrait", new TraitProperties("Tandem", true));
        internal static Trait tEidolon = ModManager.RegisterTrait("EidolonCompanion", new TraitProperties("Eidolon", true));
        internal static Trait tPrimaryAttackType = ModManager.RegisterTrait("EidolonPrimaryWeaponType", new TraitProperties("Eidolon Primary Weapon Type", false));
        internal static Trait tPrimaryAttackStats = ModManager.RegisterTrait("EidolonPrimaryWeaponStats", new TraitProperties("Eidolon Primary Weapon Stats", false));
        internal static Trait tSecondaryAttackType = ModManager.RegisterTrait("EidolonSecondaryWeaponType", new TraitProperties("Eidolon Secondary Weapon Type", false));
        internal static Trait tAlignment = ModManager.RegisterTrait("EidolonAlignment", new TraitProperties("Eidolon Alignment", false));
        internal static Trait tAdvancedWeaponryAtkType = ModManager.RegisterTrait("AdvancedWeaponAttackType", new TraitProperties("Advanced Weaponry Attack Type", false));
        internal static Trait tAdvancedWeaponryAtkTrait = ModManager.RegisterTrait("AdvancedWeaponAttackTrait", new TraitProperties("Advanced Weaponry Attack Trait", false));
        internal static Trait tEnergyHeartDamage = ModManager.RegisterTrait("EnergyHeartDamage", new TraitProperties("Energy Heart Damage Type", false));
        internal static Trait tEnergyHeartWeapon = ModManager.RegisterTrait("EnergyHeartWeapon", new TraitProperties("Energy Heart Weapon", false));
        internal static Trait tGrapple = ModManager.RegisterTrait("SummonerGrapple", new TraitProperties("Grapple", true, "You can add your item bonus to grapple checks made using this weapon."));
        internal static Trait tBreathWeaponArea = ModManager.RegisterTrait("SummonerBreathWeaponArea", new TraitProperties("Breath Weapon Area", false));
        internal static Trait tDragonType = ModManager.RegisterTrait("SummonerDragonType", new TraitProperties("Dragon Type", false));
        internal static Trait tElementalType = ModManager.RegisterTrait("SummonerElementalType", new TraitProperties("Elemental Type", false));
        internal static Trait tPortrait = ModManager.RegisterTrait("EidolonPortrait", new TraitProperties("Portrait", true));
        internal static Trait tPortraitCategory = ModManager.RegisterTrait("EidolonPortraitCategory", new TraitProperties("Portrait Category", true));
        internal static Trait tOutsider = ModManager.RegisterTrait("EidolonPortraitOutsiderCategory", new TraitProperties("Outsider", true));
        internal static Trait tEidolonASI = ModManager.RegisterTrait("EidolonASIBoost", new TraitProperties("Ability Score Boost", false));
        internal static Trait tParry = ModManager.RegisterTrait("EidolonParryTrait", new TraitProperties("Parry", true, "While wielding this weapon, if your proficiency with it is trained or better, you can spend a single action to position your weapon defensively, gaining a +1 circumstance bonus to AC until the start of your next turn."));
        internal static Trait tEidolonSpellLvl1 = ModManager.RegisterTrait("EidolonSpellLevel1", new TraitProperties("", false));
        internal static Trait tEidolonSpellLvl2 = ModManager.RegisterTrait("EidolonSpellLevel2", new TraitProperties("", false));
        internal static Trait tEidolonSpellFeat = ModManager.RegisterTrait("EidolonSpellFeat", new TraitProperties("", false));
        internal static Trait tEidolonsWrathType = ModManager.RegisterTrait("EidolonsWrathDamageType", new TraitProperties("", false));
        internal static Trait tSummonerSubclass = ModManager.RegisterTrait("SummonerEidolonBond", new TraitProperties("Eidolon", false));
        internal static Trait tMetalElementalAtkType = ModManager.RegisterTrait("SummonerMetalElementalAttackType", new TraitProperties("", false));
        internal static Trait tEidolonArray = ModManager.RegisterTrait("eidolonabilityscorearray", new TraitProperties("Eidolon Ability Score Array", false));

        // Feat names
        internal static FeatName classSummoner = ModManager.RegisterFeatName("SummonerClass", "Summoner");

        internal static FeatName scAngelicEidolon = ModManager.RegisterFeatName("Angel Eidolon");
        internal static FeatName scAngelicEidolonAvenger = ModManager.RegisterFeatName("Angelic Avenger");
        internal static FeatName scAngelicEidolonEmmissary = ModManager.RegisterFeatName("Angelic Emmisary");

        internal static FeatName scDraconicEidolon = ModManager.RegisterFeatName("Dragon Eidolon");
        internal static FeatName scDraconicEidolonCunning = ModManager.RegisterFeatName("Cunning Dragon");
        internal static FeatName scDraconicEidolonMarauding = ModManager.RegisterFeatName("Marauding Dragon");
        internal static FeatName ftBreathWeaponLine = ModManager.RegisterFeatName("Breath Weapon: Line");
        internal static FeatName ftBreathWeaponCone = ModManager.RegisterFeatName("Breath Weapon: Cone");

        internal static FeatName scBeastEidolon = ModManager.RegisterFeatName("Beast Eidolon");
        internal static FeatName scBeastEidolonBrutal = ModManager.RegisterFeatName("Brutal Beast");
        internal static FeatName scBeastEidolonFleet = ModManager.RegisterFeatName("Fleet Beast");

        internal static FeatName scDevoPhantomEidolon = ModManager.RegisterFeatName("Devotion Phantom");
        internal static FeatName scDevoPhantomEidolonStalwart = ModManager.RegisterFeatName("Stalward Guardian");
        internal static FeatName scDevoPhantomEidolonSwift = ModManager.RegisterFeatName("Swift Protector");

        internal static FeatName scAzataEidolon = ModManager.RegisterFeatName("Azata Eidolon");
        internal static FeatName scAzataEidolonCrusader = ModManager.RegisterFeatName("Crusader Azata");
        internal static FeatName scAzataEidolonPoet = ModManager.RegisterFeatName("Poet Azata");

        internal static FeatName scFeyEidolon = ModManager.RegisterFeatName("Fey Eidolon");
        internal static FeatName scFeyEidolonSkirmisher = ModManager.RegisterFeatName("Skirmisher Fey");
        internal static FeatName scFeyEidolonTrickster = ModManager.RegisterFeatName("Trickster Fey");

        internal static FeatName scDevilEidolon = ModManager.RegisterFeatName("Devil Eidolon");
        internal static FeatName scDevilEidolonLegionnaire = ModManager.RegisterFeatName("Inferal Legionnaire");
        internal static FeatName scDevilEidolonBarrister = ModManager.RegisterFeatName("Infernal Barrister");

        internal static FeatName scAngerPhantom = ModManager.RegisterFeatName("Anger Phantom Eidolon");
        internal static FeatName scAngerPhantomBerserker = ModManager.RegisterFeatName("Wrathful Berserker");
        internal static FeatName scAngerPhantomAssassin = ModManager.RegisterFeatName("Enraged Assassin");

        internal static FeatName scPlantEidolon = ModManager.RegisterFeatName("Plant Eidolon");
        internal static FeatName scPlantEidolonCreeping = ModManager.RegisterFeatName("Creeping Plant");
        internal static FeatName scPlantEidolonGuardian = ModManager.RegisterFeatName("Guardian Plant");

        internal static FeatName scUndeadEidolon = ModManager.RegisterFeatName("Undead Eidolon");
        internal static FeatName scUndeadEidolonBrute = ModManager.RegisterFeatName("Undead Brute");
        internal static FeatName scUndeadEidolonStalker = ModManager.RegisterFeatName("Undead Stalker");

        internal static FeatName scPsychopompEidolon = ModManager.RegisterFeatName("Psychopomp Eidolon");
        internal static FeatName scPsychopompEidolonScribe = ModManager.RegisterFeatName("Scribe of the Dead");
        internal static FeatName scPsychopompEidolonGuardian = ModManager.RegisterFeatName("Soul Guardian");

        internal static FeatName scElementalEidolon = ModManager.RegisterFeatName("Elemental Eidolon");
        internal static FeatName scElementalEidolonPrimordial = ModManager.RegisterFeatName("Primordial Elemental");
        internal static FeatName scElementalEidolonAdaptable = ModManager.RegisterFeatName("Adaptable Elemental");

        // Class Feat names
        internal static FeatName ftAbundantSpellcasting1 = ModManager.RegisterFeatName("AbundantSpellCastingSummoner1", "Abundant Spellcasting");
        internal static FeatName ftAbundantSpellcasting4 = ModManager.RegisterFeatName("AbundantSpellCastingSummoner4", "Abundant Spellcasting 2");
        public static FeatName ftBoostSummons = ModManager.RegisterFeatName("SummonerClassFeatBoostSummons", "Boost Summons");
        public static FeatName ftMagicalUnderstudy = ModManager.RegisterFeatName("SummonerMagicalUnderstudy", "Magical Understudy");
        public static FeatName ftMagicalAdept = ModManager.RegisterFeatName("SummonerMagicalAdept", "Magical Adept");
        public static FeatName ftAirbornForm = ModManager.RegisterFeatName("Airborn Form");

        // Primary Weapon Feat Names
        internal static FeatName ftPSword = ModManager.RegisterFeatName("P_Sword", "Sword");
        internal static FeatName ftPPolearm = ModManager.RegisterFeatName("P_Polearm", "Polearm");
        internal static FeatName ftPMace = ModManager.RegisterFeatName("P_Mace", "Mace");
        internal static FeatName ftPWing = ModManager.RegisterFeatName("P_Wing", "Wing");
        internal static FeatName ftPKick = ModManager.RegisterFeatName("P_Kick", "Kick");
        internal static FeatName ftPClaw = ModManager.RegisterFeatName("P_Claw", "Claw");
        internal static FeatName ftPJaws = ModManager.RegisterFeatName("P_Jaws", "Jaws");
        internal static FeatName ftPFist = ModManager.RegisterFeatName("P_Fist", "Fist");
        internal static FeatName ftPTendril = ModManager.RegisterFeatName("P_Tendril", "Tendril");
        internal static FeatName ftPHorn = ModManager.RegisterFeatName("P_Horn", "Horn");
        internal static FeatName ftPTail = ModManager.RegisterFeatName("P_Tail", "Tail");

        // Primary Weapon Statblock Feat Names
        internal static FeatName ftPSPowerful = ModManager.RegisterFeatName("PS_Powerful", "Powerful");
        internal static FeatName ftPSFatal = ModManager.RegisterFeatName("PS_Fatal", "Fatal");
        internal static FeatName ftPSUnstoppable = ModManager.RegisterFeatName("PS_Unstoppable", "Unstoppable");
        internal static FeatName ftPSGraceful = ModManager.RegisterFeatName("PS_Graceful", "Graceful");

        // Secondary Weapon Feat Names
        internal static FeatName ftSWing = ModManager.RegisterFeatName("S_Wing", "Wing");
        internal static FeatName ftSKick = ModManager.RegisterFeatName("S_Kick", "Kick");
        internal static FeatName ftSClaw = ModManager.RegisterFeatName("S_Claw", "Claw");
        internal static FeatName ftSJaws = ModManager.RegisterFeatName("S_Jaws", "Jaws");
        internal static FeatName ftSFist = ModManager.RegisterFeatName("S_Fist", "Fist");
        internal static FeatName ftSTendril = ModManager.RegisterFeatName("S_Tendril", "Tendril");
        internal static FeatName ftSHorn = ModManager.RegisterFeatName("S_Horn", "Horn");
        internal static FeatName ftSTail = ModManager.RegisterFeatName("S_Tail", "Tail");

        // Ability BoostOptions
        internal static FeatName ftStrengthBoost = ModManager.RegisterFeatName("EidolonStrengthBoost", "Strength Boost");
        internal static FeatName ftDexterityBoost = ModManager.RegisterFeatName("EidolonDexterityBoost", "Dexterity Boost");
        internal static FeatName ftConstitutionBoost = ModManager.RegisterFeatName("EidolonConstitutionBoost", "Constitution Boost");
        internal static FeatName ftIntelligenceBoost = ModManager.RegisterFeatName("EidolonIntelligenceBoost", "Intelligence Boost");
        internal static FeatName ftWisdomBoost = ModManager.RegisterFeatName("EidolonWisdomBoost", "Wisdom Boost");
        internal static FeatName ftCharismaBoost = ModManager.RegisterFeatName("EidolonCharismaBoost", "Charisma Boost");
        internal static FeatName ftKeyEidolonAbilityStr = ModManager.RegisterFeatName("ftKeyEidolonAbilityStr", "Key Ability: Strength");
        internal static FeatName ftKeyEidolonAbilityDex = ModManager.RegisterFeatName("ftKeyEidolonAbilityDex", "Key Ability: Dexterity");

        // Alignment Options
        internal static FeatName ftALawfulGood = ModManager.RegisterFeatName("LawfulGood", "Lawful Good");
        internal static FeatName ftAGood = ModManager.RegisterFeatName("Good", "Good");
        internal static FeatName ftAChaoticGood = ModManager.RegisterFeatName("ChaoticGood", "Chaotic Good");
        internal static FeatName ftALawful = ModManager.RegisterFeatName("Lawful", "Lawful");
        internal static FeatName ftANeutral = ModManager.RegisterFeatName("TrueNeutral", "Neutral");
        internal static FeatName ftAChaotic = ModManager.RegisterFeatName("Chaotic", "Chaotic");
        internal static FeatName ftALawfulEvil = ModManager.RegisterFeatName("LawfulEvil", "Lawful Evil");
        internal static FeatName ftAEvil = ModManager.RegisterFeatName("Evil", "Evil");
        internal static FeatName ftAChaoticEvil = ModManager.RegisterFeatName("ChaoticEvil", "Chaotic Evil");

        // QEffectIDs
        internal static QEffectId qfSharedActions = ModManager.RegisterEnumMember<QEffectId>("Summoner_Shared Actions");
        internal static QEffectId qfSummonerBond = ModManager.RegisterEnumMember<QEffectId>("Summoner_Shared HP");
        internal static QEffectId qfActTogetherToggle = ModManager.RegisterEnumMember<QEffectId>("Act Together Toggle");
        internal static QEffectId qfActTogether = ModManager.RegisterEnumMember<QEffectId>("Act Together");
        internal static QEffectId qfExtendBoostExtender = ModManager.RegisterEnumMember<QEffectId>("Extend Boost Extended");
        internal static QEffectId qfReactiveStrikeCheck = ModManager.RegisterEnumMember<QEffectId>("Reactive Strike Check");
        internal static QEffectId qfParrying = ModManager.RegisterEnumMember<QEffectId>("Eidolon Parry");
        internal static QEffectId qfInvestedWeapon = ModManager.RegisterEnumMember<QEffectId>("Invested Weapon");
        internal static QEffectId qfDrainedMirror = ModManager.RegisterEnumMember<QEffectId>("Drained (Mirror)");
        internal static QEffectId qfMummyRotMirror = ModManager.RegisterEnumMember<QEffectId>("Mummy Rot (Mirror)");
        internal static QEffectId qfEidolonsWrath = ModManager.RegisterEnumMember<QEffectId>("Eidolon's Wrath QF");
        internal static QEffectId qfOstentatiousArrival = ModManager.RegisterEnumMember<QEffectId>("Ostentatious Arrival Toggled");
        internal static QEffectId qfWhimsicalAura = ModManager.RegisterEnumMember<QEffectId>("Whimsical Aura");
        internal static QEffectId qfSeethingFrenzy = ModManager.RegisterEnumMember<QEffectId>("Seething Frenzy");
        internal static QEffectId qfSoulSiphon = ModManager.RegisterEnumMember<QEffectId>("Soul Siphon");
        internal static QEffectId qfElementalBurst = ModManager.RegisterEnumMember<QEffectId>("Elemental Burst");

        // Actions
        internal static ActionId acCelestialPassion = ModManager.RegisterEnumMember<ActionId>("CelestialPassion");

        // Menus
        internal static PossibilitySectionId psTandemActions = ModManager.RegisterEnumMember<PossibilitySectionId>("TandemActionsSubMenu");
        internal static PossibilitySectionId psTandemActionsMain = ModManager.RegisterEnumMember<PossibilitySectionId>("TandemActionsMainBar");
        internal static PossibilitySectionId psSummonerExtra = ModManager.RegisterEnumMember<PossibilitySectionId>("SummonerActionsExtra");

        // Illustrations
        internal static ModdedIllustration illActTogether = new ModdedIllustration("SummonerAssets/ActTogether.png");
        internal static ModdedIllustration illActTogetherStatus = new ModdedIllustration("SummonerAssets/ActTogetherStatus.png");
        internal static ModdedIllustration illDismiss = new ModdedIllustration("SummonerAssets/Dismiss.png");
        internal static ModdedIllustration illEidolonBoost = new ModdedIllustration("SummonerAssets/EidolonBoost.png");
        internal static ModdedIllustration illReinforceEidolon = new ModdedIllustration("SummonerAssets/ReinforceEidolon.png");
        internal static ModdedIllustration illEvolutionSurge = new ModdedIllustration("SummonerAssets/EvolutionSurge.png");
        internal static ModdedIllustration illLifeLink = new ModdedIllustration("SummonerAssets/LifeLink.png");
        internal static ModdedIllustration illExtendBoost = new ModdedIllustration("SummonerAssets/ExtendBoost.png");
        internal static ModdedIllustration illTandemMovement = new ModdedIllustration("SummonerAssets/TandemMovement.png");
        internal static ModdedIllustration illTandemStrike = new ModdedIllustration("SummonerAssets/TandemStrike.png");
        internal static ModdedIllustration illBeastsCharge = new ModdedIllustration("SummonerAssets/BeastsCharge.png");
        internal static ModdedIllustration illInvest = new ModdedIllustration("SummonerAssets/Parry.png");
        internal static ModdedIllustration illAngelicAegis = new ModdedIllustration("SummonerAssets/AngelicAegis.png");
        internal static ModdedIllustration illDevoStance = new ModdedIllustration("SummonerAssets/DevotionStance.png");
        internal static ModdedIllustration illPrimalRoar = new ModdedIllustration("SummonerAssets/PrimalRoar.png");
        internal static ModdedIllustration illDraconicFrenzy = new ModdedIllustration("SummonerAssets/DraconicFrenzy.png");
        internal static ModdedIllustration illOstentatiousArrival = new ModdedIllustration("SummonerAssets/OstentatiousArrival.png");
        internal static ModdedIllustration illFrenziedAssault = new ModdedIllustration("SummonerAssets/FrenziedAssault.png");
        internal static ModdedIllustration illSeethingFrenzy = new ModdedIllustration("SummonerAssets/SeethingFrenzy.png");
        internal static ModdedIllustration illDisciplineTheLegion = new ModdedIllustration("SummonerAssets/DisciplineTheLegion.png");
        internal static ModdedIllustration illConstrictingHold = new ModdedIllustration("SummonerAssets/ConstrictingHold.png");
        internal static ModdedIllustration illTendrilStrike = new ModdedIllustration("SummonerAssets/TendrilStrike.png");
        internal static ModdedIllustration illSoulWrench = new ModdedIllustration("SummonerAssets/SoulWrench.png");
        internal static ModdedIllustration illElementalBurst = new ModdedIllustration("SummonerAssets/ElementalBurst.png");
    }
}
