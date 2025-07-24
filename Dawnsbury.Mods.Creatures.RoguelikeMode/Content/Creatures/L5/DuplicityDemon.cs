using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
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
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Display.Notifications;
using Dawnsbury.Modding;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Tables;
using Microsoft.Xna.Framework;
using System;
using System.Xml.Linq;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class DuplicityDemon {
        public static Creature Create() {
            Item legAtk = new Item(new SpiderIllustration(Illustrations.StabbingAppendage, IllustrationName.DragonClaws), "stabbing appendage", new Trait[] { Trait.Unarmed, Trait.Finesse, Trait.Agile, Trait.DeadlyD6 }).WithWeaponProperties(new WeaponProperties("1d6", DamageKind.Piercing));

            Creature monster = new Creature(new SpiderIllustration(Illustrations.DuplicityDemon, Illustrations.Bear4), "Duplicity Demon",
                [Trait.Chaotic, Trait.Evil, Trait.Demon, Trait.Fiend, ModTraits.Spider, ModTraits.MeleeMutator],
                5, 12, 8, new Defenses(21, 9, 15, 14), 75,
            new Abilities(3, 6, 2, 4, 2, 6),
            new Skills(acrobatics: 13, stealth: 13, deception: 16))
            .WithCreatureId(CreatureIds.DuplicityDemon)
            .WithProficiency(Trait.Melee, Proficiency.Expert)
            .WithBasicCharacteristics()
            .WithUnarmedStrike(legAtk)
            .AddQEffect(QEffect.DamageWeakness(DamageKind.Good, 5))
            .AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, 5))
            .AddQEffect(new QEffect("Webwalk", "This creature moves through webs unimpeded.") { Id = QEffectId.IgnoresWeb })
            .AddQEffect(QEffect.WebSense())
            .AddQEffect(QEffect.PackAttack("duplicity demon", "1d8"))
            .AddQEffect(QEffect.SneakAttack("1d6"))
            .AddQEffect(new QEffect("Duplicate", "At the start of combat, the duplicity demon splits into 4 copies of itself, with slowed 1. Only one is real, the rest will be revealed as harmless illusions upon taking damage or after the real demon is damaged.") {
                StartOfCombat = async self => {
                    Duplicate(self.Owner);
                }
            })
            .Builder
            .AddMainAction(you => {
                return new CombatAction(you, IllustrationName.MirrorImage, "Duplicate", [Trait.Illusion], "The duplicity demon splits into 4 copies of itself, with slowed 1. Only one is real, the rest will be revealed as harmless illusions upon taking damage or after the real demon is damaged.",
                    Target.Self((caster, ai) => 10f))
                .WithActionCost(3)
                .WithSoundEffect(SfxName.Fear)
                .WithEffectOnSelf(async caster => {
                    Duplicate(caster);
                });
            })
            .Done();
            return monster;
        }

        private static void Duplicate(Creature monster) {
            monster.RemoveAllQEffects(qf => qf.Illustration != null);

            var duplicates = new List<Creature>() { monster };

            var freeTiles = monster.Battle.Map.AllTiles.Where(t => t.IsTrulyGenuinelyFreeToEveryCreature && t.DistanceTo(monster.Occupies) <= 3 && t.HasLineOfEffectToIgnoreLesser(monster.Occupies) != CoverKind.Blocked).ToList();
            for (int i = 0; i < 3; i++) {
                Tile tile = monster.Occupies;
                if (tile == null) return;
                if (freeTiles.Count > 0) {
                    tile = R.ChooseAtRandom(freeTiles.ToArray());
                    freeTiles.Remove(tile!);
                }
                var duplicate = Create();
                CopiableCharacteristics.CopyFromTo(monster, duplicate);
                duplicate.TakeDamage(monster.Damage);
                duplicates.Add(duplicate);
                monster.Battle.SpawnCreature(duplicate, monster.OwningFaction, tile!);
            }

            var origional = R.ChooseAtRandom(duplicates.ToArray());
            if (origional == null) return;
            foreach (Creature duplicate in duplicates.ToList()) {
                var effect = new QEffect("Duplicate",
                    "The duplicity demon is hiding within these duplicates. The duplicates are destroyed by upon taking damage. " +
                    "The real demon will instead reveal all other duplicates as fake when damaged. While maintaining these illusions, the duplicity demon and its copies are slowed 1.") {
                    Illustration = origional.Illustration,
                    StateCheck = self => {
                        self.Owner.AddQEffect(QEffect.Slowed(1).WithExpirationEphemeral());
                    }
                };
                if (duplicate == origional) {
                    effect.AfterYouTakeDamage = async (self, amount, kind, action, crit) => {
                        if (amount > 0) {
                            foreach (Creature d in duplicates.Where(cr => cr != self.Owner)) {
                                DuplicateDeath(d);
                            }
                        }
                    };

                    Action<QEffect>? lambda = self => {
                        if (!duplicates.Any(cr => !cr.Destroyed && cr != self.Owner)) {
                            self.ExpiresAt = ExpirationCondition.Immediately;
                        }
                    };
                    effect.StateCheck += lambda;
                }

                if (duplicate != origional) {
                    effect.AfterYouTakeDamage = async (self, amount, kind, action, crit) => {
                        if (amount > 0) {
                            DuplicateDeath(self.Owner);
                        }
                    };
                }
                duplicate.AddQEffect(effect);
            }

            //duplicates.Remove(origional);

        }

        private static void DuplicateDeath(Creature duplicate) {
            duplicate.Traits.Add(Trait.NoDeathOverhead);
            duplicate.Traits.Add(Trait.NoDeathScream);
            Sfxs.Play(SfxName.PhaseBolt);
            duplicate.Overhead("*disbelieved*", Color.Violet, duplicate.Name + " was revealed as an illusion!");
            duplicate.Battle.RemoveCreatureFromGame(duplicate);
        }
    }
}
