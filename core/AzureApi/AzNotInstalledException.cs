namespace core.AzureApi;

[Serializable]
public class AzNotInstalledException : Exception
{
    public AzNotInstalledException() : base("Azure CLI not installed. Get it on https://aka.ms/installazurecliwindows")
    {
    }
}