using System.Collections.ObjectModel;
using core.AzureApi.model;

namespace core.AzureApi;

public interface IAzDnsZonesApiClient
{
    Task CreateDnsZone(string hostName, string resourceGroupName, Collection<string>? statusCollection = null);
    Task<List<AzDnsZone>> ListDnsZonesAsync(bool forceRefresh = false, Collection<string>? statusCollection = null);
    Task SetCnameRecordAsync(string hostname, string value, Collection<string>? statusCollection = null);
    Task SetTxtRecordAsync(string hostname, string value, Collection<string>? statusCollection = null);
}