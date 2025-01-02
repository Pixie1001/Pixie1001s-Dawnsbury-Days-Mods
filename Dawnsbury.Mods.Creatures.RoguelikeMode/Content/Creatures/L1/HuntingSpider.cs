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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class HuntingSpider
    {
        public static Creature Create()
        {
            return new Creature(Illustrations.HuntingSpider, "Hunting Spider", new List<Trait>() { Trait.Animal, ModTraits.Spider }, 1, 7, 5, new Defenses(17, 6, 9, 5), 16,
            new Abilities(2, 4, 1, -5, 2, -2), new Skills(acrobatics: 7, stealth: 7, athletics: 5))
            .WithAIModification(ai =>
            {
                ai.OverrideDecision = (self, options) =>
                {
                    Creature creature = self.Self;
                    Option best = options.MaxBy(o => o.AiUsefulness.MainActionUsefulness);
                    return null;
                };
            })
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithProficiency(Trait.Ranged, Proficiency.Trained)
            .WithCharacteristics(false, true)
            .WithUnarmedStrike(new Item(IllustrationName.Jaws, "fangs", new Trait[] { Trait.Melee, Trait.Finesse, Trait.Unarmed, Trait.Brawling }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing)))
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            .AddQEffect(CommonQEffects.SpiderVenomAttack(16, "fangs"))
            .AddQEffect(CommonQEffects.WebAttack(16));
        }
    }
}