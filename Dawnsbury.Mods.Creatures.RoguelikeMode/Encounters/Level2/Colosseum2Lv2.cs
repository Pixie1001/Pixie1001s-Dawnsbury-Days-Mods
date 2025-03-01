using Dawnsbury.Core.Creatures;
using Dawnsbury.Core;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Auxiliary;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Colosseum2Lv2 : Level2Encounter
    {
        public Colosseum2Lv2(string filename) : base("Colosseum", filename)
        {
            RewardGold = (int)(CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.NORMAL) * 0.7);

            int round = 1;

            ReplaceTriggerWithCinematic(TriggerName.StartOfEncounterBeforeStateCheck, async (TBattle battle) =>
            {
                battle.AllCreatures.RemoveAll((creature) => creature.Name.Contains("Kobold"));

                for (int i = 0; i < 2; i++)
                {
                    var creature = Crocodile.Create();
                    battle.SpawnCreature(creature, battle.Enemy, 6 + i % 2, 3);
                }
            });

            ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async (TBattle battle) =>
            {
                if (round == 1)
                {
                    if (await AskToContinueBattle(battle, round))
                    {
                        RewardGold += (int)(CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.NORMAL) * 0.6);
                        var newItemRewards = new List<Item>();
                        CommonEncounterFuncs.SetItemRewards(newItemRewards, CharacterLevel, EncounterType.NORMAL);

                        Rewards.AddRange(newItemRewards);

                        for (int i = 0; i < 2; i++)
                        {
                            var creature = Bodyguard.Create();
                            creature.MainName = "Gladiator";
                            battle.SpawnCreature(creature, battle.Enemy, 6 + i % 2, 3);
                        }

                        for (int i = 0; i < 2; i++)
                        {
                            battle.SpawnCreature(Pikeman.Create(), battle.Enemy, 6 + i % 2, 2);
                        }

                        foreach (var character in battle.AllCreatures.Where((creature) => creature.OwningFaction == battle.You))
                        {
                            await character.HealAsync($"{character.Level}d4+{character.Level * 2}", new(character, IllustrationName.Heal, "Heal", [Trait.Healing], "", Target.Self()));

                            if (character.Spellcasting != null && character.Spellcasting.FocusPoints < character.Spellcasting.FocusPointsMaximum)
                            {
                                character.Spellcasting.FocusPoints++;
                            }
                        }
                    }
                    else
                    {
                        await battle.EndTheGame(true, "You won the first round, and chose to leave with your winnings.");
                    }
                }
                else if (round == 2)
                {
                    if (await AskToContinueBattle(battle, round))
                    {
                        RewardGold += (int)(CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.NORMAL) * 0.8);

                        battle.SpawnCreature(R.Coin() ? RalknarTheRude.Create() : SorantonTheSkilled.Create(), battle.Enemy, 7, 2);

                        foreach (var character in battle.AllCreatures.Where((creature) => creature.OwningFaction == battle.You))
                        {
                            await character.HealAsync($"{character.Level}d4+{character.Level * 2}", new(character, IllustrationName.Heal, "Heal", [Trait.Healing], "", Target.Self()));

                            if (character.Spellcasting != null && character.Spellcasting.FocusPoints < character.Spellcasting.FocusPointsMaximum)
                            {
                                character.Spellcasting.FocusPoints++;
                            }
                        }
                    }
                    else
                    {
                        await battle.EndTheGame(true, "You won the second round, and chose to leave with your winnings.");
                    }
                }
                else if (round == 3)
                {
                    foreach (var creature in battle.AllCreatures.Where((c) => c.OwningFaction == battle.You))
                    {
                        GrantFeatEffect(creature);
                    }

                    await battle.EndTheGame(true, "You beat the colosseum! For your prize, each of your characters has been trained in a martial technique.");
                }

                round++;
            });
        }

        private static async Task<bool> AskToContinueBattle(TBattle battle, int round)
        {
            return await battle.AskForConfirmation(battle.AllCreatures.First((creature) => creature.OwningFaction == battle.You), IllustrationName.WinningStreak, $"You've beaten round {round}! Do you wish to continue fighting for extra rewards?", "Yes", "No");
        }

        private static void GrantFeatEffect(Creature character)
        {
            if (character.LongTermEffects == null)
            {
                character.LongTermEffects = new();
            }

            var martial = character.Proficiencies.Get(Trait.Martial) >= Proficiency.Trained;
            var hasShieldBlock = character.HasEffect(QEffectId.ShieldBlock);
            var intimidation = character.Proficiencies.Get(Trait.Intimidation) >= Proficiency.Trained;

            var effects = new List<LTEs.ColosseumFeat>()
            {
                LTEs.ColosseumFeat.KiRush,
                LTEs.ColosseumFeat.Mobility,
                LTEs.ColosseumFeat.NimbleDodge,
                LTEs.ColosseumFeat.QuickDraw,
                LTEs.ColosseumFeat.RapidResponse,
                LTEs.ColosseumFeat.ShakeItOff
            };

            if (martial)
            {
                effects.Add(LTEs.ColosseumFeat.BrutalBeating);
                effects.Add(LTEs.ColosseumFeat.GravityWeapon);
                effects.Add(LTEs.ColosseumFeat.PowerAttack);
                effects.Add(LTEs.ColosseumFeat.SuddenCharge);

                if (intimidation)
                {
                    effects.Add(LTEs.ColosseumFeat.YoureNext);
                }
            }

            if (hasShieldBlock)
            {
                effects.Add(LTEs.ColosseumFeat.AggressiveBlock);
                effects.Add(LTEs.ColosseumFeat.ReactiveShield);
            }

            for (int i = 0; i < effects.Count; i++)
            {
                if (character.HasFeat(LTEs.ColosseumFeatNames[effects[i]].Item1))
                {
                    effects.RemoveAt(i);
                    i--;
                }
            }

            character.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect(LTEs.ColosseumFeatNames[effects[R.Next(effects.Count)]].Item2)!);
        }
    }
}
