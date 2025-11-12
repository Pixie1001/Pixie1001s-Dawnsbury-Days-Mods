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
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3;
using Dawnsbury.Core.StatBlocks.Monsters.L1;
using Dawnsbury.Core.StatBlocks.Monsters.L2;
using Dawnsbury.Core.StatBlocks.Monsters.L_1;
using Dawnsbury.Core.StatBlocks.Monsters.L3;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Dawnsbury.Core.StatBlocks.Monsters.L4;
using Dawnsbury.Core.StatBlocks.Monsters.L5;
using Dawnsbury.Core.StatBlocks.Monsters.L6;
using Dawnsbury.Core.StatBlocks.Monsters.L7;
using Dawnsbury.Campaign.LongTerm;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act2 {

    internal class DefendTheReliquary : NormalEncounter {
        public DefendTheReliquary(string filename) : base("Defend the Reliquary", filename) {

            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                await OpeningCutscene(battle);
            });

            this.ReplaceTriggerWithCinematic(TriggerName.InitiativeCountZero, async battle => {
                await SummonWaves(battle);
            });

            this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {
                if (CheckWinCondtion(battle)) {
                    var baraquielle = battle.AllCreatures.Find(cr => cr.CreatureId == CreatureIds.Baraquielle) ?? Creature.CreateSimpleCreature("Baraquielle").WithLargeIllustration(IllustrationName.BaraquielleLarge);
                    baraquielle.Subtitle = "Archangel of retribution";

                    battle.Cinematics.EnterCutscene();

                    if (!baraquielle.Alive) {
                        battle.RemoveCreatureFromGame(baraquielle);
                        battle.SpawnCreature(baraquielle, battle.GaiaFriends, battle.Map.GetTile(11, 8)!);
                        Sfxs.Play(SfxName.PhaseBolt);
                        baraquielle.AnimationData.ColorBlinkFast(Color.Yellow);
                    }

                    battle.SmartCenterCreatureAlways(baraquielle);

                    await battle.Cinematics.LineAsync(baraquielle, "Thank you adventurers. Your performance was...");
                    await battle.Cinematics.LineAsync(baraquielle, "{i}(looks away){/i} Satisfactory.");
                    await battle.Cinematics.LineAsync(baraquielle, "Please vacate the area whilst I complete the ritual. I trust you will speak of it to no one.");
                    await battle.Cinematics.LineAsync(baraquielle, "However as you have aided Heaven in this most trying of challenges, so too shall Heaven render her aid upon you.");
                    await battle.Cinematics.LineAsync(baraquielle, "Whatever foul cretin awaits you at the end of your expedition, I shall face it beside you.");
                    battle.Cinematics.ExitCutscene();

                    var pm = battle.AllCreatures.FirstOrDefault(cr => cr.PersistentCharacterSheet?.IsCampaignCharacter ?? false);
                    if (pm != null)
                        pm.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Angel Companion")!);

                    await CommonEncounterFuncs.StandardEncounterResolve(battle);
                }
            });
        }

        public static bool CheckWinCondtion(TBattle battle) {
            return battle.RoundNumber >= 5;
        }

        public async static Task OpeningCutscene(TBattle battle) {
            var baraquielle = battle.AllCreatures.Find(cr => cr.CreatureId == CreatureIds.Baraquielle) ?? Creature.CreateSimpleCreature("Baraquielle").WithLargeIllustration(IllustrationName.BaraquielleLarge);
            baraquielle.Subtitle = "Archangel of retribution";

            battle.Cinematics.EnterCutscene();
            await battle.Cinematics.LineAsync(baraquielle, "Ready yourselves adventurers. Just as I suspected, my senses detect a great many creatures of an evil alignment heading towards the reliquary as we speak.");
            if (battle.Encounter.CharacterLevel <= 4)
                await battle.Cinematics.LineAsync(baraquielle, "They are of paltry power before the full mgiht of heaven, but alas, I cannot wield my full power lest the Starborn send forth a legion of greater demons.");
            await battle.Cinematics.LineAsync(baraquielle, "I will aid you in this fight as best I can, but you must be prepared to lay down your lives to protect the reliquary.");
            await battle.Cinematics.LineAsync(baraquielle, "Without it, the terms between the mortal realms and the other side shall be rendered null and void, and the Demon Queen of Spiders shall have free reign to renegotiate the terms as she please.");
            await battle.Cinematics.LineAsync(baraquielle, "That cannot be allowed to happen.");
            battle.Cinematics.ExitCutscene();
        }

        public async static Task SummonWaves(TBattle battle) {
            int[] round = [1, 2, 4, 5];

            if (!round.Contains(battle.RoundNumber)) return;

            Tile[] spawnPoints = [battle.Map.GetTile(11, 0)!, battle.Map.GetTile(22, 9)!, battle.Map.GetTile(11, 18)!, battle.Map.GetTile(0, 9)!];

            var lowLvlWaves = new List<Creature[]>();

            // Low level waves
            lowLvlWaves.Add([DrowShootist.Create(), DrowFighter.Create(), DrowShootist.Create()]);
            lowLvlWaves.Add([DrowInquisitrix.Create(), HuntingSpider.Create()]);
            lowLvlWaves.Add([Skeleton.CreateSkeleton(), SkeletalChampion.CreateSkeletalChampion()]);
            lowLvlWaves.Add([KoboldWarrior.CreateKoboldWarrior(), KoboldWarrior.CreateKoboldWarrior(), KoboldWarrior.CreateKoboldWarrior(), KoboldWarrior.CreateKoboldWarrior()]);
            lowLvlWaves.Add([DrowArcanist.Create(), DrowFighter.Create()]);
            lowLvlWaves.Add([DrowNecromancer.Create(), CrawlingHand.Create()]);
            lowLvlWaves.Add([DemonPest.CreateDemonPest()]);

            var highLvlWaves = new List<Creature[]>();

            // High level waves
            highLvlWaves.Add([DrowHuntress.Create(), DrowBlademaster.Create(), DrowHuntress.Create()]);
            highLvlWaves.Add([NightmareWeaver.Create(), BebilithMinor.Create()]);
            highLvlWaves.Add([SkeletalMage.Create(), MummyGuardian.Create()]);
            highLvlWaves.Add([DrowInquisitrix.Create(), DrowPriestess.Create(), DrowInquisitrix.Create(), DrowTempleGuard.Create()]);
            highLvlWaves.Add([NightmareWeaver.Create(), WebWarden.Create()]);
            highLvlWaves.Add([EchidnaditeBroodguard.Create(), WinterWolf.Create()]);
            highLvlWaves.Add([EchidnaditeMonsterBound.Create(), EchidnaditeMonsterBound.Create()]);
            highLvlWaves.Add([EchidnaditePriestess.Create(), EchidnaditeWombCultist.Create()]);
            highLvlWaves.Add([Succubus.Create()]);

            var level = battle.Encounter.CharacterLevel;
            Creature[] chosenWave = level <= 4 ? UtilityFunctions.ChooseAtRandom(lowLvlWaves) : UtilityFunctions.ChooseAtRandom(highLvlWaves);

            if (level == 1 || level == 5) {
                chosenWave!.ForEach(cr => CommonEncounterFuncs.WeakenCreature(cr));
            } else if (level == 6 || level == 5) {
                chosenWave!.ForEach(cr => CommonEncounterFuncs.StrengthenCreature(cr));
            }

            var tile = UtilityFunctions.ChooseAtRandom(spawnPoints)!;

            chosenWave!.ForEach(cr => battle.SpawnCreature(cr, battle.Enemy, tile));

            battle.SmartCenterCreatureAlways(chosenWave![0]); ;

            var baraquielle = battle.AllCreatures.Find(cr => cr.CreatureId == CreatureIds.Baraquielle) ?? Creature.CreateSimpleCreature("Baraquielle").WithLargeIllustration(IllustrationName.BaraquielleLarge);

            battle.Cinematics.EnterCutscene();
            if (battle.RoundNumber == round[0])
                await battle.Cinematics.LineAsync(baraquielle, "The minions of evil arrive at last. To arms!");
            else if (battle.RoundNumber == round[1])
                await battle.Cinematics.LineAsync(baraquielle, "More wretched evil-doers approach!");
            else if (battle.RoundNumber == round[2])
                await battle.Cinematics.LineAsync(baraquielle, "Another group approaches! Vanquish them in the name of Heaven!");
            else if (battle.RoundNumber == round[3])
                await battle.Cinematics.LineAsync(baraquielle, "The befouled screeching of my alignment sense is at last beginning to quiet. I believe this group is the last of them.");
            await battle.Cinematics.LineAsync(baraquielle, "Put them to the sword at once so that I might complete the ritual unmolested!");
            battle.Cinematics.ExitCutscene();
        }  

    }

    //internal class DemonGateLv5 : Level5Encounter {
    //    public DemonGateLv5(string filename) : base("Demon Gate", filename) {

    //        this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
    //            await DemonGateLv6.ExplainPortal(battle);
    //        });

    //        this.ReplaceTriggerWithCinematic(TriggerName.InitiativeCountZero, async battle => {
    //            await DemonGateLv6.HandlePortal(battle);
    //        });

    //        this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {
    //            if (battle.RoundNumber >= 7)
    //                await CommonEncounterFuncs.StandardEncounterResolve(battle);
    //        });
    //    }
    //}

    //internal class DemonGateLv7 : Level7Encounter {
    //    public DemonGateLv7(string filename) : base("Demon Gate", filename) {

    //        this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
    //            await DemonGateLv6.ExplainPortal(battle);
    //        });

    //        this.ReplaceTriggerWithCinematic(TriggerName.InitiativeCountZero, async battle => {
    //            await DemonGateLv6.HandlePortal(battle);
    //        });

    //        this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {
    //            if (battle.RoundNumber >= 7)
    //                await CommonEncounterFuncs.StandardEncounterResolve(battle);
    //        });
    //    }
    //}
    
}
