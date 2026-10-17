// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Notifications.Options;

/// <summary>
/// Email transport configuration, bound from the <c>Email</c> section.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>
    /// Master switch. When false nothing is dispatched at all, which is what the integration
    /// test host uses to keep the suite offline.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Transport: <c>File</c> (default, writes the message to disk and logs it) or <c>Smtp</c>.
    /// </summary>
    public string Provider { get; set; } = "File";

    /// <summary>Directory the file transport writes to. Defaults to an <c>emails</c> folder next to the app.</summary>
    public string? OutboxPath { get; set; }

    public string FromAddress { get; set; } = "no-reply@oroidentity.local";
    public string FromName { get; set; } = "OroIdentity";

    // Reserved for the SMTP transport; ignored while Provider is "File".
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public bool SmtpUseSsl { get; set; } = true;
}