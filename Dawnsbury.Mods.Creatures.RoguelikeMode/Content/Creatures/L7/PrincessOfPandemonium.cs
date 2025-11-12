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
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Notifications;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using System.Xml.Linq;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class PrincessOfPandemonium {
        public static Creature Create() {
            int radius = 2;
            Item legAtk = new Item(new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.DragonClaws), "stabbing appendage", new Trait[] { Trait.Unarmed, Trait.Finesse, Trait.Agile, Trait.DeadlyD6, Trait.Reach }).WithWeaponProperties(new WeaponProperties("2d6", DamageKind.Piercing));

            Creature monster = new Creature(new SpiderIllustration(Illustrations.AbyssalHandmaiden, Illustrations.Bear3), "Princess of Pandemonium",
                [Trait.Chaotic, Trait.Evil, Trait.Demon, Trait.Fiend, ModTraits.Spider, Trait.NonSummonable],
                7, 6, 5, new Defenses(24, 12, 18, 15), 145,
            new Abilities(5, 5, 4, 3, 3, 4), new Skills(acrobatics: 16, athletics: 16, intimidation: 15, religion: 12, arcana: 12))
            .WithCreatureId(CreatureIds.PrincessOfPandemonium)
            .WithProficiency(Trait.Melee, Proficiency.Master)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(legAtk)
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Good, 5))
            .AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 5))
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            .AddQEffect(QEffect.WebSense())
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    int map = self.Owner.Actions.AttackedThisManyTimesThisTurn;
                    return (ActionPossibility)new CombatAction(self.Owner, new SideBySideIllustration(new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.DragonClaws), new SideBySideIllustration(new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.DragonClaws), new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.DragonClaws))),
                        "Flurry of Limbs", Array.Empty<Trait>(),
                        "The Abyssal Handmaiden makes up to three stabbing appendage Strikes against different targets. These attacks count toward the Abyssal Handmaiden's multiple attack penalty, but the penalty doesn't increase until after all the attacks have been made.",
                        // Target.MultipleCreatureTargets(Target.AdjacentCreature(), Target.AdjacentCreature(), Target.AdjacentCreature())
                        Target.MultipleCreatureTargets(Target.Reach(legAtk), Target.Reach(legAtk), Target.Reach(legAtk))
                        .WithMustBeDistinct().WithMinimumTargets(1)
                        .WithOverriddenOverallGoodness((t, hm) => {
                            switch (hm.Battle.AllCreatures.Where(cr => cr.Alive && cr.EnemyOf(hm) && hm.HasLineOfEffectTo(cr.Occupies) <= CoverKind.Greater && hm.DistanceTo(cr) <= radius).Count()) {
                                case 0:
                                    return int.MinValue;
                                case 1:
                                    return int.MinValue;
                                case 2:
                                    return 20f;
                                case 3:
                                    return 30f;
                                default:
                                    return 30f;
                            }
                        })
                    )
                    .WithActionCost(2)
                    .WithGoodnessAgainstEnemy((t, a, d) => 12)
                    .WithEffectOnEachTarget(async (action, attacker, defender, result) => {
                        Item weapon = attacker.UnarmedStrike;
                        if (weapon == null)
                            return;
                        int num4 = (int)await attacker.MakeStrike(defender, weapon, map);
                    });
                },
            })
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    Delegates.EffectOnEachTarget effect = async (action, attacker, defender, result) => {
                        if (result < CheckResult.Success) {
                            return;
                        }
                        await Possibilities.Grapple(attacker, defender, result);
                    };
                    CombatAction bite = self.Owner.CreateStrike(CommonItems.CreateNaturalWeapon(IllustrationName.GluttonsJaw, "bite", "2d10", DamageKind.Piercing, Trait.Unarmed, Trait.Finesse, Trait.Restraining));
                    bite.Description = "The Princess of Pandemonium attempts to grab and sink their teeth into an isolated opponent, afflicting them with her terrible, wasting poison.";
                    bite.ShortDescription += " and the target becomes grabbed and exposed to abyssal rot.";
                    bite.ActionCost = 2;
                    bite.WithGoodnessAgainstEnemy((targeting, a, d) => d.FindQEffect(QEffectIds.AbyssalRot) != null ? 14 : 18f);
                    bite.EffectOnOneTarget = (Delegates.EffectOnEachTarget)Delegate.Combine(bite.EffectOnOneTarget, effect);
                    return (ActionPossibility)bite;
                },
            })
            .AddQEffect(CommonQEffects.AbyssalRotAttack(24, "1d12", "bite"))
            .AddQEffect(new QEffect() {
                StartOfCombat = async self => {
                    var caster = self.Owner;

                    var telegraphCandidates = caster.Battle.AllCreatures.Where(cr => cr.Alive && cr.OwningFaction.EnemyFactionOf(caster.OwningFaction) && cr.PersistentCharacterSheet != null).ToList();
                    if (telegraphCandidates.Count > 0) {
                        var target = UtilityFunctions.ChooseAtRandom(telegraphCandidates.ToArray());
                        target!.AddQEffect(new QEffect("Impending Pandemonium", $"You will become confused for 1 round next time {caster.Name} uses Invoke Pandemonium.", ExpirationCondition.ExpiresAtEndOfSourcesTurn, caster, new FunctionLibs.DualIllustration(IllustrationName.Confusion, caster.Illustration)) {
                            Id = QEffectIds.ImpendingPandemonium,
                        });
                        target.Overhead("*impending pandemonium*", Color.PaleVioletRed, target.Name + $" starts to feel maddness taking hold of their thoughts. They will become confused the next time {caster.Name} uses {{b}}Invoke Pandemomium{{/b}}.");
                    }
                }
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Demon, Ability.Charisma, Trait.Divine).WithSpells (
                level2: [SpellId.MirrorImage],
                level3: [SpellId.AcidArrow, SpellId.AcidArrow, SpellId.AcidArrow]).Done()
            .Builder
            .AddMainAction(you => {
                return new CombatAction(you, IllustrationName.Confusion, "Invoke Pandemonium", [Trait.Enchantment, Trait.Flourish], "The creature marked for {b}Impending Pandemonium{/b} becomes confused for 1 round, and another random enemy creature gains {b}Impending Pandemonium{/b} in their place. " +
                    "In addition, all enemy creatures within 10 feet of the marked creature take 6d6 mental damage (Basic will save mitigates), as their minds are assaulted by the chaos of the demon world.", Target.Self((user, ai) => AIConstants.ALWAYS))
                .WithActionCost(0)
                .WithSoundEffect(SfxName.Necromancy)
                .WithEffectOnSelf(async (action, caster) => {
                    var telegraph = caster.Battle.AllCreatures.FirstOrDefault(cr => cr.FindQEffect(QEffectIds.ImpendingPandemonium)?.Source == caster);
                    var telegraphCandidates = caster.Battle.AllCreatures.Where(cr => cr.Alive && cr.OwningFaction.EnemyFactionOf(caster.OwningFaction) && cr.PersistentCharacterSheet != null && cr != telegraph).ToList();
                    if (telegraphCandidates.Count > 0) {
                        var target = UtilityFunctions.ChooseAtRandom(telegraphCandidates.ToArray());
                        target!.AddQEffect(new QEffect("Impending Pandemonium", $"You will become confused for 1 round next time {caster.Name} uses Invoke Pandemonium.", ExpirationCondition.Never, caster, new FunctionLibs.DualIllustration(IllustrationName.Confusion, caster.Illustration)) {
                            Id = QEffectIds.ImpendingPandemonium,
                        }.WithExpirationAtEndOfSourcesNextTurn(caster, false));
                        target.Overhead("*impending pandemonium*", Color.PaleVioletRed, target.Name + $" starts to feel maddness taking hold of their thoughts. They will become confused the next time {caster.Name} uses {{b}}Invoke Pandemomium{{/b}}, and all allied creature within 10 feet of them will take 6d6 mental damage (Basic will save mitigates).");
                    }
                    if (telegraph != null) {
                        telegraph.RemoveAllQEffects(qf => qf.Id == QEffectIds.ImpendingPandemonium && qf.Source == caster);
                        telegraph.AddQEffect(QEffect.Confused(false, action).WithExpirationAtStartOfSourcesTurn(caster, 1));
                        telegraph.Overhead("*confused*", Color.PaleVioletRed, telegraph.Name + $" becomes confused.");

                        await CommonAnimations.CreateConeAnimation(telegraph.Battle, telegraph.Occupies.ToCenterVector(), telegraph.Battle.Map.AllTiles.Where(t => t.DistanceTo(telegraph.Occupies) <= 2).ToList(), 15, ProjectileKind.Cone, IllustrationName.Confusion);
                        foreach (Creature creature in telegraph.Battle.AllCreatures.Where(cr => cr.DistanceTo(telegraph) <= 2 && cr.EnemyOf(caster) && cr != telegraph && !cr.HasEffect(QEffectId.OutOfCombat))) {
                            var result = CommonSpellEffects.RollSavingThrow(creature, action, Defense.Will, caster.Spellcasting?.PrimarySpellcastingSource?.GetSpellSaveDC() ?? 0);
                            await CommonSpellEffects.DealBasicDamage(action, caster, creature, result, DiceFormula.FromText("6d6", "Invoke Pandemonium"), DamageKind.Mental);
                        }
                    }
                })
                ;
            })
            .Done();
            return monster;
        }
    }
}
