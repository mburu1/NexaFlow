using NexaFlow.Application.DTOs.Tenants;

namespace NexaFlow.Application.Interfaces;

public interface ITenantService
{
    Task<TenantResponse> GetCurrentAsync(CancellationToken cancellationToken = default);

    Task<TenantResponse> UpdateCurrentAsync(UpdateTenantRequest request, CancellationToken cancellationToken = default);
}
