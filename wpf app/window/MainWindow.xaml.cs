using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using core.AzureApi;
using core.services;
using wpf_app.model;

namespace wpf_app.window;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly ApplicationModel _applicationModel;
    private readonly IAzAppServicePlansApiClient _azAppServicePlansApiClient;
    private readonly IAzDnsZonesApiClient _azDnsZonesApiClient;
    private readonly IAzLocationApiClient _azLocationApiClient;
    private readonly IAzLoginClient _azLoginClient;
    private readonly IAzGlobalStore _azGlobalStore;
    private readonly IAzResourceGroupApiClient _azResourceGroupApiClient;
    private readonly IAzStorageApiClient _azStorageApiClient;
    private readonly IAzWebAppsApiClient _azWebAppsApiClient;
    private readonly IDnsService _dnsService;
    private readonly IProfileManager _profileManager;

    public MainWindow()
    {
        _profileManager = new ProfileManager();
        _dnsService = new DnsService();
        _azLoginClient = new AzLoginClient(_profileManager);
        _azDnsZonesApiClient = new AzDnsZonesApiClient(_azLoginClient, _profileManager);
        _azLocationApiClient = new AzLocationApiClient(_azLoginClient, _profileManager);
        _azAppServicePlansApiClient = new AzAppServicePlansApiClient(_azLoginClient, _profileManager);
        _azWebAppsApiClient =
            new AzWebAppsApiClient(_azDnsZonesApiClient, _azLoginClient, _dnsService, _profileManager);
        _azGlobalStore = new AzGlobalStore(_azDnsZonesApiClient, _azLocationApiClient, _azWebAppsApiClient);
        _azResourceGroupApiClient = new AzResourceGroupApiClient(_azLoginClient, _profileManager);
        _azStorageApiClient = new AzStorageApiClient(_azLoginClient, _profileManager);
        _applicationModel = new ApplicationModel(_azAppServicePlansApiClient, _azDnsZonesApiClient, _azGlobalStore,
            _azResourceGroupApiClient, _azStorageApiClient, _azWebAppsApiClient);
        DataContext = _applicationModel;
        InitializeComponent();
    }

    private async void InitDataAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _azLoginClient.LogInIfNeededAsync();
        }
        catch (AzNotInstalledException)
        {
            MessageBox.Show(
                "You need to install Azure CLI and restart this application. Your browser will start now with the right page to get it");

            Process.Start(new ProcessStartInfo
            {
                FileName = "https://aka.ms/installazurecliwindows",
                UseShellExecute = true
            });

            this.Close();
            return;
        }

        await _applicationModel.RefreshAppServicePlansAsync(false);
        await _applicationModel.RefreshDnsZonesAsync(false);
        await _applicationModel.RefreshResourceGroupsAsync(false);
        await _applicationModel.RefreshStorageAccountsAsync(false);
        await _applicationModel.RefreshWebAppsAsync(false);
    }

    // DNS Zone
    private void DnsZoneCreate(object sender, RoutedEventArgs e)
    {
        new DnsZoneCreateWindow(_applicationModel, _azDnsZonesApiClient, _azGlobalStore, _azLocationApiClient,
            _azResourceGroupApiClient).Show();
    }

    private async void DnsZonesRefreshAsync(object sender, RoutedEventArgs e)
    {
        await _applicationModel.RefreshDnsZonesAsync(true);
    }

    // Resource Groups
    private async void ResourceGroupsRefreshAsync(object sender, RoutedEventArgs e)
    {
        await _applicationModel.RefreshResourceGroupsAsync(true);
    }

    // App Service Plan
    private void AppServicePlansCreate(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://portal.azure.com/#create/Microsoft.AppServicePlanCreate",
            UseShellExecute = true
        });
    }

    private async void AppServicePlansRefreshAsync(object sender, RoutedEventArgs e)
    {
        await _applicationModel.RefreshAppServicePlansAsync(true);
    }

    // Storage Account
    private void StorageAccountCreate(object sender, RoutedEventArgs e)
    {
        new StorageAccountCreateWindow(_applicationModel, _azStorageApiClient, _azGlobalStore,
            _azLocationApiClient, _azResourceGroupApiClient).Show();
    }

    private async void StorageAccountsRefreshAsync(object sender, RoutedEventArgs e)
    {
        await _applicationModel.RefreshStorageAccountsAsync(true);
    }

    // Web Apps
    private async void WebAppsRefreshAsync(object sender, RoutedEventArgs e)
    {
        await _applicationModel.RefreshWebAppsAsync(true);
    }

    private void WebAppClone(object sender, MouseButtonEventArgs e)
    {
        var item = (AzWebAppWithHostname) WebApps.SelectedItem;

        // TODO WebAppClone - Support other languages
        new PhpCreateWindow(_applicationModel, _azGlobalStore, _azWebAppsApiClient, item).Show();
    }

    // PHP
    private void PhpCreateWebApp(object sender, RoutedEventArgs e)
    {
        new PhpCreateWindow(_applicationModel, _azGlobalStore, _azWebAppsApiClient).Show();
    }
}