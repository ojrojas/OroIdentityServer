// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Api.Models;

public class LoginResponse
{
    public Enums.ResultTypes ResultType { get; set; }
    public ClaimsPrincipal? Principal { get; set; }
    public AuthenticationProperties? Properties { get; set; }
    public string[] Schemes { get; set; }

    public LoginResponse(Enums.ResultTypes resultType, ClaimsPrincipal? principal, AuthenticationProperties? properties, string[] schemes)
    {
        ResultType = resultType;
        Principal = principal;
        Properties = properties;
        Schemes = schemes;
    }
}