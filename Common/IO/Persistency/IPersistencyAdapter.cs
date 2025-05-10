public interface IPersistencyAdapter<T>
{
    T Load();
    void Persists( T t );
}
