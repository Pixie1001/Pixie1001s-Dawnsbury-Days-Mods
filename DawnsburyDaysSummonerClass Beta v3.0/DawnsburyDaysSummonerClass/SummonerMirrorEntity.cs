using Dawnsbury;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.StatBlocks.Monsters.L15;
using Dawnsbury.IO;
using Dawnsbury.Mods.Classes.Summoner;
using Microsoft.Xna.Framework;
using static Dawnsbury.Mods.Classes.Summoner.Enums;
using static Dawnsbury.Mods.Classes.Summoner.SummonerSpells;
using static Dawnsbury.Mods.Classes.Summoner.SummonerClassLoader;

namespace Dawnsbury.Mods.Classes.Summoner {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class SummonerMirrorEntity {

        public static void RegisterMirrorEntity() {
            MirrorEntity.RegisterClassTemplate(tSummoner, MirrorEntity.MirrorEntityBaseStatblock.SpellcasterDivineSpontaneous, summoner => {
                var handwraps = Items.CreateNew(ItemName.HandwrapsOfMightyBlows)
                    .WithModificationRune(ItemName.WeaponPotencyRunestone2)
                    .WithModificationRune(ItemName.GreaterStrikingRunestone)
                    .WithModificationRune(ItemName.NightmareRunestone)
                    .WithModificationRune(ItemName.UnholyRunestone)
                ;
                handwraps.IsWorn = true;
                handwraps.Traits.Add(Trait.EncounterEphemeral);
                summoner.CarriedItems.Add(handwraps);

                // Setup spells
                summoner.Spellcasting?.Sources.Clear();
                summoner.WithSpellProficiencyBasedOnSpellAttack(27, Ability.Charisma);
                if (PlayerProfile.Instance.IsBooleanOptionEnabled("Summoner_PsychicSpellProgression")) {
                    summoner.AddSpellcastingSource(SpellcastingKind.Spontaneous, tSummoner, Ability.Charisma, Trait.Divine)
                    .WithSpells(level8: [SpellId.DivineArmageddon, spells[SummonerSpellId.EidolonBoost]],
                        level7: [SpellId.DeitysStrike, SpellId.FlameStrike],
                        level6: [SpellId.Heroism, SpellId.VampiricExsanguination],
                        level5: [SpellId.Heal, SpellId.PainfulVibrations],
                        level4: [SpellId.Fear, SpellId.Slow])
                    .WithSpontaneousSlots(level8: 1, level7: 2, level6: 2, level5: 2, level4: 2);
                } else {
                    summoner.AddSpellcastingSource(SpellcastingKind.Spontaneous, tSummoner, Ability.Charisma, Trait.Divine)
                    .WithSpells(level8: [SpellId.Heal, SpellId.DivineArmageddon, spells[SummonerSpellId.EidolonBoost]],
                        level7: [SpellId.Heroism, SpellId.DeitysStrike])
                    .WithSpontaneousSlots(level8: 2, level7: 2);
                }

                Creature eidolon = new Creature(IllustrationName.Urglid, "Eidolon", [tEidolon, Trait.Fiend, Trait.Demon, Trait.Starborn, Trait.Chaotic, Trait.Evil, Trait.Large], summoner.Level, perception: summoner.Perception - 2, speed: 7, new Defenses(37, 27, 26, 24), summoner.MaxHP, new Abilities(6, 6, 6, 4, 4, 4), summoner.Skills)
                        .WithProficiency(Trait.Unarmed, Proficiency.Master)
                        .WithUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Claw, "1d8", DamageKind.Slashing, [Trait.VersatileB, Trait.VersatileP, Trait.Evil, Trait.Trip]).WithAdditionalWeaponProperties(wp => {
                            wp.AdditionalDamage.Add(("1", DamageKind.Evil));
                            wp.WithOnTarget(async (ca, a, d, result) => {
                                if (result == CheckResult.CriticalSuccess) d.AddQEffect(QEffect.PersistentDamage("1d6+2", DamageKind.Bleed));
                            });
                        }))
                        .WithAdditionalUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Slam, "1d6", DamageKind.Bludgeoning, [Trait.Agile, Trait.Finesse, Trait.Evil]).WithAdditionalWeaponProperties(wp => wp.AdditionalDamage.Add(("1", DamageKind.Evil))))
                        .AddQEffect(new QEffect("Bloodletting Claws", "If your eidolon critically hits with a melee unarmed Strike that deals slashing or piercing damage, its target takes 1d6 + item bonus persistent bleed damage."))
                        .AddQEffect(new QEffect("Tandem Movement", "When this creature Strides or Steps, their master may do the same as a {icon:FreeAction}free action.") {
                            AfterYouTakeAction = async (qfSelf, action) => {
                                if (summoner.OwningFaction.IsControlledByHumanUser || summoner.HasEffect(QEffectId.Controlled) || qfSelf.Owner.HasEffect(qfActTogether) || qfSelf.Owner.HasEffect(qfTandemTurn)) return;

                                if (action.ActionId == ActionId.Stride) {
                                    summoner.AddQEffect(new QEffect() { Id = qfTandemTurn });
                                    summoner.Overhead("Tandem Movement", Color.Blue, qfSelf.Owner + " used Tandem Movement.");
                                    await summoner.StrideAsync("Tandem Movement", true, allowPass: true);
                                    summoner.RemoveAllQEffects(qf => qf.Id == qfTandemTurn);
                                }
                            },
                            ProvideActionIntoPossibilitySection = (qf, section) => {
                                if (section.Name == "Tandem Actions") {
                                    return GenerateTandemMovementAction(qf.Owner, summoner, summoner);
                                }
                                return null;
                            }
                        })
                        .AddQEffect(QEffect.AttackOfOpportunity().WithName("Eidolon's Opportunity"));

