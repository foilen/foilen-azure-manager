using core.AzureApi;
using core.AzureApi.model;

namespace core
{
    public static class AzGlobalStore
    {

        public static async Task<List<AzEmailAccount>> EmailAccountAsync()
        {
            var emailAccounts = new List<AzEmailAccount>();
            foreach (var item in await AzWebAppsApiClient.ListWebAppsAsync())
            {
                var settings = item.Settings;
                if (settings.ContainsKey("EMAIL_DEFAULT_FROM_ADDRESS") &&
                    settings.ContainsKey("EMAIL_HOSTNAME") &&
                    settings.ContainsKey("EMAIL_PORT") &&
                    settings.ContainsKey("EMAIL_USER") &&
                    settings.ContainsKey("EMAIL_PASSWORD"))
                {
                    emailAccounts.Add(new AzEmailAccount(
                        settings["EMAIL_DEFAULT_FROM_ADDRESS"],
                        settings["EMAIL_HOSTNAME"], int.Parse(settings["EMAIL_PORT"]),
                        settings["EMAIL_USER"], settings["EMAIL_PASSWORD"]
                        ));
                }
            }

            return emailAccounts;
        }

        public static async Task<List<string>> RegionsUsedAndOthersAsync()
        {

            // Used first
            var regionsList = await RegionsUsedAsync();
            var alreadyPresent = new HashSet<string>(regionsList);

            // Then the others
            var locations = AzLocationApiClient.ListLocations();
            locations.Sort((a, b) => String.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal));
            foreach (var location in locations)
            {
                if (alreadyPresent.Add(location.DisplayName))
                {
                    regionsList.Add(location.DisplayName);
                }
            }

            return regionsList;

        }

        public static async Task<List<string>> RegionsUsedAsync()
        {

            var regions = new HashSet<string>();
            foreach (var item in await AzDnsZonesApiClient.ListDnsZonesAsync())
            {
                regions.Add(item.RegionName);
            }
            foreach (var item in await AzWebAppsApiClient.ListWebAppsAsync())
            {
                regions.Add(item.RegionName);
            }

            var regionsList = new List<string>(regions);
            regionsList.Sort();
            return regionsList;

        }

        public static async Task<List<string>> ResourceGroupsUsedAsync()
        {

            var regions = new HashSet<string>();
            foreach (var item in await AzDnsZonesApiClient.ListDnsZonesAsync())
            {
                regions.Add(item.ResourceGroupName);
            }
            foreach (var item in await AzWebAppsApiClient.ListWebAppsAsync())
            {
                regions.Add(item.ResourceGroupName);
            }

            var regionsList = new List<string>(regions);
            regionsList.Sort();
            return regionsList;

        }

    }
}
