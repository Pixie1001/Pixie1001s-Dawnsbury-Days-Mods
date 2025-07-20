using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static class Crocodile
    {
        public static Creature Create()
        {
            var creature = new Creature(Illustrations.Crocodile,
                "Crocodile",
                [Trait.Animal, ModTraits.MeleeMutator, Trait.NoPhysicalUnarmedAttack],
                2, 7, 4,
                new Defenses(17, 9, 7, 5),
                30,
                new Abilities(4, 1, 3, -5, 1, -4),
                new Skills(athletics: 8, stealth: 7))
            .WithCharacteristics(false, true)
            .AddQEffect(QEffect.MonsterGrab(false))
            .AddQEffect(QEffect.Swimming())
            .Builder
            .AddMainAction((creature) =>
            {
                return new CombatAction(creature, IllustrationName.Jaws, "Death Roll", [], "The crocodile must have a creature grabbed; Effect The crocodile tucks its legs and rolls rapidly, twisting its victim. It makes a jaws Strike with a +2 circumstance bonus to the attack roll against the grabbed creature. If it hits, it also knocks the creature prone. If it fails, it releases the creature.",
                    Target.ReachWithAnyWeapon().WithAdditionalConditionOnTargetCreature((Creature user, Creature target) =>
                    {
                        return target.QEffects.Any((effect) => effect.Id == QEffectId.Grappled && effect.Source == user) ? Usability.Usable : Usability.NotUsableOnThisCreature("not grappling");
                    }))
                .WithActionCost(1)
                .WithGoodness((Target _, Creature user, Creature target) => user.Actions.AttackedThisManyTimesThisTurn < 2 ? AIConstants.EXTREMELY_PREFERRED : AIConstants.NEVER)
                .WithEffectOnEachTarget(async (CombatAction action, Creature user, Creature target, CheckResult _) =>
                {
                    //The unarmed strike is replaced by the first natural weapon added
                    user.AddQEffect(new(ExpirationCondition.ExpiresAtEndOfAnyTurn)
                    {
                        Tag = user,
                        BonusToAttackRolls = (_, _, _) => new Bonus(2, BonusType.Circumstance, "Death Roll", true)
                    });

                    var result = await user.MakeStrike(target, user.UnarmedStrike);

                    if (result >= CheckResult.Success)
                    {
                        target.AddQEffect(QEffect.Prone());
                    }
                    else
                    {
                        target.RemoveAllQEffects((effect) => effect.Id == QEffectId.Grappled && effect.Source == user);
                    }

                    user.RemoveAllQEffects((effect) => effect.Tag == user);
                });
            })
            .Done();
            creature = UtilityFunctions.AddNaturalWeapon(creature, "jaws", IllustrationName.Jaws, 10, [Trait.Grab], "1d10+4", DamageKind.Piercing, null);
            creature = UtilityFunctions.AddNaturalWeapon(creature, "tail", IllustrationName.Tail, 10, [], "1d6+4", DamageKind.Bludgeoning, null);

            return creature;
        }
    }
}
