using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawnsbury.Display;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using FMOD;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Tables {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class SkillChallengeTables {
        public static List<SkillChallenge> events = new List<SkillChallenge>();
        public static Dictionary<int, SkillChallenge> chosenEvents = new Dictionary<int, SkillChallenge>();

        public static void LoadSkillChallengeTables() {
            chosenEvents.Clear();

            events.Add(new SkillChallenge("Cursed Relic", async (level, battle) => {
                Item cursedItem = LootTables.RollWearable(GetParty(battle).GetRandom(), lvl => CommonEncounterFuncs.Between(lvl, 3, Math.Max(3, lvl + 1)));
                await battle.Cinematics.NarratorLineAsync($"As the party decends further into the winding depths of the {Loader.UnderdarkName}, they emerge into a small chamber that bears the telltale marks of a demonic ritual.");
                await battle.Cinematics.NarratorLineAsync("Jagged profane symbols hewn in crusting blood sprawl across the cavern floor in great rings, alongside the rotting remains of several manacled corpses.");
                await battle.Cinematics.NarratorLineAsync($"...and in the centre, a {cursedItem.Name.CapitalizeEachWord}, bereft of dust and seemingly abandoned by those it was bequeathed upon...");
                await battle.Cinematics.NarratorLineAsync($"Perhaps the ritualists responsible for this terrible act where slain by whatever foul creatured they hoped to contact... Or left their loathsome reward behind as a cunning trap.");
                await battle.Cinematics.NarratorLineAsync($"Regardless the {cursedItem.Name.CapitalizeEachWord} emanates a dark and terrible aura, no doubt possessed of a great demonic taint. And yet, the party can hardly afford to be picky in times such as these...");
                battle.Cinematics.ExitCutscene();
                SCOption opt1 = GetBestPartyMember(battle, level, 0, Skill.Religion);
                SCOption opt2 = GetBestPartyMember(battle, level, 0, Skill.Occultism);
                SCOption opt3 = GetBestPartyMember(battle, level, -2, Skill.Arcana, Skill.Diplomacy);
                var choice = await CommonQuestions.OfferDialogueChoice(GetParty(battle).First(), GetNarrator(),
                    $"Regardless the {cursedItem.Name} emanates a dark and terrible aura, no doubt possessed of a great demonic taint. And yet, the party can hardly afford to be picky in times such as these...",
                    $"{opt1.printInfoTag()} Have {opt1.Nominee.Name} perform a ritual to cleanse the {cursedItem.Name.CapitalizeEachWord} of its curse.",
                    $"{opt2.printInfoTag()} {opt2.Nominee.Name} believes they might be able to find a loophole in the curse.",
                    $"{opt3.printInfoTag()} {opt3.Nominee.Name} hesitantly suggests that the demon might yet still be bargained with to bequeash the {cursedItem.Name.CapitalizeEachWord} to the party.",
                    "The party decides to leave the item where it lies."
                    );
                battle.Cinematics.EnterCutscene();

                CheckResult result = CheckResult.Failure;

                switch (choice.Index) {
                    case 0:
                        await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} spends several hours setting up an elaborate ritual to cleanse the {cursedItem.Name.CapitalizeEachWord}.");
                        result = opt1.Roll();
                        if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Heavenly light bathes the {cursedItem.Name.CapitalizeEachWord}, banishing the evil energy lurking within!");
                            battle.CampaignState.CommonLoot.Add(cursedItem);
                        } else {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"{opt1.Nominee.Name}'s attempts to cleanse the item do not go unnoticed. Barely a moment passes before the {cursedItem.Name.CapitalizeEachWord} explodes into " +
                                $"a cloud of sickly black smoke and violently drives itself down {opt1.Nominee.Name}'s throat!");
                            await battle.Cinematics.NarratorLineAsync($"Though the party manages to rouse {opt1.Nominee.Name} several hours later, they appear afflicted by a sickly pallour.");
                            await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} has been cursed with Clumsy, Enfeebled and Stupified 1 until their next long rest.");
                            opt1.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Lingering Curse"));
                        }
                        break;
                    case 1:
                        await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} spends several hours consulting a collection of heavy grimoires and leather bound volumes in order to identify and circumvent the maladiction placed on the {cursedItem.Name.CapitalizeEachWord}.");
                        result = opt2.Roll();
                        if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"{opt2.Nominee.Name} emerges with the {cursedItem.Name.CapitalizeEachWord} several hours later, now bound in rune scribed binding wraps, before quickly running the rest of their through how to safely operate it without activating the curse.");
                            await battle.Cinematics.NarratorLineAsync($"The {cursedItem.Name.CapitalizeEachWord} should be safe for the party to use now... Probably. Though it's unlikely any merchant will want to take it.");
                            cursedItem.Price = 0;
                            foreach (Item rune in cursedItem.Runes) {
                                rune.Price = 0;
                            }
                            battle.CampaignState.CommonLoot.Add(cursedItem);
                        } else {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"After severa labourious hours, {opt2.Nominee.Name} tenetatively reaches out to put their theory to the test... Holding it aloft for several promising moment passes before the {cursedItem.Name.CapitalizeEachWord} abruptly explodes into " +
                                $"a cloud of sickly black smoke and violently drives itself down {opt1.Nominee.Name}'s throat!");
                            await battle.Cinematics.NarratorLineAsync($"Though the party manages to rouse {opt1.Nominee.Name} several hours later, they appear afflicted by a sickly pallour.");
                            await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} has been cursed with Clumsy, Enfeebled and Stupified 1 until their next long rest.");
                            opt1.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Lingering Curse"));
                        }
                        break;
                    case 2:
                        await battle.Cinematics.NarratorLineAsync($"{opt3.Nominee.Name} quickly sets about recruiting the rest of the party to help shore up the abandoned ritual circle, before attempting to make contact with the demon.");
                        result = opt3.Roll();
                        if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"A series of complicated clauses and entreaties soon follow as {opt3.Nominee.Name} negotiates with sinister, lascivious voice eminating from the {cursedItem.Name.CapitalizeEachWord}.");
                            await battle.Cinematics.NarratorLineAsync("And then, all at once before the party even fully realises what they've agreed to, a deal is struck. Each party members gains {b}Drained 2{/b}.");
                            foreach (Creature pm in GetParty(battle)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Drained", null, 2));
                            }
                            battle.CampaignState.CommonLoot.Add(cursedItem);
                        } else {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"And yet, {opt3.Nominee.Name}'s attempts to treat with the demon remain unanswered until the party reluctantly gives up and moves on, disappointed but quietly relieved.");
                        }
                        break;
                    case 3:
                        await battle.Cinematics.NarratorLineAsync($"With one last look at the ominous smoke curling off the {cursedItem.Name.CapitalizeEachWord} the party wisely moves on ahead, leaving the accursed item untouched.");
                        break;
                }
            }));

            events.Add(new SkillChallenge("Strange Mushrooms", async (level, battle) => {
                await battle.Cinematics.NarratorLineAsync("The party finds some weird mushrooms.");
                battle.Cinematics.ExitCutscene();
                SCOption opt1 = GetBestPartyMember(battle, level, -2, Skill.Nature);
                SCOption opt2 = GetBestPartyMember(battle, level, 2, Skill.Occultism);

                var choice = await CommonQuestions.OfferDialogueChoice(GetParty(battle).First(), GetNarrator(),
                    $"What does the party choose to do about these unusual mushrooms?",
                    $"{opt1.printInfoTag()} {opt1.Nominee.Name} believes the mushrooms could be used to create healing tonics.",
                    $"{opt2.printInfoTag()} {opt2.Nominee.Name} once read about a strange rite once practiced centuries ago to form a symbiotic connection with mushrooms such as these.",
                    "The party decides to leave the strange mushrooms be."
                    );
                battle.Cinematics.EnterCutscene();

                CheckResult result = CheckResult.Failure;

                switch (choice.Index) {
                    case 0:
                        await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} dilligently begins harvesting the strange mushrooms.");
                        result = opt1.Roll();
                        if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"{opt1.Nominee.Name} returns with several tonics of healing concocted from the strange mushroom's sap.");
                            if (level == 1) {
                                await battle.Cinematics.NarratorLineAsync("The party gained a {b}lesser healing potion.{/b}");
                                battle.CampaignState.CommonLoot.Add(Items.CreateNew(ItemName.LesserHealingPotion));
                            } else if (level == 2) {
                                await battle.Cinematics.NarratorLineAsync("The party gained {b}lesser healing potion x2{/b}");
                                battle.CampaignState.CommonLoot.Add(Items.CreateNew(ItemName.LesserHealingPotion));
                                battle.CampaignState.CommonLoot.Add(Items.CreateNew(ItemName.LesserHealingPotion));
                            } else if (level == 3) {
                                await battle.Cinematics.NarratorLineAsync("The party gained a {b}moderate healing potion{/b} and a {b}lesser healing potion{/b}");
                                battle.CampaignState.CommonLoot.Add(Items.CreateNew(ItemName.ModerateHealingPotion));
                                battle.CampaignState.CommonLoot.Add(Items.CreateNew(ItemName.LesserHealingPotion));
                            }
                        } else {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"{opt1.Nominee.Name} was poised by a puff of spores!");
                            await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} has become sickened 1 for the duration of the next encounter.");
                            opt1.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Mushroom Sickness", null, 1));
                        }
                        break;
                    case 1:
                        // TODO: Fill this out for poison immunity
                        await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} recreates the ritual to the best of their ability, before sitting down and pressing their head up against the mushroom.");
                        result = opt2.Roll();
                        if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The mushroom's trill happily, illuminating the cavern in eerie bioluminescent light as a thin cobweb of Mycelium roots emerge from the ground to form a cocoon around {opt2.Nominee.Name} as they kneel.");
                            await battle.Cinematics.NarratorLineAsync($"In exchange for spreading their spores to distant caverns, the mushrooms will cleans {opt2.Nominee.Name}'s body of harmful organisms. They gains the Mushroom Symbiote ability, protecting them from poisons for the rest of the adventure.");
                            opt2.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Mushroom Symbiote"));
                        } else {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The mushrooms are angered by {opt2.Nominee.Name}'s meager offering, expelling a large cloud of poisonous spores!");
                            await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} has become sickened 1 for the duration of the next encounter.");
                            opt2.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Mushroom Sickness", null, 2));
                        }
                        break;
                    case 2:
                        await battle.Cinematics.NarratorLineAsync($"The party decides to move on, letting the strange mushrooms be.");
                        break;
                }
            }));

            events.Add(new SkillChallenge("Drow Renegades", async (level, battle) => {
                await battle.Cinematics.NarratorLineAsync("You stumble upon a drow hunting party, adorned in strange luna iconography. After some initial tension, they reveal that they're renegade disciples of the Cerulean Sky, on a mission to liberate their people from the starborn.");
                await battle.Cinematics.NarratorLineAsync($"Yet suspicions still linger. Drow are known for their deceit, and many evil adventurer parties often delve into the {Loader.UnderdarkName} to barter for slaves or bargain for demonic boons.");
                await battle.Cinematics.NarratorLineAsync("To earn their trust and cooperate towards the two groups' mutual goals...");
                battle.Cinematics.ExitCutscene();
                SCOption opt1 = GetBestPartyMember(battle, level, 0, Skill.Diplomacy);
                SCOption opt2 = GetBestPartyMember(battle, level, 2, (cr, skill) => cr.HasFeat(FeatName.TheCeruleanSky) ? 2 : 0, Skill.Religion);
                var choice = await CommonQuestions.OfferDialogueChoice(GetParty(battle).First(), GetNarrator(),
                    $"To earn their trust and cooperate towards the two group's mutual goals...",
                    $"{opt1.printInfoTag()} {opt1.Nominee.Name} suggests an exchange of information.",
                    $"{opt2.printInfoTag()} {opt2.Nominee.Name} suggests appealing to their shared devotion to the surface deities.",
                    "The party agrees the risk of spies is too great."
                    );
                battle.Cinematics.EnterCutscene();
                CheckResult result = CheckResult.Failure;

                switch (choice.Index) {
                    case 0:
                        await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} suggests that whilst they could hardly ask the reneges to divert resources from their own righteous cause, the two groups could at least share information about enemy movements and hazards they've encountered on their journey so far.");
                        result = opt1.Roll();
                        if (result == CheckResult.CriticalFailure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Despite {opt1.Nominee.Name}'s best efforts, the renegades remain tight-lipped, sharing only the vaguest of details about their movements.");
                            await battle.Cinematics.NarratorLineAsync($"The party is ready to give up, until one young paladin approaches them in private, seemingly disillusioned by her companion's paranoia and needless caution.");
                            await battle.Cinematics.NarratorLineAsync($"They talk for many hours, but it isn't until the two groups have long parted ways and the party begin to feel watchful eyes raising the hair on their necks, that they begin to realise her information is subtly but all too insidiously wrong...");
                            await battle.Cinematics.NarratorLineAsync("The enemy knows you're coming. Each member of the party gains {b}Compromised Route{/b}, reducing their inititive by 1.");
                            foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Compromised Route", null, null));
                            }
                        } else if (result == CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Despite {opt1.Nominee.Name}'s best efforts, the renegades remain tight-lipped, sharing only the vaguest of details about their movements.");
                            await battle.Cinematics.NarratorLineAsync($"It seems even between friends, trust in the {Loader.UnderdarkName} is difficult to find.");
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Though initially cautious, the renegades eventually warm up to {opt1.Nominee.Name} and before long the party finds themselves engrossed in conversation with the group, swapping war stories and critical intel alike.");
                            foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Information Sharing", null, null));
                            }
                        }
                        break;
                    case 1:
                        await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} speaks to the group of the surface world, and the beauty of the Cerulean Sky at night, insisting that their mission to foil the Demon Lord's plot is of utmost urgency to their goddess.");
                        result = opt2.Roll();
                        if (result == CheckResult.CriticalFailure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + "The group initially seems uncertain. Whilst they owe a great deal to their goddess, the weight of those still enthralled by the spider queen weighs heavy on their shoulders.");
                            await battle.Cinematics.NarratorLineAsync($"Yet at the last moment a young woman steps up from among their ranks, inspired by {opt2.Nominee.Name}'s words, offering to act as a guide for their journey.");
                            await battle.Cinematics.NarratorLineAsync($"It isn't until the group hears the clicking of mandibles, their earnest guide nowhere to be seen, that they realise too late that they've been led into a trap.");
                            await battle.Cinematics.NarratorLineAsync("Each member of the party gains {b}Injured 1{/b}, reducing their max HP by 10% until they rest.");
                            foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Injured", null, 1));
                            }
                        } else if (result == CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The renegades thank {opt2.Nominee.Name} for their kind and illuminating words, but reluctantly insist they cannot spare any among their group to aid them in their quest.");
                            await battle.Cinematics.NarratorLineAsync($"Whether through doubt or concern for their own difficult struggles, it's clear the party will find no succor here after all.");
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The renegades seem inspired by {opt2.Nominee.Name}'s words, one amongst their number steadfastly plunging her gleaming blade into the gloomy obsidian rocks as she pledges to hasten them on their journey.");
                            await battle.Cinematics.NarratorLineAsync($"A Drow Renegade will aid {opt2.Nominee.Name} in battle until she perishes, or the party returns to town.");
                            opt2.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Drow Renegade Companion", null, null));
                        }
                        break;
                    case 2:
                        await battle.Cinematics.NarratorLineAsync($"The renegades nod in grim agreement, seemingly no strangers to betrayal themselves.");
                        await battle.Cinematics.NarratorLineAsync($"Nevertheless, they volunteer a small amount of supplies to hasten the party on their journey, for it is a rare thing indeed to meet a group in the {Loader.UnderdarkName} willing to exchange pleasantries without an ulterior motive.");
                        await battle.Cinematics.NarratorLineAsync("The party obtained {b}" + level * 7 + " gold{/b} worth of supplies.");
                        break;
                }
            }));

            events.Add(new SkillChallenge("Escaped Slaves", async (level, battle) => {
                await battle.Cinematics.NarratorLineAsync("Coming from an opposing cavern, the party spots a group of bedraggled figures shambling towards them, fanning out like starving jackals with hungry sunken eyes.");
                await battle.Cinematics.NarratorLineAsync($"Dirty, emaciated and still wearing the remains of broken shackles, they can only be a group of escaped slaves. Yet even with a common enemy, the harsh environment of the {Loader.UnderdarkName} has little mercy for those who cannot take what they need to survive.");
                await battle.Cinematics.NarratorLineAsync("What does the party do?");
                battle.Cinematics.ExitCutscene();
                SCOption opt1 = GetBestPartyMember(battle, level, -2, Skill.Intimidation);
                SCOption opt2 = GetBestPartyMember(battle, level, 0, Skill.Diplomacy);

                List<string> choices = new List<string>() {
                    $"{opt1.printInfoTag()} {opt1.Nominee.Name} suggests scaring the group away.",
                    $"{opt2.printInfoTag()} {opt2.Nominee.Name} believes the group can be reasoned with.",
                    "{DimGray}{b}" + $"[Lose {level * 5} gold]" + "{/b}{/DimGray} Despite the slave's aggression, the party offers what little they can all the same."
                };

                if (battle.CampaignState.CommonGold < level * 5) {
                    choices.RemoveAt(choices.Count - 1);
                }

                var choice = await CommonQuestions.OfferDialogueChoice(GetParty(battle).First(), GetNarrator(),
                    $"What does the party do?",
                    choices.ToArray()
                    );
                battle.Cinematics.EnterCutscene();
                CheckResult result = CheckResult.Failure;

                switch (choice.Index) {
                    case 0:
                        await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} steps confidently towards the encroaching mob, motioning for the party to draw steel and spell alike, daring them to come closer.");
                        result = opt1.Roll();
                        if (result <= CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The slaves eyes widen in panic, mistaking the attempt to scare them away as an attack... desperately converging on the party.");
                            await battle.Cinematics.NarratorLineAsync($"The escaped slaves are no match for a group of trained adventurers, and before long the rest are send fearfully scuyrrying away into the cavern... The cooling bodies of their friend's a grim reminder of the party's failure.");
                            await battle.Cinematics.NarratorLineAsync("Each member of the party gains {b}Guilt 2{/b}, reducing their Will saves by 2 until they rest.");
                            foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Guilt", null, 2));
                            }
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The mob fearfully skitters away from {opt1.Nominee.Name}, their hungry eyes lingering on the party's supply packs and then their weapons...");
                            await battle.Cinematics.NarratorLineAsync("The tense silence weighs heavy on the two groups for several moments, before they reluctantly shamble away.");
                        }
                        break;
                    case 1:
                        await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} meets the escapee's ravenous gazes with compassion, speaking of their common enemy and offering a map of the path they've cleared so far.");
                        result = opt2.Roll();
                        result = opt1.Roll();
                        if (result <= CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Yet the {Loader.UnderdarkName} prove themselves too cruel for soft words and lofty ideals. Sensing weakness, the desperate slaves surge forwards, enveloping {opt2.Nominee.Name} before the party has time to step in.");
                            await battle.Cinematics.NarratorLineAsync(opt2.Nominee.Name + " gains {b}Injured 1{/b}, reducing their max HP by 10% until they rest.");
                            opt2.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Injured", null, 1));
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The group are scared, hungry and desperate… Yet {opt2.Nominee.Name}'s words remind them of who they used to be.");
                            await battle.Cinematics.NarratorLineAsync($"Thanking {opt2.Nominee.Name} for their kindness and directions they shuffle on, seeking refuge in Dawnsbury.");
                            await battle.Cinematics.NarratorLineAsync("Each member of the party gains {b}Hope 1{/b}, granting a +1 status bonus to their Will saves and attack bonus until they rest.");
                            foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Hope", null, 1));
                            }
                        }
                        break;
                    case 2:
                        await battle.Cinematics.NarratorLineAsync($"Despite the slave's aggression, the party offers what little they can all the same.");
                        await battle.Cinematics.NarratorLineAsync($"The slaves accept the party's offer of aid with wary eyes, before departing, unwilling to push their luck any further against an armed group.");
                        await battle.Cinematics.NarratorLineAsync("The party lost {b}" + level * 5 + " gold{/b}, but each member gains {b}Hope 1{/b}, granting a +1 status bonus to their Will saves and attack bonus until they rest.");
                        foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                            pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Hope", null, 1));
                        }
                        battle.CampaignState.CommonGold -= level * 5;
                        break;
                }
            }));

            //events.Add(new SkillChallenge("Magical Traps", async (level, battle) => {
            //    await battle.Cinematics.NarratorLineAsync("Coming from an opposing cavern, the party spots a group of bedraggled figures shambling towards them, fanning out like starving jackals with hungry sunken eyes.");
            //    await battle.Cinematics.NarratorLineAsync("Dirty, emaciated and still wearing the remains of broken shackles, they can only be a group of escaped slaves. Yet even with a common enemy, the harsh environment of the {Loader.UnderdarkName} has little mercy for those who cannot take what they need to survive.");
            //    await battle.Cinematics.NarratorLineAsync("What does the party do?");
            //    battle.Cinematics.ExitCutscene();
            //    SCOption opt1 = GetBestPartyMember(battle, level, -2, Skill.Intimidation);
            //    SCOption opt2 = GetBestPartyMember(battle, level, 0, Skill.Diplomacy);

            //    List<string> choices = new List<string>() {
            //        $"{opt1.printInfoTag()} {opt1.Nominee.Name} suggests scaring the group away.",
            //        $"{opt2.printInfoTag()} {opt2.Nominee.Name} believes the group can be reasoned with.",
            //        "Despite the slave's aggression, the party offers what little they can all the same."
            //    };

            //    if (battle.CampaignState.CommonGold < level * 5) {
            //        choices.RemoveAt(choices.Count - 1);
            //    }

            //    var choice = await CommonQuestions.OfferDialogueChoice(GetParty(battle).First(), GetNarrator(),
            //        $"What does the party do?",
            //        choices.ToArray()
            //        );
            //    battle.Cinematics.EnterCutscene();
            //    CheckResult result = CheckResult.Failure;

            //    switch (choice.Index) {
            //        case 0:
            //            await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} steps confidently towards the encroaching mob, motioning for the party to draw steel and spell alike, daring them to come closer.");
            //            result = opt1.Roll();
            //            if (result <= CheckResult.Failure) {
            //                await battle.Cinematics.NarratorLineAsync("{b}" + PrintResult(result) + "!{/b} " + $"The slaves eyes widen in panic, mistaking the attempt to scare them away as an attack... desperately converging on the party.");
            //                await battle.Cinematics.NarratorLineAsync($"The escaped slaves are no match for a group of trained adventurers, and before long the rest are send fearfully scuyrrying away into the cavern... The cooling bodies of their friend's a grim reminder of the party's failure.");
            //                await battle.Cinematics.NarratorLineAsync("Each member of the party gains {b}Guilt 2{/b}, reducing their Will saves by 2 until they rest.");
            //                foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
            //                    pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Guilt", null, 2));
            //                }
            //            } else if (result >= CheckResult.Success) {
            //                await battle.Cinematics.NarratorLineAsync("{b}" + rPrintResult(result) + "!{/b} " + $"The mob fearfully skitters away from {opt1.Nominee.Name}, their hungry eyes lingering on the party's supply packs and then their weapons...");
            //                await battle.Cinematics.NarratorLineAsync("The tense silence weighs heavy on the two groups for several moments, before they reluctantly shamble away.");
            //            }
            //            break;
            //        case 1:
            //            await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} meets the escapee's ravenous gazes with compassion, speaking of their common enemy and offering a map of the path they've cleared so far.");
            //            result = opt2.Roll();
            //            result = opt1.Roll();
            //            if (result <= CheckResult.Failure) {
            //                await battle.Cinematics.NarratorLineAsync("{b}" + PrintResult(result) + "!{/b} " + $"Yet the {Loader.UnderdarkName} prove themselves too cruel for soft words and lofty ideals. Sensing weakness, the desperate slaves surge forwards, enveloping {opt2.Nominee.Name} before the party has time to step in.");
            //                await battle.Cinematics.NarratorLineAsync(opt2.Nominee.Name + " gains {b}Injured 1{/b}, reducing their max HP by 10% until they rest.");
            //                opt2.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Injured", null, 1));
            //            } else if (result >= CheckResult.Success) {
            //                await battle.Cinematics.NarratorLineAsync("{b}" + PrintResult(result) + "!{/b} " + $"The group are scared, hungry and desperate… Yet {opt2.Nominee.Name}'s words remind them of who they used to be.");
            //                await battle.Cinematics.NarratorLineAsync($"Thanking {opt2.Nominee.Name} for their kindness and directions they shuffle on, seeking refuge in Dawnsbury.");
            //                await battle.Cinematics.NarratorLineAsync("Each member of the party gains {b}Hope 1{/b}, granting a +1 status bonus to their Will saves and attack bonus until they rest.");
            //                foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
            //                    pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Hope", null, 1));
            //                }
            //            }
            //            break;
            //        case 2:
            //            await battle.Cinematics.NarratorLineAsync($"Despite the slave's aggression, the party offers what little they can all the same.");
            //            await battle.Cinematics.NarratorLineAsync($"The slaves accept the party's offer of aid with wary eyes, before departing, unwilling to push their luck any further against an armed group.");
            //            await battle.Cinematics.NarratorLineAsync("The party lost {b}" + level * 5 + " gold{/b}, but each member gains {b}Hope 1{/b}, granting a +1 status bonus to their Will saves and attack bonus until they rest.");
            //            foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
            //                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Hope", null, 1));
            //            }
            //            battle.CampaignState.CommonGold -= level * 5;
            //            break;
            //    }
            //}));
        }

        private static SCOption GetBestPartyMember(TBattle battle, int level, int difficultyModifier, Func<Creature, Skill, int>? bonusLogic, params Skill[] skills) {
            var party = battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null);
            Creature bestCreature = party.First();
            int bestScore = -50;
            Skill bestSkill = Skill.Acrobatics;
            foreach (Skill skill in skills) {
                Skill currSkill = skill;
                Creature currCreature = party.OrderBy(cr => cr.Skills.Get(skill)).Last();
                int currScore = currCreature.Skills.Get(skill) + (bonusLogic != null ? bonusLogic(currCreature, skill) : 0);
                if (currScore > bestScore) {
                    bestScore = currScore;
                    bestCreature = currCreature;
                    bestSkill = currSkill;
                }
            }
            return new SCOption(bestCreature, bestSkill, bestScore, level, difficultyModifier, bonusLogic);
        }

        private static SCOption GetBestPartyMember(TBattle battle, int level, int difficultyModifier, params Skill[] skills) {
            return GetBestPartyMember(battle, level, difficultyModifier, null, skills);
        }

        private static List<Creature> GetParty(TBattle battle) {
            return battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null).ToList();
        }

        internal static int GetDCByLevel(int level) {
            return 14 + level + level / 3;
        }

        private static Creature GetNarrator() {
            return new Creature(IllustrationName.NarratorBook, "Narration", new List<Trait>(), 0, 0, 0, new Defenses(0, 0, 0, 0), 0, new Abilities(0, 0, 0, 0, 0, 0), new Skills());
        }

        private static string PrintResult(CheckResult result) {
            string colour = "Black";
            if (result == CheckResult.CriticalFailure) {
                colour = "DarkRed";
            } else if (result == CheckResult.Failure) {
                colour = "Red";
            } else if (result == CheckResult.Success) {
                colour = "Green";
            } else if (result == CheckResult.CriticalSuccess) {
                colour = "Chartreuse";
            }
            return "{b}{" + colour + "}" + result.HumanizeTitleCase2().CapitalizeEachWord() + "!{" + colour + "/}{/b} ";
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    class SCOption {
        public Creature Nominee { get; }
        public Skill Skill { get; }
        public int Bonus { get; }
        public int DC { get; set; }
        public Func<Creature, Skill, int>? BonusLogic { get; }

        public SCOption(Creature nominee, Skill skill, int bonus, int level, int mod, Func<Creature, Skill, int>? bonusLogic) {
            Nominee = nominee;
            Skill = skill;
            Bonus = bonus;
            DC = SkillChallengeTables.GetDCByLevel(level) + mod;
            BonusLogic = bonusLogic;
        }

        public string printInfoTag() {
            return "{b}{DimGray}" + $"[{Skill.HumanizeTitleCase2()} {UtilityFunctions.WithPlus(Bonus)} (DC {DC})]" + "{/DimGray}{/b} ";
        }

        public CheckResult Roll() {
            Sfxs.Play(SfxName.BookClosed);
            QEffect bonus = new QEffect();
            if (BonusLogic != null) {
                bonus.BonusToSkills = skill => new Bonus(BonusLogic(Nominee, skill), BonusType.Untyped, "Special Bonus");
                Nominee.AddQEffect(bonus);
            }
            CheckResult output = CommonSpellEffects.RollCheck("Skill Challenge", new ActiveRollSpecification(Checks.SkillCheck(Skill), Checks.FlatDC(DC)), Nominee, Nominee);
            bonus.ExpiresAt = ExpirationCondition.Immediately;
            return output;
        }
    }
}
