// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Interfaces;

/// <summary>A recovery code in both the form shown to the user once and the form stored.</summary>
public sealed record GeneratedRecoveryCode(string PlainCode, string CodeHash);

/// <summary>
/// Produces the single-use codes a user falls back to when the authenticator is unavailable.
/// </summary>
public interface IRecoveryCodeGenerator
{
    /// <summary>Generates codes and their hashes in one pass.</summary>
    IReadOnlyList<GeneratedRecoveryCode> Generate(int count = 10);

    /// <summary>Normalises user input (trim, upper-case, drop separators) before hashing.</summary>
    string Normalize(string code);
}