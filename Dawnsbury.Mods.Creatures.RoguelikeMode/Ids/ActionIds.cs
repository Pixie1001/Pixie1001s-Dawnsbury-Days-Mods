using System;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Modding;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Ids {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class ActionIds {
        public static ActionId Parry { get; private set; }

        public static void RegisterConflictedIds() {
            Parry = ModManager.TryParse("Parry", out ActionId parryAct) ? parryAct : ModManager.RegisterEnumMember<ActionId>("Parry");
        }
    }
}
