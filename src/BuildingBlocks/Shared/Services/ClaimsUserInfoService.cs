// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.Shared.Services;

public class ClaimsUserInfoService(IHttpContextAccessor contextAccessor) : IPostConfigureOptions<UserInfo>
{
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));

    public void PostConfigure(string? name, UserInfo options)
    {
        var claims = _contextAccessor?.HttpContext?.User.Claims;

        _ = int.TryParse(claims?.First(x => x.Type == "").Value, out int state);
        options.State = (EntityBaseState)state;
        var guidId = claims?.First(x => x.Type == "Sub").Value;

        options.Id = string.IsNullOrEmpty(guidId) ? Guid.Empty : Guid.Parse(guidId);

        options.UserName = claims?.First(x => x.Type == "UserName").Value ?? string.Empty;
        options.Email = claims?.First(x => x.Type == "Email").Value ?? string.Empty;
    }
}