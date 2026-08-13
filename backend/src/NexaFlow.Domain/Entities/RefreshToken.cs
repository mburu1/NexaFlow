using NexaFlow.Domain.Common;

namespace NexaFlow.Domain.Entities;

/// <summary>
/// Only a SHA-256 hash of the refresh token is ever persisted — the raw token is
/// returned to the client once and never stored, so a database leak alone can't be
/// used to impersonate a session. Rotation on use: /auth/refresh issues a new token
/// and marks this one revoked + linked via ReplacedByTokenHash for audit purposes.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
