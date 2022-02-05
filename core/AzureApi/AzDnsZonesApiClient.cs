using System.Collections.ObjectModel;
using core.AzureApi.model;
using core.services;
using Microsoft.Azure.Management.Dns.Fluent;

namespace core.AzureApi;

public class AzDnsZonesApiClient : IAzDnsZonesApiClient
{
    private readonly IAzLoginClient _azLoginClient;
    private readonly IProfileManager _profileManager;

    public AzDnsZonesApiClient(IAzLoginClient azLoginClient, IProfileManager profileManager)
    {
        _azLoginClient = azLoginClient;
        _profileManager = profileManager;
    }

    public async Task CreateDnsZone(string hostName, string resourceGroupName, Collection<string>? statusCollection = null)
    {
        AzApiClientHelper.PrintStatus(statusCollection,
            $"Create the DNS Zone {hostName} in existing resource group {resourceGroupName}");
        var newDnsZone = await _azLoginClient.GetAzure().DnsZones
            .Define(hostName)
            .WithExistingResourceGroup(resourceGroupName)
            .CreateAsync();


        // Cache newDnsZone
        _profileManager.SaveNewToJsonFolder("cache-dnszones", ToAzDnsZone(newDnsZone),
            item => item.Id.Replace('/', '_')
        );
    }

    public async Task<List<AzDnsZone>> ListDnsZonesAsync(bool forceRefresh = false, Collection<string>? statusCollection = null)
    {
        // Get from cache if available
        if (!forceRefresh)
        {
            var cached = _profileManager.LoadFromJsonFolder<AzDnsZone>("cache-dnszones");
            if (cached != null)
            {
                AzApiClientHelper.PrintStatus(statusCollection, "ListDnsZones from the cache");
                return cached;
            }
        }


        // Get from API
        AzApiClientHelper.PrintStatus(statusCollection, "ListDnsZones from the API");
        var dnsZonesEnumerable = await _azLoginClient.GetAzure().DnsZones
            .ListAsync();

        // Persist in cache
        var items = new List<AzDnsZone>();
        foreach (var dnsZone in dnsZonesEnumerable)
        {
            items.Add(ToAzDnsZone(dnsZone));
        }

        _profileManager.SaveToJsonFolder("cache-dnszones", items,
            item => item.Id.Replace('/', '_')
        );

        return items;
    }

    public async Task SetCnameRecordAsync(string hostname, string value, Collection<string>? statusCollection = null)
    {
        // Find the DnsZone for that hostname
        var azDnsZone = await FindDnsZoneForHostAsync(hostname, statusCollection);

        // Get the Dns Zone
        AzApiClientHelper.PrintStatus(statusCollection, $"Get DnsZone {azDnsZone.Name}");
        var dnsZone = await _azLoginClient.GetAzure().DnsZones
            .GetByIdAsync(azDnsZone.Id);

        // Delete and add TXT
        var subDomain = hostname.Substring(0, hostname.Length - 1 - azDnsZone.Name.Length);
        AzApiClientHelper.PrintStatus(statusCollection, $"Set the CNAME entry {subDomain} -> {value} in DnsZone {azDnsZone.Name}");
        await dnsZone.Update()
            .WithoutCNameRecordSet(subDomain)
            .ApplyAsync();
        await dnsZone.Update()
            .DefineCNameRecordSet(subDomain)
            .WithAlias(value)
            .WithTimeToLive(300)
            .Attach()
            .ApplyAsync();
    }

    public async Task SetTxtRecordAsync(string hostname, string value, Collection<string>? statusCollection = null)
    {
        // Find the DnsZone for that hostname
        var azDnsZone = await FindDnsZoneForHostAsync(hostname, statusCollection);

        // Get the Dns Zone
        AzApiClientHelper.PrintStatus(statusCollection, $"Get DnsZone {azDnsZone.Name}");
        var dnsZone = await _azLoginClient.GetAzure().DnsZones
            .GetByIdAsync(azDnsZone.Id);

        // Delete and add TXT
        var subDomain = hostname.Substring(0, hostname.Length - 1 - azDnsZone.Name.Length);
        AzApiClientHelper.PrintStatus(statusCollection, $"Set the TXT entry {subDomain} -> {value} in DnsZone {azDnsZone.Name}");
        await dnsZone.Update()
            .WithoutTxtRecordSet(subDomain)
            .ApplyAsync();
        await dnsZone.Update()
            .DefineTxtRecordSet(subDomain)
            .WithText(value)
            .WithTimeToLive(300)
            .Attach()
            .ApplyAsync();
    }

    private async Task<AzDnsZone> FindDnsZoneForHostAsync(string hostname, Collection<string>? statusCollection = null)
    {
        if (hostname.Length == 0)
        {
            throw new Exception("Could not find a Dns Zone");
        }

        AzApiClientHelper.PrintStatus(statusCollection, $"Search for DnsZone for host {hostname}");
        foreach (var dnsZone in await ListDnsZonesAsync())
        {
            if (dnsZone.Name == hostname)
            {
                return dnsZone;
            }
        }

        var hostnameParts = hostname.Split('.', 2);
        if (hostnameParts.Length == 1)
        {
            throw new Exception("Could not find a Dns Zone");
        }

        return await FindDnsZoneForHostAsync(hostnameParts[1], statusCollection);
    }

    private static AzDnsZone ToAzDnsZone(IDnsZone dnsZone)
    {
        return new AzDnsZone()
        {
            Id = dnsZone.Id,
            Name = dnsZone.Name,
            ResourceGroupName = dnsZone.ResourceGroupName,

            Type = dnsZone.Type,
            RegionName = dnsZone.RegionName,
            Tags = dnsZone.Tags,

            NameServers = (List<string>) dnsZone.Inner.NameServers,
        };
    }
}