using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Intelligence;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Content;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DemonicPustule {
        public static Creature Create() {
            Creature hazard = new Creature(Illustrations.DemonicPustule, "Demonic Pustule", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Object, Trait.Demon, Trait.Mindless }, 2, 0, 0, new Defenses(10, 10, 0, 0), 30, new Abilities(0, 0, 0, 0, 0, 0), new Skills())
            .WithTactics(Tactic.DoNothing)
            .AddQEffect(QEffect.TraitImmunity(Trait.Mental))
            .AddQEffect(new QEffect() {
                StateCheck = self => self.Owner.WeaknessAndResistance.AddWeakness(DamageKind.Good, 5)
            })
            .AddQEffect(new QEffect("Incubator",
            "The pustule is incubating a nascent demon, ready to emergy into our point of light. Destroy it before this condition reaches 0 and the demon emerges.", ExpirationCondition.CountsDownAtEndOfYourTurn, null, new SameSizeDualIllustration(Illustrations.StatusBackdrop, Illustrations.DemonicPustule)) {
                Value = 3,
                WhenExpires = async self => {
                    if (self.Value == 0) {
                        self.Owner.Battle.SmartCenter(self.Owner.Occupies.X, self.Owner.Occupies.Y);
                        self.Owner.Occupies.Overhead($"Hatched!", Color.DarkRed, $"{self.Owner.Name} hatches into a Bebilith Spawn!");
                        self.Owner.AnimationData.ColorBlink(Color.White);
                        Sfxs.Play(SoundEffects.EggHatch);
                        Tile pos = self.Owner.Occupies;
                        self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                        Creature spider = BebilithSpawn.Create();
                        if (self.Owner.Level <= 0) {
                            spider.ApplyWeakAdjustments(false, true);
                        } else if (self.Owner.Level == 1) {
                            spider.ApplyWeakAdjustments(false);
                        } else if (self.Owner.Level == 3) {
                            spider.ApplyEliteAdjustments();
                        } else if (self.Owner.Level >= 4) {
                            spider.ApplyEliteAdjustments(true);
                        }
                        self.Owner.Battle.SpawnCreature(spider, self.Owner.OwningFaction, pos);
                    }
                }
            });

            return hazard;
        }
    }
}