public interface IPlayerPrefsAdapter
{
    bool HasKey(string key);
    int GetInt(string key, int defaultValue = default);
    float GetFloat(string key, float defaultValue = default);
    string GetString(string key, string defaultValue = default);
    void SetInt(string key, int value);
    void SetFloat(string key, float value);
    void SetString(string key, string value);
    void Load();
    void Save();
    void DeleteAll();
    void DeleteKey(string key);
}

public static class PlayerPrefsAdapter
{
	private static IPlayerPrefsAdapter _implementation = new DefaultPlayerPrefs();
	
	public static void SetImplementation(IPlayerPrefsAdapter implementation) { _implementation = implementation; }
	
	public static bool HasKey(string key) => _implementation.HasKey(key);
    public static int GetInt(string key, int defaultValue = default) => _implementation.GetInt(key, defaultValue);
    public static float GetFloat(string key, float defaultValue = default) => _implementation.GetFloat(key, defaultValue);
    public static string GetString(string key, string defaultValue = default) => _implementation.GetString(key);
    public static void SetInt(string key, int value) => _implementation.SetInt(key, value);
    public static void SetFloat(string key, float value) => _implementation.SetFloat(key, value);
    public static void SetString(string key, string value) => _implementation.SetString(key, value);
    public static void Load() => _implementation.Load();
    public static void Save() => _implementation.Save();
    public static void DeleteAll() => _implementation.DeleteAll();
    public static void DeleteKey(string key) => _implementation.DeleteKey(key);
}
