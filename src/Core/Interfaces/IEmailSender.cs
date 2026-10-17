// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Interfaces;

/// <summary>
/// Outbound email. The transport is chosen by configuration (see
/// <c>Core.Modules.Notifications.Options.EmailOptions</c>); callers must not assume a
/// particular one, and a send failure must never break the flow that triggered it.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}