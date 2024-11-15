using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Enumerations;
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
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
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
using System.Reflection.Metadata;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Microsoft.Xna.Framework;
using System.Text;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Specific;
using System.Runtime.CompilerServices;

namespace Dawnsbury.Mods.Backgrounds.BundleOfBackgrounds {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class BoBAssets {
        public static Dictionary<ImageId, ModdedIllustration> imgs = new Dictionary<ImageId, ModdedIllustration>();

        public enum ImageId {
            BERSERKERS_BREW,
            DRAGON_WHISKY,
            ROTGUT,
            E_MEAL,
            F_MEAL,
            I_MEAL,
            CONCOCT_POULTICE,
            MOONSHINE,
            NO_CAUSE_FOR_ALARM,
            FEY_FORTUNE
        }

        internal static void RegisterIllustrations() {
            imgs.Add(ImageId.BERSERKERS_BREW, new ModdedIllustration("BundleOfBackgroundsAssets/BerserkersBrew.png"));
            imgs.Add(ImageId.DRAGON_WHISKY, new ModdedIllustration("BundleOfBackgroundsAssets/DragonWhisky.png"));
            imgs.Add(ImageId.ROTGUT, new ModdedIllustration("BundleOfBackgroundsAssets/Rotgut.png"));
            imgs.Add(ImageId.E_MEAL, new ModdedIllustration("BundleOfBackgroundsAssets/EmboldeningMeal.png"));
            imgs.Add(ImageId.F_MEAL, new ModdedIllustration("BundleOfBackgroundsAssets/FortifyingMeal.png"));
            imgs.Add(ImageId.I_MEAL, new ModdedIllustration("BundleOfBackgroundsAssets/InvigoratingMeal.png"));
            imgs.Add(ImageId.CONCOCT_POULTICE, new ModdedIllustration("BundleOfBackgroundsAssets/Herbalism.png"));
            imgs.Add(ImageId.MOONSHINE, new ModdedIllustration("BundleOfBackgroundsAssets/Moonshine.png"));
            imgs.Add(ImageId.NO_CAUSE_FOR_ALARM, new ModdedIllustration("BundleOfBackgroundsAssets/NoCauseForAlarm.png"));
            imgs.Add(ImageId.FEY_FORTUNE, new ModdedIllustration("BundleOfBackgroundsAssets/FeyFortune.png"));
        }
    }
}
