using Dawnsbury.IO;

namespace Dawnsbury.Mods.Creatures.RoguelikeMode.Content;

public class CrossRunEffectsManager
{
	[Serializable]
	private struct RoguelikeExtraData
	{
		public List<ACrossRunEffect> CrossRunEffects;
	}
	
	private readonly string _filePath;
	private RoguelikeExtraData _extraData;
	
	public CrossRunEffectsManager(int profile)
	{
		_filePath = Path.Combine(LocalDataStore.StorageFolder, $"roguelike-extra-{profile}.json");
		try
		{
			_extraData = LocalDataStore.Load<RoguelikeExtraData>(_filePath);
		}
		catch (Exception ex)
		{
			Eqatec.SendLog("EXCEPTION SUPPRESSED");
			Eqatec.SendException(ex);
		}
	}

	public void AddEffect(ACrossRunEffect effect)
	{
		_extraData.CrossRunEffects.Add(effect);
		Save();
	}

	public void ApplyEffects()
	{
		var previousCount = _extraData.CrossRunEffects.Count; 
		_extraData.CrossRunEffects = _extraData.CrossRunEffects.Where(effect =>
		{
			effect.ApplyEffect();
			return effect.Duration == ACrossRunEffect.EffectDuration.Permanent;
		}).ToList();
			
		if (previousCount != _extraData.CrossRunEffects.Count)
			Save();
	}

	private void Save()
	{
		try
		{
			LocalDataStore.Save(_filePath, _extraData);
		}
		catch (Exception ex)
		{
			Eqatec.SendLog("EXCEPTION SUPPRESSED");
			Eqatec.SendException(ex);
		}
	}
}

[Serializable]
public abstract class ACrossRunEffect
{
	public enum EffectDuration
	{
		NextRun,
		Permanent
	}
	public EffectDuration Duration { get; }
	
	protected ACrossRunEffect(EffectDuration effectDuration)
	{
		Duration = effectDuration;
	}
	
	public abstract void ApplyEffect();
}