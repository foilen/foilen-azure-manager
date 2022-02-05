using core.AzureApi.model;

namespace core.services;

public interface IAzGlobalStore
{
    Task<List<AzEmailAccount>> EmailAccountAsync();
    Task<List<string>> RegionsUsedAndOthersAsync();
    Task<List<string>> RegionsUsedAsync();
    Task<List<string>> ResourceGroupsUsedAsync();
}