using core.AzureApi.model;

namespace core.AzureApi
{
    public static class AzLocationApiClient
    {
        public static List<AzLocation> ListLocations(bool forceRefresh = false)
        {

            // Get from cache if available
            if (!forceRefresh)
            {
                var cached = ProfileManager.LoadFromJsonFolder<AzLocation>("cache-locations");
                if (cached != null)
                {
                    Console.WriteLine("ListLocations from the cache");
                    return cached;
                }
            }

            // Get from API
            Console.WriteLine("ListLocations from the API");
            var locationsEnumerable = AzLoginClient.GetAzure().GetCurrentSubscription().ListLocations();

            // Persist in cache
            var items = new List<AzLocation>();
            foreach (var location in locationsEnumerable)
            {
                items.Add(new AzLocation()
                {
                    Id = location.Inner.Id,
                    Name = location.Name,
                    DisplayName = location.DisplayName,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                });
            }

            ProfileManager.SaveToJsonFolder("cache-locations", items,
                item => item.Id.Replace('/', '_')
                );

            return items;

        }

        public static AzLocation? LocationByName(string regionName)
        {
            foreach (var location in ListLocations())
            {
                if (location.Name == regionName)
                {
                    return location;
                }
            }

            return null;
        }
    }
}
