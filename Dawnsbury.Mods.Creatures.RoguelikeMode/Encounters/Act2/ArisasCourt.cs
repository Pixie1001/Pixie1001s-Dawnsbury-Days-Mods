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

    internal class ArisasCourt : EliteEncounter {
        public ArisasCourt(string filename) : base("Arisa's Court", filename, eliteRewards: new List<(Item, string)?> {
            (Items.CreateNew(CustomItems.RunestoneOfPandemomium), "..."),
            (Items.CreateNew(CustomItems.SceptreOfPandemonium), "...")
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
