using UnityEngine;

public class DefaultPlayerPrefs : IPlayerPrefsAdapter
{
    public bool HasKey(string key) => PlayerPrefs.HasKey(key);
    public int GetInt(string key, int defaultValue = default) => PlayerPrefs.GetInt(key, defaultValue);
    public float GetFloat(string key, float defaultValue = default) => PlayerPrefs.GetFloat(key, defaultValue);
    public string GetString(string key, string defaultValue = default)  => PlayerPrefs.GetString(key, defaultValue);
    public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
    public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
    public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);
    public void Load() {}
    public void Save() => PlayerPrefs.Save();
    public void DeleteAll() => PlayerPrefs.DeleteAll();
    public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
}
