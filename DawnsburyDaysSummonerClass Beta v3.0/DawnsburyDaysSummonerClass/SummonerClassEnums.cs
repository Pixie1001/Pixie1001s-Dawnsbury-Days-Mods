using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawnsbury.Mods.Classes.Summoner {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class SummonerClassEnums {
        public enum EffectKind {
            HARM,
            HEAL,
            HEAL_HARM,
            HARM_HEAL,
            NONE
        }

        public enum InterceptKind {
            TARGET,
            DAMAGE
        }
    }
}
