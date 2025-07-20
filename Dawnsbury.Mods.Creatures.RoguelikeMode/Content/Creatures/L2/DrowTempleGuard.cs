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
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework.Graphics;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DrowTempleGuard {
        public static Creature Create() {
            return new Creature(Illustrations.DrowTempleGuard, "Drow Temple Guard", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Elf, ModTraits.Drow, Trait.Humanoid, ModTraits.MeleeMutator }, 2, 8, 6, new Defenses(18, 11, 8, 9), 28,
            new Abilities(4, 2, 3, 0, 2, 0), new Skills(athletics: 8, intimidation: 6))
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;

                    AiFuncs.PositionalGoodness(monster, options, (t, _, _, cr) => cr.HasEffect(QEffectIds.DrowClergy) && t.DistanceTo(cr.Occupies) <= 3, 2f);
                    AiFuncs.PositionalGoodness(monster, options, (t, _, _, cr) => cr.HasEffect(QEffectIds.DrowClergy) && t.DistanceTo(cr.Occupies) <= 2, 1f);

                    return null;
                };
            })
            .WithCreatureId(CreatureIds.DrowTempleGuard)
            .AddQEffect(CommonQEffects.Drow())
            .AddQEffect(CommonQEffects.DrowBloodBond())
            .AddQEffect(CommonQEffects.RetributiveStrike(2, cr => cr.HasEffect(QEffectIds.DrowClergy), "a member of the drow clergy", true))
            .AddQEffect(QEffect.DamageResistance(DamageKind.Negative, 5))
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .AddHeldItem(Items.CreateNew(ItemName.Halberd))
            .Builder
            .AddMainAction(you => {
                return new CombatAction(you, IllustrationName.SanguineMist, "Blood Ward", [Trait.Necromancy, Trait.Abjuration, Trait.Manipulate], "[Manipulate, 15-feet] Grant target cleric a magical barrier until the start of your next turn, that increases their AC by +2 and inflicts 1d6 negative damage to any creature that attacks them.", Target.RangedFriend(3)
                    .WithAdditionalConditionOnTargetCreature((a, d) => d.HasEffect(QEffectIds.DrowClergy) ? Usability.Usable : Usability.NotUsableOnThisCreature("not-a-member-of-the-drow-clergy")))
                .WithActionCost(2)
                .WithGoodness((t, a, d) => 2f)
                .WithSoundEffect(SfxName.Necromancy)
                .WithProjectileCone(IllustrationName.SanguineMist, 5,ProjectileKind.Cone)
                .WithEffectOnEachTarget(async (action, user, target, result) => {
                    target.AddQEffect(new QEffect("Blood Ward", "You gain +2 AC, and enemy creatures that damage you suffer 1d6 negative damage.", ExpirationCondition.ExpiresAtStartOfSourcesTurn, user, IllustrationName.SanguineMist) {
                        BonusToDefenses = (self, action, def) => def == Defense.AC ? new Bonus(2, BonusType.Untyped, "Blood Ward", true) : null,
                        AfterYouTakeDamage = async (self, amount, kind, action, crit) => {
                            if (action?.Owner?.Occupies == null || action?.Owner.OwningFaction == self.Owner.OwningFaction) return;
                            await CommonSpellEffects.DealDirectDamage(CombatAction.CreateSimple(self.Owner, "Blood Ward"), DiceFormula.FromText("1d6", "Blood Ward"), action!.Owner, CheckResult.Success, DamageKind.Negative);
                            Sfxs.Play(SfxName.Necromancy);
                        }
                    });
                })
                ;
            })
            .Done();
        }
    }
}