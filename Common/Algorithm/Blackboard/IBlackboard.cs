using System.Collections.Generic;

public interface IBlackboard : IBlackboardQuery, IBlackboardEdit, IEnumerable<KeyValuePair<string, object>>
{
}

public interface IBlackboardQuery
{
    bool TryGet<T>(string key, out T value);
    bool ContainsKey(string key);
}

public interface IBlackboardEdit
{
    void Set<T>(string key, T value);
    bool Remove(string key);
}
