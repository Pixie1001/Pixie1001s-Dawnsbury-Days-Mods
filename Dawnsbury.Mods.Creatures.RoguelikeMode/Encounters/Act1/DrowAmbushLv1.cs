using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act1
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class DrowAmbushLv1 : NormalEncounter {
        public DrowAmbushLv1(string filename) : base("Drow Ambush", filename) { }
    }
}
