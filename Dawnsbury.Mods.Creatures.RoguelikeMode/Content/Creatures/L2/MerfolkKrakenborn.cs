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
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using System.Threading;
using System;
using static System.Collections.Specialized.BitVector32;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Tiles;
using HarmonyLib;
using static System.Net.Mime.MediaTypeNames;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class MerfolkKrakenborn {
        public static Creature Create() {
            // Reaction
            // grapple
            // 

            Item tentacle = new Item(IllustrationName.Tentacle, "tentacle", new Trait[] { Trait.Unarmed, Trait.Brawling, Trait.Agile })
            .WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Bludgeoning))
            .WithAdditionalWeaponProperties(wp => wp.WithOnTarget(async (action, a, d, r) => {
                if (r >= CheckResult.Success && !a.HeldItems.Any(item => item.HasTrait(Trait.Grapplee) && item.Grapplee == d)) {
                    await CommonAbilityEffects.Grapple(a, d);
                }
            }));

            Creature monster = new Creature(Illustrations.MerfolkKrakenborn, "Merfolk Krakenborn", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Merfolk, Trait.Humanoid, Trait.Aquatic, ModTraits.MeleeMutator }, 2, 7, 6, new Defenses(17, 10, 8, 7), 30,
                new Abilities(4, 2, 1, 0, 2, 0), new Skills(athletics: 8))
                .WithCreatureId(CreatureIds.MerfolkKrakenBorn)
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .WithBasicCharacteristics()
                .WithUnarmedStrike(tentacle)
                .AddHeldItem(Items.CreateNew(ItemName.Trident)
                .WithAdditionalWeaponProperties(wp => wp.WithOnTarget(async (action, a, d, r) => {
                    if (r >= CheckResult.Success) {
                        await CommonAbilityEffects.Grapple(a, d);
                    }
                })))
                .AddQEffect(CommonQEffects.UnderwaterMarauder())
                .AddQEffect(CommonQEffects.OceanFlight())
                .AddQEffect(new QEffect("Grabbing Tentacles", "After hitting an enemy with an attack, they also become grabbed by your tentacles.") {

                })
                .AddQEffect(new QEffect("Thrashing Tentacles {icon:Reaction}", "{b}Trigger{/b} An enemy within 5 feet attacks you. {b}Effect{/b} You may make a tentacle strike against the attacker.") {
                    YouAreTargeted = async (self, action) => {
                        if (!action.HasTrait(Trait.Attack) || action.Owner.DistanceTo(self.Owner) > 1) {
                            return;
                        }

                        CombatAction strike = self.Owner.CreateStrike(self.Owner.UnarmedStrike, 0).WithActionCost(0);
                        strike.ActionCost = 0;
                        strike.ChosenTargets = ChosenTargets.CreateSingleTarget(action.Owner);
                        
                        int map = self.Owner.Actions.AttackedThisManyTimesThisTurn;

                        if ((bool)strike.CanBeginToUse(self.Owner) && (strike.Target as CreatureTarget)!.IsLegalTarget(self.Owner, action.Owner).CanBeUsed && await self.Owner.AskToUseReaction($"{action.Owner.Name} is attempting to attack you in melee. Would you like to retaliate with thrashing tentacles?")) {
                            if (strike.CanBeginToUse(action.Owner)) {
                                await strike.AllExecute();
                                self.Owner.Actions.AttackedThisManyTimesThisTurn = map;
                            }

                        }
                    }
                })
                .AddQEffect(new QEffect() {
                    ProvideContextualAction = self => {
                        int dc = 17 + self.Owner.Level;
                        string dmg = "1d4+" + (self.Owner.HasEffect(QEffectId.Inferior) ? 1 : self.Owner.HasEffect(QEffectId.Weak) ? 3 : self.Owner.HasEffect(QEffectId.Elite) ? 7 : self.Owner.HasEffect(QEffectId.Supreme) ? 9 : 5);
                        float avgDmg = DiceFormula.FromText(dmg).ExpectedValue;

                        Creature constrictor = self.Owner;
                        return (ActionPossibility)new CombatAction(constrictor, IllustrationName.Constrict, "Crushing Tentacles", Array.Empty<Trait>(),
                            "Deal " + dmg + " damage to each creature you're grappling.", Target.Self().WithAdditionalRestriction(a => !a.HeldItems.Any(item => item.HasTrait(Trait.Grapplee) && item.Grapplee != null) ? "you're not grappling anyone" : null))
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.Boneshaker)
                        .WithSavingThrow(new SavingThrow(Defense.Fortitude, dc))
                        .WithGoodness((t, a, d) => a.Actions.AttackedThisManyTimesThisTurn != 0 ? avgDmg * a.Battle.AllCreatures.Where(cr => cr.QEffects.Any(qf => qf.Id == QEffectId.Grappled && qf.Source == a && cr.Alive)).Count() : 1f)
                        .WithEffectOnSelf(async (action, user) => {
                            foreach (Creature target in user.Battle.AllCreatures.Where(cr => cr.QEffects.Any(qf => qf.Id == QEffectId.Grappled && qf.Source == user))) {
                                CheckResult result = CommonSpellEffects.RollSavingThrow(target, action, Defense.Fortitude, dc);
                                await CommonSpellEffects.DealBasicDamage(action, user, target, result, DiceFormula.FromText(dmg, "Crushing Tentacles"), DamageKind.Bludgeoning);
                            }
                        })
                        ;
                    }
                })
                .AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        if (self.Owner.FindQEffect(QEffectId.AquaticCombat) == null) {
                            return null;
                        }

                        return (ActionPossibility)new CombatAction(self.Owner, Illustrations.InkCloud, "Ink Cloud", new Trait[] { },
                            $"You emit a cloud of dark-brown ink in a 10-foot emanation. Creatures inside the cloud are hidden. You can't use Ink Cloud again for 2d6 rounds.", Target.Emanation(2)
                            .WithGoodness((t, a, d) => a.Battle.AllCreatures.Any(cr => cr.DistanceTo(a) <= 1 && cr.QEffects.Any(qf => qf.Id == QEffectId.Grappled && qf.Source == a)) ? 10f : int.MinValue))
                        //.WithGoodness((t, a, d) => a.Battle.AllCreatures.Any(cr => cr.DistanceTo(a) <= 1 && cr.QEffects.Any(qf => qf.Id == QEffectId.Grappled && qf.Source == a)) ? 10f : int.MinValue)
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.GaleBlast)
                        .WithProjectileCone(Illustrations.InkCloud, 15, ProjectileKind.Cone)
                        .WithEffectOnSelf(caster => {
                            foreach (Tile tile in caster.Battle.Map.AllTiles.Where(t => t.Kind != TileKind.Wall && t.DistanceTo(caster.Occupies) <= 2)) {
                                tile.AddQEffect(new TileQEffect(tile) {
                                    Name = "Ink Cloud",
                                    VisibleDescription = "Creatures inside this cloud are hidden and blinded.",
                                    Illustration = Illustrations.InkCloud,
                                    StateCheck = self => {
                                        if (self.Owner.PrimaryOccupant != null) {
                                            self.Owner.PrimaryOccupant.AddQEffect(QEffect.Invisibility(true).WithExpirationEphemeral());
                                            self.Owner.PrimaryOccupant.AddQEffect(QEffect.Blinded().WithExpirationEphemeral());
                                        }
                                    },
                                });
                            }
                            caster.AddQEffect(QEffect.CannotUseForXRound("Ink Cloud", caster, R.Next(1, 7) + R.Next(1, 7)));
                        })
                        ;
                    }
                })
                ;

            return monster;
        }
    }
}