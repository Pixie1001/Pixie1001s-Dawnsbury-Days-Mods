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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act2 {

    internal class DemonGate : NormalEncounter {
        public DemonGate(string filename) : base("Demon Gate", filename) {

            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                await base.Setup(battle);
                await ExplainPortal(battle);
            });

            this.ReplaceTriggerWithCinematic(TriggerName.InitiativeCountZero, async battle => {
                await HandlePortal(battle);
            });

            this.ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async battle => {
                if (battle.RoundNumber >= 7) {
                    await base.Cleanup(battle);
                }
            });
        }

        public async static Task ExplainPortal(TBattle battle) {
            var advisor1 = UtilityFunctions.ChooseAtRandom(battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null).ToArray());
            var advisor2 = UtilityFunctions.ChooseAtRandom(battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null && cr != advisor1).ToArray());
            var demon = battle.AllCreatures.FirstOrDefault(cr => cr.CreatureId == CreatureIds.NightmareWeaver);
            if (advisor1 == null || advisor2 == null || demon == null) return;
            advisor1.Battle.Cinematics.EnterCutscene();
            Sfxs.Play(SfxName.OminousActivation, 1);
            await advisor1.Battle.Cinematics.LineAsync(advisor1, "They've opened a rift to the Demon Queen of Spider's domain!");
            await advisor1.Battle.Cinematics.LineAsync(advisor1, "But it's still small... If we can clear anything that comes crawling out, we should have time to close the breach before it grows large enough for her generals to march through!");
            await advisor1.Battle.Cinematics.LineAsync(advisor2, "Yes. I'd say about three more pulses should be enough to exhaust it...");
            await advisor1.Battle.Cinematics.LineAsync(demon, "Impudent mortals... You cannot stop the inveitable. I look forward to sampling the taste of your fear when we drag you before our Queen!");
            advisor1.Battle.Cinematics.ExitCutscene();
        }

        public async static Task HandlePortal(TBattle battle) {
            var level = battle.Encounter.CharacterLevel;

            int[] rounds = [1, 3, 5];
            if (!rounds.Contains(battle.RoundNumber)) return;

            var list = MonsterStatBlocks.MonsterExemplars.Where(pet => pet.HasTrait(Trait.Demon) && CommonEncounterFuncs.Between(pet.Level, level - 1, level + 2) && !pet.HasTrait(Trait.DemonLord) && !pet.HasTrait(Trait.NonSummonable)).ToArray();

            if (list.Count() <= 0) return;

            var demon = MonsterStatBlocks.MonsterFactories[UtilityFunctions.ChooseAtRandom(list)!.Name](battle.Encounter, battle.Map.Tiles[7, 10]);

            if (demon.Level - level >= 2) {
                demon.ApplyWeakAdjustments(false, true);
            } else if (demon.Level - level == 1) {
                demon.ApplyWeakAdjustments(false);
            } else if (demon.Level - level == -1) {
                demon.ApplyEliteAdjustments();
            } else if (demon.Level - level == -2) {
                demon.ApplyEliteAdjustments(true);
            }

            var toSpawn = new Creature[] { demon };

            if (battle.RoundNumber == 3) {

                list = MonsterStatBlocks.MonsterExemplars.Where(pet => pet.HasTrait(Trait.Demon) && CommonEncounterFuncs.Between(pet.Level, level - 4, level - 1) && !pet.HasTrait(Trait.DemonLord) && !pet.HasTrait(Trait.NonSummonable)).ToArray();

                if (list.Count() <= 0) return;

                var lesserDemon = MonsterStatBlocks.MonsterFactories[UtilityFunctions.ChooseAtRandom(list)!.Name](battle.Encounter, battle.Map.Tiles[7, 10]);

                if (lesserDemon.Level - level >= -1) {
                    lesserDemon.ApplyWeakAdjustments(false, true);
                } else if (lesserDemon.Level - level == -2) {
                    lesserDemon.ApplyWeakAdjustments(false);
                } else if (lesserDemon.Level - level == -4) {
                    lesserDemon.ApplyEliteAdjustments();
                } else if (lesserDemon.Level - level == -5) {
                    lesserDemon.ApplyEliteAdjustments(true);
                }
                toSpawn = new Creature[] { demon, lesserDemon };
            }

            SpawnGateAtInitiativeCount1(battle, toSpawn, battle.Map.Tiles[7, 10]);

            void SpawnGateAtInitiativeCount1(TBattle battle, Creature[] monsters, Tile spawnTile) {
                var gate = Creature.CreateNoncombatCreature(IllustrationName.PlaneshiftGateActivation, $"Abyssal Breach", [Trait.Demon, Trait.IllusoryObject, Trait.Horizontal, Trait.UnderneathCreatures, Trait.ElementalColor]);
                battle.SpawnIllusoryCreature(gate, spawnTile);
                gate.OwningFaction = battle.Gaia;
                gate.DescriptionFulltext = $"{{b}}Incoming planeshift.{{/b}} At this initiative count, this planar gate will summon a wave of demons, then deactivate again.";
                gate.Initiative = 1;
                gate.Level = battle.Encounter.CharacterLevel;
                gate.EntersInitiativeOrder = true;
                gate.RegeneratePossibilities();
                battle.InitiativeOrder.Add(gate);
                Sfxs.Play(SfxName.AuraExpansion);
                var pillars = FindPillars(spawnTile);
                foreach (var pillar in pillars) {
                    AdjustImageTo(pillar, true);
                }

                gate.AddQEffect(new QEffect() {
                    StartOfYourPrimaryTurn = async (qf, self) => {
                        Sfxs.Play(SfxName.PhaseBolt);
                        self.Battle.RemoveCreatureFromGame(self);
                        var pillars2 = FindPillars(self.Space.TopLeftTile);
                        foreach (var pillar in pillars2) {
                            AdjustImageTo(pillar, false);
                        }
                        foreach (var monster in monsters) {
                            monster.Traits.Add(Trait.Summoned);
                            self.Battle.SpawnCreature(monster, battle.Enemy, spawnTile);
                        }
                    }
                });
            }

            Tile[] FindPillars(Tile tile) {
                Tile?[] tiles =
                [
                    tile.Battle.Map.GetTile(tile.X - 2, tile.Y),
            tile.Battle.Map.GetTile(tile.X + 2, tile.Y),
            tile.Battle.Map.GetTile(tile.X, tile.Y - 2),
            tile.Battle.Map.GetTile(tile.X, tile.Y + 2)
                ];
                return tiles.Where(tl => tl != null && tl.PostCreatureIllustrations.Any()).WhereNotNull().ToArray();
            }

            void AdjustImageTo(Tile pillar, bool active) {
                var existingIllustration = pillar.PostCreatureIllustrations.FirstOrDefault();
                if (existingIllustration == IllustrationName.None) return;
                pillar.PostCreatureIllustrations.Clear();
                if (active) {
                    if (existingIllustration == IllustrationName.PlaneshiftGateDown) {
                        pillar.PostCreatureIllustrations.Add(IllustrationName.ActivePlaneshiftGateDown);
                    } else if (existingIllustration == IllustrationName.PlaneshiftGateUp) {
                        pillar.PostCreatureIllustrations.Add(IllustrationName.ActivePlaneshiftGateUp);
                    } else if (existingIllustration == IllustrationName.PlaneshiftGateLeft) {
                        pillar.PostCreatureIllustrations.Add(IllustrationName.ActivePlaneshiftGateLeft);
                    } else if (existingIllustration == IllustrationName.PlaneshiftGateRight) {
                        pillar.PostCreatureIllustrations.Add(IllustrationName.ActivePlaneshiftGateRight);
                    }
                } else {
                    if (existingIllustration == IllustrationName.ActivePlaneshiftGateDown) {
                        pillar.PostCreatureIllustrations.Add(IllustrationName.PlaneshiftGateDown);
                    } else if (existingIllustration == IllustrationName.ActivePlaneshiftGateUp) {
                        pillar.PostCreatureIllustrations.Add(IllustrationName.PlaneshiftGateUp);
                    } else if (existingIllustration == IllustrationName.ActivePlaneshiftGateLeft) {
                        pillar.PostCreatureIllustrations.Add(IllustrationName.PlaneshiftGateLeft);
                    } else if (existingIllustration == IllustrationName.ActivePlaneshiftGateRight) {
                        pillar.PostCreatureIllustrations.Add(IllustrationName.PlaneshiftGateRight);
                    }
                }
            }
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
