using System.Collections.ObjectModel;
using System.Text.Json;
using core.AzureApi.model;
using core.services;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.AppService.Fluent.Models;

namespace core.AzureApi;

public class AzWebAppsApiClient : IAzWebAppsApiClient
{
    private readonly IAzDnsZonesApiClient _azDnsZonesApiClient;
    private readonly IAzLoginClient _azLoginClient;
    private readonly IDnsService _dnsService;
    private readonly IProfileManager _profileManager;

    public AzWebAppsApiClient(IAzDnsZonesApiClient azDnsZonesApiClient, IAzLoginClient azLoginClient, IDnsService dnsService, IProfileManager profileManager)
    {
        _azDnsZonesApiClient = azDnsZonesApiClient;
        _azLoginClient = azLoginClient;
        _dnsService = dnsService;
        _profileManager = profileManager;
    }

    public async Task CreateWebApp(string webAppName, string dockerImageAndTag, string hostName, string resourceGroupName, string appServicePlanId, Dictionary<string, string> settings, Collection<string>? statusCollection = null)
    {
        var retriesLeft = 2;
        while (retriesLeft > 0)
        {
            --retriesLeft;

            var appServicePlan = await _azLoginClient.GetAzure().AppServices.AppServicePlans
                .GetByIdAsync(appServicePlanId);

            AzApiClientHelper.PrintStatus(statusCollection, $"Create the WebApp {webAppName} with hostname {hostName} in existing resource group {resourceGroupName}");
            try
            {
                var newWebApp = await _azLoginClient.GetAzure().WebApps
                    .Define(webAppName)
                    .WithExistingLinuxPlan(appServicePlan)
                    .WithExistingResourceGroup(resourceGroupName)
                    .WithPublicDockerHubImage(dockerImageAndTag)
                    .WithAppSettings(settings)
                    .WithHttpsOnly(true)
                    .WithThirdPartyHostnameBinding("foilen-lab.me", hostName)
                    .CreateAsync();

                // Cache newWebApp
                _profileManager.SaveNewToJsonFolder("cache-webapps", ToAzWebApp(newWebApp, settings),
                    item => item.Id.Replace('/', '_')
                );


                // Create DNS Entry
                try
                {
                    // TODO Create Webapp - Handle root case (cannot be CNAME; must be A)
                    var cnameValue = newWebApp.DefaultHostName;
                    await _azDnsZonesApiClient.SetCnameRecordAsync(hostName, cnameValue, statusCollection);
                    AzApiClientHelper.PrintStatus(statusCollection, "[OK] DNS Entry creation completed");
                    // DNS - Wait until visible
                    if (!await _dnsService.WaitForCnameAsync(hostName, cnameValue, TimeSpan.FromSeconds(5), 120 / 5, statusCollection))
                    {
                        AzApiClientHelper.PrintStatus(statusCollection, $"[ERROR] Could not see the CNAME record");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    AzApiClientHelper.PrintStatus(statusCollection, $"[ERROR] DNS Entry creation issue: {ex.Message}");
                    return;
                }

                // Create App Service Managed Certificate
                try
                {
                    AzApiClientHelper.PrintStatus(statusCollection, "Create App Service Managed Certificate");
                    var certificateDefinition = newWebApp.Manager.AppServiceCertificates
                        .Define(hostName)
                        .WithRegion(newWebApp.Region)
                        .WithExistingResourceGroup(newWebApp.ResourceGroupName)
                        .WithExistingCertificateOrder(null);
                    var innerCertificate = ((IAppServiceCertificate) certificateDefinition).Inner;
                    innerCertificate.ServerFarmId = newWebApp.AppServicePlanId;
                    innerCertificate.CanonicalName = hostName;
                    innerCertificate.Password = "";

                    await certificateDefinition.CreateAsync();
                    AzApiClientHelper.PrintStatus(statusCollection, "[OK] App Service Managed Certificate creation completed");
                }
                catch (DefaultErrorResponseException ex)
                {
                    if (ex.Message.Contains("Accepted"))
                    {
                        AzApiClientHelper.PrintStatus(statusCollection, "[OK] App Service Managed Certificate creation completed");
                    }
                    else
                    {
                        AzApiClientHelper.PrintStatus(statusCollection, $"[ERROR] App Service Managed Certificate creation issue: {ex.Message}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    AzApiClientHelper.PrintStatus(statusCollection, $"[ERROR] App Service Managed Certificate creation issue: {ex.Message}");
                    return;
                }

                // Add binding
                try
                {
                    AzApiClientHelper.PrintStatus(statusCollection, $"Create Certificate binding for host {hostName}");
                    await newWebApp.Update()
                        .DefineSslBinding()
                        .ForHostname(hostName)
                        .WithExistingCertificate(hostName)
                        .WithSniBasedSsl()
                        .Attach()
                        .ApplyAsync();
                }
                catch (Exception ex)
                {
                    AzApiClientHelper.PrintStatus(statusCollection,
                        $"[ERROR] Certificate binding creation issue: {ex.Message}");
                    return;
                }

                return;
            }
            catch (DefaultErrorResponseException ex)
            {
                var error = JsonSerializer.Deserialize<AzErrorResponse>(ex.Response.Content);
                AzApiClientHelper.PrintStatus(statusCollection,
                    $"[ERROR] Webapp creation issue: {ex.Message} - {error.Code} - {error.Message}");

                // Check if missing a TXT record for asuid
                var foundTxtRecord = false;
                foreach (var detail in error.Details)
                {
                    if (detail.ErrorEntity != null)
                    {
                        var errorEntity = detail.ErrorEntity.Value;
                        if (errorEntity.ExtendedCode == "04006" && errorEntity.Parameters.Count == 2)
                        {
                            var txtHostname = $"asuid.{errorEntity.Parameters[0]}";
                            var txtValue = errorEntity.Parameters[1];
                            AzApiClientHelper.PrintStatus(statusCollection, $"[MISSING] Need to create a TXT record for {txtHostname} -> {txtValue}. Will try to create it");

                            foundTxtRecord = true;
                            await _azDnsZonesApiClient.SetTxtRecordAsync(txtHostname, txtValue, statusCollection);

                            // DNS - Wait until visible
                            if (!await _dnsService.WaitForTxtAsync(txtHostname, txtValue, TimeSpan.FromSeconds(5), 120 / 5, statusCollection))
                            {
                                AzApiClientHelper.PrintStatus(statusCollection, $"[ERROR] Could not see the TXT record");
                                throw new Exception($"Could not create the webapp because could not see the TXT record for {txtHostname}");
                            }
                        }
                    }
                }

                if (!foundTxtRecord)
                {
                    retriesLeft = 0;
                }
            }
        }

        throw new Exception("Could not create the webapp");
    }

    public async Task<List<AzWebApp>> ListWebAppsAsync(bool forceRefresh = false)
    {
        // Get from cache if available
        if (!forceRefresh)
        {
            var cached = _profileManager.LoadFromJsonFolder<AzWebApp>("cache-webapps");
            if (cached != null)
            {
                Console.WriteLine("ListWebApps from the cache");
                return cached;
            }
        }


        // Get from API
        Console.WriteLine("ListWebApps from the API");
        var webAppsEnumerable = await _azLoginClient.GetAzure().WebApps
            .ListAsync();

        // Persist in cache
        var items = new List<AzWebApp>();
        foreach (var webApp in webAppsEnumerable)
        {
            // Get the app settings
            var webAppSettings = await webApp.GetAppSettingsAsync();
            var settings = new Dictionary<String, String>();
            foreach (var appSetting in webAppSettings.Values)
            {
                settings[appSetting.Key] = appSetting.Value;
            }

            // Transform
            items.Add(ToAzWebApp(webApp, settings));
        }

        _profileManager.SaveToJsonFolder("cache-webapps", items,
            item => item.Id.Replace('/', '_')
        );

        return items;
    }

    private static AzWebApp ToAzWebApp(IWebApp webApp, Dictionary<String, String> settings)
    {
        return new AzWebApp()
        {
            Id = webApp.Id,
            Name = webApp.Name,
            ResourceGroupName = webApp.ResourceGroupName,

            Type = webApp.Type,
            RegionName = webApp.RegionName,
            Tags = webApp.Tags,

            State = webApp.Inner.State,
            HostNames = (List<string>) webApp.Inner.HostNames,
            Enabled = webApp.Inner.Enabled,
            HostNameSslStates = (List<HostNameSslState>) webApp.Inner.HostNameSslStates,
            SiteConfig = webApp.Inner.SiteConfig,
            OutboundIpAddresses = webApp.Inner.OutboundIpAddresses,
            HttpsOnly = webApp.Inner.HttpsOnly,

            Settings = settings
        };
    }
}