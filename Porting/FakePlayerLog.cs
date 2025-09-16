using UnityEngine;
using System.IO;
using System.Text;

public class FakePlayerLog : IService
{
    private string _basePath;
    private int _logsPendingWriting;
    private StringBuilder _stringBuilder = new();
    private const int MAX_LOGS_PENDING_WRITING = 100;
    private const string PLAYER_LOG_FILE_NAME = "Player.log";
    private const string PREV_PLAYER_LOG_FILE_NAME = "Player-prev.log";
    private string PlayerLogPath => Path.Join(_basePath, PLAYER_LOG_FILE_NAME);
    private string PrevPlayerLogPath => Path.Join(_basePath, PREV_PLAYER_LOG_FILE_NAME);


    public bool HasPendingLogs => _logsPendingWriting > 0;

    public FakePlayerLog(string basePath)
    {
        _basePath = basePath;
        FileAdapter.RequestDirectory(_basePath);

        MoveToPrevPlayerLog();

        ApplicationEventsRelay.IsSuspended.RegisterOnFalse(OnResume);
        ApplicationEventsRelay.IsSuspended.RegisterOnTrue(OnSuspend);

        Debug.Log($":::><::: Started FakePlayerLog with base path: {basePath}");
    }

    private void OnResume() => Application.logMessageReceived += OnLogReceived;
    private void OnSuspend()
    {
        Application.logMessageReceived -= OnLogReceived;
        WriteToFile();
    }


    private void MoveToPrevPlayerLog()
    {
        if (!FileAdapter.Exists(PlayerLogPath))
            return;

        File.Copy(PlayerLogPath, PrevPlayerLogPath, true);
        FileAdapter.Delete(PlayerLogPath);
        // Debug.Log($":::><::: Moved {PlayerLogPath} to {PREV_PLAYER_LOG_FILE_NAME}");
    }

    private void OnLogReceived(string logString, string stackTrace, LogType type)
    {
        // Debug.Log($":::><::: Received log {_logsPendingWriting}: {logString}");

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
            WriteToFile();
    }

    // TODO: Is Async Needed?
    public void WriteToFile()
    {
        Debug.Log($":::><::: WritingToFile {_logsPendingWriting} files: {PlayerLogPath}");
        if (!HasPendingLogs)
            return;

        File.AppendAllText(PlayerLogPath, _stringBuilder.ToString());
        _stringBuilder.Clear();
        _logsPendingWriting = 0;

    }
}
