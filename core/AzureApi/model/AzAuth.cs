namespace core.AzureApi.model;

public readonly record struct AzAuth(
    string subscriptionId,string tenantId,
    string clientId, string clientSecret,
    string activeDirectoryEndpointUrl,
    string activeDirectoryGraphResourceId,
    string galleryEndpointUrl,
    string managementEndpointUrl,
    string resourceManagerEndpointUrl,
    string sqlManagementEndpointUrl
);