using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceGraph;

namespace core.AzureApi;

public interface IAzLoginClient
{
    Task LogInIfNeededAsync();
    IAzure GetAzure();
    Task<ResourceGraphClient> GetResourceGraphClientAsync();
}