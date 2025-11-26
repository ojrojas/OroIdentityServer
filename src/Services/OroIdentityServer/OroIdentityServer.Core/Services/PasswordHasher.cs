// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Services;

using BCrypt.Net;

public class PasswordHasher : IPasswordHasher
{
    public async ValueTask<string> HashPassword(string password)
    {
        // Use BCrypt to hash the password with a work factor of 12 (specialized configuration)
        string hashedPassword = BCrypt.HashPassword(password, workFactor: 12);
        return await Task.FromResult(hashedPassword);
    }

    public async ValueTask<bool> VerifyPassword(string password, string hashedPassword)
    {
        // Use BCrypt to verify the password against the hashed password
        bool isValid = BCrypt.Verify(password, hashedPassword);
        return await Task.FromResult(isValid);
    }
}