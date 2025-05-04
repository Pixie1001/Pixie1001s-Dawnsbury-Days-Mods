using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Ids {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class ModTraits {
        internal static Trait Wand { get; } = ModManager.RegisterTrait("RL_Wand", new TraitProperties("Wand", true, "Wands allow a creature capable of casting their spell to do so once per day.", false));
        internal static Trait CasterWeapon { get; } = ModManager.RegisterTrait("RL_Caster Weapon", new TraitProperties("Caster Weapon", false));
        internal static Trait CannotHavePropertyRune { get; } = ModManager.RegisterTrait("RL_CannotHavePropertyRune", new TraitProperties("Specific Magic Weapon", true, "This weapon cannot be attached with property runes or new materials.", false));
        internal static Trait LegendaryItem { get; } = ModManager.RegisterTrait("RL_LegendaryItem", new TraitProperties("Legendary Weapon", true, "This weapon cannot be attached with runes of any kind.", false));
        internal static Trait BoostedWeapon { get; } = ModManager.RegisterTrait("RL_BoostedWeapon", new TraitProperties("Boosted Weapon", false));
        internal static Trait Spider { get; } = ModManager.RegisterTrait("RL_Spider", new TraitProperties("Spider", true));
        internal static Trait Drow { get; } = ModManager.RegisterTrait("RL_Drow", new TraitProperties("Drow", true));
        internal static Trait Witch { get; } = ModManager.RegisterTrait("RL_Witch", new TraitProperties("Witch", false));
        internal static Trait Hexshot { get; } = ModManager.RegisterTrait("RL_Hexshot", new TraitProperties("Hexshot", false));
        internal static Trait Roguelike { get; } = ModManager.RegisterTrait("RL_RoguelikemodSignature", new TraitProperties("Roguelike", true));
        internal static Trait Archetype { get; } = ModManager.RegisterTrait("RL_ArchetypeFeat", new TraitProperties("Archetype", true));
        internal static Trait Dedication { get; } = ModManager.RegisterTrait("RL_ArchetypeDedication", new TraitProperties("Dedication", true));
        internal static Trait Event { get; } = ModManager.RegisterTrait("RL_EventFeat", new TraitProperties("Event", true, "These feats can be unlocked by meeting specific conditions in the Roguelike mode adventure path.", false) {
            BackgroundColor = Color.MediumPurple,
            WhiteForeground = true
        });
        internal static Trait MonkWeapon { get; } = ModManager.RegisterTrait("RL_MonkWeapon", new TraitProperties("Monk", true));
        internal static Trait Reload0 { get; } = ModManager.RegisterTrait("RL_Reload0", new TraitProperties("Reload 0", true, "While you have a free hand, you can draw and throw this weapon as a single action.", true));

        // Mutator Traits
        internal static Trait UniversalMutator { get; } = ModManager.RegisterTrait("UniversalMutator", new TraitProperties("UniversalMutator", false));
        internal static Trait SpellcasterMutator { get; } = ModManager.RegisterTrait("SpellcasterMutator", new TraitProperties("SpellcasterMutator", false));
        internal static Trait MeleeMutator { get; } = ModManager.RegisterTrait("MeleeMutator", new TraitProperties("MeleeMutator", false));
        internal static Trait ArcherMutator { get; } = ModManager.RegisterTrait("ArcherMutator", new TraitProperties("ArcherMutator", false));

    }
}
