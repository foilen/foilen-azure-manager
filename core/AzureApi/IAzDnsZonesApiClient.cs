using System.Collections.ObjectModel;
using core.AzureApi.model;

namespace core.AzureApi;

public interface IAzDnsZonesApiClient
{
    Task CreateDnsZone(string hostName, string resourceGroupName, IList<string>? statusCollection = null);
    Task<List<AzDnsZone>> ListDnsZonesAsync(bool forceRefresh = false, IList<string>? statusCollection = null);
    Task SetARecordAsync(string hostname, IList<string> values, IList<string>? statusCollection = null);
    Task SetCnameRecordAsync(string hostname, string value, IList<string>? statusCollection = null);
    Task SetTxtRecordAsync(string hostname, string value, IList<string>? statusCollection = null);
    Task<AzDnsZone> FindDnsZoneForHostAsync(string hostname, IList<string>? statusCollection = null);
}