using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Campaign.Encounters.Evil_from_the_Stars;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using System;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.BossFights
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class Boss_Hydra : BossFightEncounter {
        public Boss_Hydra(string filename) : base("Vault of the Hydra", filename) {
            // Run setup
            this.AddTrigger(TriggerName.StartOfEncounter, async battle => {
                var loot = new Item[] {
                    // Alchemist
                    Items.CreateNew(ItemName.AlchemistsFire).WithModification(ItemModification.Create("moderate-bomb")),
                    Items.CreateNew(ItemName.AlchemistsFire).WithModification(ItemModification.Create("moderate-bomb")),
                    Items.CreateNew(ItemName.AcidFlask).WithModification(ItemModification.Create("moderate-bomb")),

                    // Thief
                    Items.CreateNew(ItemName.Sickle).WithModificationRune(ItemName.CorrosiveRunestone).WithModificationRune(ItemName.WeaponPotencyRunestone).WithModificationRune(ItemName.StrikingRunestone),
                    Items.CreateNew(ItemName.SpellScroll).WithModification(new ItemModification(ItemModificationKind.SpellScroll) {
                        SpellId = SpellId.RoaringApplause,
                        HeightenedToSpellLevel = 3
                    }),
                    Items.CreateNew(ItemName.SpellScroll).WithModification(new ItemModification(ItemModificationKind.SpellScroll) {
                        SpellId = SpellId.AcidArrow,
                        HeightenedToSpellLevel = 2
                    }),

                    // Ranger
                    Items.CreateNew(ItemName.Crossbow).WithModificationRune(ItemName.FlamingRunestone).WithModificationRune(ItemName.WeaponPotencyRunestone).WithModificationRune(ItemName.StrikingRunestone),

                    // Fighter
                    Items.CreateNew(ItemName.Glaive).WithModificationRune(ItemName.CorrosiveRunestone).WithModificationRune(ItemName.WeaponPotencyRunestone).WithModificationRune(ItemName.StrikingRunestone),
                    Items.CreateNew(ItemName.AcidFlask).WithModification(ItemModification.Create("moderate-bomb")),

                    // Alchemist 2
                    Items.CreateNew(ItemName.AlchemistsFire).WithModification(ItemModification.Create("moderate-bomb")),
                    Items.CreateNew(ItemName.AcidFlask).WithModification(ItemModification.Create("moderate-bomb")),
                    Items.CreateNew(ItemName.AcidFlask).WithModification(ItemModification.Create("moderate-bomb")),
                };

                foreach (Item item in loot) {
                    item.Traits.Add(Trait.EncounterEphemeral);
                }

                battle.Map.AllTiles.FirstOrDefault(t => t.X == 12 && t.Y == 10)?.DroppedItems.AddRange([
                    loot[0], loot[1], loot[2], loot[4]
                ]);
                battle.Map.AllTiles.FirstOrDefault(t => t.X == 3 && t.Y == 14)?.DroppedItems.AddRange([
                    loot[3], loot[5]
                ]); ;
                battle.Map.AllTiles.FirstOrDefault(t => t.X == 1 && t.Y == 3)?.DroppedItems.AddRange([
                    loot[6]
                ]);
                battle.Map.AllTiles.FirstOrDefault(t => t.X == 9 && t.Y == 1)?.DroppedItems.AddRange([
                    loot[7], loot[8]
                ]);
                battle.Map.AllTiles.FirstOrDefault(t => t.X == 13 && t.Y == 6)?.DroppedItems.AddRange([
                    loot[9], loot[10], loot[11]
                ]);
            });
        }
    }
}
