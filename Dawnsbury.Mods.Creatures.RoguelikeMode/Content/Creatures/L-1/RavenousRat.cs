﻿using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Monsters.L_1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class RavenousRat {
        public static Creature Create() {
            Creature monster = GiantRat.CreateGiantRat();
            monster.MainName = "Ravenous Rat";
            monster.Traits.Add(Trait.NonSummonable);
            monster.Traits.Add(ModTraits.MeleeMutator);
            monster.AddQEffect(QEffect.PackAttack("ravenous rat", "1d4"));
            monster.WithTactics(Tactic.PackAttack);
            monster.CreatureId = CreatureIds.RavenousRat;
            return monster;
        }
    }
}