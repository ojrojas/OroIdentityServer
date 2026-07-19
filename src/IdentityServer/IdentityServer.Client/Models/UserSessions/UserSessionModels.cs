namespace IdentityServer.Client.Models.UserSessions;

public sealed record UserSessionModel(
    Guid? UserId,
    string? Device,
    string? SessionToken,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? LastActivityAt,
    string? IpAddress,
    string? UserAgent,
    string? Location);

public sealed record CreateUserSessionRequest(
    Guid UserId,
    string Device,
    string SessionToken,
    DateTime ExpiresAt,
    string? IpAddress,
    string? UserAgent,
    string? Location);
