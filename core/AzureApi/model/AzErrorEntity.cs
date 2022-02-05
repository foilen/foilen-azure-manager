namespace core.AzureApi.model;

public readonly record struct AzErrorEntity(
    string ExtendedCode,
    string MessageTemplate,
    List<string> Parameters,
    string Code, string Message
);