using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using core;
using core.AzureApi;
using wpf_app.model;

namespace wpf_app.window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private ApplicationModel GetApplicationModel()
        {
            return ((ApplicationModel)DataContext);
        }

        private async void InitDataAsync(object sender, RoutedEventArgs e)
        {
            await ApplicationManager.InitAsync();

            await GetApplicationModel().RefreshAppServicePlansAsync(false);
            await GetApplicationModel().RefreshDnsZonesAsync(false);
            await GetApplicationModel().RefreshResourceGroupsAsync(false);
            await GetApplicationModel().RefreshWebAppsAsync(false);

        }

        // DNS Zone
        private void DnsZoneCreate(object sender, RoutedEventArgs e)
        {
            new DnsZoneCreateWindow(GetApplicationModel()).Show();
        }

        private async void DnsZonesRefreshAsync(object sender, RoutedEventArgs e)
        {
            await GetApplicationModel().RefreshDnsZonesAsync(true);
        }

        // Resource Groups
        private async void ResourceGroupsRefreshAsync(object sender, RoutedEventArgs e)
        {
            await GetApplicationModel().RefreshResourceGroupsAsync(true);
        }

        // App Service Plan
        private void AppServicePlansCreate(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://portal.azure.com/#create/Microsoft.AppServicePlanCreate",
                UseShellExecute = true
            });
        }

        private async void AppServicePlansRefreshAsync(object sender, RoutedEventArgs e)
        {
            await GetApplicationModel().RefreshAppServicePlansAsync(true);
        }

        // Web Apps
        private async void WebAppsRefreshAsync(object sender, RoutedEventArgs e)
        {
            await GetApplicationModel().RefreshWebAppsAsync(true);
        }

        private void WebAppClone(object sender, MouseButtonEventArgs e)
        {
            var item = (AzWebAppWithHostname)WebApps.SelectedItem;

            // TODO WebAppClone - Support other languages
            new PhpCreateWindow(GetApplicationModel(), item).Show();
        }

        // PHP
        private void PhpCreateWebApp(object sender, RoutedEventArgs e)
        {
            new PhpCreateWindow(GetApplicationModel()).Show();
        }
    }
}
