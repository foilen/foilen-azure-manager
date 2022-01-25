using core.AzureApi;

namespace core
{

    public class ApplicationManager
    {

        public static async Task InitAsync()
        {
            await AzLoginClient.LogInIfNeededAsync();
        }

    }
}

