using core.AzureApi.model;

namespace core.AzureApi;

public interface IAzWebAppsApiClient
{
    Task CreateWebApp(string webAppName, string dockerImageAndTag, IList<string> hostNames, string resourceGroupName, string appServicePlanId, Dictionary<string, string> settings, IList<string>? statusCollection = null);
    Task<List<AzWebApp>> ListWebAppsAsync(bool forceRefresh = false);
}