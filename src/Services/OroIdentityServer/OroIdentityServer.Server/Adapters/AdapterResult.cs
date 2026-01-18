// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Server.Adapters;

public class AdapterResult(IResult inner) : IResult
{
    private readonly IResult _inner = inner;

    public async Task ExecuteAsync(HttpContext context)
    {
        await _inner.ExecuteAsync(context);
    }
}