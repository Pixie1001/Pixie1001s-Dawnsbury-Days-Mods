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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2 {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class WitchMother {
        public static Creature Create() {
            return new Creature(IllustrationName.WaterElemental256, "Mother Cassandra", new List<Trait>() { Trait.Neutral, Trait.Evil, Trait.Human, Trait.Tiefling, Trait.Humanoid, ModTraits.Witch, Trait.Female }, 3, 4, 5, new Defenses(17, 9, 6, 12), 60,
            new Abilities(0, 2, 3, 4, 2, 0), new Skills(nature: 10, occultism: 14, intimidation: 9))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (tile, _, _, cr) => cr.OwningFaction.IsEnemy && !cr.HasTrait(Trait.Animal) && cr.DistanceTo(tile) <= 3, 1.5f, false);
                    return null;
                };
            })
            .WithProficiency(Trait.Unarmed, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(new Item(IllustrationName.Fist, "nails", new Trait[] { Trait.Unarmed, Trait.Melee, Trait.Brawling, Trait.Finesse }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Slashing)))
            .AddHeldItem(Items.CreateNew(CustomItems.ProtectiveAmulet))
            .AddQEffect(new QEffect("Curse of Dread", "The party are afflicted by a powerful supernatural uncertainty, as if fate itself will conspire against them so long as the caster lives.") {
                StateCheckWithVisibleChanges = async self => {
                    if (!self.Owner.Alive) {
                        return;
                    }
                    List<Creature> party = self.Owner.Battle.AllCreatures.Where(cr => cr.OwningFaction.EnemyFactionOf(self.Owner.OwningFaction)).ToList();
                    party.ForEach(cr => {
                        cr.AddQEffect(new QEffect("Curse of Dread", $"You're frightened 1 so long as {self.Owner.Name} lives.") {
                            ExpiresAt = ExpirationCondition.Ephemeral,
                            Innate = false,
                            Source = self.Owner,
                            Illustration = self.Owner.Illustration,
                            StateCheck = self => {
                                self.Owner.AddQEffect(QEffect.Frightened(1).WithExpirationEphemeral());
                            }
                        });
                    });
                },
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, ModTraits.Witch, Ability.Intelligence, Trait.Divine).WithSpells(
                level1: new SpellId[] { SpellId.GrimTendrils, SpellId.GrimTendrils, SpellId.Heal },
                level2: new SpellId[] { SpellId.Heal, SpellId.HideousLaughter }).Done();
        }
    }
}