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
using Dawnsbury.Core.Animations.Movement;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class ScriptHooks {

        internal static void LoadHooks() {
            ModManager.RegisterCodeHook("Hall of Smoke", async battle => {
                Faction faction = Faction.CreateEnemy(battle);
                List<Tile> spawnPoints = battle.Encounter.Map.AllTiles.Where(t => {
                    if (!t.IsFree) {
                        return false;
                    }

                    foreach (Creature pc in battle.AllCreatures.Where(cr => cr.OwningFaction.IsHumanControlled)) {
                        if (pc.DistanceTo(t) < 4) {
                            return false;
                        }
                    }
                    return true;
                }).ToList();
                List<Creature> enemyList = new List<Creature>() {
                    CreatureList.Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN](battle.Encounter),
                    CreatureList.Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN](battle.Encounter),
                    CreatureList.Creatures[ModEnums.CreatureId.UNSEEN_GUARDIAN](battle.Encounter)
                };
                foreach (Creature enemy in enemyList) {
                    Tile spawn = spawnPoints[R.Next(0, spawnPoints.Count)];
                    spawnPoints.Remove(spawn);
                    battle.SpawnCreature(enemy, faction, spawn.X, spawn.Y);
                }

                foreach (Creature enemy in enemyList) {
                    CombatAction hide = CreatureList.CommonMonsterActions.CreateHide(enemy);
                    hide.ChosenTargets = new ChosenTargets() { ChosenCreature = enemy };
                    await hide.AllExecute();

                    enemy.AddQEffect(new QEffect() {
                        Id = QEffectId.Slowed,
                        Value = 2,
                        PreventTakingAction = action => action.HasTrait(Trait.Move) ? null : "Can only move.",
                    });
                    await enemy.Battle.GameLoop.Turn(enemy, false);
                    enemy.RemoveAllQEffects(qf => qf.Id == QEffectId.Slowed);
                }
            });

            ModManager.RegisterCodeHook("Drow Ambush", async battle => {
                // Find player with lowest HP
                // Spawn assassin nearby with hidden
                // Have assassin pass turn/move around using 1 turn qeffect

                //List<Creature> party = battle.AllCreatures.Where(c => c.OwningFaction.IsHumanControlled).ToList();
                //Creature target = party.OrderBy(c => c.HP / 100 * (c.Defenses.GetBaseValue(Defense.AC) * 5)).ToList()[0];
                //Faction enemyFaction = battle.AllCreatures.FirstOrDefault(c => c.OwningFaction.IsEnemy).OwningFaction;

                //Tile? spawnPoint = battle.Encounter.Map.AllTiles.Where(t => t.IsFree && t.DistanceTo(target.Occupies) < 5 && t.DistanceTo(target.Occupies) > 3).ToList().GetRandom();

                //if (spawnPoint == null) {
                //    spawnPoint = target.Occupies;
                //}

                //Creature assassin = CreatureList.Creatures[ModEnums.CreatureId.DROW_ASSASSIN](battle.Encounter);
                //assassin.AddQEffect(new QEffect {
                //    Id = QEffectIds.Lurking,
                //    PreventTakingAction = action => action.ActionId != ActionId.Sneak ? "Stalking prey, cannot act." : null,
                //    EndOfAnyTurn = self => {
                //        if (!self.Owner.DetectionStatus.Undetected) {
                //            self.ExpiresAt = ExpirationCondition.Immediately;
                //        }
                //    },
                //    ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
                //    WhenExpires = self => {
                //        self.Owner.Occupies.Overhead("Lurking ended 1", Color.Black, "Lurking ended 2", "Lurking ended 3", "Lurking ended 4");
                //    }
                //});

                //foreach (Creature player in party) {
                //    assassin.DetectionStatus.HiddenTo.Add(player);
                //}
                //assassin.DetectionStatus.Undetected = true;
                //target.AddQEffect(CommonQEffects.Stalked(assassin));

                //battle.SpawnCreature(assassin, enemyFaction, spawnPoint.X, spawnPoint.Y);
            });

            ModManager.RegisterCodeHook("Hall of Beginnings", async battle => {

                //battle.SpawnCreature(CreatureList.Creatures[ModEnums.CreatureId.HUNTING_SPIDER](battle.Encounter), battle.Enemy, battle.Map.Tiles[5, 5]);

                //CampaignState? campaign = battle.CampaignState;

                //if (campaign != null) {
                //    GenerateRun(campaign);
                //}
            });

        }

        //private static void GenerateRun(CampaignState campaign) {
        //    //CampaignStop[]? oldPath = new CampaignStop[] { path[0], path[1], path[2] };

        //    //path = new List<CampaignStop>();
        //    //foreach (CampaignStop stop in oldPath) {

        //    //}

        //    // TODO: Fix up so that this method can be called without starting an encounter, and so that the path.xml file starts with all 9 encounters already there, to prevent a crash on load.
        //    // Maybe use Harmony to replace the CampaignMenuPhase.CreateViews() method, so that it shuffles the encounters if the campaign is called 'Roguelike Mode'.
        //    // Add extra dummu encounters to .xml, so that i can tell if the adventure path has been set up or not

        //    if (campaign.AdventurePath == null) {
        //        return;
        //    }

        //    List<CampaignStop> path = campaign.AdventurePath.CampaignStops;

        //    CampaignStop lastNode = path.Last();
        //    path.Remove(lastNode);

        //    path.Add(new MediumRestCampaignStop("Rest after first encounter."));
        //    path.Add(new TypedEncounterCampaignStop<HallOfSmoke>());
        //    path.Add(new MediumRestCampaignStop("Rest after second encounter."));
        //    path.Add(new TypedEncounterCampaignStop<DrowAmbush>());
        //    path.Add(new MediumRestCampaignStop("Rest after third encounter."));
        //    path.Add(lastNode);

        //    for (int i = 0; i < path.Count; ++i) {
        //        path[i].Index = i;
        //    }
        //    //campaign.CurrentStopIndex = path[2].Index;
        //    //campaign.CurrentStop = path[2];

        //    //campaign.CurrentStop = path[1];
        //    //int stop = campaign.CurrentStopIndex;
        //}
    }
}
