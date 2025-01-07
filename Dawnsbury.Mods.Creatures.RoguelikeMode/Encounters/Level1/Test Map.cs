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
using Dawnsbury.ThirdParty.SteamApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Collections.Specialized.BitVector32;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Intrinsics.Arm;
using System.Xml;
using Dawnsbury.Core.Mechanics.Damage;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using static System.Reflection.Metadata.BlobBuilder;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Core.Animations.Movement;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Campaign.Encounters.Quest_for_the_Golden_Candelabra;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class TestMap : Encounter
    {

        public TestMap(string filename) : base("Test Map", filename, null, 0)
        {
            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                Sfxs.SlideIntoSong(SoundEffects.BossMusic);

                CommonEncounterFuncs.ApplyEliteAdjustments(battle);

                Creature td = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy);

                // TODO: Check why effect isn't removed

                Creature pm = battle.AllCreatures.FirstOrDefault(cr => cr.PersistentCharacterSheet != null);
                //pm.AddQEffect(new QEffect("Drow Renegade Companion", "You've acquired the aid of a Drow Renegade. She will fight besides you until dying or the party returns to town.") {
                //    StartOfCombat = async self => {
                //        Creature companion = CreatureList.Creatures[ModEnums.CreatureId.DROW_RENEGADE](self.Owner.Battle.Encounter);
                //        self.Owner.Battle.SpawnCreature(companion, Faction.CreateFriends(self.Owner.Battle), self.Owner.Occupies);
                //        companion.AddQEffect(new QEffect() {
                //            Source = self.Owner,
                //            WhenMonsterDies = qfDeathCheck => {
                //                self.ExpiresAt = ExpirationCondition.Immediately;
                //            }
                //        });
                //        //self.Tag = companion;
                //    },

                //});

                //pm.AddQEffect(QEffect.Drained(2));
                //pm.AddQEffect(new QEffect("Injured", "You've sustained an injury that won't quite fully heal until you've had a full night's rest reducing your max HP by 10% per value.") {
                //    Value = 3,
                //    StateCheck = self => {
                //        self.Owner.DrainedMaxHPDecrease += (int)(0.1f * self.Value * self.Owner.TrueMaximumHP);
                //    }
                //});

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
            //    Creature td = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy);


            //    // Debug
            //    battle.AllCreatures.FirstOrDefault(cr => cr.BaseName.ToLower() == "test summoner's eidolon").AddQEffect(new QEffect() { BonusToDefenses = (self, action, defence) => defence == Defense.Fortitude ? new Bonus(-5, BonusType.Status, "Testing") : null });
            //    //await Affliction.ExposeToInjury(Affliction.CreateSpiderVenom(), td, battle.AllCreatures.FirstOrDefault(cr => cr.BaseName.ToLower() == "test summoner's eidolon"));
            //});
        }

    }
}
