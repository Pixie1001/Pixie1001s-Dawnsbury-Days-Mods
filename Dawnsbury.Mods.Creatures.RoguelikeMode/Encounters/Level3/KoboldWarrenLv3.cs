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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class KoboldWarrenLv3 : Level3Encounter
    {
        public KoboldWarrenLv3(string filename) : base("Kobold Warren", filename) { }
    }
}
