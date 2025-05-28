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
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Notifications;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using Microsoft.Xna.Framework;
using System;
using System.Xml.Linq;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class NightmareWeaver {
        public static Creature Create() {
            Item legAtk = new Item(new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.DragonClaws), "stabbing appendage", new Trait[] { Trait.Unarmed, Trait.Finesse, Trait.DeadlyD6, Trait.Reach }).WithWeaponProperties(new WeaponProperties("2d6", DamageKind.Piercing));

            Creature monster = new Creature(new SpiderIllustration(Illustrations.AbyssalHandmaiden, Illustrations.Bear3), "Nightmare Weaver",
                [Trait.Chaotic, Trait.Evil, Trait.Demon, Trait.Fiend, ModTraits.Spider, ModTraits.SpellcasterMutator],
                6, 14, 6, new Defenses(22, 11, 17, 14), 95,
            new Abilities(5, 5, 4, 4, 2, 6),
            new Skills(acrobatics: 15, athletics: 14, intimidation: 18, religion: 10, occultism: 14))
            .WithCreatureId(CreatureIds.NightmareWeaver)
            .WithProficiency(Trait.Melee, Proficiency.Master)
            .WithProficiency(Trait.Spell, Proficiency.Expert)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(legAtk)
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Good, 5))
            .AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 5))
            .AddQEffect(QEffect.FrightfulPresence(6, 18))
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            .AddQEffect(QEffect.WebSense())
            .AddQEffect(new QEffect("Bravery Vulnerability", "When an enemy critical succeeds against a fear effect, or you critically fail a demoralize attempt against them, you suffer 3d6 mental damage and become frightened yourself.") {
                AfterYouTakeActionAgainstTarget = async (effect, action, defender, checkResult) => {
                    var demon = action.Owner;
                    if (action.HasTrait(Trait.Fear) && !defender.FriendOf(demon)) {
                        if ((action.ActiveRollSpecification != null && checkResult == CheckResult.CriticalFailure) ||
                            (action.SavingThrow != null && checkResult >= CheckResult.CriticalSuccess)) {
                            await CommonSpellEffects.DealDirectDamage(null, DiceFormula.FromText("3d6", "Bravery vulnerability"), demon, CheckResult.Failure, DamageKind.Mental);
                            demon.AddQEffect(QEffect.Frightened(1));
                        }
                    }
                }
            })
            .AddQEffect(new QEffect("Strength from Nightmares", "The nightmare weaver's strike deals +6 damage to frightened creatures, and +2 AC against their attacks.") {
                BonusToDamage = (self, action, target) => target.HasEffect(QEffectId.Frightened) ? new Bonus(6, BonusType.Untyped, "strength from nightmares") : null,
                BonusToDefenses = (self, action, def) => action?.Owner?.Occupies != null && action.Owner.HasEffect(QEffectId.Frightened) ? new Bonus(2, BonusType.Untyped, "strength from nightmares") : null,
                AdditionalGoodness = (self, action, target) => action.HasTrait(Trait.Strike) && target.HasEffect(QEffectId.Frightened) ? 6 : 0
            })
            .AddSpellcastingSource(SpellcastingKind.Prepared, Trait.Demon, Ability.Charisma, Trait.Divine).WithSpells(
                level1: [SpellId.Daze, SpellId.Fear, SpellId.Fear, SpellId.Fear],
                level2: [SpellId.PhantomPain, SpellId.PhantomPain, SpellId.PhantomPain],
                level3: [SpellLoader.AgonisingDespair, SpellId.Fear, SpellLoader.AgonisingDespair]).Done()
            ;
            return monster;
        }
    }
}
