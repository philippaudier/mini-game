using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Système de sauvegarde extensible basé sur JSON/PlayerPrefs.
/// Chaque mini-jeu stocke son high score via un gameId string.
/// Extensible avec des données custom int/string par clé.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SaveKey = "MiniGamesSaveData";
    private SaveData data;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ── High Scores ──

    public int GetHighScore(string gameId)
    {
        if (data.highScores.TryGetValue(gameId, out int score))
            return score;
        return 0;
    }

    public bool TrySaveHighScore(string gameId, int score)
    {
        int current = GetHighScore(gameId);
        if (score > current)
        {
            data.highScores[gameId] = score;
            WriteToDisk();
            return true;
        }
        return false;
    }

    // ── Generic data (extensible per mini-game) ──

    public void SetInt(string key, int value)
    {
        data.customInts[key] = value;
        WriteToDisk();
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        return data.customInts.TryGetValue(key, out int val) ? val : defaultValue;
    }

    public void SetString(string key, string value)
    {
        data.customStrings[key] = value;
        WriteToDisk();
    }

    public string GetString(string key, string defaultValue = "")
    {
        return data.customStrings.TryGetValue(key, out string val) ? val : defaultValue;
    }

    // ── Persistence ──

    private void WriteToDisk()
    {
        data.SyncLists();
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        data = new SaveData();
        string json = PlayerPrefs.GetString(SaveKey, "");
        if (!string.IsNullOrEmpty(json))
            JsonUtility.FromJsonOverwrite(json, data);
        data.RebuildDictionaries();
    }

    public void ClearAll()
    {
        data = new SaveData();
        data.RebuildDictionaries();
        WriteToDisk();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) WriteToDisk();
    }

    private void OnApplicationQuit()
    {
        WriteToDisk();
    }

    // ── Serializable data ──
    // JsonUtility ne supporte pas Dictionary → listes parallèles

    [System.Serializable]
    private class SaveData
    {
        public List<string> hsKeys = new List<string>();
        public List<int> hsValues = new List<int>();
        public List<string> intKeys = new List<string>();
        public List<int> intValues = new List<int>();
        public List<string> strKeys = new List<string>();
        public List<string> strValues = new List<string>();

        [System.NonSerialized] public Dictionary<string, int> highScores = new Dictionary<string, int>();
        [System.NonSerialized] public Dictionary<string, int> customInts = new Dictionary<string, int>();
        [System.NonSerialized] public Dictionary<string, string> customStrings = new Dictionary<string, string>();

        public void RebuildDictionaries()
        {
            highScores = ToDictInt(hsKeys, hsValues);
            customInts = ToDictInt(intKeys, intValues);
            customStrings = ToDictStr(strKeys, strValues);
        }

        public void SyncLists()
        {
            FromDict(highScores, out hsKeys, out hsValues);
            FromDict(customInts, out intKeys, out intValues);
            FromDictStr(customStrings, out strKeys, out strValues);
        }

        private static Dictionary<string, int> ToDictInt(List<string> keys, List<int> vals)
        {
            var d = new Dictionary<string, int>();
            for (int i = 0; i < keys.Count && i < vals.Count; i++)
                d[keys[i]] = vals[i];
            return d;
        }

        private static Dictionary<string, string> ToDictStr(List<string> keys, List<string> vals)
        {
            var d = new Dictionary<string, string>();
            for (int i = 0; i < keys.Count && i < vals.Count; i++)
                d[keys[i]] = vals[i];
            return d;
        }

        private static void FromDict(Dictionary<string, int> d, out List<string> keys, out List<int> vals)
        {
            keys = new List<string>(d.Keys);
            vals = new List<int>(d.Values);
        }

        private static void FromDictStr(Dictionary<string, string> d, out List<string> keys, out List<string> vals)
        {
            keys = new List<string>(d.Keys);
            vals = new List<string>(d.Values);
        }
    }
}
