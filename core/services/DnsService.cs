using System.Collections.ObjectModel;
using core.AzureApi;
using DnsClient;

namespace core.services;

public class DnsService : IDnsService
{
    public async Task<List<string>> GetCnameAsync(string hostName)
    {
        var client = new LookupClient(new LookupClientOptions()
        {
            UseCache = false
        });

        var values = new List<string>();
        var result = await client.QueryAsync(hostName, QueryType.CNAME);
        foreach (var cNameRecord in result.Answers.CnameRecords())
        {
            values.Add(cNameRecord.CanonicalName.Value);
        }

        return values;
    }

    public async Task<List<string>> GetTxtAsync(string hostName)
    {
        var client = new LookupClient(new LookupClientOptions()
        {
            UseCache = false
        });

        var values = new List<string>();
        var result = await client.QueryAsync(hostName, QueryType.TXT);
        foreach (var txtRecord in result.Answers.TxtRecords())
        {
            values.AddRange(txtRecord.Text);
        }

        return values;
    }

    public async Task<bool> WaitForCnameAsync(string hostname, string expectedValue, 
        TimeSpan waitBetweenRetries, int maxRetries,
        Collection<string>? statusCollection = null)
    {
        expectedValue += ".";
        
        while (maxRetries > 0)
        {
            --maxRetries;

            AzApiClientHelper.PrintStatus(statusCollection, $"[DNS] Check {hostname} CNAME");
            var values = await GetCnameAsync(hostname);
            if (values.Contains(expectedValue))
            {
                AzApiClientHelper.PrintStatus(statusCollection, $"[DNS] {hostname} CNAME is as expected with value {expectedValue}");
                return true;
            }

            AzApiClientHelper.PrintStatus(statusCollection, $"[DNS] {hostname} CNAME is not as expected. Current values {string.Join(", ", values)}");
            await Task.Delay(waitBetweenRetries);
        }

        return false;
    }

    public async Task<bool> WaitForTxtAsync(string hostname, string expectedValue, 
        TimeSpan waitBetweenRetries, int maxRetries, 
        Collection<string>? statusCollection = null)
    {
        while (maxRetries > 0)
        {
            --maxRetries;

            AzApiClientHelper.PrintStatus(statusCollection, $"[DNS] Check {hostname} TXT");
            var values = await GetTxtAsync(hostname);
            if (values.Contains(expectedValue))
            {
                AzApiClientHelper.PrintStatus(statusCollection, $"[DNS] {hostname} TXT is as expected with value {expectedValue}");
                return true;
            }

            AzApiClientHelper.PrintStatus(statusCollection, $"[DNS] {hostname} TXT is not as expected. Current values {string.Join(", ", values)}");
            await Task.Delay(waitBetweenRetries);
        }

        return false;
    }
}