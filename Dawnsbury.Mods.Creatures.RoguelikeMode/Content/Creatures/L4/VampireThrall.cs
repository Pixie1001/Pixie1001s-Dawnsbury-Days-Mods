using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Path;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.Animations.AuraAnimations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Mechanics.Zoning;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Text;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class VampireThrall {
        public static Creature Create() {
            var weapons = new Item[] {
                Items.CreateNew(ItemName.Spear),
                Items.CreateNew(ItemName.Halberd),
                Items.CreateNew(ItemName.Dagger),
                Items.CreateNew(ItemName.Club),
                Items.CreateNew(ItemName.Longsword),
                Items.CreateNew(ItemName.Maul)
            };

            return new Creature(IllustrationName.UnknownCreature, "Vampire Thrall", [Trait.Humanoid, ModTraits.MeleeMutator], level: 4, perception: 8, speed: 6, new Defenses(18, 8, 8, 8), hp: 40, new Abilities(3, 2, 2, -1, 2, 0), new Skills(athletics: 9, acrobatics: 8))
                .WithBasicCharacteristics()
                .WithCreatureId(CreatureIds.VampireThrall)
                .WithProficiency(Trait.Weapon, Proficiency.Expert)
                .AddHeldItem(Items.CreateNew(ItemName.Dagger).WithAdditionalWeaponProperties(wp => wp.DamageDieCount = 2))
                .AddQEffect(new QEffect() {
                    BonusToAttackRolls = (_, ca, _) => ca.HasTrait(Trait.Strike) ? new Bonus(2, BonusType.Status, "Heedless strength") : null,
                    StartOfCombat = async (self) => {
                        // Grant random weapon
                        int seed = CampaignState.Instance != null && CampaignState.Instance.Tags.TryGetValue("seed", out string result) ? Int32.TryParse(result, out int r2) ? r2 : R.Next(1000) : R.Next(1000);
                        seed += CampaignState.Instance?.CurrentStopIndex != null ? CampaignState.Instance.CurrentStopIndex : 0;
                        seed += self.Owner.Battle.Map.AllTiles.IndexOf(self.Owner.Space.CenterTile);

                        Random rand = new Random(seed);

                        self.Owner.HeldItems.Clear();
                        var weapon = weapons[rand.Next(0, weapons.Count())];
                        self.Owner.HeldItems.Add(weapon.WithAdditionalWeaponProperties(wp => wp.DamageDieCount = 2));
                        if (!weapon.HasTrait(Trait.TwoHanded) && rand.Next(0, 2) == 0) {
                            self.Owner.HeldItems.Add(Items.CreateNew(ItemName.SteelShield));
                        }
                    }
                })
                .Builder
                .Done();
        }
    }
}