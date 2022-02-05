using System.Collections.ObjectModel;

namespace core.AzureApi;

public static class AzApiClientHelper
{
    public static void PrintStatus(Collection<string>? statusCollection, string text)
    {
        Console.WriteLine(text);
        statusCollection?.Add(text);
    }
}