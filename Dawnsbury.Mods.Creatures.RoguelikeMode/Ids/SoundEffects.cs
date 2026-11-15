using Dawnsbury.Audio;
using Dawnsbury.Modding;
using System;
using System.Reflection;

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

        // Voice Lines
        public static string skill1 = GetVoiceLocation("skill1.ogg");
        public static string skill2 = GetVoiceLocation("skill2.ogg");
        public static string skill3 = GetVoiceLocation("skill3.ogg");
        public static string skill4 = GetVoiceLocation("skill4.ogg");
        public static string skill5 = GetVoiceLocation("skill5.ogg");
        public static string skill6 = GetVoiceLocation("skill6.ogg");

        public static string tdstart1 = GetVoiceLocation("tdstart1-2.ogg");
        public static string tdstart2 = GetVoiceLocation("tdstart2.ogg");
        public static string tdstart3 = GetVoiceLocation("tdstart3.ogg");
        public static string tdstart4 = GetVoiceLocation("tdstart4.ogg");
        public static string tdstart5 = GetVoiceLocation("tdstart5.ogg");

        public static string tdlocation1 = GetVoiceLocation("tdlocation1.ogg");
        public static string tdlocation2 = GetVoiceLocation("tdlocation2.ogg");
        public static string tdlocation3 = GetVoiceLocation("tdlocation3.ogg");
        public static string tdlocation4 = GetVoiceLocation("tdlocation4.ogg");

        public static string tdspawn1 = GetVoiceLocation("tdspawn1.ogg");
        public static string tdspawn2 = GetVoiceLocation("tdspawn2.ogg");
        public static string tdspawn3 = GetVoiceLocation("tdspawn3.ogg");

        public static string tdpost1 = GetVoiceLocation("tdpost1.ogg");
        public static string tdpost2 = GetVoiceLocation("tdpost2.ogg");
        public static string tdpost3 = GetVoiceLocation("tdpost3.ogg");
        public static string tdpost4 = GetVoiceLocation("tdpost4.ogg");
        public static string tdpost5 = GetVoiceLocation("tdpost5.ogg");

        public static string attack1 = GetVoiceLocation("attack1.ogg");
        public static string attack2 = GetVoiceLocation("attack2.ogg");
        public static string attack3 = GetVoiceLocation("attack3.ogg");
        public static string attack4 = GetVoiceLocation("attack4.ogg");
        public static string attack5 = GetVoiceLocation("attack5.ogg");

        public static string heal1 = GetVoiceLocation("heal1.ogg");
        public static string heal2 = GetVoiceLocation("heal2.ogg");
        public static string heal3 = GetVoiceLocation("heal3.ogg");
        public static string heal4 = GetVoiceLocation("heal4.ogg");
        public static string heal5 = GetVoiceLocation("heal5.ogg");

        public static string intercept1 = GetVoiceLocation("intercept1.ogg");
        public static string intercept2 = GetVoiceLocation("intercept2.ogg");
        public static string intercept3 = GetVoiceLocation("intercept3-1.ogg");
        public static string intercept4 = GetVoiceLocation("intercept4.ogg");
        public static string intercept5 = GetVoiceLocation("intercept5.ogg");

        public static string nooo = GetVoiceLocation("nooooo.ogg");

        private static string GetVoiceLocation(string fileName) {
            string rootLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            rootLocation?.Substring(0, rootLocation.Length - "/DawnsburyDaysSummonerClass.dll".Length);
            string extraPath = "/RoguelikeModeAssets/VoiceLines/";
            return rootLocation + extraPath + fileName;
        }
    }
}
