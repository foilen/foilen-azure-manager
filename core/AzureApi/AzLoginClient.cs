using System.ComponentModel;
using core.services;
using Microsoft.Azure.Management.Fluent;

namespace core.AzureApi;

public class AzLoginClient : IAzLoginClient
{
    private readonly IProfileManager _profileManager;

    public AzLoginClient(IProfileManager profileManager)
    {
        _profileManager = profileManager;
    }

    public async Task LogInIfNeededAsync()
    {
        var azProfileFile = _profileManager.GetAzFilePath("azureProfile.json");
        var authFile = _profileManager.GetAzFilePath("service_principal_auth.json");

        // Log in
        if (!File.Exists(azProfileFile))
        {
            Console.WriteLine("Not logged in. Running az login");
            try
            {
                var processStartInfo = new System.Diagnostics.ProcessStartInfo("az", "login")
                {
                    UseShellExecute = true,
                };
                var process = new System.Diagnostics.Process();
                process.StartInfo = processStartInfo;
                process.Start();

                await process.WaitForExitAsync();
            }
            catch (Win32Exception)
            {
                throw new AzNotInstalledException();
            }
        }

        // Create service principal
        if (!File.Exists(authFile))
        {
            Console.WriteLine("No service principal created. Running az ad sp create-for-rbac");
            var processStartInfo = new System.Diagnostics.ProcessStartInfo("az", "ad sp create-for-rbac --sdk-auth > " + authFile)
            {
                UseShellExecute = true,
            };
            var process = new System.Diagnostics.Process();
            process.StartInfo = processStartInfo;
            process.Start();

            await process.WaitForExitAsync();
        }
    }

    public IAzure GetAzure()
    {
        var authFile = _profileManager.GetAzFilePath("service_principal_auth.json");

        return Microsoft.Azure.Management.Fluent.Azure.Authenticate(authFile)
            .WithDefaultSubscription();
    }
}