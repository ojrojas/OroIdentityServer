namespace IdentityServer.Client.Models.Diagnostics;

public sealed record DailyValidationCountModel(DateOnly Date, int SucceededCount, int FailedCount);

public sealed record ValidationLogEntryModel(
    DateTime OccurredAtUtc,
    string EventType,
    bool Succeeded,
    Guid? UserId,
    string? ClientId,
    string? Scopes,
    string? FailureReason);
