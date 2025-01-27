using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using FmodForFoxes;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class Cutscenes {

        public async static Task HatcheryCutscene(TBattle battle) {
            Creature guard = battle.AllCreatures.First(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Drow Fighter");
            guard.Subtitle = "Hatchery Guard";
            battle.Cinematics.EnterCutscene();
            await battle.Cinematics.LineAsync(guard, "Who goes there!? Ah... Adventurers.", null);
            await battle.Cinematics.LineAsync(guard, "In the undercity, each and every drow child is taught that some secrets are best left uncovered...", null);
            await battle.Cinematics.LineAsync(guard, "It's a pity you weren't taught the same. But now I'm afraid the secrets of this facility will die with you.", null);
            await battle.Cinematics.LineAsync(guard, "Best let me grant you a quick death... Before they start to {i}hatch{/i}.", null);
            battle.Cinematics.ExitCutscene();

            List<Creature> eggs = battle.AllCreatures.Where(cr => cr.Illustration == Illustrations.DemonicPustule).ToList();
            for (int i = 0; i < 6; i++) {
                eggs.Remove(eggs.GetRandom());
            }
            foreach (Creature egg in eggs) {
                battle.RemoveCreatureFromGame(egg);
            }
        }

        public async static Task HatcheryCutscene2(TBattle battle) {
            if (battle.RoundNumber > 1) {
                if (battle.RoundNumber == 4) {
                    var doors = battle.AllCreatures.Where(cr => cr.CreatureId == Core.Creatures.Parts.CreatureId.Door).ToList();
                    while (doors.Count() > 0) {
                        Creature door = doors[0];
                        doors.Remove(door);
                        await door.DieFastAndWithoutAnimation();
                    }
                }
                foreach (Creature egg in battle.AllCreatures.Where(cr => cr.Alive && cr.Illustration == Illustrations.DemonicPustule && cr.Occupies.FogOfWar == Core.Tiles.FogOfWar.Blackened)) {
                    QEffect? effect = egg.QEffects.FirstOrDefault(qf => qf.Name == "Incubator");
                    if (effect != null) {
                        effect.Value -= 1;
                        await battle.GameLoop.StateCheck();
                    }
                }
            }

        }

        public async static Task AntipartyCutscene(TBattle battle) {
            Creature priestess = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Drow Priestess");
            priestess.MainName = "Princess Melantha";
            priestess.Subtitle = "High Priestess and First Princess of House Vextra";

            Creature inquisitrix = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Drow Inquisitrix");
            inquisitrix.MainName = "Princess Amethyst";
            inquisitrix.Subtitle = "Royal Torturer and Second Princess of House Vextra";

            Creature mage = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && (cr.BaseName == "Drow Shadowcaster" || cr.BaseName ==  "Drow Arcanist"));
            mage.MainName = "Prince Valdis";
            mage.Subtitle = "Court Mage and First Prince of House Vextra";

            Creature guard = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Drow Temple Guard");
            guard.MainName = "Royal Protector Kauth";
            guard.Subtitle = "Royal Protector of House Vextra";

            battle.Cinematics.EnterCutscene();
            Sfxs.SlideIntoSong(Songname.HighTensionBegins);
            await battle.Cinematics.LineAsync(priestess, "Ah, we meet at last meddlesome adventurers! But now it is time you met a real adventuring party.", null);
            await battle.Cinematics.LineAsync(inquisitrix, "Yes! One that stands for important adventuring values, like inter-family drama, themes of dark academia and killing people for exciting loot.", null);
            await battle.Cinematics.LineAsync(mage, "And sexy, flamboyant brooding~", null);
            await battle.Cinematics.LineAsync(priestess, "Now wallow in regret at how much cooler our party is, as you face the scions of House Vextra!", null);
            await battle.Cinematics.LineAsync(inquisitrix, "I demand their heads at once Kauth!", null);
            await battle.Cinematics.LineAsync(guard, "Of course, princess.", null);
            battle.Cinematics.ExitCutscene();
        }

        public async static Task HandmaidenCutscene(TBattle battle) {
            Creature handmaiden = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Abyssal Handmaiden");
            handmaiden.Subtitle = "Handmaiden of the Spider Queen";

            battle.Cinematics.EnterCutscene();
            Sfxs.SlideIntoSong(Songname.HighTensionBegins);
            await battle.Cinematics.LineAsync(handmaiden, "Ah, welcome adventurers. You're just in time! I was beginning to worry you'd miss the Queen's big baby shower... I spent a lot of time setting this up for her.", null);
            await battle.Cinematics.LineAsync(handmaiden, "You know it was actually in this very chamber that my Queen was born. Cast out by the Gods for daring to ask what dirty little secrets they were hiding on the Other Side.", null);
            await battle.Cinematics.LineAsync(handmaiden, "So really, it's only fitting that the first of her children should bubble up and take their revenge upon the surface world from here...", null);
            await battle.Cinematics.LineAsync(handmaiden, "And you're just in time for their very first meal! How exciting! Perhaps one of you will even be lucky enough to become my new meat suit... It's a bit embarrassing but this one's starting to show a little too much leg...", null);
            Sfxs.SlideIntoSong(SoundEffects.BossMusic);
            battle.Cinematics.ExitCutscene();
        }

        public async static Task WitchCoven(TBattle battle) {
            Creature crone = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Agatha Agaricus");
            crone.Subtitle = "Crone of the Wilds";

            Creature maiden = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Harriet Hex");
            maiden.Subtitle = "Maiden of the Lost";

            Creature mother = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Mother Cassandra");
            mother.Subtitle = "Mother of the Pool";

            battle.Cinematics.EnterCutscene();
            Sfxs.SlideIntoSong(SoundEffects.VikingMusic);
            await battle.Cinematics.LineAsync(mother, "Double, double toil and trouble.", null);
            await battle.Cinematics.LineAsync(maiden, "Fire burn and cauldron bubble.", null);
            await battle.Cinematics.LineAsync(crone, "For a charm of powerful trouble.", null);

            if (battle.CampaignState != null && battle.CampaignState.AdventurePath.CampaignStops.Any(stop => stop.Name == "Mother of the Pool")) {
                await battle.Cinematics.LineAsync(mother, "Have you reconsidered my offer, children? Face me upon the hour of the witch at your peril.", null);
            }
            if (battle.CampaignState != null && battle.CampaignState.AdventurePath.CampaignStops.Any(stop => stop.Name == "Maiden of the Lost")) {
                await battle.Cinematics.LineAsync(maiden, "I'm sorry our paths must cross again like this. Alas I must keep to the word of my contract. I hope you understand.", null);
            }
            if (battle.CampaignState != null && battle.CampaignState.AdventurePath.CampaignStops.Any(stop => stop.Name == "Crone of the Wilds")) {
                await battle.Cinematics.LineAsync(crone, "Haha! I told you you hadn't seen the last of old Agatha, vermin!", null);
            }
            await battle.Cinematics.LineAsync(mother, "I curse thee once with the doubt of your hubris.", null);
            await battle.Cinematics.LineAsync(maiden, "I curse thee twice with the pain of this unjust world.", null);
            await battle.Cinematics.LineAsync(crone, "And I curse thee thrice with the enmity of the unspoiled wilds. May it pick your bones clean before it bubbles up to retake what has been despoiled by man.", null);

            battle.Cinematics.ExitCutscene();
        }

        public async static Task MaidenOfTheLost(TBattle battle) {
            Creature maiden = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Harriet Hex");
            maiden.Subtitle = "Maiden of the Lost";

            battle.Cinematics.EnterCutscene();
            await battle.Cinematics.LineAsync(maiden, "Welcome to my sanctuary adventurers. There are many a soul in the Below who've met a gruesome fate, I'm afraid. Truly, it is not a place mortals were meant to tread.", null);
            await battle.Cinematics.LineAsync(maiden, "But at least here, in my sanctuary, they can find peace.", null);
            await battle.Cinematics.LineAsync(maiden, "And perhaps with the help of the linelines, even those from the world above. When the time comes.", null);
            await battle.Cinematics.LineAsync(maiden, "But you, brave souls, may rest well knowing you'll have my personal assistance in moving onto a... better place. No matter how gruesome a death I shall be forced to afflict upon you.", null);
            await battle.Cinematics.LineAsync(maiden, "I... I really am quite sorry.", null);
            Sfxs.SlideIntoSong(Songname.HighTensionBegins);
            battle.Cinematics.ExitCutscene();
        }

        public async static Task MotherOfThePool(TBattle battle) {
            Creature witch = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Mother Cassandra");
            witch.Subtitle = "Mother of the Pool";

            battle.Cinematics.EnterCutscene();
            Sfxs.SlideIntoSong(SoundEffects.MotherOfThePoolTheme);
            await battle.Cinematics.LineAsync(witch, "Welcome children... Please, come. Sit. All are welcome in my church. Have you come seeking enlightenment from my waters?", null);
            await battle.Cinematics.LineAsync(witch, "No? Ah. A pity.", null);
            await battle.Cinematics.LineAsync(witch, "It would seem the false gods have sent their champions to test us, my flock.", null);
            await battle.Cinematics.LineAsync(witch, "Do not be afraid, for you have drank of my waters. Recite the chant as we practiced. Embrace the touch of your true goddess, my chosen disciples.", null);
            battle.Cinematics.ExitCutscene();
        }

        public async static Task CroneOfTheWilds(TBattle battle) {
            Creature witch = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Agatha Agaricus");
            witch.Subtitle = "Crone of the Wilds";

            battle.Cinematics.EnterCutscene();
            Sfxs.SlideIntoSong(SoundEffects.VikingMusic);
            await battle.Cinematics.LineAsync(witch, "Welcome, my pretties, to my oasis. Ah, but this place is not for the likes of you.", null);
            await battle.Cinematics.LineAsync(witch, "I would send you on your way, but the grass grows brittle, and my wolves hungry... A pity the drow and their runaways have since learned not to come this way anymore.", null);
            await battle.Cinematics.LineAsync(witch, "But you pretty little morsels will do just fine.", null);
            battle.Cinematics.ExitCutscene();
        }
    }
}