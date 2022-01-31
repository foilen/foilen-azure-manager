﻿using System.Collections.ObjectModel;
using DnsClient;

namespace core.AzureApi
{
    public static class DnsService
    {

        public static async Task<List<string>> GetTxt(string hostName)
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

        public static async Task<bool> WaitForTxt(string hostname, string expectedValue, TimeSpan waitBetweenRetries, int maxRetries, Collection<string>? statusCollection = null)
        {

            while (maxRetries > 0)
            {
                --maxRetries;

                AzApiClientHelper.PrintStatus(statusCollection, $"[DNS] Check {hostname} TXT");
                var values = await GetTxt(hostname);
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
}