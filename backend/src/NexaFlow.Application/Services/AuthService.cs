using NexaFlow.Application.Common;
using NexaFlow.Application.Common.Exceptions;
using NexaFlow.Application.Common.Interfaces;
using NexaFlow.Application.DTOs.Auth;
using NexaFlow.Application.Interfaces;
using NexaFlow.Domain.Entities;
using NexaFlow.Domain.Enums;
using NexaFlow.Domain.Interfaces;

namespace NexaFlow.Application.Services;

public sealed class AuthService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ICurrentUserService currentUserService) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var users = unitOfWork.Repository<User>();

        var existing = await users.ListAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (existing.Count > 0)
        {
            throw new ConflictException($"An account with email '{normalizedEmail}' already exists.");
        }

        var tenant = new Tenant
        {
            Name = request.OrganizationName.Trim(),
            Slug = SlugGenerator.Generate(request.OrganizationName)
        };
        await unitOfWork.Repository<Tenant>().AddAsync(tenant, cancellationToken);

        var user = new User
        {
            TenantId = tenant.Id,
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = request.FullName.Trim(),
            Role = Role.Admin
        };
        await users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, tenant, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var matches = await unitOfWork.Repository<User>().ListAsync(u => u.Email == normalizedEmail, cancellationToken);
        var user = matches.FirstOrDefault();

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        var tenant = await unitOfWork.Repository<Tenant>().GetByIdAsync(user.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), user.TenantId);

        return await IssueTokensAsync(user, tenant, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);
        var tokens = unitOfWork.Repository<RefreshToken>();
        var matches = await tokens.ListAsync(t => t.TokenHash == tokenHash, cancellationToken);
        var existingToken = matches.FirstOrDefault();

        if (existingToken is null || !existingToken.IsActive)
        {
            throw new AuthenticationException("Refresh token is invalid or has expired.");
        }

        var user = await unitOfWork.Repository<User>().GetByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), existingToken.UserId);
        var tenant = await unitOfWork.Repository<Tenant>().GetByIdAsync(user.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), user.TenantId);

        var (rawRefreshToken, refreshTokenHash, refreshExpiresAtUtc) = tokenService.GenerateRefreshToken();

        existingToken.RevokedAtUtc = DateTime.UtcNow;
        existingToken.ReplacedByTokenHash = refreshTokenHash;
        tokens.Update(existingToken);

        await tokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAtUtc = refreshExpiresAtUtc
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var (accessToken, accessExpiresAtUtc) = tokenService.GenerateAccessToken(user);
        return new AuthResponse(accessToken, accessExpiresAtUtc, rawRefreshToken, refreshExpiresAtUtc, MapUser(user, tenant));
    }

    public async Task<UserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (currentUserService.UserId is not { } userId)
        {
            throw new AuthenticationException("No authenticated user.");
        }

        var user = await unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);
        var tenant = await unitOfWork.Repository<Tenant>().GetByIdAsync(user.TenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), user.TenantId);

        return MapUser(user, tenant);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, Tenant tenant, CancellationToken cancellationToken)
    {
        var (accessToken, accessExpiresAtUtc) = tokenService.GenerateAccessToken(user);
        var (rawRefreshToken, refreshTokenHash, refreshExpiresAtUtc) = tokenService.GenerateRefreshToken();

        await unitOfWork.Repository<RefreshToken>().AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAtUtc = refreshExpiresAtUtc
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, accessExpiresAtUtc, rawRefreshToken, refreshExpiresAtUtc, MapUser(user, tenant));
    }

    private static UserResponse MapUser(User user, Tenant tenant) =>
        new(user.Id, user.Email, user.FullName, user.Role.ToString(), tenant.Id, tenant.Name);
}
