using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using core.AzureApi.model;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Tests.mocks;

namespace core.services.Tests
{
    [TestClass()]
    public class AzGlobalStoreTests
    {
        [TestMethod()]
        public async Task EmailAccountAsyncTest()
        {
            var azWebAppsApiClient = new AzWebAppsApiClientMock();
            azWebAppsApiClient.AzWebApps = new List<AzWebApp>()
            {
                new AzWebApp("id1", "name1", "rg", "Microsoft.Web/sites", "Canada Central",
                    new Dictionary<string, string>(), "Running", new List<string>(), true, new List<HostNameSslState>(),
                    new SiteConfig(), "20.200.64.2", true, new Dictionary<string, string>()),
                
                new AzWebApp("id2", "name2", "rg", "Microsoft.Web/sites", "Canada Central",
                    new Dictionary<string, string>(), "Running", new List<string>(), true, new List<HostNameSslState>(),
                    new SiteConfig(), "20.200.64.2", true, new Dictionary<string, string>()
                    {
                        {"EMAIL_DEFAULT_FROM_ADDRESS", "ex1@example.com"},
                        {"EMAIL_HOSTNAME", "mail.example.com"},
                        {"EMAIL_PORT", "587"},
                        {"EMAIL_USER", "user1"},
                        {"EMAIL_PASSWORD", "pass1"},
                    }),
                
                new AzWebApp("id3", "name3", "rg", "Microsoft.Web/sites", "Canada Central",
                    new Dictionary<string, string>(), "Running", new List<string>(), true, new List<HostNameSslState>(),
                    new SiteConfig(), "20.200.64.2", true, new Dictionary<string, string>()
                    {
                        {"EMAIL_DEFAULT_FROM_ADDRESS", "ex1@example.com"},
                        {"EMAIL_HOSTNAME", "mail.example.com"},
                        {"EMAIL_PORT", "587"},
                        {"EMAIL_USER", "user1"},
                        {"EMAIL_PASSWORD", "pass1"},
                    }), 
                
                new AzWebApp("id4", "name4", "rg", "Microsoft.Web/sites", "Canada Central",
                    new Dictionary<string, string>(), "Running", new List<string>(), true, new List<HostNameSslState>(),
                    new SiteConfig(), "20.200.64.2", true, new Dictionary<string, string>()
                    {
                        {"EMAIL_DEFAULT_FROM_ADDRESS", "ex2@example.com"},
                        {"EMAIL_HOSTNAME", "mail.example.com"},
                        {"EMAIL_PORT", "587"},
                        {"EMAIL_USER", "user1"},
                        {"EMAIL_PASSWORD", "pass2"},
                    }),
                
            };
            
            var azGlobalStore = new AzGlobalStore(default, default, azWebAppsApiClient);

            var actual = await azGlobalStore.EmailAccountAsync();
            
            var expected = new List<AzEmailAccount>
            {
                new AzEmailAccount("ex1@example.com", "mail.example.com", 587, "user1", "pass1"),
                new AzEmailAccount("ex2@example.com", "mail.example.com", 587, "user1", "pass2"),
            };

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}