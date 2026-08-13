using NexaFlow.Domain.Entities;

namespace NexaFlow.Application.Common.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user);

    (string RawToken, string TokenHash, DateTime ExpiresAtUtc) GenerateRefreshToken();

    string HashRefreshToken(string rawToken);
}
