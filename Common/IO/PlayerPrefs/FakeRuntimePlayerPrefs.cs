using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class FakeRuntimePlayerPrefs : IPlayerPrefsAdapter
{
    private string _relativePath = "PlayerPrefs.sav";
    private string Path => $"{FileAdapter.persistentDataPath}/{_relativePath}";

    private Dictionary<string, object> _data = new();

    public FakeRuntimePlayerPrefs() {}
    public FakeRuntimePlayerPrefs(string relativePath)
    {
        _relativePath = relativePath;
    }

    public bool HasKey(string key) => _data.ContainsKey(key);
    public int GetInt(string key, int defaultValue = default) 
    {
        if (!HasKey(key)) return defaultValue;
        try { return (int)_data[key]; }
        catch (Exception) { return defaultValue; }
    }
    public float GetFloat(string key, float defaultValue = default) 
    {
        if (!HasKey(key)) return defaultValue;
        try { return (float)_data[key]; }
        catch (Exception) { return defaultValue; }
    }

    public string GetString(string key, string defaultValue = default)
    {
        if (!HasKey(key)) return defaultValue;
        try { return (string)_data[key]; }
        catch (Exception) { return defaultValue; }
    }

    public void SetInt(string key, int value) => _data[key] = value;
    public void SetFloat(string key, float value) => _data[key] = value;
    public void SetString(string key, string value) => _data[key] = value;
    public void DeleteAll() => _data.Clear();
    public void DeleteKey(string key) => _data.Remove(key);

    public void Save() 
    {
        if (_data.Count == 0)
            return;

        var binaryFormatter = new BinaryFormatter();
        byte[] bytes;

        using(var stream = new MemoryStream())
        {
            binaryFormatter.Serialize(stream, _data);
            bytes = stream.ToArray();
        }

        FileAdapter.WriteAllBytes(Path, bytes);
    }

    public void Load()
    {
        if (!FileAdapter.Exists(Path))
            return;

        byte[] bytes = FileAdapter.ReadAllBytes(Path);
        if (bytes == null || bytes.Length == 0)
            return;

        var binaryFormatter = new BinaryFormatter();
        using(var stream = new MemoryStream(bytes))
        {
            stream.Read(bytes, 0, bytes.Length);
            stream.Position = 0;
            _data = (Dictionary<string, object>)binaryFormatter.Deserialize(stream);
            stream.Flush();
        }
    }
}
