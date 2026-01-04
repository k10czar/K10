using UnityEngine;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class FakePlayerLog : IService
{
    private string _basePath;
    private int _logsPendingWriting;
    private StringBuilder _stringBuilder = new();

    private Task _currentAwaitTask = null;    
    private int _semaphoreIdCount;
    private Semaphore _writeToFileSemaphore = new();
    
    private const int MAX_LOGS_PENDING_WRITING = 200;
    public const string PLAYER_LOG_FILE_NAME = "Player.log";
    public const string PREV_PLAYER_LOG_FILE_NAME = "Player-prev.log";

    private static string PlayerLogPath(string basePath) => Path.Join(basePath, PLAYER_LOG_FILE_NAME);
    private static string PrevPlayerLogPath(string basePath) => Path.Join(basePath, PREV_PLAYER_LOG_FILE_NAME);

    public bool HasPendingLogs => _logsPendingWriting > 0;
    public bool IsWritingToFile => !_writeToFileSemaphore.Free;

    public FakePlayerLog(string basePath)
    {
        _basePath = basePath;
        FileAdapter.RequestDirectory(_basePath);

        MoveToPrevPlayerLog();

        ApplicationEventsRelay.IsSuspended.RegisterOnFalse(OnResume);
        ApplicationEventsRelay.IsSuspended.RegisterOnTrue(OnSuspend);

        Debug.Log($"FPL: Started FakePlayerLog with base path: {basePath}");
    }

    private void OnResume() => Application.logMessageReceived += OnLogReceived;
    private void OnSuspend()
    {
        Application.logMessageReceived -= OnLogReceived;
        WriteCompleteLogToFile();
    }

    public static void Clean( string basePath )
    {
        FileAdapter.Delete( PlayerLogPath(basePath) );
        FileAdapter.Delete( PrevPlayerLogPath(basePath) );
    }

    private void MoveToPrevPlayerLog()
    {
        if (!FileAdapter.Exists(PlayerLogPath(_basePath)))
            return;

        File.Copy(PlayerLogPath(_basePath), PrevPlayerLogPath(_basePath), true);
        FileAdapter.Delete(PlayerLogPath(_basePath));
        // Debug.Log($"FPL: Moved {PlayerLogPath} to {PREV_PLAYER_LOG_FILE_NAME}");
    }

    private void OnLogReceived(string logString, string stackTrace, LogType type)
    {
        // Debug.Log($"FPL: Received log {_logsPendingWriting}: {logString}");

        string formattedLogString;
        switch (type)
        {
            case LogType.Exception:
            case LogType.Error:
                formattedLogString = $"!@#Error: {logString}";
                break;

            case LogType.Warning:
            default:
                formattedLogString = logString;
                break;
        }

        string logMessage = $"{formattedLogString}\n{stackTrace}\n";
        _stringBuilder.Append(logMessage);

        if (++_logsPendingWriting >= MAX_LOGS_PENDING_WRITING)
            WriteCompleteLogToFile();
    }

    public async Task WriteCompleteLogToFile()
    {
        // Debug.Log($"FPL: Called function with {_semaphoreIdCount}");
        if (!_writeToFileSemaphore.Free)
        {
            // TODO: the last logs will be missing in this case
            // Debug.Log($"FPL: Waiting... {Time.frameCount}");
            await _currentAwaitTask;
            return;
        }

        if (!HasPendingLogs)
            return;

        string semaphoreId = $"Write_{_semaphoreIdCount++}";
        // Debug.Log($"FPL: Locking {semaphoreId}");
        _writeToFileSemaphore.Block(semaphoreId);

        string stringToWrite = _stringBuilder.ToString();
        _stringBuilder.Clear();
        _logsPendingWriting = 0;

        // Debug.Log($"FPL: Writing... {Time.unscaledTime}");
        _currentAwaitTask = File.AppendAllTextAsync(PlayerLogPath(_basePath), stringToWrite);
        await _currentAwaitTask;
        // await Task.Delay(3000);
        // Debug.Log($"FPL: ...written {Time.unscaledTime}");

        // Debug.Log($"FPL: Unlocking {semaphoreId}");
        _writeToFileSemaphore.Release(semaphoreId);
    }
}
