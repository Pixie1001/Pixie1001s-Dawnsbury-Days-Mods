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
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Core.CharacterBuilder.Selections.Selected;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Tables {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class SkillChallengeTables {
        public static List<SkillChallenge> events = new List<SkillChallenge>();
        public static Dictionary<int, SkillChallenge> chosenEvents = new Dictionary<int, SkillChallenge>();

        public static void LoadSkillChallengeTables() {
            events.Clear();
            chosenEvents.Clear();

            events.Add(new SkillChallenge("Cursed Relic", async (level, battle) => {
                Item cursedItem = LootTables.RollWearable(GetParty(battle).GetRandom(), lvl => CommonEncounterFuncs.Between(lvl, 3, Math.Max(3, level + 1)));
                await battle.Cinematics.NarratorLineAsync($"As the party decends further into the winding depths of the {Loader.UnderdarkName}, they emerge into a small chamber that bears the telltale marks of a demonic ritual.", null);
                await battle.Cinematics.NarratorLineAsync("Jagged profane symbols hewn in crusting blood sprawl across the cavern floor in great rings, alongside the rotting remains of several manacled corpses.", null);
                await battle.Cinematics.NarratorLineAsync($"...and in the centre, a {cursedItem.Name.CapitalizeEachWord()}, bereft of dust and seemingly abandoned by those it was bequeathed upon...", null);
                await battle.Cinematics.NarratorLineAsync($"Perhaps the ritualists responsible for this terrible act where slain by whatever foul creatured they hoped to contact... Or left their loathsome reward behind as a cunning trap.", null);
                await battle.Cinematics.NarratorLineAsync($"Regardless the {cursedItem.Name.CapitalizeEachWord()} emanates a dark and terrible aura, no doubt possessed of a great demonic taint. And yet, the party can hardly afford to be picky in times such as these...", null);
                battle.Cinematics.ExitCutscene();
                SCOption opt1 = GetBestPartyMember(battle, level, 0, Skill.Religion);
                SCOption opt2 = GetBestPartyMember(battle, level, 0, Skill.Occultism);
                SCOption opt3 = GetBestPartyMember(battle, level, -2, Skill.Arcana, Skill.Diplomacy);
                var choice = await CommonQuestions.OfferDialogueChoice(GetParty(battle).First(), GetNarrator(),
                    $"Regardless the {cursedItem.Name} emanates a dark and terrible aura, no doubt possessed of a great demonic taint. And yet, the party can hardly afford to be picky in times such as these...",
                    $"{opt1.printInfoTag()} Have {opt1.Nominee.Name} perform a ritual to cleanse the {cursedItem.Name.CapitalizeEachWord()} {cursedItem.Illustration.IllustrationAsIconString} of its curse.",
                    $"{opt2.printInfoTag()} {opt2.Nominee.Name} believes they might be able to find a loophole in the curse.",
                    $"{opt3.printInfoTag()} {opt3.Nominee.Name} hesitantly suggests that the demon might yet still be bargained with to bequeash the {cursedItem.Name.CapitalizeEachWord()} {cursedItem.Illustration.IllustrationAsIconString} to the party.",
                    "The party decides to leave the item where it lies."
                    );
                battle.Cinematics.EnterCutscene();

                CheckResult result = CheckResult.Failure;

                switch (choice.Index) {
                    case 0:
                        await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} spends several hours setting up an elaborate ritual to cleanse the {cursedItem.Name.CapitalizeEachWord()} {cursedItem.Illustration.IllustrationAsIconString}.", null);
                        result = opt1.Roll();
                        if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Heavenly light bathes the {cursedItem.Name.CapitalizeEachWord()} {cursedItem.Illustration.IllustrationAsIconString}, banishing the evil energy lurking within!", null);
                            battle.CampaignState.CommonLoot.Add(cursedItem);
                        } else {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"{opt1.Nominee.Name}'s attempts to cleanse the item do not go unnoticed. Barely a moment passes before the {cursedItem.Name.CapitalizeEachWord()} {cursedItem.Illustration.IllustrationAsIconString} explodes into " +
                                $"a cloud of sickly black smoke and violently drives itself down {opt1.Nominee.Name}'s throat!", null);
                            await battle.Cinematics.NarratorLineAsync($"Though the party manages to rouse {opt1.Nominee.Name} several hours later, they appear afflicted by a sickly pallour.", null);
                            await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} has been cursed with Clumsy, Enfeebled and Stupified 1 until their next long rest.", null);
                            opt1.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Lingering Curse"));
                        }
                        break;
                    case 1:
                        await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} spends several hours consulting a collection of heavy grimoires and leather bound volumes in order to identify and circumvent the maladiction placed on the {cursedItem.Name.CapitalizeEachWord()} {cursedItem.Illustration.IllustrationAsIconString}.", null);
                        result = opt2.Roll();
                        if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"{opt2.Nominee.Name} emerges with the {cursedItem.Name.CapitalizeEachWord()} {cursedItem.Illustration.IllustrationAsIconString} several hours later, now bound in rune scribed binding wraps, before quickly running the rest of the party through how to safely operate it without activating the curse.", null);
                            await battle.Cinematics.NarratorLineAsync($"The {cursedItem.Name.CapitalizeEachWord()} {cursedItem.Illustration.IllustrationAsIconString} should be safe for the party to use now... Probably. Though it's unlikely any merchant will want to take it.", null);
                            cursedItem.Price = 0;
                            foreach (Item rune in cursedItem.Runes) {
                                rune.Price = 0;
                            }
                            battle.CampaignState.CommonLoot.Add(cursedItem);
                        } else {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"After several labourious hours, {opt2.Nominee.Name} tenetatively reaches out to put their theory to the test... Holding it aloft for several promising moment passes before the {cursedItem.Name.CapitalizeEachWord()} {cursedItem.Illustration.IllustrationAsIconString} abruptly explodes into " +
                                $"a cloud of sickly black smoke and violently drives itself down {opt1.Nominee.Name}'s throat!", null);
                            await battle.Cinematics.NarratorLineAsync($"Though the party manages to rouse {opt1.Nominee.Name} several hours later, they appear afflicted by a sickly pallour.", null);
                            await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} has been cursed with Clumsy, Enfeebled and Stupified 1 until their next long rest.", null);
                            opt1.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Lingering Curse"));
                        }
                        break;
                    case 2:
                        await battle.Cinematics.NarratorLineAsync($"{opt3.Nominee.Name} quickly sets about recruiting the rest of the party to help shore up the abandoned ritual circle, before attempting to make contact with the demon.", null);
                        result = opt3.Roll();
                        if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"A series of complicated clauses and entreaties soon follow as {opt3.Nominee.Name} negotiates with sinister, lascivious voice eminating from the {cursedItem.Name.CapitalizeEachWord()} {cursedItem.Illustration.IllustrationAsIconString}.", null);
                            await battle.Cinematics.NarratorLineAsync("And then, all at once before the party even fully realises what they've agreed to, a deal is struck. Each party members gains {b}Drained 2{/b}.", null);
                            foreach (Creature pm in GetParty(battle)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Drained", null, 2));
                            }
                            battle.CampaignState.CommonLoot.Add(cursedItem);
                        } else {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"And yet, {opt3.Nominee.Name}'s attempts to treat with the demon remain unanswered until the party reluctantly gives up and moves on, disappointed but quietly relieved.", null);
                        }
                        break;
                    case 3:
                        await battle.Cinematics.NarratorLineAsync($"With one last look at the ominous smoke curling off the {cursedItem.Name.CapitalizeEachWord()} {cursedItem.Illustration.IllustrationAsIconString} the party wisely moves on ahead, leaving the accursed item untouched.", null);
                        break;
                }
            }));

            events.Add(new SkillChallenge("Strange Mushrooms", async (level, battle) => {
                await battle.Cinematics.NarratorLineAsync("The party finds some weird mushrooms.", null);
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
                        await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} dilligently begins harvesting the strange mushrooms.", null);
                        result = opt1.Roll();
                        if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"{opt1.Nominee.Name} returns with several tonics of healing concocted from the strange mushroom's sap.", null);
                            if (level == 1) {
                                await battle.Cinematics.NarratorLineAsync($"The party gained a {{b}}lesser healing potion {Items.CreateNew(ItemName.LesserHealingPotion).Illustration.IllustrationAsIconString}.{{/b}}", null);
                                battle.CampaignState.CommonLoot.Add(Items.CreateNew(ItemName.LesserHealingPotion));
                            } else if (level == 2) {
                                await battle.Cinematics.NarratorLineAsync($"The party gained {{b}}lesser healing potion x2 {Items.CreateNew(ItemName.LesserHealingPotion).Illustration.IllustrationAsIconString}.{{/b}}", null);
                                battle.CampaignState.CommonLoot.Add(Items.CreateNew(ItemName.LesserHealingPotion));
                                battle.CampaignState.CommonLoot.Add(Items.CreateNew(ItemName.LesserHealingPotion));
                            } else if (level == 3) {
                                await battle.Cinematics.NarratorLineAsync($"The party gained a {{b}}moderate healing potion{{/b}} {Items.CreateNew(ItemName.ModerateHealingPotion).Illustration.IllustrationAsIconString} and a {{b}}lesser healing potion{{/b}} {Items.CreateNew(ItemName.LesserHealingPotion).Illustration.IllustrationAsIconString}.", null);
                                battle.CampaignState.CommonLoot.Add(Items.CreateNew(ItemName.ModerateHealingPotion));
                                battle.CampaignState.CommonLoot.Add(Items.CreateNew(ItemName.LesserHealingPotion));
                            }
                        } else {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"{opt1.Nominee.Name} was poised by a puff of spores!", null);
                            await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} has become sickened 1 for the duration of the next encounter.", null);
                            opt1.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Mushroom Sickness", null, 1));
                        }
                        break;
                    case 1:
                        await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} recreates the ritual to the best of their ability, before sitting down and pressing their head up against the mushroom.", null);
                        result = opt2.Roll();
                        if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The mushroom's trill happily, illuminating the cavern in eerie bioluminescent light as a thin cobweb of Mycelium roots emerge from the ground to form a cocoon around {opt2.Nominee.Name} as they kneel.", null);
                            await battle.Cinematics.NarratorLineAsync($"In exchange for spreading their spores to distant caverns, the mushrooms will cleans {opt2.Nominee.Name}'s body of harmful organisms. They gains the Mushroom Symbiote ability, protecting them from poisons for the rest of the adventure.", null);
                            opt2.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Mushroom Symbiote"));
                        } else {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The mushrooms are angered by {opt2.Nominee.Name}'s meager offering, expelling a large cloud of poisonous spores!", null);
                            await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} has become sickened 1 for the duration of the next encounter.", null);
                            opt2.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Mushroom Sickness", null, 2));
                        }
                        break;
                    case 2:
                        await battle.Cinematics.NarratorLineAsync($"The party decides to move on, letting the strange mushrooms be.", null);
                        break;
                }
            }));

            events.Add(new SkillChallenge("Drow Renegades", async (level, battle) => {
                await battle.Cinematics.NarratorLineAsync("You stumble upon a drow hunting party, adorned in strange luna iconography. After some initial tension, they reveal that they're renegade disciples of the Cerulean Sky, on a mission to liberate their people from the starborn.", null);
                await battle.Cinematics.NarratorLineAsync($"Yet suspicions still linger. Drow are known for their deceit, and many evil adventurer parties often delve into the {Loader.UnderdarkName} to barter for slaves or bargain for demonic boons.", null);
                await battle.Cinematics.NarratorLineAsync("To earn their trust and cooperate towards the two groups' mutual goals...", null);
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
                        await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} suggests that whilst they could hardly ask the reneges to divert resources from their own righteous cause, the two groups could at least share information about enemy movements and hazards they've encountered on their journey so far.", null);
                        result = opt1.Roll();
                        if (result == CheckResult.CriticalFailure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Despite {opt1.Nominee.Name}'s best efforts, the renegades remain tight-lipped, sharing only the vaguest of details about their movements.", null);
                            await battle.Cinematics.NarratorLineAsync($"The party is ready to give up, until one young paladin approaches them in private, seemingly disillusioned by her companion's paranoia and needless caution.", null);
                            await battle.Cinematics.NarratorLineAsync($"They talk for many hours, but it isn't until the two groups have long parted ways and the party begin to feel watchful eyes raising the hair on their necks, that they begin to realise her information is subtly but all too insidiously wrong...", null);
                            await battle.Cinematics.NarratorLineAsync("The enemy knows you're coming. Each member of the party gains {b}Compromised Route{/b}, reducing their inititive by 1.", null);
                            foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Compromised Route", null, null));
                            }
                        } else if (result == CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Despite {opt1.Nominee.Name}'s best efforts, the renegades remain tight-lipped, sharing only the vaguest of details about their movements.", null);
                            await battle.Cinematics.NarratorLineAsync($"It seems even between friends, trust in the {Loader.UnderdarkName} is difficult to find.", null);
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Though initially cautious, the renegades eventually warm up to {opt1.Nominee.Name} and before long the party finds themselves engrossed in conversation with the group, swapping war stories and critical intel alike.", null);
                            foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Information Sharing", null, null));
                            }
                        }
                        break;
                    case 1:
                        await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} speaks to the group of the surface world, and the beauty of the Cerulean Sky at night, insisting that their mission to foil the Demon Lord's plot is of utmost urgency to their goddess.", null);
                        result = opt2.Roll();
                        if (result == CheckResult.CriticalFailure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + "The group initially seems uncertain. Whilst they owe a great deal to their goddess, the weight of those still enthralled by the spider queen weighs heavy on their shoulders.", null);
                            await battle.Cinematics.NarratorLineAsync($"Yet at the last moment a young woman steps up from among their ranks, inspired by {opt2.Nominee.Name}'s words, offering to act as a guide for their journey.", null);
                            await battle.Cinematics.NarratorLineAsync($"It isn't until the group hears the clicking of mandibles, their earnest guide nowhere to be seen, that they realise too late that they've been led into a trap.", null);
                            await battle.Cinematics.NarratorLineAsync("Each member of the party gains {b}Injured 1{/b}, reducing their max HP by 10% until they rest.", null);
                            foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Injured", null, 1));
                            }
                        } else if (result == CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The renegades thank {opt2.Nominee.Name} for their kind and illuminating words, but reluctantly insist they cannot spare any among their group to aid them in their quest.", null);
                            await battle.Cinematics.NarratorLineAsync($"Whether through doubt or concern for their own difficult struggles, it's clear the party will find no succor here after all.", null);
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The renegades seem inspired by {opt2.Nominee.Name}'s words, one amongst their number steadfastly plunging her gleaming blade into the gloomy obsidian rocks as she pledges to hasten them on their journey.", null);
                            await battle.Cinematics.NarratorLineAsync($"A Drow Renegade will aid {opt2.Nominee.Name} in battle until she perishes, or the party returns to town.", null);
                            opt2.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Drow Renegade Companion", null, null));
                        }
                        break;
                    case 2:
                        await battle.Cinematics.NarratorLineAsync($"The renegades nod in grim agreement, seemingly no strangers to betrayal themselves.", null);
                        await battle.Cinematics.NarratorLineAsync($"Nevertheless, they volunteer a small amount of supplies to hasten the party on their journey, for it is a rare thing indeed to meet a group in the {Loader.UnderdarkName} willing to exchange pleasantries without an ulterior motive.", null);
                        await battle.Cinematics.NarratorLineAsync("The party obtained {b}" + level * 7 + " gold{/b} worth of supplies.", null);
                        break;
                }
            }));

            events.Add(new SkillChallenge("Escaped Slaves", async (level, battle) => {
                await battle.Cinematics.NarratorLineAsync("Coming from an opposing cavern, the party spots a group of bedraggled figures shambling towards them, fanning out like starving jackals with hungry sunken eyes.", null);
                await battle.Cinematics.NarratorLineAsync($"Dirty, emaciated and still wearing the remains of broken shackles, they can only be a group of escaped slaves. Yet even with a common enemy, the harsh environment of the {Loader.UnderdarkName} has little mercy for those who cannot take what they need to survive.", null);
                await battle.Cinematics.NarratorLineAsync("What does the party do?", null);
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
                        await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} steps confidently towards the encroaching mob, motioning for the party to draw steel and spell alike, daring them to come closer.", null);
                        result = opt1.Roll();
                        if (result <= CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The slaves eyes widen in panic, mistaking the attempt to scare them away as an attack... desperately converging on the party.", null);
                            await battle.Cinematics.NarratorLineAsync($"The escaped slaves are no match for a group of trained adventurers, and before long the rest are send fearfully scuyrrying away into the cavern... The cooling bodies of their friend's a grim reminder of the party's failure.", null);
                            await battle.Cinematics.NarratorLineAsync("Each member of the party gains {b}Guilt 2{/b}, reducing their Will saves by 2 until they rest.", null);
                            foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Guilt", null, 2));
                            }
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The mob fearfully skitters away from {opt1.Nominee.Name}, their hungry eyes lingering on the party's supply packs and then their weapons...", null);
                            await battle.Cinematics.NarratorLineAsync("The tense silence weighs heavy on the two groups for several moments, before they reluctantly shamble away.", null);
                        }
                        break;
                    case 1:
                        await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} meets the escapee's ravenous gazes with compassion, speaking of their common enemy and offering a map of the path they've cleared so far.", null);
                        result = opt2.Roll();
                        if (result <= CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Yet the {Loader.UnderdarkName} prove themselves too cruel for soft words and lofty ideals. Sensing weakness, the desperate slaves surge forwards, enveloping {opt2.Nominee.Name} before the party has time to step in.", null);
                            await battle.Cinematics.NarratorLineAsync(opt2.Nominee.Name + " gains {b}Injured 1{/b}, reducing their max HP by 10% until they rest.", null);
                            opt2.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Injured", null, 1));
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The group are scared, hungry and desperate… Yet {opt2.Nominee.Name}'s words remind them of who they used to be.", null);
                            await battle.Cinematics.NarratorLineAsync($"Thanking {opt2.Nominee.Name} for their kindness and directions they shuffle on, seeking refuge in Dawnsbury.", null);
                            await battle.Cinematics.NarratorLineAsync("Each member of the party gains {b}Hope 1{/b}, granting a +1 status bonus to their Will saves and attack bonus until they rest.", null);
                            foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Hope", null, 1));
                            }
                        }
                        break;
                    case 2:
                        await battle.Cinematics.NarratorLineAsync($"Despite the slave's aggression, the party offers what little they can all the same.", null);
                        await battle.Cinematics.NarratorLineAsync($"The slaves accept the party's offer of aid with wary eyes, before departing, unwilling to push their luck any further against an armed group.", null);
                        await battle.Cinematics.NarratorLineAsync("The party lost {b}" + level * 5 + " gold{/b}, but each member gains {b}Hope 1{/b}, granting a +1 status bonus to their Will saves and attack bonus until they rest.", null);
                        foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                            pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Hope", null, 1));
                        }
                        battle.CampaignState.CommonGold -= level * 5;
                        break;
                }
            }));

            events.Add(new SkillChallenge("Magical Traps", async (level, battle) => {
                await battle.Cinematics.NarratorLineAsync("Too late to doubleback, the party finds their path black by an abandoned Duergar stronghold built between a narrow passage.", null);
                await battle.Cinematics.NarratorLineAsync($"The arrow slits lie empty and dusty, yet the gates glow ominously with cruel arcane runes - no doubt set to unleash their terrible magic on any would-be intruders.", null);
                await battle.Cinematics.NarratorLineAsync("How does the party proceed?", null);
                battle.Cinematics.ExitCutscene();
                SCOption opt1 = GetBestPartyMember(battle, level, -2, Skill.Arcana);
                SCOption opt2 = GetBestPartyMember(battle, level, -2, Skill.Acrobatics);

                List<string> choices = new List<string>() {
                    $"{opt1.printInfoTag()} {opt1.Nominee.Name} thinks they might be able to disarm the magical traps.",
                    $"{opt2.printInfoTag()} {opt2.Nominee.Name} believes that a particular dexterous thief might be able to avoid the triggering mechanism and safely disable the traps from inside the fortress."
                };

                var choice = await CommonQuestions.OfferDialogueChoice(GetParty(battle).First(), GetNarrator(),
                    $"What does the party do?",
                    choices.ToArray()
                    );
                battle.Cinematics.EnterCutscene();
                CheckResult result = CheckResult.Failure;

                switch (choice.Index) {
                    case 0:
                        await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} quickly sets to work identifying the runes used in the traps, and sketching out how best to safely circumvent the magic.", null);
                        result = opt1.Roll();
                        if (result <= CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"{opt1.Nominee.Name} seems to be making good progress, until they encounter a rune of acid arrow cleverly disguised as a rune of stinking cloud...", null);
                            await battle.Cinematics.NarratorLineAsync($"The misstep triggers a chain reaction, leaving {opt1.Nominee.Name} severely injured.", null);
                            await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} gains Injured 2, reducing their max HP by 20% until they rest.", null);
                            opt1.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Injured", null, 2));
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"It takes several hour to completely dismantle the traps and ensure the Duergar haven't left any nasty surprises.", null);
                            await battle.Cinematics.NarratorLineAsync($"But when {opt1.Nominee.Name} is done, the party continues with several new scrolls to show for it, diligently copied from the traps.", null);

                            int itemLevel = battle.Encounter.CharacterLevel <= 2 ? 3 : 5;

                            Item scroll1 = LootTables.RollScroll(itemLevel, itemLevel, item => item.HasTrait(Trait.Evocation));
                            Item scroll2 = LootTables.RollScroll(itemLevel, itemLevel, item => item.HasTrait(Trait.Evocation));

                            await battle.Cinematics.NarratorLineAsync($"The party gained a {scroll1.Name} and {scroll2.Name}.", null);
                            battle.CampaignState.CommonLoot.Add(scroll1);
                            battle.CampaignState.CommonLoot.Add(scroll2);
                        }
                        break;
                    case 1:
                        await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} takes a moment to scope out the passage, before cautiously placing their foot upon a narrow deadzone in the defences, tottering perilously as the party waits with bated breath...", null);
                        result = opt2.Roll();
                        if (result <= CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The only warning {opt2.Nominee.Name} gets is a low buzzing drone, before the traps abruptly detonate.", null);
                            await battle.Cinematics.NarratorLineAsync(opt2.Nominee.Name + " gains {b}Injured 2{/b}, reducing their max HP by 20% until they rest.", null);
                            opt2.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Injured", null, 2));
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Yet the traps remain dormant as {opt2.Nominee.Name} continues to expertly leap, tip toe and crawl their way past the rest of the traps.", null);
                            await battle.Cinematics.NarratorLineAsync($"Inside, they quickly locate the mechanism to disable the traps, allowing the party to safely continue their journey.", null);
                        }
                        break;
                }
            }));

            events.Add(new SkillChallenge("Injured Unicorn", async (level, battle) => {
                await battle.Cinematics.NarratorLineAsync("As the party treks through the jagged stalagmites, they're drawn towards a soft, almost musical whinny of distress.", null);
                await battle.Cinematics.NarratorLineAsync($"Venturing closer, they soon locate the source of the disturbance - an injured unicorn foal, curling up in a rocky alcove, collapsed from a gorey wound upon its hind leg.", null);
                await battle.Cinematics.NarratorLineAsync("What could possibly have driven such a pure creature to wander into a place such as this?", null);
                battle.Cinematics.ExitCutscene();
                SCOption opt1 = GetBestPartyMember(battle, level, 2, Skill.Medicine, Skill.Nature);
                SCOption opt2 = GetBestPartyMember(battle, level, 2, (user, skill) => new NineCornerAlignment[] { NineCornerAlignment.LawfulGood, NineCornerAlignment.NeutralGood, NineCornerAlignment.ChaoticGood }.Contains(user.PersistentCharacterSheet.IdentityChoice.Alignment) ? 2 : 0, Skill.Diplomacy);
                SCOption opt3 = GetBestPartyMember(battle, level, -4, Skill.Arcana, Skill.Occultism);

                List<string> choices = new List<string>() {
                    $"{opt1.printInfoTag()} {opt1.Nominee.Name} believes they might be able to nurse the creature back to health",
                    $"{opt2.printInfoTag()} {opt2.Nominee.Name} suggests beseeching the unicorn to use the last of its strength to aid the party with a blessing.",
                    $"{opt3.printInfoTag()} With an uneasy glance, {opt3.Nominee.Name} apprehensively mentions that the dying creature's horn could be used to forge a powerful Alicorn weapon."
                };

                var choice = await CommonQuestions.OfferDialogueChoice(GetParty(battle).First(), GetNarrator(),
                    $"Kneeling around the injured creature, the party ponders their options.",
                    choices.ToArray()
                );
                battle.Cinematics.EnterCutscene();
                CheckResult result = CheckResult.Failure;

                switch (choice.Index) {
                    case 0:
                        await battle.Cinematics.NarratorLineAsync($"Reaching out to gently stroke the creature's majestic main, {opt1.Nominee.Name} tries their best to sooth the creature and tend to its wounds.", null);
                        result = opt1.Roll();
                        if (result <= CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Despite {opt1.Nominee.Name}'s best efforts and many long hours spent sitting with the creature, the poor creature has already lost too much blood, the infection spread too far up the weakening creature's body.", null);
                            await battle.Cinematics.NarratorLineAsync($"The party stays with it till it's final moments, head resting up {opt1.Nominee.Name}'s lap until it finally grows still.", null);
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The unicorn whinnies in pain as {opt1.Nominee.Name} cauterises the infection from its wound and bandages its leg, nursing the creature back to health with what meager paultices can be produced from the scant fungal life that struggles to grow in the Below.", null);
                            await battle.Cinematics.NarratorLineAsync($"Though things seem uncertain at times, the unicorn eventually begins to regain its celestial glow... Finally regaining enough strength to magically restore itself back to health.", null);
                            await battle.Cinematics.NarratorLineAsync($"Nuzzling up to {opt1.Nominee.Name}'s cheek fondly, it seems determined to repay the party by fighting along their side until they're able to safely guide it back to the surface.", null);
                            await battle.Cinematics.NarratorLineAsync($"The Unicorn Foal will aid {opt1.Nominee.Name} in battle until it perishes, or the party returns to town.", null);
                            opt1.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Unicorn Companion", null, null));
                        }
                        break;
                    case 1:
                        await battle.Cinematics.NarratorLineAsync($"With a regretful look towards the creature's nasty wound, {opt2.Nominee.Name} kneels beside the creature, beseeching it to use the last of its strength to help them avenge its death.", null);
                        result = opt2.Roll();
                        if (result <= CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"But the creature merely shakes its head, clinging to some greater, unknown purpose - or perhaps simply finding {opt2.Nominee.Name} unworthy of such a boon" +
                                " - as it rises to its feet and limps off into the caverns to die.", null);
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The creature's big sorrowful eyes harden into determination as it listens to {opt2.Nominee.Name}'s request, before squeezing its eyes shut in concentration.", null);
                            await battle.Cinematics.NarratorLineAsync("The beautiful creature seems to glow momentarily with a warm light that bathes the party... When it finally fades, the creature's eyes do not open.", null);
                            await battle.Cinematics.NarratorLineAsync("The party gains 'Unicorn Blessing', increasing their max HP by 5 and their saving throws by 1 until they return to town.", null);
                            foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                                pm.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Unicorn's Blessing", null, null));
                            }
                        }
                        break;
                    case 2:
                        await battle.Cinematics.NarratorLineAsync($"Kneeling by the creature, {opt3.Nominee.Name} apprehensively attempts to soothe the creature with one hand, while raising their knife in the other.", null);
                        await battle.Cinematics.NarratorLineAsync($"Moments before they bring the blade down, the creature startles - emitting a hard purple light, cursing {opt3.Nominee.Name} for their cruelty even as they bring the knife down into the pure creature's jugular.", null);
                        result = opt3.Roll();
                        if (result <= CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"Yet despite the terrible deed, it yields no spoils. Without the proper tools and with few smiths willing to work with such forbidden materials, {opt3.Nominee.Name} is unable to craft the alicorn into a usable weapon.", null);
                            await battle.Cinematics.NarratorLineAsync("The unicorn died for nothing.", null);
                            await battle.Cinematics.NarratorLineAsync($"{opt3.Nominee.Name} was inflicted by a Unicorn's Curse, reducing their max HP by 5, and their saving throws by 1 until their next long rest.", null);
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The party watches on guiltily as {opt3.Nominee.Name} sets about carving up the majestic creature and inscribing the forbidden runes required to forge the poached spoils into a righteous Alicorn armament.", null);
                            var chosenWeapon = await CommonQuestions.OfferDialogueChoice(opt3.Nominee, GetNarrator(),
                                $"{opt3.Nominee} uses the unicorn's carcass to forge...",
                                $"An easily handled Alicorn Dagger. {Illustrations.AlicornDagger.IllustrationAsIconString}", $"A stout Alicorn Pike for the martially inclined. {Illustrations.AlicornPike.IllustrationAsIconString}"
                            );
                            await battle.Cinematics.NarratorLineAsync($"The party gains an Alicorn {(chosenWeapon.Index == 0 ? "Dagger" : "Pike")}, but {opt3.Nominee.Name} was inflicted by a Unicorn's Curse, reducing their max HP by 5, and their saving throws by 1 until they return to turn.", null);
                            Item AlicornWeapon;
                            if (chosenWeapon.Index == 0) {
                                AlicornWeapon = Items.CreateNew(CustomItems.AlicornDagger);
                            } else {
                                AlicornWeapon = Items.CreateNew(CustomItems.AlicornPike);
                            }
                            if (level == 2) {
                                AlicornWeapon = AlicornWeapon.WithModificationPlusOne();
                            }
                            if (level >= 3) {
                                AlicornWeapon = AlicornWeapon.WithModificationPlusOneStriking();
                            }
                            battle.CampaignState.CommonLoot.Add(AlicornWeapon);
                        }
                        opt3.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Unicorn's Curse", null, null));
                        break;
                }
            }));

            events.Add(new SkillChallenge("The Rat Fiend's Offer", async (level, battle) => {
                List<(List<Item>, Item)> items = new List<(List<Item>, Item)>();
                foreach (Creature pm in battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null)) {
                    if (pm.CarriedItems.Count > 0)
                        items.Add((pm.CarriedItems, pm.CarriedItems.MaxBy(i => i.Price)));
                }
                if (battle.CampaignState.CommonLoot.Count > 0)
                    items.Add((battle.CampaignState.CommonLoot, battle.CampaignState.CommonLoot.MaxBy(i => i.Price)));

                Item? wageredItem = null;
                List<Item> container = null;
                if (items.Count > 0) {
                    var temp = items.MaxBy(tuple => tuple.Item2.Price);
                    wageredItem = temp.Item2;
                    container = temp.Item1;
                }

                await battle.Cinematics.NarratorLineAsync("As the party ventures forth into the murky caverns, they notice a strange carnival tent off to the side of the passageway that they swear hadn't been there just a moment before...", null);
                await battle.Cinematics.NarratorLineAsync($"Upon cautiously peering inside its flaps, they spy a looming rodent-like creature grinning back at them from atop a pile shiny trinkets - a table with two sets of playing cards filling the gulf between it and the party.", null);

                if (wageredItem == null) {
                    await battle.Cinematics.NarratorLineAsync($"Yet upon seeing them, the creature merely sneers and tells them to be gone. It seems they do not possess the treasure neccessary to place a wager in the foul creature's game.", null);
                    return;
                }

                await battle.Cinematics.NarratorLineAsync($"The thing is clearly a demon, yet its offer cannot be ignored. The terms are simple, a game of skill, guile and chance. If the party wins, it will share its power to help them on their journey. If they lose, they must surrender their {wageredItem.Name} to the fiend's collection.", null);
                battle.Cinematics.ExitCutscene();
                SCOption opt1 = GetBestPartyMember(battle, level, 1, Skill.Deception);
                SCOption opt2 = GetBestPartyMember(battle, level, 3, Skill.Thievery);
                SCOption opt3 = GetBestPartyMember(battle, level, -2, Skill.Religion);

                List<string> choices = new List<string>() {
                    $"{opt1.printInfoTag()} {opt1.Nominee.Name} believes they should accept the fiend's challenge, wager their {wageredItem.Name} for a chance at demonic power.",
                    $"{opt2.printInfoTag()} {opt2.Nominee.Name} suggests using some sleight of hand to rig the game in their favour.",
                    $"{opt3.printInfoTag()} {opt3.Nominee.Name} claims no good can come of dealing with demons. The fiend must be banished so that it might tempt travellers no more.",
                    "The party declines the suspicious creature's offer."
                };

                var choice = await CommonQuestions.OfferDialogueChoice(GetParty(battle).First(), GetNarrator(),
                    $"What do they do?",
                    choices.ToArray()
                );
                battle.Cinematics.EnterCutscene();
                CheckResult result = CheckResult.Failure;

                switch (choice.Index) {
                    case 0:
                        await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} boldly thunks the {wageredItem.Name} down on the small table, accepting the creature's wager.", null);
                        result = opt1.Roll();
                        if (result <= CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"{opt1.Nominee.Name} is a shrewd player, diligently stumbling through the rules of the odd card game, but the thing is better.", null);
                            await battle.Cinematics.NarratorLineAsync($"After several tense bouts, {opt1.Nominee.Name} is soon completely out of bone chips... And then quick as lightning, the creature looms high with victorious grin on its muzzle, snatches up the {wageredItem.Name}...", null);
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"...And the party finds themselves once again standing in the cold open cavern, with no tent or rodent-like demon in tight.", null);
                            await battle.Cinematics.NarratorLineAsync($"The party's {wageredItem.Name} has been lost.", null);
                            container.Remove(wageredItem);
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"It doesn't take long to realise the game of bluffing suggested by the creature is clearly rigged in its favour. For what mortal could best a demon of alien mannerisms and tells in a game of deceit?", null);
                            await battle.Cinematics.NarratorLineAsync($"And yet, the creature's creed and hubris is ultimately its downfall. Feigning, {opt1.Nominee.Name} lures the thing into a false sense of security, overplaying its hand so that it might claim its prize all the sooner.", null);
                            await battle.Cinematics.NarratorLineAsync($"And all at once, a large stack of bone chips are reluctantly swept towards their side of the table.", null);
                            await battle.Cinematics.NarratorLineAsync($"Yet the foul creature's grin only seems to grow larger, as its claw extends to bequeath its strange power upon them, as if this, too, was its true intention all along...", null);
                            await battle.Cinematics.NarratorLineAsync($"{opt1.Nominee.Name} has won the power of the rat fiend, drawing rat familiars forth to serve them and allowing them to retrain into the Rat Monarch archetype.", null);
                            if (!opt1.Nominee.PersistentCharacterSheet.Calculated.Sheet.SelectedFeats.ContainsKey("Power of the Rat Fiend"))
                                opt1.Nominee.PersistentCharacterSheet.Calculated.Sheet.SelectedFeats.Add("Power of the Rat Fiend", new FeatSelectedChoice(AllFeats.All.First(ft => ft.FeatName == FeatLoader.PowerOfTheRatFiend), null));
                            opt1.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Power of the Rat Fiend", null, null));
                        }
                        break;
                    case 1:
                        await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} slyly accepts the creature's challenge, discreetly slipping one of the esoteric playing cards up their sleeve as they sit down.", null);
                        result = opt2.Roll();
                        if (result <= CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"But when the critical moment comes to swap out the card, the creature's beady eyes lock onto the offending card with a screech of feral rage!", null);
                            await battle.Cinematics.NarratorLineAsync($"The tent begins to dissolve, deforming into swarm of rats that nibble and crawl across the party, as the creature's eyes glow with baleful power, as it proclaims {opt2.Nominee.Name} a cheater!", null);
                            await battle.Cinematics.NarratorLineAsync($"The rats, along with the demon and its treasure vanish as quickly as they come. And yet {opt2.Nominee.Name} is left with a feeling of great dread, their eyes twitching towards the dark corners of the cavern in paranoia, as if something is watching them from the shadows.", null);
                            await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} has been inflicted by the Rat Fiend's Curse, facing a 25% chance for a Giant Rat to crawl out of the corpse of any enemy they defeat until their next rest.", null);
                            opt2.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Curse of the Rat Fiend", null, null));
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"It doesn't take long to realise the game of bluffing suggested by the creature is clearly rigged in its favour. For what mortal could best a demon of alien mannerisms and tells in a game of deceit?", null);

                            await battle.Cinematics.NarratorLineAsync($"And yet, but {opt2.Nominee.Name} isn't fool enough to play fair either. The game is close, but within the odds against it, {opt2.Nominee.Name}'s superior guile and sleight of hand makes victory an inevitability.", null);
                            await battle.Cinematics.NarratorLineAsync($"Yet the foul creature's grin only seems to grow larger, as its claw extends to bequeath its strange power upon them, as if this, too, was its true intention all along...", null);
                            await battle.Cinematics.NarratorLineAsync($"{opt2.Nominee.Name} has won the power of the rat fiend, drawing rat familiars forth to serve them and allowing them to retrain into the Rat Monarch archetype.", null);
                            if (!opt2.Nominee.PersistentCharacterSheet.Calculated.Sheet.SelectedFeats.ContainsKey("Power of the Rat Fiend"))
                                opt2.Nominee.PersistentCharacterSheet.Calculated.Sheet.SelectedFeats.Add("Power of the Rat Fiend", new FeatSelectedChoice(AllFeats.All.First(ft => ft.FeatName == FeatLoader.PowerOfTheRatFiend), null));
                            opt2.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Power of the Rat Fiend", null, null));
                        }
                        break;
                    case 2:
                        await battle.Cinematics.NarratorLineAsync($"Brooking no patience for the poisoned words of demons, {opt3.Nominee.Name} raises their holy symbol, urging the party to help them banish whatever sorcery has brought this creature to them.", null);
                        result = opt3.Roll();
                        if (result <= CheckResult.Failure) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The creature shrieks in rage, fighting {opt3.Nominee.Name} at every inch - pitting its sinister will against their sacred conviction, as the tent dissolves around them.", null);
                            await battle.Cinematics.NarratorLineAsync($"The creature is eventually beaten back, but not before lunging towards {opt3.Nominee.Name} and sinking its rotten teeth deep into their arm.", null);
                            await battle.Cinematics.NarratorLineAsync($"When all is said and done, instead of an oozing wound, there is only the cursed mark of a snarling rat tattooed into their flesh.", null);
                            await battle.Cinematics.NarratorLineAsync($"{opt3.Nominee.Name} has been inflicted by the Rat Fiend's Curse, facing a 25% chance for a Giant Rat to crawl out of the corpse of any enemy they defeat until their next long rest.", null);
                            opt3.Nominee.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Curse of the Rat Fiend", null, null));
                        } else if (result >= CheckResult.Success) {
                            await battle.Cinematics.NarratorLineAsync(PrintResult(result) + $"The fiend screeches in agony before {opt2.Nominee.Name}'s holy symbol - the very fabric of the tent bursting apart into swarms of grotesquely gauged rats as its sorcery is undone!", null);
                            await battle.Cinematics.NarratorLineAsync($"With one last forlorn look towards its treasure, the demon cravenly skitters away, leaving its ill gotten horde behind for the taking.", null);
                            int gold = 30 * level;
                            await battle.Cinematics.NarratorLineAsync($"The party gains {gold} gold from the various coins and shiny trinkets in the creature's treasure hoard.", null);
                            battle.CampaignState.CommonGold += 30;
                            battle.Encounter.RewardGold = 30;
                        }
                        break;
                    case 3:
                        await battle.Cinematics.NarratorLineAsync("The party politely declines the creature's offer. The demon's only response is to cackle wickedly as they depart. When they looks back, the tent and all traces of the creature are gone.", null);
                        break;
                }
            }));
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
                colour = "Green"; //"Chartreuse";
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
            CheckResult output = CommonSpellEffects.RollCheck("Skill Challenge", new ActiveRollSpecification(TaggedChecks.SkillCheck(Skill), Checks.FlatDC(DC)), Nominee, Nominee);
            bonus.ExpiresAt = ExpirationCondition.Immediately;
            return output;
        }
    }
}
