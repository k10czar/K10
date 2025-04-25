using System.Collections.Generic;
using System.Linq;

public class CatalogedUniqueStock<Key, Value> : ICustomDisposableKill where Value : IObjectLifeState
{
	protected readonly Dictionary<Key, Value> entries = new();

	private EventSlot onEntriesChanged;
	public IEventRegister OnEntriesChanged => Lazy.Request(ref onEntriesChanged);

	public void AddEntry(Key key, Value t)
	{
		if (entries.ContainsKey(key)) entries.Remove(key);
		entries.Add(key, t);
		t.IsValid.RegisterOnFalse(() => RemoveEntry(key));
		onEntriesChanged?.Trigger();
	}

	public bool RemoveEntry(Key key)
	{
		var removed = entries.Remove(key);
		if (removed) onEntriesChanged?.Trigger();

		return removed;
	}

	public bool ContainsKey(Key key) => entries.ContainsKey(key);

	public Value GetEntry(Key key) => entries[key];

	public bool TryGetValue(Key key, out Value t) => entries.TryGetValue(key, out t);

	public void Clear() => entries?.Clear();

	public int Count => entries.Count;

	public void Kill()
	{
		entries?.Clear();
		onEntriesChanged?.Kill();
	}

	public override string ToString()
	{
		return $"[ {string.Join(", ", entries.ToList().ConvertAll((kvp) => $"({kvp.Key.ToStringOrNull()} => {kvp.Value.ToStringOrNull()})"))} ]";
	}
}