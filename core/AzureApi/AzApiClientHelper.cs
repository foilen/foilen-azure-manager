namespace core.AzureApi;

public static class AzApiClientHelper
{
    public static void PrintStatus(IList<string>? statusCollection, string text)
    {
        Console.WriteLine(text);
        statusCollection?.Add(text);
    }
}