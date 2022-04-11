using System;
using System.Collections.ObjectModel;
using System.Windows;
using core.AzureApi;
using core.services;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using wpf_app.model;

namespace wpf_app.window;

/// <summary>
/// Interaction logic for StorageAccountCreateWindow.xaml
/// </summary>
public partial class StorageAccountCreateWindow : Window
{
    private readonly ApplicationModel _applicationModel;
    private readonly IAzStorageApiClient _azStorageApiClient;
    private readonly IAzGlobalStore _azGlobalStore;
    private readonly IAzLocationApiClient _azLocationApiClient;
    private readonly IAzResourceGroupApiClient _azResourceGroupApiClient;

    private readonly ObservableCollection<string> _resourceGroups = new();
    private readonly ObservableCollection<string> _regions = new();

    public StorageAccountCreateWindow(ApplicationModel applicationModel, IAzStorageApiClient azStorageApiClient, IAzGlobalStore azGlobalStore, IAzLocationApiClient azLocationApiClient, IAzResourceGroupApiClient azResourceGroupApiClient)
    {
        _applicationModel = applicationModel;
        _azStorageApiClient = azStorageApiClient;
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
            await _azStorageApiClient.CreateStorageAccountAsync(StorageAccountName.Text, Region.Create(RegionName.Text), ResourceGroupName.Text, statusItems);
            statusItems.Add("[OK] Creation completed. Refreshing");
        }
        catch (Exception ex)
        {
            statusItems.Add($"[ERROR] {ex.Message}");
            return;
        }


        // Refresh list
        _applicationModel.StorageAccounts.Clear();
        foreach (var item in await _azStorageApiClient.ListStorageAccountsAsync())
        {
            _applicationModel.StorageAccounts.Add(item);
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