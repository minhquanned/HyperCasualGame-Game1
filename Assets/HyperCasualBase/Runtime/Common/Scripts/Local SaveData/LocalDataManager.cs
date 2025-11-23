using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public static class LocalDataManager
{
    private static readonly string defaultFolder = Application.persistentDataPath;

    private static string GetFilePath(string fileName, string folder = null)
    {
        folder ??= defaultFolder;
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        return Path.Combine(folder, fileName + ".json");
    }

    /// <summary>
    /// Save data object ra JSON file
    /// </summary>
    public static void Save<T>(T data, string fileName, string folder = null, Formatting formatting = Formatting.Indented)
    {
        string path = GetFilePath(fileName, folder);
        string json = JsonConvert.SerializeObject(data, formatting);
        File.WriteAllText(path, json);
#if UNITY_EDITOR
        Debug.Log($"[LocalDataManager] Saved data to: {path}");
#endif
    }

    /// <summary>
    /// Load data từ JSON file, nếu không có thì trả về default(T)
    /// </summary>
    public static T Load<T>(string fileName, string folder = null)
    {
        string path = GetFilePath(fileName, folder);
        if (!File.Exists(path))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[LocalDataManager] File not found: {path}");
#endif
            return default(T);
        }

        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// Kiểm tra file tồn tại
    /// </summary>
    public static bool Exists(string fileName, string folder = null)
    {
        string path = GetFilePath(fileName, folder);
        return File.Exists(path);
    }

    /// <summary>
    /// Xóa file
    /// </summary>
    public static void Delete(string fileName, string folder = null)
    {
        string path = GetFilePath(fileName, folder);
        if (File.Exists(path))
            File.Delete(path);
    }

    /// <summary>
    /// Reset file về dữ liệu mặc định
    /// </summary>
    public static void Reset<T>(T defaultData, string fileName, string folder = null)
    {
        Save(defaultData, fileName, folder);
    }
}
