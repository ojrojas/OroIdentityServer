// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Interfaces;

/// <summary>
/// Defines methods for hashing and verifying passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the specified plain text password.
    /// </summary>
    /// <param name="password">The plain text password to hash.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the hashed password as a string.</returns>
    ValueTask<string> HashPassword(string password);

    /// <summary>
    /// Verifies whether the specified plain text password matches the hashed password.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <param name="hashedPassword">The hashed password to compare against.</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates whether the password matches the hashed password.</returns>
    ValueTask<bool> VerifyPassword(string password, string hashedPassword);
}