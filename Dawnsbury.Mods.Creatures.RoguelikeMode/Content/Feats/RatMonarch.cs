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
using Dawnsbury.Core.StatBlocks.Monsters.L_1;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.IO;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using Dawnsbury.ThirdParty.SteamApi;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content {

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class RatMonarch {
        public static FeatName PlagueRats { get; } = ModManager.RegisterFeatName("Plague Rats");
        public static FeatName SwarmLord { get; } = ModManager.RegisterFeatName("Swarm Lord");
        public static FeatName DireRats { get; } = ModManager.RegisterFeatName("RL_Dire Rats", "Dire Rats");
        public static FeatName BurrowingDeath { get; } = ModManager.RegisterFeatName("RL_Burrowing Death", "Burrowing Death");
        public static FeatName Incubator { get; } = ModManager.RegisterFeatName("RL_Incubator", "Incubator");
        public static FeatName PowerOfTheRatFiend { get; } = ModManager.RegisterFeatName("Power of the Rat Fiend");

        public static IEnumerable<Feat> CreateFeats() {
            //List<Trait> classTraits = new List<Trait>();
            //AllFeats.All.ForEach(ft => {
            //    if (ft is ClassSelectionFeat classFeat) {
            //        classTraits.Add(classFeat.ClassTrait);
            //    }
            //});

            yield return new Feat(PowerOfTheRatFiend, "", "", [ModTraits.Event], null);

            var ratMonarch = ArchetypeFeats.CreateAgnosticArchetypeDedication(ModTraits.RatMonarch, "The Rat Monarch lords over their flock of rodents, that emerge from the most neglected corners of the abyss to fulfill their master's will.",
                "You gain the following actions, allowing to summon forth and direct a swarm of vicious rats:\n\n" +
                "{b}Call Rats {icon:TwoActions}.{/b}\n" +
                "{b}Range{/b} self\n\nSpawn 2 friendly rat familiars into the nearest unoccupied space available to you." +
                "Your familiars have the base statistics of a Giant Rat, but their level is equal to your own -2, adjusting their defences and attack bonuses and increasing their max HP by 3 per level.\n\n" +
                "{b}Command Swarm {icon:Action}.{/b}\n" +
                "{b}Range{/b} 30 feet\n{b}Target{/b} 1 enemy creature\n\n" +
                "You mark the target creature with a demonic curse, attracting your rat familiars to them. While marked, your familiars gain a +1 status bonus to attack, and a +2 damage bonus against the target and are more likely to attack them."
                , null)
            .WithOnCreature((sheet, creature) => {
                creature.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        var action = (ActionPossibility)new CombatAction(self.Owner, IllustrationName.SummonAnimal, "Call Rats", new Trait[] { Trait.Conjuration, Trait.Manipulate },
                            "{b}Range{/b} self\n\nSpawn 2 friendly rat familiars into the nearest unoccupied space available to you.\n\n" +
                            "Your familiars have the base statistics of a Giant Rat, but their level is equal to your own -2, adjusting their defences and attack bonuses and increasing their max HP by 3 per level.", Target.Self()) {
                            ShortDescription = "Summoner 3 familirs into nearby unoccupied spaces."
                        }
                        .WithActionCost(2)
                        .WithSoundEffect(SfxName.ScratchFlesh)
                        .WithEffectOnSelf(async (action, caster) => {
                            for (int i = 0; i <= 1; i++) {
                                SpawnRatFamiliar(self.Owner);
                            }
                        });
                        action.PossibilityGroup = "Rat Monarch";
                        action.PossibilitySize = PossibilitySize.Half;
                        return action;
                    },
                });

                creature.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        var action = (ActionPossibility)new CombatAction(self.Owner, IllustrationName.Command, "Command Swarm", new Trait[] { Trait.Manipulate },
                            "{b}Range{/b} 30 feet\n{b}Target{/b} 1 enemy creature\n\n" +
                            "You mark the target creature with a demonic curse, attracting your rat familiars to them. While marked, your familiars gain a +1 status bonus to attack, and a +2 damage bonus against the target and are more likely to attack them.", Target.Ranged(6)) {
                            ShortDescription = "Command your rat familiars to attack the target creature, granting them a +1 status bonus to attack rolls, and a +2 bonus to damage against them until you move the mark."
                        }
                        .WithActionCost(1)
                        .WithSoundEffect(SfxName.ScratchFlesh)
                        .WithEffectOnEachTarget(async (action, caster, target, result) => {
                            caster.Battle.AllCreatures.ForEach(cr => cr.RemoveAllQEffects(qf => qf.Id == QEffectIds.CommandSwarm && qf.Source == caster));
                            QEffect effect = new QEffect("Marked for the Swarm",
                                $"{caster.Name}'s rat familiars are drawn to you, causing them to deal +2 damage and gain a +1 bonus to attack rolls.",
                                ExpirationCondition.Never, caster, IllustrationName.GiantRat256) { Id = QEffectIds.CommandSwarm };
                            effect.AddGrantingOfTechnical(cr => cr.QEffects.Any(qf => qf.Id == QEffectIds.RatFamiliar && qf.Source == caster), qfMark => {
                                qfMark.AdditionalGoodness = (self, action, d) => d.QEffects.Any(qf => qf.Id == QEffectIds.CommandSwarm && qf.Source == caster) ? 20f : -5f;
                                qfMark.BonusToAttackRolls = (self, action, d) => d != null && d.QEffects.Any(qf => qf.Id == QEffectIds.CommandSwarm && qf.Source == caster) ? new Bonus(1, BonusType.Status, "Marked for the Swarm", true) : null;
                                qfMark.BonusToDamage = (self, action, d) => action.HasTrait(Trait.Strike) && d.QEffects.Any(qf => qf.Id == QEffectIds.CommandSwarm && qf.Source == caster) ? new Bonus(2, BonusType.Untyped, "Marked for the Swarm", true) : null;
                            });
                            target.AddQEffect(effect);
                        });
                        action.PossibilityGroup = "Rat Monarch";
                        action.PossibilitySize = PossibilitySize.Half;
                        return action;
                    },
                });
            })
            .WithPrerequisite(sheet => {
                if (sheet.Sheet.SelectedFeats.TryGetValue("Power of the Rat Fiend", out SelectedChoice val) || PlayerProfile.Instance.IsBooleanOptionEnabled("RL_AllowRatMonarch")) {
                    return true;
                } else {
                    return false;
                }
            }, "This archetype can only be taken by one who has stolen the power of the Rat Fiend.");

            ratMonarch.Traits.Add(ModTraits.Event);
            ratMonarch.Traits.Add(ModTraits.Roguelike);

            yield return ratMonarch;

            yield return new TrueFeat(PlagueRats, 4, "Your rats grow larger and more vicious, carrying a wasting otherwordly plague.",
                "Your rat familiars gain +5 HP and inflict Rat Plague on a successful jaws attack, an affliction with a DC equal to the higher of your class or spell DC.\n\n{b}Rat Plague{/b}\n{b}Stage 1{/b} 1d6 poison damage and enfeebled 1; {b}Stage 2{/b} 2d6 poison damage and enfeebled 2; {b}Stage 3{/b} 3d6 poison damage and enfeebled 2.",
                new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            .WithAvailableAsArchetypeFeat(ModTraits.RatMonarch);

            yield return new TrueFeat(SwarmLord, 4, "You and your rats fight as one.",
                "When you hit a creature with an attack roll, each adjacent rat familiar may use its reaction to make a jaws attack against them. Attacks triggered by ranged attacks suffer a -2 penalty.",
                new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            .WithAvailableAsArchetypeFeat(ModTraits.RatMonarch)
            .WithOnCreature((sheet, creature) => {
                creature.AddQEffect(new QEffect("Swarm Lord", "When you hit a creature with an attack roll, each adjacent rat familiar may use its reaction to make a jaws attack against them. Attacks triggered by ranged attacks suffer a -2 penalty.") {
                    AfterYouTakeActionAgainstTarget = async (self, action, target, result) => {
                        if (!action.HasTrait(Trait.Attack) || result <= CheckResult.Failure) {
                            return;
                        }
                        bool ranged = !self.Owner.IsAdjacentTo(target) && (action.Target is not CreatureTarget || action.Target is CreatureTarget ct && ct.RangeKind == RangeKind.Ranged);

                        foreach (Creature rat in self.Owner.Battle.AllCreatures.Where(cr => cr.QEffects.Any(qf => qf.Id == QEffectIds.RatFamiliar && qf.Source == self.Owner) && cr.IsAdjacentTo(target))) {
                            if (!await rat.AskToUseReaction("Would you like to attack the target of your master's attack?")) {
                                return;
                            }

                            StrikeModifiers mod = new StrikeModifiers();
                            if (ranged) {
                                mod.AdditionalBonusesToAttackRoll = new List<Bonus>() { new Bonus(-2, BonusType.Untyped, "Ranged Trigger") };
                            }
                            CombatAction ca = rat.CreateStrike(rat.UnarmedStrike, 0, mod).WithActionCost(0);
                            ca.ChosenTargets.ChosenCreature = target;
                            ca.ChosenTargets.ChosenCreatures.Add(target);
                            await ca.AllExecute();
                        }
                    }
                });
            });

            yield return new TrueFeat(BurrowingDeath, 6, "You command your rats to swarm over the subject of your ire, burrowing their into their flesh with suicidal determination.",
                "You learn the {i}burrowing death{/i} focus spell. Increase the number of Focus Points in your focus pool by 1.\n\n{b}Special.{/b} Your Rat Monarch spellcasting DC is equal to your highest class or spell save DC.",
                new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            .WithAvailableAsArchetypeFeat(ModTraits.RatMonarch)
            .WithActionCost(2)
            .WithIllustration(Illustrations.BurrowingDeath)
            .WithOnSheet(sheet => {
                // sheet.SetProficiency(ModTraits.RatMonarch, (Proficiency)Math.Max((int)sheet.GetProficiency(sheet.Class?.ClassTrait ?? Trait.Spell), (int)sheet.GetProficiency(Trait.Spell)));
                sheet.AddFocusSpellAndFocusPoint(ModTraits.RatMonarch, sheet.FinalAbilityScores.KeyAbility, SpellLoader.BurrowingDeath);
            })
            .WithOnCreature((sheet, creature) => {
                creature.Proficiencies.Set(ModTraits.RatMonarch, (Proficiency)Math.Max((int)creature.Proficiencies.Get(sheet.Class?.ClassTrait ?? Trait.Spell), (int)creature.Proficiencies.Get(Trait.Spell)));
            })
            .WithRulesBlockForSpell(SpellLoader.BurrowingDeath);

            yield return new TrueFeat(Incubator, 6, "The corpses of your foes bulge and writhe, as fresh subjects burrow free of their carcass to serve you.",
                "After you or one of your familiars reduces an enemy to 0 HP, you summon a rat familiar in their place.",
                new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            .WithAvailableAsArchetypeFeat(ModTraits.RatMonarch)
            .WithPermanentQEffectAndSameRulesText((qfSelf) => {
                qfSelf.AfterYouDealDamage = async (you, _, target) => {
                    if (target.HP <= 0 && target.EnemyOf(you) && target.IsLivingCreature) {
                        you.Overhead("*incubator*", Color.White, $"A rat crawls out from {target.Name}'s corpse to serve {you.Name}.");
                        SpawnRatFamiliar(you, target.Occupies);
                    }
                };
            });

            yield return new TrueFeat(DireRats, 8, "Your rats grow larger and more aggressive.",
                "Your rat familiar's jaw attack deals an extra damage die, and they gain a +1 bonus to their strength, dexterity, constitution and wisdom modifiers.",
                new Trait[] { ModTraits.Event, ModTraits.Roguelike }, null)
            .WithPermanentQEffectAndSameRulesText(qf => { })
            .WithAvailableAsArchetypeFeat(ModTraits.RatMonarch);
        }

        internal static void SpawnRatFamiliar(Creature master, Tile? location = null) {
            Creature rat = GiantRat.CreateGiantRat();
            rat.MainName = master.MainName + "'s Rat Familiar";
            rat.Level = master.Level - 2;
            rat.MaxHP += (master.Level - 1) * 3;
            rat.Defenses.Set(Defense.AC, rat.Defenses.GetBaseValue(Defense.AC) + master.Level - 1);
            rat.Defenses.Set(Defense.Fortitude, rat.Defenses.GetBaseValue(Defense.Fortitude) + master.Level - 1);
            rat.Defenses.Set(Defense.Reflex, rat.Defenses.GetBaseValue(Defense.Reflex) + master.Level - 1);
            rat.Defenses.Set(Defense.Will, rat.Defenses.GetBaseValue(Defense.Will) + master.Level - 1);
            rat.Initiative += master.Level - 1;
            rat.ProficiencyLevel += master.Level - 1;
            rat.AddQEffect(new QEffect("Familiar", "This creature is permanantly slowed 1.") { Id = QEffectId.Slowed, Value = 1 });
            rat.AddQEffect(CommonQEffects.CantOpenDoors());
            rat.AddQEffect(new QEffect() {
                Id = QEffectIds.RatFamiliar,
                Source = master,
                StateCheck = self => {
                    if (self.Owner.HP == 0) {
                        self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                    }
                }
            });
            // Check for super rat feat here
            if (master.HasFeat(PlagueRats)) {
                rat.MaxHP += 5;
                rat.AddQEffect(CommonQEffects.RatPlagueAttack(master, "jaws"));
            }
            if (master.HasFeat(Incubator)) {
                rat.AddQEffect(new QEffect("Incubator", "On reducing an enemy to 0 HP, the rat familiar summons another rat familiar under its master's control.") {
                    AfterYouDealDamage = async (you, _, target) => {
                        if (target.HP <= 0 && target.EnemyOf(master) && target.IsLivingCreature) {
                            you.Overhead("*incubator*", Color.White, $"A rat crawls out from {target.Name}'s corpse to serve {master.Name}.");
                            SpawnRatFamiliar(master, target.Occupies);
                        }
                    }
                });
            }

            if (master.HasFeat(DireRats)) {
                rat.UnarmedStrike.WeaponProperties!.DamageDieCount = 2;
                rat.Abilities.Set(Ability.Dexterity, rat.Abilities.Dexterity + 1);
                rat.Abilities.Set(Ability.Strength, rat.Abilities.Strength + 1);
                rat.Abilities.Set(Ability.Constitution, rat.Abilities.Constitution + 1);
                rat.Abilities.Set(Ability.Wisdom, rat.Abilities.Wisdom + 1);
                rat.Defenses.Set(Defense.AC, rat.Defenses.GetBaseValue(Defense.AC) + 1);
                rat.Defenses.Set(Defense.Fortitude, rat.Defenses.GetBaseValue(Defense.Fortitude) + 1);
                rat.Defenses.Set(Defense.Reflex, rat.Defenses.GetBaseValue(Defense.Reflex) + 1);
                rat.Defenses.Set(Defense.Will, rat.Defenses.GetBaseValue(Defense.Will) + 1);

            }
            master.Battle.SpawnCreature(rat, master.Battle.GaiaFriends, location ?? master.Occupies);
        }
    }
}