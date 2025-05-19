using Dawnsbury.Campaign.Path;
using Dawnsbury.Core.Mechanics.Treasure;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content.CrossRunEffects;

[Serializable]
public class GrantItem : ACrossRunEffect
{
	private ItemName _itemName;
	
	public GrantItem(EffectDuration duration, ItemName itemName) : base(duration)
	{
		_itemName = itemName;
	}

	public override void ApplyEffect()
	{
		CampaignState.Instance?.CommonLoot.Add(Items.CreateNew(_itemName));
	}
}