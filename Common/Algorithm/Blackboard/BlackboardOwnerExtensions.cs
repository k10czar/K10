

public interface IBlackboardOwner
{
    IBlackboardOwner ParentBlackboard { get; }
    IBlackboard Blackboard { get; }
    IBlackboard RequestBlackboard();
}

public static class BlackboardOwnerExtensions
{
    public static bool TryGet<T>( this IBlackboardOwner owner, string key, out T value) 
    { 
        if( owner == null || owner.Blackboard == null )
        {
            value = default;
            return false;
        }
        return owner.Blackboard.TryGet( key, out value );
    }

    public static bool TryGetOnHierarchy<T>( this IBlackboardOwner owner, string key, out T value) 
    { 
        var blackboard = owner;
        while( blackboard != null )
        {
            if( blackboard.TryGet( key, out value ) ) return true;
            blackboard = blackboard.ParentBlackboard;
        }
        value = default;
        return false;
    }

    public static T FindOrCreate<T>( this IBlackboardOwner owner, string key ) where T : new()
    { 
        var blackboard = owner;
        while( blackboard.ParentBlackboard != null )
        {
            if( blackboard.TryGet( key, out T found ) ) return found;
            blackboard = blackboard.ParentBlackboard;
        }
        return Request<T>( blackboard, key );
    }

    public static bool ContainsKey( this IBlackboardOwner owner, string key) 
    { 
        if( owner == null || owner.Blackboard == null ) return false;
        return owner.Blackboard.ContainsKey( key );
    }

    public static void Set<T>( this IBlackboardOwner owner, string key, T value) 
    {
        var blackboard = owner.RequestBlackboard();
        blackboard.Set( key, value );
    }

    public static T Request<T>( this IBlackboardOwner owner, string key ) where T : new()
    {
        var blackboard = owner.RequestBlackboard();
        if( blackboard.TryGet( key, out T value ) ) return value;
        var newValue = new T();
        blackboard.Set( key, newValue );
        return newValue;
    }

    public static bool Remove( this IBlackboardOwner owner, string key) 
    { 
        if( owner == null || owner.Blackboard == null ) return false;
        return owner.Blackboard.Remove( key );
    }
}
