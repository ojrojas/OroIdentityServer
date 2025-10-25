// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Interfaces;

public interface IPasswordHasher
{
    ValueTask<string> HashPassword(string password);
    ValueTask<bool> VerifyPassword(string password, string hashedPassword);
}