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
using Dawnsbury.Core.Mechanics.Treasure;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Ids
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class SoundEffects
    {
        // Music
        public static Songname AntipartyTheme = ModManager.RegisterNewMusic("RoguelikeModeAssets/Music/battle-theme-194713.mp3", 0.5f);
        public static Songname MotherOfThePoolTheme = ModManager.RegisterNewMusic("RoguelikeModeAssets/Music/MotherOfThePool.mp3", 0.5f);
        public static Songname BossMusic = ModManager.RegisterNewMusic("RoguelikeModeAssets/Music/music_epic_orchestral_bg_underscore.wav", 0.5f);
        public static Songname VikingMusic = ModManager.RegisterNewMusic("RoguelikeModeAssets/Music/skjaldmr-norse-viking-background-music-110364.mp3", 0.5f);

        // SE
        public static SfxName EggHatch = ModManager.RegisterNewSoundEffect("RoguelikeModeAssets/Sfx/SPLAT Squelch 02.ogg");
        public static SfxName WandOverload = ModManager.RegisterNewSoundEffect("RoguelikeModeAssets/Sfx/DESTRUCTION Break Impact Wood 06.ogg");
        public static SfxName BebilithHiss = ModManager.RegisterNewSoundEffect("RoguelikeModeAssets/Sfx/ghost_witch_voice_hiss_11.wav");
    }
}
