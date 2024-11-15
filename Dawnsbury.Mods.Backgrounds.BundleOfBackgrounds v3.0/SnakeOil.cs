using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Enumerations;
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
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using System.Reflection.Metadata;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Microsoft.Xna.Framework;
using System.Text;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Specific;
using System.Runtime.CompilerServices;

namespace Dawnsbury.Mods.Backgrounds.BundleOfBackgrounds {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class SnakeOil {

        public static CombatAction CreateSnakeOilAction(Creature self, Proficiency proficiency) {
            int flatDC = proficiency == Proficiency.Trained ? 15 : proficiency == Proficiency.Expert ? 20 : 30;
            string str = proficiency == Proficiency.Trained ? "" : proficiency == Proficiency.Expert ? "+3" : "+13";
            Creature owner = self;
            Illustration illustration = (Illustration)IllustrationName.PotionOfSwimming;
            string name = "Snake Oil" + (proficiency == Proficiency.Trained ? "" : proficiency == Proficiency.Expert ? " (DC 20)" : " (DC 30)");
            Trait[] traits = new Trait[]
            {
                Trait.Manipulate,
                Trait.Basic
            };
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(100, 1);
            interpolatedStringHandler.AppendLiteral("{b}Range{/b} touch\n{b}Requirements{/b}You must have a hand free.\n\nMake a Deception check against DC ");
            interpolatedStringHandler.AppendFormatted<int>(flatDC);
            interpolatedStringHandler.AppendLiteral(".\n\n{b}Success{/b} The target gains 1d6");
            if (proficiency != Proficiency.Trained) {
                interpolatedStringHandler.AppendLiteral("+");
                interpolatedStringHandler.AppendFormatted<int>(proficiency == Proficiency.Master ? 13 : 3);
            }
            interpolatedStringHandler.AppendLiteral(" temporary hit points.");
            string description = interpolatedStringHandler.ToStringAndClear() + "" + "\n\nRegardless of your result, the target is then temporarily immune to your Snake Oil for the rest of the day.";
            CreatureTarget creatureTarget = Target.AdjacentFriend().WithAdditionalConditionOnTargetCreature((Func<Creature, Creature, Usability>)((a, d) => {
                if (!a.HasFreeHand)
                    return Usability.CommonReasons.NoFreeHandForManeuver;
                if (d.Damage == 0)
                    return Usability.NotUsableOnThisCreature("healthy");
                return d.PersistentUsedUpResources.UsedUpActions.Contains("SnakeOilFrom:" + self.Name) ? Usability.NotUsableOnThisCreature("immune") : Usability.Usable;
            }));

            return new CombatAction(owner, illustration, name, traits, description, (Target)creatureTarget).WithActionCost(1).WithSoundEffect(SfxName.DrinkPotion)
                .WithActiveRollSpecification(new ActiveRollSpecification(Checks.SkillCheck(Skill.Deception), Checks.FlatDC(flatDC)))
                .WithEffectOnEachTarget((Delegates.EffectOnEachTarget)(async (spell, caster, target, result) => {
                if (result >= CheckResult.Success) {
                    DiceFormula diceFormula = DiceFormula.FromText("1d6", "Snake Oil");
                    if (proficiency == Proficiency.Expert) {
                        diceFormula = diceFormula.Add(DiceFormula.FromText("3", "Snake Oil (expert)"));
                    } else if (proficiency == Proficiency.Master) {
                        diceFormula = diceFormula.Add(DiceFormula.FromText("13", "Snake Oil (master)"));
                    }
                    target.GainTemporaryHP(diceFormula.RollResult());
                    Sfxs.Play(SfxName.Healing);
                }
                target.PersistentUsedUpResources.UsedUpActions.Add("SnakeOilFrom:" + caster.Name);
            }));
        }

    }
}
