using FluentAssertions;
using NexaFlow.Domain.Entities;

namespace NexaFlow.Tests.Domain;

public class RefreshTokenTests
{
    [Fact]
    public void IsActive_True_WhenNotExpiredAndNotRevoked()
    {
        var token = new RefreshToken { ExpiresAtUtc = DateTime.UtcNow.AddDays(1) };
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_False_WhenExpired()
    {
        var token = new RefreshToken { ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1) };
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_False_WhenRevoked()
    {
        var token = new RefreshToken { ExpiresAtUtc = DateTime.UtcNow.AddDays(1), RevokedAtUtc = DateTime.UtcNow };
        token.IsActive.Should().BeFalse();
    }
}
