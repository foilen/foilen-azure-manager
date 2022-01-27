using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using core;
using core.AzureApi;
using core.AzureApi.model;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using wpf_app.model;

namespace wpf_app.window
{
    /// <summary>
    /// Interaction logic for PhpCreateWindow.xaml
    /// </summary>
    public partial class PhpCreateWindow : Window
    {

        private readonly ApplicationModel _applicationModel;

        private readonly ObservableCollection<string> _resourceGroups = new();
        private readonly ObservableCollection<string> _regions = new();
        private readonly ObservableCollection<string> _phpVersions = new();

        public PhpCreateWindow(ApplicationModel applicationModel, AzWebAppWithHostname? azWebAppWithHostname = null)
        {
            this._applicationModel = applicationModel;
            InitializeComponent();

            if (azWebAppWithHostname != null)
            {
                var azWebApp = azWebAppWithHostname.Value.AzWebApp;
                WebAppName.Text = azWebApp.Name;
                HostName.Text = azWebAppWithHostname?.HostName;
                ResourceGroupName.Text = azWebApp.ResourceGroupName;

                EmailRelayDefaultEmail.Text = azWebApp.Settings.GetValueOrDefault("EMAIL_DEFAULT_FROM_ADDRESS", "");
                EmailRelayHostname.Text = azWebApp.Settings.GetValueOrDefault("EMAIL_HOSTNAME", "");
                EmailRelayPort.Text = azWebApp.Settings.GetValueOrDefault("EMAIL_PORT", "");
                EmailRelayUser.Text = azWebApp.Settings.GetValueOrDefault("EMAIL_USER", "");
                EmailRelayPassword.Text = azWebApp.Settings.GetValueOrDefault("EMAIL_PASSWORD", "");

                PhpMaxExecutionTimeSec.Text = azWebApp.Settings.GetValueOrDefault("PHP_MAX_EXECUTION_TIME_SEC", "");
                PhpMaxUploadFilesizeMb.Text = azWebApp.Settings.GetValueOrDefault("PHP_MAX_UPLOAD_FILESIZE_MB", "");
                PhpMaxMemoryLimitMb.Text = azWebApp.Settings.GetValueOrDefault("PHP_MAX_MEMORY_LIMIT_MB", "");
            }
        }

        private async void InitDataAsync(object sender, RoutedEventArgs e)
        {
            DataContext = _applicationModel;

            ResourceGroups.ItemsSource = _resourceGroups;
            foreach (var item in await AzGlobalStore.ResourceGroupsUsedAsync())
            {
                _resourceGroups.Add(item);
            }

            _phpVersions.Add("foilen/az-docker-apache_php:7.4.9-1");
            PhpVersions.ItemsSource = _phpVersions;
            PhpVersions.SelectedItem = _phpVersions[0];

        }

        private async void CreateClickAsync(object sender, RoutedEventArgs e)
        {
            var statusItems = new ObservableCollection<string>();
            StatusList.ItemsSource = statusItems;

            // Validations
            var isSuccess = true;
            isSuccess = UiHelper.ValidateMandatory(statusItems, WebAppName, "You must provide a web app name") && isSuccess;
            isSuccess = UiHelper.ValidateMandatory(statusItems, HostName, "You must provide an host name") && isSuccess;
            isSuccess = UiHelper.ValidateMandatory(statusItems, ResourceGroupName, "You must provide a resource group name") && isSuccess;
            isSuccess = UiHelper.ValidateMandatory(statusItems, AppServicePlan, "You must select an app service plan") && isSuccess;
            if (!isSuccess)
            {
                statusItems.Add("Stop processing");
                return;
            }

            // Prepare settings
            var settings = new Dictionary<string, string>();

            settings["DOCKER_REGISTRY_SERVER_URL"] = "https://index.docker.io/v1";

            if (FilesystemAppServiceStorage.IsChecked == true)
            {
                settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"] = "true";
            }

            AddSettingsIfPresent(settings, "EMAIL_DEFAULT_FROM_ADDRESS", EmailRelayDefaultEmail.Text);
            AddSettingsIfPresent(settings, "EMAIL_HOSTNAME", EmailRelayHostname.Text);
            AddSettingsIfPresent(settings, "EMAIL_PORT", EmailRelayPort.Text);
            AddSettingsIfPresent(settings, "EMAIL_USER", EmailRelayUser.Text);
            AddSettingsIfPresent(settings, "EMAIL_PASSWORD", EmailRelayPassword.Text);

            AddSettingsIfPresent(settings, "PHP_MAX_EXECUTION_TIME_SEC", PhpMaxExecutionTimeSec.Text);
            AddSettingsIfPresent(settings, "PHP_MAX_UPLOAD_FILESIZE_MB", PhpMaxUploadFilesizeMb.Text);
            AddSettingsIfPresent(settings, "PHP_MAX_MEMORY_LIMIT_MB", PhpMaxMemoryLimitMb.Text);

            // Create Webapp
            try
            {
                await AzWebAppsApiClient.CreateWebApp(WebAppName.Text, (string)PhpVersions.SelectionBoxItem,
                     HostName.Text, ResourceGroupName.Text, ((AzAppServicePlan)AppServicePlan.SelectedItem).Id,
                     settings, statusItems);
                statusItems.Add("[OK] Webapp creation completed");
            }
            catch (DefaultErrorResponseException ex)
            {
                statusItems.Add($"[ERROR] Webapp creation issue: {ex.Message} - {ex.Response.Content}");
                return;
            }
            catch (Exception ex)
            {
                statusItems.Add($"[ERROR] Webapp creation issue: {ex.Message}");
                return;
            }

            // Refresh list
            await _applicationModel.RefreshWebAppsAsync(true);
            statusItems.Add("Refresh completed");
        }

        private static void AddSettingsIfPresent(IDictionary<string, string> settings, string name, string value)
        {
            if (value.Length > 0)
            {
                settings[name] = value;
            }
        }

        private void EmailRelay_OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (EmailRelay.SelectedItem == null) return;

            var azEmailAccount = (AzEmailAccount)EmailRelay.SelectedItem;
            EmailRelayDefaultEmail.Text = azEmailAccount.DefaultEmail;
            EmailRelayHostname.Text = azEmailAccount.Hostname;
            EmailRelayPort.Text = azEmailAccount.Port.ToString();
            EmailRelayUser.Text = azEmailAccount.User;
            EmailRelayPassword.Text = azEmailAccount.Password;
        }

        private void ResourceGroupName_OnSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ResourceGroupName.Text = (string)ResourceGroups.SelectedItem;
        }

    }
}
