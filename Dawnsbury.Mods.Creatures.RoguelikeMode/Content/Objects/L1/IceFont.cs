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
using Dawnsbury.Core.StatBlocks;
using Dawnsbury.Mods.Creatures.RoguelikeMode.FunctionLibs;
using Dawnsbury.Mods.Creatures.RoguelikeMode.Ids;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.Creatures {
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class IceFont {
        public static Creature Create() {
            Creature hazard = MonsterStatBlocks.CreateElementalFont("Ice", IllustrationName.FontOfIce, "Ice Mephit");
            hazard.RemoveAllQEffects(qf => qf.Name == "Summoning Font");
            hazard.AddQEffect(new QEffect("Summoning Font", "At the beginning of your turn, summon " + ("Ice Mephit").WithIndefiniteArticle() + ". Then you can't summon mephits for 1d4 rounds.") {
                StartOfYourPrimaryTurn = async (QEffect qfSelf, Creature self) => {
                    if (!self.QEffects.Any((QEffect qf) => qf.Id == QEffectId.Recharging)) {
                        Creature creature = MonsterStatBlocks.CreateIceMephit();
                        if (self.Level <= 0) {
                            creature.ApplyWeakAdjustments(false);
                        } else if (self.Level >= 3) {
                            creature.ApplyEliteAdjustments();
                        }
                        self.Battle.SpawnCreature(creature, self.OwningFaction, self.Occupies);
                        self.AddQEffect(QEffect.Recharging("Summon"));
                    }
                }
            });

            return hazard;
        }
    }
}