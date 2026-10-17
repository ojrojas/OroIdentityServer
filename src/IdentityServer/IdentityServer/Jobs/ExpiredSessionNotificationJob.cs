// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Server.Jobs;

using BuildingBlocks.CQRS.Abstractions;
using OroIdentityServer.Application.Modules.UserSessions.Commands;
using Quartz;

/// <summary>
/// Periodic sweep that closes sessions which ran out of time and queues the "your session
/// expired" email for each. Running it from Quartz rather than from the request that happens to
/// notice the expiry is what makes the notification reliable: the user's browser may be gone by
/// then, which is exactly when the mail is still useful.
/// </summary>
[DisallowConcurrentExecution]
public sealed class ExpiredSessionNotificationJob(
    ILogger<ExpiredSessionNotificationJob> logger,
    IServiceScopeFactory scopeFactory) : IJob
{
    public const string JobName = "oro.sessions.notify-expired";

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting the expired-session notification sweep");

        try
        {
            using var scope = scopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

            await dispatcher.SendAsync(
                new ProcessExpiredSessionNotificationsCommand(),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("The expired-session notification sweep was cancelled during shutdown");
        }
        catch (Exception ex)
        {
            // A failed sweep must not kill the scheduler: the rows stay eligible and the next
            // tick picks them up (the handler stamps each row as it processes it).
            logger.LogError(ex, "The expired-session notification sweep failed");
        }
    }
}