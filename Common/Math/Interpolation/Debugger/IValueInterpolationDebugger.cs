public interface IValueInterpolationDebugger<T>
{
    void Debug( double time, IValueInterpolationDataAccess<T> interpolationData );
}
