using System.Collections.ObjectModel;
using core.AzureApi.model;
using core.services;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace core.AzureApi;

public class AzResourceGroupApiClient : IAzResourceGroupApiClient
{
    private readonly IAzLoginClient _azLoginClient;
    private readonly IProfileManager _profileManager;

    public AzResourceGroupApiClient(IAzLoginClient azLoginClient, IProfileManager profileManager)
    {
        _azLoginClient = azLoginClient;
        _profileManager = profileManager;
    }

    public async Task CreateResourceGroup(string resourceGroupName, Region region, Collection<string>? statusCollection = null)
    {
        AzApiClientHelper.PrintStatus(statusCollection, $"Check if the resource group {resourceGroupName} exists");
        var resourceGroupExists = await _azLoginClient.GetAzure().ResourceGroups
            .ContainAsync(resourceGroupName);
        AzApiClientHelper.PrintStatus(statusCollection, $"Resource group {resourceGroupName} exists? {resourceGroupExists}");
        if (resourceGroupExists)
        {
            return;
        }

        AzApiClientHelper.PrintStatus(statusCollection,
            $"Create the resource group {resourceGroupName} in region {region}");
        var newResourceGroup = await _azLoginClient.GetAzure().ResourceGroups
            .Define(resourceGroupName)
            .WithRegion(region)
            .CreateAsync();

        // Cache newResourceGroup
        _profileManager.SaveNewToJsonFolder("cache-resourcegroups", ToResourceGroup(newResourceGroup),
            item => item.Id.Replace('/', '_')
        );
    }

    public async Task<List<AzResourceGroup>> ListResourceGroupsAsync(bool forceRefresh = false)
    {
        // Get from cache if available
        if (!forceRefresh)
        {
            var cached = _profileManager.LoadFromJsonFolder<AzResourceGroup>("cache-resourcegroups");
            if (cached != null)
            {
                Console.WriteLine("ListResourceGroups from the cache");
                return cached;
            }
        }

        // Get from API
        Console.WriteLine("ListResourceGroups from the API");
        var resourceGroupsEnumerable = await _azLoginClient.GetAzure().ResourceGroups
            .ListAsync();

        // Persist in cache
        var items = new List<AzResourceGroup>();
        foreach (var resourceGroup in resourceGroupsEnumerable)
        {
            items.Add(ToResourceGroup(resourceGroup));
        }

        _profileManager.SaveToJsonFolder("cache-resourcegroups", items,
            item => item.Id.Replace('/', '_')
        );

        return items;
    }

    public async Task<AzResourceGroup?> ResourceGroupByNameAsync(string resourceGroupName)
    {
        foreach (var resourceGroup in await ListResourceGroupsAsync())
        {
            if (resourceGroup.Name == resourceGroupName)
            {
                return resourceGroup;
            }
        }

        return null;
    }

    private static AzResourceGroup ToResourceGroup(IResourceGroup resourceGroup)
    {
        return new AzResourceGroup()
        {
            Id = resourceGroup.Inner.Id,
            Name = resourceGroup.Name,
            RegionName = resourceGroup.RegionName,
        };
    }
}