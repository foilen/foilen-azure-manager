namespace core.AzureApi.model
{
    public readonly record struct AzResourceGroup(
        string Id, // IHasId
        string Name, // IHasName
        string RegionName // IResource
    );
}
