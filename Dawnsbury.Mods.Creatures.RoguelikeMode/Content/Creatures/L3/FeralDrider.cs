using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines.Options.Reactive;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    public class FeralDrider {
        public static Creature Create() {
            return new Creature(Illustrations.FeralDrider, "Feral Drider", new List<Trait>() { Trait.Large, Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Aberration, ModTraits.Spider, Trait.Female, Trait.MetalArmor, ModTraits.MeleeMutator }, 3, 6, 6, new Defenses(17, 9, 7, 6), 56,
            new Abilities(5, 3, 3, 1, 3, 2), new Skills(athletics: 10, intimidation: 8))
            .WithCreatureId(CreatureIds.FeralDrider)
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Ranged, Proficiency.Expert)
            .WithBasicCharacteristics()
            .AddHeldItem(Items.CreateNew(ItemName.Halberd))
            .WithUnarmedStrike(new Item(IllustrationName.Jaws, "fangs", new Trait[] { Trait.Melee, Trait.Finesse, Trait.Unarmed, Trait.Brawling }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)))
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(new QEffect("Feral Sweep {icon:Reaction}", "After being damaged by an enemy, the Feral Drider may make an attack against all enemies within its reach, with a -3 circumstance penalty to hit.") {
                AfterYouTakeDamageReaction = (qfSelf, dmg) => {
                    if (!qfSelf.Owner.Actions.CanTakeReaction() || qfSelf.Owner.PrimaryWeapon == null) return null;
                    var reach = qfSelf.Owner.PrimaryWeapon.DetermineReach(qfSelf.Owner);
                    var targets = qfSelf.Owner.Battle.AllCreatures.Where(cr => cr.EnemyOf(qfSelf.Owner) && qfSelf.Owner.DistanceTo(cr) <= reach && qfSelf.Owner.HasLineOfEffectToIgnoreLesser(cr) != CoverKind.Blocked).ToList();
                    if (!dmg.Source.EnemyOf(qfSelf.Owner) || targets.Count == 0) return null;
                    
                    var ca = new CombatAction(qfSelf.Owner, IllustrationName.Swipe, "Feral Sweep", [], "Make an attack against all enemies within your reach, with a -3 circumstance penalty.", Target.Uncastable())
                    .WithActionCost(0);
                    return new ReactionOptions([ReactionOption.CreateFromCombatActionCustom(ca, "Make a Strike against each enemy within your reach, with a -3 circumstance penalty.", async () => {
                        qfSelf.Owner.Actions.UseUpReaction();
                        Sfxs.Play(SfxName.RageFemale1);
                        var penalty = new QEffect() { BonusToAttackRolls = (qf, strike, t) => strike.HasTrait(Trait.Strike) ? new Bonus(-3, BonusType.Circumstance, "Feral Sweep") : null };
                        qfSelf.Owner.AddQEffect(penalty);
                        foreach(var target in targets) {
                            await qfSelf.Owner.MakeStrike(target, qfSelf.Owner.PrimaryWeapon, 0);
                        }
                        penalty.ExpiresAt = ExpirationCondition.Immediately;
                    })]);
                }
            })
            .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "fangs")) // Change to drider venom?
            .AddQEffect(CommonQEffects.WebAttack(16))
            .AddQEffect(CommonQEffects.MiniBoss())
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            .AddQEffect(QEffect.WebSense());
        }
    }
}