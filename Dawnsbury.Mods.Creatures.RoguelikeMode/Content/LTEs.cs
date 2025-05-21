using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
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
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Kineticist;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Animations;
using Dawnsbury.Display.Text;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Champion;
using Dawnsbury.Display;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class LTEs {

        internal enum ColosseumFeat
        {
            AggressiveBlock,
            BrutalBeating,
            BurningJet,
            DangerousSorcery,
            DesperatePrayer,
            FlyingFlame,
            ForceFang,
            FourWinds,
            GravityWeapon,
            KiRush,
            LayOnHands,
            LesserFireShieldStance,
            LesserLevitationStance,
            Mobility,
            NimbleDodge,
            OceansBalm,
            PowerAttack,
            QuickDraw,
            RapidResponse,
            ReachSpell,
            ReactiveShield,
            ShakeItOff,
            ShardStrike,
            SuddenCharge,
            TimberSentinel,
            WidenSpell,
            WintersClutch,
            YoureNext
        }

        internal static Dictionary<ColosseumFeat, Tuple<FeatName, string>> ColosseumFeatNames = new()
        {
            { ColosseumFeat.AggressiveBlock, new(FeatName.AggressiveBlock, "Aggressive Block") },
            { ColosseumFeat.BrutalBeating, new(FeatName.BrutalBeating, "Brutal Beating") },
            { ColosseumFeat.BurningJet, new(FeatName.BurningJet, "Burning Jet") },
            { ColosseumFeat.DangerousSorcery, new(FeatName.DangerousSorcery, "Dangerous Sorcery") },
            { ColosseumFeat.FlyingFlame, new(FeatName.FlyingFlame, "Flying Flame") },
            { ColosseumFeat.ForceFang, new(FeatName.ForceFang, "Force Fang") },
            { ColosseumFeat.FourWinds, new(FeatName.FourWinds, "Four Winds") },
            { ColosseumFeat.GravityWeapon, new(FeatName.GravityWeapon, "Gravity Weapon") },
            { ColosseumFeat.KiRush, new(FeatName.KiRush, "Ki Rush") },
            { ColosseumFeat.LayOnHands, new(FeatName.Paladin, "Lay on Hands") },
            { ColosseumFeat.LesserFireShieldStance, new(FeatName.LesserFireShieldStance, "Lesser Fire Shield Stance") },
            { ColosseumFeat.LesserLevitationStance, new(FeatName.LesserLevitationStance, "Lesser Levitation Stance") },
            { ColosseumFeat.Mobility, new(FeatName.Mobility, "Mobility") },
            { ColosseumFeat.NimbleDodge, new(FeatName.NimbleDodge, "Nimble Dodge") },
            { ColosseumFeat.OceansBalm, new(FeatName.OceansBalm, "Ocean's Balm") },
            { ColosseumFeat.PowerAttack, new(FeatName.PowerAttack, "Power Attack") },
            { ColosseumFeat.QuickDraw, new(FeatName.QuickDraw, "Quick Draw") },
            { ColosseumFeat.RapidResponse, new(FeatName.RapidResponse, "Rapid Response") },
            { ColosseumFeat.ReachSpell, new(FeatName.ReachSpell, "Reach Spell") },
            { ColosseumFeat.ReactiveShield, new(FeatName.ReactiveShield, "Reactive Shield") },
            { ColosseumFeat.ShakeItOff, new(FeatName.ShakeItOff, "Shake It Off") },
            { ColosseumFeat.ShardStrike, new(FeatName.ShardStrike, "Shard Strike") },
            { ColosseumFeat.SuddenCharge, new(FeatName.SuddenCharge, "Sudden Charge") },
            { ColosseumFeat.TimberSentinel, new(FeatName.TimberSentinel, "Timber Sentinel") },
            { ColosseumFeat.WidenSpell, new(FeatName.WidenSpell, "Widen Spell") },
            { ColosseumFeat.WintersClutch, new(FeatName.WintersClutch, "Winter's Clutch") },
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

            LongTermEffects.EasyRegister("Heavenly Favour", LongTermEffectDuration.Forever, (_, _) => {
                return new QEffect("Heavenly Favour", "The gods bless your cause, imparting a +1 bonus to your attack, save DCs, spell DC and AC.") {
                    HideFromPortrait = true,
                    Illustration = IllustrationName.AngelicWings,
                    ExpiresAt = ExpirationCondition.Never,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Heavenly Favour")),
                    BonusToAttackRolls = (self, action, target) => new Bonus(1, BonusType.Untyped, "Heavenly Favour (Easy Mode)"),
                    BonusToDefenses = (self, action, def) => new Bonus(1, BonusType.Untyped, "Heavenly Favour (Easy Mode)"),
                    BonusToSpellSaveDCs = (self) => new Bonus(1, BonusType.Untyped, "Heavenly Favour (Easy Mode)"),
                };
            });

            LongTermEffects.EasyRegister("Unicorn Companion", LongTermEffectDuration.UntilDowntime, (_, _) => {
                return new QEffect("Unicorn Companion", "You've acquired the aid of a unicorn. They will fight besides you until dying or the party returns to town.") {
                    HideFromPortrait = true,
                    Illustration = Illustrations.Unicorn,
                    ExpiresAt = ExpirationCondition.Never,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Unicorn Companion")),
                    StartOfCombat = async self => {
                        Creature companion = CreatureList.Creatures[CreatureIds.UnicornFoal](self.Owner.Battle.Encounter);
                        self.Owner.Battle.SpawnCreature(companion, Faction.CreateFriends(self.Owner.Battle), self.Owner.Occupies);
                        companion.AddQEffect(CommonQEffects.CantOpenDoors());
                        companion.AddQEffect(new QEffect() {
                            HideFromPortrait = true,
                            Illustration = Illustrations.Unicorn,
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
                    HideFromPortrait = true,
                    Illustration = IllustrationName.GiantRat256,
                    ExpiresAt = ExpirationCondition.Never,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Power of the Rat Fiend")),
                    StartOfCombat = async self => {
                        FeatLoader.SpawnRatFamiliar(self.Owner);
                    },
                };
            });

            LongTermEffects.EasyRegister("Curse of the Rat Fiend", LongTermEffectDuration.UntilLongRest, (_, _) => {
                return new QEffect("Curse of the Rat Fiend", "Each enemy you defeat has a 25% chance of spawning a Giant Rat from its corpse.") {
                    HideFromPortrait = true,
                    Illustration = IllustrationName.BestowCurse,
                    AfterYouDealDamage = async (owner, action, defender) => {
                        if (defender.HP <= 0 && defender.OwningFaction.EnemyFactionOf(owner.OwningFaction) && defender.BaseName != "Giant Rat") {
                            if (R.NextD20() <= 15) {
                                return;
                            }
                            var rat = MonsterStatBlocks.CreateGiantRat();
                            if (owner.Level == 2) {
                                rat = rat.ApplyEliteAdjustments();
                            } else if (owner.Level == 3) {
                                rat = rat.ApplyEliteAdjustments(true);
                            }
                            owner.Battle.SpawnCreature(rat, defender.OwningFaction, defender.Occupies);
                            owner.Occupies.Overhead("Curse of the Rat Fiend!", Color.Red, $"A giant rat crawls up out of {defender.Name}'s corpse, thanks to the curse of the Rat Fiend.");
                        }
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Curse of the Rat Fiend")),
                };
            });

            LongTermEffects.EasyRegister("Drow Renegade Companion", LongTermEffectDuration.UntilDowntime, (_, _) => {
                return new QEffect("Drow Renegade Companion", "You've acquired the aid of a Drow Renegade. She will fight besides you until dying or the party returns to town.") {
                    HideFromPortrait = true,
                    Illustration = Illustrations.DrowRenegade,
                    ExpiresAt = ExpirationCondition.Never,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Drow Renegade Companion")),
                    StartOfCombat = async self => {
                        Creature companion = CreatureList.Creatures[CreatureIds.DrowRenegade](self.Owner.Battle.Encounter);
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
                    HideFromPortrait = true,
                    Illustration = IllustrationName.Wounded,
                    Value = val,
                    StateCheck = self => {
                        self.Owner.DrainedMaxHPDecrease += (int)(0.1f * self.Value * self.Owner.MaxHP);
                        self.Owner.AddQEffect(new QEffect() { Id = QEffectId.Drained }.WithExpirationEphemeral());
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Injured", null, val))
                };
            });

            LongTermEffects.EasyRegister("Unicorn's Curse", LongTermEffectDuration.UntilLongRest, (_, _) => {
                return new QEffect("Unicorn's Curse", $"You've been cursed by a unicorn for attempting to poach it, reducing your max HP by 5 and your saves by 1 until you take a long rest.") {
                    HideFromPortrait = true,
                    Illustration = IllustrationName.BestowCurse,
                    StateCheck = self => {
                        self.Owner.DrainedMaxHPDecrease += 5;
                        self.Owner.AddQEffect(new QEffect() { Id = QEffectId.Drained }.WithExpirationEphemeral());
                    },
                    BonusToDefenses = (self, action, def) => def != Defense.AC ? new Bonus(-1, BonusType.Untyped, "Unicorn's Curse") : null,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Unicorn's Curse", null, null))
                };
            });

            LongTermEffects.EasyRegister("Unicorn's Blessing", LongTermEffectDuration.UntilLongRest, (_, _) => {
                return new QEffect("Unicorn's Blessing", $"You've been blessed by a unicorn using the last of its dying strength, increasing your max HP by 5 and your saves by +1 until you take a long rest.") {
                    HideFromPortrait = true,
                    Illustration = IllustrationName.Bless,
                    StartOfCombat = async self => self.Owner.MaxHP += 5,
                    BonusToDefenses = (self, action, def) => def != Defense.AC ? new Bonus(1, BonusType.Untyped, "Unicorn's Blessing") : null,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Unicorn's Blessing", null, null))
                };
            });

            LongTermEffects.EasyRegister("Guilt", LongTermEffectDuration.UntilLongRest, (_, val) => {
                return new QEffect("Guilt", $"Your failures weigh heavy on your conscience. You gain a -{val} status penalty to Will saves.") {
                    HideFromPortrait = true,
                    Illustration = IllustrationName.Fear,
                    Value = val,
                    BonusToDefenses = (self, action, defence) => defence == Defense.Will ? new Bonus(-val, BonusType.Status, "Guilt") : null,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Guilt", null, val))
                };
            });

            LongTermEffects.EasyRegister("Hope", LongTermEffectDuration.UntilLongRest, (name, val) => {
                return new QEffect("Hope", $"You're spurred onwards by the changes your good deeds have wrought. You gain a +{val} status penalty to Will saves and attack rolls.") {
                    HideFromPortrait = true,
                    Illustration = IllustrationName.Heroism,
                    Value = val,
                    BonusToDefenses = (self, action, defence) => defence == Defense.Will ? new Bonus(val, BonusType.Status, "Hope") : null,
                    BonusToAttackRolls = (self, action, target) => new Bonus(val, BonusType.Status, "Hope"),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Hope", name, val))
                };
            });

            LongTermEffects.EasyRegister("Information Sharing", LongTermEffectDuration.UntilDowntime, (_, _) => {
                return new QEffect("Information Sharing", "The information about hazards, enemy movements and strongholds shared by the Drow Renegades grants you a +1 bonus to inititive.") {
                    HideFromPortrait = true,
                    Illustration = IllustrationName.DeviseAStratagem,
                    BonusToInitiative = self => new Bonus(1, BonusType.Untyped, "Information Sharing"),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Information Sharing"))
                };
            });

            LongTermEffects.EasyRegister("Compromised Route", LongTermEffectDuration.UntilDowntime, (_, _) => {
                return new QEffect("Compromised Route", "The party's route was leaked by a spy. You suffer a -1 penalty to inititive.") {
                    HideFromPortrait = true,
                    Illustration = IllustrationName.Sneak64,
                    BonusToInitiative = self => new Bonus(-1, BonusType.Untyped, "Compromised Route"),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Compromised Route"))
                };
            });

            LongTermEffects.EasyRegister("Mushroom Symbiote", LongTermEffectDuration.UntilLongRest, (_, _) => {
                return new QEffect("Mushroom Symbiote", "Your mushroom symbiote renders you immune to poison.") {
                    HideFromPortrait = true,
                    Illustration = Illustrations.ChokingMushroom,
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

            LongTermEffects.EasyRegister("Well Spirit's Blessing", LongTermEffectDuration.UntilLongRest, (_, _) => {
                return new QEffect("Well Spirit's Blessing", $"You've been blessed by the spirit of a magic well, increasing and your will saves by +1 until you take a long rest.") {
                    HideFromPortrait = true,
                    Illustration = IllustrationName.Bless,
                    BonusToDefenses = (self, action, def) => def == Defense.Will ? new Bonus(1, BonusType.Untyped, "Well Spirit's Blessing") : null,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Well Spirit's Blessing", null, null))
                };
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
                return new QEffect("Ki Rush", "You gain the {i}ki rush{/i} ki spell and a focus pool of 1 Focus Point.")
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
            
            #region Magical Colosseum Feats

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.BurningJet].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect()
                {
                    ProvideMainAction = (QEffect qfSelf) => new ActionPossibility(CreateBasicImpulse(qfSelf.Owner, "Burning Jet", FeatName.BurningJet, IllustrationName.BurningJet,
                    [
                        Trait.Fire,
                        Trait.DoesNotRequireAttackRollOrSavingThrow
                    ], 1, "A condensed burst of flame shoots behind you, propelling you forward with its sheer force.", (qfSelf.Owner.Level >= 6) ? "{b}Leap{/b} up to {b}60{/b} feet. This leap doesn't trigger reactions." : "Stride up to 40 feet in a straight line. Movement from this impulse ignores difficult and uneven terrain and doesn't trigger reactions.", (qfSelf.Owner.Level >= 6) ? Target.LeapTarget(12) : Target.Line(8).WithLesserDistanceIsOkay().WithIsBurningJet()).WithSoundEffect(SfxName.RejuvenatingFlames).WithImpulseHeighteningAtSpecificLevel(qfSelf.Owner.Level, 6, "Instead of striding 40 feet, you leap 60 feet in any direction.")
                    .WithEffectOnChosenTargets(async delegate (CombatAction spell, Creature caster, ChosenTargets targets)
                    {
                        if (caster.Level >= 6)
                        {
                            await caster.SingleTileMove(targets.ChosenTile, spell);
                        }
                        else
                        {
                            Tile tile = LineAreaTarget.DetermineFinalTile(caster.Occupies, targets.ChosenTiles);
                            if (tile != null)
                            {
                                await caster.MoveTo(tile, spell, new MovementStyle
                                {
                                    Shifting = true,
                                    IgnoresUnevenTerrain = true,
                                    ShortestPath = true,
                                    MaximumSquares = 100
                                });
                            }
                        }
                    })),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.BurningJet].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.DangerousSorcery].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("Dangerous Sorcery", "Your damage spells deal extra damage.")
                {
                    BonusToDamage = (QEffect qfSelf, CombatAction spell, Creature target) => (spell.HasTrait(Trait.Spell) && !spell.HasTrait(Trait.Cantrip) && !spell.HasTrait(Trait.Focus) && spell.CastFromScroll == null) ? new Bonus(spell.SpellLevel, BonusType.Status, "Dangerous Sorcery") : null,
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.DangerousSorcery].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.FlyingFlame].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect()
                {
                    ProvideMainAction = (QEffect qfSelf) => new ActionPossibility(CreateBasicImpulse(qfSelf.Owner, "Flying Flame", FeatName.FlyingFlame, IllustrationName.FlyingFlame, [Trait.Fire], 1, "A tiny shape of flame travels along a path you choose, burning your enemies.", "Choose a path up to 30 feet long starting from your space.\n\nDeal " + S.HeightenedVariable((qfSelf.Owner.Level + 1) / 2, 1) + "d6 fire damage to each creature the path passes through (basic Reflex save mitigates).\n\nA creature attempts only one save, even if the flame passes through it multiple times.", new FlyingFlameTarget()).WithSoundEffect(SfxName.FireRay).WithSavingThrow(new(Defense.Reflex, qfSelf.Owner.ClassOrSpellDC()))
                    .WithImpulseHeighteningNumerical(qfSelf.Owner.Level, 1, 2, "The damage increases by 1d6.")
                    .WithEffectOnEachTarget(async delegate (CombatAction spell, Creature caster, Creature target, CheckResult result)
                    {
                        await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, DiceFormula.FromText(caster.MaximumSpellRank + "d6", "Flying Flame"), DamageKind.Fire);
                    })),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.FlyingFlame].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.ForceFang].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("Force Fang", "You gain the {i}force fang{/i} conflux spell and a focus pool of 1 Focus Point.")
                {
                    StartOfCombat = async (effect) =>
                    {
                        AddFocusSpell(effect.Owner, SpellId.ForceFang, Ability.Intelligence, Trait.Arcana, Trait.Magus);
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.ForceFang].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.FourWinds].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect()
                {
                    ProvideMainAction = (QEffect qfSelf) => new ActionPossibility(CreateBasicImpulse(qfSelf.Owner, "Four Winds", FeatName.FourWinds, IllustrationName.FourWinds,
                    [
                        Trait.Air,
                        Trait.DoesNotRequireAttackRollOrSavingThrow
                    ], 1, "Gathering the winds from all four corners of the sky dome, you propel four creatures.", "You and each ally within 30 feet of you can Stride up to half its Speed.", Target.ThirtyFootEmanation().WithIncludeOnlyIf((AreaTarget target, Creature ally) => ally.FriendOf(target.OwnerAction.Owner))).WithSoundEffect(SfxName.GaleBlast).WithEffectOnChosenTargets(async delegate (CombatAction action, Creature self, ChosenTargets targets)
                    {
                        foreach (Creature chosenCreature in targets.ChosenCreatures)
                        {
                            await chosenCreature.StrideAsync("Four Winds: " + chosenCreature?.ToString() + " may Stride up to " + 5 * (chosenCreature.Speed / 2) + " feet.", allowStep: false, maximumFiveFeet: false, null, allowCancel: false, allowPass: true, maximumHalfSpeed: true);
                        }
                    })),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.FourWinds].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.LayOnHands].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("Lay on Hands", "You gain the {i}lay on hands{/i} devotion spell and a focus pool of 1 Focus Point.")
                {
                    StartOfCombat = async (effect) =>
                    {
                        AddFocusSpell(effect.Owner, ChampionFocusSpells.LayOnHands, Ability.Charisma, Trait.Divine, Trait.Champion);
                    },
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.LayOnHands].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.LesserFireShieldStance].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect()
                {
                    ProvideMainAction = (QEffect qfSelf) => new ActionPossibility(CreateBasicImpulse(qfSelf.Owner, "Lessser Fire Shield Stance", FeatName.LesserFireShieldStance, IllustrationName.LesserFireShieldStance,
                    [
                        Trait.Fire,
                        Trait.Homebrew,
                        Trait.Stance
                    ], 1, "You wreathe yourself in flames that lash out at any attackers.", "You enter a stance. While you're in this stance, each time a creature hits you with a melee attack, that creature takes " + S.HeightenedVariable((qfSelf.Owner.Level + 2) / 3, 1) + " fire damage (no save).", Target.Self().WithAdditionalRestriction((Creature self) => (!self.QEffects.Any((QEffect qf) => qf.IsStance && qf.Name == "Lesser Fire Shield Stance")) ? null : "You're already in this stance.")).WithSoundEffect(SfxName.FireRay).WithImpulseHeighteningNumerical(qfSelf.Owner.Level, 1, 3, "The damage increases by 1.")
                    .WithEffectOnSelf(async delegate (CombatAction fireShield, Creature self)
                    {
                        CombatAction fireShield2 = fireShield;
                        QEffect qEffect2 = KineticistCommonEffects.EnterStance(self, IllustrationName.LesserFireShieldStance, "Lesser Fire Shield Stance", "Each time a creature hits you with a melee attack, that creature takes " + (qfSelf.Owner.Level + 2) / 3 + " fire damage.");
                        qEffect2.AfterYouTakeDamage = async delegate (QEffect effect, int amount, DamageKind damageKind, CombatAction? combatAction, bool critical)
                        {
                            if (combatAction?.HasTrait(Trait.Melee) ?? false)
                            {
                                await CommonSpellEffects.DealDirectDamage(fireShield2, DiceFormula.FromText(((qfSelf.Owner.Level + 2) / 3).ToString(), "Lesser Fire Shield Stance"), combatAction.Owner, CheckResult.Failure, DamageKind.Fire);
                            }
                        };
                    })),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.LesserFireShieldStance].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.LesserLevitationStance].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect()
                {
                    ProvideMainAction = (QEffect qfSelf) => new ActionPossibility(CreateBasicImpulse(qfSelf.Owner, "Lesser Levitation Stance", FeatName.LesserLevitationStance, IllustrationName.LesserLevitationStance,
                    [
                        Trait.Air,
                        Trait.Homebrew,
                        Trait.Stance
                    ], 1, "You lift your feet just a few inches above ground, which gives you much greater mobility.", "You enter a stance. While you're in this stance, you ignore difficult terrain.", Target.Self().WithAdditionalRestriction((Creature self) => (!self.QEffects.Any((QEffect qf) => qf.IsStance && qf.Name == "Lesser Levitation Stance")) ? null : "You're already in this stance.")).WithSoundEffect(SfxName.Bless).WithEffectOnSelf(delegate (Creature self)
                    {
                        KineticistCommonEffects.EnterStance(self, IllustrationName.LesserLevitationStance, "Lesser Levitation Stance", "You ignore difficult terrain.", QEffectId.IgnoresDifficultTerrain);
                    })),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.LesserLevitationStance].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.OceansBalm].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect()
                {
                    ProvideMainAction = (QEffect qfSelf) => new ActionPossibility(CreateBasicImpulse(qfSelf.Owner, "Ocean's Balm", FeatName.OceansBalm, IllustrationName.OceansBalm,
                    [
                        Trait.Water,
                        Trait.Healing,
                        Trait.Manipulate,
                        Trait.Positive
                    ], 1, "A blessing of the living sea salves wounds and douses flames.", $"You or target adjacent ally regains {S.HeightenedVariable((qfSelf.Owner.Level + 1) / 2, 1)}d8 Hit Points and gains resistance {S.HeightenedVariable((qfSelf.Owner.Level + 3) / 2, 2)} to fire for the rest of the encounter. If it has persistent fire damage, it immediately attempts a flat check against DC 10 to remove it {{i}}(55% success chance){{/i}}. The target is then temporarily immune to Ocean's Balm for the rest of the encounter.", Target.AdjacentFriendOrSelf()).WithActionId(ActionId.OceansBalm).WithActionCost(1)
                    .WithSoundEffect(SfxName.OceansBalm)
                    .WithImpulseHeighteningNumerical(qfSelf.Owner.Level, 1, 2, "The healing increases by 1d8, and the resistance increases by 1.")
                    .WithEffectOnEachTarget(async delegate (CombatAction spell, Creature caster, Creature target, CheckResult result)
                    {
                        await target.HealAsync((qfSelf.Owner.Level + 1) / 2 + "d8", spell);
                        target.AddQEffect(QEffect.DamageResistance(DamageKind.Fire, (qfSelf.Owner.Level + 3) / 2).WithExpirationNever());
                        target.AddQEffect(QEffect.ImmunityToTargeting(ActionId.OceansBalm));
                        target.QEffects.FirstOrDefault((QEffect qf) => qf.Id == QEffectId.PersistentDamage && qf.GetPersistentDamageKind() == DamageKind.Fire)?.RollPersistentDamageRecoveryCheck(assisted: true);
                    })),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.OceansBalm].Item2)!)
                };
            });
            
            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.ReachSpell].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("Reach Spell", "You can extend the range of your spells.")
                {
                    MetamagicProvider = new MetamagicProvider("Reach spell", delegate (CombatAction spell)
                    {
                        CombatAction metamagicSpell = Spell.DuplicateSpell(spell).CombatActionSpell;
                        if (metamagicSpell.ActionCost == 3 || metamagicSpell.ActionCost == -2)
                        {
                            return null;
                        }

                        if (!IncreaseTargetLine(metamagicSpell.Target))
                        {
                            return null;
                        }

                        metamagicSpell.Name = "Reach " + metamagicSpell.Name;
                        CommonSpellEffects.IncreaseActionCostByOne(metamagicSpell);
                        int num3 = metamagicSpell.Target.ToDescription()?.Count((char c) => c == '\n') ?? 0;
                        string[] array2 = metamagicSpell.Description.Split('\n', 4 + num3);
                        if (array2.Length >= 4)
                        {
                            metamagicSpell.Description = array2[0] + "\n" + array2[1] + "\n{Blue}" + metamagicSpell.Target.ToDescription() + "{/Blue}\n" + array2[3 + num3];
                        }

                        return metamagicSpell;
                        bool IncreaseTargetLine(Target? targetLine)
                        {
                            if (targetLine == null)
                            {
                                return false;
                            }

                            if (targetLine is CreatureTarget creatureTarget2)
                            {
                                return IncreaseTarget(creatureTarget2);
                            }

                            if (targetLine is MultipleCreatureTargetsTarget multipleCreatureTargetsTarget)
                            {
                                bool flag = false;
                                CreatureTarget[] targets2 = multipleCreatureTargetsTarget.Targets;
                                foreach (CreatureTarget creatureTarget3 in targets2)
                                {
                                    flag |= IncreaseTarget(creatureTarget3);
                                }

                                return flag;
                            }

                            if (targetLine is BurstAreaTarget burstAreaTarget2)
                            {
                                burstAreaTarget2.Range += 6;
                                return true;
                            }

                            if (targetLine is DependsOnActionsSpentTarget dependsOnActionsSpentTarget)
                            {
                                return IncreaseTargetLine(dependsOnActionsSpentTarget.IfOneAction) | IncreaseTargetLine(dependsOnActionsSpentTarget.IfTwoActions);
                            }

                            if (targetLine is DependsOnSpellVariantTarget dependsOnSpellVariantTarget)
                            {
                                bool flag2 = false;
                                {
                                    foreach (Target target in dependsOnSpellVariantTarget.Targets)
                                    {
                                        if (target is CreatureTarget creatureTarget4)
                                        {
                                            flag2 |= IncreaseTarget(creatureTarget4);
                                        }
                                    }

                                    return flag2;
                                }
                            }

                            return false;
                        }
                    }),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.ReachSpell].Item2)!)
                };
            });

            //Do not use, variant actions don't work
            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.ShardStrike].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect()
                {
                    ProvideMainAction = (QEffect qfSelf) => new ActionPossibility(CreateBasicImpulse(qfSelf.Owner, "Shard Strike", FeatName.ShardStrike, IllustrationName.ShardStrike, [Trait.Metal], 1, "Jagged metal shards form in the air and lash out from you.", $"Choose shards or spines.\n\nShards — Deal {S.HeightenedVariable((qfSelf.Owner.Level + 1) / 2, 1)}d6 slashing damage in a 15-foot cone, and a creature that critically fails takes 1d6 persistent bleed damage.\nSpines — Deal {S.HeightenedVariable((qfSelf.Owner.Level + 1) / 2, 1)}d6 piercing damage in a 30-foot line, and a creature that critically fails is clumsy 1 until the start of your next turn.\n\nEach creature attempts a basic Reflex save against your class DC to mitigate these effects.", Target.Self()).WithSoundEffect(SfxName.ElementalBlastMetal).WithVariants(
                    [
                        new SpellVariant("Shards", "Shards", IllustrationName.VariantCone15).WithNewTarget(Target.FifteenFootCone()),
                        new SpellVariant("Spines", "Spines", IllustrationName.VariantLine30).WithNewTarget(Target.ThirtyFootLine())
                    ])
                    .WithSavingThrow(new(Defense.Reflex, qfSelf.Owner.ClassOrSpellDC()))
                    .WithCreateVariantDescription((int _, SpellVariant? variant) => (variant.Id == "Shards") ? ("Deal " + S.HeightenedVariable((qfSelf.Owner.Level + 1) / 2, 1) + "d6 slashing damage in a 15-foot cone (basic Reflex save mitigates), and a creature that critically fails takes 1d6 persistent bleed damage.") : ("Deal " + S.HeightenedVariable((qfSelf.Owner.Level + 1) / 2, 1) + "d6 piercing damage in a 30-foot line (basic Reflex save mitigates), and a creature that critically fails is clumsy 1 until the start of your next turn."))
                    .WithImpulseHeighteningNumerical(qfSelf.Owner.Level, 1, 2, "The damage increases by 1d6.")
                    .WithEffectOnEachTarget(async delegate (CombatAction spell, Creature caster, Creature target, CheckResult result)
                    {
                        if (spell.ChosenVariant == null)
                        {
                            return;
                        }

                        string diceExpression = caster.MaximumSpellRank + "d6";
                        if (spell.ChosenVariant.Id == "Shards")
                        {
                            await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, diceExpression, DamageKind.Slashing);
                            if (result == CheckResult.CriticalFailure)
                            {
                                target.AddQEffect(QEffect.PersistentDamage("1d6", DamageKind.Bleed));
                            }
                        }
                        else
                        {
                            await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, diceExpression, DamageKind.Piercing);
                            if (result == CheckResult.CriticalFailure)
                            {
                                target.AddQEffect(QEffect.Clumsy(1).WithExpirationAtStartOfSourcesTurn(caster, 1));
                            }
                        }
                    })),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.ShardStrike].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.TimberSentinel].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect()
                {
                    ProvideMainAction = (QEffect qfSelf) => new ActionPossibility(CreateBasicImpulse(qfSelf.Owner, "Timber Sentinel", FeatName.TimberSentinel, IllustrationName.TimberSentinel,
                    [
                        Trait.Wood,
                        Trait.Plant
                    ], 1, "A slim, symmetrical tree grows at your command.", "You create a protector tree in an unoccupied square within 30 feet of you. The tree has AC 10 and " + S.HeightenedVariable(10 * ((qfSelf.Owner.Level + 1) / 2), 10) + " Hit Points. Whenever an ally adjacent to the tree is hit by a Strike, the tree interposes its branches and takes the damage first. Any additional damage beyond what it takes to reduce the tree to 0 Hit Points is dealt to the original target. Even enemies can move through the square without Tumble Through. If you invoke this impulse again, any previous tree dissipates.", Target.Tile(delegate (Creature cr, Tile tl)
                    {
                        if (tl.IsTrulyGenuinelyFreeToEveryCreature)
                        {
                            Tile occupies = cr.Occupies;
                            if (occupies == null)
                            {
                                return false;
                            }

                            return occupies.DistanceTo(tl) <= 6;
                        }

                        return false;
                    }, null)).WithSoundEffect(SfxName.MinorAbjuration).WithImpulseHeighteningNumerical(qfSelf.Owner.Level, 1, 2, "The tree has an additional 10 HP.")
                    .WithEffectOnChosenTargets(async delegate (CombatAction spell, Creature caster, ChosenTargets targets)
                    {
                        Creature caster3 = caster;
                        foreach (Creature item9 in caster3.Battle.AllCreatures.Where((Creature cr) => cr.QEffects.Any((QEffect qf) => qf.Id == QEffectId.ProtectorTree && qf.Source == caster3)))
                        {
                            item9.Die();
                        }

                        DifficultSpells.CreateProtectorTree(caster3, targets.ChosenTile, timberSentinel: true);
                    })),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.TimberSentinel].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.WidenSpell].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect("Widen Spell", "You can expand the area of your spells.")
                {
                    MetamagicProvider = new MetamagicProvider("Widen Spell", delegate (CombatAction spell)
                    {
                        CombatAction combatActionSpell = Spell.DuplicateSpell(spell).CombatActionSpell;
                        if (combatActionSpell.ActionCost == 3 || Constants.IsVariableActionCost(combatActionSpell.ActionCost) || combatActionSpell.HasTrait(Trait.SpellWithDuration))
                        {
                            return null;
                        }

                        if (combatActionSpell.Target is BurstAreaTarget burstAreaTarget)
                        {
                            burstAreaTarget.Radius++;
                        }
                        else if (combatActionSpell.Target is ConeAreaTarget coneAreaTarget)
                        {
                            coneAreaTarget.ConeLength += ((coneAreaTarget.ConeLength <= 3) ? 1 : 2);
                        }
                        else
                        {
                            if (!(combatActionSpell.Target is LineAreaTarget lineAreaTarget))
                            {
                                return null;
                            }

                            lineAreaTarget.LineLength += 2;
                        }

                        combatActionSpell.Name = "Widened " + combatActionSpell.Name;
                        CommonSpellEffects.IncreaseActionCostByOne(combatActionSpell);
                        int num2 = combatActionSpell.Target.ToDescription()?.Count((char c) => c == '\n') ?? 0;
                        string[] array = combatActionSpell.Description.Split('\n', 4 + num2);
                        if (array.Length >= 4 && combatActionSpell.Target is AreaTarget areaTarget)
                        {
                            combatActionSpell.Description = array[0] + "\n" + array[1] + "\n{Blue}" + areaTarget.ToDescription() + "{/Blue}\n" + array[3 + num2];
                        }

                        return combatActionSpell;
                    }),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.WidenSpell].Item2)!)
                };
            });

            LongTermEffects.EasyRegister(ColosseumFeatNames[ColosseumFeat.WintersClutch].Item2, LongTermEffectDuration.Forever, (_, _) =>
            {
                return new QEffect()
                {
                    ProvideMainAction = (QEffect qfSelf) => new ActionPossibility(CreateBasicImpulse(qfSelf.Owner, "Wnter's Clutch", FeatName.WintersClutch, IllustrationName.WintersClutch, new Trait[2]
                    {
                        Trait.Water,
                        Trait.Cold
                    }, 1, "Gleaming flakes of chilling snow fall around you.", "Each creature in a 10-foot burst within 60 feet of you takes " + S.HeightenedVariable((qfSelf.Owner.Level + 3) / 2, 2) + "d4 cold damage with a basic Reflex save against your class DC. The ground in the area is covered in a snow drift, which is difficult terrain. Each square of the drift lasts until the end of the encounter or until fire damage is dealt in that square.", Target.Burst(12, 2)).WithSavingThrow(new(Defense.Reflex, qfSelf.Owner.ClassOrSpellDC())).WithImpulseHeighteningNumerical(qfSelf.Owner.Level, 1, 2, "The damage increases by 1d4.")
                    .WithSoundEffect(SfxName.WintersClutch)
                    .WithEffectOnEachTarget(async delegate (CombatAction spell, Creature caster, Creature target, CheckResult result)
                    {
                        await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, (qfSelf.Owner.Level + 3) / 2 + "d4", DamageKind.Cold);
                    })
                    .WithEffectOnChosenTargets(async delegate (CombatAction spell, Creature caster, ChosenTargets targets)
                    {
                        foreach (Tile item7 in targets.ChosenTiles.Where((Tile tl) => !tl.AlwaysBlocksMovement && !tl.IsFireTerrain))
                        {
                            TileQEffect tileQEffect2 = new TileQEffect(item7)
                            {
                                Illustration = new IllustrationName[4]
                                {
                                    IllustrationName.SnowTile1,
                                    IllustrationName.SnowTile2,
                                    IllustrationName.SnowTile3,
                                    IllustrationName.SnowTile4
                                }.GetRandom(),
                                TransformsTileIntoDifficultTerrain = true
                            };
                            tileQEffect2.AfterDamageIsDealtHere = delegate (DamageKind damageKind)
                            {
                                if (damageKind == DamageKind.Fire)
                                {
                                    tileQEffect2.ExpiresAt = ExpirationCondition.Immediately;
                                }
                            };
                            item7.QEffects.Add(tileQEffect2);
                        }
                    })),
                    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect(ColosseumFeatNames[ColosseumFeat.WintersClutch].Item2)!)
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
            newSource.FocusSpells.Add(AllSpells.CreateModernSpell(spell, creature, creature.MaximumSpellRank, true, new SpellInformation
            {
                ClassOfOrigin = source
            }).CombatActionSpell);
            creature.Spellcasting.Sources.Add(newSource);
        }

        private static CombatAction CreateBasicImpulse(Creature owner, string name, FeatName featName, IllustrationName illustration, Trait[] traits, int baseLevel, string flavorText, string rulesText, Target target)
        {
            var traitList = traits.ToList();
            traitList.AddRange([Trait.Primal, Trait.Basic, Trait.Concentrate]);

            CombatAction combatAction = new CombatAction(owner, illustration, name, traitList.ToArray(),
                "{i}" + flavorText + "{/i}\n\n" + rulesText, target)
            {
                ImpulseInformation = new ImpulseInformation(featName, baseLevel, flavorText, rulesText)
            };
            combatAction.WithProjectileCone(illustration, 15, ProjectileKind.Cone);
            combatAction.WithActionCost(traits.Contains(Trait.Stance) ? 1 : 2);
            return combatAction;
        }

        private static bool IncreaseTarget(CreatureTarget creatureTarget)
        {
            if (creatureTarget.RangeKind == RangeKind.Melee)
            {
                creatureTarget.OwnerAction.Traits.Remove(Trait.Melee);
                creatureTarget.OwnerAction.Traits.Add(Trait.Ranged);
                //P_1.metamagicSpell.Traits = new Traits(P_1.metamagicSpell.Traits.Except(new _003C_003Ez__ReadOnlySingleElementList<Trait>(Trait.Melee)).Concat(new _003C_003Ez__ReadOnlySingleElementList<Trait>(Trait.Ranged)));
                creatureTarget.RangeKind = RangeKind.Ranged;
                creatureTarget.CreatureTargetingRequirements.RemoveAll((CreatureTargetingRequirement ctr) => ctr is AdjacencyCreatureTargetingRequirement || ctr is AdjacentOrSelfTargetingRequirement);
                creatureTarget.CreatureTargetingRequirements.Add(new MaximumRangeCreatureTargetingRequirement(6));
                creatureTarget.CreatureTargetingRequirements.Add(new UnblockedLineOfEffectCreatureTargetingRequirement());
                return true;
            }

            MaximumRangeCreatureTargetingRequirement maximumRangeCreatureTargetingRequirement = creatureTarget.CreatureTargetingRequirements.OfType<MaximumRangeCreatureTargetingRequirement>().FirstOrDefault();
            if (maximumRangeCreatureTargetingRequirement != null)
            {
                maximumRangeCreatureTargetingRequirement.Range += 6;
                return true;
            }

            return false;
        }

        private class FlyingFlameTarget : GeneratorTarget
        {
            public override bool IsAreaTarget => true;

            public override bool RemoveDuplicates => true;

            public override bool TracePathFromOrigin => true;

            public override GeneratedTargetInSequence? GenerateNextTarget()
            {
                List<Tile> chosenTiles = base.OwnerAction.ChosenTargets.ChosenTiles;
                if (chosenTiles.Count == 0)
                {
                    return new GeneratedTargetInSequence(Target.Tile((Creature caster, Tile tile) => !tile.AlwaysBlocksLineOfEffect && caster.Occupies.DistanceTo(tile) == 1, null).WithAlsoSelectCreatures())
                    {
                        AdditionalTargetingText = " (Select first square to move to.)",
                        DisableConfirmNoMoreTargets = true
                    };
                }

                Tile tile2 = base.OwnerAction.Owner.Occupies;
                bool flag = false;
                int num = 0;
                foreach (Tile item in chosenTiles)
                {
                    if (Math.Abs(item.X - tile2.X) == 1 && Math.Abs(item.Y - tile2.Y) == 1)
                    {
                        if (flag)
                        {
                            num++;
                            flag = false;
                        }
                        else
                        {
                            flag = true;
                        }
                    }

                    num++;
                    tile2 = item;
                }

                if (num >= 6)
                {
                    return null;
                }

                Tile from = chosenTiles.Last();
                bool canStillMoveOnDiagonal = !flag || num <= 4;
                return new GeneratedTargetInSequence(Target.Tile((Creature caster, Tile tile) => !tile.AlwaysBlocksLineOfEffect && from.DistanceTo(tile) == 1 && (canStillMoveOnDiagonal || from.X == tile.X || from.Y == tile.Y), null).WithAlsoSelectCreatures())
                {
                    AdditionalTargetingText = $" (current distance {num * 5}/30ft.)",
                    ConfirmationTextToFinish = "Confirm this Flying Flame path"
                };
            }
        }
    }
}
