namespace core.AzureApi.model;

public readonly record struct AzErrorDetail(
    string? Message,
    string? Code,
    AzErrorEntity? ErrorEntity
);