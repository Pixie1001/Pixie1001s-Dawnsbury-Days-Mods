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
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Notifications;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using Dawnsbury.ThirdParty.SteamApi;
using Microsoft.Xna.Framework;
using System;
using System.Xml.Linq;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class ShadowWebStalker {
        public static Creature Create() {
            Item legAtk = new Item(new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.DragonClaws), "stabbing appendage", new Trait[] { Trait.Unarmed, Trait.Finesse, Trait.Agile, Trait.DeadlyD6 }).WithWeaponProperties(new WeaponProperties("2d6", DamageKind.Piercing));

            Creature monster = new Creature(new SpiderIllustration(Illustrations.ShadowWebStalker, Illustrations.ShadowBear2), "Shadow Web Stalker",
                [Trait.Chaotic, Trait.Evil, Trait.Demon, Trait.Fiend, ModTraits.Spider, ModTraits.MeleeMutator],
                5, 12, 8, new Defenses(21, 9, 15, 14), 75,
            new Abilities(3, 6, 4, -2, 4, 0),
            new Skills(acrobatics: 13, athletics: 11, stealth: 15))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    if (monster.HasEffect(QEffectIds.ShadowSlip)) {
                        monster.RemoveAllQEffects(qf => qf.Id == QEffectIds.ShadowSlip);
                        return options.Where(opt => opt.Text == "Sneak (Shadow Slip)").MaxBy(opt => opt.AiUsefulness);
                    }

                    if (monster.Actions.ActionsLeft == 1) {
                        return options.Where(opt => opt.Text == "Shadow Slip").MaxBy(opt => opt.AiUsefulness);
                    }

                    return null;
                };
            })
            .WithCreatureId(CreatureIds.ShadowWebStalker)
            .WithProficiency(Trait.Melee, Proficiency.Master)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(legAtk)
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Good, 5))
            .AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 5))
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            .AddQEffect(QEffect.WebSense())
            .AddQEffect(new QEffect("Attention Vulnerability", "When the shadow web stalker is spotted by the seek action or hit by an ability that has the Light trait or that counters invisibility, it suffers 2d6 mental damage.") {
                AfterYouAreTargeted = async (self, action) => {
                    if (action?.Owner?.Occupies != null && ((action.ActionId == ActionId.Seek && action.CheckResult >= CheckResult.Success) || action.HasTrait(Trait.Light) || action.SpellId == SpellId.Glitterdust)) {
                        Sfxs.Play(SoundEffects.BebilithHiss);
                        await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("2d6", "Attention vulnerability"), self.Owner, CheckResult.Failure, DamageKind.Mental);
                    }
                }
            })
            .AddQEffect(CommonQEffects.ShadowWebSicknessAttack(20, "fang"))
            .Builder
            .AddNaturalWeapon(NaturalWeaponKind.Fang, 17, [Trait.Finesse, Trait.AddsInjuryPoison], "2d6+3", DamageKind.Piercing)
            .AddMainAction(you => {
                return new CombatAction(you, IllustrationName.Invisibility, "Shadow Slip", [Trait.Illusion], "The shadow web stalker becomes invisible, hides and then sneaks.",
                    Target.Self((caster, ai) => 0))
                .WithActionCost(1)
                .WithSoundEffect(SfxName.PhaseBolt)
                .WithEffectOnSelf(async caster => {
                    caster.AddQEffect(QEffect.Invisibility().WithExpirationAtEndOfSourcesNextTurn(caster, true));
                    var hide = CommonStealthActions.CreateHide(caster);
                    hide.ChosenTargets.ChosenCreature = caster;
                    hide.ActionCost = 0;
                    await hide.AllExecute();
                    caster.AddQEffect(new QEffect() {
                        Id = QEffectIds.ShadowSlip,
                        ExpiresAt = ExpirationCondition.ExpiresAtEndOfYourTurn,
                        ProvideMainAction = self => {
                            var sneak = CommonStealthActions.CreateSneak(caster);
                            sneak.ActionCost = 0;
                            sneak.Name = "Sneak (Shadow Slip)";
                            // sneak.WithEffectOnSelf(_ => self.ExpiresAt = ExpirationCondition.Immediately);
                            return (ActionPossibility)sneak;
                        }
                    });
                });
            })
            .Done();
            return monster;
        }
    }
}
