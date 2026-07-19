// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Security.Claims;
using IdentityServer.Server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OroIdentityServer.Core.Interfaces;
using OroIdentityServer.Core.Modules.Users.Repositories;
using OroIdentityServer.Infraestructure.Interfaces;
using OroIdentityServer.Server.Authentication;

namespace OroIdentityServer.Server.Endpoints;

public static class AuthEndpoints
{
    public sealed record LoginRequest(string LoginIdentifier, string Password, string? ReturnUrl);
    public sealed record ChangePasswordInputModel(string NewPassword, string ConfirmPassword);

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", async (
            HttpContext http,
            [FromForm]LoginInputModel loginInput,
            [FromServices]AdminPasswordSignInService signInService,
            CancellationToken ct) =>
        {
            var principal = await signInService.SignInAsync(loginInput.LoginIdentifier, loginInput.Password, ct);
            if (principal is null)
            {
                var errorReturnUrl = Uri.EscapeDataString(loginInput.ReturnUrl ?? string.Empty);
                return Results.Redirect($"/Account/Login?error=invalid&ReturnUrl={errorReturnUrl}");
            }

            await http.SignInAsync(CookieAuthHandlerSetup.AdminScheme, principal);

            if (principal.HasClaim(c => c.Type == AdminPasswordSignInService.MustChangePasswordClaimType))
                return Results.Redirect("/Account/ChangePassword");

            var target = string.IsNullOrWhiteSpace(loginInput.ReturnUrl) ? "/" : loginInput.ReturnUrl;
            return Results.Redirect(target);
        }).DisableAntiforgery();

        group.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthHandlerSetup.AdminScheme);
            return Results.Redirect("/Account/Login");
        }).DisableAntiforgery();

        group.MapPost("/change-password", async (
            HttpContext http,
            [FromForm] ChangePasswordInputModel input,
            [FromServices] IUserRepository userRepository,
            [FromServices] ISecurityUserRepository securityUserRepository,
            [FromServices] IPasswordHasher passwordHasher,
            [FromServices] AdminPasswordSignInService signInService,
            CancellationToken ct) =>
        {
            if (http.User.Identity?.IsAuthenticated != true)
                return Results.Redirect("/Account/Login");

            if (string.IsNullOrWhiteSpace(input.NewPassword) || input.NewPassword != input.ConfirmPassword)
                return Results.Redirect("/Account/ChangePassword?error=mismatch");

            var userId = Guid.Parse(http.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await userRepository.GetUserByIdAsync(new(userId), ct);
            if (user?.SecurityUserId is null)
                return Results.Redirect("/Account/Login");

            var securityUser = await securityUserRepository.GetSecurityUserAsync(user.SecurityUserId.Value, ct);
            securityUser.ChangePassword(await passwordHasher.HashPassword(input.NewPassword));
            await securityUserRepository.UpdateSecurityUserAsync(securityUser, ct);

            var principal = await signInService.RefreshPrincipalAsync(userId, ct);
            if (principal is not null)
                await http.SignInAsync(CookieAuthHandlerSetup.AdminScheme, principal);

            return Results.Redirect("/");
        }).DisableAntiforgery();

        return app;
    }
}
