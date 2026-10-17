// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.UnitTests;

using System.Globalization;
using IdentityServer.Shared.Resources;
using IdentityServer.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.UserSessions.Repositories;
using OroIdentityServer.Shared.Options;

/// <summary>
/// The expiry notification is composed by a Quartz job that has no request, so the language can
/// only come from the session. This exercises that path against the real .resx files.
/// </summary>
public class SessionExpiryEmailComposerTests
{
    private static SessionExpiryEmailComposer CreateComposer()
    {
        var localization = Options.Create(new Microsoft.Extensions.Localization.LocalizationOptions());
        var factory = new Microsoft.Extensions.Localization.ResourceManagerStringLocalizerFactory(
            localization,
            NullLoggerFactory.Instance);

        return new SessionExpiryEmailComposer(
            factory,
            Options.Create(new BrandingOptions { FullName = "OroIdentityServer" }));
    }

    [Fact]
    public void Compose_ReturnsNullWhenTheUserHasNoAddress()
    {
        var composer = CreateComposer();

        var message = composer.Compose(Session(email: null));

        Assert.Null(message);
    }

    [Fact]
    public void Compose_WritesTheSubjectAndBodyInEnglish()
    {
        var composer = CreateComposer();

        var message = composer.Compose(Session(culture: "en"));

        Assert.NotNull(message);
        Assert.Equal("user@example.com", message!.To);
        Assert.Equal("Your session expired", message.Subject);
        Assert.Equal(EmailBodyFormat.Html, message.BodyFormat);
        Assert.Equal("SessionExpired", message.TemplateName);
        Assert.Contains("OroIdentityServer", message.Body);
        Assert.Contains("Chrome on Linux", message.Body);
        Assert.Contains("jdoe", message.Body);
    }

    [Fact]
    public void Compose_UsesTheCultureRecordedOnTheSession()
    {
        var composer = CreateComposer();

        var spanish = composer.Compose(Session(culture: "es-419"));
        var german = composer.Compose(Session(culture: "de"));

        Assert.Equal("Tu sesión caducó", spanish!.Subject);
        Assert.Equal("Deine Sitzung ist abgelaufen", german!.Subject);
        Assert.Contains("OroIdentityServer", spanish.Body);
    }

    [Fact]
    public void Compose_FallsBackToTheNeutralCultureForAnUnknownOne()
    {
        var composer = CreateComposer();

        var message = composer.Compose(Session(culture: "kl-KL"));

        Assert.NotNull(message);
        // No satellite assembly for that culture: the neutral resources answer.
        Assert.Equal("Your session expired", message!.Subject);
    }

    [Fact]
    public void Compose_LeavesTheAmbientCultureUnchanged()
    {
        var composer = CreateComposer();
        var original = CultureInfo.CurrentUICulture;

        composer.Compose(Session(culture: "es-419"));

        // The composer has to restore the thread culture: it runs on a pooled scheduler thread.
        Assert.Equal(original, CultureInfo.CurrentUICulture);
    }

    [Fact]
    public void Compose_SubstitutesAnUnknownDevice()
    {
        var composer = CreateComposer();

        var message = composer.Compose(Session(culture: "en", device: null));

        Assert.Contains("an unknown device", message!.Body);
    }

    private static ExpiredSessionInfo Session(string? email = "user@example.com", string? culture = "en", string? device = "Chrome on Linux")
        => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "jdoe",
            email,
            device,
            "Santiago",
            culture,
            new DateTime(2026, 3, 14, 15, 9, 26, DateTimeKind.Utc));
}