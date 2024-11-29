using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
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
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class Cutscenes {

        public async static Task AntipartyCutscene(TBattle battle) {
            Creature priestess = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Drow Priestess");
            priestess.MainName = "Princess Melantha";
            priestess.Subtitle = "High Priestess and First Princess of House Vextra";

            Creature inquisitrix = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Drow Inquisitrix");
            inquisitrix.MainName = "Princess Amethyst";
            inquisitrix.Subtitle = "Royal Torturer and Second Princess of House Vextra";

            Creature mage = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Drow Shadowcaster");
            mage.MainName = "Prince Valdis";
            mage.Subtitle = "Court Mage and First Prince of House Vextra";

            Creature guard = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.BaseName == "Drow Temple Guard");
            guard.MainName = "Royal Protector Kauth";
            guard.Subtitle = "Royal Protector of House Vextra";

            battle.Cinematics.EnterCutscene();
            Sfxs.SlideIntoSong(Songname.HighTensionBegins);
            await battle.Cinematics.LineAsync(priestess, "Ah, we meet at last meddlesome adventurers! But now it is time you met a real adventuring party.");
            await battle.Cinematics.LineAsync(inquisitrix, "Yes! One that stands for important adventuring values, like inter-family drama, themes of dark academia and killing people for exciting loot.");
            await battle.Cinematics.LineAsync(mage, "And sexy, flamboyant brooding~");
            await battle.Cinematics.LineAsync(priestess, "Now wallow in regret at how much cooler our party is, as you face the scions of House Vextra!");
            await battle.Cinematics.LineAsync(inquisitrix, "I demand their heads at once Kauth!");
            await battle.Cinematics.LineAsync(guard, "Of course, princess.");
            battle.Cinematics.ExitCutscene();
        }

    }
}