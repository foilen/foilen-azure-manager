using core;
using core.AzureApi;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace ConsoleApp
{

    public class Program
    {
        public static async Task Main()
        {

            await ApplicationManager.InitAsync();
            foreach (var item in await AzGlobalStore.ResourceGroupsUsedAsync())
            {
                Console.WriteLine(item);
            }

        }
    }

}