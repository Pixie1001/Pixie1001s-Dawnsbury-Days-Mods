using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Library;
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
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class ExplosiveMushroom {
        public static Creature Create(Encounter? encounter) {
            var level = UtilityFunctions.GetEncounterLevel(encounter, 0, 4) ?? 2;

            var dmg = 2 + level + "d6";
            var dc = 17 + level;

            Creature hazard = new Creature(Illustrations.BoomShroom, "Explosive Mushroom", new List<Trait>() { Trait.Object, Trait.Plant }, level, 0, 0, new Defenses(10, 10, 0, 0), 20, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
                    .WithTactics(Tactic.DoNothing)
                    .WithEntersInitiativeOrder(false)
                    .AddQEffect(CommonQEffects.Hazard())
                    .AddQEffect(new QEffect("Combustible Spores",
                    $"This mushroom expels a cloud of highly reactive spores. Upon taking fire or electricity damage, the spores ignite in a devastating chain reaction, dealing {dmg} fire damage vs. a DC {dc} Basic reflex save to each creature within a 10 foot radius.") {
                        AfterYouTakeDamageOfKind = async (self, action, kind) => {
                            string name = self.Owner.Name;

                            if (!self.UsedThisTurn && (kind == DamageKind.Fire || kind == DamageKind.Electricity || (action != null && (action.HasTrait(Trait.Fire) || action.HasTrait(Trait.Electricity))))) {
                                CombatAction explosion = new CombatAction(self.Owner, IllustrationName.Fireball, "Combustible Spores", new Trait[] { Trait.Fire, Trait.UsableEvenWhenUnconsciousOrParalyzed, Trait.UsableThroughConfusion }, "", Target.SelfExcludingEmanation(2))
                                .WithActionCost(0)
                                .WithSoundEffect(SfxName.Fireball)
                                .WithSavingThrow(new SavingThrow(Defense.Reflex, dc))
                                .WithProjectileCone(IllustrationName.Fireball, 15, ProjectileKind.Cone)
                                .WithEffectOnEachTarget(async (spell, a, d, r) => {
                                    await CommonSpellEffects.DealBasicDamage(spell, a, d, r, DiceFormula.FromText(dmg, "Combustible Spores"), DamageKind.Fire);
                                })
                                ;
                                self.Owner.Battle.AllCreatures.Where(cr => cr != self.Owner && cr.DistanceTo(self.Owner.Occupies) <= 2 && cr.HasLineOfEffectTo(self.Owner.Occupies) < CoverKind.Blocked).ForEach(cr => explosion.ChosenTargets.ChosenCreatures.Add(cr));
                                self.Owner.Battle.Map.AllTiles.Where(t => t.DistanceTo(self.Owner.Occupies) <= 2 && t.DistanceTo(self.Owner.Occupies) > 0).ForEach(t => explosion.ChosenTargets.ChosenTiles.Add(t));
                                //await CommonAnimations.CreateConeAnimation(self.Owner.Battle, self.Owner.Occupies.ToCenterVector(), self.Owner.Battle.Map.AllTiles.Where(t => t.DistanceTo(self.Owner.Occupies) <= 2).ToList(), 15, ProjectileKind.Cone, IllustrationName.Fireball);
                                self.UsedThisTurn = true;
                                await explosion.AllExecute();
                                self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                                self.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        }
                    });

            return hazard;
        }
    }
}