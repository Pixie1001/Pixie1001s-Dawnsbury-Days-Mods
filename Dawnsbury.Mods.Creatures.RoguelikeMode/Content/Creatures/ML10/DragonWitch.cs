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
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Roller;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DragonWitch {
        public static Creature Create() {
            return new Creature(Illustrations.DragonWitch, "Dragon Witch", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, Trait.Humanoid, Trait.Female, ModTraits.SpellcasterMutator },
                10, 19, 6, new Defenses(27, 16, 19, 22), 130,
            new Abilities(3, 3, 5, 7, 3, 3), new Skills(arcana: 25, intimidation: 22))
            .WithCreatureId(CreatureIds.DragonWitch)
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Melee, Proficiency.Master)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .AddHeldItem(Items.CreateNew(ItemName.Ranseur).WithMonsterWeaponSpecialization(6).WithAdditionalWeaponProperties(p => {
                p.WithAdditionalDamage("1d6", DamageKind.Fire);
            }))
            //.WithSpellProficiencyBasedOnSpellAttack(23, Ability.Intelligence)
            .AddSpellcastingSource(SpellcastingKind.Prepared, ModTraits.Witch, Ability.Intelligence, Trait.Arcane).WithSpells(
                level1: [SpellId.ProduceFlame, SpellId.Shield],
                level3: [SpellId.Fireball, SpellId.ScorchingRay, SpellId.Fear],
                level4: [SpellId.FireShield, SpellId.ScorchingRay, SpellId.BurningHands]
                //level5: [SpellId.BurningHands, SpellId.Fireball, SpellId.ScorchingRay]
                ).Done()
            .Builder
            .AddMainAction(you => {
                return new CombatAction(you, IllustrationName.BestowCurse, "Draconic Hex", [Trait.Spell, Trait.Manipulate, Trait.Flourish, Trait.SpellWithDuration, Trait.Cantrip],
                    $"[30 feet] spell save DC vs. Fort; Deals 5d4 fire damage vs. a Basic Fortitude save. Inflicts fire vulnability 5 on a failure and fire vulnability 5 on a critical failure. In addition, {you.Name}'s familiar deals 1d12 fire damage to each creature within 10ft.", Target.Ranged(6))
                .WithActionCost(1)
                .WithProjectileCone(IllustrationName.BestowCurse, 5, ProjectileKind.Cone)
                .WithSavingThrow(new SavingThrow(Defense.Fortitude, you.ClassOrSpellDC()))
                .WithSoundEffect(SfxName.BeastRoar)
                .WithGoodnessAgainstEnemy((t, a, d) => {
                    float score = 17f;
                    Creature? familiar = a.Battle.AllCreatures.FirstOrDefault(cr => cr.CreatureId == CreatureIds.YoungRedDragon);
                    if (familiar != null && familiar.Alive) {
                        score += a.Battle.AllCreatures.Where(cr => cr.EnemyOf(a) && cr.DistanceTo(familiar) <= 2).Count() * 6.5f;
                        score -= a.Battle.AllCreatures.Where(cr => cr != familiar && cr.FriendOf(a) && cr.DistanceTo(familiar) <= 2).Count() * 6.5f;
                    }
                    return score;
                })
                .WithEffectOnEachTarget(async (spell, caster, d, r) => {
                    await CommonSpellEffects.DealBasicDamage(spell, caster, d, r, DiceFormula.FromText("5d4", "Draconic Hex"), DamageKind.Fire);
                    if (r <= CheckResult.Failure) {
                        int num = r == CheckResult.Failure ? 5 : 10; 
                        var fireVuln = new QEffect {
                            StateCheck = delegate (QEffect qf) {
                                qf.Owner.WeaknessAndResistance.AddWeakness(DamageKind.Fire, num);
                            }
                        };
                        fireVuln.AddGrantingOfTechnical(cr => cr.EnemyOf(d), effect => {
                            effect.AdditionalGoodness = (self, action, target) => action.HasTrait(Trait.Fire) && target == d ? num : 0f;
                        });
                        d.AddQEffect(fireVuln);
                    }

                    Creature? familiar = caster.Battle.AllCreatures.FirstOrDefault(cr => cr.CreatureId == CreatureIds.YoungRedDragon);
                    if (familiar != null && familiar.Alive) {
                        var ca = new CombatAction(familiar, IllustrationName.Fireball, "Draconic Wrath", [Trait.Fire], "", Target.SelfExcludingEmanation(2))
                        .WithActionCost(0)
                        .WithSoundEffect(SfxName.Fireball)
                        .WithProjectileCone(IllustrationName.Fireball, 20, ProjectileKind.Cone)
                        .WithEffectOnEachTarget(async (action, _, d2, _) => {
                            await CommonSpellEffects.DealDirectDamage(action, DiceFormula.FromText("1d12"), d2, CheckResult.Success, DamageKind.Fire);
                        });
                        caster.Battle.AllCreatures.Where(cr => cr != familiar && cr.DistanceTo(familiar.Occupies) <= 2 && cr.HasLineOfEffectTo(familiar.Occupies) < CoverKind.Blocked).ForEach(cr => ca.ChosenTargets.ChosenCreatures.Add(cr));
                        caster.Battle.Map.AllTiles.Where(t => t != familiar.Occupies && t.DistanceTo(familiar.Occupies) <= 2 && t.HasLineOfEffectToIgnoreLesser(familiar.Occupies) < CoverKind.Blocked).ForEach(t => ca.ChosenTargets.ChosenTiles.Add(t));
                        await ca.AllExecute();
                        //caster.Battle.AllCreatures.Where(cr => cr.DistanceTo(familiar) <= 2).ForEach(async cr => {
                        //    if (cr.DistanceTo(familiar) <= 2 && familiar.HasLineOfEffectTo(cr) <= CoverKind.Greater)
                        //        await CommonSpellEffects.DealDirectDamage();
                        //});
                    }



                    //var qSustaining = QEffect.Sustaining(spell, qHideousLaughter);
                    //caster.AddQEffect(qSustaining);
                })
                ;
            })
            .Done();
        }
    }
}