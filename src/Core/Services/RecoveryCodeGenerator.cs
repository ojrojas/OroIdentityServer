// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Services;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Ten-character recovery codes (alphabet without I/O/0/1/U) generated from a cryptographic RNG
/// and stored only as BCrypt hashes, so a database leak does not hand an attacker a way past
/// the second factor.
/// </summary>
public sealed class RecoveryCodeGenerator : IRecoveryCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 10;

    public IReadOnlyList<GeneratedRecoveryCode> Generate(int count = 10)
    {
        var codes = new List<GeneratedRecoveryCode>(count);
        for (var i = 0; i < count; i++)
        {
            var plain = Normalize(RandomText(CodeLength));
            codes.Add(new GeneratedRecoveryCode(plain, BCrypt.Net.BCrypt.HashPassword(plain, workFactor: 10)));
        }

        return codes;
    }

    /// <summary>
    /// Upper-cases and strips everything that is not a letter or digit, so "abcd-ef12 34" and
    /// "ABCDE F1234" compare equal to the generated code.
    /// </summary>
    public string Normalize(string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        var builder = new StringBuilder(CodeLength);
        foreach (var ch in code)
        {
            if (char.IsLetterOrDigit(ch))
                builder.Append(char.ToUpperInvariant(ch));
        }

        return builder.ToString();
    }

    private static string RandomText(int length)
    {
        var buffer = new char[length];
        for (var i = 0; i < length; i++)
            buffer[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];

        return new string(buffer);
    }
}