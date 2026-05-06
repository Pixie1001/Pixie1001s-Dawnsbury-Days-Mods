using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Monsters.L_1;
using Dawnsbury.Core.StatBlocks.Monsters.L1;
using Dawnsbury.Core.StatBlocks.Monsters.L2;
using Dawnsbury.Core.StatBlocks.Monsters.L3;
using Dawnsbury.Core.StatBlocks.Monsters.L4;
using Dawnsbury.Core.StatBlocks.Monsters.L5;
using Dawnsbury.Core.StatBlocks.Monsters.L6;
using Dawnsbury.Core.StatBlocks.Monsters.L7;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act2 {

    internal class DefendTheReliquary : NormalEncounter {

        private List<Creature[]> waves = new List<Creature[]>();
        private TileQEffect? waveMarker;
        private bool finalWave = false;

        public DefendTheReliquary(string filename) : base("Defend the Reliquary", filename) {

            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                await OpeningCutscene(battle);
            });

            this.ReplaceTriggerWithCinematic(TriggerName.InitiativeCountZero, async battle => {
                await SummonWaves(battle);
            });

            this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {if (CheckWinCondtion(battle)) {
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
                        pm.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Angelic Companion")!);

                    await CommonEncounterFuncs.StandardEncounterResolve(battle);
                }
            });
        }

        public bool CheckWinCondtion(TBattle battle) {
            return finalWave;
        }

        public async Task OpeningCutscene(TBattle battle) {
            if (battle.Encounter.CharacterLevel < 4) {
                // Low level waves
                waves.Add([DrowShootist.Create(), DrowFighter.Create(), DrowShootist.Create()]);
                waves.Add([DrowInquisitrix.Create(), HuntingSpider.Create()]);
                waves.Add([Skeleton.CreateSkeleton(), SkeletalChampion.CreateSkeletalChampion()]);
                waves.Add([KoboldWarrior.CreateKoboldWarrior(), KoboldWarrior.CreateKoboldWarrior(), KoboldWarrior.CreateKoboldWarrior(), KoboldWarrior.CreateKoboldWarrior()]);
                waves.Add([DrowArcanist.Create(), DrowFighter.Create()]);
                waves.Add([DrowNecromancer.Create(), CrawlingHand.Create()]);
                waves.Add([DemonPest.CreateDemonPest()]);
            } else {
                // High level waves
                waves.Add([DrowHuntress.Create(), DrowBlademaster.Create(), DrowHuntress.Create()]);
                waves.Add([NightmareWeaver.Create(), BebilithMinor.Create()]);
                waves.Add([SkeletalMage.Create(), MummyGuardian.Create()]);
                waves.Add([DrowInquisitrix.Create(), DrowPriestess.Create(), DrowInquisitrix.Create(), DrowTempleGuard.Create()]);
                waves.Add([NightmareWeaver.Create(), WebWarden.Create()]);
                waves.Add([EchidnaditeBroodguard.Create(), WinterWolf.Create()]);
                waves.Add([EchidnaditeMonsterBound.Create(), EchidnaditeMonsterBound.Create()]);
                waves.Add([EchidnaditePriestess.Create(), EchidnaditeWombCultist.Create()]);
                waves.Add([Succubus.Create()]);
            }

            var baraquielle = battle.AllCreatures.Find(cr => cr.CreatureId == CreatureIds.Baraquielle) ?? Creature.CreateSimpleCreature("Baraquielle").WithLargeIllustration(IllustrationName.BaraquielleLarge);
            baraquielle.Subtitle = "Archangel of retribution";

            battle.Cinematics.EnterCutscene();
            await battle.Cinematics.LineAsync(baraquielle, "Ready yourselves adventurers. Just as I suspected, my senses detect a great many creatures of an evil alignment heading towards the reliquary as we speak.");
            if (battle.Encounter.CharacterLevel <= 4)
                await battle.Cinematics.LineAsync(baraquielle, "They are of paltry power before the full mgiht of heaven, but alas, I cannot wield my full power lest the Starborn send forth a legion of greater demons.");
            await battle.Cinematics.LineAsync(baraquielle, "I will aid you in this fight as best I can, but you must be prepared to lay down your lives to protect the reliquary.");
            await battle.Cinematics.LineAsync(baraquielle, "Without it, the terms between the mortal realms and the other side shall be rendered null and void, and the Demon Queen of Spiders shall have free reign to renegotiate the terms as she pleases.");
            await battle.Cinematics.LineAsync(baraquielle, "That {i}cannot{/i} be allowed to happen.");
            battle.Cinematics.ExitCutscene();

            await TelegraphWave(battle);
        }

        // TODO: Make waves telegraphed
        // * If wave number-1, create a spawn marker and shift camera to centre on it. Then play dialogue, before shifting back.
        // * If wave is a spawn round, don't play dialogue (or play an alternate msg) but otherwise proceed with default behaviour.

        public async Task TelegraphWave(TBattle battle) {
            Tile tile = UtilityFunctions.ChooseAtRandom(new Tile[] { battle.Map.GetTile(11, 0)!, battle.Map.GetTile(22, 9)!, battle.Map.GetTile(11, 18)!, battle.Map.GetTile(0, 9)! })!;
            waveMarker = new TileQEffect(tile) {
                Illustration = Illustrations.BossEncounter,
                Name = "Incoming Wave Marker",
                VisibleDescription = "{b}Incoming Wave Marker.{/b} The servants of the starborn can be heard approaching from this ingress. They will arrive next round."
            };
            tile.AddQEffect(waveMarker);

            var baraquielle = battle.AllCreatures.Find(cr => cr.CreatureId == CreatureIds.Baraquielle) ?? Creature.CreateSimpleCreature("Baraquielle").WithLargeIllustration(IllustrationName.BaraquielleLarge);

            battle.Cinematics.EnterCutscene();
            battle.SmartCenterTileAlways(tile);
            if (battle.RoundNumber == 0)
                await battle.Cinematics.LineAsync(baraquielle, "Now prepare yourselves! I sense they will attack from this direction first.", null, true);
            else
                await battle.Cinematics.LineAsync(baraquielle, "I sense a strong evil presense approaching from yonder direction!", null, true);
            battle.Cinematics.ExitCutscene();
        }

        public async Task SummonWaves(TBattle battle) {
            int[] round = [1, 2, 4, 5];

            if (round.Contains(battle.RoundNumber) && waveMarker != null) {
                var level = battle.Encounter.CharacterLevel;
                Creature[] chosenWave = UtilityFunctions.ChooseAtRandom(waves);
                waves.Remove(chosenWave!);

                if (level == 1 || level == 5) {
                    chosenWave!.ForEach(cr => CommonEncounterFuncs.WeakenCreature(cr));
                } else if (level == 3 || level >= 7) {
                    chosenWave!.ForEach(cr => CommonEncounterFuncs.StrengthenCreature(cr));
                }

                chosenWave!.ForEach(async cr => {
                    battle.SpawnCreature(cr, battle.Enemy, waveMarker.Owner);
                    await UtilityFunctions.RunCombatStartSetup(cr);
                });

                waveMarker.Owner.RemoveAllQEffects(tqf => tqf == waveMarker);
                waveMarker = null;

                var baraquielle = battle.AllCreatures.Find(cr => cr.CreatureId == CreatureIds.Baraquielle) ?? Creature.CreateSimpleCreature("Baraquielle").WithLargeIllustration(IllustrationName.BaraquielleLarge);

                battle.Cinematics.EnterCutscene();
                battle.SmartCenterCreatureAlways(chosenWave![0]);
                if (battle.RoundNumber == round[0]) {
                    await battle.Cinematics.LineAsync(baraquielle, "The minions of evil arrive at last. To arms!", null, true);
                    await battle.Cinematics.LineAsync(baraquielle, "Put them to the sword at once so that I might complete the ritual unmolested!", null, true);
                } else if (battle.RoundNumber == round[1])
                    await battle.Cinematics.LineAsync(baraquielle, "More wretched evil-doers approach!", null, true);
                else if (battle.RoundNumber == round[2])
                    await battle.Cinematics.LineAsync(baraquielle, "Another group approaches! Vanquish them in the name of Heaven!", null, true);
                else if (battle.RoundNumber == round[3]) {
                    await battle.Cinematics.LineAsync(baraquielle, "The befouled screeching of my alignment sense is at last beginning to quiet. I believe this group is the last of them.", null, true);
                    finalWave = true;
                } 
                battle.Cinematics.ExitCutscene();
            }

            // Telegraph wave
            if (round.Contains(battle.RoundNumber + 1)) {
                await TelegraphWave(battle);
            }

        }

    }
}
