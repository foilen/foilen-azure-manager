using System.Collections.ObjectModel;
using core.AzureApi.model;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace core.AzureApi
{
    public static class AzResourceGroupApiClient
    {

        public static async Task CreateResourceGroup(string resourceGroupName, Region region, Collection<string>? statusCollection = null)
        {

            AzApiClientHelper.PrintStatus(statusCollection, $"Check if the resource group {resourceGroupName} exists");
            var resourceGroupExists = await AzLoginClient.GetAzure().ResourceGroups
                .ContainAsync(resourceGroupName);
            AzApiClientHelper.PrintStatus(statusCollection, $"Resource group {resourceGroupName} exists? {resourceGroupExists}");
            if (resourceGroupExists)
            {
                return;
            }

            AzApiClientHelper.PrintStatus(statusCollection,
                $"Create the resource group {resourceGroupName} in region {region}");
            var newResourceGroup = await AzLoginClient.GetAzure().ResourceGroups
                  .Define(resourceGroupName)
                  .WithRegion(region)
                  .CreateAsync();

            // Cache newResourceGroup
            ProfileManager.SaveNewToJsonFolder("cache-resourcegroups", ToResourceGroup(newResourceGroup),
                item => item.Id.Replace('/', '_')
            );

        }

        public static async Task<List<AzResourceGroup>> ListResourceGroupsAsync(bool forceRefresh = false)
        {

            // Get from cache if available
            if (!forceRefresh)
            {
                var cached = ProfileManager.LoadFromJsonFolder<AzResourceGroup>("cache-resourcegroups");
                if (cached != null)
                {
                    Console.WriteLine("ListResourceGroups from the cache");
                    return cached;
                }
            }

            // Get from API
            Console.WriteLine("ListResourceGroups from the API");
            var resourceGroupsEnumerable = await AzLoginClient.GetAzure().ResourceGroups
                .ListAsync();

            // Persist in cache
            var items = new List<AzResourceGroup>();
            foreach (var resourceGroup in resourceGroupsEnumerable)
            {
                items.Add(ToResourceGroup(resourceGroup));
            }

            ProfileManager.SaveToJsonFolder("cache-resourcegroups", items,
                item => item.Id.Replace('/', '_')
                );

            return items;

        }

        public static async Task<AzResourceGroup?> ResourceGroupByNameAsync(string resourceGroupName)
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
}
