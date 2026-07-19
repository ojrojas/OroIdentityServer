namespace IdentityServer.Client.Models.Sessions;

/// <summary>
/// The nested "User" navigation on the server's SessionDto is intentionally omitted here
/// to keep this a lightweight connector model.
/// </summary>
public sealed record SessionModel(
    Guid SessionId,
    Guid UserId,
    Guid TenantId,
    string? AuthorizationId,
    string IpAddress,
    string Country,
    DateTime StartedAtUtc,
    DateTime? EndedAtUtc);
