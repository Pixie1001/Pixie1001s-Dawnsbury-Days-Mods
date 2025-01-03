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
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L6
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class AbyssalHandmaiden
    {
        public static Creature Create()
        {
            // TODO: Aura of madness not working
            // TODO: Not using bite on single targets
            // TODO: Bite short desc needs an update to explain grab and poison
            // CREATURE - Abyssal Handmaiden

            Item legAtk = CommonItems.CreateNaturalWeapon(IllustrationName.Spear, "stabbing appendage", "2d6", DamageKind.Piercing, Trait.Unarmed, Trait.Finesse, Trait.DeadlyD6, Trait.Reach);
            Creature monster = new Creature(Illustrations.AbyssalHandmaiden, "Abyssal Handmaiden", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Demon, Trait.Fiend, ModTraits.Spider }, 6, 6, 5, new Defenses(23, 11, 17, 14), 90,
            new Abilities(5, 5, 4, 2, 2, 4), new Skills(acrobatics: 15, athletics: 14, intimidation: 14, religion: 10, arcana: 10))
            .WithAIModification(ai =>
            {
                ai.OverrideDecision = (self, options) =>
                {
                    Creature creature = self.Self;

                    return null;
                };
            })
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(legAtk)
            .AddQEffect(CommonQEffects.MiniBoss())
            .AddQEffect(new QEffect()
            {
                ProvideMainAction = self =>
                {
                    int map = self.Owner.Actions.AttackedThisManyTimesThisTurn;
                    return (ActionPossibility)new CombatAction(self.Owner, new SideBySideIllustration(IllustrationName.Spear, new SideBySideIllustration(IllustrationName.Spear, IllustrationName.Spear)),
                        "Flurry of Limbs", Array.Empty<Trait>(),
                        "The Abyssal Handmaiden makes up to three stabbing appendage Strikes against different targets. These attacks count toward the Abyssal Handmaiden's multiple attack penalty, but the penalty doesn't increase until after all the attacks have been made.",
                        // Target.MultipleCreatureTargets(Target.AdjacentCreature(), Target.AdjacentCreature(), Target.AdjacentCreature())
                        Target.MultipleCreatureTargets(Target.Reach(legAtk), Target.Reach(legAtk), Target.Reach(legAtk))
                        .WithMustBeDistinct().WithMinimumTargets(1).WithSimultaneousAnimation()
                        .WithOverriddenOverallGoodness((t, hm) =>
                        {
                            //switch (hm.Occupies.DistanceTo.Creatures.Count(cr => cr.EnemyOf(hm) && cr.Alive)) {
                            switch (hm.Battle.AllCreatures.Where(cr => cr.EnemyOf(hm) && hm.HasLineOfEffectTo(cr.Occupies) <= CoverKind.Greater && hm.DistanceTo(cr) <= 2).Count())
                            {
                                case 0:
                                    return int.MinValue;
                                case 1:
                                    return 12f;
                                case 2:
                                    return 24f;
                                case 3:
                                    return 36f;
                                default:
                                    return 36f;
                            }
                        })
                    )
                    .WithActionCost(2)
                    //.WithGoodnessAgainstEnemy((t, a, d) => 12)
                    .WithEffectOnEachTarget(async (action, attacker, defender, result) =>
                    {
                        Item weapon = attacker.UnarmedStrike;
                        if (weapon == null)
                            return;
                        int num4 = (int)await attacker.MakeStrike(defender, weapon, map);
                    });
                },
            })
            .AddQEffect(new QEffect()
            {
                ProvideMainAction = self =>
                {
                    //Func<CombatAction, Creature, Creature, CheckResult, Task?> effect = async (action, attacker, defender, result) => {
                    //};
                    Delegates.EffectOnEachTarget effect = async (action, attacker, defender, result) =>
                    {
                        if (result < CheckResult.Success)
                        {
                            return;
                        }
                        await Possibilities.Grapple(attacker, defender, result);
                        Affliction poison = Affliction.CreateDemonWebspinnerSpiderVenom();
                        poison.DC = 24;
                        await Affliction.ExposeToInjury(poison, attacker, defender);
                    };
                    CombatAction bite = self.Owner.CreateStrike(CommonItems.CreateNaturalWeapon(IllustrationName.GluttonsJaw, "bite", "2d8", DamageKind.Piercing, Trait.Unarmed, Trait.Finesse));
                    bite.Description = "The Abyssal Handmaiden attemps to grab and sink their teeth into an isolated opponent, afflicting them with her terrible, wasting poison.";
                    bite.ActionCost = 2;
                    bite.WithGoodnessAgainstEnemy((targeting, a, d) => 18f);
                    //bite.WithEffectOnEachTarget + effect;
                    bite.EffectOnOneTarget = (Delegates.EffectOnEachTarget)Delegate.Combine(bite.EffectOnOneTarget, effect);
                    return (ActionPossibility)bite;
                },
            });
            //.AddSpellcastingSource(SpellcastingKind.Innate, Trait.Demon, Ability.Charisma, Trait.Divine).WithSpells(
            //    level2: new SpellId[] { SpellId.Harm }).Done()
            var animation = monster.AnimationData.AddAuraAnimation(IllustrationName.AngelicHaloCircle, 2);
            animation.Color = Color.DarkRed;
            QEffect aura = new QEffect("Aura of Madness", "...")
            {

            };
            aura.AddGrantingOfTechnical(cr => monster.EnemyOf(cr), qfAura =>
            {
                qfAura.StartOfYourPrimaryTurn = async (self, you) =>
                {
                    if (you.DistanceTo(monster) <= 1)
                    {
                        QEffect? auraDebuff = you.QEffects.FirstOrDefault(qf => qf.Key == "Aura of Madness Debuff");
                        if (auraDebuff != null)
                        {
                            auraDebuff.Value += 1;
                        }
                        else
                        {
                            you.AddQEffect(new QEffect("Demonic Exposure", "Increased each time you end your turn within the Abyssal Handmaiden's aura of madness, and removed when you start your turn outside her aura. Once this reaches 2 or higher, you are confused until you leave the aura.", ExpirationCondition.Never, monster, IllustrationName.Chaos)
                            {
                                Value = 1,
                                Key = "Aura of Madness Debuff",
                                StartOfYourPrimaryTurn = async (self, you) =>
                                {
                                    if (self.Value >= 2)
                                    {
                                        you.AddQEffect(QEffect.Confused(false, null).WithExpirationAtStartOfOwnerTurn());
                                    }
                                }
                            });
                        }
                    }
                    else
                    {
                        you.RemoveAllQEffects(qf => qf.Key == "Aura of Madness Debuff");
                    }
                };
            });
            return monster;
        }
    }
}
