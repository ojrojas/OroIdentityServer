// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Interfaces;

public enum EmailBodyFormat
{
    Html = 0,
    Text = 1
}

/// <summary>
/// A message handed to whichever email transport is configured. Bodies are composed by the
/// caller (the job that owns the context of the message) so the transport stays dumb.
/// </summary>
public sealed record EmailMessage(
    string To,
    string Subject,
    string Body,
    EmailBodyFormat BodyFormat = EmailBodyFormat.Html,
    string? TemplateName = null)
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}