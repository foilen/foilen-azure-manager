namespace core.AzureApi.model;

public readonly record struct AzErrorResponse(
    string Code, string Message,
    List<AzErrorDetail> Details
);