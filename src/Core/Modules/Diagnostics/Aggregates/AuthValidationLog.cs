// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Diagnostics.Aggregates;

public sealed class AuthValidationLog : AggregateRoot<AuthValidationLogId>
{
    public AuthValidationEventType EventType { get; private set; }
    public bool Succeeded { get; private set; }
    public Guid? UserId { get; private set; }
    public string? ClientId { get; private set; }
    public string? Scopes { get; private set; }
    public string? IpAddress { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private AuthValidationLog() { }

    private AuthValidationLog(
        AuthValidationLogId id,
        AuthValidationEventType eventType,
        bool succeeded,
        Guid? userId,
        string? clientId,
        string? scopes,
        string? ipAddress,
        string? failureReason,
        DateTime occurredAtUtc)
    {
        Id = id;
        EventType = eventType;
        Succeeded = succeeded;
        UserId = userId;
        ClientId = clientId;
        Scopes = scopes;
        IpAddress = ipAddress;
        FailureReason = failureReason;
        OccurredAtUtc = occurredAtUtc;
    }

    public static AuthValidationLog Create(
        AuthValidationEventType eventType,
        bool succeeded,
        Guid? userId,
        string? clientId,
        string? scopes,
        string? ipAddress,
        string? failureReason)
    {
        return new AuthValidationLog(
            AuthValidationLogId.New(null),
            eventType,
            succeeded,
            userId,
            clientId,
            scopes,
            ipAddress,
            failureReason,
            DateTime.UtcNow);
    }
}
