using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Microsoft.Xna.Framework;
using Dawnsbury.Campaign.Encounters;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class Reliquary {
        public static Creature Create(Encounter? encounter) {
            // Could have a qEffect with a Tag that can be populated with wave information?
            var level = UtilityFunctions.GetEncounterLevel(encounter) ?? 1;

            Creature reliquary = new Creature(IllustrationName.GoldenCandelabra, "Reliquary", [Trait.Lawful, Trait.Good, Trait.Object], level, 0, 0, new Defenses(10, 10, 0, 0), (level + 1) * 15, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
            .WithSpawnAsGaiaFriends()
            .WithEntersInitiativeOrder(false)
            .WithTactics(Tactic.DoNothing)
            .AddQEffect(QEffect.ObjectImmunities())
            .AddQEffect(new QEffect("Holy Reliquary", "This reliquary holds a divine power of utmost importance to the defence of the plane against the Starborn, which waves of enemies will seek to destroy. If they succeed, the party will perish in the ensuring cosmic backlack.") {
                YouAreDealtLethalDamage = async (self, a, dmg, d) => {
                    Sfxs.Play(SfxName.DivineLance);
                    Sfxs.Play(SfxName.Fireball);
                    await CommonAnimations.CreateConeAnimation(a.Battle, d.Occupies.ToCenterVector(), a.Battle.Map.AllTiles.Where(t => t.DistanceTo(d.Occupies) <= 12).ToList(), 10, ProjectileKind.Cone, IllustrationName.DivineWrath);
                    await d.Battle.EndTheGame(false, "The reliquary was destroyed.");
                    return null;
                }
            });

            var effect = new QEffect();
            effect.AddGrantingOfTechnical(cr => cr.EnemyOf(reliquary), qfTech => {
                qfTech.AdditionalGoodness = (self, action, target) => {
                    if (target == reliquary) return 10f;
                    else return 0f;
                };
            });
            reliquary.AddQEffect(effect);

            return reliquary;
        }
    }
}