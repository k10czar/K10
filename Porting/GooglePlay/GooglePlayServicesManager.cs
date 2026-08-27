using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System;

public static class GooglePlayServicesManager
{
    public static bool IsAuthenticated { get; private set; } 
    private const string DefaultSaveSlot = "DefaultSavedGame";
    private static ISavedGameClient SavedGameClient => PlayGamesPlatform.Instance.SavedGame;

    public static void Authenticate(Action<SignInStatus> callback)
    {
        PlayGamesPlatform.Instance.Authenticate((SignInStatus status) => {
            if (status == SignInStatus.Success)
            {
                IsAuthenticated = true;
                NotificationConsole.Notify($"Sucessfully authenticated to Play Games");
                // Debug.Log($"Sucessfully authenticated to Play Games");
            }
            else
                NotificationConsole.Notify($"Failed to authenticate to Play Games: {status}");
                // Debug.LogError($"Failed to authenticate to Play Games: {status}");

            callback?.Invoke(status);
        });
    }

    public static void ManuallyAuthenticate(Action<SignInStatus> callback)
    {
        PlayGamesPlatform.Instance.ManuallyAuthenticate((SignInStatus status) => {
            if (status == SignInStatus.Success)
            {
                IsAuthenticated = true;
                NotificationConsole.Notify($"Sucessfully manually authenticated to Play Games");
                // Debug.Log($"Sucessfully manually authenticated to Play Games");
            }
            else
                NotificationConsole.Notify($"Failed to manually authenticate to Play Games: {status}");
                // Debug.LogError($"Failed to manually authenticate to Play Games: {status}");

            callback?.Invoke(status);
        });
    }

    private static void OpenDefaultSavedGame(Action<ISavedGameMetadata> callback) => OpenSavedGame(DefaultSaveSlot, callback);
    private static void OpenSavedGame(string saveSlot, Action<ISavedGameMetadata> callback)
    {
        SavedGameClient.OpenWithAutomaticConflictResolution(
            saveSlot, 
            DataSource.ReadCacheOrNetwork, 
            ConflictResolutionStrategy.UseLongestPlaytime, 
            (SavedGameRequestStatus status, ISavedGameMetadata savedGame) => {
                callback?.Invoke(savedGame);
            }
        ); 
    }

    public static void SaveToDefaultSlot(byte[] data, TimeSpan totalPlaytime, Action<SavedGameRequestStatus> callback = null)
    {
        OpenDefaultSavedGame((ISavedGameMetadata savedGame) => SaveGame(savedGame, data, totalPlaytime));
    }

    public static void SaveGame(string filename, byte[] data, TimeSpan totalPlaytime, Action<SavedGameRequestStatus> callback = null)
    {
        OpenSavedGame(filename, (ISavedGameMetadata savedGame) => SaveGame(savedGame, data, totalPlaytime));
    }

    private static void SaveGame(ISavedGameMetadata savedGame, byte[] savedData, TimeSpan totalPlaytime, Action<SavedGameRequestStatus> callback = null)
    {
        if (savedGame == null || !savedGame.IsOpen)
        {
            NotificationConsole.Notify($"Couldn't save because ISavedGameMetadata is not valid");
            // Debug.LogError($"Couldn't save because couldn't Open Saved Game");
            return;
        }

        SavedGameMetadataUpdate.Builder builder = new();
        builder = builder
            .WithUpdatedPlayedTime(totalPlaytime)
            .WithUpdatedDescription(GetSaveDescription());
        SavedGameMetadataUpdate updatedMetadata = builder.Build();
        
        SavedGameClient.CommitUpdate(savedGame, updatedMetadata, savedData, 
            (SavedGameRequestStatus status, ISavedGameMetadata game) => {
                if (status == SavedGameRequestStatus.Success)
                    NotificationConsole.Notify($"Sucessfully saved game to Play Games");
                    // Debug.Log($"Sucessfully saved game to Play Games");
                else
                    NotificationConsole.Notify($"Failed to save game to Play Games: {status}");
                    // Debug.LogError($"Failed to save game to Play Games: {status}");

                callback?.Invoke(status);
            }
        );
    }

    private static string GetSaveDescription()
    {
        return $"Last played on {DateTime.Now} on device {SystemInfo.deviceName} ({SystemInfo.deviceModel})";
    }

    public static void LoadFromDefaultSlot(Action<SavedGameRequestStatus, byte[]> callback)
    {
        OpenDefaultSavedGame((ISavedGameMetadata savedGame) => LoadGame(savedGame,callback));
    }

    public static void LoadGame(string saveSlot, Action<SavedGameRequestStatus, byte[]> callback)
    {
        OpenSavedGame(saveSlot, (ISavedGameMetadata savedGame) => LoadGame(savedGame, callback));
    }

    private static void LoadGame(ISavedGameMetadata savedGame, Action<SavedGameRequestStatus, byte[]> callback)
    {
        SavedGameClient.ReadBinaryData(savedGame, 
            (SavedGameRequestStatus status, byte[] data) => {
                if (status == SavedGameRequestStatus.Success)
                    NotificationConsole.Notify($"Sucessfully loaded game from Play Games");
                    // Debug.Log($"Sucessfully loaded game from Play Games");
                else
                    NotificationConsole.Notify($"Failed to load game from Play Games: {status}");
                    // Debug.LogError($"Failed to load game from Play Games: {status}");
                
                callback?.Invoke(status, data);
            }
        );
    }
}
