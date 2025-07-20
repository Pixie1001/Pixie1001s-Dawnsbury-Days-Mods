using System;
using Dawnsbury.Audio;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Ids {

    internal static class SoundEffects {
        // Music
        public static Songname AntipartyTheme = ModManager.RegisterNewMusic("RoguelikeModeAssets/Music/battle-theme-194713.mp3", 0.3f);
        public static Songname MotherOfThePoolTheme = ModManager.RegisterNewMusic("RoguelikeModeAssets/Music/MotherOfThePool.mp3", 0.3f);
        public static Songname BossMusic = ModManager.RegisterNewMusic("RoguelikeModeAssets/Music/music_epic_orchestral_bg_underscore.wav", 0.3f);
        public static Songname VikingMusic = ModManager.RegisterNewMusic("RoguelikeModeAssets/Music/skjaldmr-norse-viking-background-music-110364.mp3", 0.3f);
        public static Songname OceanTheme = ModManager.RegisterNewMusic("RoguelikeModeAssets/Music/blackout-hybrid-cinematic-action-battle-electronic-268916.mp3", 0.3f);

        // SE
        public static SfxName EggHatch = ModManager.RegisterNewSoundEffect("RoguelikeModeAssets/Sfx/SPLAT Squelch 02.ogg");
        public static SfxName WandOverload = ModManager.RegisterNewSoundEffect("RoguelikeModeAssets/Sfx/DESTRUCTION Break Impact Wood 06.ogg");
        public static SfxName BebilithHiss = ModManager.RegisterNewSoundEffect("RoguelikeModeAssets/Sfx/ghost_witch_voice_hiss_11.wav");
        public static SfxName VomitSwarm = ModManager.RegisterNewSoundEffect("RoguelikeModeAssets/Sfx/VomitSwarm.wav");
    }
}
