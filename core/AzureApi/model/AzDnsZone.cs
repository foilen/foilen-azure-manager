namespace core.AzureApi.model
{
    public readonly record struct AzDnsZone(
        string Id, // IHasId
        string Name, // IHasName
        string ResourceGroupName, // IHasResourceGroup
        string Type, string RegionName, IReadOnlyDictionary<string, string> Tags, // IResource

        // IHasInner<ZoneInner>
        List<string> NameServers

    );
}
