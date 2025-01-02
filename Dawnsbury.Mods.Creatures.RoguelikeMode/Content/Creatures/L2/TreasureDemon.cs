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
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;
using Microsoft.Xna.Framework;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures.L2
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class TreasureDemon
    {
        public static Creature Create()
        {
            return new Creature(Illustrations.TreasureDemon, "Treasure Demon", new List<Trait>() { Trait.Chaotic, Trait.Evil, Trait.Fiend, Trait.Demon, Trait.NoPhysicalUnarmedAttack }, 2, 8, 5, new Defenses(17, 5, 8, 11), 25,
            new Abilities(-1, 4, 0, 3, 1, -1), new Skills(acrobatics: 8, thievery: 10))
            .WithBasicCharacteristics()
            .AddQEffect(new QEffect("Treasure Hoarder",
            $"Treasure Demons hop between dimensions, often travelling through the safety of the {Loader.UnderdarkName} to endow the demon lord's mortal servants with funds for their foul schemes. Kill it before it escapes to steal its delivery for yourselves.")
            {
                Id = QEffectId.FleeingAllDanger
            })
            .AddQEffect(new QEffect("Emergency Planeshift", "When this condition expires, the treasure demon will teleport to safety along with its loot.", ExpirationCondition.CountsDownAtEndOfYourTurn, null, IllustrationName.DimensionDoor)
            {
                Value = 3,
                WhenExpires = async self =>
                {
                    if (self.Value == 0)
                    {
                        self.Owner.Battle.SmartCenter(self.Owner.Occupies.X, self.Owner.Occupies.Y);
                        self.Owner.Occupies.Overhead($"Escaped!", Color.Black, "The treasure demon escaped with its loot!");
                        self.Owner.AnimationData.ColorBlink(Color.White);
                        Sfxs.Play(SfxName.SpellFail);
                        self.Owner.Battle.RemoveCreatureFromGame(self.Owner);
                    }
                },
                WhenCreatureDiesAtStateCheckAsync = async self =>
                {
                    if (self.Owner.Battle.CampaignState == null)
                    {
                        return;
                    }
                    int amount = 0;
                    for (int i = 0; i < self.Owner.Battle.Encounter.CharacterLevel; i++)
                    {
                        amount += R.NextD20();
                    }
                    self.Owner.Occupies.Overhead($"{amount} gold", Color.Goldenrod, "The party looted {b}" + amount + " gold{/b} from the treasure demon.");
                    self.Tag = amount;
                },
                EndOfCombat = async (self, victory) =>
                {
                    if (victory && self.Tag != null)
                    {
                        self.Owner.Battle.CampaignState.CommonGold += (int)self.Tag;
                    }
                }
            });
        }
    }
}