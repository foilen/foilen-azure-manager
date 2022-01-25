using System.ComponentModel;
using Microsoft.Azure.Management.Fluent;

namespace core.AzureApi
{
    public static class AzLoginClient
    {
        public static async Task LogInIfNeededAsync()
        {
            var azProfileFile = ProfileManager.GetAzFilePath("azureProfile.json");
            var authFile = ProfileManager.GetAzFilePath("service_principal_auth.json");

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

        public static IAzure GetAzure()
        {
            var authFile = ProfileManager.GetAzFilePath("service_principal_auth.json");

            return Microsoft.Azure.Management.Fluent.Azure.Authenticate(authFile)
                .WithDefaultSubscription();
        }
    }
}
