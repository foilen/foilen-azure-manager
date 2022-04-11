using core.AzureApi.model;
using core.services;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Storage.Fluent;

namespace core.AzureApi;

public class AzStorageApiClient : IAzStorageApiClient
{
    private readonly IAzLoginClient _azLoginClient;
    private readonly IProfileManager _profileManager;

    public AzStorageApiClient(IAzLoginClient azLoginClient, IProfileManager profileManager)
    {
        _azLoginClient = azLoginClient;
        _profileManager = profileManager;
    }
    
    public async Task CreateStorageAccountAsync(string storageName, Region region, string resourceGroupName,
        IList<string>? statusCollection = null)
    {
        AzApiClientHelper.PrintStatus(statusCollection,
            $"Create the Storage Account {storageName} in existing resource group {resourceGroupName}");
        var newStorageAccount = await _azLoginClient.GetAzure().StorageAccounts
            .Define(storageName)
            .WithRegion(region)
            .WithExistingResourceGroup(resourceGroupName)
            .WithSku(StorageAccountSkuType.Standard_RAGRS)
            .WithGeneralPurposeAccountKindV2()
            .WithOnlyHttpsTraffic()
            .WithFileEncryption().WithBlobEncryption()
            .WithAccessFromAzureServices()
            .CreateAsync();
        
        // Cache newStorageAccount
        _profileManager.SaveNewToJsonFolder("cache-storageaccounts", ToStorageAccount(newStorageAccount),
            item => item.Id.Replace('/', '_')
        );
    }

    public async Task<List<AzStorageAccount>> ListStorageAccountsAsync(bool forceRefresh = false)
    {
        // Get from cache if available
        if (!forceRefresh)
        {
            var cached = _profileManager.LoadFromJsonFolder<AzStorageAccount>("cache-storageaccounts");
            if (cached != null)
            {
                Console.WriteLine("ListStorageAccounts from the cache");
                return cached;
            }
        }

        // Get from API
        Console.WriteLine("ListStorageAccounts from the API");
        var storageAccountsEnumerable = await _azLoginClient.GetAzure().StorageAccounts
            .ListAsync();

        // Persist in cache
        var items = new List<AzStorageAccount>();
        foreach (var storageAccount in storageAccountsEnumerable)
        {
            items.Add(ToStorageAccount(storageAccount));
        }

        _profileManager.SaveToJsonFolder("cache-storageaccounts", items,
            item => item.Id.Replace('/', '_')
        );

        return items;
    }

    public async Task<AzStorageAccount?> StorageAccountByNameAsync(string storageAccountName)
    {
        foreach (var storageAccount in await ListStorageAccountsAsync())
        {
            if (storageAccount.Name == storageAccountName)
            {
                return storageAccount;
            }
        }

        return null;
    }

    private static AzStorageAccount ToStorageAccount(IStorageAccount storageAccount)
    {
        return new AzStorageAccount()
        {
            Id = storageAccount.Inner.Id,
            Name = storageAccount.Name,
            ResourceGroupName = storageAccount.ResourceGroupName,
            Type = storageAccount.Type,
            RegionName = storageAccount.RegionName,
            Tags = storageAccount.Tags,
            
            Sku = storageAccount.SkuType.Name.Value,
            Kind = storageAccount.Kind.ToString(),
            AccessTier = storageAccount.AccessTier.ToString(),
        };
    }
}