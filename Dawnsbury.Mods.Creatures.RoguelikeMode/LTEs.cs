using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury;
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
using static Dawnsbury.Mods.Creatures.RoguelikeMode.ModEnums;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class LTEs {

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

            LongTermEffects.EasyRegister("Test Boon", LongTermEffectDuration.Forever, (string _, int _) => {
                return new QEffect("Test Boon", "Test to see if this shows up on chaacter sheets outside of combat.") {
                    
                };
            });

            LongTermEffects.EasyRegister("Drow Renegade Companion", LongTermEffectDuration.UntilDowntime, (string _, int _) => {
                return new QEffect("Drow Renegade Companion", "You've acquired the aid of a Drow Renegade. She will fight besides you until dying or the party returns to town.") {
                    StartOfCombat = async self => {
                        Creature companion = CreatureList.Creatures[CreatureId.DROW_RENEGADE](self.Owner.Battle.Encounter);
                        self.Owner.Battle.SpawnCreature(companion, Faction.CreateFriends(self.Owner.Battle), self.Owner.Occupies);
                        companion.AddQEffect(new QEffect() {
                            Source = self.Owner,
                            WhenMonsterDies = qfDeathCheck => self.ExpiresAt = ExpirationCondition.Immediately
                        });
                        //self.Tag = companion;
                    },

                };
            });

            LongTermEffects.EasyRegister("Injured", LongTermEffectDuration.UntilLongRest, (string _, int val) => {
                return new QEffect("Injured", $"You've sustained an injury that won't quite fully heal until you've had a full night's rest reducing your max HP by {val}0%.") {
                    Value = val,
                    StateCheck = self => {
                        self.Owner.DrainedMaxHPDecrease += (int)(0.1f * self.Value * self.Owner.TrueMaximumHP);
                    }
                };
            });

            LongTermEffects.EasyRegister("Guilt", LongTermEffectDuration.UntilLongRest, (string _, int val) => {
                return new QEffect("Guilt", $"Your failures weigh heavy on your conscience. You gain a -{val} status penalty to Will saves.") {
                    Value = val,
                    BonusToDefenses = (self, action, defence) => defence == Defense.Will ? new Bonus(-val, BonusType.Status, "Guilt") : null
                };
            });

            LongTermEffects.EasyRegister("Hope", LongTermEffectDuration.UntilLongRest, (string name, int val) => {
                return new QEffect("Guilt", $"You're spurred onwards by the changes your good deeds have wrought. You gain a +{val} status penalty to Will saves and attack rolls.") {
                    Value = val,
                    BonusToDefenses = (self, action, defence) => defence == Defense.Will ? new Bonus(val, BonusType.Status, "Hope") : null,
                    BonusToAttackRolls = (self, action, target) => new Bonus(val, BonusType.Status, "Hope")
                };
            });

            LongTermEffects.EasyRegister("Information Sharing", LongTermEffectDuration.UntilDowntime, (string _, int _) => {
                return new QEffect("Information Sharing", "The information about hazards, enemy movements and strongholds shared by the Drow Renegades grants you a +1 bonus to inititive.") {
                    BonusToInitiative = self => new Bonus(1, BonusType.Untyped, "Information Sharing")
                };
            });

            LongTermEffects.EasyRegister("Compromised Route", LongTermEffectDuration.UntilDowntime, (string _, int _) => {
                return new QEffect("Compromised Route", "The party's route was leaked by a spy. You suffer a -1 penalty to inititive.") {
                    BonusToInitiative = self => new Bonus(-1, BonusType.Untyped, "Compromised Route")
                };
            });

            LongTermEffects.EasyRegister("Mushroom Symbiote", LongTermEffectDuration.UntilLongRest, (string _, int _) => {
                return new QEffect("Mushroom Symbiote", "Your mushroom symbiote renders you immune to poison.") {
                    ImmuneToTrait = Trait.Poison,
                    StateCheck = self => {
                        self.Owner.WeaknessAndResistance.AddImmunity(DamageKind.Poison);
                    }
                };
            });

            LongTermEffects.EasyRegister("Lingering Curse", LongTermEffectDuration.UntilLongRest, (string _, int _) => {
                return new QEffect("Lingering Curse", "This creature is afflicted by a lingering curse, imposing Clumsy, Enfeebled and Stupidied 1 upon them.") {
                    Innate = false,
                    Illustration = IllustrationName.BestowCurse,
                    StateCheck = self => {
                        self.Owner.AddQEffect(QEffect.Clumsy(1).WithExpirationEphemeral());
                        self.Owner.AddQEffect(QEffect.Enfeebled(1).WithExpirationEphemeral());
                        self.Owner.AddQEffect(QEffect.Stupefied(1).WithExpirationEphemeral());
                    }
                };
            });

            LongTermEffects.EasyRegister("Mushroom Sickness", LongTermEffectDuration.UntilLongRest, (string _, int value) => {
                QEffect effect = new QEffect("Mushroom Sickness", $"This creature is afflicted by sickness {value} for the duration of the encounter. Retching cannot be used to remove this sickness.") {
                    Innate = false,
                    Value = value,
                    Illustration = new SameSizeDualIllustration(Illustrations.StatusBackdrop, Illustrations.ChokingMushroom),
                    BonusToAllChecksAndDCs = (QEffect qf) => new Bonus(-qf.Value, BonusType.Status, "sickened"),
                    PreventTakingAction = (CombatAction ca) => (ca.ActionId != ActionId.Drink) ? null : "You're sickened.",
                    EndOfCombat = async (self, victory) => {
                        if (victory) {
                            self.ExpiresAt = ExpirationCondition.Immediately;
                        }
                    }
                };
                effect.PreventTargetingBy = ca => ca.ActionId != ActionId.Administer || effect.Owner.HasEffect(QEffectId.Unconscious) ? (string)null : "sickened";

                return effect;
            });


        }
    }
}
