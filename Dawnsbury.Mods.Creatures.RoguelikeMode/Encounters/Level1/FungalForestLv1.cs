﻿using System;
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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class FungalForestlv1 : Level1Encounter
    {
        public FungalForestlv1(string filename) : base("Fungal Forest", filename) { }
    }
}