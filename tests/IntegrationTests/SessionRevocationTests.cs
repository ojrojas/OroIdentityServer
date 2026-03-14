using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

public class SessionRevocationTests
{
    [Fact]
    public async Task TerminateSession_RevokesAssociatedTokens()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<OroIdentityServer.Services.OroIdentityServer.Infraestructure.OroIdentityAppContext>();
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();
        var authorizationManager = provider.GetRequiredService<IOpenIddictAuthorizationManager>();

        // Create an OpenIddict application
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "integration-test-client",
            DisplayName = "Integration Test Client",
            ClientType = OpenIddictConstants.ClientTypes.Public,
            ApplicationType = OpenIddictConstants.ApplicationTypes.Native,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit
        };

        await applicationManager.CreateAsync(descriptor);
        var application = await applicationManager.FindByClientIdAsync("integration-test-client");
        var applicationId = await applicationManager.GetIdAsync(application);

        // Create a subject and identity
        var subject = System.Guid.NewGuid().ToString();
        var identity = new ClaimsIdentity(new[] { new Claim(OpenIddictConstants.Claims.Subject, subject) }, "Test");

        // Create an authorization for that subject
        var authorization = await authorizationManager.CreateAsync(identity, subject, applicationId, OpenIddictConstants.AuthorizationTypes.Permanent, new[] { OpenIddictConstants.Scopes.OpenId });
        var authorizationId = await authorizationManager.GetIdAsync(authorization);

        // Create a token record directly in the DB linked to the authorization
        var tokenEntity = new OpenIddictEntityFrameworkCoreToken
        {
            AuthorizationId = authorizationId,
            Subject = subject,
            Type = OpenIddictConstants.TokenTypeHints.RefreshToken,
            Status = OpenIddictConstants.Statuses.Valid,
            CreationDate = System.DateTime.UtcNow,
            ExpirationDate = System.DateTime.UtcNow.AddHours(1),
            Payload = "{}"
        };

        context.Add(tokenEntity);
        await context.SaveChangesAsync();

        // Create a session linked to the authorization
        var session = OroIdentityServer.Services.OroIdentityServer.Core.Models.Session.Create(
            new OroIdentityServer.Services.OroIdentityServer.Core.Models.UserId(System.Guid.NewGuid()),
            "127.0.0.1",
            "unknown",
            OroIdentityServer.Services.OroIdentityServer.Core.Models.TenantId.New(),
            authorizationId
        );

        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        // Call the TerminateSession endpoint
        var response = await client.DeleteAsync($"/sessions/{session.Id.Value}");
        response.EnsureSuccessStatusCode();

        // Reload token from DB and assert status changed to revoked
        var token = await context.Set<OpenIddictEntityFrameworkCoreToken>().FirstOrDefaultAsync(t => t.AuthorizationId == authorizationId);
        token.Should().NotBeNull();
        token!.Status.Should().Be(OpenIddictConstants.Statuses.Revoked);
    }
}
