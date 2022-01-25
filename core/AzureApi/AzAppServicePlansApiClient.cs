using System.Collections.ObjectModel;
using core.AzureApi.model;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.Management.Dns.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace core.AzureApi
{
    public static class AzAppServicePlansApiClient
    {

        public static async Task<List<AzAppServicePlan>> ListAzAppServicePlansAsync(bool forceRefresh = false)
        {

            // Get from cache if available
            if (!forceRefresh)
            {
                var cached = ProfileManager.LoadFromJsonFolder<AzAppServicePlan>("cache-appserviceplan");
                if (cached != null)
                {
                    Console.WriteLine("ListAzAppServicePlans from the cache");
                    return cached;
                }
            }


            // Get from API
            Console.WriteLine("ListAzAppServicePlans from the API");
            var appServicePlansEnumerable = await AzLoginClient.GetAzure().AppServices.AppServicePlans
                .ListAsync();

            // Persist in cache
            var items = new List<AzAppServicePlan>();
            foreach (var appService in appServicePlansEnumerable)
            {
                items.Add(ToAzAppServicePlan(appService));
            }

            ProfileManager.SaveToJsonFolder("cache-appserviceplan", items,
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
}
