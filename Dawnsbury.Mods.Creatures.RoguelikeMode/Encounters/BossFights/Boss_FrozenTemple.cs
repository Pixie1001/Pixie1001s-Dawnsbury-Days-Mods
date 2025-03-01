using System;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Campaign.Encounters;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Boss_FrozenTemple : BossFightEncounter {
        public Boss_FrozenTemple(string filename) : base("Fozen Temple", filename)
        {
            // Run setup
            this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
                Creature? dragon = battle.AllCreatures.FirstOrDefault(creature => creature.BaseName == "Young White Dragon");
                if (dragon != null)
                {
                    // Dragon Undead changes
                    dragon.MainName = "Undead " + dragon.MainName;
                    dragon.Traits.Add(Trait.Undead);
                    dragon.Traits.Add(Trait.Mindless);
                    dragon.AddQEffect(QEffect.TraitImmunity(Trait.Death));
                    dragon.AddQEffect(QEffect.TraitImmunity(Trait.Disease));
                    dragon.AddQEffect(QEffect.TraitImmunity(Trait.Poison));
                    dragon.AddQEffect(QEffect.TraitImmunity(Trait.Mental));
                    dragon.AddQEffect(QEffect.ImmunityToCondition(QEffectId.Unconscious));
                    QEffect reanimatedRecharge = QEffect.Recharging("Breath Weapon");
                    reanimatedRecharge.Value = 1;
                    dragon.AddQEffect(reanimatedRecharge);
                }
            });
        }
    }
}
