namespace core.AzureApi.model;

public readonly record struct AzLocation(
    string Id,
    string Name,
    string DisplayName,
    string Latitude, string Longitude
);