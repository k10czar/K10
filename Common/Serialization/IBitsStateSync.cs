
public interface IBitsStateSync
{
    /// <summary>  Constant number of bits that sync all possible states of this object </summary>
    byte BitsToSync { get; }
    int Pack();
    void Unpack( int data );
    
    bool IsDirty { get; }
    void RemoveDirty();
}