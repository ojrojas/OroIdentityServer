// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OpenIddict.Abstractions;

namespace OroIdentityServer.Application.Modules.UserSessions.Commands;

public class DeactivateUserSessionCommandHandler(
    ILogger<DeactivateUserSessionCommandHandler> logger,
    IUserSessionRepository userSessionRepository,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictTokenManager tokenManager
) : ICommandHandler<DeactivateUserSessionCommand>
{
    public async Task<Result> HandleAsync(DeactivateUserSessionCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling DeactivateUserSessionCommand for SessionId: {SessionId}", command.SessionId);
        try
        {
            var session = await userSessionRepository.GetUserSessionByIdAsync(new(command.SessionId), cancellationToken);
            if (session == null)
            {
                logger.LogWarning("Session not found: {SessionId}", command.SessionId);
                return Result.Success();
            }

            // Revoke OpenIddict tokens for this user before deactivating the session
            var subject = session.UserId?.Value.ToString();
            if (!string.IsNullOrEmpty(subject))
            {
                await foreach (var authorization in authorizationManager.FindBySubjectAsync(subject, cancellationToken))
                {
                    try { await authorizationManager.TryRevokeAsync(authorization, cancellationToken); }
                    catch (Exception ex) { logger.LogWarning(ex, "Failed to revoke authorization for subject {Subject}", subject); }
                }

                await foreach (var token in tokenManager.FindBySubjectAsync(subject, cancellationToken))
                {
                    try { await tokenManager.TryRevokeAsync(token, cancellationToken); }
                    catch (Exception ex) { logger.LogWarning(ex, "Failed to revoke token for subject {Subject}", subject); }
                }
            }

            session.DeactivateSession();
            await userSessionRepository.UpdateUserSessionAsync(session, cancellationToken);
            logger.LogInformation("Deactivated session {SessionId}", command.SessionId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating session {SessionId}", command.SessionId);
            throw;
        }
    }
}
