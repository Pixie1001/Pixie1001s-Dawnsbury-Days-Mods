using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Display.Illustrations;
using Microsoft.Xna.Framework;
using System.Text;
using System.IO;
using System.Buffers.Text;
using static System.Net.Mime.MediaTypeNames;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Animations.Movement;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Coroutines.Requests;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Roller;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.IO;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class UtilityFunctions {

        internal static bool DiedThisRun(CampaignState save) {
            if (PlayerProfile.Instance.IsBooleanOptionEnabled("RL_HideDeathIcon")) {
                return false;
            }
            
            if (!save.Tags.TryGetValue("deaths", out string deaths) || !save.Tags.TryGetValue("restarts", out string restarts)) {
                return false;
            }

            if (deaths != "0" || restarts != "0") {
                return true;
            }
            return false;
        }


        //internal static void ReplaceSpiderSprite(Creature monster) {
        //    if (monster.Illustration == Illustrations.HuntingSpider) {
        //        monster.Illustration = Illustrations.Bear1;
        //    } else if (monster.Illustration == Illustrations.SpiderHatchling) {
        //        monster.Illustration = IllustrationName.Bear256;
        //    } else if (monster.Illustration == Illustrations.BebilithSpawn) {
        //        monster.Illustration = Illustrations.Bear2;
        //    } else if (monster.Illustration == Illustrations.AbyssalHandmaiden) {
        //        monster.Illustration = IllustrationName.Succubus;
        //    } else if (monster.Illustration == IllustrationName.LengSpider) {
        //        monster.Illustration = Illustrations.Bear3;
        //    } else if (monster.Illustration == IllustrationName.GiantSpider) {
        //        monster.Illustration = Illustrations.Bear4;
        //    } else if (monster.Illustration == IllustrationName.DemonWebspinner256) {
        //        monster.Illustration = Illustrations.Bear5;
        //    }
        //}
        internal static void CleanUnlockFeats(CampaignState state) {
            foreach (var hero in state.Heroes) {
                hero.CharacterSheet.SelectedFeats.Remove("Power of the Rat Fiend");
            }
        }

        internal static string GetShopBanter() {
            string? lastEliteName = null;
            //int 
            lastEliteName = CampaignState.Instance?.AdventurePath?.CampaignStops[CampaignState.Instance.CurrentStopIndex - 1].Name;

            if (lastEliteName == null) {
                return "";
            }

            switch (lastEliteName) {
                case "Mother of the Pool":
                    return "Mother Cassandra? That name sounds familiar. I think a young tiefling girl by that name grew up here. Alas, they were less tolerant times. It seems one way or another, she finally found a way to gain the affection she was so cruelly denied...";
                case "Maiden of the Lost":
                    return "She was building a cemetery you say? How peculiar, I've never seen the Starborn's disciples put so much care and attention towards the dignity of the dead before.";
                case "Crone of the Wilds":
                    return "Agatha Agarthia you say? Yes I think I've heard of her. Legend has it she used to be a wise hermit, reveered by the clergy of the Blooming Flower. Well, until they learned about the whole child eating thing anyway...";
                case "Lair of the Drider":
                    return "So it was like a centaur, but instead of a horse it was part spider? Do you think they used the magic from the leyline to create such a creature, or was she merely its guardian...";
                case "Grand Staircase":
                    return "An owlbear? How cute! But I do wonder how the drow even managed to tame a creature like that... Perhaps the Starborn granted them owl kibble from other conquered worlds? Or maybe they just gave really good chin scritches?";
                case "Hall of Smoke":
                    return "A lost temple you say? I wonder what god it was dedicated to... Those guardians must have been waiting down their for aoens... How tragic.";
                case "Aqueducts":
                    return "Those aquaducts sure do sound claustrophobic. An entire Drow city, just abandoned though? I wonder what possessed them to flee. Perhaps a cave in? Or some kind of creature decided to make the city its lairs? It makes you wonder why the Drow chose to seclude themselves down there in the first place...";
                case "Fissure Duel":
                    return "An entire other adventurer party composed entirely of Drow? How exciting!";
                default:
                    return "";
            }
        }

        internal static Creature AddNaturalWeapon(Creature creature, string naturalWeaponName, Illustration illustration, int attackBonus, Trait[] traits, string damage, DamageKind damageKind, Action<WeaponProperties>? additionalWeaponPropertyActions = null)
        {
            bool flag = traits.Contains(Trait.Finesse) || traits.Contains(Trait.Ranged);
            int num = creature.Abilities.Strength;
            if (flag)
            {
                num = Math.Max(num, creature.Abilities.Dexterity);
            }

            int proficiencyLevel = creature.ProficiencyLevel;
            if (creature.Proficiencies.Get(Trait.Weapon) == Proficiency.Untrained)
            {
                creature.WithProficiency(Trait.Weapon, (Proficiency)(attackBonus - proficiencyLevel - num));
            }

            MediumDiceFormula mediumDiceFormula = DiceFormula.ParseMediumFormula(damage, naturalWeaponName, naturalWeaponName);
            int additionalFlatBonus = mediumDiceFormula.FlatBonus - creature.Abilities.Strength;
            Item item = new Item(illustration, naturalWeaponName, traits.Concat([Trait.Unarmed]).ToArray()).WithWeaponProperties(new WeaponProperties(mediumDiceFormula.DiceCount + "d" + (int)mediumDiceFormula.DieSize, damageKind)
            {
                AdditionalFlatBonus = additionalFlatBonus
            });
            additionalWeaponPropertyActions?.Invoke(item.WeaponProperties!);
            if (creature.UnarmedStrike == null)
            {
                creature.UnarmedStrike = item;
            }
            else
            {
                creature.WithAdditionalUnarmedStrike(item);
            }

            return creature;
        }

        public static Creature AddManufacturedWeapon(Creature creature, ItemName manufacturedWeapon, int attackBonus, Trait[] traits, string damage, DamageKind damageKind, Action<WeaponProperties>? additionalWeaponPropertyActions = null, Action<Item>? additionalWeaponActions = null)
        {
            Item item = Items.CreateNew(manufacturedWeapon);
            var itemTraits = item.Traits.Where((trait) => trait == Trait.Weapon || trait == Trait.Ranged || trait == Trait.Melee || trait == Trait.Simple || trait == Trait.Martial || trait == Trait.Advanced);
            item.Traits.Clear();
            item.Traits.AddRange(itemTraits);
            item.Traits.AddRange(traits);
            item.Traits.Add(Trait.EncounterEphemeral);

            if (creature.Proficiencies.Get(Trait.Weapon) == Proficiency.Untrained)
            {
                bool flag = traits.Contains(Trait.Finesse);
                int num = creature.Abilities.Strength;
                if (flag)
                {
                    num = Math.Max(num, creature.Abilities.Dexterity);
                }

                int proficiencyLevel = creature.ProficiencyLevel;
                creature.WithProficiency(Trait.Weapon, (Proficiency)(attackBonus - proficiencyLevel - num));
            }

            item.WithWeaponProperties(new WeaponProperties("1d1", damageKind));

            MediumDiceFormula mediumDiceFormula = DiceFormula.ParseMediumFormula(damage, item.Name, item.Name);
            item.WeaponProperties!.DamageDieSize = (int)mediumDiceFormula.DieSize;
            item.WeaponProperties.DamageDieCount = mediumDiceFormula.DiceCount;
            int additionalFlatBonus = mediumDiceFormula.FlatBonus - (item.HasTrait(Trait.Melee) ? creature.Abilities.Strength : 0);
            item.WeaponProperties.AdditionalFlatBonus = additionalFlatBonus;
            if (additionalWeaponPropertyActions != null)
            {
                item.WithAdditionalWeaponProperties(additionalWeaponPropertyActions);
            }

            additionalWeaponActions?.Invoke(item);
            creature.AddHeldItem(item);
            return creature;
        }

        internal static async Task<Tile?> GetChargeTiles(Creature self, MovementStyle movementStyle, int minimumDistance, string msg, Illustration img) {
            Tile startingPos = self.Occupies;
            Vector2 pos = self.Occupies.ToCenterVector();
            List<Option> options = new List<Option>();
            Dictionary<Option, Tile> pairs = new Dictionary<Option, Tile>();

            PathfindingDescription pathfindingDescription = new PathfindingDescription() {
                Squares = movementStyle.MaximumSquares,
                Style = movementStyle
            };



            IList<Tile>? tiles = Pathfinding.Floodfill(self, self.Battle, pathfindingDescription);

            if (tiles == null) {
                self.Occupies.Overhead("cannot move", Color.White);
                return null;
            }

            movementStyle.MaximumSquares *= 5;
            movementStyle.ShortestPath = true;

            // Compile destination tiles
            foreach (Tile tile in tiles) {
                // Check if valid tile
                if (!tile.LooksFreeTo(self)) {
                    continue;
                }

                // Handle minimum distance
                if (self.DistanceTo(tile) < minimumDistance) {
                    continue;
                }

                // Handle LoS
                if (new CoverKind[] { CoverKind.Greater, CoverKind.Blocked, CoverKind.Standard }.Contains(self.HasLineOfEffectTo(tile))) {
                    continue;
                }

                // Add tile as option
                CombatAction movement = new CombatAction(self, img, "Powerful Charge", new Trait[] { Trait.Move }, "", Target.Tile((cr, t) => t.LooksFreeTo(cr), (cr, t) => (float)int.MinValue)
                    .WithPathfindingGuidelines((cr => pathfindingDescription))
                )
                .WithActionCost(0)
                ;
                options.Add(movement.CreateUseOptionOn(tile));
                pairs.Add(options.Last(), tile);
            }

            // Adds a Cancel Option
            if (self.OwningFaction.IsControlledByHumanUser) {
                options.Add(new CancelOption(true));
            }

            // Prompts the user for their desired tile and returns it or null
            Option selectedOption = (await self.Battle.SendRequest(new AdvancedRequest(self, msg, options) {
                IsMainTurn = false,
                IsStandardMovementRequest = true,
                TopBarIcon = img,
                TopBarText = msg
            })).ChosenOption;

            if (selectedOption != null) {
                if (selectedOption is CancelOption cancel) {
                    return null;
                }

                return pairs[selectedOption];
            }

            return null;
        }

        internal static string WithPlus(int num) {
            if (num >= 0) {
                return $"+{num}";
            }
            return $"{num}";
        }

        public static string FilenameToMapName(string filename) {
            filename = filename.Substring(0, filename.Length - 4);

            // From Github User Binary Worrier: https://stackoverflow.com/a/272929
            if (string.IsNullOrWhiteSpace(filename))
                return "";
            StringBuilder newText = new StringBuilder(filename.Length * 2);
            newText.Append(filename[0]);
            for (int i = 1; i < filename.Length; i++) {
                if (char.IsUpper(filename[i]) && filename[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(filename[i]);
            }
            string output = newText.ToString();
            // This part was added on by me
            if (output.EndsWith(".png")) {
                output = output.Substring(0, output.Length - 4);
            }
            if (output.EndsWith("256")) {
                output = output.Substring(0, output.Length - 3);
            }
            return output;
        }

        public static bool IsFlanking(Creature flanker, Creature flankee) {
            int num1 = flankee.Occupies.X - flanker.Occupies.X;
            int num2 = flankee.Occupies.Y - flanker.Occupies.Y;
            int num3 = Math.Abs(num1);
            int num4 = Math.Abs(num2);
            if (num3 > 2 || num4 > 2)
                return false;
            bool flag = num3 <= 1 && num4 <= 1;
            if (num3 != num4 && num3 != 0 && num4 != 0)
                return FlanksWith(flanker, num1 <= 1 && num2 <= 1, flankee.Occupies.X + num1, flankee.Occupies.Y + num2);
            int num5 = Math.Sign(num1);
            int num6 = Math.Sign(num2);
            return FlanksWith(flanker, true, flankee.Occupies.X + num5, flankee.Occupies.Y + num6) || FlanksWith(flanker, false, flankee.Occupies.X + num5 * 2, flankee.Occupies.Y + num6 * 2);
        }

        private static bool FlanksWith(Creature flanker, bool adjacent, int otherX, int otherY) {
            Creature primaryOccupant = flanker.Battle.Map.GetTile(otherX, otherY)?.PrimaryOccupant;
            return primaryOccupant != null && primaryOccupant.FriendOf(flanker) && (adjacent || primaryOccupant.WieldsItem(Trait.Reach)) && primaryOccupant.Actions.CanTakeActions();
        }

    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public record WandIllustration(Illustration NewMain, Illustration Center) : ScrollIllustration(IllustrationName.None, Center) {
        public override string IllustrationAsIconString => Center.IllustrationAsIconString;

        public override void DrawImage(Rectangle rectangle, Color? color, bool scale, bool scaleUp, Color? scaleBgColor) {
            int num = rectangle.Width / 6;
            int num2 = rectangle.Height / 6;
            //NewMain.DrawImage(new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            // Primitives.DrawImage(Assets.TextureFromName(Main), rectangle, color, scale, scaleUp, scaleBgColor);

            Primitives.DrawImage(NewMain, rectangle: new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImage(Center, rectangle, color, scale, scaleUp, scaleBgColor);

            //Primitives.DrawImage(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            //Primitives.DrawImage(rectangle: new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), illustration: Center, color: color, scale: scale, scaleUp: scaleUp, scaleBgColor: scaleBgColor);

        }

        public override void DrawImageNative(Rectangle rectangle, Color? color, bool scale, bool scaleUp, Color? scaleBgColor) {
            int num = rectangle.Width / 6;
            int num2 = rectangle.Height / 6;
            //NewMain.DrawImage(new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            //Primitives.DrawImageNative(Assets.TextureFromName(Main), rectangle, color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImageNative(NewMain, new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImageNative(Center, rectangle, color, scale, scaleUp, scaleBgColor);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public record DualIllustration(Illustration NewMain, Illustration Center) : ScrollIllustration(IllustrationName.None, Center) {
        public override string IllustrationAsIconString => NewMain.IllustrationAsIconString;

        public override void DrawImage(Rectangle rectangle, Color? color, bool scale, bool scaleUp, Color? scaleBgColor) {
            int num = rectangle.Width / 6;
            int num2 = rectangle.Height / 6;
            //NewMain.DrawImage(new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            // Primitives.DrawImage(Assets.TextureFromName(Main), rectangle, color, scale, scaleUp, scaleBgColor);

            Primitives.DrawImage(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImage(Center, rectangle: new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);

            //Primitives.DrawImage(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            //Primitives.DrawImage(rectangle: new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), illustration: Center, color: color, scale: scale, scaleUp: scaleUp, scaleBgColor: scaleBgColor);

        }

        public override void DrawImageNative(Rectangle rectangle, Color? color, bool scale, bool scaleUp, Color? scaleBgColor) {
            int num = rectangle.Width / 6;
            int num2 = rectangle.Height / 6;
            //NewMain.DrawImage(new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            //Primitives.DrawImageNative(Assets.TextureFromName(Main), rectangle, color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImageNative(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImageNative(Center, new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public record SameSizeDualIllustration(Illustration NewMain, Illustration Center) : ScrollIllustration(IllustrationName.None, Center) {
        public override string IllustrationAsIconString => NewMain.IllustrationAsIconString;

        public override void DrawImage(Rectangle rectangle, Color? color, bool scale, bool scaleUp, Color? scaleBgColor) {
            //NewMain.DrawImage(new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), color, scale, scaleUp, scaleBgColor);
            // Primitives.DrawImage(Assets.TextureFromName(Main), rectangle, color, scale, scaleUp, scaleBgColor);

            Primitives.DrawImage(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImage(Center, rectangle, color, scale, scaleUp, scaleBgColor);

            //Primitives.DrawImage(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            //Primitives.DrawImage(rectangle: new Rectangle(rectangle.X + num, rectangle.Y + num2, rectangle.Width - 2 * num, rectangle.Height - 2 * num2), illustration: Center, color: color, scale: scale, scaleUp: scaleUp, scaleBgColor: scaleBgColor);

        }

        public override void DrawImageNative(Rectangle rectangle, Color? color, bool scale, bool scaleUp, Color? scaleBgColor) {
            Primitives.DrawImageNative(NewMain, rectangle, color, scale, scaleUp, scaleBgColor);
            Primitives.DrawImageNative(Center, rectangle, color, scale, scaleUp, scaleBgColor);
        }
    }
}
