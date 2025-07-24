using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Display;
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
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Core.Mechanics.Targeting.Targets;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    public class NightmareGrowth {
        public static Creature Create() {
            int radius = 2;
            //int dc = 22;

            Creature hazard = new Creature(Illustrations.NightmareGrowth, "Nightmare Growth", new List<Trait>() { Trait.Object, Trait.Indestructible, Trait.NoDeathOverhead }, 6, 0, 0, new Defenses(10, 10, 0, 0), 20, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
            .WithTactics(Tactic.DoNothing)
            .WithEntersInitiativeOrder(false)
            .AddQEffect(QEffect.FullImmunity())
            .AddQEffect(new QEffect() { Id = QEffectId.SeeInvisibility })
            .Builder
            .AddNaturalWeapon("Opportunistic Stab", new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.Spear), 16, [Trait.Basic, Trait.Reach], "1d8+0", DamageKind.Piercing, wp => wp.WithAdditionalDamage("1d8", DamageKind.Mental))
            .Done()
            ;

            var animation = hazard.AnimationData.AddAuraAnimation(IllustrationName.KineticistAuraCircle, radius);
            animation.Color = Color.Black;

            QEffect effect = new QEffect("Looming Branches",
                $"(aura) 10 feet. Whenever an enemy of the demon queen of spiders uses an action with cost of 1 or greater within the aura, there's a 20% chance that it will make a +{10 + hazard.Level} strike against them for 1d8 piercing and 1d8 mental damage.") {
            };

            effect.AddGrantingOfTechnical(cr => cr.OwningFaction.IsPlayer && cr.DistanceTo(hazard.Occupies) <= 2, qfTechnical => {
                qfTechnical.Source = hazard;
                qfTechnical.Innate = false;
                qfTechnical.AfterYouTakeAction = async (self, action) => {
                    if (action.ActionCost >= 1 && R.Next(6) == 0) {
                        var strike = hazard.CreateStrike(hazard.UnarmedStrike, 0);
                        strike.ChosenTargets.ChosenCreature = self.Owner;
                        strike.ChosenTargets.ChosenCreatures.Add(self.Owner);
                        if ((strike.Target as CreatureTarget)!.IsLegalTarget(hazard, self.Owner))
                            await strike.AllExecute();
                    }
                };
            });
            hazard.AddQEffect(effect);

            return hazard;
        }
    }
}