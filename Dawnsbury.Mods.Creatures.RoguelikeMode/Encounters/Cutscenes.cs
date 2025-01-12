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

    }
}