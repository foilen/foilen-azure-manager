using Microsoft.Azure.Management.AppService.Fluent.Models;

namespace core.AzureApi.model
{
    public readonly record struct AzWebApp(
        string Id, // IHasId
        string Name, // IHasName
        string ResourceGroupName, // IHasResourceGroup
        string Type, string RegionName, IReadOnlyDictionary<string, string> Tags, // IResource

        // IHasInner<SiteInner>
        string State,
        List<string> HostNames,
        bool? Enabled,
        List<HostNameSslState> HostNameSslStates,
        SiteConfig SiteConfig,
        string OutboundIpAddresses,
        bool? HttpsOnly,

        // Settings
        IReadOnlyDictionary<String, String> Settings
        );
}
