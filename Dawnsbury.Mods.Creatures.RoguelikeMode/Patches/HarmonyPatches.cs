using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dawnsbury.Core;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Campaign.Path.CampaignStops;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters;
using Dawnsbury.Phases.Menus;
using Dawnsbury.Phases.Menus.StoryMode;
using HarmonyLib;
using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Rules;
using System.Data;
using System.Runtime.CompilerServices;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level1;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level3;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using static Dawnsbury.Core.Mechanics.Rules.RunestoneRules;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Phases.Ingame;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Dawnsbury.Display;
using Dawnsbury.Audio;
using Dawnsbury.Phases.Popups;
using System.Reflection;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Tiles;
using Dawnsbury.IO;
using Dawnsbury.Phases.Menus.CampaignViews;
using System.IO;
using Dawnsbury.Display.Controls.Listbox;
using Dawnsbury.Display.Controls;
using Dawnsbury.Core.Coroutines.Options;
using static System.Net.Mime.MediaTypeNames;
using Dawnsbury.Campaign.LongTerm;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Display.Illustrations;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Patches
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    [HarmonyLib.HarmonyPatch]
    public class HarmonyPatches {



        // TODO: Create a patch for Shop.cs -> CreateAdventurersDawnForTrueShopping that will check if the town is a custom type, and then replace the shop keeper portrait, and possibly add unique items to the store like wands and anti-drow rings

        // internal static void GenerateTemplatesAndFactoriesFor(ItemName[] itemNames)

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(Items), "GenerateTemplatesAndFactoriesFor")]
        //private static void CanUsePatch(ItemName[] itemNames) {

        //    if (item != null && item.HasTrait(Traits.Wand)) {
        //        foreach (Trait trait in __instance.Calculated.SpellTraditionsKnown) {
        //            if (item.HasTrait(trait))
        //                __result = true;
        //        }
        //    }
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(CharacterSheet), "ToCreature")]
        //private static void ToCreaturePatch(CharacterSheet __instance, ref Creature __result, int level) {
        //    if (LTEs.PartyBoons.TryGetValue(__instance, out List<QEffect> boons)) {
        //        LTEs.ApplyBoons(__result);
        //    }

        //    //__instance.Calculated.Tags.TryGetValue("KipUp", out object? feat);
        //    //if (feat != null) {
        //    //    __instance.Calculated.GrantFeat((FeatName)feat);
        //    //}

        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(BattlePhase), "Draw")]
        //private static void BattlePhaseDrawPatch(BattlePhase __instance, SpriteBatch sb, Game game, float elapsedSeconds) {
        //    if (__instance.Battle.CampaignState == null || __instance.Battle.CampaignState.AdventurePath.Id != "RoguelikeMode") {
        //        return;
        //    }

        //    if (__instance.Battle.Victory.HasValue && __instance.Battle.Victory.Value == false) {
        //        if (Int32.TryParse(__instance.Battle.CampaignState.Tags["deaths"], out int deaths)) {
        //            deaths++;
        //            __instance.Battle.CampaignState.Tags["deaths"] = $"{deaths}";
        //        }
        //    }
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(TBattle), "SpawnCreature", [typeof(Creature), typeof(Faction), typeof(Tile)])]
        //private static void ArchanophobiaPatch(TBattle __instance, Creature creature, Faction controller, Tile where) {
        //    if (PlayerProfile.Instance.IsBooleanOptionEnabled("RL_ArachnophobiaMode")) {
        //        UtilityFunctions.ReplaceSpiderSprite(creature);
        //    }
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(ForcedActions), "ExecuteForcedActions")]
        //private static async void ExecuteForcedActionsPatch(Creature selectedCreature, List<Option> options) {
        //    var confused = new QEffect() { Id = QEffectId.Controlled }.WithExpirationNever();
        //    if (selectedCreature.HasEffect(QEffectIds.LesserConfused) && selectedCreature.Actions.ActionHistoryThisTurn.Count() == 0) {
        //        //selectedCreature.Actions.UseUpActions(1, ActionDisplayStyle.UsedUp, CombatAction.DefaultCombatAction);
        //        selectedCreature.AddQEffect(confused);
        //        Item melee = selectedCreature.PrimaryWeapon;
        //        Item ranged = selectedCreature.PrimaryWeaponIncludingRanged;

        //        if (ranged == null) {
        //            goto CannotStrike;
        //        }
        //        CombatAction strike = null;
        //        if (!ranged.WeaponProperties.Melee) {
        //            strike = selectedCreature.CreateStrike(ranged);
        //        } else {
        //            strike = selectedCreature.CreateStrike(melee);
        //        }
        //        var target = selectedCreature.Battle.AllCreatures.Where(cr => (strike.Target as CreatureTarget).IsLegalTarget(selectedCreature, cr)).ToList().GetRandom();

        //        if (target == null)
        //            goto CannotStrike;
        //        else
        //            await selectedCreature.MakeStrike(target, strike.Item);

        //        selectedCreature.RemoveAllQEffects(qf => qf == confused);
        //    }

        //    return;

        //    CannotStrike:
        //        selectedCreature.RegeneratePossibilities();
        //        Option? chosenOption = options.Where(opt => opt.AiUsefulness.ObjectiveAction?.Action.ActionCost == 1).ToList().GetRandom();
        //        if (chosenOption == null)
        //            selectedCreature.Actions.UseUpActions(1, ActionDisplayStyle.UsedUp, CombatAction.DefaultCombatAction);
        //        else
        //            chosenOption.Action();
        //        selectedCreature.RemoveAllQEffects(qf => qf == confused);
        //        return;
        //}

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AI), "ModifyGoodness")]
        private static void BattlePhaseDrawPatch(ref float __result, Target target, Creature attacker, Creature defender, float howMuchDamageEquivalent) {
            if (defender.HasEffect(QEffectIds.RatFamiliar))
                __result = Math.Min(1.5f, __result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TBattle), "EndTheGame")]
        private static void BattlePhaseDrawPatch(TBattle __instance, bool victory, string reason) {
            if (__instance.CampaignState == null || __instance.CampaignState?.AdventurePath?.Id != "RoguelikeMode") {
                return;
            }

            (__instance.CampaignState?.AdventurePath?.CampaignStops?.Last() as DawnsburyStop)!.CustomText = "{b}Congratulations!{/b} You survived the Below and saved Dawnsbury from the Machinations of the Spider Queen! But it won't be long before she tries again, and another brave group of adventurers will need to once again brave the Below...\n\n" +
            "{b}Stats{/b}\n" +
            "{b}Deaths:{/b} " + __instance.CampaignState.Tags["deaths"] + "\n" +
            "{b}Restarts:{/b} " + __instance.CampaignState.Tags["restarts"] + "\n" +
            "{b}Corruption Level:{/b} " + __instance.CampaignState.Tags["corruption level"] +
            "\n\n" + Loader.Credits;

            if (!victory) {
                if (Int32.TryParse(__instance.CampaignState.Tags["deaths"], out int deaths)) {
                    ++deaths;
                    __instance.CampaignState.Tags["deaths"] = $"{deaths}";

                    CampaignState.Autosave();
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(IngameMenuPhase), "Draw")]
        private static void IngameMenuPhaseDrawPatch(IngameMenuPhase __instance, SpriteBatch sb, Game game, float elapsedSeconds) {
            TBattle? battle = (TBattle?)__instance.GetType().GetField("battleInProgress", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)?.GetValue(__instance)!;
            bool fromCampaignScreen = (bool)__instance.GetType().GetField("fromCampaignScreen", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)?.GetValue(__instance)!;

            if (battle == null) {
                return;
            }

            if (battle.CampaignState == null || battle?.CampaignState?.AdventurePath?.Id != "RoguelikeMode") {
                return;
            }

            int num1 = 90;
            int num2 = 20;
            int height = 80;

            if (!fromCampaignScreen && battle != null) {
                // Draw over restart button
                Type.GetType("Dawnsbury.Display.UI, Dawnsbury Days")!
                    .GetMethod("DrawUIButton", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, new[] {
                        typeof(Rectangle),
                        typeof(string),
                        typeof(Action),
                        typeof(Writer.TextAlignment),
                        typeof(BitmapFontGroup),
                        typeof(string),
                        typeof(Color),
                    })?
                    .Invoke(null, new object[] { new Rectangle(__instance.Window.X + 10, __instance.Window.Y + 10 + num2 + num1 * 2, __instance.Window.Width - 20, height), "Restart encounter (Recorded))", () => {
                        Sfxs.Play(SfxName.Button);
                        Root.PushPhase((GamePhase)new ConfirmationDialogPhase("Restart encounter? The number of times you restarted will be recorded at the end of the run.", "Restart", "No", (Action)(() => {
                            ImprovedStack<GamePhase> phaseStack3 = Root.PhaseStack;
                            phaseStack3[phaseStack3.Count - 3].Destruct((Game)Root.Game);
                            ImprovedStack<GamePhase> phaseStack4 = Root.PhaseStack;
                            phaseStack4[phaseStack4.Count - 2].Destruct((Game)Root.Game);
                            Root.PopFromPhase();
                            Sfxs.Silence();
                            Root.PushPhase((GamePhase)new BattlePhase(new TBattle(battle.RestartedEncounterGenerator, battle.CampaignState, battle.Heroes)));
                            if (Int32.TryParse(battle.CampaignState.Tags["restarts"], out int restarts)) {
                                restarts++;
                                battle.CampaignState.Tags["restarts"] = $"{restarts}";
                                CampaignState.Autosave();
                            }
                        })));
                    },
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing });
                //Writer.TextAlignment.Middle, null, null, Color.DarkSlateGray });


                // Draw over return to menu button
                Type.GetType("Dawnsbury.Display.UI, Dawnsbury Days")!
                    .GetMethod("DrawUIButton", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, new[] {
                        typeof(Rectangle),
                        typeof(string),
                        typeof(Action),
                        typeof(Writer.TextAlignment),
                        typeof(BitmapFontGroup),
                        typeof(string),
                        typeof(Color),
                    })?.Invoke(null, new object[] {
                        new Rectangle(__instance.Window.X + 10, __instance.Window.Y + 10 + num2 * 3 + num1 * 5, __instance.Window.Width - 20, height), "Exit to menu (Recorded)", () => {
                        Sfxs.Play(SfxName.Button);
                        if (fromCampaignScreen) {
                            Sfxs.StopVoice();
                            Root.PopFromPhase();
                            ImprovedStack<GamePhase> phaseStack = Root.PhaseStack;
                            phaseStack[phaseStack.Count - 2].Destruct((Game)Root.Game);
                        } else
                            Root.PushPhase(new ConfirmationDialogPhase("Abandon encounter, and return to " + (Root.PhaseStack.Any<GamePhase>(ph => ph is RandomEncounterModePhase) ? "encounter selection" : "campaign") + " screen?", "Yes", "No", () => {
                                Sfxs.Silence();
                                if (Int32.TryParse(battle.CampaignState.Tags["restarts"], out int restarts)) {
                                    restarts++;
                                    battle.CampaignState.Tags["restarts"] = $"{restarts}";
                                }
                                ImprovedStack<GamePhase> phaseStack8 = Root.PhaseStack;
                                int song;
                                switch (phaseStack8[phaseStack8.Count - 4]) {
                                    case CampaignMenuPhase campaignMenuPhase2:
                                        campaignMenuPhase2.RefreshAfterBattle();
                                        Sfxs.BeginSong(campaignMenuPhase2.Songname);
                                        CampaignState.Autosave();
                                        goto label_5;
                                    case RandomEncounterModePhase _:
                                        song = 2;
                                        break;
                                    default:
                                        song = 1;
                                        break;
                                }
                                Sfxs.BeginSong((Songname)song);
                            label_5:
                                ImprovedStack<GamePhase> phaseStack9 = Root.PhaseStack;
                                phaseStack9[phaseStack9.Count - 2].Destruct((Game)Root.Game);
                                ImprovedStack<GamePhase> phaseStack10 = Root.PhaseStack;
                                phaseStack10[phaseStack10.Count - 3].Destruct((Game)Root.Game);
                            }));
                        },
                        Type.Missing, Type.Missing, Type.Missing, Type.Missing });
            }
        }

        //      if (!this.fromCampaignScreen && this.battleInProgress != null)
        //UI.DrawUIButton(new Rectangle(this.Window.X + 10, this.Window.Y + 10 + num2 + num1* 2, this.Window.Width - 20, height), "Restart encounter", (Action) (() =>
        //{
        //  Sfxs.Play(SfxName.Button);
        //  Root.PushPhase((GamePhase) new ConfirmationDialogPhase("Restart encounter?", "Restart", "No", (Action) (() =>
        //  {
        //    ImprovedStack<GamePhase> phaseStack3 = Root.PhaseStack;
        //    phaseStack3[phaseStack3.Count - 3].Destruct((Game)Root.Game);
        //    ImprovedStack<GamePhase> phaseStack4 = Root.PhaseStack;
        //    phaseStack4[phaseStack4.Count - 2].Destruct((Game)Root.Game);
        //    Root.PopFromPhase();
        //    Sfxs.Silence();
        //    Root.PushPhase((GamePhase)new BattlePhase(new TBattle(this.battleInProgress.RestartedEncounterGenerator, this.battleInProgress.CampaignState, this.battleInProgress.Heroes)));
        //})));
        //}));

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CampaignState), MethodType.Constructor, new Type[] { typeof(List<CharacterSheet>), typeof(AdventurePath) })]
        private static void CampaignStatePatch(CampaignState __instance, List<CharacterSheet> heroes, AdventurePath adventurePath) {
            if (__instance.AdventurePath != null && __instance.AdventurePath.Id == "RoguelikeMode") {
                __instance.Tags.Add("new run", "true");
                __instance.Tags.Add("cleanup", "true");
            }
        }

        // AddRuneTo
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RunestoneRules), "AddRuneTo")]
        private static bool AddRuneToPrefixPatch(Item runestone, Item equipment) {
            if (equipment.ItemName == CustomItems.ThrowersBandolier) {
                equipment.Price += runestone.Price;
                equipment.Runes.Add(runestone);

                Item itemTemplate = Items.GetItemTemplate(equipment.ItemName);
                if (runestone.Level > equipment.Level)
                    equipment.Level = runestone.Level;
                if (equipment.Description != "")
                    equipment.Description += "\n";
                Item obj1 = equipment;
                obj1.Description = obj1.Description + "{b}" + runestone?.RuneProperties?.Prefix.Capitalize() + ".{/b} " + runestone?.RuneProperties?.RulesText;
                equipment.Name = itemTemplate.Name;
                foreach (Item obj2 in (IEnumerable<Item>)equipment.Runes.OrderByDescending(rune => rune?.RuneProperties?.RuneKind))
                    equipment.Name = obj2.RuneProperties?.Prefix + " " + equipment.Name;
                return false;
            }
            return true;
        }

        // AddRuneTo
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RunestoneRules), "AddRuneTo")]
        private static void AddRuneToPostfixPatch(Item runestone, Item equipment) {
            if (equipment.HasTrait(Trait.SpecificMagicWeapon) && equipment.HasTrait(ModTraits.Roguelike)) {
                equipment.Price += Items.GetItemTemplate(equipment.ItemName).Price;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RunestoneRules), "AttachSubitem")]
        private static bool AttachSubitemPatch(ref SubitemAttachmentResult __result, Item runestone, Item? equipment) {
            var allowedRunes = new RuneKind[] { RuneKind.WeaponPotency, RuneKind.WeaponStriking, RuneKind.WeaponProperty };
            if (equipment?.ItemName == CustomItems.ThrowersBandolier && runestone.RuneProperties != null && allowedRunes.Contains(runestone.RuneProperties.RuneKind)) {
                var runesOfThisTypeAlready = equipment.Runes.Count(itm => itm.RuneProperties!.RuneKind == runestone.RuneProperties.RuneKind);
                var numPropertyRunes = equipment.Runes.Where(r => r.RuneProperties?.RuneKind == RuneKind.WeaponProperty).Count();
                int? propertyRuneSlots = equipment.Runes.FirstOrDefault(r => r.RuneProperties?.RuneKind == RuneKind.WeaponPotency)?.RuneProperties?.FundamentalLevel;

                if (runestone.RuneProperties.RuneKind == RuneKind.WeaponProperty && !equipment.Runes.Any(r => r.RuneProperties?.RuneKind == RuneKind.WeaponPotency)) {
                    __result = new SubitemAttachmentResult(SubitemAttachmentResultKind.Unallowed, "Only +1, +2 or +3 weapons can be enchanted with weapon property runes.");
                    return false;
                }

                if (equipment.Runes.Any((Item rn) => rn.ItemName == runestone.ItemName)) {
                    __result = new SubitemAttachmentResult(SubitemAttachmentResultKind.Unallowed, "This weapon already has that property rune.");
                    return false;
                }

                if (runestone.RuneProperties.RuneKind == RuneKind.WeaponProperty && runesOfThisTypeAlready >= propertyRuneSlots || runesOfThisTypeAlready > 0 && runestone.RuneProperties.RuneKind != RuneKind.WeaponProperty) {
                    __result = new SubitemAttachmentResult(SubitemAttachmentResultKind.SwapOrUpgradeRune, null);
                    return false;
                }

                if (runestone.RuneProperties.RuneKind == RuneKind.WeaponPotency && equipment.Runes.Any(r => r.RuneProperties?.RuneKind == RuneKind.WeaponPotency)) {
                    //int nLevel = runestone.RuneProperties.Prefix[1] - '0';
                    __result = new SubitemAttachmentResult(SubitemAttachmentResultKind.Unallowed, "Only one potency rune can be attached to this item at a time.");
                    return false;
                }

                if (runestone.RuneProperties.RuneKind == RuneKind.WeaponStriking && equipment.Runes.Any(r => r.RuneProperties?.RuneKind == RuneKind.WeaponStriking)) {
                    __result = new SubitemAttachmentResult(SubitemAttachmentResultKind.Unallowed, "Only one striking rune can be attached to this item at a time.");
                    return false;
                }

                if (runestone.RuneProperties.RuneKind == RuneKind.WeaponProperty && numPropertyRunes >= propertyRuneSlots) {
                    __result = new SubitemAttachmentResult(SubitemAttachmentResultKind.Unallowed, "This weapon cannot support any more property runes.");
                    return false;
                }

                __result = new SubitemAttachmentResult(SubitemAttachmentResultKind.PlaceAsSubitem, null, delegate {
                    equipment.WithModification(new ItemModification(ItemModificationKind.Rune) {
                        ItemName = runestone.ItemName
                    });
                    // RunestoneRules.AddRuneTo(Items.CreateNew(runestone.ItemName), equipment);
                    Sfxs.Play(SfxName.AttachRune);
                });
                return false;
            }
            
            if (runestone.RuneProperties == null) {
                return true;
            }

            if (equipment != null && equipment.HasTrait(ModTraits.CannotHavePropertyRune) && (runestone.RuneProperties.RuneKind == RuneKind.WeaponProperty)) {
                __result = new SubitemAttachmentResult(SubitemAttachmentResultKind.Unallowed, "Specific magic items cannot be inscribed with property runes.");
                return false;
            }

            if (equipment != null && equipment.HasTrait(ModTraits.CannotHavePropertyRune) && (runestone.RuneProperties.RuneKind == RuneKind.WeaponMaterial)) {
                __result = new SubitemAttachmentResult(SubitemAttachmentResultKind.Unallowed, "Specific magic items cannot be reforged with weapon materials.");
                return false;
            }

            if (equipment != null && equipment.HasTrait(ModTraits.LegendaryItem)) {
                __result = new SubitemAttachmentResult(SubitemAttachmentResultKind.Unallowed, "Legendary magic items cannot be inscribed with runes."); ;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterSheet), "CanUse")]
        private static void CanUsePatch(CharacterSheet __instance, ref bool __result, Item? item) {
            if (item != null && item.HasTrait(ModTraits.Wand)) {
                __result = false;
                foreach (Trait trait in __instance.Calculated.SpellTraditionsKnown) {
                    if (item.HasTrait(trait)) {
                        __result = true;
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("ChooseProfilePhase", "Draw")]
        private static void ChooseProfilePhaseDrawPatch(object __instance, SpriteBatch sb, Game game, float elapsedSeconds) {
            Savegame?[] profiles = (Savegame?[]) Type.GetType("Dawnsbury.Phases.Menus.StoryMode.ChooseProfilePhase, Dawnsbury Days")?.GetProperty("Profiles")?.GetValue(__instance);

            if (profiles == null) {
                return;
            }

            int i = 0;
            foreach (Savegame? save in profiles) {
                if (save != null && UtilityFunctions.DiedThisRun(save.CampaignState!)) {
                    int h = 200;
                    int w = 200;
                    int padding = 10;
                    Illustrations.FailedRun.DrawImage(new Rectangle(1860 - padding - w, 500 + i * 210, w, h), null, true, false, null);
                }

                i += 1;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Shop), "CreateAdventurersDawn")]
        private static void CreateAdventurersDawnPatch(Shop __instance, ref Shop __result, int level, bool unlimitedBuyAndSell) {
            if (CampaignState.Instance?.AdventurePath?.Name != "Roguelike Mode") {
                return;
            }

            string comment = "<Comment about last elite encounter goes here>";
            if (level == 1) {
                comment = "Welcome adventurers!";
            } else {
                comment = UtilityFunctions.GetShopBanter();
            }


            string desc = comment + "\n\n" + (unlimitedBuyAndSell ? "The Adventurer's Dawn buys any items, and sells all items of level " + level.ToString() + " or lower. {i}(You can sell back items for a full refund before an adventure path begins.){/i}" : "The Adventurer's Dawn buys any items {b}at half price{/b}, and sells all items of level " + level.ToString() + " or lower.");
            __result.GetType()?.GetProperty("Description", BindingFlags.Instance | BindingFlags.Public)?.SetValue(__result, desc);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Shop), "Carries")]
        private static void ShopCarriesPatch(Shop __instance, ref bool __result, Item item) {
            if (CampaignState.Instance?.AdventurePath?.Name != "Roguelike Mode") {
                return;
            }

            if (item.ItemName == CustomItems.StaffOfSpellPenetration) {
                Func<Item, bool> carried = (Func<Item, bool>)__instance.GetType()?.GetProperty("Carried", BindingFlags.Instance | BindingFlags.Public)?.GetValue(__instance);
                __result = carried!(item);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterInventoryControl), "DrawSlot")]
        private static void CharacterInventoryControl_DrawSlot_Patch(Shop __instance, Rectangle rectangle, InventoryItemSlot itemSlot, string textIfEmpty) {
            if (itemSlot.Item == null || !itemSlot.Item.HasTrait(ModTraits.Wand) || !itemSlot.Item.ItemModifications.Any(mod => mod.Kind == ItemModificationKind.UsedThisDay)) {
                return;
            }

            Rectangle rectangle1 = new Rectangle(rectangle.X + 1, rectangle.Y + 2, rectangle.Width - 2, rectangle.Height - 40 - 2);

            int height = rectangle1.Height / 4;
            Rectangle rectangle6 = new Rectangle(rectangle1.X, rectangle1.Bottom - height - height, rectangle1.Width, height);
            Primitives.FillRectangleNative(rectangle6, Color.Red.Alpha(200));
            Writer.DrawStringNative("{b}used up{/b}", rectangle6, new Color?(Color.White), alignment: Writer.TextAlignment.Middle);
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(AdventurePathView), "Draw")]
        //private static void CampaignMenuPhaseDrawPatch(AdventurePathView __instance) {
        //    CampaignState state = CampaignState.Instance;

        //    if (state?.AdventurePath?.Name != "Roguelike Mode")
        //        return;

        //    var stopItem = (CampaignStopListboxItem)__instance.AdventurePath.SelectedItem!;
        //    var stop = stopItem.Stop;
        //    var rectangleStop = new Rectangle(__instance.ContentRectangle.X + 610, __instance.ContentRectangle.Y + 100, __instance.ContentRectangle.Width - 620, __instance.ContentRectangle.Height - 20).Extend(-40, -40);

        //    if (stop.Icon == IllustrationName.None) {
        //        Writer.DrawString("{icon:" + Illustrations.EliteEncounter.ToString() + "} " + stop.Name, rectangleStop, font: BitmapFontGroup.Mia48Font);
        //    } else if (stop.Icon == IllustrationName.Action) {
        //        Writer.DrawString("{icon:" + Illustrations.BossEncounter.ToString() + "} " + stop.Name, rectangleStop, font: BitmapFontGroup.Mia48Font);
        //    }
        //}

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CampaignMenuPhase), "Draw")]
        private static void CampaignMenuPhaseDrawPatch(CampaignMenuPhase __instance, SpriteBatch sb, Game game, float elapsedSeconds) {
            CampaignState state = CampaignState.Instance;

            if (state?.AdventurePath?.Name != "Roguelike Mode")
                return;

            //if (__instance.CurrentStopReal.Icon == IllustrationName.None) {
            //    var rectBottomBar = new Rectangle(0, 1440 - 100, 2560, 100);
            //    Primitives.DrawImage(Illustrations.EliteEncounter, new Rectangle(5, rectBottomBar.Y + 5, rectBottomBar.Height - 10, rectBottomBar.Height - 10));
            //} else if (__instance.CurrentStopReal.Icon == IllustrationName.Action) {
            //    var rectBottomBar = new Rectangle(0, 1440 - 100, 2560, 100);
            //    Primitives.DrawImage(Illustrations.BossEncounter, new Rectangle(5, rectBottomBar.Y + 5, rectBottomBar.Height - 10, rectBottomBar.Height - 10));
            //}

            if (UtilityFunctions.DiedThisRun(state)) {
                // && (state.Tags.TryGetValue("deaths", ) != "0" || state.Tags["restarts"] != "0")
                int h = 250;
                int w = 250;
                int padding = 10;

                if (__instance.CurrentView is PartyView) {
                    Illustrations.FailedRun.DrawImage(new Rectangle(padding, padding, w, h), null, false, false, null);
                } else if (__instance.CurrentView is AdventurePathView && ((__instance.CurrentView as AdventurePathView)?.AdventurePath.SelectedItem as CampaignStopListboxItem)?.Stop is LevelUpStop) {
                    Illustrations.FailedRun.DrawImage(new Rectangle(Root.ScreenWidth - padding - w, padding, w, h), null, false, false, null);
                } else if (__instance.CurrentView is AdventurePathView && ((__instance.CurrentView as AdventurePathView)?.AdventurePath.SelectedItem as CampaignStopListboxItem)?.Stop is LongRestCampaignStop) {
                    Illustrations.FailedRun.DrawImage(new Rectangle(Root.ScreenWidth - padding - w, padding, w, h), null, false, false, null);
                } else if (__instance.CurrentView is AdventurePathView && ((__instance.CurrentView as AdventurePathView)?.AdventurePath.SelectedItem as CampaignStopListboxItem)?.Stop is DawnsburyStop) {
                    Illustrations.FailedRun.DrawImage(new Rectangle((Root.ScreenWidth + w) / 2, Root.ScreenHeight - h - 100, w, h), null, false, false, null);
                } else if (__instance.CurrentView is ShopView) {
                    return;
                } else {
                    Illustrations.FailedRun.DrawImage(new Rectangle((Root.ScreenWidth + w) / 2, Root.ScreenHeight - h - 100, w, h), null, false, false, null);
                    // Illustrations.FailedRun.DrawImage(new Rectangle(Root.ScreenWidth - padding - w, padding, w, h), null, false, false, null);
                }
            }

            if (!state.Tags.ContainsKey("corruption level")) {
                //var bg = new Rectangle(0, 0, Root.ScreenWidth, Root.ScreenHeight);
                //Primitives.FillRectangleGradient(bg, ColorScheme.Instance.ButtonBorderGradient1, ColorScheme.Instance.ButtonBorderGradient2);
                var rectangle = new Rectangle(0, 0, Root.ScreenWidth, Root.ScreenHeight);  //LeftMenu.ContentRectangle; //new Rectangle(0, 0, Root.ScreenWidth, Root.ScreenHeight);
                Primitives.FillRectangleGradient(rectangle, Color.PaleVioletRed, Color.Violet);

                //Rectangle rectangle2 = Primitives.Scale(rectangle);
                //Primitives.DrawAndFillRectangleNative(rectangle2.Extend(-2, -2), Color.SteelBlue, Color.Black);
                //Primitives.DrawRectangleNative(rectangle2, Color.Black);

                //Writer.DrawString("Corruption Level", rectangle2, Color.Black, BitmapFontGroup.Mia48Font, Writer.TextAlignment.TopLeft, degrading: true);

                // int width = LeftMenu.LEFT_MENU_WIDTH - 20;

                // Root.ScreenWidth / 2 - width / 2, 

                //UI.DrawUIButton(new Rectangle(10, 400 + 1 * 210, width, 200), "Normal Difficulty", () => {
                //    state.Tags.Add("corruption level", "0");
                //}, Writer.TextAlignment.Middle, BitmapFontGroup.Mia48Font, "This is the intended difficulty of the roguelike mode, without any additional rules.", ColorScheme.Instance.ButtonBorderGradient1);

                //UI.DrawUIButton(new Rectangle(10, 400 + 2 * 210, width, 200), "Corruption Level 1", () => {
                //    state.Tags.Add("corruption level", "1");
                //}, Writer.TextAlignment.Middle, BitmapFontGroup.Mia48Font, "Enemies encountered during regular encounters in this difficulty sometimes have unique templates, granting them additional abilities.", Color.Violet);

                LeftMenu.Draw([
                    new LeftMenuButton("Easy Difficulty", () => {
                        state.Tags.Add("corruption level", "-1");
                        foreach (AdventurePathHero hero in state.Heroes) {
                            hero.LongTermEffects.Add(WellKnownLongTermEffects.CreateLongTermEffect("Heavenly Favour")!);
                        }
                        CampaignState.Autosave();
                    }, "The party gains the Heavenly Favour boon, granting them a permanent +1 bonus to their attack, save and spell DCs and AC."),
                    new LeftMenuButton("Normal Difficulty", () => {
                        state.Tags.Add("corruption level", "0");
                        CampaignState.Autosave();

                    }, "This is the intended difficulty of the roguelike mode, without any additional rules."),
                    new LeftMenuButton("Corruption Level 1", () => {
                        state.Tags.Add("corruption level", "1");
                        CampaignState.Autosave();
                    }, "Enemies encountered during regular encounters in this difficulty sometimes have unique templates, granting them additional abilities.")
                ]);

                Writer.DrawString("Choose your Difficulty", new Rectangle(10, 10, Root.ScreenWidth, 400), Color.Black, BitmapFontGroup.Mia48Font);

                string? tooltip = (string)Type.GetType("Dawnsbury.Display.MouseOver, Dawnsbury Days")?.GetProperty("MouseOverMenuTooltip", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                if (tooltip != null)
                    Writer.DrawString(tooltip, new Rectangle(620, 400, 1000, 800));
            }

            if (state.InitialCreationStatus == CampaignStateInitialCreationStatus.Complete && state.Tags.ContainsKey("cleanup")) {
                state.Tags.Remove("cleanup");
                UtilityFunctions.CleanUnlockFeats(state);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CampaignMenuPhase), "CreateViews")]
        private static void CreateViewsPatch() {
            CampaignState state = CampaignState.Instance;
            if (state == null || state?.AdventurePath?.Name != "Roguelike Mode") return;

            if (state.AdventurePath?.CampaignStops[2].Name == "Random Encounter"
                || state.Tags.ContainsKey("seed") && state.Tags["seed"] != Loader.Seed[0]
                || state.Tags.ContainsKey("new run")) {
                GenerateRun(state);
            }
        }

        private static void GenerateRun(CampaignState campaign) {
            if (campaign.AdventurePath == null) {
                return;
            }

            List<CampaignStop> path = campaign.AdventurePath.CampaignStops;
            LootTables.GenerateParty(campaign);
            EncounterTables.LoadEncounterTables();
            SkillChallengeTables.LoadSkillChallengeTables();

            // Declare campaign tags
            if (!campaign.Tags.ContainsKey("seed") || campaign.Tags.ContainsKey("new run")) {
                campaign.Tags.Clear();
                campaign.Tags.Add("seed", R.NextVisualOnly(100000).ToString());
                Loader.Seed[0] = campaign.Tags["seed"];
                campaign.Tags.Add("restarts", "0");
                campaign.Tags.Add("deaths", "0");
                campaign.Tags.Add("cleanup", "true");

                // Apply cross-run effects
                var creManager = new CrossRunEffectsManager(CampaignState.InstanceProfileNumber);
                creManager.TransferItems();
                
                CampaignState.Autosave();
            } else {
                Loader.Seed[0] = campaign.Tags["seed"];
            }

            if (!Int32.TryParse(campaign.Tags["seed"], out int result)) {
                throw new ArgumentException("ERROR: Seed is not an integer (Roguelike Mod)");
            }

            //LTEs.InitBoons(campaign);

            var rand = new Random(result);

            bool newTDList = campaign.Tags.TryAdd("TreasureDemonEncounters", "");

            for (int i = 1; i <= 3; i++) {
                if (!campaign.Tags.ContainsKey($"Lv{i}Encounters")) {
                    campaign.Tags.Add($"Lv{i}Encounters", EncounterTables.encounters[i-1].Count.ToString());
                }
                if (!campaign.Tags.ContainsKey($"Lv{i}EliteEncounters")) {
                    campaign.Tags.Add($"Lv{i}EliteEncounters", EncounterTables.eliteEncounters[i - 1].Count.ToString());
                }
            }

            if (!campaign.Tags.ContainsKey($"Lv4Encounters")) {
                campaign.Tags.Add($"Lv4Encounters", EncounterTables.encounters[3].Count.ToString());
            }

            if (!campaign.Tags.ContainsKey($"Bosses")) {
                campaign.Tags.Add($"Bosses", EncounterTables.bossFights.Count.ToString());
            }

            int removed = 0;
            int level = 1;
            int fightNum = 0;

            for (int i = 0; i < path.Count; i++) {
                if (path[i] is LevelUpStop) {
                    fightNum = 0;
                    removed = 0;
                    level += 1;
                }

                if (path[i] is EncounterCampaignStop) {
                    fightNum += 1;
                    ModEnums.EncounterType encounterType = level == 4 && fightNum == 3 ? ModEnums.EncounterType.BOSS : fightNum == 1 || fightNum == 3 || level == 4 ? ModEnums.EncounterType.NORMAL : fightNum == 2 ? ModEnums.EncounterType.EVENT : ModEnums.EncounterType.ELITE;
                    if (newTDList && encounterType == ModEnums.EncounterType.NORMAL && R.NextVisualOnly(0, 8) <= 3) {
                        campaign.Tags["TreasureDemonEncounters"] += $"{i}, ";
                    }
                    path[i] = GenerateRandomEncounter(rand, removed, level, encounterType, campaign);
                    path[i].Index = i;

                    if (encounterType == ModEnums.EncounterType.NORMAL) {
                        removed += 1;
                    } else if (encounterType == ModEnums.EncounterType.ELITE) {
                        typeof(CampaignStop).GetField("<Icon>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(path[i], IllustrationName.GrimTendrils);
                    } else if (encounterType == ModEnums.EncounterType.BOSS) {
                        //typeof(CampaignStop).GetField("<Icon>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(path[i], IllustrationName.Evil);
                        typeof(CampaignStop).GetField("<Icon>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(path[i], IllustrationName.Evil);
                    } else if (encounterType == ModEnums.EncounterType.EVENT) {
                        SkillChallengeTables.chosenEvents.Add(i, SkillChallengeTables.events[rand.Next(0, SkillChallengeTables.events.Count())]);
                        SkillChallengeTables.events.Remove(SkillChallengeTables.chosenEvents[i]);
                    }
                }
            }

            (path[path.Count - 1] as DawnsburyStop)!.CustomText = "{b}Congratulations!{/b} You survived the Below and saved Dawnsbury from the Machinations of the Spider Queen! But it won't be long before she tries again, and another brave group of adventurers will need to once again brave the Below...\n\n" +
                    "{b}Stats{/b}\n" +
                    "{b}Deaths:{/b} " + campaign.Tags["deaths"] + "\n" +
                    "{b}Restarts:{/b} " + campaign.Tags["restarts"] +
                    "\n\n" + Loader.Credits;
        }

        private static CampaignStop GenerateRandomEncounter(Random rng, int removed, int level, ModEnums.EncounterType encounterType, CampaignState campaign) {

            if (level == 0) {
                throw new ArgumentException("ERROR: Cannot load level 0 encounters. (Roguelike Mod)");
            }

            EncounterCampaignStop stop = new TypedEncounterCampaignStop<HallOfBeginnings>(); ;

            if (encounterType == ModEnums.EncounterType.NORMAL) {
                stop = EncounterTables.encounters[level - 1][rng.Next(0, Int32.Parse(campaign.Tags[$"Lv{level}Encounters"]) - removed)];
                EncounterTables.encounters[level - 1].Remove(stop);
            } else if (encounterType == ModEnums.EncounterType.ELITE) {
                stop = EncounterTables.eliteEncounters[level - 1][rng.Next(0, Int32.Parse(campaign.Tags[$"Lv{level}EliteEncounters"]) - (level - 1))];
                if (level == 1) {
                    EncounterTables.eliteEncounters[level].RemoveAll(en => en.Name == stop.Name);
                    EncounterTables.eliteEncounters[level + 1].RemoveAll(en => en.Name == stop.Name);
                } else if (level == 2) {
                    EncounterTables.eliteEncounters[level].RemoveAll(en => en.Name == stop.Name);
                }
            } else if (encounterType == ModEnums.EncounterType.BOSS) {
                stop = EncounterTables.bossFights[rng.Next(0, Int32.Parse(campaign.Tags["Bosses"]))];
                EncounterTables.bossFights.Remove(stop);
            } else if (encounterType == ModEnums.EncounterType.EVENT) {
                if (level == 1) stop = new TypedEncounterCampaignStop<Level1SkillChallenge>();
                else if (level == 2) stop = new TypedEncounterCampaignStop<Level2SkillChallenge>();
                else if (level == 3) stop = new TypedEncounterCampaignStop<Level3SkillChallenge>();
            }

            // Default value for debugging
            return stop;
        }

    }
}
