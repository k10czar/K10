using System.Collections.Generic;

public interface IFinder<T, K>
{
    IEnumerator<K> Find(T t);
}
