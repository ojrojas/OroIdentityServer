// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Api.Models;

public record LoginResponse(
    ResultTypes ResultType,
    ClaimsPrincipal? Principal,
    AuthenticationProperties? Properties,
    string[] Schemes) : IResult
{

    public Task ExecuteAsync(HttpContext httpContext)
    {
        IResult result;
        switch (ResultType)
        {
            case ResultTypes.SignIn:
                ArgumentNullException.ThrowIfNull(Principal);
                result = Results.SignIn(Principal, Properties, Schemes.FirstOrDefault());
                break;
            case ResultTypes.Forbid:
                result = Results.Forbid(Properties, Schemes);
                break;
            case ResultTypes.BadRequest:
                result = Results.BadRequest(Schemes);
                break;
            case ResultTypes.Challenge:
                result = Results.Challenge(Properties, Schemes);
                break;
            case ResultTypes.Unauthorized:
                result = Results.Unauthorized();
                break;
            case ResultTypes.Redirect:
                result = Results.Redirect(Properties?.RedirectUri ?? "/");
                break;
            default:
                result = Results.Forbid(Properties, Schemes);
                break;
        }

        var adapter = new AdapterResult(result);
        return adapter.ExecuteAsync(httpContext);
    }
}