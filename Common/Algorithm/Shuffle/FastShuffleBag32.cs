

public struct FastShuffleBag32
{
	public const int MAX_ID = 31;

	int _mask;
	int _count;

    public int Count => _count;

	public void Add(int id)
    {
        var elementMask = 1 << id;
        if ((_mask & elementMask) != 0) return;
        _mask |= elementMask;
        _count++;
    }

	public void Remove(int id)
	{
		var elementMask = 1 << id;
		if ((_mask & elementMask) == 0) return;
		_mask &= ~elementMask;
		_count--;
	}

    public int SearchElementAt(int id)
    {
        if (_count == 0) return -1;

        int it = 0;

        for (int i = 0; i <= id; i++, it++)
        {
            while (it < MAX_ID && ((_mask & (1 << it)) == 0)) it++;
        }

        return it;
    }

	public int SortID()
    {
        if (_count == 0) return -1;

        int randomIndex = UnityEngine.Random.Range(0, _count);
        return SearchElementAt(randomIndex);
    }

    public int PopID()
    {
        var id = SortID();
        if (id < 0) return id;
        Remove(id);
        return id;
    }
}