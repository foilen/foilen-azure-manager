using System.Collections.Generic;
using System.Threading.Tasks;
using core.AzureApi;
using core.AzureApi.model;

namespace Tests.mocks;

public class AzWebAppsApiClientMock : IAzWebAppsApiClient
{
    public List<AzWebApp> AzWebApps = new List<AzWebApp>();

    public Task CreateWebApp(string webAppName, string dockerImageAndTag, IList<string> hostNames, string resourceGroupName,
        string appServicePlanId, Dictionary<string, string> settings, IList<string>? statusCollection = null)
    {
        throw new System.NotImplementedException();
    }

    public Task<List<AzWebApp>> ListWebAppsAsync(bool forceRefresh = false)
    {
        return Task<List<AzWebApp>>.Factory.StartNew(() => AzWebApps);
    }
}