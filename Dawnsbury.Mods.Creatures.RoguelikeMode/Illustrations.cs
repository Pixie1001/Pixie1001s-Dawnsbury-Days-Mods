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
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using System.Diagnostics.Metrics;
using Microsoft.Xna.Framework.Audio;
using static System.Reflection.Metadata.BlobBuilder;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Animations.Movement;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class Illustrations {
        // Enemies
        internal static ModdedIllustration DrowPriestess = new ModdedIllustration("RoguelikeModeAssets/Enemies/DrowPriestess.png");
        internal static ModdedIllustration DrowInquisitrix = new ModdedIllustration("RoguelikeModeAssets/Enemies/DrowInquisitrix.png");
        internal static ModdedIllustration DrowFighter = new ModdedIllustration("RoguelikeModeAssets/Enemies/DrowFighter.png");
        internal static ModdedIllustration DrowTempleGuard = new ModdedIllustration("RoguelikeModeAssets/Enemies/DrowTempleGuard.png");
        internal static ModdedIllustration Drider = new ModdedIllustration("RoguelikeModeAssets/Enemies/Drider.png");
        internal static ModdedIllustration HuntingSpider = new ModdedIllustration("RoguelikeModeAssets/Enemies/HuntingSpider.png");
        internal static ModdedIllustration DrowShootist = new ModdedIllustration("RoguelikeModeAssets/Enemies/DrowShootist.png");
        internal static ModdedIllustration DrowSniper = new ModdedIllustration("RoguelikeModeAssets/Enemies/DrowSniper.png");
        internal static ModdedIllustration DrowAssassin = new ModdedIllustration("RoguelikeModeAssets/Enemies/DrowAssassin.png");
        internal static ModdedIllustration DrowArcanist = new ModdedIllustration("RoguelikeModeAssets/Enemies/DrowArcanist.png");
        internal static ModdedIllustration DrowShadowcaster = new ModdedIllustration("RoguelikeModeAssets/Enemies/DrowShadowcaster.png");
        internal static ModdedIllustration AnimalFormBear = new ModdedIllustration("RoguelikeModeAssets/Enemies/AnimalFormBear.png");
        internal static ModdedIllustration AnimalFormSnake = new ModdedIllustration("RoguelikeModeAssets/Enemies/AnimalFormSnake.png");
        internal static ModdedIllustration AnimalFormWolf = new ModdedIllustration("RoguelikeModeAssets/Enemies/AnimalFormWolf.png");

        // Items
        internal static ModdedIllustration Wand = new ModdedIllustration("RoguelikeModeAssets/Items/Wand.png");
        internal static ModdedIllustration ChillwindBow = new ModdedIllustration("RoguelikeModeAssets/Items/ChillwindBow.png");
        internal static ModdedIllustration DreadPlate = new ModdedIllustration("RoguelikeModeAssets/Items/DreadPlate.png");
        internal static ModdedIllustration HungeringBlade = new ModdedIllustration("RoguelikeModeAssets/Items/HungeringBlade.png");
        internal static ModdedIllustration SpiderChopper = new ModdedIllustration("RoguelikeModeAssets/Items/SpiderChopper.png");
        internal static ModdedIllustration WebWalkerArmour = new ModdedIllustration("RoguelikeModeAssets/Items/WebWalkerArmour.png");
        internal static ModdedIllustration ProtectiveAmulet = new ModdedIllustration("RoguelikeModeAssets/Items/ProtectiveAmulet.png");
        internal static ModdedIllustration Hexshot = new ModdedIllustration("RoguelikeModeAssets/Items/HexshotPistol.png");
        internal static ModdedIllustration MaskOfConsumption = new ModdedIllustration("RoguelikeModeAssets/Items/MaskOfConsumption.png");
        internal static ModdedIllustration Widowmaker = new ModdedIllustration("RoguelikeModeAssets/Items/Widowmaker.png");

        // Terrain
        internal static ModdedIllustration ChokingMushroom = new ModdedIllustration("RoguelikeModeAssets/Terrain/ChokingMushroom.png");
        internal static ModdedIllustration BoomShroom = new ModdedIllustration("RoguelikeModeAssets/Terrain/BoomShroom.png");
        internal static ModdedIllustration SpiderShrine = new ModdedIllustration("RoguelikeModeAssets/Terrain/SpiderShrine.png");

        // Other
        internal static ModdedIllustration StatusBackdrop = new ModdedIllustration("RoguelikeModeAssets/Other/StatusBackdrop.png");
    }
}
