// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Interfaces;

using OroIdentityServer.Core.Modules.UserSessions.Repositories;

/// <summary>
/// Renders the "your session expired" message. Lives behind an interface so the Application
/// handler owns the workflow (which sessions, when, dedupe) while the host owns the wording and
/// the localisation — a Quartz job has no request culture to resolve.
/// </summary>
public interface ISessionExpiryEmailComposer
{
    /// <summary>
    /// Returns null when the session's user has no address to notify, so the caller can skip
    /// without treating it as a failure.
    /// </summary>
    EmailMessage? Compose(ExpiredSessionInfo session);
}