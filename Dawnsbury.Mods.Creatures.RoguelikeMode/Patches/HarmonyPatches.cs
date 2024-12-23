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

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Patches {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    [HarmonyLib.HarmonyPatch]
    public class HarmonyPatches {

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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CampaignState), MethodType.Constructor, new Type[] { typeof(List<CharacterSheet>), typeof(AdventurePath) })]
        private static void CampaignStatePatch(CampaignState __instance, List<CharacterSheet> heroes, AdventurePath adventurePath) {
            if (__instance.AdventurePath != null && __instance.AdventurePath.Id == "RoguelikeMode")
            __instance.Tags.Add("new run", "true");
        }

        // AddRuneTo
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RunestoneRules), "AddRuneTo")]
        private static void AddRuneToPatch(Item runestone, Item equipment) {
            if (equipment.HasTrait(Trait.SpecificMagicWeapon)) {
                equipment.Price += Items.GetItemTemplate(equipment.ItemName).Price;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RunestoneRules), "AttachSubitem")]
        private static bool AttachSubitemPatch(ref SubitemAttachmentResult __result, Item runestone, Item? equipment) {
            if (runestone.RuneProperties == null) {
                return true;
            }

            if (equipment != null && equipment.HasTrait(Traits.CannotHavePropertyRune) && runestone.RuneProperties.RuneKind == RuneKind.WeaponProperty) {
                __result = new SubitemAttachmentResult(SubitemAttachmentResultKind.Unallowed, "Specific magic items cannot be inscribed with property runes.");
                return false;
            }
            if (equipment != null && equipment.HasTrait(Traits.LegendaryItem)) {
                __result = new SubitemAttachmentResult(SubitemAttachmentResultKind.Unallowed, "Legendary magic items cannot be inscribed with runes."); ;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterSheet), "CanUse")]
        private static void CanUsePatch(CharacterSheet __instance, ref bool __result, Item? item) {
            if (item != null && item.HasTrait(Traits.Wand)) {
                __result = false;
                foreach (Trait trait in __instance.Calculated.SpellTraditionsKnown) {
                    if (item.HasTrait(trait)) {
                        __result = true;
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CampaignMenuPhase), "CreateViews")]
        private static void CreateViewsPatch(CampaignMenuPhase __instance) {
            CampaignState state = __instance.CurrentCampaignState;

            if (state.AdventurePath.CampaignStops[2].Name == "Random Encounter" || state.Tags.ContainsKey("new run")) {
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

            // Debug for testing
            //if (campaign.Tags.ContainsKey("new run")) {
            //    campaign.Tags.Remove("new run");
            //}

            // Declare campaign tags
            if (!campaign.Tags.ContainsKey("seed") || campaign.Tags.ContainsKey("new run")) {
                campaign.Tags.Clear();
                campaign.Tags.Add("seed", R.Next(100000).ToString());
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
                    ModEnums.EncounterType encounterType = level == 4 ? ModEnums.EncounterType.BOSS : fightNum == 1 || fightNum == 3 ? ModEnums.EncounterType.NORMAL : fightNum == 2 ? ModEnums.EncounterType.EVENT : ModEnums.EncounterType.ELITE;
                    if (newTDList && encounterType == ModEnums.EncounterType.NORMAL && R.Next(0, 8) <= 3) {
                        campaign.Tags["TreasureDemonEncounters"] += $"{i}, ";
                    }
                    path[i] = GenerateRandomEncounter(rand, removed, level, encounterType, campaign);
                    path[i].Index = i;

                    if (encounterType == ModEnums.EncounterType.NORMAL) {
                        removed += 1;
                    }
                    else if (encounterType == ModEnums.EncounterType.EVENT) {
                        SkillChallengeTables.chosenEvents.Add(i, SkillChallengeTables.events[rand.Next(0, SkillChallengeTables.events.Count())]);
                        SkillChallengeTables.events.Remove(SkillChallengeTables.chosenEvents[i]);
                    }
                }
            }

            // TODO: Override the .Description property of DawnsburyStop to instead show my credits if city name matches one from this adventure path.
            //(path.Last() as DawnsburyStop).Description = Loader.Credits;
            
            
            
            var stop = path[path.Count - 1];

            var t1 = (string)typeof(DawnsburyStop).GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stop);
            var t2 = (int)typeof(DawnsburyStop).GetField("dawnsburyStopIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stop);
            var t3 = (int)typeof(DawnsburyStop).GetField("<ShopLevel>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stop);

            path[path.Count - 1] = new CustomLastStop(path[path.Count - 1] as DawnsburyStop, path[path.Count - 1].Index);


            //path.Remove(path.Last());
            //path.Add(new DawnsburyStop("You won! Congrats!", path.Last().Index + 1, level, false, "Post Init"));

            var test = 3;
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
                // TODO: Rework removed to be a seperate count for elite and regular encounters if I add optional mid-run elite fights.
                stop = EncounterTables.eliteEncounters[level - 1][rng.Next(0, Int32.Parse(campaign.Tags[$"Lv{level}EliteEncounters"]))];
                EncounterTables.eliteEncounters[level - 1].Remove(stop);
            } else if (encounterType == ModEnums.EncounterType.BOSS) {
                stop = EncounterTables.bossFights[rng.Next(0, Int32.Parse(campaign.Tags["Bosses"]) - removed)];
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
