using System;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Core.Creatures.Parts;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level4
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class SuccubusCult : NormalEncounter {


        public SuccubusCult(string filename) : base("Succubus Cult", filename) {

            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                await base.Setup(battle);
                Creature succubus = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy && cr.CreatureId == CreatureId.Succubus);
                succubus!.Spellcasting!.PrimarySpellcastingSource?.Spells.RemoveFirst(spell => spell.SpellId == SpellId.Dominate);
                succubus.Spellcasting.PrimarySpellcastingSource?.WithSpells([SpellLoader.LesserDominate], 5);
                succubus.Spellcasting.PrimarySpellcastingSource?.WithSpells([SpellLoader.LesserDominate], 5);
            });
        }

    }
}
