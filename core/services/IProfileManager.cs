namespace core.services;

public interface IProfileManager
{
    string GetAzFilePath(string filePart);
    string GetProfileFilePath(string filePart);
    string GetProfileFilePath(string filePart1, string filePart2);
    T? LoadFromJson<T>(string fileName);
    void SaveToJson<T>(string fileName, T item);
    List<T>? LoadFromJsonFolder<T>(string folderName);
    void SaveToJsonFolder<T>(string folderName, IList<T> items, Func<T, string> itemFileName);
    void SaveNewToJsonFolder<T>(string folderName, T item, Func<T, string> itemFileName);
}