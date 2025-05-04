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
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Core.Animations.Movement;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Campaign.Encounters.Quest_for_the_Golden_Candelabra;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Campaign.Encounters.Evil_from_the_Stars;
using Dawnsbury.Campaign.LongTerm;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1 {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class TestMap : Encounter {

        public TestMap(string filename) : base("Test Map", filename, null, 0) {
            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                // CommonEncounterFuncs.ApplyEliteAdjustments(battle);

                Creature td = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy);
                Creature pm = battle.AllCreatures.FirstOrDefault(cr => cr.PersistentCharacterSheet != null);

                //battle.AllCreatures.FirstOrDefault(cr => cr.BaseName == "test summoner").AddQEffect(new QEffect() { BonusToDefenses = (self, action, defence) => defence == Defense.Fortitude ? new Bonus(-5, BonusType.Untyped, "Testing") : null });
                // Debug
                //await Affliction.ExposeToInjury(Affliction.CreateSpiderVenom(), td, battle.AllCreatures.FirstOrDefault(cr => cr.BaseName == "test summoner"));

                //CommonEncounterFuncs.ApplyEliteAdjustments(battle);

                //td.AddQEffect(new QEffect() {
                //    AdditionalGoodness = (self, action, target) => {
                //        if (action.HasTrait(Trait.Strike)) {
                //            return 7;
                //        }
                //        return 0;
                //    }
                //});
                //td.Traits.Add(Trait.Undead);
                //td.Level = 9;
            });

            //this.ReplaceTriggerWithCinematic(TriggerName.InitiativeCountZero, async battle => {
            //    //var eidolon = battle.AllCreatures.FirstOrDefault(cr => cr.Name == "test summoner's Eidolon");
            //    //eidolon.AddQEffect(QEffect.Quickened(a => true));

            //    battle.Cinematics.EnterCutscene();

            //    // await RecoverAllFriends(battle);
            //    List<Creature> party = battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null).ToList();

            //    //async Task BoostHero(Creature character) {
            //    //    character.AnimationData.ColorBlink(Color.Yellow);
            //    //    //cr.AddQEffect(WellKnownLongTermEffects.CreateQEffect(Settings.CurrentDifficulty >= Difficulty.Hard ? WellKnownLongTermEffects.CeruleanSkyBoon : WellKnownLongTermEffects.CeruleanSkyMajorBoon));
            //    //    await character.HealAsync("4d6+4", CombatAction.CreateSimple(character.Battle.Pseudocreature, "Boon of the Cerulean Sky"));
            //    //}

            //    //foreach (Creature hero in party) {
            //    //    await BoostHero(hero);
            //    //}

            //    foreach (Creature hero in party) {
            //        hero.Heal("1000", null);
            //    }

            //    //await battle.Cinematics.LineAsync(party[0], "Test line!");

            //    battle.Cinematics.ExitCutscene();

            //});

            //this.ReplaceTriggerWithCinematic(TriggerName.InitiativeCountZero, async battle => {
            //    Creature td = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy);


            //    // Debug
            //    battle.AllCreatures.FirstOrDefault(cr => cr.BaseName.ToLower() == "test summoner's eidolon").AddQEffect(new QEffect() { BonusToDefenses = (self, action, defence) => defence == Defense.Fortitude ? new Bonus(-5, BonusType.Status, "Testing") : null });
            //    //await Affliction.ExposeToInjury(Affliction.CreateSpiderVenom(), td, battle.AllCreatures.FirstOrDefault(cr => cr.BaseName.ToLower() == "test summoner's eidolon"));
            //});
        }

        //public override void ModifyCreatureSpawningIntoTheEncounter(Creature creature) {
        //    S4E2OnTheSeabed.AquaticCombatModify(creature);
        //}

    }
}
