namespace core.AzureApi.model;

public readonly record struct AzAppServicePlan(
    string Id, // IHasId
    string Name, // IHasName
    string ResourceGroupName, // IHasResourceGroup
    string Type, string RegionName, IReadOnlyDictionary<string, string> Tags, // IResource

    int NumberOfWebApps, int MaxInstances
);