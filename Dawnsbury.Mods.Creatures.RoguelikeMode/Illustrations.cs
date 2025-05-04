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
        internal static ModdedIllustration DrowNecromancer = new ModdedIllustration("RoguelikeModeAssets/Enemies/DrowNecromancer.png");
        internal static ModdedIllustration AnimalFormBear = new ModdedIllustration("RoguelikeModeAssets/Enemies/AnimalFormBear.png");
        internal static ModdedIllustration AnimalFormSnake = new ModdedIllustration("RoguelikeModeAssets/Enemies/AnimalFormSnake.png");
        internal static ModdedIllustration AnimalFormWolf = new ModdedIllustration("RoguelikeModeAssets/Enemies/AnimalFormWolf.png");
        internal static ModdedIllustration DrowRenegade = new ModdedIllustration("RoguelikeModeAssets/Enemies/DrowRenegade.png");
        internal static ModdedIllustration TreasureDemon = new ModdedIllustration("RoguelikeModeAssets/Enemies/TreasureDemon.png");
        internal static ModdedIllustration AbyssalHandmaiden = new ModdedIllustration("RoguelikeModeAssets/Enemies/AbyssalHandmaiden.png");
        internal static ModdedIllustration UnseenGuardian = new ModdedIllustration("RoguelikeModeAssets/Enemies/UnseenGuardian.png");
        internal static ModdedIllustration DevotedCultist = new ModdedIllustration("RoguelikeModeAssets/Enemies/DevotedCultist.png");
        internal static ModdedIllustration Automoton = new ModdedIllustration("RoguelikeModeAssets/Enemies/Automoton.png"); // Unused
        internal static ModdedIllustration AnimatedStatue = new ModdedIllustration("RoguelikeModeAssets/Enemies/AnimatedStatue.png");
        internal static ModdedIllustration Homunculus = new ModdedIllustration("RoguelikeModeAssets/Enemies/Homunculus.png");
        internal static ModdedIllustration CrawlingHand = new ModdedIllustration("RoguelikeModeAssets/Enemies/CrawlingHand.png");
        internal static ModdedIllustration BebilithSpawn = new ModdedIllustration("RoguelikeModeAssets/Enemies/BebilithSpawn.png");
        internal static ModdedIllustration Nuglub = new ModdedIllustration("RoguelikeModeAssets/Enemies/Nuglub.png");
        internal static ModdedIllustration Owlbear = new ModdedIllustration("RoguelikeModeAssets/Enemies/Owlbear.png");
        internal static ModdedIllustration WhiteDragon = new ModdedIllustration("RoguelikeModeAssets/Enemies/WhiteDragon.png");
        internal static ModdedIllustration HuntingShark = new ModdedIllustration("RoguelikeModeAssets/Enemies/HuntingShark.png");
        internal static ModdedIllustration MerfolkKrakenborn = new ModdedIllustration("RoguelikeModeAssets/Enemies/MerfolkKrakenborn.png");
        internal static ModdedIllustration Unicorn = new ModdedIllustration("RoguelikeModeAssets/Enemies/UnicornFoal.png");
        internal static ModdedIllustration MinnowOfMulnok = new ModdedIllustration("RoguelikeModeAssets/Enemies/MinnowOfMulnok.png");
        internal static ModdedIllustration DrowAlchemist = new ModdedIllustration("RoguelikeModeAssets/Enemies/DrowAlchemist.png");
        internal static ModdedIllustration MerfolkSeaWitch = new ModdedIllustration("RoguelikeModeAssets/Enemies/MerfolkSeaWitch.png");
        internal static ModdedIllustration SpiderHatchling = new ModdedIllustration("RoguelikeModeAssets/Enemies/SpiderHatchling.png");
        internal static ModdedIllustration Ardamok = new ModdedIllustration("RoguelikeModeAssets/Enemies/Ardamok.png");
        internal static ModdedIllustration Chimera = new ModdedIllustration("RoguelikeModeAssets/Enemies/Chimera.png");
        internal static ModdedIllustration Crocodile = new ModdedIllustration("RoguelikeModeAssets/Enemies/Crocodile.png");
        internal static ModdedIllustration AngerPhantom = new ModdedIllustration("RoguelikeModeAssets/Enemies/AngerPhantom.png");
        internal static ModdedIllustration FlailSnail = new ModdedIllustration("RoguelikeModeAssets/Enemies/FlailSnail.png");

        internal static ModdedIllustration EMonsterbound = new ModdedIllustration("RoguelikeModeAssets/Enemies/Monsterbound.png");
        internal static ModdedIllustration EWombCultist = new ModdedIllustration("RoguelikeModeAssets/Enemies/WombCultist.png");
        internal static ModdedIllustration EBroodGuard = new ModdedIllustration("RoguelikeModeAssets/Enemies/BroodGuard.png");
        internal static ModdedIllustration EBroodNurse = new ModdedIllustration("RoguelikeModeAssets/Enemies/BroodNurse.png");
        internal static ModdedIllustration WinterWolf = new ModdedIllustration("RoguelikeModeAssets/Enemies/WinterWolf.png");
        internal static ModdedIllustration Sigbin = new ModdedIllustration("RoguelikeModeAssets/Enemies/Sigbin.png");
        internal static ModdedIllustration Basilisk = new ModdedIllustration("RoguelikeModeAssets/Enemies/Basilisk.png");

        // Bears for Archanophobia Mode
        internal static ModdedIllustration Bear1 = new ModdedIllustration("RoguelikeModeAssets/Enemies/Bear1.png");
        internal static ModdedIllustration Bear2 = new ModdedIllustration("RoguelikeModeAssets/Enemies/Bear2.png");
        internal static ModdedIllustration Bear3 = new ModdedIllustration("RoguelikeModeAssets/Enemies/Bear3.png");
        internal static ModdedIllustration Bear4 = new ModdedIllustration("RoguelikeModeAssets/Enemies/Bear4.png");
        internal static ModdedIllustration Bear5 = new ModdedIllustration("RoguelikeModeAssets/Enemies/Bear5.png");

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
        internal static ModdedIllustration DolmanOfVanishing = new ModdedIllustration("RoguelikeModeAssets/Items/DolmanOfVanishing.png");
        internal static ModdedIllustration CloakOfAir = new ModdedIllustration("RoguelikeModeAssets/Items/CloakOfAir.png");
        internal static ModdedIllustration BloodBondAmulet = new ModdedIllustration("RoguelikeModeAssets/Items/BloodBondAmulet.png");
        internal static ModdedIllustration HornOfTheHunt = new ModdedIllustration("RoguelikeModeAssets/Items/HornOfTheHunt.png");
        internal static ModdedIllustration DemonBoundRing = new ModdedIllustration("RoguelikeModeAssets/Items/DemonBoundRing.png");
        internal static ModdedIllustration ShifterFurs = new ModdedIllustration("RoguelikeModeAssets/Items/ShifterFurs.png");
        internal static ModdedIllustration SpiritBeaconAmulet = new ModdedIllustration("RoguelikeModeAssets/Items/SpiritBeaconAmulet.png");
        internal static ModdedIllustration DuelingSpear = new ModdedIllustration("RoguelikeModeAssets/Items/DuelingSpear.png");
        internal static ModdedIllustration KrakenMail = new ModdedIllustration("RoguelikeModeAssets/Items/KrakenMail.png");
        internal static ModdedIllustration RobesOfTheWarWizard = new ModdedIllustration("RoguelikeModeAssets/Items/RobesOfTheWarWizard.png");
        internal static ModdedIllustration WhisperMail = new ModdedIllustration("RoguelikeModeAssets/Items/WhisperMail.png");
        internal static ModdedIllustration DeathDrinkerAmulet = new ModdedIllustration("RoguelikeModeAssets/Items/DeathDrinkerAmulet.png");
        internal static ModdedIllustration SpellbanePlate = new ModdedIllustration("RoguelikeModeAssets/Items/SpellbanePlate.png");
        internal static ModdedIllustration ThrowersBandolier = new ModdedIllustration("RoguelikeModeAssets/Items/ThrowersBandolier.png");
        internal static ModdedIllustration LightHammer = new ModdedIllustration("RoguelikeModeAssets/Items/LightHammer.png");
        internal static ModdedIllustration Hatchet = new ModdedIllustration("RoguelikeModeAssets/Items/Hatchet.png");
        internal static ModdedIllustration SceptreOfTheSpider = new ModdedIllustration("RoguelikeModeAssets/Items/SceptreOfTheSpider.png");
        internal static ModdedIllustration AlicornPike = new ModdedIllustration("RoguelikeModeAssets/Items/AlicornPike.png");
        internal static ModdedIllustration AlicornDagger = new ModdedIllustration("RoguelikeModeAssets/Items/AlicornDagger.png");
        internal static ModdedIllustration SpideryHalberd = new ModdedIllustration("RoguelikeModeAssets/Items/SpideryHalberd.png");
        internal static ModdedIllustration StaffOfSpellPenetration = new ModdedIllustration("RoguelikeModeAssets/Items/StaffOfSpellPenetration.png");
        internal static ModdedIllustration Shuriken = new ModdedIllustration("RoguelikeModeAssets/Items/Shuriken.png");

        // Terrain
        internal static ModdedIllustration ChokingMushroom = new ModdedIllustration("RoguelikeModeAssets/Terrain/ChokingMushroom.png");
        internal static ModdedIllustration BoomShroom = new ModdedIllustration("RoguelikeModeAssets/Terrain/BoomShroom.png");
        internal static ModdedIllustration SpiderShrine = new ModdedIllustration("RoguelikeModeAssets/Terrain/SpiderShrine.png");
        internal static ModdedIllustration RestlessSpirit = new ModdedIllustration("RoguelikeModeAssets/Terrain/RestlessSouls.png");
        internal static ModdedIllustration DemonicPustule = new ModdedIllustration("RoguelikeModeAssets/Terrain/DemonicPustule.png");
        internal static ModdedIllustration TripWire = new ModdedIllustration("RoguelikeModeAssets/Terrain/TripWire.png");

        // Icons
        internal static ModdedIllustration RitualOfAscension = new ModdedIllustration("RoguelikeModeAssets/Icons/RitualOfAscension.png");
        internal static ModdedIllustration StabbingAppendage = new ModdedIllustration("RoguelikeModeAssets/Icons/StabbingAppendage.png");
        internal static ModdedIllustration MermaidTail = new ModdedIllustration("RoguelikeModeAssets/Icons/MermaidTail.png");
        internal static ModdedIllustration BrinyBolt = new ModdedIllustration("RoguelikeModeAssets/Icons/BrinyBolt.png");
        internal static ModdedIllustration FailedRun = new ModdedIllustration("RoguelikeModeAssets/Icons/FailedRun.png");
        internal static ModdedIllustration AvertGaze = new ModdedIllustration("RoguelikeModeAssets/Icons/AvertGaze.png");
        internal static ModdedIllustration CoverEyes = new ModdedIllustration("RoguelikeModeAssets/Icons/CoverEyes.png");

        // Other
        internal static ModdedIllustration StatusBackdrop = new ModdedIllustration("RoguelikeModeAssets/Other/StatusBackdrop.png");
        internal static ModdedIllustration BaneCircleWhite = new ModdedIllustration("RoguelikeModeAssets/Other/BaneCircleWhite.png");
        internal static ModdedIllustration InkCloud = new ModdedIllustration("RoguelikeModeAssets/Other/InkCloud.png");
        internal static ModdedIllustration KinestistCircleWhite = new ModdedIllustration("RoguelikeModeAssets/Other/KineticistAuraCircle.png");
    }
}
