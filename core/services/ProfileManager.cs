using System.Text.Json;

namespace core.services;

public class ProfileManager : IProfileManager
{
    private static readonly string AzPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".azure");
    private static readonly string ProfilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "foilen-azure-manager");
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions() { WriteIndented = true };

    static ProfileManager()
    {
        Directory.CreateDirectory(ProfilePath);
    }

    public string GetAzFilePath(string filePart)
    {
        return System.IO.Path.Combine(AzPath, filePart);
    }


    public string GetProfileFilePath(string filePart)
    {
        return System.IO.Path.Combine(ProfilePath, filePart);
    }

    public string GetProfileFilePath(string filePart1, string filePart2)
    {
        return System.IO.Path.Combine(ProfilePath, filePart1, filePart2);
    }

    public T? LoadFromJson<T>(string fileName)
    {
        var fullPath = GetProfileFilePath(fileName);
        Console.WriteLine($"Try to load {fileName} in {fullPath}");

        try
        {
            var json = File.ReadAllText(fullPath);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception)
        {
            // ignored
        }

        return default;
    }

    public void SaveToJson<T>(string fileName, T item)
    {
        var fullPath = GetProfileFilePath(fileName);
        Console.WriteLine($"Save {fileName} in {fullPath}");

        var json = JsonSerializer.Serialize(item, JsonSerializerOptions);
        File.WriteAllText(fullPath, json);
    }

    public List<T>? LoadFromJsonFolder<T>(string folderName)
    {
        var basePath = GetProfileFilePath(folderName);
        if (!Directory.Exists(basePath))
        {
            return default;
        }

        var items = new List<T>();
        foreach (var file in Directory.EnumerateFiles(basePath))
        {
            var json = File.ReadAllText(file);
            var item = JsonSerializer.Deserialize<T>(json);
            if (item != null)
            {
                items.Add(item);
            }
        }

        return items;
    }

    public void SaveToJsonFolder<T>(string folderName, IList<T> items, Func<T, string> itemFileName)
    {
        var basePath = GetProfileFilePath(folderName);
        if (Directory.Exists(basePath))
        {
            // Delete all files in folder
            foreach (var file in Directory.EnumerateFiles(basePath))
            {
                File.Delete(file);
            }
        }
        else
        {
            Directory.CreateDirectory(basePath);
        }

        // Save all items
        foreach (var item in items)
        {
            var fullPath = GetProfileFilePath(folderName, itemFileName.Invoke(item) + ".json");
            var json = JsonSerializer.Serialize(item, JsonSerializerOptions);
            File.WriteAllText(fullPath, json);
        }
    }

    public void SaveNewToJsonFolder<T>(string folderName, T item, Func<T, string> itemFileName)
    {
        var basePath = GetProfileFilePath(folderName);
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        // Save all item
        var fullPath = GetProfileFilePath(folderName, itemFileName.Invoke(item) + ".json");
        var json = JsonSerializer.Serialize(item, JsonSerializerOptions);
        File.WriteAllText(fullPath, json);
    }
}