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
    public static class DrowHeritage {

        public static FeatName Drow = ModManager.RegisterFeatName("RL_Drow_ElfHeritage", "Drow");
        public static FeatName GreaterDrowResilience = ModManager.RegisterFeatName("RL_Greater Drow Resilience", "Greater Drow Resilience");
        public static FeatName DrowWeaponFamiliarity = ModManager.RegisterFeatName("RL_DrowWeaponFamiliarity", "Drow Weapon Familiarity");
        public static FeatName DrowTerrorTactics = ModManager.RegisterFeatName("RL_DrowTerrorTactics", "Drow Terror Tactics");
        public static FeatName ChildOfTheSpider = ModManager.RegisterFeatName("RL_ChildOfTheSpider", "Child of the Spider");
        public static FeatName DrowMagic = ModManager.RegisterFeatName("RL_DrowMagic", "Drow Magic");
        public static FeatName SpiderAffinity = ModManager.RegisterFeatName("RL_SpiderAffinity", "Spider Affinity");
        public static FeatName DrowLethargyPoisoner = ModManager.RegisterFeatName("RL_LegargyPoisoner", "Drow Lethargy Poisoner");

        public static IEnumerable<Feat> CreateFeats() {
            List<Trait> classTraits = new List<Trait>();
            AllFeats.All.ForEach(ft => {
                if (ft is ClassSelectionFeat classFeat) {
                    classTraits.Add(classFeat.ClassTrait);
                }
            });

            var drow = new HeritageSelectionFeat(Drow, "Said to be the ancestors of the Demon Queen of Spider's subjects before her apotheosis to the ranks of the Starborn, that followed her down on her pilgrimage to the other side. " +
                "Exactly what happened to them is lost to time, but reports soon followed of sinister elves with striking lavander skin and violet eyes emerging from the Below to raid and pillage.\n\n" +
                "Though may drow still worship the Demon Queen of Spiders, it is not unheard of to encounter defectors who have cast off the cruel ways of their people to life a more fulfilling life, or otherwise oppose the Starborn in the hopes of seizing power within drow high society for themselves.",
                "You gain a +1 status bonus to saves against mental effects and cantrips.", [ModTraits.Drow])
            .WithOnCreature(creature => {
                creature.AddQEffect(new QEffect(creature.HasFeat(GreaterDrowResilience) ? "Greater " : "" + "Drow Resilience", creature.HasFeat(GreaterDrowResilience) ? "+2 status bonus to vs. mental saves; +1 status bonus vs. spells" : "+1 status bonus vs. mental saves; +1 status bonus vs. cantrips") {
                    BonusToDefenses = (self, action, defence) => {
                        if (action == null) {
                            return null;
                        }

                        if (action.HasTrait(Trait.Mental) && !(action.SpellId != SpellId.None && action.Owner.HeldItems.Any(item => item.ItemName == CustomItems.StaffOfSpellPenetration)) && defence != Defense.AC) {
                            return new Bonus(creature.HasFeat(GreaterDrowResilience) ? 2 : 1, BonusType.Status, self.Name!);
                        }

                        if ((action.HasTrait(Trait.Cantrip) || (creature.HasFeat(GreaterDrowResilience) && action.HasTrait(Trait.Spell))) && !action.Owner.HasEffect(QEffectId.SpellPenetration) && defence != Defense.AC) {
                            return new Bonus(1, BonusType.Status, self.Name!);
                        }

                        return null;
                    }
                });
            });
            AllFeats.All.Find(ft => ft.FeatName == FeatName.Elf)?.Subfeats?.Add(drow);
            yield return drow;

            yield return new TrueFeat(DrowWeaponFamiliarity, 1, "You're trained in the archetypal weaponry of the cruel and secretive drow.", "You are trained with repeating hand crossbows, rapiers and whips.\n\nIn addition, for the purposes of determining your proficiency, you treat whips and rapiers as simple weapons and repeating hand crossbows as martial weapons.", [ModTraits.Drow, Trait.Elf])
            .WithOnSheet(sheet => {
                sheet.Proficiencies.Set(Trait.RepeatingHandCrossbow, Proficiency.Trained);
                sheet.Proficiencies.Set(Trait.Whip, Proficiency.Trained);
                sheet.Proficiencies.Set(Trait.Rapier, Proficiency.Trained);

                Trait[] drowWeapons = [Trait.RepeatingHandCrossbow, Trait.Rapier, Trait.Whip];

                sheet.Proficiencies.AddProficiencyAdjustment((item) => item.ContainsOneOf(drowWeapons) && item.Contains(Trait.Martial), Trait.Simple);
                sheet.Proficiencies.AddProficiencyAdjustment((item) => item.ContainsOneOf(drowWeapons) && item.Contains(Trait.Advanced), Trait.Martial);
            })
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.");

            yield return new TrueFeat(DrowTerrorTactics, 1, "Drow are taught from a young age that fear can be as lethal a weapon as any blade.", "You gain a +1 circumstance bonus to Demoralize enemies you have damaged during your turn.", [ModTraits.Drow, Trait.Elf])
            .WithPermanentQEffectAndSameRulesText(qfSelf => {
                qfSelf.AfterYouDealDamage = async (you, action, target) => {
                    if (target.Occupies == null || action == null) return;

                    target.AddQEffect(new QEffect() {
                        Id = QEffectIds.DrowTerrorTactics,
                        Source = you,
                        ExpiresAt = ExpirationCondition.ExpiresAtEndOfSourcesTurn
                    });
                };
                qfSelf.BonusToAttackRolls = (self, action, target) => target != null && action.ActionId == ActionId.Demoralize && target.FindQEffect(QEffectIds.DrowTerrorTactics)?.Source == self.Owner ? new Bonus(1, BonusType.Circumstance, "Drow terror tactics", true) : null;
            })
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.")
            .WithPrerequisite(values => values.Proficiencies.Get(Trait.Intimidation) >= Proficiency.Trained, "You must be trained in intimidation.");

            yield return new TrueFeat(ChildOfTheSpider, 5, "Since the Demon Queen of Spiders lead the drow down into the sunless depths of the Below, they've always had a deep connection with spiders.", $"You gain the ability to walk through webs unimpeded, and can cast {AllSpells.CreateModernSpellTemplate(SpellId.Web, Trait.Elf, 2).ToSpellLink()} as a 2nd level innate spell once per day. In addition, you can intimidate spiders even if you don't share a language.", [ModTraits.Drow, Trait.Elf])
            .WithOnSheet(sheet => { sheet.SetProficiency(Trait.Spell, Proficiency.Trained); })
            .WithOnCreature(creature => {
                creature.AddQEffect(new QEffect("Child of the Spider", "You can move through webs unimpeded and ignore the penalty to demoralize spiders that do not speak common.") {
                    BonusToSkillChecks = (skill, action, target) =>
                        target != null && action.ActionId == ActionId.Demoralize && !action.Owner.HasFeat(FeatName.IntimidatingGlare) && target.HasTrait(ModTraits.Spider) && target.DoesNotSpeakCommon ? new Bonus(4, BonusType.Circumstance, "Spider affinity") : null,
                    Id = QEffectId.IgnoresWeb
                });
                var source = creature.GetOrCreateSpellcastingSource(SpellcastingKind.Innate, Trait.Elf, Ability.Charisma, Trait.Divine);
                source.WithSpells([SpellId.Web], 2);
            })
            .WithRulesBlockForSpell(SpellId.Web, Trait.Elf)
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.");

            yield return new TrueFeat(DrowMagic, 5, "The demonic magic running through your veins grants you access to the ability to both dispel and invoke duplicity.",
                $"You may cast both the {AllSpells.CreateModernSpellTemplate(SpellId.FaerieFire, Trait.Elf, 2).ToSpellLink()} and {AllSpells.CreateModernSpellTemplate(SpellId.ObscuringMist, Trait.Elf, 2).ToSpellLink()} spells once per day as innate 2nd level spells.", [ModTraits.Drow, Trait.Elf])
            .WithOnSheet(sheet => { sheet.SetProficiency(Trait.Spell, Proficiency.Trained); })
            .WithOnCreature(creature => {
                var source = creature.GetOrCreateSpellcastingSource(SpellcastingKind.Innate, Trait.Elf, Ability.Charisma, Trait.Divine);
                source.WithSpells([SpellId.FaerieFire], 2);
                source.WithSpells([SpellId.ObscuringMist], 2);
            })
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.");

            yield return new TrueFeat(GreaterDrowResilience, 5, "Your natural resilience from magic and trickery grows stronger.",
                $"Your drow resilience now protects you against all spells, not just cantrips, and you gain a +2 status bonus against mental effects instead of a +1.", [ModTraits.Drow, Trait.Elf])
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.");

            yield return new TrueFeat(SpiderAffinity, 9, "Spiders yield to the dirge of demonic power in your blood, coming to your defence when called upon for aid.", "Once per day, you may cast {i}summon spider{/i} as an innate spell, heightened to the highest slot level.", [ModTraits.Drow, Trait.Elf])
            .WithOnCreature(creature => {
                var source = creature.GetOrCreateSpellcastingSource(SpellcastingKind.Innate, Trait.Elf, Ability.Charisma, Trait.Divine);
                source.WithSpells([SpellLoader.SummonSpider], (creature.Level + 1) / 2);
            })
            .WithRulesBlockForSpell(SpellLoader.SummonSpider, Trait.Elf)
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.");

            yield return new TrueFeat(DrowLethargyPoisoner, 9, "Lethargy poison is commonly used in hit-and-run tactics by drow; the ambusher retreats until the poison sets in and the victim falls unconscious.", "Once per day, you may coat a weapon you are holding with Drow Lethargy Poison", [ModTraits.Drow, Trait.Elf])
            .WithOnCreature(creature => {
                creature.AddQEffect(new QEffect() {
                    StartOfCombat = async self => {
                        if (self.Owner.PersistentUsedUpResources.UsedUpActions.Contains("DrowLethargyPoisoner"))
                            self.Name += " (expended)";
                    },
                    ProvideMainAction = self => {

                        if (self.Owner.PersistentUsedUpResources.UsedUpActions.Contains("DrowLethargyPoisoner")) return null;

                        bool IsValidTargetForPoison(Item wp) {
                            return wp.HasTrait(Trait.Weapon) && wp.WeaponProperties != null && !wp.Runes.Any(rune => rune.RuneProperties?.RuneKind == RuneKind.WeaponPoison);
                        }

                        return new ActionPossibility(new CombatAction(self.Owner, IllustrationName.AlchemicalPoison, "Apply Lethargy Poison", [Trait.Manipulate, Trait.Concentrate],
                            "{b}Frequency{/b} Once per day\n\nUsing a free hand, apply drow lethargy poison to a weapon you're carrying.\n\n" +
                            $"{{b}}Drow Lethargy Poison (DC {SkillChallengeTables.GetDCByLevel(self.Owner.Level)}){{/b}}\n" +
                            "{b}Stage 1{/b} slowed 1; {b}Stage 2{/b} slowed 1 for rest of encounter.",
                            Target.Self().WithAdditionalRestriction(user => !user.HasFreeHand ? "no-free-hand" : !user.HeldItems.Any(wp => IsValidTargetForPoison(wp)) ? "no-poisonable-weapons" : null))
                            .WithActionCost(1)
                            .WithSoundEffect(SfxName.ItemGet)
                            .WithEffectOnSelf(async (action, user) => {
                                var dc = SkillChallengeTables.GetDCByLevel(self.Owner.Level);

                                var poison = new Item(IllustrationName.AlchemicalPoison, "drow lethargy poison", [Trait.Alchemical, Trait.Consumable, Trait.Poison]) {
                                    AlchemicalDC = dc
                                }
                                .WithRuneProperties(new RuneProperties(
                                    "poisoned",
                                    RuneKind.WeaponPoison,
                                    null,
                                    $"On a hit, the target contracts drow lethargy poison (DC {dc} Fortitude save negates).",
                                    (rune, item) => {
                                        item.Traits.Add(Trait.Poisoned);
                                        item.WeaponProperties!.AdditionalSuccessDescription += $" {{Blue}}On a hit, the target contracts drow lethargy poison (DC {dc} Fortitude save negates).{{/}}";
                                        item.WeaponProperties!.AdditionalPoisonOnTarget = async (spell, caster, target, result) => {
                                            if (result >= CheckResult.Success) {
                                                var affliction = new Affliction(QEffectIds.LethargyPoison, "Drow Lethargy Poison", dc, "{b}Stage 1{/b} slowed 1; {b}Stage 2{/b} slowed 1 for rest of encounter", 2, dmg => null, qf => {
                                                    if (qf.Value == 1) {
                                                        qf.Owner.AddQEffect(QEffect.Slowed(1).WithExpirationEphemeral());
                                                    }

                                                    if (qf.Value == 2) {
                                                        QEffect nEffect = QEffect.Slowed(1).WithExpirationNever();
                                                        nEffect.CounteractLevel = qf.CounteractLevel;
                                                        qf.Owner.AddQEffect(nEffect);
                                                        qf.Owner.RemoveAllQEffects(qf2 => qf2.Id == QEffectIds.LethargyPoison);
                                                        qf.Owner.Overhead("*lethargy poison converted to slowed 1*", Color.Black);
                                                    }
                                                });

                                                await Affliction.ExposeToInjury(affliction, caster, target);

                                            }

                                            if (result != CheckResult.Failure) {
                                                AlchemicalItems.DestroyAllPoisonsOn(item);
                                            }
                                        };
                                    }));

                                var weapons = user.HeldItems.Where(wp => IsValidTargetForPoison(wp));
                                if (weapons.Count() == 0) {
                                    action.RevertRequested = true;
                                    return;
                                }
                                ;
                                Item weapon = weapons.Count() == 1
                                ? user.HeldItems.First(IsValidTargetForPoison)
                                : (await user.Battle.AskForConfirmation(user, action.Illustration, "Which weapon would you like to poison?",
                                    user.HeldItems[0].Name, user.HeldItems[1].Name))
                                    ? user.HeldItems[0]
                                    : user.HeldItems[1];

                                RunestoneRules.AddRuneTo(poison, weapon);

                                self.Owner.PersistentUsedUpResources.UsedUpActions.Add("DrowLethargyPoisoner");
                                self.Name += " (expended)";
                            })
                            ).WithPossibilityGroup(Constants.POSSIBILITY_GROUP_ANCESTRY_POWERS);
                    },
                });
            })
            .WithPrerequisite(values => values.HasFeat(Drow), "You must be a Drow elf.");
        }
    }
}