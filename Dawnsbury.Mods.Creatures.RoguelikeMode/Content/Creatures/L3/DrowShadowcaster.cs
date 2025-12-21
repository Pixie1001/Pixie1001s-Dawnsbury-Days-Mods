using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowShadowcaster {
        public static Creature Create() {
            return new Creature(Illustrations.DrowShadowcaster, "Drow Shadowcaster", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid, ModTraits.SpellcasterMutator }, 3, 9, 6, new Defenses(17, 6, 9, 12), 31,
            new Abilities(1, 3, 0, 4, 1, 2), new Skills(acrobatics: 10, intimidation: 11, arcana: 13, deception: 11))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;
                    AiFuncs.PositionalGoodness(creature, options, (t, crSelf, step, cr) => (step || crSelf.Occupies == t) && t.IsAdjacentTo(cr.Occupies) && cr.OwningFaction.EnemyFactionOf(crSelf.OwningFaction), -0.2f, false);
                    AiFuncs.AverageDistanceGoodness(creature, options, cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction), cr => cr.OwningFaction.AlliedFactionOf(creature.OwningFaction), -15, 5);

                    return null;
                };
            })
            .WithCreatureId(CreatureIds.DrowShadowcaster)
            .WithProficiency(Trait.Melee, Proficiency.Trained)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(ItemName.RepeatingHandCrossbow))
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(CommonQEffects.SlipAway())
            .AddQEffect(new QEffect("Dark Arts", "The drow shadwcaster excels at causing pain with their black practice. Their non-cantrip spells gain a +4 status bonus to damage.") {
                BonusToDamage = (qfSelf, spell, target) => {
                    return spell.HasTrait(Trait.Spell) && !spell.HasTrait(Trait.Cantrip) && !spell.HasTrait(Trait.Focus) && spell.CastFromScroll == null ? new Bonus(4, BonusType.Status, "Dark Arts") : null;
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Wizard, Ability.Intelligence, Trait.Arcane).WithSpells(
            new SpellId[] { SpellId.MagicMissile, SpellId.MagicMissile, SpellId.GrimTendrils, SpellId.Fear, SpellId.RayOfFrost, SpellId.ProduceFlame, SpellId.Shield },
            new SpellId[] { SpellId.AcidArrow, SpellId.AcidArrow, SpellId.HideousLaughter }).Done();
        }
    }
}