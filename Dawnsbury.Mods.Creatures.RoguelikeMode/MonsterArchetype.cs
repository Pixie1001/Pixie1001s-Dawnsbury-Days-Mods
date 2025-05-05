using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.AbilityScores;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Description;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class MonsterArchetype {
        private string name;
        public Trait[] AppliesTo { get; }
        private Action<Creature> adjustment;

        public MonsterArchetype(string name, Trait[] appliesTo, Action<Creature> adjustment) {
            this.name = name;
            this.adjustment = adjustment;
            this.AppliesTo = appliesTo;
        }

        public bool CheckIfValid(Creature creature) {
            return AppliesTo.Contains(ModTraits.UniversalMutator) || creature.Traits.ContainsOneOf(AppliesTo);
        }

        public void Apply(Creature creature) {
            adjustment(creature);
            creature.MainName = this.name + " " + creature.MainName;
            creature.Illustration = new SameSizeDualIllustration(creature.Illustration, Illustrations.MutatedBorder);
        }
    }




}
