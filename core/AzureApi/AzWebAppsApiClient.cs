using System.Collections.ObjectModel;
using System.Text.Json;
using core.AzureApi.model;
using core.services;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Newtonsoft.Json.Linq;

namespace core.AzureApi;

public class AzWebAppsApiClient : IAzWebAppsApiClient
{
    private readonly IAzDnsZonesApiClient _azDnsZonesApiClient;
    private readonly IAzLoginClient _azLoginClient;
    private readonly IDnsService _dnsService;
    private readonly IProfileManager _profileManager;

    public AzWebAppsApiClient(IAzDnsZonesApiClient azDnsZonesApiClient, IAzLoginClient azLoginClient,
        IDnsService dnsService, IProfileManager profileManager)
    {
        _azDnsZonesApiClient = azDnsZonesApiClient;
        _azLoginClient = azLoginClient;
        _dnsService = dnsService;
        _profileManager = profileManager;
    }

    public async Task CreateWebApp(string webAppName, string dockerImageAndTag, IList<string> hostNames,
        string resourceGroupName, string appServicePlanId, Dictionary<string, string> settings,
        IList<string>? statusCollection = null)
    {
        var appServicePlan = await _azLoginClient.GetAzure().AppServices.AppServicePlans
            .GetByIdAsync(appServicePlanId);

        AzApiClientHelper.PrintStatus(statusCollection,
            $"Create the WebApp {webAppName} with hostnames {string.Join(", ", hostNames)} in existing resource group {resourceGroupName}");

        var newWebApp = await _azLoginClient.GetAzure().WebApps
            .Define(webAppName)
            .WithExistingLinuxPlan(appServicePlan)
            .WithExistingResourceGroup(resourceGroupName)
            .WithPublicDockerHubImage(dockerImageAndTag)
            .WithAppSettings(settings)
            .WithHttpsOnly(true)
            .CreateAsync();
        AzApiClientHelper.PrintStatus(statusCollection, $"[OK] Created the WebApp {webAppName}");
        Cache(newWebApp, settings);

        var resourceGraphClient = await _azLoginClient.GetResourceGraphClientAsync();
        string? asuid = null;
        await retryAsync(async () =>
            {
                AzApiClientHelper.PrintStatus(statusCollection, $"Get the asuid for webapp {webAppName}");
                var request = new QueryRequest
                {
                    Query =
                        $"project name, properties.customDomainVerificationId, type | where type == 'microsoft.web/sites' and name == '{webAppName}'"
                };
                var asuidResult = await resourceGraphClient.ResourcesAsync(request);
                if (asuidResult.Count != 1)
                {
                    AzApiClientHelper.PrintStatus(statusCollection, "[RETRY] The query to get the asuid for webapp didn't return any value");
                    throw new Exception(
                        "The query to get the asuid for webapp didn't return any value");
                }

                var asuidData = (JArray) asuidResult.Data;
                foreach (var jToken in asuidData[0])
                {
                    var nextData = (JProperty) jToken;
                    if (nextData.Name == "properties_customDomainVerificationId")
                    {
                        asuid = (string) nextData.Value;
                    }
                }

                if (asuid == null)
                {
                    AzApiClientHelper.PrintStatus(statusCollection, $"[ERROR] Could not get the customDomainVerificationId for the webapp {webAppName}");
                    throw new Exception(
                        $"Could not get the customDomainVerificationId for the webapp {webAppName}");
                }
            },
            TimeSpan.FromMinutes(1), "The query to get the asuid for webapp didn't return any value");

        AzApiClientHelper.PrintStatus(statusCollection, $"[OK] asuid for webapp {webAppName} to use is {asuid}");

        // Create all asuids TXT Entries
        foreach (var hostName in hostNames)
        {
            var txtHostname = $"asuid.{hostName}";
            AzApiClientHelper.PrintStatus(statusCollection,
                $"Create a TXT record for {txtHostname} -> {asuid}");
            await _azDnsZonesApiClient.SetTxtRecordAsync(txtHostname, asuid, statusCollection);
            AzApiClientHelper.PrintStatus(statusCollection,
                $"[OK] Create a TXT record for {txtHostname} -> {asuid}");
        }

        // Create all DNS Entries
        try
        {
            foreach (var hostName in hostNames)
            {
                var dnsZone = await _azDnsZonesApiClient.FindDnsZoneForHostAsync(hostName);

                if (dnsZone.Name == hostName)
                {
                    // Root entry (cannot be CNAME; must be A)
                    var cnameValue = newWebApp.DefaultHostName;
                    var ips = await _dnsService.GetAAsync(cnameValue);
                    AzApiClientHelper.PrintStatus(statusCollection,
                        $"Add DNS Entry {hostName} -> {string.Join(", ", ips)}");
                    await _azDnsZonesApiClient.SetARecordAsync(hostName, ips, statusCollection);
                    AzApiClientHelper.PrintStatus(statusCollection,
                        $"[OK] DNS Entry creation completed {hostName} -> {string.Join(", ", ips)}");
                }
                else
                {
                    // Subdomain (can be CNAME)
                    var cnameValue = newWebApp.DefaultHostName;
                    AzApiClientHelper.PrintStatus(statusCollection, $"Add DNS Entry {hostName} -> {cnameValue}");
                    await _azDnsZonesApiClient.SetCnameRecordAsync(hostName, cnameValue, statusCollection);
                    AzApiClientHelper.PrintStatus(statusCollection,
                        $"[OK] DNS Entry creation completed {hostName} -> {cnameValue}");
                }
            }
        }
        catch (Exception ex)
        {
            AzApiClientHelper.PrintStatus(statusCollection, $"[ERROR] DNS Entry creation issue: {ex.Message}");
            return;
        }

        // Wait for all DNS Entries to be visible
        foreach (var hostName in hostNames)
        {
            // asuid
            var txtHostname = $"asuid.{hostName}";
            if (!await _dnsService.WaitForTxtAsync(txtHostname, asuid,
                    TimeSpan.FromSeconds(5), 120 / 5,
                    statusCollection))
            {
                AzApiClientHelper.PrintStatus(statusCollection,
                    $"[ERROR] Could not see the TXT record for {txtHostname} after 2 minutes");
                throw new Exception($"Could not see the TXT record for {txtHostname}");
            }

            // Site
            var dnsZone = await _azDnsZonesApiClient.FindDnsZoneForHostAsync(hostName);
            if (dnsZone.Name == hostName)
            {
                // Root entry (cannot be CNAME; must be A)
                var cnameValue = newWebApp.DefaultHostName;
                var ips = await _dnsService.GetAAsync(cnameValue);
                if (!await _dnsService.WaitForAAsync(hostName, ips, TimeSpan.FromSeconds(5), 120 / 5,
                        statusCollection))
                {
                    AzApiClientHelper.PrintStatus(statusCollection,
                        $"[ERROR] Could not see the A record for {hostName}");
                    return;
                }
            }
            else
            {
                // Subdomain (can be CNAME)
                var cnameValue = newWebApp.DefaultHostName;
                if (!await _dnsService.WaitForCnameAsync(hostName, cnameValue, TimeSpan.FromSeconds(5),
                        120 / 5,
                        statusCollection))
                {
                    AzApiClientHelper.PrintStatus(statusCollection,
                        $"[ERROR] Could not see the CNAME record for {hostName}");
                    return;
                }
            }
        }

        // Add the custom domains to the webapp
        foreach (var hostName in hostNames)
        {
            AzApiClientHelper.PrintStatus(statusCollection, $"WebApp {webAppName}, add hostname {hostName}");
            var dnsZone = await _azDnsZonesApiClient.FindDnsZoneForHostAsync(hostName);

            if (dnsZone.Name == hostName)
            {
                newWebApp = await newWebApp.Update()
                    .DefineHostnameBinding()
                    .WithThirdPartyDomain(dnsZone.Name)
                    .WithSubDomain(hostName)
                    .WithDnsRecordType(CustomHostNameDnsRecordType.A)
                    .Attach()
                    .ApplyAsync();
            }
            else
            {
                newWebApp = await newWebApp.Update()
                    .DefineHostnameBinding()
                    .WithThirdPartyDomain(dnsZone.Name)
                    .WithSubDomain(hostName)
                    .WithDnsRecordType(CustomHostNameDnsRecordType.CName)
                    .Attach()
                    .ApplyAsync();
            }

            AzApiClientHelper.PrintStatus(statusCollection,
                $"[OK] WebApp {webAppName}, added hostname {hostName}");
            Cache(newWebApp, settings);
        }

        // Create App Service Managed Certificate
        foreach (var hostName in hostNames)
        {
            try
            {
                AzApiClientHelper.PrintStatus(statusCollection,
                    $"Create App Service Managed Certificate for {hostName}");
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
                AzApiClientHelper.PrintStatus(statusCollection,
                    "[OK] App Service Managed Certificate creation completed for {hostName}");
            }
            catch (DefaultErrorResponseException ex)
            {
                if (ex.Message.Contains("Accepted"))
                {
                    AzApiClientHelper.PrintStatus(statusCollection,
                        $"[OK] App Service Managed Certificate creation completed for {hostName}");
                }
                else
                {
                    AzApiClientHelper.PrintStatus(statusCollection,
                        $"[ERROR] App Service Managed Certificate creation issue for {hostName}: {ex.Message}");
                    return;
                }
            }
            catch (Exception ex)
            {
                AzApiClientHelper.PrintStatus(statusCollection,
                    $"[ERROR] App Service Managed Certificate creation issue for {hostName}: {ex.Message}");
                return;
            }
        }

        // Add binding
        foreach (var hostName in hostNames)
        {
            try
            {
                await retryAsync(async () =>
                {
                    AzApiClientHelper.PrintStatus(statusCollection,
                        $"Create Certificate binding for host {hostName}");
                    newWebApp = await newWebApp.Update()
                        .DefineSslBinding()
                        .ForHostname(hostName)
                        .WithExistingCertificate(hostName)
                        .WithSniBasedSsl()
                        .Attach()
                        .ApplyAsync();
                }, TimeSpan.FromMinutes(1), "Object reference not set to an instance of an object.");
            }
            catch (Exception ex)
            {
                AzApiClientHelper.PrintStatus(statusCollection,
                    $"[ERROR] Certificate binding creation issue for {hostName}: {ex.Message}");
                return;
            }
        }
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

    private void Cache(IWebApp newWebApp, Dictionary<string, string> settings)
    {
        _profileManager.SaveNewToJsonFolder("cache-webapps", ToAzWebApp(newWebApp, settings),
            item => item.Id.Replace('/', '_')
        );
    }

    private static AzWebApp ToAzWebApp(IWebApp webApp, Dictionary<string, string> settings)
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

    private static async Task retryAsync(Func<Task> action, TimeSpan retryDelay, params string[] exceptionMessagesToRetry)
    {
        while (true)
        {
            try
            {
                await action.Invoke();
                return;
            }
            catch (Exception e)
            {
                var toRetry = false;
                foreach (var exceptionMessageToRetry in exceptionMessagesToRetry)
                {
                    if (e.Message == exceptionMessageToRetry)
                    {
                        toRetry = true;
                        break;
                    }
                }

                if (!toRetry)
                {
                    throw;
                }
            }

            await Task.Delay(retryDelay);
        }
    }
}