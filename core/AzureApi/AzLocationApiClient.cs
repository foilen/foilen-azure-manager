using core.AzureApi.model;
using core.services;

namespace core.AzureApi;

public class AzLocationApiClient : IAzLocationApiClient
{
    private readonly IAzLoginClient _azLoginClient;
    private readonly IProfileManager _profileManager;

    public AzLocationApiClient(IAzLoginClient azLoginClient, IProfileManager profileManager)
    {
        _azLoginClient = azLoginClient;
        _profileManager = profileManager;
    }

    public List<AzLocation> ListLocations(bool forceRefresh = false)
    {
        // Get from cache if available
        if (!forceRefresh)
        {
            var cached = _profileManager.LoadFromJsonFolder<AzLocation>("cache-locations");
            if (cached != null)
            {
                Console.WriteLine("ListLocations from the cache");
                return cached;
            }
        }

        // Get from API
        Console.WriteLine("ListLocations from the API");
        var locationsEnumerable = _azLoginClient.GetAzure().GetCurrentSubscription().ListLocations();

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

        _profileManager.SaveToJsonFolder("cache-locations", items,
            item => item.Id.Replace('/', '_')
        );

        return items;
    }

    public AzLocation? LocationByName(string regionName)
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