using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class FakeRuntimePlayerPrefs : IPlayerPrefsAdapter
{
    private string _relativePath = "PlayerPrefs.sav";
    private string Path => $"{FileAdapter.persistentDataPath}/{_relativePath}";

    private Dictionary<string, object> _data = new();

    public FakeRuntimePlayerPrefs(string relativePath)
    {
        _relativePath = relativePath;
    }

    public bool HasKey(string key) => _data.ContainsKey(key);
    public int GetInt(string key) 
    {
        if (!HasKey(key)) return default;
        try { return (int)_data[key]; }
        catch (Exception) { return default; }
    }
    public float GetFloat(string key) 
    {
        if (!HasKey(key)) return default;
        try { return (float)_data[key]; }
        catch (Exception) { return default; }
    }

    public string GetString(string key)
    {
        if (!HasKey(key)) return default;
        try { return (string)_data[key]; }
        catch (Exception) { return default; }
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
            binaryFormatter.Serialize(stream, this);
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
        PlayerPrefsSerialization loadedData = new();

        using(var stream = new MemoryStream(bytes))
        {
            stream.Read(bytes, 0, bytes.Length);
            loadedData = (PlayerPrefsSerialization)binaryFormatter.Deserialize(stream);
            stream.Flush();
        }

        _data = loadedData.ToDictionary();
    }

    [Serializable]
    private class PlayerPrefsSerialization
    {
        private List<string> Keys = new();
        private List<object> Values = new();

        public PlayerPrefsSerialization() {}
        public PlayerPrefsSerialization(Dictionary<string, object> data) => FromDictionary(data);

        public void FromDictionary(Dictionary<string, object> data)
        {
            Keys = data.Keys.ToList();
            Values = data.Values.ToList();
        }
        
        public Dictionary<string, object> ToDictionary()
        {
            if (Keys.Count != Values.Count)
                Debug.LogError($"Fake Player Prefs dictionary is inconsistent, has {Keys.Count} keys and {Values.Count} values");

            Dictionary<string, object> dictionary = new();
            for (int i = 0; i < Keys.Count && i < Values.Count; i++)
                dictionary.Add(Keys[i], Values[i]);

            return dictionary;
        }
    }
}
