// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Diagnostics.Commands;

public class LogAuthValidationCommandHandler(
    ILogger<LogAuthValidationCommandHandler> logger,
    IAuthValidationLogRepository authValidationLogRepository)
: ICommandHandler<LogAuthValidationCommand>
{
    public async Task<Result> HandleAsync(LogAuthValidationCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling LogAuthValidationCommand for EventType: {EventType}", command.EventType);

        try
        {
            var log = AuthValidationLog.Create(
                command.EventType,
                command.Succeeded,
                command.UserId,
                command.ClientId,
                command.Scopes,
                command.IpAddress,
                command.FailureReason);

            await authValidationLogRepository.AddAsync(log, cancellationToken);
            logger.LogInformation("Successfully created auth validation log with Id: {Id}", log.Id);
            return Result.Success();
        }
        catch (Exception ex)
        {
            // Auditing must never break the auth flow: log the failure and swallow it.
            logger.LogError(ex, "An error occurred while handling LogAuthValidationCommand for EventType: {EventType}", command.EventType);
            return Result.Success();
        }
    }
}
