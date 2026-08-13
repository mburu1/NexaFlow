using NexaFlow.Application.DTOs.Users;

namespace NexaFlow.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserSummaryResponse>> ListAsync(CancellationToken cancellationToken = default);

    Task<UserSummaryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserSummaryResponse> CreateAsync(AdminCreateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserSummaryResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
}
