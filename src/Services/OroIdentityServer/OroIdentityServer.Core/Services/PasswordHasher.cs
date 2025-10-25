// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SALTSIZE = 16;
    private const int HASHSIZE = 32;
    private const int ITERATIONS = 10_000;

    public async ValueTask<string> HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new Byte[SALTSIZE];
        rng.GetBytes(salt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, ITERATIONS, HashAlgorithmName.SHA256, HASHSIZE);

        var hashBytes = new byte[SALTSIZE + HASHSIZE];
        Array.Copy(salt, 0, hashBytes, 0, SALTSIZE);
        Array.Copy(hash, 0, hashBytes, SALTSIZE, HASHSIZE);

        return await Task.FromResult(Convert.ToBase64String(hashBytes));
    }

    public async ValueTask<bool> VerifyPassword(string password, string hashedPassword)
    {
        var hashBytes = Convert.FromBase64String(hashedPassword);

        var salt = new byte[SALTSIZE];
        Array.Copy(hashBytes, 0, salt, 0, SALTSIZE);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, ITERATIONS, HashAlgorithmName.SHA256, HASHSIZE);

        for (int i = 0; i < HASHSIZE; i++)
        {
            if (hashBytes[SALTSIZE + i] != hash[i])
                return await Task.FromResult(false);
        }

        return await Task.FromResult(true);
    }
}