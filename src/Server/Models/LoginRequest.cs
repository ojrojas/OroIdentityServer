// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Server.Models;

public class LoginRequest(string userName, string password, bool rememberMe)
{
    public string UserName { get; set; } = userName;
    public string Password { get; set; } = password;
    public bool RememberMe { get; set; } = rememberMe;
}