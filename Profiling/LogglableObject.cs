using UnityEngine;

public interface LogglableObject<T> where T : IK10LogCategory, new()
{

}

public static class LogglableBehaviourExtentions
{
    public static void Log<T>( this LogglableObject<T> behaviour, string message, LogSeverity logSeverity = LogSeverity.Info ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, message, behaviour as MonoBehaviour );
    }

    public static void VerboseLog<T>( this LogglableObject<T> behaviour, string message, LogSeverity logSeverity = LogSeverity.Info ) where T : IK10LogCategory, new()
    {
        K10Log<T>.Log( logSeverity, message, behaviour as MonoBehaviour, true );
    }

    public static bool CanLog<T>( this LogglableObject<T> behaviour, bool verbose = false ) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebug<T>( verbose );
    }

    public static bool CanLogVisuals<T>( this LogglableObject<T> behaviour ) where T : IK10LogCategory, new()
    {
        return K10DebugSystem.CanDebugVisuals<T>();
    }
    
    public static bool SkipVisuals<T>( this LogglableObject<T> behaviour ) where T : IK10LogCategory, new()
    {
        
        return K10DebugSystem.SkipVisuals<T>();
    }
}
