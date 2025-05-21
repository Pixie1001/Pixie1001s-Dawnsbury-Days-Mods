using Dawnsbury.Campaign.Path;
using Dawnsbury.Core.Mechanics.Treasure;
using Dawnsbury.IO;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content;

public class CrossRunEffectsManager
{
	[Serializable]
	public struct RoguelikeExtraData
	{
		public List<Item> TransferredItems;
	}
	
	private readonly string _filePath;
	private RoguelikeExtraData _extraData;
	
	public CrossRunEffectsManager(int profile)
	{
		_filePath = Path.Combine(LocalDataStore.StorageFolder, $"roguelike-extra-{profile}.json");
		_extraData = LocalDataStore.Load<RoguelikeExtraData>(_filePath);
	}

	public void AddTransferredItem(Item item)
	{
		_extraData.TransferredItems ??= new List<Item>();
		_extraData.TransferredItems.Add(item);
		Save();
	}

	public void TransferItems()
	{
		if (_extraData.TransferredItems == null)
			return;

		foreach (var item in _extraData.TransferredItems)
		{
			CampaignState.Instance?.CommonLoot.Add(item);
		}
		
		_extraData.TransferredItems.Clear();
		Save();
	}

	private void Save()
	{
		LocalDataStore.Save(_filePath, _extraData);
	}
}