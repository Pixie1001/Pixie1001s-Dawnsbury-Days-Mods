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
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DevotedCultist {
        public static Creature Create() {
            return new Creature(Illustrations.DevotedCultist, "Devoted Cultist", new List<Trait>() { Trait.Neutral, Trait.Evil, Trait.Human, Trait.Humanoid }, 1, 6, 5, new Defenses(15, 7, 4, 10), 26,
            new Abilities(1, 2, 4, -1, 1, 0), new Skills(acrobatics: 7, athletics: 6))
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(ItemName.Dagger))
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    return (ActionPossibility)new CombatAction(self.Owner, Illustrations.RitualOfAscension, "Ritual of Ascension", new Trait[] { Trait.Flourish, Trait.Divine, Trait.Magical }, "", Target.Self())
                    .WithSoundEffect(SfxName.AuraExpansion)
                    .WithActionCost(1)
                    .WithGoodness((t, a, d) => 100f)
                    .WithProjectileCone(Illustrations.RitualOfAscension, 7, ProjectileKind.Cone)
                    .WithEffectOnSelf(async caster => {
                        if (!caster.QEffects.Any(qf => qf.Name == "Ritual of Ascension")) {
                            caster.AddQEffect(new QEffect("Ritual of Ascension", $"When this condition reaches 3, the {caster.BaseName} will transform, fulfilling their life's purpose.", ExpirationCondition.Never, caster, Illustrations.RitualOfAscension) {
                                Value = 1,
                            });
                        } else {
                            int stacks = caster.QEffects.First(qf => qf.Name == "Ritual of Ascension").Value++;
                            if (stacks >= 2) {
                                Tile pos = caster.Occupies;
                                caster.Battle.RemoveCreatureFromGame(caster);
                                int rand = R.Next(0, 5);
                                Creature newForm = null;
                                switch (rand) {
                                    case 0:
                                        newForm = MonsterStatBlocks.CreateAbrikandilu();
                                        break;
                                    case 1:
                                        newForm = MonsterStatBlocks.CreateBarghest();
                                        break;
                                    case 2:
                                        newForm = MonsterStatBlocks.CreateDretch();
                                        break;
                                    case 3:
                                        newForm = BebilithSpawn.Create();
                                        break;
                                    default:
                                        newForm = MonsterStatBlocks.CreateAbrikandilu();
                                        break;
                                }
                                if (newForm.Level - caster.Level >= 4) {
                                    newForm.ApplyWeakAdjustments(false, true);
                                } else if (newForm.Level - caster.Level == 3) {
                                    newForm.ApplyWeakAdjustments(false);
                                } else if (newForm.Level - caster.Level == 1) {
                                    newForm.ApplyEliteAdjustments();
                                } else if (newForm.Level - caster.Level == 0) {
                                    newForm.ApplyEliteAdjustments(true);
                                }
                                Sfxs.Play(SfxName.DeepNecromancy);
                                Sfxs.SlideIntoSong(SoundEffects.VikingMusic);
                                caster.Battle.SpawnCreature(newForm, caster.OwningFaction, pos);
                            }
                        }

                    })
                    ;
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, ModTraits.Witch, Ability.Intelligence, Trait.Arcane).WithSpells(
                level1: new SpellId[] { SpellId.Guidance, SpellId.ChillTouch }).Done();
        }
    }
}