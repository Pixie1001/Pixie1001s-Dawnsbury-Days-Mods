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
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Creatures.Parts;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level4
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class SplitTheParty : NormalEncounter {


        public SplitTheParty(string filename) : base("Lost", filename) {

            this.ReplaceTriggerWithCinematic(TriggerName.InitiativeCountZero, async battle => {
                await base.Setup(battle);
                if (battle.RoundNumber == 1) {
                    battle.AllCreatures
                    .Where(cr => cr.CreatureId == CreatureId.Door && cr.Occupies != null && CommonEncounterFuncs.DistanceToNearestPartyMember(cr.Occupies, battle) <= 2)
                    .ToList()
                    .ForEach(cr => {
                        Sfxs.Play(SfxName.OpenLock);
                        cr.DieFastAndWithoutAnimation();
                    });
                }
            }); 
        }

    }
}
