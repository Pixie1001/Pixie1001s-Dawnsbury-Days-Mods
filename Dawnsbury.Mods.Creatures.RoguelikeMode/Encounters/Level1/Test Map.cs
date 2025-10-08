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
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Core.Animations.Movement;
using static Dawnsbury.Mods.Creatures.RoguelikeMode.Ids.ModEnums;
using Dawnsbury.Campaign.Encounters.Quest_for_the_Golden_Candelabra;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Campaign.Encounters.Evil_from_the_Stars;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.IO;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Display.Text;
using Microsoft.Xna.Framework;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using System.Security.Permissions;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1 {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class TestMap : Encounter {

        public TestMap(string filename) : base("Test Map", filename, null, 0) {
            // Run setup
            this.ReplaceTriggerWithCinematic(TriggerName.StartOfEncounter, async battle => {
                // CommonEncounterFuncs.ApplyEliteAdjustments(battle);

                Creature td = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy);
                Creature pm = battle.AllCreatures.FirstOrDefault(cr => cr.PersistentCharacterSheet != null);

                //var boon1 = new QEffect("Lyra's Boon", $"You can use the Baraquielle's Boon once.", ExpirationCondition.Never, null, Illustrations.Baraquielle) {
                //    HideFromPortrait = true,
                //    LongTermEffectDuration = LongTermEffectDuration.Forever,
                //    ProvideActionIntoPossibilitySection = (effect, section) => {
                //        if (section.PossibilitySectionId != PossibilitySectionId.OtherManeuvers) return null;
                //        return new ActionPossibility(new CombatAction(effect.Owner, effect.Illustration!, $"Use Baraquielle's Boon",
                //                [Trait.Basic, Trait.Divine, Trait.Polymorph, Trait.Transmutation], $"{{i}}You reach within yourself and use the boon you received from Baraquielle, to assume fearsome angelic avatar form.{{/i}}\n\n" +
                //                $"Whilest in Angel form, you gain a {SkillChallengeTables.GetDCByLevel(effect.Owner.Level) - 7} bonus to attack and athletics, a 2d8+{effect.Owner.Level} damage reach lance attack that deals an additional 1d4 fire and good damage, and a 40 foot fly speed and retributive strike to allow you to protect your allies and punish their attackers." +
                //                $"\n\nAfter you use this boon once, it's gone forever.", Target.Self())
                //            .WithActionCost(0)
                //            .WithSoundEffect(SfxName.Angelic)
                //            .WithEffectOnSelf(caster => {
                //                effect.ExpiresAt = ExpirationCondition.Immediately;

                //                int ac = SkillChallengeTables.GetDCByLevel(caster.Level) + 2;
                //                QEffect form = CommonSpellEffects.EnterBattleform(caster, Illustrations.AngelForm, ac, 8, false);
                //                form.Name = "Angel Form";
                //                form.StateCheck = (Action<QEffect>)Delegate.Combine(form.StateCheck, delegate (QEffect qfForm) {
                //                    qfForm.Owner.ReplacementUnarmedStrike = new Item(IllustrationName.Halberd, "Lance of Retribution", [Trait.Good, Trait.Fire, Trait.Reach, Trait.VersatileS, Trait.Polearm, Trait.BattleformAttack])
                //                    .WithWeaponProperties(new WeaponProperties("2d8", DamageKind.Piercing))
                //                    .WithMonsterWeaponSpecialization(caster.Level)
                //                    .WithAdditionalWeaponProperties(wp => {
                //                        wp.WithAdditionalDamage("1d4", DamageKind.Good);
                //                        wp.WithAdditionalDamage("1d4", DamageKind.Fire);
                //                    });

                //                    qfForm.Owner.AddQEffect(QEffect.Flying().WithExpirationEphemeral());
                //                    if (!caster.HasFeat(FeatName.Paladin))
                //                        qfForm.Owner.AddQEffect(CommonQEffects.RetributiveStrike(2, cr => true, "an ally", true).WithExpirationEphemeral());

                //                    form.BattleformMinimumStrikeModifier = SkillChallengeTables.GetDCByLevel(caster.Level) - 7;
                //                    form.BattleformMinimumAthleticsModifier = SkillChallengeTables.GetDCByLevel(caster.Level) - 7;
                //                });
                //            })
                //        );
                //    }
                //};

                //var boon2 = new QEffect("Azata Companion", "You've acquired the aid of the Azata Lyra. They will fight besides you until dying or the party returns to town.") {
                //    HideFromPortrait = true,
                //    Illustration = Illustrations.Lyra,
                //    ExpiresAt = ExpirationCondition.Never,
                //    EndOfCombat = async (effect, b) => effect.Owner.LongTermEffects?.Add(WellKnownLongTermEffects.CreateLongTermEffect("Azata Companion")!),
                //    StartOfCombat = async self => {
                //        Creature companion = Lyra.Create(self.Owner.Battle.Encounter);
                //        self.Owner.Battle.SpawnCreature(companion, self.Owner.Battle.GaiaFriends, self.Owner.Occupies);
                //        companion.AddQEffect(CommonQEffects.CantOpenDoors());
                //        companion.AddQEffect(new QEffect() {
                //            HideFromPortrait = true,
                //            Source = self.Owner,
                //            WhenMonsterDies = qfDeathCheck => {
                //                self.ExpiresAt = ExpirationCondition.Immediately;
                //            }
                //        });
                //    },
                //};

                //pm.AddQEffect(boon1);
                //pm.AddQEffect(boon2);



                // pm.AddQEffect(lteEffect);

                //battle.AllCreatures.FirstOrDefault(cr => cr.BaseName == "test summoner").AddQEffect(new QEffect() { BonusToDefenses = (self, action, defence) => defence == Defense.Fortitude ? new Bonus(-5, BonusType.Untyped, "Testing") : null });
                // Debug
                //await Affliction.ExposeToInjury(Affliction.CreateSpiderVenom(), td, battle.AllCreatures.FirstOrDefault(cr => cr.BaseName == "test summoner"));

                //CommonEncounterFuncs.ApplyEliteAdjustments(battle);

                //td.AddQEffect(new QEffect() {
                //    AdditionalGoodness = (self, action, target) => {
                //        if (action.HasTrait(Trait.Strike)) {
                //            return 7;
                //        }
                //        return 0;
                //    }
                //});
                //td.Traits.Add(Trait.Undead);
                //td.Level = 9;
            });

            //this.ReplaceTriggerWithCinematic(TriggerName.InitiativeCountZero, async battle => {
            //    //var eidolon = battle.AllCreatures.FirstOrDefault(cr => cr.Name == "test summoner's Eidolon");
            //    //eidolon.AddQEffect(QEffect.Quickened(a => true));

            //    // await RecoverAllFriends(battle);
            //    List<Creature> party = battle.AllCreatures.Where(cr => cr.PersistentCharacterSheet != null).ToList();
            //    party[0].AddQEffect(new QEffect("Lyra's Boon", $"You can use the Lyra's Boon once.", ExpirationCondition.Never, null, IllustrationName.AngelCouncillor) {
            //        HideFromPortrait = true,
            //        LongTermEffectDuration = LongTermEffectDuration.Forever,
            //        ProvideActionIntoPossibilitySection = (effect, section) => {
            //            if (section.PossibilitySectionId != PossibilitySectionId.OtherManeuvers) return null;
            //            return new ActionPossibility(new CombatAction(effect.Owner, effect.Illustration!, $"Use Lyra's Boon",
            //                    [Trait.Basic, Trait.Divine, Trait.Evocation], $"{{i}}You reach within yourself and use the boon you received from Lyra, to smite your foes with a beam of rainbows and friendship.{{/i}}\n\n" +
            //                    $"Each enemy in a 60 foot lines takes {Math.Max(effect.Owner.Level / 2, 1)}d10 good damage with a Reflex saving throw." + S.FourDegreesOfSuccess(null, "The target takes half damage", "The target takes full damage and is dazzled for 1 round.", "The target takes double damage and is blinded for 1 round.") +
            //                    $"\n\nAfter you use this boon once, it's gone forever.", Target.Line(12).WithIncludeOnlyIf((area, target) => area.OwnerAction.Owner.EnemyOf(target)))
            //                .WithActionCost(1)
            //                .WithSoundEffect(SfxName.MagicMissile)
            //                .WithProjectileCone(IllustrationName.ColorSpray, 25, ProjectileKind.Ray)
            //                .WithSavingThrow(new SavingThrow(Defense.Reflex, effect.Owner.ClassOrSpellDC()))
            //                .WithEffectOnSelf(caster => {
            //                    effect.ExpiresAt = ExpirationCondition.Immediately;
            //                })
            //                .WithEffectOnEachTarget(async (spell, caster, target, result) => {
            //                    target.AddQEffect(new QEffect() {
            //                        WhenMonsterDies = self => {
            //                            Sfxs.Play(SfxName.Angelic, 0.75f);
            //                            target.Traits.Add(Trait.NoDeathOverhead);
            //                            target.Traits.Add(Trait.NoDeathScream);
            //                            self.Owner.Overhead("*sees the error of their ways*", Color.Lavender, self.Owner.Name + " is redeemed of their evil ways, and peacefully leaves the field of battle.");
            //                        },
            //                        ExpiresAt = ExpirationCondition.EphemeralAtEndOfImmediateAction
            //                    });

            //                    await CommonSpellEffects.DealBasicDamage(spell, caster, target, result, Math.Max(effect.Owner.Level / 2, 1) + "d10", DamageKind.Good);
            //                    if (result == CheckResult.Failure) {
            //                        target.AddQEffect(QEffect.Dazzled().WithExpirationOneRoundOrRestOfTheEncounter(caster, false));
            //                    } else if (result == CheckResult.CriticalFailure) {
            //                        target.AddQEffect(QEffect.Blinded().WithExpirationOneRoundOrRestOfTheEncounter(caster, false));
            //                    }
            //                })
            //            );
            //        }
            //    });



            //});

            //this.ReplaceTriggerWithCinematic(TriggerName.InitiativeCountZero, async battle => {
            //    Creature td = battle.AllCreatures.FirstOrDefault(cr => cr.OwningFaction.IsEnemy);


            //    // Debug
            //    battle.AllCreatures.FirstOrDefault(cr => cr.BaseName.ToLower() == "test summoner's eidolon").AddQEffect(new QEffect() { BonusToDefenses = (self, action, defence) => defence == Defense.Fortitude ? new Bonus(-5, BonusType.Status, "Testing") : null });
            //    //await Affliction.ExposeToInjury(Affliction.CreateSpiderVenom(), td, battle.AllCreatures.FirstOrDefault(cr => cr.BaseName.ToLower() == "test summoner's eidolon"));
            //});
        }

        //public override void ModifyCreatureSpawningIntoTheEncounter(Creature creature) {
        //    S4E2OnTheSeabed.AquaticCombatModify(creature);
        //}

    }
}
