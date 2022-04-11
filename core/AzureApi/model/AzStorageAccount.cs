namespace core.AzureApi.model;

public readonly record struct AzStorageAccount(
    string Id, // IHasId
    string Name, // IHasName
    string ResourceGroupName, // IHasResourceGroup
    string Type, string RegionName, IReadOnlyDictionary<string, string> Tags, // IResource

    // IHasInner<StorageAccountInner>
    string Sku,
    string Kind,
    string AccessTier
);