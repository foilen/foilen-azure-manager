using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using core.AzureApi;
using core.AzureApi.model;
using core.services;

namespace wpf_app.model;

public class ApplicationModel : INotifyPropertyChanged
{
    private readonly IAzAppServicePlansApiClient _azAppServicePlansApiClient;
    private readonly IAzDnsZonesApiClient _azDnsZonesApiClient;
    private readonly IAzGlobalStore _azGlobalStore;
    private readonly IAzResourceGroupApiClient _azResourceGroupApiClient;
    private readonly IAzWebAppsApiClient _azWebAppsApiClient;
    
    public ApplicationModel(IAzAppServicePlansApiClient azAppServicePlansApiClient, IAzDnsZonesApiClient azDnsZonesApiClient, IAzGlobalStore azGlobalStore, IAzResourceGroupApiClient azResourceGroupApiClient, IAzWebAppsApiClient azWebAppsApiClient)
    {
        _azAppServicePlansApiClient = azAppServicePlansApiClient;
        _azDnsZonesApiClient = azDnsZonesApiClient;
        _azGlobalStore = azGlobalStore;
        _azResourceGroupApiClient = azResourceGroupApiClient;
        _azWebAppsApiClient = azWebAppsApiClient;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ObservableCollection<AzAppServicePlan> AppServicePlans { get; } = new();
    public ObservableCollection<AzDnsZone> DnsZones { get; } = new();
    public ObservableCollection<AzEmailAccount> EmailAccounts { get; } = new();
    public ObservableCollection<AzResourceGroup> ResourceGroups { get; } = new();
    public ObservableCollection<AzWebAppWithHostname> WebApps { get; } = new();

    public async Task RefreshAppServicePlansAsync(bool forceRefresh)
    {
        AppServicePlans.Clear();
        foreach (var item in await _azAppServicePlansApiClient.ListAzAppServicePlansAsync(forceRefresh))
        {
            AppServicePlans.Add(item);
        }
    }

    public async Task RefreshDnsZonesAsync(bool forceRefresh)
    {
        DnsZones.Clear();
        foreach (var item in await _azDnsZonesApiClient.ListDnsZonesAsync(forceRefresh))
        {
            DnsZones.Add(item);
        }
    }

    public async Task RefreshResourceGroupsAsync(bool forceRefresh)
    {
        ResourceGroups.Clear();
        foreach (var item in await _azResourceGroupApiClient.ListResourceGroupsAsync(forceRefresh))
        {
            ResourceGroups.Add(item);
        }
    }

    public async Task RefreshWebAppsAsync(bool forceRefresh)
    {
        WebApps.Clear();
        foreach (var item in await _azWebAppsApiClient.ListWebAppsAsync(forceRefresh))
        {
            foreach (var hostName in item.HostNames)
            {
                WebApps.Add(new AzWebAppWithHostname(hostName, item));
            }
        }

        await RefreshEmailAccountsAsync();
    }

    private async Task RefreshEmailAccountsAsync()
    {
        EmailAccounts.Clear();
        foreach (var emailAccount in await _azGlobalStore.EmailAccountAsync())
        {
            EmailAccounts.Add(emailAccount);
        }
    }
}