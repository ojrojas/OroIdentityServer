// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
namespace OroIdentityServer.Shared.Options;

public sealed class BrandingOptions
{
    public const string SectionName = "Branding";

    public string AppName { get; set; } = "OroIdentity";
    public string FullName { get; set; } = "OroIdentityServer";
    public string DisplayName { get; set; } = "OroIdentity Admin SPA";
    public string? LogoUrl { get; set; }
    public string? FullLogoUrl { get; set; }

    public string EffectiveLogoUrl => string.IsNullOrWhiteSpace(LogoUrl) ? "/img/logo.png" : LogoUrl;
    public string EffectiveFullLogoUrl => string.IsNullOrWhiteSpace(FullLogoUrl) ? "/img/full-logo.png" : FullLogoUrl;
}
