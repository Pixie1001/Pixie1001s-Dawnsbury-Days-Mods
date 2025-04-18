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
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

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
            .AddQEffect(CommonQEffects.RetributiveStrike(4, cr => cr.HasEffect(QEffectIds.DrowClergy), "a member of the drow clergy", true))
            //.AddQEffect(QEffect.AttackOfOpportunity())
            .AddQEffect(QEffect.DamageResistance(DamageKind.Negative, 5))
            .WithBasicCharacteristics()
            .WithProficiency(Trait.Weapon, Proficiency.Expert)
            .AddHeldItem(Items.CreateNew(ItemName.Halberd));
        }
    }
}