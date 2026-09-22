using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.AuraAnimations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Mechanics.Zoning;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class VampireDuelist {
        public static Creature Create() {
            var weapon = Items.CreateNew(ItemName.Rapier).WithAdditionalWeaponProperties(wp => {
                wp.WithAdditionalPersistentDamage("1d6", DamageKind.Bleed);
                wp.DamageDieCount = 2;
                wp.ItemBonus = 1;
            });
            weapon.Traits.Add(Trait.Grab);

            var monster = CommonQEffects.MakeVampire(new Creature(IllustrationName.ElfF2, "Vampire Duelist", [Trait.Undead, Trait.Lawful, Trait.Evil, ModTraits.MeleeMutator], level: 5, perception: 18, speed: 6, new Defenses(21, 9, 17, 12), hp: 65, new Abilities(4, 5, 3, 3, 5, 4), new Skills(athletics: 13, intimidation: 14, acrobatics: 15, society: 13)), 5, 10)
                .WithBasicCharacteristics()
                .WithCreatureId(CreatureIds.VampireDuelist)
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .WithUnarmedStrike(NaturalWeapons.Create(NaturalWeaponKind.Claw, "2d4", DamageKind.Slashing, [Trait.Agile, Trait.Finesse, Trait.Grab]))
                .AddHeldItem(weapon)
                .AddQEffect(new QEffect("Vampiric Reflexes", "The vampire duelist gains a +4 status bonus to AC against ranged attacks.") {
                    AddToDefenseBlock = qfSelf => "Vampiric Reflexes",
                    BonusToDefenses = (qfSelf, ca, def) => ca != null && def == Defense.AC && ca.HasTrait(Trait.Attack) && !ca.IsMeleeAgainst(qfSelf.Owner) ? new Bonus(4, BonusType.Status, "Vampiric reflexes") : null
                })
                .AddQEffect(new QEffect("Vampiric Reflexes", "The vampire duelist gains a +4 status bonus to AC against ranged attacks.") {
                    AddToDefenseBlock = qfSelf => "Vampiric Reflexes",
                    BonusToDefenses = (qfSelf, ca, def) => ca != null && def == Defense.AC && ca.HasTrait(Trait.Attack) && !ca.IsMeleeAgainst(qfSelf.Owner) ? new Bonus(4, BonusType.Status, "Vampiric reflexes") : null
                })
                .AddQEffect(CommonQEffects.Counterattack("Riposte"))
                .Builder
                .AddMainAction(you => {
                    if (you.PrimaryWeapon == null) return null;
                    return new CombatAction(you, IllustrationName.BlinkCharge, "Misty Assault", [Trait.Teleportation, Trait.Attack],
                        UtilityFunctions.CombatActionStats(area: "10-foot emanation") + "THe vampire duelist makes an attack against each enemy creature in the area. These strikes do not count against it's multiple attack penalty.",
                        Target.EnemiesOnlyEmanation(2))
                    .WithSoundEffect(SfxName.PhaseBolt)
                    .WithActionCost(3)
                    .WithGoodnessAgainstEnemy((t, a, d) => a.CreateStrike(a.PrimaryWeapon!).TrueDamageFormula?.ExpectedValue ?? 0)
                    .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                        int map = caster.Actions.AttackedThisManyTimesThisTurn;
                        await caster.MakeStrike(target, caster.PrimaryWeapon!, 0);
                        caster.Actions.AttackedThisManyTimesThisTurn = map;
                    });
                })
                .Done();

            return monster;
        }
    }
}