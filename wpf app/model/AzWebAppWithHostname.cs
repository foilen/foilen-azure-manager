using core.AzureApi.model;

namespace wpf_app.model
{
    public readonly record struct AzWebAppWithHostname(string HostName, AzWebApp AzWebApp);
}