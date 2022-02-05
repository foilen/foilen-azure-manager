using System.Collections.ObjectModel;
using core.AzureApi.model;

namespace core.AzureApi;

public interface IAzWebAppsApiClient
{
    Task CreateWebApp(string webAppName, string dockerImageAndTag, string hostName, string resourceGroupName, string appServicePlanId, Dictionary<string, string> settings, Collection<string>? statusCollection = null);
    Task<List<AzWebApp>> ListWebAppsAsync(bool forceRefresh = false);
}