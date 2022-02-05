namespace core.AzureApi;

[Serializable]
internal class AzNotInstalledException : Exception
{
    public AzNotInstalledException() : base("Azure CLI not installed. Get it on https://aka.ms/installazurecliwindows")
    {
    }
}