                summoner.AddQEffect(new QEffect("Eidolon", "This character can summon and command an Eidolon.") {
                    StartOfCombatAfterInitiativeOrderIsSetUp = async qfSelf => {
                        eidolon.WithEntersInitiativeOrder(false);
                        eidolon.InitiativeControlledBy = summoner;

                        eidolon.MainName = summoner.Name + "'s " + eidolon.MainName;

                        InvestedWeaponLogic.MagicItemLogic(summoner, eidolon);
                        ClassLogic.GenerateActionShareQEffect(summoner, eidolon, forEidolon: true, forceTandemActionsMenu: true);
                        ClassLogic.GenerateHPShareQEffect(summoner, eidolon, true);
                        ClassLogic.GenerateEidolonLogic(eidolon, summoner);

                        ClassLogic.GenerateActionShareQEffect(summoner, eidolon, forceTandemActionsMenu: true);
                        ClassLogic.GenerateHPShareQEffect(summoner, eidolon);
                        ClassLogic.GenerateSummonerActions(summoner, eidolon, Trait.Divine);

                        summoner.Battle.SpawnCreature(eidolon, summoner.OwningFaction, summoner.Occupies);

                        if (eidolon.Battle.InitiativeOrder.Any(cr => cr == eidolon)) {
                            eidolon.Battle.InitiativeOrder.Remove(eidolon);
                        }

                        // Balance HP
                        HPShareEffect shareHP = (HPShareEffect)summoner.QEffects.FirstOrDefault(qf => qf.Id == qfSummonerBond);
                        if (eidolon.HP < summoner.HP) {
                            FlatHeal(eidolon, DiceFormula.FromText($"{summoner.HP - eidolon.HP}"), shareHP!.CA);
                        } else if (eidolon.HP > summoner.HP) {
                            await CommonSpellEffects.DealDirectSplashDamage(shareHP!.CA, DiceFormula.FromText($"{eidolon.HP - summoner.HP}"), eidolon, DamageKind.Untyped);
                        }
                    },
                    ProvideActionIntoPossibilitySection = (qf, section) => {
                        if (section.PossibilitySectionId != PossibilitySectionId.InvisibleActions) return null;

                        return new ActionPossibility(new CombatAction(summoner, eidolon.Illustration, "Command your Eidolon", [Trait.Basic, Trait.Flourish], "", Target.Self((cr, ai) => cr.Actions.ActionsLeft == 0 ? int.MinValue : 5f))
                            .WithActionCost(0)
                            .WithEffectOnSelf(async (spell, caster) => {
                                await PartnerActs(caster, eidolon);
                            })
                        );
                    }
                });

                summoner.AddQEffect(new QEffect("Tandem Movement", "When this creature Strides or Steps, their master may do the same as a {icon:FreeAction}free action.") {
                    AfterYouTakeAction = async (qfSelf, action) => {
                        if (summoner.OwningFaction.IsControlledByHumanUser || summoner.HasEffect(QEffectId.Controlled) || qfSelf.Owner.HasEffect(qfActTogether) || qfSelf.Owner.HasEffect(qfActTogetherToggle) || qfSelf.Owner.HasEffect(qfTandemTurn) || !eidolon.Alive || !eidolon.Actions.CanTakeActions() || eidolon.HasEffect(QEffectId.Confused)) return;

                        if (action.ActionId == ActionId.Stride) {
                            eidolon.Overhead("Tandem Movement", Color.Blue, summoner + " used Tandem Movement.");
                            eidolon.AddQEffect(new QEffect() { Id = qfTandemTurn });
                            await eidolon.StrideAsync("Tandem Movement", true, allowPass: true);
                            eidolon.RemoveAllQEffects(qf => qf.Id == qfTandemTurn);
                        }
                    },
                    ProvideActionIntoPossibilitySection = (qf, section) => {
                        if (section.Name == "Tandem Actions") {
                            return GenerateTandemMovementAction(qf.Owner, eidolon, summoner);
                        }
                        return null;
                    }
                });
            });
        }
    }
}
