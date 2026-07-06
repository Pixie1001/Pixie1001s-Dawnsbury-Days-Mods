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
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.StatBlocks;
using System.Collections.Generic;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act2 {

    internal class Medusa : EliteEncounter {
        public Medusa(string filename) : base("Medusa", filename, eliteRewards: new List<(Item, string)?> {
            (Items.CreateNew(CustomItems.MedusaEyeChoker), "The huntress' eye could be taken back to town and fashened in a powerful artefact, capable of turning her petrifying gaze onto your enemies."),
            (Items.CreateNew(CustomItems.SerpentineBow), "The huntress' living serpentile bow, whose arrows are just as lethal as any cobra's venomous bite.")
        }) {

            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                await base.Setup(battle);
                Sfxs.SlideIntoSong(Songname.HighTensionBegins);
            });

            //this.ReplaceTriggerWithCinematic(TriggerName.InitiativeCountZero, async battle => {

            //});

            //this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {

            //});
        }
    }
}
