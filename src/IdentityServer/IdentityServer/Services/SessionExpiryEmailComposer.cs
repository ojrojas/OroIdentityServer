// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace IdentityServer.Services;

using System.Globalization;
using IdentityServer.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.UserSessions.Repositories;
using OroIdentityServer.Shared.Options;

/// <summary>
/// Builds the "your session expired" email. It lives in the host rather than in Application
/// because the Quartz job that uses it has no request, so the culture has to come from the
/// session itself instead of from <c>RequestLocalization</c>.
/// </summary>
public sealed class SessionExpiryEmailComposer(
    IStringLocalizerFactory localizerFactory,
    IOptions<BrandingOptions> branding) : ISessionExpiryEmailComposer
{
    public EmailMessage? Compose(ExpiredSessionInfo session)
    {
        if (string.IsNullOrWhiteSpace(session.Email)) return null;

        var appName = branding.Value.FullName;
        var culture = ResolveCulture(session.Culture);

        // Resource lookups follow CultureInfo.CurrentUICulture (there is no per-call culture
        // overload on IStringLocalizer), so the session's language is applied for the duration of
        // the lookup and restored afterwards.
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            var strings = localizerFactory.Create(typeof(SharedResources));
            var when = session.ExpiresAt.ToString("g", culture);
            var device = string.IsNullOrWhiteSpace(session.Device) ? strings["UnknownDevice"] : session.Device;

            var subject = strings["EmailSessionExpiredSubject"];
            var body = $"""
                <p>{strings["EmailSessionExpiredGreeting", session.UserName ?? session.Email]}</p>
                <p>{strings["EmailSessionExpiredBody", appName, when, device]}</p>
                <p>{strings["EmailSessionExpiredAction"]}</p>
                <p style="color:#6b7280;font-size:12px">{strings["EmailSessionExpiredFooter", appName]}</p>
                """;

            return new EmailMessage(session.Email, subject, body, EmailBodyFormat.Html, TemplateName: "SessionExpired")
            {
                CreatedAtUtc = DateTime.UtcNow
            };
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    private static CultureInfo ResolveCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture)) return CultureInfo.InvariantCulture;

        try
        {
            return CultureInfo.GetCultureInfo(culture);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }
}