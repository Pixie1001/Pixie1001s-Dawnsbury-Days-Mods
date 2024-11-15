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
using static Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.BarbarianFeatsDb.AnimalInstinctFeat;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Metrics;
using Microsoft.Xna.Framework.Audio;
using static System.Reflection.Metadata.BlobBuilder;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Core.Animations.Movement;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;
using Dawnsbury.Campaign.Encounters.Quest_for_the_Golden_Candelabra;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class HallOfSmokeLv3 : Level3EliteEncounter
    {
        public HallOfSmokeLv3(string filename) : base("Hall of Smoke", filename) { }

        //public HallOfSmokeLv3(string filename) : base("Hall of Smoke", filename, new List<Item>() { }, 0)
        //{
        //    this.CharacterLevel = 3;
        //    this.RewardGold = CommonEncounterFuncs.GetGoldReward(this.CharacterLevel, ModEnums.EncounterType.ELITE);
        //    // Run setup
        //    ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle =>
        //    {

        //        Faction faction = Faction.CreateEnemy(battle);
        //        List<Tile> spawnPoints = battle.Encounter.Map.AllTiles.Where(t =>
        //        {
        //            if (!t.IsFree)
        //            {
        //                return false;
        //            }

        //            foreach (Creature pc in battle.AllCreatures.Where(cr => cr.OwningFaction.IsHumanControlled))
        //            {
        //                if (pc.DistanceTo(t) < 4)
        //                {
        //                    return false;
        //                }
        //            }
        //            return true;
        //        }).ToList();
        //        List<Creature> enemyList = new List<Creature>() {
        //            CreatureList.Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN](battle.Encounter),
        //            CreatureList.Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN](battle.Encounter),
        //            CreatureList.Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN](battle.Encounter),
        //            CreatureList.Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN](battle.Encounter),
        //            CreatureList.Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN](battle.Encounter)
        //        };
        //        foreach (Creature enemy in enemyList)
        //        {
        //            Tile spawn = spawnPoints[R.Next(0, spawnPoints.Count)];
        //            spawnPoints.Remove(spawn);
        //            battle.SpawnCreature(enemy, faction, spawn.X, spawn.Y);
        //        }

        //        foreach (Creature enemy in enemyList)
        //        {
        //            enemy.DetectionStatus.Undetected = true;
        //            foreach (Creature opponent in enemy.Battle.AllCreatures.Where(cr => cr.OwningFaction != enemy.OwningFaction)) {
        //                enemy.DetectionStatus.HiddenTo.Add(opponent);
        //            }
        //            //CombatAction hide = CreatureList.CommonMonsterActions.CreateHide(enemy);
        //            //hide.ChosenTargets = new ChosenTargets() { ChosenCreature = enemy };
        //            //await hide.AllExecute();

        //            //enemy.AddQEffect(new QEffect()
        //            //{
        //            //    Id = QEffectId.Slowed,
        //            //    Value = 2,
        //            //    PreventTakingAction = action => action.HasTrait(Trait.Move) ? null : "Can only move.",
        //            //});
        //            //await enemy.Battle.GameLoop.Turn(enemy, false);
        //            //enemy.RemoveAllQEffects(qf => qf.Id == QEffectId.Slowed);
        //        }

        //        CommonEncounterFuncs.ApplyEliteAdjustments(battle);
        //    });

        //    // Run cleanup
        //    //this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {

        //    //});
        //}

    }
}