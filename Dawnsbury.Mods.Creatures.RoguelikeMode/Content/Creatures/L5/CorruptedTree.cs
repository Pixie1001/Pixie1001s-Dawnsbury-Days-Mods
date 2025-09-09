using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Audio;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Tiles;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class CorruptedTree
    {
        public static Creature Create()
        {
            var creature = new Creature(IllustrationName.HappyTree256,
                "Corrupted Tree",
                [Trait.Plant, Trait.Evil, Trait.Chaotic],
                5, 15, 0,
                new Defenses(19, 16, 8, 12),
                60,
                new Abilities(5, 0, 4, -1, 2, 0),
                new Skills(nature: 13, stealth: 13))
            .WithCharacteristics(false, false)
            .AddQEffect(QEffect.DamageResistance(DamageKind.Bludgeoning, 5))
            .AddQEffect(QEffect.DamageResistance(DamageKind.Piercing, 5))
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Slashing, 5))
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Fire, 5))
            .AddQEffect(CommonQEffects.MonsterPush())
            .Builder
            .AddMainAction((creature) =>
            {
                var tileTarget = new TileTarget(delegate (Creature caster, Tile tile)
                {
                    if (tile.IsTrulyGenuinelyFreeToEveryCreature)
                    {
                        Tile occupies = caster.Occupies;
                        return occupies != null && occupies.DistanceTo(tile) <= 100;
                    }

                    return false;
                }, (Creature caster, Tile targetTile) =>
                {
                    var adjacentEnemyCouunt = caster.Battle.AllCreatures.Count((enemy) => enemy.EnemyOf(caster) && enemy.IsAdjacentTo(caster));

                    if (adjacentEnemyCouunt < 2)
                    {
                        return caster.AI.SummonCreatureAtTile(targetTile);
                    }

                    return AIConstants.NEVER;
                })
                {
                    OverriddenTargetLine = "{b}Range{/b} " + 500 + " feet"
                };

                return new CombatAction(creature, IllustrationName.SummonAnimal, "Grow Root", [], "You animate one of your roots, pushing it out of the ground in an unoccupied space.", tileTarget)
                .WithActionCost(2)
                .WithSoundEffect(SfxName.StandUp)
                .WithEffectOnEachTile(async (CombatAction _, Creature user, IReadOnlyList<Tile> targets) =>
                {
                    if (targets.Count >= 1)
                    {
                        user.Battle.SpawnCreature(CreateRoot(user), user.OwningFaction, targets[0]);
                    }
                });
            })
            .AddNaturalWeapon("branch", IllustrationName.Branch, 13, [Trait.Shove], "2d6+6", DamageKind.Bludgeoning)
            .Done();
            
            return creature;
        }

        public static Creature CreateRoot(Creature owner)
        {
            var creature = new Creature(IllustrationName.ProtectorTree,
                "Root",
                [Trait.Plant, Trait.Evil, Trait.Chaotic],
                1, 10, 6,
                new Defenses(16, 10, 7, 4),
                12,
                new Abilities(4, 1, 3, -5, 2, 0),
                new Skills(athletics: 7, stealth: 4))
            .WithCharacteristics(false, false)
            .AddQEffect(QEffect.Flying())
            .AddQEffect(QEffect.TraitImmunity(Trait.Mental))
            .AddQEffect(QEffect.DamageResistance(DamageKind.Bludgeoning, 3))
            .AddQEffect(QEffect.DamageResistance(DamageKind.Piercing, 3))
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Slashing, 3))
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Fire, 3))
            .AddQEffect(QEffect.MonsterKnockdown())
            .Builder
            .AddNaturalWeapon("root", IllustrationName.Branch, 9, [Trait.Trip, Trait.Knockdown], "1d6+3", DamageKind.Piercing)
            .Done();

            if (owner.HasEffect(QEffectId.Weak))
            {
                creature.ApplyWeakAdjustments(true);
            }
            else if (owner.HasEffect(QEffectId.Elite))
            {
                creature.ApplyEliteAdjustments();
            }

            owner.AddQEffect(new()
            {
                WhenCreatureDiesAtStateCheckAsync = async (_) =>
                {
                    await creature.DieFastAndWithoutAnimation();
                }
            });

            return creature;
        }
    }
}
