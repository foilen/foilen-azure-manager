using core.AzureApi.model;
using core.services;
using Microsoft.Azure.Management.AppService.Fluent;

namespace core.AzureApi;

public class AzAppServicePlansApiClient : IAzAppServicePlansApiClient
{
    private readonly IAzLoginClient _azLoginClient;
    private readonly IProfileManager _profileManager;

    public AzAppServicePlansApiClient(IAzLoginClient azLoginClient, IProfileManager profileManager)
    {
        _azLoginClient = azLoginClient;
        _profileManager = profileManager;
    }

    public async Task<List<AzAppServicePlan>> ListAzAppServicePlansAsync(bool forceRefresh = false)
    {
        // Get from cache if available
        if (!forceRefresh)
        {
            var cached = _profileManager.LoadFromJsonFolder<AzAppServicePlan>("cache-appserviceplan");
            if (cached != null)
            {
                Console.WriteLine("ListAzAppServicePlans from the cache");
                return cached;
            }
        }


        // Get from API
        Console.WriteLine("ListAzAppServicePlans from the API");
        var appServicePlansEnumerable = await _azLoginClient.GetAzure().AppServices.AppServicePlans
            .ListAsync();

        // Persist in cache
        var items = new List<AzAppServicePlan>();
        foreach (var appService in appServicePlansEnumerable)
        {
            items.Add(ToAzAppServicePlan(appService));
        }

        _profileManager.SaveToJsonFolder("cache-appserviceplan", items,
            item => item.Id.Replace('/', '_')
        );

        return items;
    }

    private static AzAppServicePlan ToAzAppServicePlan(IAppServicePlan appServicePlan)
    {
        return new AzAppServicePlan()
        {
            Id = appServicePlan.Id,
            Name = appServicePlan.Name,
            ResourceGroupName = appServicePlan.ResourceGroupName,

            Type = appServicePlan.Type,
            RegionName = appServicePlan.RegionName,
            Tags = appServicePlan.Tags,

            NumberOfWebApps = appServicePlan.NumberOfWebApps,
            MaxInstances = appServicePlan.MaxInstances,
        };
    }
}