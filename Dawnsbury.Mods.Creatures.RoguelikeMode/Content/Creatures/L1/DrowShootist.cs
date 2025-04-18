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
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowShootist {
        public static Creature Create() {
            return new Creature(Illustrations.DrowShootist, "Drow Shootist", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid, ModTraits.ArcherMutator }, 1, 10, 6, new Defenses(15, 4, 10, 7), 18,
            new Abilities(-1, 4, 1, 1, 2, 2), new Skills(acrobatics: 7, stealth: 7, deception: 7, intimidation: 5))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature creature = self.Self;
                    foreach (Option option in options.Where(opt => opt.Text == "Reload" || opt.AiUsefulness.ObjectiveAction != null && opt.AiUsefulness.ObjectiveAction.Action.Name == "Reload")) {
                        option.AiUsefulness.MainActionUsefulness = 0f;
                    }
                    return null;
                };
            })
            .WithCreatureId(CreatureIds.DrowShootist)
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(QEffect.SneakAttack("1d8"))
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Melee, Proficiency.Trained)
            .WithProficiency(Trait.Ranged, Proficiency.Master)
            .AddHeldItem(Items.CreateNew(ItemName.HandCrossbow))
            .AddHeldItem(Items.CreateNew(ItemName.HandCrossbow))
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    Item? xbow = self.Owner.HeldItems.FirstOrDefault(item => item.ItemName == ItemName.HandCrossbow && !item.EphemeralItemProperties.NeedsReload);
                    if (xbow == null) {
                        return null;
                    }

                    StrikeModifiers strikeModifiers = new StrikeModifiers() {
                        OnEachTarget = async (a, d, result) => {
                            if (result == CheckResult.Success) {
                                d.AddQEffect(QEffect.FlatFooted("Distracting Shot").WithExpirationAtStartOfSourcesTurn(a, 0));
                            } else if (result == CheckResult.CriticalSuccess) {
                                d.AddQEffect(QEffect.FlatFooted("Distracting Shot").WithExpirationAtEndOfSourcesNextTurn(a, false));
                            }

                        }
                    };
                    CombatAction action = self.Owner.CreateStrike(xbow, -1, strikeModifiers);
                    action.ActionCost = 2;
                    action.Name = "Distracting Shot";
                    action.Description = StrikeRules.CreateBasicStrikeDescription2(action.StrikeModifiers, additionalSuccessText: "The target is flat footed until the start of your next turn.", additionalCriticalSuccessText: "The target is flat footed until the end of your next turn.");
                    action.ShortDescription += " and the target is flat footed until the start of the Drow Shootist's next turn, or the end of a critical success.";
                    action.Illustration = new SideBySideIllustration(action.Illustration, IllustrationName.CreateADiversion);
                    action.WithGoodnessAgainstEnemy((target, attacker, defender) => {
                        return defender.QEffects.FirstOrDefault(qf => qf.Name == "Flat-footed") != null ? 2 : 6.5f;
                    });

                    return (ActionPossibility)action;
                }
            })
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    if (self.Owner.HeldItems.Count < 2) {
                        return null;
                    }

                    Item xbow1 = self.Owner.HeldItems[0];
                    Item xbow2 = self.Owner.HeldItems[1];
                    if (xbow1.ItemName != ItemName.HandCrossbow || xbow2.ItemName != ItemName.HandCrossbow) {
                        return null;
                    }

                    if (!xbow1.EphemeralItemProperties.NeedsReload || !xbow2.EphemeralItemProperties.NeedsReload) {
                        return null;
                    }

                    CombatAction action = new CombatAction(self.Owner, new SideBySideIllustration(IllustrationName.HandCrossbow, IllustrationName.HandCrossbow), "Reloading Trick", new Trait[] { Trait.Manipulate }, "The Drow Shootist reloads both of their hand crossbows", Target.Self((cr, ai) => 15))
                    .WithActionCost(1)
                    .WithSoundEffect(SfxName.OpenLock)
                    .WithEffectOnSelf(user => {
                        xbow1.EphemeralItemProperties.NeedsReload = false;
                        xbow2.EphemeralItemProperties.NeedsReload = false;
                    })
                    ;
                    return (ActionPossibility)action;
                }
            });
        }
    }
}