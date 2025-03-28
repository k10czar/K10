
public interface IValueInterpolationDataAccess<T>
{
    int BufferSize { get; }
    int CurrentPackageId { get; }
    T From( double time );
    public double GetPackageTime( int index );
    public T GetPackageValue( int index );
}
