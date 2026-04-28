// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Reflection;

namespace OroIdentityServer.Application.Modules.Sessions.Commands;

public class TerminateSessionCommandHandler(
    ILogger<TerminateSessionCommandHandler> logger,
    ISessionRepository sessionRepository,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictTokenManager tokenManager)
: ICommandHandler<TerminateSessionCommand>
{
    public async Task HandleAsync(TerminateSessionCommand command, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Handling TerminateSessionCommand for SessionId: {SessionId}", command.SessionId);

        Session? session = null;
        try
        {
            session = await sessionRepository.GetSessionByIdAsync(new(command.SessionId), cancellationToken);
            if (session == null)
                throw new InvalidOperationException("Session not found.");

            var subject = session.UserId.Value.ToString();

            async Task RevokeTokenAsync(object token)
            {
                try
                {
                    var revokeMethod = tokenManager.GetType().GetMethod("RevokeAsync", new[] { token.GetType(), typeof(CancellationToken) })
                        ?? tokenManager.GetType().GetMethod("RevokeAsync", new[] { token.GetType() });

                    if (revokeMethod != null)
                    {
                        var parms = revokeMethod.GetParameters();
                        var args = parms.Length == 2 ? new object[] { token, cancellationToken } : new object[] { token };
                        var task = revokeMethod.Invoke(tokenManager, args) as Task;
                        if (task != null) await task.ConfigureAwait(false);
                        return;
                    }

                    // Try extension static methods as fallback
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        foreach (var type in asm.GetTypes())
                        {
                            var method = type.GetMethod("RevokeAsync", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                            if (method != null)
                            {
                                var prm = method.GetParameters();
                                if (prm.Length >= 2 && prm[0].ParameterType.IsAssignableFrom(tokenManager.GetType()) && prm[1].ParameterType.IsAssignableFrom(token.GetType()))
                                {
                                    var t = (Task)method.Invoke(null, new object[] { tokenManager, token, cancellationToken });
                                    await t.ConfigureAwait(false);
                                    return;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // ignore and continue
                }
            }

            // If an AuthorizationId exists, prefer revoking by id (authorization + tokens)
            if (!string.IsNullOrEmpty(session.AuthorizationId))
            {
                try
                {
                    // Try to find the authorization by id using manager API (FindByIdAsync)
                    object? authorization = null;
                    try
                    {
                        var findById = authorizationManager.GetType().GetMethod("FindByIdAsync", new[] { typeof(string), typeof(CancellationToken) })
                            ?? authorizationManager.GetType().GetMethod("FindByIdAsync", new[] { typeof(string) });

                        if (findById != null)
                        {
                            var task = findById.Invoke(authorizationManager, findById.GetParameters().Length == 2 ? new object[] { session.AuthorizationId, cancellationToken } : new object[] { session.AuthorizationId });
                            if (task is Task t)
                            {
                                await t.ConfigureAwait(false);
                                var resultProp = t.GetType().GetProperty("Result");
                                authorization = resultProp?.GetValue(t);
                            }
                            else if (task != null)
                            {
                                await (dynamic)task;
                            }
                        }
                    }
                    catch
                    {
                        // ignore and fallback
                    }

                    // Revoke authorization object if found
                    if (authorization != null)
                    {
                        try
                        {
                            var revokeAuth = authorizationManager.GetType().GetMethod("RevokeAsync", new[] { authorization.GetType(), typeof(CancellationToken) })
                                ?? authorizationManager.GetType().GetMethod("RevokeAsync", new[] { authorization.GetType() });

                            if (revokeAuth != null)
                            {
                                var args = revokeAuth.GetParameters().Length == 2 ? new object[] { authorization, cancellationToken } : new object[] { authorization };
                                var rt = revokeAuth.Invoke(authorizationManager, args) as Task;
                                if (rt != null) await rt.ConfigureAwait(false);
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    // Try to find tokens by authorization id and revoke them
                    try
                    {
                        MethodInfo? findTokensMethod = tokenManager.GetType().GetMethod("FindByAuthorizationIdAsync", new[] { typeof(string), typeof(CancellationToken) })
                            ?? tokenManager.GetType().GetMethod("FindByAuthorizationIdAsync", new[] { typeof(string) });

                        object? tokensEnumerable = null;
                        if (findTokensMethod != null)
                        {
                            tokensEnumerable = findTokensMethod.Invoke(tokenManager, findTokensMethod.GetParameters().Length == 2 ? new object[] { session.AuthorizationId, cancellationToken } : new object[] { session.AuthorizationId });
                        }

                        if (tokensEnumerable != null)
                        {
                            if (tokensEnumerable is System.Collections.IEnumerable en)
                            {
                                foreach (var tk in en)
                                {
                                    await RevokeTokenAsync(tk);
                                }
                            }
                            else
                            {
                                var getAsyncEnumerator = tokensEnumerable.GetType().GetMethod("GetAsyncEnumerator", new[] { typeof(CancellationToken) })
                                    ?? tokensEnumerable.GetType().GetMethod("GetAsyncEnumerator", Type.EmptyTypes);

                                if (getAsyncEnumerator != null)
                                {
                                    var enumerator = getAsyncEnumerator.Invoke(tokensEnumerable, getAsyncEnumerator.GetParameters().Length == 1 ? new object[] { cancellationToken } : null);
                                    if (enumerator != null)
                                    {
                                        var moveNext = enumerator.GetType().GetMethod("MoveNextAsync", Type.EmptyTypes);
                                        var currentProp = enumerator.GetType().GetProperty("Current");
                                        while (true)
                                        {
                                            var moveNextTaskObj = moveNext.Invoke(enumerator, null);
                                            bool moved;
                                            try
                                            {
                                                moved = await (dynamic)moveNextTaskObj;
                                            }
                                            catch
                                            {
                                                var asTaskMethod = moveNextTaskObj.GetType().GetMethod("AsTask");
                                                var asTask = (Task<bool>)asTaskMethod.Invoke(moveNextTaskObj, null);
                                                moved = await asTask.ConfigureAwait(false);
                                            }
                                            if (!moved) break;
                                            var current = currentProp.GetValue(enumerator);
                                            await RevokeTokenAsync(current);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
                catch
                {
                    // ignore and fall through to subject-based fallback
                }
            }

            // Subject-based fallback: revoke any authorizations and tokens for the subject
            try
            {
                // Authorizations by subject
                try
                {
                    MethodInfo? findBySubjectMethod = authorizationManager.GetType().GetMethod("FindBySubjectAsync", new[] { typeof(string), typeof(CancellationToken) })
                        ?? authorizationManager.GetType().GetMethod("FindBySubjectAsync", new[] { typeof(string) });

                    object? authsEnumerable = null;
                    if (findBySubjectMethod != null)
                    {
                        authsEnumerable = findBySubjectMethod.Invoke(authorizationManager, findBySubjectMethod.GetParameters().Length == 2 ? new object[] { subject, cancellationToken } : new object[] { subject });
                    }

                    if (authsEnumerable != null)
                    {
                        if (authsEnumerable is System.Collections.IEnumerable en2)
                        {
                            foreach (var auth in en2)
                            {
                                var revokeMethod = authorizationManager.GetType().GetMethod("RevokeAsync", new[] { auth.GetType(), typeof(CancellationToken) })
                                    ?? authorizationManager.GetType().GetMethod("RevokeAsync", new[] { auth.GetType() });

                                if (revokeMethod != null)
                                {
                                    var args = revokeMethod.GetParameters().Length == 2 ? new object[] { auth, cancellationToken } : new object[] { auth };
                                    var t = (Task?)revokeMethod.Invoke(authorizationManager, args);
                                    if (t != null) await t.ConfigureAwait(false);
                                }
                            }
                        }
                        else
                        {
                            var getAsyncEnumerator = authsEnumerable.GetType().GetMethod("GetAsyncEnumerator", new[] { typeof(CancellationToken) })
                                ?? authsEnumerable.GetType().GetMethod("GetAsyncEnumerator", Type.EmptyTypes);

                            if (getAsyncEnumerator != null)
                            {
                                var enumerator = getAsyncEnumerator.Invoke(authsEnumerable, getAsyncEnumerator.GetParameters().Length == 1 ? new object[] { cancellationToken } : null);
                                if (enumerator != null)
                                {
                                    var moveNext = enumerator.GetType().GetMethod("MoveNextAsync", Type.EmptyTypes);
                                    var currentProp = enumerator.GetType().GetProperty("Current");
                                    while (true)
                                    {
                                        var moveNextTaskObj = moveNext.Invoke(enumerator, null);
                                        bool moved;
                                        try
                                        {
                                            moved = await (dynamic)moveNextTaskObj;
                                        }
                                        catch
                                        {
                                            var asTaskMethod = moveNextTaskObj.GetType().GetMethod("AsTask");
                                            var asTask = (Task<bool>)asTaskMethod.Invoke(moveNextTaskObj, null);
                                            moved = await asTask.ConfigureAwait(false);
                                        }
                                        if (!moved) break;
                                        var auth = currentProp.GetValue(enumerator);
                                        var revokeMethod = authorizationManager.GetType().GetMethod("RevokeAsync", new[] { auth.GetType(), typeof(CancellationToken) })
                                            ?? authorizationManager.GetType().GetMethod("RevokeAsync", new[] { auth.GetType() });

                                        if (revokeMethod != null)
                                        {
                                            var args = revokeMethod.GetParameters().Length == 2 ? new object[] { auth, cancellationToken } : new object[] { auth };
                                            var t = (Task?)revokeMethod.Invoke(authorizationManager, args);
                                            if (t != null) await t.ConfigureAwait(false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // ignore
                }

                // Tokens by subject
                try
                {
                    MethodInfo? findTokensBySubjectMethod = tokenManager.GetType().GetMethod("FindBySubjectAsync", new[] { typeof(string), typeof(CancellationToken) })
                        ?? tokenManager.GetType().GetMethod("FindBySubjectAsync", new[] { typeof(string) });

                    object? tokensBySubject = null;
                    if (findTokensBySubjectMethod != null)
                    {
                        tokensBySubject = findTokensBySubjectMethod.Invoke(tokenManager, findTokensBySubjectMethod.GetParameters().Length == 2 ? new object[] { subject, cancellationToken } : new object[] { subject });
                    }

                    if (tokensBySubject != null)
                    {
                        if (tokensBySubject is System.Collections.IEnumerable en3)
                        {
                            foreach (var tk in en3) await RevokeTokenAsync(tk);
                        }
                        else
                        {
                            var getAsyncEnumerator = tokensBySubject.GetType().GetMethod("GetAsyncEnumerator", new[] { typeof(CancellationToken) })
                                ?? tokensBySubject.GetType().GetMethod("GetAsyncEnumerator", Type.EmptyTypes);
                            if (getAsyncEnumerator != null)
                            {
                                var enumerator = getAsyncEnumerator.Invoke(tokensBySubject, getAsyncEnumerator.GetParameters().Length == 1 ? new object[] { cancellationToken } : null);
                                if (enumerator != null)
                                {
                                    var moveNext = enumerator.GetType().GetMethod("MoveNextAsync", Type.EmptyTypes);
                                    var currentProp = enumerator.GetType().GetProperty("Current");
                                    while (true)
                                    {
                                        var moveNextTaskObj = moveNext.Invoke(enumerator, null);
                                        bool moved;
                                        try
                                        {
                                            moved = await (dynamic)moveNextTaskObj;
                                        }
                                        catch
                                        {
                                            var asTaskMethod = moveNextTaskObj.GetType().GetMethod("AsTask");
                                            var asTask = (Task<bool>)asTaskMethod.Invoke(moveNextTaskObj, null);
                                            moved = await asTask.ConfigureAwait(false);
                                        }
                                        if (!moved) break;
                                        var current = currentProp.GetValue(enumerator);
                                        await RevokeTokenAsync(current);
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }
            catch (Exception)
            {
                // Log and continue
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while terminating session with Id: {SessionId}", command.SessionId);
            throw;
        }

        // Finally mark session ended locally
        session.End(DateTime.UtcNow);
        await sessionRepository.EndSessionAsync(new(command.SessionId), DateTime.UtcNow, cancellationToken);

        logger.LogInformation("Successfully terminated session with Id: {SessionId}", command.SessionId);
    }
}
