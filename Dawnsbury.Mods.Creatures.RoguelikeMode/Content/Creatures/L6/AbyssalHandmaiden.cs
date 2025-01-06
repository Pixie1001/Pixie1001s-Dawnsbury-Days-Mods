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
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Notifications;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using Microsoft.Xna.Framework;
using System.Xml.Linq;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L6 {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class AbyssalHandmaiden {
        public static Creature Create() {
            // TODO: Aura of madness not working
            // TODO: Not using bite on single targets
            // TODO: Bite short desc needs an update to explain grab and poison
            // CREATURE - Abyssal Handmaiden
            int radius = 2;
            Item legAtk = new Item(Illustrations.StabbingAppendage, "stabbing appendage", new Trait[] { Trait.Unarmed, Trait.Finesse, Trait.DeadlyD6, Trait.Reach }).WithWeaponProperties(new WeaponProperties("1d8", DamageKind.Piercing));

            Creature monster = new Creature(Illustrations.AbyssalHandmaiden, "Abyssal Handmaiden", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Demon, Trait.Fiend, ModTraits.Spider }, 6, 6, 5, new Defenses(23, 11, 17, 14), 90,
            new Abilities(5, 5, 4, 2, 2, 4), new Skills(acrobatics: 15, athletics: 14, intimidation: 14, religion: 10, arcana: 10))
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(legAtk)
            .AddQEffect(CommonQEffects.MiniBoss())
            .AddQEffect(new QEffect() {
                StateCheck = self => self.Owner.WeaknessAndResistance.AddWeakness(DamageKind.Good, 5)
            })
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            .AddQEffect(new QEffect() {
                ProvideMainAction = self => {
                    int map = self.Owner.Actions.AttackedThisManyTimesThisTurn;
                    return (ActionPossibility)new CombatAction(self.Owner, new SideBySideIllustration(Illustrations.StabbingAppendage, new SideBySideIllustration(Illustrations.StabbingAppendage, Illustrations.StabbingAppendage)),
                        "Flurry of Limbs", Array.Empty<Trait>(),
                        "The Abyssal Handmaiden makes up to three stabbing appendage Strikes against different targets. These attacks count toward the Abyssal Handmaiden's multiple attack penalty, but the penalty doesn't increase until after all the attacks have been made.",
                        // Target.MultipleCreatureTargets(Target.AdjacentCreature(), Target.AdjacentCreature(), Target.AdjacentCreature())
                        Target.MultipleCreatureTargets(Target.Reach(legAtk), Target.Reach(legAtk), Target.Reach(legAtk))
                        .WithMustBeDistinct().WithMinimumTargets(1)
                        .WithOverriddenOverallGoodness((t, hm) => {
                            //switch (hm.Occupies.DistanceTo.Creatures.Count(cr => cr.EnemyOf(hm) && cr.Alive)) {
                            switch (hm.Battle.AllCreatures.Where(cr => cr.Alive && cr.EnemyOf(hm) && hm.HasLineOfEffectTo(cr.Occupies) <= CoverKind.Greater && hm.DistanceTo(cr) <= 2).Count()) {
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
                    CombatAction bite = self.Owner.CreateStrike(CommonItems.CreateNaturalWeapon(IllustrationName.GluttonsJaw, "bite", "2d8", DamageKind.Piercing, Trait.Unarmed, Trait.Finesse));
                    bite.Description = "The Abyssal Handmaiden attemps to grab and sink their teeth into an isolated opponent, afflicting them with her terrible, wasting poison.";
                    bite.ShortDescription += " and the target becomes grabbed and exposed to abyssal rot.";
                    bite.ActionCost = 2;
                    bite.WithGoodnessAgainstEnemy((targeting, a, d) => 18f);
                    bite.EffectOnOneTarget = (Delegates.EffectOnEachTarget)Delegate.Combine(bite.EffectOnOneTarget, effect);
                    return (ActionPossibility)bite;
                },
            })
            .AddQEffect(CommonQEffects.AbyssalRotAttack(18, "1d10", "bite"));
            //.AddSpellcastingSource(SpellcastingKind.Innate, Trait.Demon, Ability.Charisma, Trait.Divine).WithSpells(
            //    level2: new SpellId[] { SpellId.Harm }).Done()
            var animation = monster.AnimationData.AddAuraAnimation(IllustrationName.AngelicHaloCircle, radius);
            animation.Color = Color.DarkRed;
            QEffect aura = new QEffect("Aura of Madness", $"Creatures that begin their turn within your aura for 2 turns in a row must pass a DC {SkillChallengeTables.GetDCByLevel(monster.Level) - 2} Will save or become confused until the start of their next turn.") {

            };
            aura.AddGrantingOfTechnical(cr => cr.EnemyOf(monster), qfAura => {
                qfAura.StartOfYourPrimaryTurn = async (self, you) => {
                    if (you.DistanceTo(monster) <= radius) {
                        QEffect? auraDebuff = you.QEffects.FirstOrDefault(qf => qf.Key == "Aura of Madness Debuff");
                        if (auraDebuff != null) {
                            auraDebuff.Value += 1;
                            CombatAction action = CombatAction.CreateSimple(monster, "Aura of Madness", Trait.Demon, Trait.Divine, Trait.Mental, Trait.Emotion);
                            CheckResult result = CommonSpellEffects.RollSavingThrow(you, action, Defense.Will, SkillChallengeTables.GetDCByLevel(monster.Level) - 2);
                            if (result < CheckResult.Success) {
                                you.AddQEffect(QEffect.Confused(false, action).WithExpirationAtStartOfOwnerTurn());
                                you.Occupies.Overhead("*madness*", Color.Crimson, $"{you.Name} succumbs to madness!");
                            } else {
                                you.Occupies.Overhead("*resisted*", Color.Crimson, $"{you.Name} resisted the {monster.Name}'s madness aura!");
                            }
                        } else {
                            you.AddQEffect(new QEffect("Demonic Exposure", $"Increased each time you end your turn within the Abyssal Handmaiden's aura of madness, and removed when you start your turn outside her aura. Once this reaches 2 or higher, you must pass a DC {SkillChallengeTables.GetDCByLevel(monster.Level) - 2} Will save or become confused until your next turn.", ExpirationCondition.Never, monster, IllustrationName.Chaos) {
                                Value = 1,
                                Key = "Aura of Madness Debuff"
                            });
                        }
                    } else {
                        you.RemoveAllQEffects(qf => qf.Key == "Aura of Madness Debuff");
                    }
                };
            });
            monster.AddQEffect(aura);
            return monster;
        }
    }
}
