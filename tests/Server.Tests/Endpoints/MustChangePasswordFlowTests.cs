// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using System.Net;
using System.Text.RegularExpressions;
using OroIdentityServer.Core.Modules.IdentificationTypes.Aggregates;
using OroIdentityServer.Core.Modules.Tenants.Aggregates;
using OroIdentityServer.Core.Modules.Tenants.ValueObjects;
using OroIdentityServer.Core.Modules.Users.Aggregates;
using OroIdentityServer.Core.Modules.Users.Entities;
using OroIdentityServer.Infraestructure;
using OroIdentityServer.Server.Tests.Infrastructure;
using Xunit;

namespace OroIdentityServer.Server.Tests.Endpoints;

[Collection(nameof(AspireTestCollection))]
public sealed class MustChangePasswordFlowTests(AspireIdentityServerApp app)
{
    private const string Password = "Abc123456#";

    [Fact]
    public async Task ChangePasswordFlow_StaticAssets_AreServed()
    {
        var userName = $"mustchange-{Guid.NewGuid():N}";

        await using (var context = app.CreateDbContext())
        {
            var passwordHasher = app.PasswordHasher;

            var identificationType = context.IdentificationTypes
                .AsEnumerable()
                .FirstOrDefault(i => i.Name.Value == "Passport");
            if (identificationType is null)
            {
                identificationType = IdentificationType.Create("Passport");
                context.IdentificationTypes.Add(identificationType);
            }

            var tenant = Tenant.Create($"Tenant-MustChange-{Guid.NewGuid():N}");
            context.Tenants.Add(tenant);

            var user = User.Create(
                userName, $"{userName}@example.com", "Test", "", "User",
                Guid.NewGuid().ToString("N"), identificationType.Id, tenant.Id);

            // No ExemptFromPasswordChange(): the must_change_password claim stays set.
            var securityUser = SecurityUser.Create(await passwordHasher.HashPassword(Password));
            context.SecurityUsers.Add(securityUser);
            user.AssignSecurityUser(securityUser);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            tenant.AddUser(user.Id, TenantRole.Admin);
            await context.SaveChangesAsync();
        }

        // 1. Login -> the must_change_password claim forces a redirect to ChangePassword.
        var client = app.CreateClient();
        var login = await client.PostAsync("/auth/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["loginIdentifier"] = userName,
            ["password"] = Password
        }));
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal("/Account/ChangePassword", login.Headers.Location?.OriginalString);

        // 2. GET the ChangePassword page (full navigation with the claim present).
        var page = await client.GetAsync("/Account/ChangePassword");
        var html = await page.Content.ReadAsStringAsync();
        Assert.True(page.IsSuccessStatusCode, $"page status={page.StatusCode}");

        // 3. Every static asset the page references must be served with its real content type,
        //    never redirected to HTML by the must-change-password middleware.
        //    The page emits relative hrefs/srcs resolved against <base href="/" />, so normalize
        //    them to absolute paths and drop the base tag itself.
        var assetUrls = Regex.Matches(html, "(src|href)=\"([^\"]+)\"")
            .Select(m => m.Groups[2].Value)
            .Where(u => !u.Contains("/Account/") && !u.StartsWith("_framework/debug"))
            .Select(u => u.StartsWith("/") ? u : "/" + u)
            .Where(u => u != "/")
            .Distinct()
            .ToList();

        Assert.NotEmpty(assetUrls);

        var failures = new List<string>();
        foreach (var url in assetUrls)
        {
            var asset = await client.GetAsync(url);
            var ctype = asset.Content.Headers.ContentType?.ToString();
            if (!asset.IsSuccessStatusCode || ctype?.StartsWith("text/html") == true)
                failures.Add($"{url} -> {asset.StatusCode} ({ctype})");
        }

        Assert.True(failures.Count == 0, $"Broken assets: {string.Join(" | ", failures)}");
    }
}
