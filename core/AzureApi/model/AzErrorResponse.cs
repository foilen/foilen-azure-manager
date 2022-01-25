using Microsoft.Azure.Management.AppService.Fluent.Models;

namespace core.AzureApi.model
{
    public readonly record struct AzErrorResponse(
        string Code, string Message, 
        List<AzErrorDetail> Details
    );
}
