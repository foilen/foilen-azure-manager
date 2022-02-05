using System.Collections.ObjectModel;

namespace core.services;

public interface IDnsService
{
    Task<List<string>> GetTxtAsync(string hostName);
    Task<bool> WaitForTxtAsync(string hostname, string expectedValue, TimeSpan waitBetweenRetries, int maxRetries, Collection<string>? statusCollection = null);
}