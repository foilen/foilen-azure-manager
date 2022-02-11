using System.Collections.ObjectModel;
using core.AzureApi.model;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace core.AzureApi;

public interface IAzResourceGroupApiClient
{
    Task CreateResourceGroup(string resourceGroupName, Region region, IList<string>? statusCollection = null);
    Task<List<AzResourceGroup>> ListResourceGroupsAsync(bool forceRefresh = false);
    Task<AzResourceGroup?> ResourceGroupByNameAsync(string resourceGroupName);
}