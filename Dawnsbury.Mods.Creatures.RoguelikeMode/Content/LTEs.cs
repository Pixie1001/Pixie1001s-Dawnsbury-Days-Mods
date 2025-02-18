using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using System.Diagnostics.Metrics;
using Microsoft.Xna.Framework.Audio;
using static System.Reflection.Metadata.BlobBuilder;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Microsoft.Xna.Framework.Graphics;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core.Mechanics.Enumerations;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using FMOD;
using Dawnsbury.Core.StatBlocks;
using Microsoft.Xna.Framework;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.CharacterBuilder.Feats;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class LTEs {

        internal enum ColosseumFeat
        {
            AggressiveBlock,
            BrutalBeating,
            GravityWeapon,
            KiRush,
            Mobility,
            NimbleDodge,
            PowerAttack,
            QuickDraw,
            RapidResponse,
            ReactiveShield,
            ShakeItOff,
            SuddenCharge,
            YoureNext
        }

        internal static Dictionary<ColosseumFeat, Tuple<FeatName, string>> ColosseumFeatNames = new()
        {
            { ColosseumFeat.AggressiveBlock, new(FeatName.AggressiveBlock, "Aggressive Block") },
            { ColosseumFeat.BrutalBeating, new(FeatName.BrutalBeating, "Brutal Beating") },
            { ColosseumFeat.GravityWeapon, new(FeatName.GravityWeapon, "Gravity Weapon") },
            { ColosseumFeat.KiRush, new(FeatName.KiRush, "Ki Rush") },
            { ColosseumFeat.Mobility, new(FeatName.Mobility, "Mobility") },
            { ColosseumFeat.NimbleDodge, new(FeatName.NimbleDodge, "Nimble Dodge") },
            { ColosseumFeat.PowerAttack, new(FeatName.PowerAttack, "Power Attack") },
            { ColosseumFeat.QuickDraw, new(FeatName.QuickDraw, "Quick Draw") },
            { ColosseumFeat.RapidResponse, new(FeatName.RapidResponse, "Rapid Response") },
            { ColosseumFeat.ReactiveShield, new(FeatName.ReactiveShield, "Reactive Shield") },
            { ColosseumFeat.ShakeItOff, new(FeatName.ShakeItOff, "Shake It Off") },
            { ColosseumFeat.SuddenCharge, new(FeatName.SuddenCharge, "Sudden Charge") },
            { ColosseumFeat.YoureNext, new(FeatName.YoureNext, "You're Next") }
        };

        //public static Dictionary<CharacterSheet, List<QEffect>> PartyBoons = new Dictionary<CharacterSheet, List<QEffect>>();
        //public static Dictionary<CharacterSheet, string> SheetToTag = new Dictionary<CharacterSheet, string>();
        //public static Dictionary<BoonId, QEffect> RegisteredBoons = new Dictionary<BoonId, QEffect>();

        //public static void InitBoons(CampaignState campaign) {
        //    PartyBoons.Clear();
        //    SheetToTag.Clear();

        //    int i = 0;
        //    foreach (AdventurePathHero hero in campaign.Heroes) {
        //        i++;
        //        PartyBoons.Add(hero.CharacterSheet, new List<QEffect>());
        //        SheetToTag.Add(hero.CharacterSheet, $"Hero{i}_Boons");
        //        if (campaign.Tags.TryGetValue($"Hero{i}_Boons", out string value)) {
        //            var list = value.Split(", ");
        //            foreach (string boonId in list) {
        //                if (!Int32.TryParse(boonId, out int result)) {
        //                    continue;
        //                }
        //                PartyBoons[hero.CharacterSheet].Add(RegisteredBoons[(BoonId)result]);
        //            }
        //        } else {
        //            campaign.Tags.Add($"Hero{i}_Boons", "");
        //        }
        //    }

        //    int test = 3;
        //}

        //public static void ApplyBoons(Creature hero) {
        //    foreach (QEffect boon in PartyBoons[hero.PersistentCharacterSheet]) {
        //        hero.AddQEffect(boon);
        //    }
        //}

        //public static void GrantBoon(Creature hero, BoonId boon) {
        //    hero.Battle.CampaignState.Tags[SheetToTag[hero.PersistentCharacterSheet]] += $"{(int)boon}, ";
        //    PartyBoons[hero.PersistentCharacterSheet].Add(RegisteredBoons[boon]);
        //}

        //public static void LoadBoons() {
        //    RegisteredBoons.Add(BoonId.POISON_IMMUNITY, new QEffect("Mushroom Sybiosis", "Your mushroom symbiot renders you immune to poison.") {
        //        ImmuneToTrait = Trait.Poison,
        //        StateCheck = self => {
        //            self.Owner.WeaknessAndResistance.AddImmunity(DamageKind.Poison);
        //        }
        //    });
        //}

        public static void LoadLongTermEffects() {
            //LongTermEffects.EasyRegister("test effect", (string text, int number) => {
            //    return new QEffect("test long term effect", text);
            //});

            LongTermEffects.EasyRegister("Test Boon", LongTermEffectDuration.UntilLongRest, (_, _) => {
                return new QEffect("Test Boon", "Test to see if this shows up on chaacter sheets outside of combat.") {

                };
            });

            LongTermEffects.EasyRegister("Unicorn Companion", LongTermEffectDuration.UntilDowntime, (_, _) => {
                return new QEffect("Unicorn Companion", "You've acquired the aid of a unicorn. They will fight besides you until dying or the party returns to town.") {
                    ExpiresAt = ExpirationCondition.Never,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Unicorn Companion")),
                    StartOfCombat = async self => {
                        Creature companion = CreatureList.Creatures[CreatureId.UNICORN_FOAL](self.Owner.Battle.Encounter);
                        self.Owner.Battle.SpawnCreature(companion, Faction.CreateFriends(self.Owner.Battle), self.Owner.Occupies);
                        companion.AddQEffect(new QEffect() {
                            Source = self.Owner,
                            WhenMonsterDies = qfDeathCheck => {
                                self.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        });
                    },
                };
            });

            LongTermEffects.EasyRegister("Power of the Rat Fiend", LongTermEffectDuration.Forever, (_, _) => {
                return new QEffect("Power of the Rat Fiend", "You've claimed the power of the rat fiend for yourself. At the start of each encounter, spawn a friendly Rat to aid you.") {
                    ExpiresAt = ExpirationCondition.Never,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Power of the Rat Fiend")),
                    StartOfCombat = async self => {
                        FeatLoader.SpawnRatFamiliar(self.Owner);
                    },
                };
            });

            LongTermEffects.EasyRegister("Drow Renegade Companion", LongTermEffectDuration.UntilDowntime, (_, _) => {
                return new QEffect("Drow Renegade Companion", "You've acquired the aid of a Drow Renegade. She will fight besides you until dying or the party returns to town.") {
                    ExpiresAt = ExpirationCondition.Never,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Drow Renegade Companion")),
                    StartOfCombat = async self => {
                        Creature companion = CreatureList.Creatures[CreatureId.DROW_RENEGADE](self.Owner.Battle.Encounter);
                        self.Owner.Battle.SpawnCreature(companion, self.Owner.Battle.GaiaFriends, self.Owner.Occupies);
                        companion.AddQEffect(new QEffect() {
                            Source = self.Owner,
                            WhenMonsterDies = qfDeathCheck => {
                                self.ExpiresAt = ExpirationCondition.Immediately;
                            }
                        });
                    },
                };
            });

            LongTermEffects.EasyRegister("Injured", LongTermEffectDuration.UntilLongRest, (_, val) => {
                return new QEffect("Injured", $"You've sustained an injury that won't quite fully heal until you've had a full night's rest reducing your max HP by {val}0%.") {
                    Value = val,
                    StateCheck = self => {
                        self.Owner.DrainedMaxHPDecrease += (int)(0.1f * self.Value * self.Owner.MaxHP);
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Injured", null, val))
                };
            });

            LongTermEffects.EasyRegister("Guilt", LongTermEffectDuration.UntilLongRest, (_, val) => {
                return new QEffect("Guilt", $"Your failures weigh heavy on your conscience. You gain a -{val} status penalty to Will saves.") {
                    Value = val,
                    BonusToDefenses = (self, action, defence) => defence == Defense.Will ? new Bonus(-val, BonusType.Status, "Guilt") : null,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Guilt", null, val))
                };
            });

            LongTermEffects.EasyRegister("Hope", LongTermEffectDuration.UntilLongRest, (name, val) => {
                return new QEffect("Hope", $"You're spurred onwards by the changes your good deeds have wrought. You gain a +{val} status penalty to Will saves and attack rolls.") {
                    Value = val,
                    BonusToDefenses = (self, action, defence) => defence == Defense.Will ? new Bonus(val, BonusType.Status, "Hope") : null,
                    BonusToAttackRolls = (self, action, target) => new Bonus(val, BonusType.Status, "Hope"),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Hope", name, val))
                };
            });

            LongTermEffects.EasyRegister("Information Sharing", LongTermEffectDuration.UntilDowntime, (_, _) => {
                return new QEffect("Information Sharing", "The information about hazards, enemy movements and strongholds shared by the Drow Renegades grants you a +1 bonus to inititive.") {
                    BonusToInitiative = self => new Bonus(1, BonusType.Untyped, "Information Sharing"),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Information Sharing"))
                };
            });

            LongTermEffects.EasyRegister("Compromised Route", LongTermEffectDuration.UntilDowntime, (_, _) => {
                return new QEffect("Compromised Route", "The party's route was leaked by a spy. You suffer a -1 penalty to inititive.") {
                    BonusToInitiative = self => new Bonus(-1, BonusType.Untyped, "Compromised Route"),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Compromised Route"))
                };
            });

            LongTermEffects.EasyRegister("Mushroom Symbiote", LongTermEffectDuration.UntilLongRest, (_, _) => {
                return new QEffect("Mushroom Symbiote", "Your mushroom symbiote renders you immune to poison.") {
                    ImmuneToTrait = Trait.Poison,
                    StateCheck = self => {
                        self.Owner.WeaknessAndResistance.AddImmunity(DamageKind.Poison);
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Mushroom Symbiote"))
                };
            });

            LongTermEffects.EasyRegister("Lingering Curse", LongTermEffectDuration.UntilLongRest, (_, _) => {
                return new QEffect("Lingering Curse", "This creature is afflicted by a lingering curse, imposing Clumsy, Enfeebled and Stupidied 1 upon them.") {
                    Innate = false,
                    Illustration = IllustrationName.BestowCurse,
                    StateCheck = self => {
                        self.Owner.AddQEffect(QEffect.Clumsy(1).WithExpirationEphemeral());
                        self.Owner.AddQEffect(QEffect.Enfeebled(1).WithExpirationEphemeral());
                        self.Owner.AddQEffect(QEffect.Stupefied(1).WithExpirationEphemeral());
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Lingering Curse"))
                };
            });

            LongTermEffects.EasyRegister("Mushroom Sickness", LongTermEffectDuration.UntilLongRest, (_, value) => {
                QEffect effect = new QEffect("Mushroom Sickness", $"This creature is afflicted by sickness {value} for the duration of the encounter. Retching cannot be used to remove this sickness.") {
                    Innate = false,
                    Value = value,
                    Illustration = new SameSizeDualIllustration(Illustrations.StatusBackdrop, Illustrations.ChokingMushroom),
                    BonusToAllChecksAndDCs = (qf) => new Bonus(-qf.Value, BonusType.Status, "sickened"),
                    PreventTakingAction = (ca) => ca.ActionId != ActionId.Drink ? null : "You're sickened.",
                };
                effect.PreventTargetingBy = ca => ca.ActionId != ActionId.Administer || effect.Owner.HasEffect(QEffectId.Unconscious) ? null : "sickened";

                return effect;
            });

            #region Martial Colosseum Feats
            
            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.AggressiveBlock].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                var effect = Core.StatBlocks.Monsters.L5.Doorwarden.CreateAggressiveBlock();
                effect.EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.AggressiveBlock].Item2)!);

                return effect;
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.BrutalBeating].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("Brutal Beating", "Whenever your Strike is a critical hit and deals damage, the target is frightened 1.")
                {
                    AfterYouTakeAction = async delegate (QEffect qff, CombatAction action)
                    {
                        if (action.HasTrait(Trait.Strike) && action.CheckResult == CheckResult.CriticalSuccess)
                        {
                            Creature chosenCreature3 = action.ChosenTargets.ChosenCreature;
                            if (chosenCreature3 != null && chosenCreature3.HP >= 1)
                            {
                                if (chosenCreature3.IsImmuneTo(Trait.Mental))
                                {
                                    chosenCreature3.Battle.Log(chosenCreature3?.ToString() + " is immune to mental effects and can't be frightened.");
                                }
                                else
                                {
                                    chosenCreature3.Occupies.Overhead("brutal beating", Color.Red, chosenCreature3?.ToString() + " became frightened because of " + qff.Owner?.ToString() + "'s brutal beating.");
                                    chosenCreature3.AddQEffect(QEffect.Frightened(1));
                                }
                            }
                        }
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.BrutalBeating].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.GravityWeapon].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("Gravity Weapon", "You gain the {i}gravity weapon{/i} warden spell and a focus pool of 1 Focus Point.")
                {
                    StartOfCombat = async (effect) =>
                    {
                        AddFocusSpell(effect.Owner, SpellId.GravityWeapon, Ability.Wisdom, Trait.Primal, Trait.Ranger);
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.GravityWeapon].Item2)!)
                };
            });
            
            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.KiRush].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("Ki Rush", "You gain the {i}ki rush{/i} warden spell and a focus pool of 1 Focus Point.")
                {
                    StartOfCombat = async (effect) =>
                    {
                        AddFocusSpell(effect.Owner, SpellId.KiRush, Ability.Wisdom, Trait.Divine, Trait.Monk);
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.KiRush].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.Mobility].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("Mobility", "You don't provoke attacks of opportunity with short movements.")
                {
                    Id = QEffectId.Mobility,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.Mobility].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.NimbleDodge].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("Nimble Dodge {icon:Reaction}", "You gain a +2 bonus to AC as a reaction.")
                {
                    YouAreTargeted = async delegate (QEffect qff, CombatAction attack)
                    {
                        CombatAction attack2 = attack;
                        Creature rogue5 = qff.Owner;
                        if (attack2.HasTrait(Trait.Attack) && rogue5.CanSee(attack2.Owner) && !attack2.HasTrait(Trait.AttackDoesNotTargetAC))
                        {
                            if (await rogue5.Battle.AskToUseReaction(rogue5, "You're targeted by " + attack2.Owner.Name + "'s " + attack2.Name + ".\nUse Nimble Dodge to gain a +2 circumstance bonus to AC?"))
                            {
                                rogue5.AddQEffect(new QEffect
                                {
                                    ExpiresAt = ExpirationCondition.EphemeralAtEndOfImmediateAction,
                                    BonusToDefenses = (QEffect effect, CombatAction? action, Defense defense) => (defense != 0) ? null : new Bonus(2, BonusType.Circumstance, "Nimble Dodge")
                                });
                            }
                        }
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.NimbleDodge].Item2)!)
                };
            });
            
            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.PowerAttack].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                var effect = new QEffect("Power Attack", "You unleash a particularly powerful attack.")
                {
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.PowerAttack].Item2)!)
                };

                effect.ProvideStrikeModifier = delegate (Item item)
                {
                    Item item5 = item;
                    if (item5.HasTrait(Trait.Melee))
                    {
                        StrikeModifiers strikeModifiers3 = new StrikeModifiers
                        {
                            AdditionalWeaponDamageDice = 1,
                            OnEachTarget = async delegate (Creature a, Creature d, CheckResult result)
                            {
                                a.Actions.AttackedThisManyTimesThisTurn++;
                            }
                        };
                        CombatAction combatAction10 = effect.Owner.CreateStrike(item5, -1, strikeModifiers3);
                        combatAction10.Name = "Power Attack";
                        combatAction10.Illustration = new SideBySideIllustration(combatAction10.Illustration, IllustrationName.StarHit);
                        combatAction10.ActionCost = 2;
                        combatAction10.Description = StrikeRules.CreateBasicStrikeDescription2(combatAction10.StrikeModifiers, null, null, null, null, "Your multiple attack penalty increases twice instead of just once.");

                        combatAction10.Traits.Add(Trait.Basic);
                        combatAction10.Traits.Add(Trait.Flourish);
                        return combatAction10;
                    }

                    return null;
                };

                return effect;
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.QuickDraw].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("Quick Draw", "You draw weapons as a free action.")
                {
                    Id = QEffectId.QuickDraw,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.QuickDraw].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.RapidResponse].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("Rapid Response {icon:Reaction}", "When an ally begins dying, you Stride towards them.")
                {
                    StateCheck = delegate (QEffect qfRapidResponse)
                    {
                        QEffect qfRapidResponse2 = qfRapidResponse;
                        foreach (Creature item3 in qfRapidResponse2.Owner.Battle.AllCreatures.Where((Creature cr) => cr.FriendOf(qfRapidResponse2.Owner) && cr != qfRapidResponse2.Owner))
                        {
                            item3.AddQEffect(new QEffect(ExpirationCondition.Ephemeral)
                            {
                                AfterYouTakeDamage = async delegate (QEffect qfInjuredCreature, int damageDealt, DamageKind damageKind, CombatAction? inflictingPower, bool criticalHit)
                                {
                                    if (qfInjuredCreature.Owner.HP <= 0)
                                    {
                                        Creature ally = qfInjuredCreature.Owner;
                                        Creature medic = qfRapidResponse2.Owner;
                                        TBattle battle = medic.Battle;
                                        if (await battle.AskToUseReaction(medic, "An ally (" + ally?.ToString() + ") was reduced to 0 HP. Stride towards the ally?"))
                                        {
                                            medic.AddQEffect(new QEffect(ExpirationCondition.EphemeralAtEndOfImmediateAction)
                                            {
                                                BonusToAllSpeeds = (QEffect _) => new Bonus(2, BonusType.Circumstance, "Rapid Response")
                                            });
                                            await medic.StrideAsync("Choose where to Stride with Rapid Response (towards " + ally?.ToString() + ").", allowStep: false, maximumFiveFeet: false, ally.Occupies, allowCancel: false, allowPass: true);
                                        }
                                    }
                                }
                            });
                        }
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.RapidResponse].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.ReactiveShield].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                var effect = QEffect.ReactiveShield();
                effect.EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.ReactiveShield].Item2)!);
                
                return effect;
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.ShakeItOff].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect()
                {
                    ProvideContextualAction = delegate (QEffect qfSelf)
                    {
                        Creature owner = qfSelf.Owner;
                        return (owner.HasEffect(QEffectId.Frightened) || owner.HasEffect(QEffectId.Sickened)) ? new ActionPossibility(new CombatAction(owner, IllustrationName.ShakeItOff, "Shake it Off", [Trait.Concentrate],
                            "Reduce your frightened condition value by 1, and attempt a Fortitude save to recover from the sickened condition as if you had spent an action retching; you reduce your sickened condition value by 1 on a failure (but not on a critical failure), by 2 on a success, or by 3 on a critical success.", Target.Self()).WithEffectOnSelf(async delegate (CombatAction action, Creature cr)
                        {
                            QEffect qEffect = cr.QEffects.FirstOrDefault((QEffect qff) => qff.Id == QEffectId.Frightened);
                            if (qEffect != null)
                            {
                                qEffect.Value--;
                                if (qEffect.Value <= 0)
                                {
                                    qEffect.ExpiresAt = ExpirationCondition.Immediately;
                                }
                            }

                            QEffect qEffect2 = cr.QEffects.FirstOrDefault((QEffect qff) => qff.Id == QEffectId.Sickened);
                            if (qEffect2 != null)
                            {
                                int dc = (int)qEffect2.Tag;
                                switch (CommonSpellEffects.RollSavingThrow(cr, action, Defense.Fortitude, dc))
                                {
                                    case CheckResult.Failure:
                                        qEffect2.Value--;
                                        break;
                                    case CheckResult.Success:
                                        qEffect2.Value -= 2;
                                        break;
                                    case CheckResult.CriticalSuccess:
                                        qEffect2.Value -= 3;
                                        break;
                                }

                                if (qEffect2.Value <= 0)
                                {
                                    qEffect2.ExpiresAt = ExpirationCondition.Immediately;
                                }
                            }
                        })).WithPossibilityGroup("Remove debuff") : null;
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.ShakeItOff].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.SuddenCharge].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect()
                {
                    ProvideMainAction = (QEffect qfSelf) => new ActionPossibility(new CombatAction(qfSelf.Owner, IllustrationName.FleetStep, "Sudden Charge",
                    [
                        Trait.Flourish,
                        Trait.Open,
                        Trait.Move
                    ], "Stride twice. If you end your movement within melee reach of at least one enemy, you can make a melee Strike against that enemy.", Target.Self()).WithActionCost(2).WithSoundEffect(SfxName.Footsteps).WithEffectOnSelf(async delegate (CombatAction action, Creature self)
                    {
                        if (!(await self.StrideAsync("Choose where to Stride with Sudden Charge. (1/2)", allowStep: false, maximumFiveFeet: false, null, allowCancel: true)))
                        {
                            action.RevertRequested = true;
                        }
                        else
                        {
                            await self.StrideAsync("Choose where to Stride with Sudden Charge. You should end your movement within melee reach of an enemy. (2/2)", allowStep: false, maximumFiveFeet: false, null, allowCancel: false, allowPass: true);
                            await CommonCombatActions.StrikeAdjacentCreature(self, null);
                        }
                    })),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.SuddenCharge].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.YoureNext].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("You're Next {icon:Reaction}", "After you reduce an enemy to 0 HP, you can spend a reaction to Demoralize one creature. You have a +2 circumstance bonus to this check.")
                {
                    AfterYouTakeAction = async delegate (QEffect qff, CombatAction action)
                    {
                        if (action.HasTrait(Trait.Strike) && action.CheckResult == CheckResult.CriticalSuccess)
                        {
                            Creature chosenCreature3 = action.ChosenTargets.ChosenCreature;
                            if (chosenCreature3 != null && chosenCreature3.HP >= 1)
                            {
                                if (chosenCreature3.IsImmuneTo(Trait.Mental))
                                {
                                    chosenCreature3.Battle.Log(chosenCreature3?.ToString() + " is immune to mental effects and can't be frightened.");
                                }
                                else
                                {
                                    chosenCreature3.Occupies.Overhead("brutal beating", Color.Red, chosenCreature3?.ToString() + " became frightened because of " + qff.Owner?.ToString() + "'s brutal beating.");
                                    chosenCreature3.AddQEffect(QEffect.Frightened(1));
                                }
                            }
                        }
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.YoureNext].Item2)!)
                };
            });

            #endregion
        }

        private static void AddFocusSpell(Creature creature, SpellId spell, Ability ability, Trait tradition, Trait source)
        {
            Spellcasting? casting = new(creature);

            if (creature.Spellcasting == null)
            {
                creature.Spellcasting = casting;
            }
            else
            {
                casting = creature.Spellcasting;
            }

            if (creature.Spellcasting.FocusPointsMaximum < 3)
            {
                creature.Spellcasting.FocusPointsMaximum++;
                creature.Spellcasting.FocusPoints++;
            }

            foreach (var spellCastingSource in creature.Spellcasting.Sources)
            {
                if (spellCastingSource.ClassOfOrigin == source)
                {
                    spellCastingSource.FocusSpells.Add(AllSpells.CreateModernSpell(spell, creature, creature.MaximumSpellRank, true, new SpellInformation
                    {
                        ClassOfOrigin = source
                    }).CombatActionSpell);

                    return;
                }
            }

            var newSource = new SpellcastingSource(casting, SpellcastingKind.Innate, ability, tradition, source);
            newSource.FocusSpells.Add(AllSpells.CreateModernSpell(SpellId.GravityWeapon, creature, creature.MaximumSpellRank, true, new SpellInformation
            {
                ClassOfOrigin = source
            }).CombatActionSpell);
            creature.Spellcasting.Sources.Add(newSource);
        }
    }
}
