using System;
using System.Collections.ObjectModel;
using System.Windows;
using core.AzureApi;
using core.services;
using wpf_app.model;

namespace wpf_app.window;

/// <summary>
/// Interaction logic for DnsZoneCreateWindow.xaml
/// </summary>
public partial class DnsZoneCreateWindow : Window
{
    private readonly ApplicationModel _applicationModel;
    private readonly IAzDnsZonesApiClient _azDnsZonesApiClient;
    private readonly IAzGlobalStore _azGlobalStore;
    private readonly IAzLocationApiClient _azLocationApiClient;
    private readonly IAzResourceGroupApiClient _azResourceGroupApiClient;

    private readonly ObservableCollection<string> _resourceGroups = new();
    private readonly ObservableCollection<string> _regions = new();

    public DnsZoneCreateWindow(ApplicationModel applicationModel, IAzDnsZonesApiClient azDnsZonesApiClient, IAzGlobalStore azGlobalStore, IAzLocationApiClient azLocationApiClient, IAzResourceGroupApiClient azResourceGroupApiClient)
    {
        _applicationModel = applicationModel;
        _azDnsZonesApiClient = azDnsZonesApiClient;
        _azGlobalStore = azGlobalStore;
        _azLocationApiClient = azLocationApiClient;
        _azResourceGroupApiClient = azResourceGroupApiClient;
        InitializeComponent();
    }

    private async void InitDataAsync(object sender, RoutedEventArgs e)
    {
        ResourceGroups.ItemsSource = _resourceGroups;
        foreach (var item in await _azGlobalStore.ResourceGroupsUsedAsync())
        {
            _resourceGroups.Add(item);
        }

        RegionNames.ItemsSource = _regions;
        foreach (var item in await _azGlobalStore.RegionsUsedAndOthersAsync())
        {
            _regions.Add(item);
        }
    }

    private async void CreateClickAsync(object sender, RoutedEventArgs e)
    {
        var statusItems = new ObservableCollection<string>();
        StatusList.ItemsSource = statusItems;

        // Create
        try
        {
            await _azDnsZonesApiClient.CreateDnsZone(HostName.Text, ResourceGroupName.Text, statusItems);
            statusItems.Add("[OK] Creation completed. Refreshing");
        }
        catch (Exception ex)
        {
            statusItems.Add($"[ERROR] {ex.Message}");
            return;
        }


        // Refresh list
        _applicationModel.DnsZones.Clear();
        foreach (var item in await _azDnsZonesApiClient.ListDnsZonesAsync())
        {
            _applicationModel.DnsZones.Add(item);
        }

        statusItems.Add("Refresh completed");
    }

    private async void ResourceGroupName_OnSelectionChangedAsync(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ResourceGroupName.Text = (string)ResourceGroups.SelectedItem;
        var regionName = (await _azResourceGroupApiClient.ResourceGroupByNameAsync((string)ResourceGroups.SelectedItem))?.RegionName;
        if (regionName != null)
        {
            var regionDisplayName = _azLocationApiClient.LocationByName(regionName)?.DisplayName;
            RegionName.Text = regionDisplayName;
        }
    }

    private void RegionName_OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        RegionName.Text = (string) RegionNames.SelectedItem;
    }
}