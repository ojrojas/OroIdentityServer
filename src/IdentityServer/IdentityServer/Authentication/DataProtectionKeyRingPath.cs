// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Server.Authentication;

using Microsoft.Extensions.Options;

/// <summary>
/// Where the ASP.NET Core Data Protection key ring lives.
/// </summary>
public sealed class OroDataProtectionOptions
{
    public const string SectionName = "DataProtection";

    /// <summary>
    /// Absolute path of the key ring directory. Leave empty to let
    /// <see cref="DataProtectionKeyRingPath.Resolve"/> pick one.
    /// </summary>
    public string KeysPath { get; set; } = string.Empty;

    /// <summary>How long a generated key stays valid before a new one is added.</summary>
    public int KeyLifetimeDays { get; set; } = 90;
}

/// <summary>
/// Resolves the directory the Data Protection key ring is persisted to.
/// </summary>
/// <remarks>
/// The obvious choice, <c>AppContext.BaseDirectory</c>, is wrong for every non-container run:
/// under <c>dotnet run</c> it points at <c>bin/Debug/net10.0</c>, which <c>dotnet clean</c>, an
/// incremental rebuild or a fresh clone wipes. A regenerated ring means every previously
/// encrypted value becomes permanently unreadable - the admin cookie logs the user out and every
/// enrolled TOTP secret fails to decrypt with
/// <c>CryptographicException: The key {guid} was not found in the key ring</c>, which surfaces as
/// a 500 on the MFA page. So the default is a directory that lives outside the build output.
/// </remarks>
public static class DataProtectionKeyRingPath
{
    public const string DefaultFolderName = "data-protection-keys";

    public const string KeysPathEnvironmentVariable = "DATA_PROTECTION_KEYS_PATH";

    /// <summary>
    /// Explicit configuration wins; otherwise the app's own folder, one level above the build
    /// output when we can find it. Containers leave <c>AppContext.BaseDirectory</c> alone because
    /// the Dockerfile mounts a volume there.
    /// </summary>
    public static string Resolve(IConfiguration configuration, string baseDirectory)
    {
        // Both spellings are accepted: "DataProtection:KeysPath" is the section form, and the flat
        // DATA_PROTECTION_KEYS_PATH env var matches the convention the AppHost/Dockerfile already
        // use for the other bootstrap settings (IDENTITY_ADMIN_HTTP, SEED_APP_CLIENT_ID, ...).
        var configured = configuration[$"{OroDataProtectionOptions.SectionName}:KeysPath"]
            ?? configuration[KeysPathEnvironmentVariable];

        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.GetFullPath(configured);
        }

        var projectDirectory = TryFindProjectDirectory(baseDirectory);
        return Path.Combine(projectDirectory ?? baseDirectory, DefaultFolderName);
    }

    /// <summary>
    /// Walks out of a <c>bin/&lt;config&gt;/&lt;tfm&gt;</c> (or <c>obj/</c>) path back to the
    /// project directory. Returns null when the base directory is not a build output (container,
    /// published app, tests), in which case the caller keeps using it.
    /// </summary>
    private static string? TryFindProjectDirectory(string baseDirectory)
    {
        var current = new DirectoryInfo(baseDirectory);

        while (current is not null)
        {
            if (IsBuildOutputSegment(current.Name))
            {
                var parent = current.Parent;
                if (parent is not null && !IsBuildOutputSegment(parent.Name))
                {
                    return parent.FullName;
                }
            }

            current = current.Parent;
        }

        return null;
    }

    private static bool IsBuildOutputSegment(string name) =>
        name.Equals("bin", StringComparison.OrdinalIgnoreCase)
        || name.Equals("obj", StringComparison.OrdinalIgnoreCase);
}