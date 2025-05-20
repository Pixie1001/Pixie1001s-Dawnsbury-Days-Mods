using Dawnsbury.Campaign.Path;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.IO;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content;

public class CrossRunEffectsManager
{
	[Serializable]
	public struct RoguelikeExtraData
	{
		public List<ItemName> TransferredItems;
	}
	
	private readonly string _filePath;
	private RoguelikeExtraData _extraData;
	
	public CrossRunEffectsManager(int profile)
	{
		_filePath = Path.Combine(LocalDataStore.StorageFolder, $"roguelike-extra-{profile}.json");
		_extraData = LocalDataStore.Load<RoguelikeExtraData>(_filePath);
	}

	public void AddTransferredItem(ItemName itemName)
	{
		_extraData.TransferredItems ??= new List<ItemName>();
		_extraData.TransferredItems.Add(itemName);
		Save();
	}

	public void TransferItems()
	{
		if (_extraData.TransferredItems == null)
			return;

		foreach (var item in _extraData.TransferredItems)
		{
			CampaignState.Instance?.CommonLoot.Add(Items.CreateNew(item));
		}
		
		_extraData.TransferredItems.Clear();
		Save();
	}

	private void Save()
	{
		LocalDataStore.Save(_filePath, _extraData);
	}
}