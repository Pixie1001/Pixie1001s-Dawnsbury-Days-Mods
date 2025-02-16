using Dawnsbury.Core.Creatures;
using Dawnsbury.Core;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Colosseum1Lv2 : Level2Encounter
    {
        private int Round;

        public Colosseum1Lv2(string filename) : base("Colosseum", filename)
        {
            RewardGold = (int)(CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.NORMAL) * 0.8);

            Round = 1;

            ReplaceTriggerWithCinematic(TriggerName.StartOfEncounterBeforeStateCheck, async (TBattle battle) =>
            {
                //var placeholderKobolds = battle.AllCreatures.Where((creature) => creature.Name.Contains("Kobold")).ToArray();

                battle.AllCreatures.RemoveAll((creature) => creature.Name.Contains("Kobold"));

                for (int i = 0; i < 4; i++)
                {
                    var creature = Bodyguard.Create();
                    creature.MainName = "Gladiator";
                    battle.SpawnCreature(creature, battle.Enemy, 6 + i % 2, 2 + ((i / 2) % 2));
                }

                //battle.SpawnCreature(RalknarTheRude.Create(), battle.Enemy, 7, 2);
            });

            ReplaceTriggerWithCinematic(TriggerName.AllEnemiesDefeated, async (TBattle battle) =>
            {
                if (Round == 1)
                {
                    if (await AskToContinueBattle(battle, Round))
                    {
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
                            await character.HealAsync($"{character.Level}d4+{character.Level * 4}", new(character, IllustrationName.Heal, "Heal", [Trait.Healing], "", Target.Self()));

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
                else if (Round == 2)
                {
                    if (await AskToContinueBattle(battle, Round))
                    {
                        battle.SpawnCreature(RalknarTheRude.Create(), battle.Enemy, 7, 2);

                        foreach (var character in battle.AllCreatures.Where((creature) => creature.OwningFaction == battle.You))
                        {
                            await character.HealAsync($"{character.Level}d4+{character.Level * 4}", new(character, IllustrationName.Heal, "Heal", [Trait.Healing], "", Target.Self()));

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
                else if (Round == 3)
                {
                    await battle.EndTheGame(true, "You beat the colosseum! For your prize, each of your characters has been trained in a martial technique.");
                }

                Round++;
            });
        }

        public override void ModifyCreatureSpawningIntoTheEncounter(Creature creature)
        {
            if (creature.MainName == "Bodyguard" && creature.OwningFaction.IsEnemy)
            {
                creature.MainName = creature.Name.Replace("Bodyguard", "Gladiator");
            }
        }

        private static async Task<bool> AskToContinueBattle(TBattle battle, int round)
        {
            return await battle.AskForConfirmation(battle.AllCreatures.First((creature) => creature.OwningFaction == battle.You), IllustrationName.WinningStreak, $"You've beaten round {round}! Do you wish to continue fighting for extra rewards?", "Yes", "No");
        }
    }
}
