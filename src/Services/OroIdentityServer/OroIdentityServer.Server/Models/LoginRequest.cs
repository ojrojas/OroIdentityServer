// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Server.Models;

public class LoginRequest
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public bool RememberMe { get; set; }

    public LoginRequest(string userName, string password, bool rememberMe)
    {
        UserName = userName;
        Password = password;
        RememberMe = rememberMe;
    }
}