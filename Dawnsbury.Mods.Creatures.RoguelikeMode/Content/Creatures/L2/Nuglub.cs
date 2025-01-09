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
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class Nuglub
    {
        public static Creature Create()
        {
            return new Creature(Illustrations.Nuglub, "Nuglub", [Trait.Chaotic, Trait.Evil, Trait.Fey, Trait.Gremlin], 2, 5, 6, new Defenses(18, 9, 10, 5), 34, new Abilities(1, 4, 3, -1, -1, 1), new Skills(acrobatics: 8, crafting: 5, intimidation: 7, stealth: 8))
                    .WithCharacteristics(false, true)
                    .AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 2))
                    .AddQEffect(new QEffect("Kneecapper {icon:Reaction}", "When an adjacent creature uses a move action, you can make an Acrobatics check against the creature's Reflex DC. On a success, disrupt the action and the target falls and lands prone.")
                    {
                        Id = QEffectId.AttackOfOpportunity,
                        WhenProvoked = async (qfWhenProvoked, action) =>
                        {
                            Creature self = qfWhenProvoked.Owner;
                            if (action.HasTrait(Trait.Move) && await self.AskToUseReaction("Make an Acrobatics check to try to make the creature fall prone?"))
                            {
                                if (CommonSpellEffects.RollCheck("Kneecapper", new ActiveRollSpecification(Checks.SkillCheck([Skill.Acrobatics]), Checks.DefenseDC(Defense.Reflex)), self, action.Owner) >= CheckResult.Success)
                                {
                                    Sfxs.Play(SfxName.DropProne);
                                    action.Owner.AddQEffect(QEffect.Prone());
                                    action.Disrupted = true;
                                }
                            }
                        },
                    })
                    .AddQEffect(new QEffect("Improved Sneak Attack", "Strikes deal an additional 1d6 precision damage to flat-footed targets, or 1d10 if the target is prone.")
                    {
                        YouBeginAction = async (qfBeginAction, action) =>
                        {
                            if (action.HasTrait(Trait.Strike))
                            {
                                string sneakAttackDamage = (action.ChosenTargets.ChosenCreature?.HasEffect(QEffectId.Prone) ?? false) ? "1d10" : "1d6";
                                QEffect sneakAttack = QEffect.SneakAttack(sneakAttackDamage);
                                sneakAttack.Name = "Improved Sneak Attack - TEMP";
                                qfBeginAction.Owner.AddQEffect(sneakAttack);
                            }
                        },
                        AfterYouTakeAction = async (qfAfterAction, action) =>
                        {
                            qfAfterAction.Owner.RemoveAllQEffects(qe => qe.Name == "Improved Sneak Attack - TEMP");
                        }
                    })
                    // MISSING SPELLS: Level 2 - Shatter | Cantrip - Prestidigitation
                    .AddMonsterInnateSpellcasting(8, Trait.Primal, level2Spells: [SpellId.BoneSpray], level1Spells: [SpellId.Grease, SpellId.ShockingGrasp, SpellId.Shield])
                    .AddQEffect(QEffect.MonsterGrab())
                    .Builder
                    .AddNaturalWeapon(NaturalWeaponKind.Jaws, 11, [Trait.Finesse, Trait.Grab], "1d8+1", DamageKind.Piercing)
                    .AddNaturalWeapon(NaturalWeaponKind.Claw, 11, [Trait.Agile, Trait.Finesse], "1d6+1", DamageKind.Slashing)
                    .Done();
        }
    }
}