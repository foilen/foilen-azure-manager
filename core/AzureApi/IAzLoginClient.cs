using Microsoft.Azure.Management.Fluent;

namespace core.AzureApi;

public interface IAzLoginClient
{
    Task LogInIfNeededAsync();
    IAzure GetAzure();
}