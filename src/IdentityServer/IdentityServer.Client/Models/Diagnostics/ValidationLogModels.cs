namespace IdentityServer.Client.Models.Diagnostics;

public sealed record DailyValidationCountModel(DateOnly Date, int SucceededCount, int FailedCount);
