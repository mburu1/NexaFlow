using FluentAssertions;
using Moq;
using NexaFlow.Application.Common.Exceptions;
using NexaFlow.Application.Common.Interfaces;
using NexaFlow.Application.DTOs.Auth;
using NexaFlow.Application.Services;
using NexaFlow.Domain.Entities;
using NexaFlow.Infrastructure.Persistence.Repositories;
using NexaFlow.Tests.TestUtilities;

namespace NexaFlow.Tests.Application;

public class AuthServiceTests
{
    private readonly TestCurrentUserService _currentUser = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private CancellationToken Ct => TestContext.Current.CancellationToken;

    private AuthService CreateSut()
    {
        var dbContext = TestDbContextFactory.Create(_currentUser);
        var unitOfWork = new UnitOfWork(dbContext);
        return new AuthService(unitOfWork, _passwordHasher.Object, _tokenService.Object, _currentUser);
    }

    private void SetupTokenService()
    {
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns(("access-token", DateTime.UtcNow.AddMinutes(15)));
        _tokenService.Setup(t => t.GenerateRefreshToken())
            .Returns(("raw-refresh", "hashed-refresh", DateTime.UtcNow.AddDays(7)));
        _tokenService.Setup(t => t.HashRefreshToken(It.IsAny<string>()))
            .Returns<string>(raw => $"hash-of-{raw}");
    }

    [Fact]
    public async Task RegisterAsync_CreatesTenantAndAdminUser_ReturnsTokens()
    {
        SetupTokenService();
        _passwordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
        var sut = CreateSut();

        var request = new RegisterRequest("Acme Inc", "admin@acme.test", "Password123!", "Ada Admin");
        var result = await sut.RegisterAsync(request, Ct);

        result.AccessToken.Should().Be("access-token");
        result.User.Email.Should().Be("admin@acme.test");
        result.User.Role.Should().Be("Admin");
        result.User.TenantName.Should().Be("Acme Inc");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsConflict()
    {
        SetupTokenService();
        _passwordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
        var sut = CreateSut();
        var request = new RegisterRequest("Acme Inc", "admin@acme.test", "Password123!", "Ada Admin");
        await sut.RegisterAsync(request, Ct);

        var act = () => sut.RegisterAsync(request with { FullName = "Someone Else" }, Ct);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsAuthenticationException()
    {
        SetupTokenService();
        _passwordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
        _passwordHasher.Setup(p => p.Verify("wrong-password", "hashed-password")).Returns(false);
        var sut = CreateSut();
        await sut.RegisterAsync(new RegisterRequest("Acme Inc", "admin@acme.test", "Password123!", "Ada Admin"), Ct);

        var act = () => sut.LoginAsync(new LoginRequest("admin@acme.test", "wrong-password"), Ct);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsAuthenticationException()
    {
        SetupTokenService();
        var sut = CreateSut();

        var act = () => sut.LoginAsync(new LoginRequest("nobody@acme.test", "whatever"), Ct);

        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task LoginAsync_CorrectPassword_ReturnsTokens()
    {
        SetupTokenService();
        _passwordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
        _passwordHasher.Setup(p => p.Verify("Password123!", "hashed-password")).Returns(true);
        var sut = CreateSut();
        await sut.RegisterAsync(new RegisterRequest("Acme Inc", "admin@acme.test", "Password123!", "Ada Admin"), Ct);

        var result = await sut.LoginAsync(new LoginRequest("admin@acme.test", "Password123!"), Ct);

        result.User.Email.Should().Be("admin@acme.test");
    }

    [Fact]
    public async Task RefreshAsync_RotatesToken_OldTokenCanNoLongerBeUsed()
    {
        var callCount = 0;
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns(("access-token", DateTime.UtcNow.AddMinutes(15)));
        _tokenService.Setup(t => t.GenerateRefreshToken())
            .Returns(() =>
            {
                callCount++;
                return ($"raw-{callCount}", $"hash-{callCount}", DateTime.UtcNow.AddDays(7));
            });
        _tokenService.Setup(t => t.HashRefreshToken(It.IsAny<string>()))
            .Returns<string>(raw => raw.Replace("raw-", "hash-"));
        _passwordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");

        var sut = CreateSut();
        var registerResult = await sut.RegisterAsync(new RegisterRequest("Acme Inc", "admin@acme.test", "Password123!", "Ada Admin"), Ct);

        var refreshResult = await sut.RefreshAsync(new RefreshRequest(registerResult.RefreshToken), Ct);
        refreshResult.RefreshToken.Should().NotBe(registerResult.RefreshToken);

        var reuseAttempt = () => sut.RefreshAsync(new RefreshRequest(registerResult.RefreshToken), Ct);
        await reuseAttempt.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task RefreshAsync_UnknownToken_ThrowsAuthenticationException()
    {
        SetupTokenService();
        var sut = CreateSut();

        var act = () => sut.RefreshAsync(new RefreshRequest("does-not-exist"), Ct);

        await act.Should().ThrowAsync<AuthenticationException>();
    }
}
