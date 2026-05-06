using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Kineticist;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Archetypes;
using Dawnsbury.Core.CharacterBuilder.Selections.Selected;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks.Monsters.L_1;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Text;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class AngelfireAdept {
        public static FeatName AngelfireNimbus { get; } = ModManager.RegisterFeatName("RL_AngelFireNimbus", "Angelfire Nimbus");
        public static FeatName AngelfireNova { get; } = ModManager.RegisterFeatName("RL_AngelfireNova", "Angelfire Nova");
        public static FeatName AngelfireBarrier { get; } = ModManager.RegisterFeatName("RL_AngelFireBarrier", "Angelfire Barrier");
        public static FeatName GiftOfAngelFire { get; } = ModManager.RegisterFeatName("RL_GiftOfAngelFire", "Gift of Angelfire");

        public static IEnumerable<Feat> CreateFeats() {
            List<Trait> classTraits = new List<Trait>();
            AllFeats.All.ForEach(ft => {
                if (ft is ClassSelectionFeat classFeat) {
                    classTraits.Add(classFeat.ClassTrait);
                }
            });

            var dedicationFeat = ArchetypeFeats.CreateAgnosticArchetypeDedication(ModTraits.AngelfireAdept, "...",
                "You gain the following action, allowing to project your angelfire in a searing blast of radiant energy:\n\n" +
                "{b}Throw Angelfire {icon:Action}.{/b} (Good, Fire)\n" +
                "{b}Range{/b} 30 feet\n{b}Saving throw{/b} reflex (basic)\n{b}Frequency{/b} once per round\n\n" +
                "The target takes 2d8 fire damage (basic reflex save mitigates).\n\n" +
                "The damage increases by 1d8 at 3rd level and every 2 levels thereafter."
                , null)
            .WithOnCreature((sheet, creature) => {
                creature.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        var action = (ActionPossibility)new CombatAction(self.Owner, IllustrationName.ProduceFlame, "Throw Angelfire", new Trait[] { Trait.Evocation, Trait.Fire, Trait.Good, Trait.Manipulate },
                            "{b}Range{/b} 30 feet\n{b}Saving throw{/b} reflex (basic)\n{b}Frequency{/b} once per round\n\n" +
                            $"The target takes {S.HeightenedVariable(1 + ((self.Owner.Level + 1) / 2), 2)}d8 fire damage (basic reflex save mitigates)", Target.Ranged(6)) {
                            ShortDescription = $"30 feet; {S.HeightenedVariable(1 + ((self.Owner.Level + 1) / 2), 2)}d8 fire damage (basic reflex save mitigates)."
                        }
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.FireRay)
                        .WithSavingThrow(new SavingThrow(Defense.Reflex, caster => caster?.ClassOrSpellDC() ?? 10))
                        .WithEffectOnEachTarget(async (spell, caster, target, result) => {
                            caster.AddQEffect(new QEffect() { Id = QEffectIds.ThrowAngelfireUsedUp });
                        });
                        action.PossibilityGroup = Constants.POSSIBILITY_GROUP_RACIAL_AND_CLASS_POWERS;
                        return action;
                    },
                });
            })
            .WithPrerequisite(sheet => {
                if (sheet.Sheet.SelectedFeats.TryGetValue("RL_GiftOfAngelFire", out SelectedChoice val) || PlayerProfile.Instance.IsBooleanOptionEnabled("RL_AllowRatMonarch")) {
                    return true;
                } else {
                    return false;
                }
            }, "This archetype can only be taken by one who has proved themselves worthy of wielding the fire of heaven.");

            dedicationFeat.Traits.Add(ModTraits.Event);
            dedicationFeat.Traits.Add(ModTraits.Roguelike);

            yield return dedicationFeat;

            yield return new TrueFeat(AngelfireNimbus, 4, "...",
                "Creatures who hit you with a melee attack take 1d8 fire damage with the good trait each time they do {i}(no save).{/i}",
                new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            .WithPermanentQEffectAndSameRulesText(qfFeat => {
                qfFeat.AfterYouAreTargeted = async (effect, action) => {
                    if (action.ChosenTargets.CheckResults.GetValueOrDefault(effect.Owner) >= CheckResult.Success) {
                        if (action.HasTrait(Trait.Attack) && action.HasTrait(Trait.Melee)) {
                            await CommonSpellEffects.DealDirectDamage(CombatAction.CreateSimple(qfFeat.Owner, "Angelfire Nimbus", [Trait.Fire, Trait.Good]), DiceFormula.FromText($"1d8", "Angelfire Nimbus"), action.Owner, CheckResult.Failure, DamageKind.Fire);
                        }
                    }
                };
            })
            .WithAvailableAsArchetypeFeat(ModTraits.AngelfireAdept);

            yield return CommonFeatTemplates.CreateFocusSpellFeat(AngelfireNova, 4, "...", SpellLoader.AgonisingDespair, ModTraits.AngelfireAdept, Ability.Charisma).WithAvailableAsArchetypeFeat(ModTraits.AngelfireAdept);

            //yield return CommonFeatTemplates.CreateFocusSpellFeat(AngelfireBarrier, 6, "...", SpellLoader.AgonisingDespair, ModTraits.AngelfireAdept, Ability.Charisma).WithAvailableAsArchetypeFeat(ModTraits.AngelfireAdept);

            //yield return new TrueFeat(AngelfireBarrier, 6, "...",
            //    $"You create either a wall of flame in a straight line up to 60 feet, or a 10-foot-radius ring. The wall initially deals no damage, but it deals 2d6 fire damage {{i}}(no save){{/i}} to each creature that enters the wall or starts its turn there.",
            //    new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            //.Focus(qfFeat => {
            //    qfFeat.AfterYouAreTargeted = async (effect, action) => {
            //        if (action.ChosenTargets.CheckResults.GetValueOrDefault(effect.Owner) >= CheckResult.Success) {
            //            if (action.HasTrait(Trait.Attack) && action.HasTrait(Trait.Melee)) {
            //                await CommonSpellEffects.DealDirectDamage(CombatAction.CreateSimple(qfFeat.Owner, "Angelfire Nimbus", [Trait.Fire, Trait.Good]), DiceFormula.FromText($"1d8", "Angelfire Nimbus"), action.Owner, CheckResult.Failure, DamageKind.Fire);
            //            }
            //        }
            //    };
            //})
            //.WithAvailableAsArchetypeFeat(ModTraits.AngelfireAdept);

            //yield return new TrueFeat(SwarmLord, 4, "You and your rats fight as one.",
            //    "When you hit a creature with an attack roll, each adjacent rat familiar may use its reaction to make a jaws attack against them. Attacks triggered by ranged attacks suffer a -2 penalty.",
            //    new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            //.WithAvailableAsArchetypeFeat(ModTraits.RatMonarch)
            //.WithOnCreature((sheet, creature) => {
            //    creature.AddQEffect(new QEffect("Swarm Lord", "When you hit a creature with an attack roll, each adjacent rat familiar may use its reaction to make a jaws attack against them. Attacks triggered by ranged attacks suffer a -2 penalty.") {
            //        AfterYouTakeActionAgainstTarget = async (self, action, target, result) => {
            //            if (!action.HasTrait(Trait.Attack) || result <= CheckResult.Failure) {
            //                return;
            //            }
            //            bool ranged = !self.Owner.IsAdjacentTo(target) && (action.Target is not CreatureTarget || action.Target is CreatureTarget ct && ct.RangeKind == RangeKind.Ranged);

            //            foreach (Creature rat in self.Owner.Battle.AllCreatures.Where(cr => cr.QEffects.Any(qf => qf.Id == QEffectIds.RatFamiliar && qf.Source == self.Owner) && cr.IsAdjacentTo(target))) {
            //                if (!await rat.AskToUseReaction("Would you like to attack the target of your master's attack?")) {
            //                    return;
            //                }

            //                StrikeModifiers mod = new StrikeModifiers();
            //                if (ranged) {
            //                    mod.AdditionalBonusesToAttackRoll = new List<Bonus>() { new Bonus(-2, BonusType.Untyped, "Ranged Trigger") };
            //                }
            //                CombatAction ca = rat.CreateStrike(rat.UnarmedStrike, 0, mod).WithActionCost(0);
            //                ca.ChosenTargets.ChosenCreature = target;
            //                ca.ChosenTargets.ChosenCreatures.Add(target);
            //                await ca.AllExecute();
            //            }
            //        }
            //    });
            //});

            //yield return new TrueFeat(BurrowingDeath, 6, "You command your rats to swarm over the subject of your ire, burrowing their into their flesh with suicidal determination.",
            //    "You learn the {i}burrowing death{/i} focus spell. Increase the number of Focus Points in your focus pool by 1.\n\n{b}Special.{/b} Your Rat Monarch spellcasting DC is equal to your highest class or spell save DC.",
            //    new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            //.WithAvailableAsArchetypeFeat(ModTraits.RatMonarch)
            //.WithActionCost(2)
            //.WithIllustration(Illustrations.BurrowingDeath)
            //.WithOnSheet(sheet => {
            //    // sheet.SetProficiency(ModTraits.RatMonarch, (Proficiency)Math.Max((int)sheet.GetProficiency(sheet.Class?.ClassTrait ?? Trait.Spell), (int)sheet.GetProficiency(Trait.Spell)));
            //    sheet.AddFocusSpellAndFocusPoint(ModTraits.RatMonarch, sheet.FinalAbilityScores.KeyAbility, SpellLoader.BurrowingDeath);
            //})
            //.WithOnCreature((sheet, creature) => {
            //    creature.Proficiencies.Set(ModTraits.RatMonarch, (Proficiency)Math.Max((int)creature.Proficiencies.Get(sheet.Class?.ClassTrait ?? Trait.Spell), (int)creature.Proficiencies.Get(Trait.Spell)));
            //})
            //.WithRulesBlockForSpell(SpellLoader.BurrowingDeath);

            //yield return new TrueFeat(Incubator, 6, "The corpses of your foes bulge and writhe, as fresh subjects burrow free of their carcass to serve you.",
            //    "After you or one of your familiars reduces an enemy to 0 HP, you summon a rat familiar in their place.",
            //    new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            //.WithAvailableAsArchetypeFeat(ModTraits.RatMonarch)
            //.WithPermanentQEffectAndSameRulesText((qfSelf) => {
            //    qfSelf.AfterYouDealDamage = async (you, _, target) => {
            //        if (target.HP <= 0 && target.EnemyOf(you) && target.IsLivingCreature) {
            //            you.Overhead("*incubator*", Color.White, $"A rat crawls out from {target.Name}'s corpse to serve {you.Name}.");
            //            SpawnRatFamiliar(you, target.Occupies);
            //        }
            //    };
            //});

            //yield return new TrueFeat(DireRats, 8, "Your rats grow larger and more aggressive.",
            //    "Your rat familiar's jaw attack deals an extra damage die, and they gain a +1 bonus to their strength, dexterity, constitution and wisdom modifiers.",
            //    new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            //.WithPermanentQEffectAndSameRulesText(qf => { })
            //.WithAvailableAsArchetypeFeat(ModTraits.RatMonarch);
        }
    }
}