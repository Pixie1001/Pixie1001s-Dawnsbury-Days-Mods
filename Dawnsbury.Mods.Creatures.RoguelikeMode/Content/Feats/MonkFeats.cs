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
    public static class MonkFeats {
        public static FeatName MonasticWeaponry { get; } = ModManager.RegisterFeatName("RL_MonasticWeaponry", "Monastic Weaponry");

        public static IEnumerable<Feat> CreateFeats() {

            yield return new TrueFeat(MonasticWeaponry, 1, "You have trained with the traditional weaponry of your monastery or school.",
                "You become trained in simple and martial monk weapons. When your proficiency rank for unarmed attacks increases to expert or master, your proficiency rank for these weapons increases to expert or master as well." +
                "\n\nYou can use melee monk weapons with any of your monk feats or monk abilities that normally require unarmed attacks, though not if the feat or ability requires you to use a single specific type of attack, such as Crane Stance." +
                "\n\n{b}Special{/b} If you have the Brawling Focus feat, the Critical Specialization effect also extends to your monk weapons.",
                [Trait.Monk, ModTraits.Roguelike], null)
            .WithOnSheet(sheet => {
                sheet.Proficiencies.AddProficiencyAdjustment((item) => item.Contains(Trait.MonkWeapon) && item.Contains(Trait.Martial), Trait.Simple);
            })
            .WithOnCreature(you => {
                if (you.HasFeat(FeatName.BrawlingFocus)) {
                    you.AddQEffect(new QEffect() {
                        YouHaveCriticalSpecialization = (self, item, action, defender) => item != null && item.HasTrait(Trait.MonkWeapon)
                    });
                }

                ReplaceFlurryOfBlows(you);
            })
            .WithOnSheet(sheet => {
                sheet.AtEndOfRecalculation = sheet => {
                    ReplaceKiStrike(sheet, sheet.Sheet.ToCreature(sheet.CurrentLevel));
                };
            });

            //yield return new TrueFeat(ModManager.RegisterFeatName("RL_MonasticArcherStance", "Monastic Archer Stance"), 1, "You enter a specialized stance for a unique martial art centered around the use of a bow.",
            //    "While in this stance, the only Strikes you can make are those using longbows, shortbows, or bows with the monk trait. You can use Flurry of Blows with these bows. " +
            //    "You can use your other monk feats or monk abilities that normally require unarmed attacks with these bows when attacking within half the first range increment (normally 50 feet for a longbow and 30 feet for a shortbow), " +
            //    "so long as the feat or ability doesn't require a single, specific Strike." +
            //    "\n\n{b}Special{/b} When you select this feat, you become trained in the longbow, shortbow, and any simple and martial bows with the monk trait. At later levels, your proficancy with these weapons scales with your unarmed attacks.",
            //    [Trait.Monk, Trait.Stance, ModTraits.Roguelike], null)
            //.WithIllustration(Illustrations.MonasticArcherStance)
            //.WithOnSheet(sheet => {
            //    sheet.Proficiencies.AddProficiencyAdjustment((item) => (item.Contains(Trait.MonkWeapon) && item.Contains(Trait.Martial) && item.Contains(Trait.Bow)) || item.ContainsOneOf([Trait.Longbow, Trait.Shortbow, Trait.CompositeLongbow, Trait.CompositeShortbow]), Trait.Simple);

            //    sheet.AtEndOfRecalculation = sheet => {
            //        ReplaceKiStrike(sheet, sheet.Sheet.ToCreature(sheet.CurrentLevel));
            //    };
            //})
            //.WithOnCreature(you => {
            //    if (!you.HasFeat(MonasticWeaponry))
            //        ReplaceFlurryOfBlows(you);
            //})
            //.WithPermanentQEffect(null, (qfMAS) => {
            //    qfMAS.ProvideMainAction = self => {
            //        return new ActionPossibility(new CombatAction(self.Owner, Illustrations.MonasticArcherStance, "Monastic Archer Stance", [Trait.Monk, Trait.Stance],
            //            "Enter a stance.\n\nWhile in this stance, you can use your monk feats or monk abilities that normally require unarmed attacks with longbows, shortsbows and monk bows instead.\n\nYou can't enter this stance if you're wearing armour.",
            //            Target.Self().WithAdditionalRestriction(self => {
            //                if (self.QEffects.Any(qf => qf.Id == QEffectIds.MonasticArcherStance))
            //                    return "You're already in this stance.";
            //                if (self.Armor.WearsArmor)
            //                    return "You're wearing armour.";
            //                return null;
            //            })) {
            //            ShortDescription = "You can use your monk feats or monk abilities that normally require unarmed attacks with longbows, shortsbows and monk bows instead."
            //        }
            //        .WithActionCost(1)
            //        .WithEffectOnSelf(user => {
            //            var stance = KineticistCommonEffects.EnterStance(user, Illustrations.MonasticArcherStance,
            //                "Monastic Archer Stance", "While in this stance, you can use your monk feats or monk abilities " +
            //                "that normally require unarmed attacks with longbows, shortsbows and monk bows instead at targets within half of your weapon's first range increment.", QEffectIds.MonasticArcherStance);
            //            stance.PreventTakingAction = action => action.HasTrait(Trait.Strike)
            //                && !((action.Item != null
            //                && action.Item.HasTrait(Trait.MonkWeapon)
            //                && action.Item.HasTrait(Trait.Bow)
            //                && !action.Item.HasTrait(Trait.Advanced)) || new Trait?[] { Trait.Longbow, Trait.Shortbow, Trait.CompositeLongbow, Trait.CompositeShortbow }.Contains(action?.Item?.MainTrait))
            //                ? "While in the monastic Archer Stance, the only Strikes you can make are those using longbows, shortbows, or bows with the monk trait." : null;

            //        })) {
            //            PossibilityGroup = Constants.POSSIBILITY_GROUP_STANCES
            //        }
            //        ;
            //    };
            //});

            yield return new TrueFeat(ModManager.RegisterFeatName("RL_ShootingStarStance", "Shooting Star Stance"), 2, "You enter a stance that lets you throw shuriken with lightning speed.",
                "While in this stance, you can use your monk feats or monk abilities that normally require unarmed attacks with shuriken instead.",
                [Trait.Monk, Trait.Stance, ModTraits.Roguelike], null)
            .WithIllustration(IllustrationName.SprayOfStars)
            .WithPermanentQEffect(null, (qfSSS) => {
                qfSSS.ProvideMainAction = self => {
                    return new ActionPossibility(new CombatAction(self.Owner, IllustrationName.SprayOfStars, "Shooting Star Stance", [Trait.Monk, Trait.Stance],
                        "Enter a stance.\n\nWhile in this stance, you can use your monk feats or monk abilities that normally require unarmed attacks with shuriken instead.\n\nUnlike most monk stances, you can enter this stance even if you're wearing armour.",
                        Target.Self().WithAdditionalRestriction(self => {
                            if (self.QEffects.Any(qf => qf.Name == "Shooting Star Stance"))
                                return "You're already in this stance.";
                            return null;
                        })) {
                        ShortDescription = "You can use your monk feats or monk abilities that normally require unarmed attacks with shuriken instead."
                    }
                    .WithActionCost(1)
                    .WithEffectOnSelf(user => {
                        var stance = KineticistCommonEffects.EnterStance(user, IllustrationName.SprayOfStars,
                            "Shooting Star Stance", "While in this stance, you can use your monk feats or monk abilities " +
                            "that normally require unarmed attacks with shuriken instead.", QEffectIds.ShootingStarStance); ;

                    })) {
                        PossibilityGroup = Constants.POSSIBILITY_GROUP_STANCES
                    }
                    ;
                };
            })
            .WithPrerequisite(sheet => sheet.HasFeat(MonasticWeaponry), "You must have the Monastic Weaponry feat.");

            yield return new TrueFeat(ModManager.RegisterFeatName("RL_PeafowlStance", "Peafowl Stance"), 4, "You enter a tall and proud stance while remaining mobile, with all the grace and composure of a peafowl.",
                "While in this stance, the only Strikes you can make are melee Strikes with the required sword. Once per round, after you hit with a monk sword Strike, you can Step as a free action as your next action.",
                [Trait.Monk, ModTraits.Roguelike], null)
            .WithOnCreature(you => {
                you.AddQEffect(new QEffect() {
                    ProvideMainAction = self => {
                        return new ActionPossibility(new CombatAction(self.Owner, IllustrationName.Bird256, "Peafowl Stance", [Trait.Monk, Trait.Stance],
                            "{b}Requirements{/b} You are wielding a sword that has the monk trait in one hand.\n\nEnter a stance.\n\nWhile in this stance, the only Strikes you can make are melee Strikes with the required sword. Once per round, after you hit with a monk sword Strike, you can Step as a free action as your next action.\n\nUnlike most monk stances, you can enter this stance even if you're wearing armour.",
                            Target.Self().WithAdditionalRestriction(self => {
                                if (self.QEffects.Any(qf => qf.Name == "Peafowl Stance"))
                                    return "You're already in this stance.";
                                else if (!self.HeldItems.Any(item => item.HasTrait(Trait.Sword) && item.HasTrait(Trait.MonkWeapon) && !item.HasTrait(Trait.TwoHanded))) return "You must be wielding a sword that has the monk trait in one hand.";
                                return null;
                            })) {
                            ShortDescription = "Enter a stance where the only Strikes you can make are melee Strikes with one handed monk swords. Once per round, after you hit with a monk sword Strike, you can Step as a free action as your next action."
                        }
                        .WithActionCost(1)
                        .WithEffectOnSelf(user => {
                            var stance = KineticistCommonEffects.EnterStance(user, IllustrationName.Bird256,
                                "Peafowl Stance", "While in this stance, the only Strikes you can make are melee Strikes with one handed monk swords. Once per round, after you hit with a monk sword Strike, you can Step as a free action as your next action.");
                            stance.AfterYouTakeActionAgainstTarget = async (_, action, target, result) => {
                                if (stance.Owner.QEffects.Any(qf => qf.Key == "Peafowl Stance Step Used")) return;
                                if (action.HasTrait(Trait.Strike) && action.Item != null && action.Item.HasTrait(Trait.Sword) && action.Item.HasTrait(Trait.MonkWeapon) && !action.Item.HasTrait(Trait.TwoHanded)) {
                                    // Add a temp QF to limit use until next round
                                    if (await stance.Owner.Battle.AskForConfirmation(stance.Owner, stance.Illustration!, "Would you like to use your one free step action per round provided by Peafowl Stance?", "Yes")) {
                                        await stance.Owner.StepAsync("Peafowl Stance", false, true);
                                        stance.Owner.AddQEffect(new QEffect() { Key = "Peafowl Stance Step Used" }.WithExpirationAtStartOfOwnerTurn());
                                    }
                                }
                            };
                            stance.PreventTakingAction = action => action.HasTrait(Trait.Strike) && (action.Item == null || !action.Item.HasTrait(Trait.Sword) || !action.Item.HasTrait(Trait.MonkWeapon) || action.Item.HasTrait(Trait.TwoHanded) || !action.HasTrait(Trait.Melee)) ?
                            "You can only strike with monk swords while in Peafowl Stance." : null;
                            stance.StateCheck = _ => {
                                if (!stance.Owner.HeldItems.Any(item => item.HasTrait(Trait.Sword) && item.HasTrait(Trait.MonkWeapon) && !item.HasTrait(Trait.TwoHanded)))
                                    stance.ExpiresAt = ExpirationCondition.Immediately;
                            };
                        })) {
                            PossibilityGroup = Constants.POSSIBILITY_GROUP_STANCES
                        };
                    }
                });
            })
            .WithIllustration(IllustrationName.Bird256)
            .WithPrerequisite(sheet => sheet.HasFeat(MonasticWeaponry), "You must have the Monastic Weaponry feat.");

            //yield return new TrueFeat(ModManager.RegisterFeatName("RL_AdvancedMonasticWeaponry", "Advanced Monastic Weaponry"), 6, "Your rigorous training regimen allows you to wield complex weaponry with ease.",
            //    "For the purposes of proficiency, you treat advanced monk weapons as if they were martial monk weapons.",
            //    [Trait.Monk, ModTraits.Roguelike], null)
            //.WithOnSheet(sheet => {
            //    sheet.Proficiencies.AddProficiencyAdjustment((item) => item.Contains(Trait.MonkWeapon) && item.Contains(Trait.Advanced), Trait.Simple);
            //})
            //.WithPrerequisite(sheet => sheet.HasFeat(MonasticWeaponry), "You must have the Monastic Weaponry feat.");
        }

        public static bool IsMonasticArcheryWeapon(Creature monk, Item weapon) {
            return (weapon.HasTrait(Trait.MonkWeapon) && weapon.HasTrait(Trait.Bow) && !weapon.HasTrait(Trait.Advanced)) || (new Trait[] { Trait.Longbow, Trait.Shortbow, Trait.CompositeLongbow, Trait.CompositeShortbow }.Contains(weapon.MainTrait));
        }

        internal static void ReplaceKiStrike(CalculatedCharacterSheetValues sheet, Creature monk) {

            if (monk.HasFeat(FeatName.KiStrike)) {
                var spellList = sheet.FocusSpells.GetOrCreate(Trait.Monk, () => new FocusSpells(Ability.Wisdom));
                if (spellList.Spells.Any(spell => spell.SpellId == SpellLoader.ModifiedKiStrike))
                    return;
                spellList.Spells.RemoveAll(spell => spell.SpellId == SpellId.KiStrike);
                spellList.Spells.Add(AllSpells.CreateModernSpell(SpellLoader.ModifiedKiStrike, null, sheet.MaximumSpellLevel, false, new SpellInformation() {
                    ClassOfOrigin = Trait.Monk
                }));
            }
        }

        internal static void ReplaceFlurryOfBlows(Creature monk) {
            monk.RemoveAllQEffects(qf => qf.ProvideMainAction != null && ((ActionPossibility)qf.ProvideMainAction(qf))?.CombatAction?.Name == "Flurry of Blows");

            if (monk.HasFeat(FeatName.OneInchPunch)) {
                monk.AddQEffect(new QEffect() {
                    ProvideStrikeModifierAsPossibility = item => {
                        if (!(item.HasTrait(Trait.MonkWeapon) && item.HasTrait(Trait.Melee)
                        || (IsMonasticArcheryWeapon(monk, item) && monk.HasEffect(QEffectIds.MonasticArcherStance)))) return null;

                        CombatAction CreateOneInchPunch(Item weapon, int actionCount) {
                            var strike = monk.CreateStrike(weapon, strikeModifiers: new StrikeModifiers() {
                                AdditionalWeaponDamageDice = actionCount == 2 ? 1 : 2
                            });
                            strike.Traits.Add(Trait.Basic);
                            strike.Name = "One-Inch Punch (" + actionCount + " actions)";
                            strike.Illustration = actionCount == 2 ? IllustrationName.TwoActions : IllustrationName.ThreeActions;
                            strike.ActionCost = actionCount;
                            (strike.Target as CreatureTarget)!.CreatureTargetingRequirements.RemoveAll(req => req is MaximumRangeCreatureTargetingRequirement);
                            (strike.Target as CreatureTarget)!.CreatureTargetingRequirements.Add(new MaximumRangeCreatureTargetingRequirement(strike.Item?.WeaponProperties?.RangeIncrement / (monk.HasEffect(QEffectId.FarShot) ? 1 : 2) ?? 1));
                            return strike;
                        }

                        return new SubmenuPossibility(new SideBySideIllustration(item.Illustration, IllustrationName.StarHit), "One-Inch Punch", PossibilitySize.Full) {
                            Subsections =
                            {
                            new PossibilitySection("One-Inch Punch")
                            {
                                Possibilities =
                                {
                                    new ActionPossibility(CreateOneInchPunch(item, 2)),
                                    new ActionPossibility(CreateOneInchPunch(item, 3))
                                }
                            }
                        }
                        };
                    },
                });
            }

            monk.AddQEffect(new QEffect() {
                ProvideMainAction = qfSelf => {
                    return (ActionPossibility)new CombatAction(qfSelf.Owner, IllustrationName.FlurryOfBlows,
                        "Flurry of Blows", [Trait.Monk, Trait.Flourish],
                        $"Make two unarmed {(qfSelf.Owner.HasFeat(MonasticWeaponry) ? " or monk weapon" : "")} strikes{(qfSelf.Owner.QEffects.Any(qf => qf.Id == QEffectIds.ShootingStarStance) ? " or two shuriken strike"
                                : qfSelf.Owner.QEffects.Any(qf => qf.Id == QEffectIds.MonasticArcherStance) ? " or two attacks with a bow at half range increment" : "")}.\n\nIf both hit the same creature, " +
                        "combine their damage for the purpose of resistances and weaknesses. Apply your multiple attack penalty to the Strikes normally." +
                        "\n\nAs it has the flourish trait, you can use Flurry of Blows only once per turn.",
                    Target.Self()
                    .WithAdditionalRestriction(user => {
                        if (user.QEffects.Any(qf => qf.Id == QEffectIds.ShootingStarStance)) {
                            Item? shuriken = null;

                            if (user.CarriedItems.Any(item => item.ItemName == CustomItems.Shuriken) || user.CarriedItems.FirstOrDefault(item => item.ItemName == CustomItems.ThrowersBandolier && item.IsWorn) != null) {
                                shuriken = Items.CreateNew(CustomItems.Shuriken);
                                CombatAction combatAction = StrikeRules.CreateStrike(user, shuriken, RangeKind.Ranged, -1, true);
                                var result = combatAction.CanBeginToUse(user);

                                if (user.HasFreeHand && shuriken != null && combatAction.CanBeginToUse(user).CanBeUsed)
                                    return null;
                            }
                        }

                        //if (user.QEffects.Any(qf => qf.Id == QEffectIds.MonasticArcherStance)) {
                        //    Item? bow = user.HeldItems.FirstOrDefault(wpn => (wpn.HasTrait(Trait.MonkWeapon) && wpn.HasTrait(Trait.Bow) && !wpn.HasTrait(Trait.Advanced)) || new Trait[] { Trait.Longbow, Trait.Shortbow, Trait.CompositeLongbow, Trait.CompositeShortbow }.Contains(wpn.MainTrait));
                        //    if (bow != null) {
                        //        CombatAction combatAction = StrikeRules.CreateStrike(user, bow, RangeKind.Ranged, -1, true);
                        //        if (combatAction.CanBeginToUse(user) && (bow.HasTrait(Trait.TwoHanded) || user.HasFreeHand)) {
                        //            return null;
                        //        }
                        //    }
                        //}

                        if (!user.CanMakeBasicUnarmedAttack && user.QEffects.All(qf => qf.AdditionalUnarmedStrike == null)) return "You must be able to make an unarmed Strike to use Flurry of Blows.";

                        if (user.Weapons.Any(weapon => Monk.CountsAsUnarmed(user, weapon) && CommonRulesConditions.CouldMakeStrike(user, weapon))) {
                            return null;
                        }

                        if ((!user.HeldItems.Any(wp => wp.HasTrait(Trait.MonkWeapon)))
                        || user.PrimaryWeapon == null
                        || user.QEffects.Any(qf => qf.PreventTakingAction != null && qf.PreventTakingAction(user.CreateStrike(user.PrimaryWeapon)) != null))
                            return $"You must be able to make a melee unarmed{(user.HasFeat(FeatName.MonasticArcherStance) ? " or monk weapon" : "")} strike{(user.QEffects.Any(qf => qf.Id == QEffectIds.ShootingStarStance) ? " or shuriken strike"
                                : user.QEffects.Any(qf => qf.Id == QEffectId.MonasticArcherStance) ? " or an attack with a bow at half its range increment" : "")} to use Flurry of Blows.";

                        if (user.MeleeWeapons.Any(weapon => (weapon.HasTrait(Trait.Unarmed) || weapon.HasTrait(Trait.MonkWeapon)) && CommonRulesConditions.CouldMakeStrike(user, weapon))) {
                            return null;
                        }

                        return "There is no nearby enemy or you can't make attacks.";
                    })) {
                        ShortDescription = $"Make two unarmed{(qfSelf.Owner.HasFeat(MonasticWeaponry) ? " or monk weapon" : "")} Strikes."
                    }
                    .WithActionCost(1)
                    .WithActionId(ActionId.FlurryOfBlows)
                    .WithEffectOnEachTarget(async (spell, self, target, irrelevantResult) => {
                        var chosenCreatures = new List<Creature>();
                        int hpBefore = -1;
                        for (int i = 0; i < 2; i++) {
                            await self.Battle.GameLoop.StateCheck();
                            var possibilities = new List<Option>();

                            if (self.QEffects.Any(qf => qf.Id == QEffectIds.ShootingStarStance)) {
                                // Add shurikens
                                Item? bandolier = self.CarriedItems.FirstOrDefault(item => item.ItemName == CustomItems.ThrowersBandolier && item.IsWorn);

                                if (bandolier != null || self.CarriedItems.Any(item => item.ItemName == CustomItems.Shuriken)) {
                                    var uniqueShurikens = new List<(Item?, Item)>();
                                    int numShurikens = 0;
                                    var allShurikens = new List<(Item?, Item)>();
                                    foreach (var item in self.CarriedItems) {
                                        if (item.ItemName == CustomItems.Shuriken)
                                            allShurikens.Add((null, item));
                                        foreach (var subItem in item.StoredItems) {
                                            if (subItem.ItemName == CustomItems.Shuriken)
                                                allShurikens.Add((item, subItem));
                                        }
                                    }
                                    allShurikens.ForEach(tuple => {
                                        numShurikens += 1;
                                        bool unique = true;
                                        foreach ((Item?, Item) skn in uniqueShurikens) {
                                            if (tuple.Item2.Name == skn.Item2.Name)
                                                unique = false;
                                        }
                                        if (unique)
                                            uniqueShurikens.Add(tuple);
                                    });

                                    List<CombatAction> shurikenThrows = new List<CombatAction>();
                                    foreach ((Item?, Item) shuriken in uniqueShurikens) {
                                        var strike = StrikeRules.CreateStrike(self, shuriken.Item2, RangeKind.Ranged, -1, true).WithActionCost(0);
                                        (strike.Target as CreatureTarget)?.WithAdditionalConditionOnTargetCreature((a, d) => a.HasFreeHand ? Usability.Usable : Usability.NotUsable("no-free-hand"));
                                        strike.WithPrologueEffectOnChosenTargetsBeforeRolls(async (action, user, targets) => {
                                            if (shuriken.Item1 != null)
                                                shuriken.Item1.StoredItems.Remove(shuriken.Item2);
                                            else
                                                user.CarriedItems.Remove(shuriken.Item2);
                                            user.AddHeldItem(shuriken.Item2);
                                        });
                                        strike.WithFullRename(strike.Name + " (" + shuriken.Item2.Name + ")");
                                        shurikenThrows.Add(strike);
                                    }

                                    if (bandolier != null) {
                                        var shuriken = Items.CreateNew(CustomItems.Shuriken);
                                        shuriken.Traits.Add(Trait.EncounterEphemeral);
                                        foreach (Item rune in bandolier.Runes) {
                                            if (rune.RuneProperties?.CanBeAppliedTo == null || rune.RuneProperties?.CanBeAppliedTo(rune, shuriken) == null)
                                                shuriken.WithModificationRune(rune.ItemName);
                                        }

                                        var strike = StrikeRules.CreateStrike(self, shuriken, RangeKind.Ranged, -1, true).WithActionCost(0);
                                        (strike.Target as CreatureTarget)?.WithAdditionalConditionOnTargetCreature((a, d) => a.HasFreeHand ? Usability.Usable : Usability.NotUsable("no-free-hand"));
                                        strike.WithPrologueEffectOnChosenTargetsBeforeRolls(async (action, user, targets) => {
                                            user.AddHeldItem(shuriken);
                                        });
                                        strike.WithFullRename(strike.Name + " from bandolier (" + shuriken.Name + ")");
                                        shurikenThrows.Add(strike);
                                    }

                                    foreach (var strike in shurikenThrows) {
                                        GameLoop.AddDirectUsageOnCreatureOptions(strike, possibilities, false);
                                    }
                                }
                            } else if (self.QEffects.Any(qf => qf.Id == QEffectIds.MonasticArcherStance)) {
                                Item? bow = self.HeldItems.FirstOrDefault(wpn => (wpn.HasTrait(Trait.MonkWeapon) && wpn.HasTrait(Trait.Bow) && !wpn.HasTrait(Trait.Advanced)) || new Trait[] { Trait.Longbow, Trait.Shortbow, Trait.CompositeLongbow, Trait.CompositeShortbow }.Contains(wpn.MainTrait));
                                if (bow != null) {
                                    var combatAction = self.CreateStrike(bow);
                                    (combatAction.Target as CreatureTarget)?.CreatureTargetingRequirements.Add(new MaximumRangeCreatureTargetingRequirement(bow.WeaponProperties?.RangeIncrement / (monk.HasEffect(QEffectId.FarShot) ? 1 : 2) ?? 1));
                                    combatAction.WithActionCost(0);
                                    GameLoop.AddDirectUsageOnCreatureOptions(combatAction, possibilities, true);
                                }
                            }

                            foreach (var item in self.Weapons.Where(weapon => Monk.CountsAsUnarmed(self, weapon))) {
                                var combatAction = self.CreateStrike(item);
                                combatAction.WithActionCost(0);
                                GameLoop.AddDirectUsageOnCreatureOptions(combatAction, possibilities, true);
                            }

                            if (self.HasFeat(MonasticWeaponry)) {
                                foreach (var item in self.Weapons.Where(weapon => weapon.HasTrait(Trait.MonkWeapon) && weapon.HasTrait(Trait.Melee))) {
                                    var combatAction = self.CreateStrike(item);
                                    combatAction.WithActionCost(0);
                                    GameLoop.AddDirectUsageOnCreatureOptions(combatAction, possibilities, true);
                                }
                            }
                            

                            if (self.HasEffect(QEffectId.FlurryOfManeuvers)) {
                                foreach (var maneuverAction in CombatManeuverPossibilities.GetAllShoveGrappleAndTripOptions(self)) {
                                    GameLoop.AddDirectUsageOnCreatureOptions(maneuverAction.WithActionCost(0), possibilities, true);
                                }
                            }

                            if (i == 0 && possibilities.Count == 0) {
                                spell.RevertRequested = true;
                                return;
                            }

                            if (possibilities.Count > 0) {
                                Option chosenOption;
                                if (possibilities.Count >= 2 || i == 0) {
                                    if (i == 0) possibilities.Add(new CancelOption(true));
                                    var result = await self.Battle.SendRequest(new AdvancedRequest(self, "Choose a creature to Strike.", possibilities) {
                                        TopBarText = (i == 0 ? "Choose a creature to Strike or right-click to cancel. (1/2)" : "Choose a creature to Strike. (2/2)"),
                                        TopBarIcon = IllustrationName.Fist
                                    });
                                    chosenOption = result.ChosenOption;
                                } else {
                                    chosenOption = possibilities[0];
                                }

                                if (chosenOption is CreatureOption creatureOption) {
                                    if (hpBefore == -1) {
                                        hpBefore = creatureOption.Creature.HP;
                                    }

                                    chosenCreatures.Add(creatureOption.Creature);
                                }

                                if (chosenOption is CancelOption) {
                                    spell.RevertRequested = true;
                                    return;
                                }

                                await chosenOption.Action();
                            }
                        }

                        if (self.HasEffect(QEffectId.StunningFist) && (chosenCreatures.Count == 1 || chosenCreatures.Count == 2 && chosenCreatures[0] == chosenCreatures[1])) {
                            if (chosenCreatures[0].HP < hpBefore) {
                                var stunningFistAction = CombatAction.CreateSimple(self, "Stunning Fist", Trait.Incapacitation);
                                var stunningFistResult = await CommonSpellEffects.RollSavingThrowAsync(chosenCreatures[0], stunningFistAction, Defense.Fortitude, self.Proficiencies.Get(Trait.Monk).ToNumber(self.ProficiencyLevel) + self.Abilities.Get(self.Abilities.KeyAbility) + 10);
                                if (stunningFistResult <= CheckResult.Failure) {
                                    chosenCreatures[0].AddQEffect(QEffect.Stunned(stunningFistResult == CheckResult.CriticalFailure ? 3 : 1));
                                }
                            }
                        }

                        Steam.CollectAchievement("MONK");
                    });
                }
            });
        }
    }
}