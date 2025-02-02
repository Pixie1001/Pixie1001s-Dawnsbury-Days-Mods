using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.Core.Tiles;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Audio;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Encounters.Level2
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal class AlchemicalAmbushLv2 : Level2Encounter
    {
        public AlchemicalAmbushLv2(string filename) : base("Alchemical Ambush", filename)
        {
            var itemList = Items.ShopItems.Where(item => (item.Level == CharacterLevel || item.Level == CharacterLevel + 1) && (item.HasTrait(Trait.Elixir) || item.HasTrait(Trait.Potion) || item.HasTrait(Trait.Bomb))).ToList();

            if (itemList.Count > 0)
            {
                Rewards.Add(itemList[Random.Shared.Next(0, itemList.Count)]);
                Rewards.Add(itemList[Random.Shared.Next(0, itemList.Count)]);
            }
            else
            {
                //Fail case if there are no elixirs in the level range.
                var elixir = Items.ShopItems.FirstOrDefault(item => item.HasTrait(Trait.Elixir));

                if (elixir != null)
                {
                    Rewards.Add(elixir);
                    Rewards.Add(elixir);
                }
            }
        }

        public override Creature CreateChest(Tile tile)
        {
            return Creature.CreateIndestructibleObject(IllustrationName.Chest256, "Chest", 0).WithSpawnAsGaia().WithCreatureId(CreatureId.Chest)
                .AddQEffect(new QEffect("Openable", "An adjacent creature can open this chest.").AddAllowActionOnSelf(QEffectId.OpenAChest, QEffectId.Chest, (Creature user) => new ActionPossibility(new CombatAction(user, IllustrationName.Chest256, "Open", new Trait[3]
                {
                Trait.Manipulate,
                Trait.Basic,
                Trait.BypassesOutOfCombat
                }, "Open a chest and collect its contents.", Target.Touch()).WithEffectOnEachTarget(async delegate (CombatAction spell, Creature caster, Creature target, CheckResult result)
                {
                    target.RemoveAllQEffects((QEffect qf) => qf.Id == QEffectId.Chest);

                    Sfxs.Play(SfxName.OpenChest);
                    Sfxs.Play(SfxName.ItemGet);
                    target.Illustration = IllustrationName.ChestOpen;

                    var itemList = Items.ShopItems.Where(item => (item.Level == CharacterLevel || item.Level == CharacterLevel + 1) && (item.HasTrait(Trait.Elixir) || item.HasTrait(Trait.Potion) || item.HasTrait(Trait.Bomb))).ToList();

                    if (itemList.Count > 0)
                    {
                        caster.AddHeldItem(itemList[Random.Shared.Next(0, itemList.Count)]);
                    }
                    else
                    {
                        //Fail case if there are no elixirs in the level range.
                        var elixir = Items.ShopItems.FirstOrDefault(item => item.HasTrait(Trait.Elixir));

                        if (elixir != null)
                        {
                            caster.AddHeldItem(elixir);
                        }
                    }
                })).WithPossibilityGroup("Interactions"), (Creature a, Creature d) => (!a.HasFreeHand) ? Usability.CommonReasons.YouMustHaveAFreeHandToOpenAChest : Usability.Usable));
        }
    }
}
