using NexaFlow.Application.DTOs.Auth;

namespace NexaFlow.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
