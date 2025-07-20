using System;
using Dawnsbury;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Tiles;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using System.IO;
using System.Text.Json.Nodes;
using System.Reflection.Metadata;
using Microsoft.VisualBasic.FileIO;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class SkillChallenge {
        public string Name { get; }

        private Func<int, TBattle, Task> cutscene;

        public SkillChallenge(string name, Func<int, TBattle, Task> cutscene) {
            this.Name = name;
            this.cutscene = cutscene;
        }

        public async Task Run(int level, TBattle battle) {
            await RunSetup(battle);
            await cutscene(level, battle);
            await RunCleanup(battle);
        }

        private static async Task RunSetup(TBattle battle) {
            //battle.AllCreatures.ForEach(cr => cr.IllustrationIsHiddenWhenCohesiveMapBackground = true);
            //battle.SmartCenterAlways(battle.AllCreatures.First(cr => cr.PersistentCharacterSheet == battle.CampaignState.Heroes[0].CharacterSheet).Occupies);
            battle.SmartCenterAlways(battle.Map.AllTiles.First(tile => tile.Kind == TileKind.Tree));
            battle.Cinematics.EnterCutscene();
            Sfxs.Play(SfxName.ScratchFlesh);
        }

        private static async Task RunCleanup(TBattle battle) {
            battle.Cinematics.ExitCutscene();
        }
    }




}
