using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Core;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Display;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Act1
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Colosseum2Lv1 : NormalEncounter
    {
        public Colosseum2Lv1(string filename) : base("Colosseum", filename)
        {
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
                        var newItemRewards = new List<Item>();
                        CommonEncounterFuncs.SetItemRewards(newItemRewards, CharacterLevel, EncounterType.NORMAL);

                        Rewards.AddRange(newItemRewards);

                        for (int i = 0; i < 3; i++)
                        {
                            var creature = Ardamok.Create().ApplyWeakAdjustments(false);
                            battle.SpawnCreature(creature, battle.Enemy, 6 + i % 2, 2 + ((i / 2) % 2));
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
                        await base.Cleanup(battle);
                        await battle.EndTheGame(true, "You won the first round, and chose to leave with your winnings.");
                    }
                }
                else if (round == 2)
                {
                    if (await AskToContinueBattle(battle, round))
                    {
                        RewardGold += (int)(CommonEncounterFuncs.GetGoldReward(CharacterLevel, EncounterType.NORMAL) * 0.8);

                        battle.SpawnCreature(R.Coin() ? YoungChimera.Create().ApplyWeakAdjustments(false) : FlailSnail.Create().ApplyWeakAdjustments(false), battle.Enemy, 7, 2);

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
                        await base.Cleanup(battle);
                        await battle.EndTheGame(true, "You won the second round, and chose to leave with your winnings.");
                    }
                }
                else if (round == 3)
                {
                    foreach (var creature in battle.AllCreatures.Where((c) => c.OwningFaction == battle.You))
                    {
                        GrantFeatEffect(creature);
                    }

                    await base.Cleanup(battle);
                    await battle.EndTheGame(true, "You beat the colosseum! For your prize, each of your characters has been trained in a magical technique.");
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

            var spellcaster = character.Spellcasting != null
                && (character.Spellcasting.PrimarySpellcastingSource?.Kind == SpellcastingKind.Spontaneous
                || character.Spellcasting.PrimarySpellcastingSource?.Kind == SpellcastingKind.Prepared);
            var martial = character.Proficiencies.Get(Trait.Martial) >= Proficiency.Trained;

            var effects = new List<LTEs.ColosseumFeat>()
            {
                LTEs.ColosseumFeat.BurningJet,
                LTEs.ColosseumFeat.FlyingFlame,
                LTEs.ColosseumFeat.FourWinds,
                LTEs.ColosseumFeat.KiRush,
                LTEs.ColosseumFeat.LayOnHands,
                LTEs.ColosseumFeat.LesserFireShieldStance,
                LTEs.ColosseumFeat.LesserLevitationStance,
                LTEs.ColosseumFeat.OceansBalm,
                LTEs.ColosseumFeat.TimberSentinel,
                LTEs.ColosseumFeat.WintersClutch
            };

            if (martial)
            {
                effects.Add(LTEs.ColosseumFeat.ForceFang);
            }

            if (spellcaster)
            {
                effects.Add(LTEs.ColosseumFeat.DangerousSorcery);
                effects.Add(LTEs.ColosseumFeat.ReachSpell);
                effects.Add(LTEs.ColosseumFeat.WidenSpell);
            }

            for (int i = 0; i < effects.Count; i++)
            {
                if (character.HasFeat(LTEs.ColosseumFeatNames[effects[i]].Item1))
                {
                    effects.RemoveAt(i);
                    i--;
                }
            }

            var feat = effects[R.Next(effects.Count)];

            character.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("ChampionOfTheColosseumMagical", feat.HumanizeTitleCase2())!);
            character.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect(LTEs.ColosseumFeatNames[feat].Item2)!);
        }
    }
}
