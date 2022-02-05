using core.AzureApi.model;

namespace core.AzureApi;

public interface IAzLocationApiClient
{
    List<AzLocation> ListLocations(bool forceRefresh = false);
    AzLocation? LocationByName(string regionName);
}