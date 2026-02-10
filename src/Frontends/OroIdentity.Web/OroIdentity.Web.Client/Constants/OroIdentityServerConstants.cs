using System.Reflection;
using OpenIddict.Abstractions;

namespace OroIdentity.Web.Client.Constants;

public class OroIdentityWebConstants
{
    public const string OroIdentityServerApis = "OroIdentityServerApis";

    public static IReadOnlyDictionary<string, string> DictionaryApplicationTypes { get; }
    = typeof(OpenIddictConstants.ApplicationTypes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(string))
        .ToDictionary(
            f => f.Name,
            f => (string)f.GetValue(null)!);

    public static IReadOnlyDictionary<string, string> DictionaryConsentTypes { get; }
    = typeof(OpenIddictConstants.ConsentTypes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(string))
        .ToDictionary(
            f => f.Name,
            f => (string)f.GetValue(null)!);

    public static IReadOnlyDictionary<string, string> DictionaryClientTypes { get; }
    = typeof(OpenIddictConstants.ClientTypes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(string))
        .ToDictionary(
            f => f.Name,
            f => (string)f.GetValue(null)!);

    public static IReadOnlyDictionary<string, string> DictionaryEndpoints { get; }
    = typeof(OpenIddictConstants.Permissions.Endpoints)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(string))
        .ToDictionary(
            f => f.Name,
            f => (string)f.GetValue(null)!);

    public static IReadOnlyDictionary<string, string> DictionaryGrantTypes { get; }
    = typeof(OpenIddictConstants.Permissions.GrantTypes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(string))
        .ToDictionary(
            f => f.Name,
            f => (string)f.GetValue(null)!);

    public static IReadOnlyDictionary<string, string> DictionaryResponseTypes { get; }
    = typeof(OpenIddictConstants.Permissions.ResponseTypes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(string))
        .ToDictionary(
            f => f.Name,
            f => (string)f.GetValue(null)!);

    public static IReadOnlyDictionary<string, string> DictionaryRequirementsFeatures { get; }
    = typeof(OpenIddictConstants.Requirements.Features)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(string))
        .ToDictionary(
            f => f.Name,
            f => (string)f.GetValue(null)!);

    public static IReadOnlyDictionary<string, string> DictionaryRequirementsPrefixes { get; }
    = typeof(OpenIddictConstants.Requirements.Prefixes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(string))
        .ToDictionary(
            f => f.Name,
            f => (string)f.GetValue(null)!);

    // Common scopes - provide a small curated list for UI selection
    public static IReadOnlyDictionary<string, string> DictionaryScopes { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "openid", "openid" },
            { "profile", "profile" },
            { "email", "email" },
            { "offline_access", "offline_access" }
        };

    // Example organizations dictionary - replace with real data source if available
    public static IReadOnlyDictionary<string, string> Organizations { get; } =
        new Dictionary<string, string>
        {
            { "default", "Default Organization" }
        };
}   