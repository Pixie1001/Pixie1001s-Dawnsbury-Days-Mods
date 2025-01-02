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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowPriestess
    {
        public static Creature Create()
        {
            return new Creature(Illustrations.DrowPriestess, "Drow Priestess", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid, Trait.Female }, 3, 9, 6, new Defenses(20, 8, 7, 11), 39,
            new Abilities(1, 2, 1, 0, 4, 2), new Skills(deception: 9, stealth: 7, intimidation: 9))
            .WithAIModification(ai =>
            {
                ai.OverrideDecision = (self, options) =>
                {
                    Creature creature = self.Self;

                    // Bane AI
                    foreach (Option option in options.Where(o => o.Text == "Bane"))
                    {
                        option.AiUsefulness.MainActionUsefulness = 30;
                    }
                    Option? expandBane = options.FirstOrDefault(o => o.Text == "Increase Bane radius");
                    if (expandBane != null)
                    {
                        QEffect bane = creature.QEffects.FirstOrDefault(qf => qf.Name == "Bane");
                        (int, bool) temp = ((int, bool))bane.Tag;
                        int radius = temp.Item1;

                        expandBane.AiUsefulness.MainActionUsefulness = 0f;
                        foreach (Creature enemy in creature.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(creature.OwningFaction) && creature.DistanceTo(cr.Occupies) == radius + 1))
                        {
                            expandBane.AiUsefulness.MainActionUsefulness += 4;
                        }
                    }

                    // Demoralize AI
                    foreach (Option option in options.Where(o => o.Text == "Demoralize" || o.AiUsefulness.ObjectiveAction != null && o.AiUsefulness.ObjectiveAction.Action.ActionId == ActionId.Demoralize))
                    {
                        option.AiUsefulness.MainActionUsefulness = 0f;
                    }

                    // Ally and enemy proximity AI
                    foreach (Option option in options.Where(o => o.OptionKind == OptionKind.MoveHere))
                    {
                        TileOption? option2 = option as TileOption;
                        if (option2 != null)
                        {
                            //option2.AiUsefulness.MainActionUsefulness += creature.Battle.AllCreatures.Where(c => c != creature && c.OwningFaction == creature.OwningFaction && !c.HasTrait(Trait.Mindless) && c.DistanceTo(option2.Tile) <= 2 && c.HasLineOfEffectTo(option2.Tile) != CoverKind.Blocked).ToArray().Length;
                            //option2.AiUsefulness.MainActionUsefulness += creature.Battle.AllCreatures.Where(c => c.OwningFaction.EnemyFactionOf(creature.OwningFaction) && c.DistanceTo(option2.Tile) <= 2).ToArray().Length * 0.2f;
                            float mod1 = creature.Battle.AllCreatures.Where(c => c != creature && c.OwningFaction == creature.OwningFaction && !c.HasTrait(Trait.Mindless) && c.DistanceTo(option2.Tile) <= 2 && c.HasLineOfEffectTo(option2.Tile) != CoverKind.Blocked).ToArray().Length;
                            float mod2 = creature.Battle.AllCreatures.Where(c => c.OwningFaction.EnemyFactionOf(creature.OwningFaction) && c.DistanceTo(option2.Tile) <= 2).ToArray().Length * 0.2f;
                            option2.AiUsefulness.MainActionUsefulness += mod1 + mod2;
                        }
                    }
                    foreach (Option option in options.Where(o => o.OptionKind != OptionKind.MoveHere && o.AiUsefulness.MainActionUsefulness != 0))
                    {
                        float mod1 = creature.Battle.AllCreatures.Where(c => c != creature && c.OwningFaction == creature.OwningFaction && !c.HasTrait(Trait.Mindless) && c.DistanceTo(creature) <= 2 && creature.HasLineOfEffectTo(c.Occupies) != CoverKind.Blocked).ToArray().Length;
                        float mod2 = creature.Battle.AllCreatures.Where(c => c.OwningFaction.EnemyFactionOf(creature.OwningFaction) && c.DistanceTo(creature) <= 2).ToArray().Length * 0.2f;
                        option.AiUsefulness.MainActionUsefulness += mod1 + mod2;
                        //option.AiUsefulness.MainActionUsefulness += creature.Battle.AllCreatures.Where(c => c != creature && c.OwningFaction == creature.OwningFaction && !c.HasTrait(Trait.Mindless) && c.DistanceTo(creature) <= 2 && creature.HasLineOfEffectTo(c.Occupies) != CoverKind.Blocked).ToArray().Length;
                        //option.AiUsefulness.MainActionUsefulness += creature.Battle.AllCreatures.Where(c => c.OwningFaction.EnemyFactionOf(creature.OwningFaction) && c.DistanceTo(creature) <= 2).ToArray().Length * 0.2f;

                    }

                    return null;
                };
            })
            .AddQEffect(CommonQEffects.CruelTaskmistress("1d6"))
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(CommonQEffects.DrowClergy())
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Weapon, Proficiency.Trained)
            .WithProficiency(Trait.Divine, Proficiency.Expert)
            .AddHeldItem(Items.CreateNew(CustomItems.ScourgeOfFangs))
            .WithSpellProficiencyBasedOnSpellAttack(11, Ability.Wisdom)
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Cleric, Ability.Wisdom, Trait.Divine).WithSpells(
                new SpellId[] { SpellId.Bane, SpellId.Fear, SpellId.Fear, SpellId.Fear, SpellId.RayOfFrost },
                new SpellId[] { SpellId.Harm, SpellId.Harm, SpellId.Harm }).Done();
        }
    }
}