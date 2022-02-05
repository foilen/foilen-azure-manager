using core.AzureApi.model;

namespace core.AzureApi;

public interface IAzAppServicePlansApiClient
{
    Task<List<AzAppServicePlan>> ListAzAppServicePlansAsync(bool forceRefresh = false);
}