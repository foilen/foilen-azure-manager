using core.AzureApi.model;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace core.AzureApi;

public interface IAzStorageApiClient
{
    Task CreateStorageAccountAsync(string storageName, Region region, string resourceGroupName, IList<string>? statusCollection = null);
    Task<List<AzStorageAccount>> ListStorageAccountsAsync(bool forceRefresh = false);
    Task<AzStorageAccount?> StorageAccountByNameAsync(string storageAccountName);
}