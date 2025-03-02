using System;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Auxiliary;
using Dawnsbury.Audio;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Text;
using Microsoft.Xna.Framework;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level4
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class SuccubusCult : Level4Encounter {


        public SuccubusCult(string filename) : base("Succubus Cult", filename) {
            this.CharacterLevel = 4;
            this.RewardGold = CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.NORMAL);
            if (Rewards.Count == 0) {
                CommonEncounterFuncs.SetItemRewards(Rewards, 4, EncounterType.NORMAL);
            }

            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                await CommonEncounterFuncs.StandardEncounterSetup(battle);
                Creature succubus = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Succubus");
                succubus.Spellcasting.PrimarySpellcastingSource.Spells.RemoveFirst(spell => spell.SpellId == SpellId.Dominate);
                //ca.SpellcastingSource = succubus.Spellcasting.PrimarySpellcastingSource;
                succubus.Spellcasting.PrimarySpellcastingSource.WithSpells([SpellLoader.LesserDominate], 5);
                succubus.Spellcasting.PrimarySpellcastingSource.WithSpells([SpellLoader.LesserDominate], 5);
            });

            // Run cleanup
            this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {
                await CommonEncounterFuncs.StandardEncounterResolve(battle);
            });
        }

    }
}
