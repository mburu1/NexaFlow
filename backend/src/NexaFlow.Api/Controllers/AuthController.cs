using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NexaFlow.Application.DTOs.Auth;
using NexaFlow.Application.Interfaces;

namespace NexaFlow.Api.Controllers;

[ApiController]
[Route("auth")]
[EnableRateLimiting("auth")]
public class AuthController(
    IAuthService authService,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator,
    IValidator<RefreshRequest> refreshValidator) : ControllerBase
{
    /// <summary>Creates a new tenant + Admin user, returns a token pair.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        await registerValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await authService.RegisterAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Exchanges credentials for a JWT access + refresh token pair.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Rotates a refresh token for a new access + refresh token pair.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        await refreshValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await authService.RefreshAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Verifies the bearer token and returns the caller's profile.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        var result = await authService.GetCurrentUserAsync(cancellationToken);
        return Ok(result);
    }
}
