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
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using System;
using System.Linq;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class Homunculus
    {
        public static Affliction HomunculusPoison = new Affliction(QEffectIds.HomunculusPoison, "Homunculus Poison", 15, "{b}Stage 1{/b} 1d6 poison damage and enfeebled 1", 1, (int stage) => stage switch
        {
            1 => "1d6",
            _ => throw new Exception("Unknown stage."),
        }, delegate (QEffect qfPoison)
        {
            qfPoison.Owner.AddQEffect(QEffect.Enfeebled(1).WithExpirationEphemeral());
        });

        public static Creature Create()
        {
            return new Creature(Illustrations.Homunculus, "Homunculus", [Trait.Construct, ModTraits.MeleeMutator], 0, 3, 8, new Defenses(17, 2, 7, 3), 17, new Abilities(-1, 3, 0, 0, 1, -2), new Skills(acrobatics: 5, stealth: 5))
                .WithCharacteristics(false, true)
                .AddQEffect(QEffect.TraitImmunity(Trait.Disease))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Doomed))
                .AddQEffect(QEffect.TraitImmunity(Trait.Healing))
                .AddQEffect(QEffect.ImmunityToCondition(QEffectId.Unconscious))
                .AddQEffect(QEffect.Flying())
                .AddQEffect(new QEffect("Master Link", "If this has a master then it is linked to its master, and adopts the same alignment. If the homunculus is destroyed, the master takes 2d10 mental damage.")
                {
                    WhenCreatureDiesAtStateCheckAsync = async (qfDies) =>
                    {
                        Creature self = qfDies.Owner;
                        if (!self.QEffects.Any(qe => qe.Name == "Death in progress"))
                        {
                            self.AddQEffect(new QEffect("Death in progress", "[technical trait]"));
                            Creature? master = ((Creature?)(self.FindQEffect(QEffectIds.HomunculusMaster)?.Tag ?? null));
                            if (master != null && master.Alive)
                            {
                                CombatAction deathDamage = new CombatAction(self, IllustrationName.Unknown, "Master Link Death", [Trait.Arcane, Trait.Divination, Trait.Mental, Trait.ExecuteEvenIfCasterCannotTakeActions], "Deals 2d10 mental damage to the master.", Target.Uncastable())
                                .WithActionCost(0)
                                .WithSoundEffect(SfxName.PhaseBolt)
                                .WithEffectOnSelf(async (action, innerself) =>
                                {
                                    await CommonSpellEffects.DealDirectDamage(action, DiceFormula.FromText("2d10"), master, CheckResult.Success, DamageKind.Mental);
                                });

                                await self.Battle.GameLoop.FullCast(deathDamage);
                            }
                        }
                    }
                })
                .AddQEffect(new QEffect("Homunculus Poison", "When this deals damage to an enemy with it's jaws Strike, the enemy must succeed a DC 15 Fortitude save or be affected by the poison ({i}maximum duration{/i} 6 rounds; {i}stage 1{/i} 1d6 poison damage and enfeebled 1).")
                {
                    YouBeginAction = async (qfBeginAction, action) =>
                    {
                        Creature self = qfBeginAction.Owner;
                        if (!self.QEffects.Any(qe => qe.Name == "Empty Homunculus Reservior"))
                        {
                            QEffect affliction = Affliction.CreateInjuryQEffect(HomunculusPoison);
                            affliction.Id = QEffectIds.HomunculusPoison;
                            self.AddQEffect(affliction);
                        }
                    },
                    AfterYouTakeAction = async (qfAfterAction, action) =>
                    {
                        Creature self = qfAfterAction.Owner;
                        self.RemoveAllQEffects(qe => qe.Id == QEffectIds.HomunculusPoison);
                        if (action.CheckResult >= CheckResult.Success && action.HasTrait(Trait.AddsInjuryPoison))
                        {
                            self.AddQEffect(new QEffect("Empty Homunculus Reservior", "You can not apply Homunculus Poison until you spend an action to refill your poison reservior."));
                        }
                    },
                    ProvideMainAction = (qfMainAction) =>
                    {
                        Creature self = qfMainAction.Owner;
                        if (self.QEffects.Any(qe => qe.Name == "Empty Homunculus Reservior"))
                        {
                            return new ActionPossibility(new CombatAction(self, IllustrationName.Unknown, "Refill Poison", [Trait.Manipulate], "Refill your Homunculus Poison reservior.", Target.Self())
                                .WithActionCost(1)
                                .WithGoodness((t, a, d) => self.Battle.AllCreatures.Any(creature => !creature.FriendOf(self) && creature.IsAdjacentTo(self)) ? AIConstants.EXTREMELY_PREFERRED : AIConstants.NEVER)
                                .WithEffectOnSelf((innerself) =>
                                {
                                    innerself.RemoveAllQEffects(qe => qe.Name == "Empty Homunculus Reservior");
                                }));
                        }

                        return null;
                    }
                })
                .Builder
                .AddNaturalWeapon(NaturalWeaponKind.Jaws, 7, [Trait.Finesse, Trait.Magical, Trait.AddsInjuryPoison], "1d4+0", DamageKind.Piercing)
                .Done();
        }

        public static void AddMasterEffect(Creature homunculus, Creature master)
        {
            homunculus.AddQEffect(new QEffect()
            {
                Id = QEffectIds.HomunculusMaster,
                Tag = master,
                Illustration = IllustrationName.Dominate,
                Name = "Master",
                Description = $"{master.Name} is {homunculus.Name}'s master"
            });
            Trait[] alignments = [Trait.Good, Trait.Lawful, Trait.Evil, Trait.Chaotic];
            foreach (Trait alignment in alignments.Except(homunculus.Traits).Intersect(master.Traits))
            {
                homunculus.Traits.Add(alignment);
            }
            homunculus.MainName = master.Name + "'s " + homunculus.BaseName;
        }
    }
}