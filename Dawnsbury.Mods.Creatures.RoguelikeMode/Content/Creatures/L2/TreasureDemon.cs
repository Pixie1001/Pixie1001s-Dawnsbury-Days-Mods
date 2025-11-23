using Dawnsbury.Audio;
using Dawnsbury.Auxiliary;
using Dawnsbury.Campaign.Encounters;
using Dawnsbury.Core;
using Dawnsbury.Core.Animations;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Coroutines;
using Dawnsbury.Core.Coroutines.Options;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class TreasureDemon {
        public static Creature Create(Encounter? encounter)
        {
            var encounterLevel = UtilityFunctions.GetEncounterLevel(encounter);
            bool highLevel = encounterLevel > 4;
            var hp = !highLevel ? 25 : 50;
            var defenses = !highLevel ? new Defenses(17, 5, 11, 8) : new Defenses(22, 10, 16, 13);
            var level = !highLevel ? 2 : 6;
            var skills = !highLevel ? new Skills(acrobatics: 8, thievery: 10) : new Skills(acrobatics: 14, thievery: 16);
            var abilities = !highLevel ? new Abilities(-1, 4, 0, 3, 1, -1) : new Abilities(-1, 5, 1, 4, 2, -1);
            var countdown = !highLevel ? 4 : 4;
            var perception = !highLevel ? 8 : 12;

            return new Creature(Illustrations.TreasureDemon, "Treasure Demon", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Fiend, Trait.Demon, Trait.NoPhysicalUnarmedAttack, Trait.NonSummonable }, level, perception, 5, defenses, hp, abilities, skills)
            .WithAIModification(ai => {
                ai.OverrideDecision = (self, options) => {
                    Creature monster = self.Self;
                    if (monster.HasEffect(QEffectId.Prone)) {
                        return options.Where(opt => opt.AiUsefulness.ObjectiveAction?.Action.ActionId == ActionId.Stand).FirstOrDefault();
                    }
                    if (monster.HasEffect(QEffectId.Grabbed)) {
                        return options.Where(opt => opt.AiUsefulness.ObjectiveAction?.Action.ActionId == ActionId.Escape).FirstOrDefault();
                    }
                    List<Creature> allEnemies = monster.Battle.AllCreatures.Where<Creature>((Func<Creature, bool>)(cr => cr.EnemyOf(monster))).ToList<Creature>();
                    List<TileOption> list1 = options.Where<Option>((Func<Option, bool>)(opt => opt is TileOption tileOption5 && tileOption5.OptionKind == OptionKind.MoveHere)).Cast<TileOption>().ToList<TileOption>();
                    if (list1.Count <= 0 || allEnemies.Count <= 0)
                        return options.FirstOrDefault(opt => opt.AiUsefulness.ObjectiveAction?.Action.ActionId == ActionId.EndTurn);
                    TileOption tileOption6 = list1.MaxBy<TileOption, int>((Func<TileOption, int>)(movementOption => allEnemies.Sum<Creature>((Func<Creature, int>)(enemy => movementOption.Tile.DistanceTo(enemy.Occupies)))));
                    if (tileOption6 != null) {
                        return tileOption6;
                    }
                    return null;
                };
            })
            .AddQEffect(QEffect.DamageWeakness(Trait.ColdIron, !highLevel ? 3 : 5))
            .WithBasicCharacteristics()
            .AddQEffect(new QEffect("Treasure Hoarder",
            $"Treasure Demons hop between dimensions, often travelling through the safety of the {Loader.UnderdarkName} to endow the demon lord's mortal servants with funds for their foul schemes. Kill it before it escapes to steal its delivery for yourselves."))
            .AddQEffect(new QEffect("Emergency Planeshift", "When this condition expires, the treasure demon will teleport to safety along with its loot.", ExpirationCondition.CountsDownAtEndOfYourTurn, null, IllustrationName.DimensionDoor) {
                Value = countdown,
                WhenExpires = async self => {
                    if (self.Value == 0) {
                        self.Owner.Battle.SmartCenter(self.Owner.Occupies.X, self.Owner.Occupies.Y);
                        self.Owner.Overhead($"Escaped!", Color.Black, "The treasure demon escaped with its loot!");
                        self.Owner.AnimationData.ColorBlink(Color.White);
                        Sfxs.Play(SfxName.SpellFail);
                        self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                    }
                },
                WhenCreatureDiesAtStateCheckAsync = async self => {
                    if (self.Owner.Battle.CampaignState == null) {
                        return;
                    }
                    int amount = 0;
                    for (int i = 0; i < self.Owner.Battle.Encounter.CharacterLevel; i++) {
                        amount += R.NextD20();
                        if (i > 4) amount += R.NextD20();
                    }
                    self.Owner.Overhead($"{amount} gold", Color.Goldenrod, "The party looted {b}" + amount + " gold{/b} from the treasure demon.");
                    self.Tag = amount;
                },
                EndOfCombat = async (self, victory) => {
                    if (victory && self.Tag != null && self.Owner.Battle.CampaignState != null) {
                        self.Owner.Battle.CampaignState.CommonGold += (int)self.Tag;
                    }
                }
            });
        }
    }
}