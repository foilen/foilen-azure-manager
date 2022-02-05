namespace core.AzureApi.model;

public readonly record struct AzEmailAccount(
    string DefaultEmail,
    string Hostname, int Port,
    string User, string Password
);