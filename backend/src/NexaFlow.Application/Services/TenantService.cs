using NexaFlow.Application.Common.Exceptions;
using NexaFlow.Application.Common.Interfaces;
using NexaFlow.Application.DTOs.Tenants;
using NexaFlow.Application.Interfaces;
using NexaFlow.Domain.Entities;
using NexaFlow.Domain.Interfaces;

namespace NexaFlow.Application.Services;

public sealed class TenantService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : ITenantService
{
    public async Task<TenantResponse> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var tenant = await GetCurrentTenantAsync(cancellationToken);
        return Map(tenant);
    }

    public async Task<TenantResponse> UpdateCurrentAsync(UpdateTenantRequest request, CancellationToken cancellationToken = default)
    {
        var tenant = await GetCurrentTenantAsync(cancellationToken);
        tenant.Name = request.Name.Trim();
        tenant.UpdatedAtUtc = DateTime.UtcNow;

        unitOfWork.Repository<Tenant>().Update(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(tenant);
    }

    private async Task<Tenant> GetCurrentTenantAsync(CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.TenantId ?? throw new AuthenticationException("No authenticated tenant.");
        return await unitOfWork.Repository<Tenant>().GetByIdAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), tenantId);
    }

    private static TenantResponse Map(Tenant tenant) =>
        new(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, tenant.CreatedAtUtc);
}
