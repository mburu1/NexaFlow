using NexaFlow.Application.Common.Exceptions;
using NexaFlow.Application.Common.Interfaces;
using NexaFlow.Application.DTOs.Users;
using NexaFlow.Application.Interfaces;
using NexaFlow.Domain.Entities;
using NexaFlow.Domain.Enums;
using NexaFlow.Domain.Interfaces;

namespace NexaFlow.Application.Services;

public sealed class UserService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService) : IUserService
{
    public async Task<IReadOnlyList<UserSummaryResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var users = await unitOfWork.Repository<User>().ListAsync(cancellationToken: cancellationToken);
        return users.Select(Map).ToList();
    }

    public async Task<UserSummaryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Repository<User>().GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), id);
        return Map(user);
    }

    public async Task<UserSummaryResponse> CreateAsync(AdminCreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = currentUserService.TenantId ?? throw new AuthenticationException("No authenticated tenant.");
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var users = unitOfWork.Repository<User>();
        var existing = await users.ListAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (existing.Count > 0)
        {
            throw new ConflictException($"An account with email '{normalizedEmail}' already exists.");
        }

        if (!Enum.TryParse<Role>(request.Role, ignoreCase: true, out var role))
        {
            throw new ConflictException($"'{request.Role}' is not a valid role.");
        }

        var user = new User
        {
            TenantId = tenantId,
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            Role = role
        };

        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(user);
    }

    public async Task<UserSummaryResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var users = unitOfWork.Repository<User>();
        var user = await users.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException(nameof(User), id);

        if (!Enum.TryParse<Role>(request.Role, ignoreCase: true, out var role))
        {
            throw new ConflictException($"'{request.Role}' is not a valid role.");
        }

        user.FullName = request.FullName.Trim();
        user.Role = role;
        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(user);
    }

    private static UserSummaryResponse Map(User user) =>
        new(user.Id, user.Email, user.FullName, user.Role.ToString(), user.IsActive, user.CreatedAtUtc);
}
