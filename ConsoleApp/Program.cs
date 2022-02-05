using core;
using core.AzureApi;
using core.services;

namespace ConsoleApp;

public class Program
{
    public static async Task Main()
    {
        var profileManager = new ProfileManager();
        var dnsService = new DnsService();
        var azLoginClient = new AzLoginClient(profileManager);
        await azLoginClient.LogInIfNeededAsync();
        var azDnsZonesApiClient = new AzDnsZonesApiClient(azLoginClient, profileManager);
        var azLocationApiClient = new AzLocationApiClient(azLoginClient, profileManager);
        var azWebAppsApiClient = new AzWebAppsApiClient(azDnsZonesApiClient, azLoginClient, dnsService, profileManager);
        var azGlobalStore = new AzGlobalStore(azDnsZonesApiClient, azLocationApiClient, azWebAppsApiClient);

        foreach (var item in await azGlobalStore.ResourceGroupsUsedAsync())
        {
            Console.WriteLine(item);
        }
    }
}