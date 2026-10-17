// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.UserSessions.Commands;

public sealed class ProcessExpiredSessionNotificationsCommandHandler(
    ILogger<ProcessExpiredSessionNotificationsCommandHandler> logger,
    IUserSessionRepository userSessionRepository,
    IEmailSender emailSender,
    ISessionExpiryEmailComposer emailComposer) : ICommandHandler<ProcessExpiredSessionNotificationsCommand, ExpiredSessionNotificationSummary>
{
    public async Task<ExpiredSessionNotificationSummary> HandleAsync(
        ProcessExpiredSessionNotificationsCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling ProcessExpiredSessionNotificationsCommand");

        try
        {
            var now = DateTime.UtcNow;
            var expired = await userSessionRepository.GetExpiredPendingNotificationAsync(
                now, Math.Max(1, command.MaxBatchSize), cancellationToken);

            var notified = 0;
            var skipped = 0;

            foreach (var info in expired)
            {
                // Close it first and stamp the notification in the same save: a failure below
                // must not leave the row eligible for the next sweep.
                var session = await userSessionRepository.GetUserSessionByIdAsync(new(info.SessionId), cancellationToken);
                if (session is null || session.EndReason != SessionEndReason.None)
                {
                    skipped++;
                    continue;
                }

                session.MarkExpired(now);

                var message = emailComposer.Compose(info);
                if (message is not null)
                {
                    try
                    {
                        await emailSender.SendAsync(message, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // A transport failure must not wedge the sweep: the row is stamped as
                        // notified so the next run moves on instead of retrying forever.
                        logger.LogError(ex, "Failed to send the session-expiry email for session {SessionId}", info.SessionId);
                    }

                    notified++;
                }
                else
                {
                    skipped++;
                    logger.LogInformation(
                        "Skipped the session-expiry email for session {SessionId}: the user has no address on file",
                        info.SessionId);
                }

                session.MarkExpiryNotified(now);
                await userSessionRepository.UpdateUserSessionAsync(session, cancellationToken);
            }

            logger.LogInformation(
                "Processed {Processed} expired sessions: {Notified} notified, {Skipped} skipped",
                expired.Count, notified, skipped);

            return new ExpiredSessionNotificationSummary(expired.Count, notified, skipped);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing expired session notifications");
            throw;
        }
    }
}