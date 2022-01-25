using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using core;
using core.AzureApi;
using core.AzureApi.model;

namespace wpf_app.model
{
    public class ApplicationModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<AzAppServicePlan> AppServicePlans { get; } = new();
        public ObservableCollection<AzDnsZone> DnsZones { get; } = new();
        public ObservableCollection<AzEmailAccount> EmailAccounts { get; } = new();
        public ObservableCollection<AzResourceGroup> ResourceGroups { get; } = new();
        public ObservableCollection<AzWebAppWithHostname> WebApps { get; } = new ();

        public async Task RefreshAppServicePlansAsync(bool forceRefresh)
        {
            AppServicePlans.Clear();
            foreach (var item in await AzAppServicePlansApiClient.ListAzAppServicePlansAsync(forceRefresh))
            {
                AppServicePlans.Add(item);
            }
        }

        public async Task RefreshDnsZonesAsync(bool forceRefresh)
        {
            DnsZones.Clear();
            foreach (var item in await AzDnsZonesApiClient.ListDnsZonesAsync(forceRefresh))
            {
                DnsZones.Add(item);
            }
        }

        public async Task RefreshResourceGroupsAsync(bool forceRefresh)
        {
            ResourceGroups.Clear();
            foreach (var item in await AzResourceGroupApiClient.ListResourceGroupsAsync(forceRefresh))
            {
                ResourceGroups.Add(item);
            }
        }

        public async Task RefreshWebAppsAsync(bool forceRefresh)
        {
            WebApps.Clear();
            foreach (var item in await AzWebAppsApiClient.ListWebAppsAsync(forceRefresh))
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
            foreach (var emailAccount in await AzGlobalStore.EmailAccountAsync())
            {
                EmailAccounts.Add(emailAccount);
            }
        }
    }
}
