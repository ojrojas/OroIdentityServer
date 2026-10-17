using IdentityServer.Client.Interfaces;
using OroIdentityServer.Server.Authentication;
using IdentityServer.Services;
using OroIdentityServer.Core.Interfaces;
namespace IdentityServer.Server.Extensions;

/// <summary>
/// Registers the CQRS-backed ServerAdminXxxService implementations consumed by the /api/* minimal
/// API endpoints (Endpoints/*.cs), which delegate to these instead of calling ICommandHandler/
/// IQueryHandler directly. Not exposed through IAdminXxxService: that interface is reserved for the
/// HTTP-based client used by Razor components (see AddIdentityServerRazorComponentClientServices).
/// </summary>
public static class ServerAdminServiceCollectionExtensions
{
    public static IServiceCollection AddServerAdminServices(this IServiceCollection services)
    {
        services.AddScoped<IAdminUserService, ServerAdminUserService>();
        services.AddScoped<IAdminRoleService,ServerAdminRoleService>();
        services.AddScoped<IAdminPermissionService,ServerAdminPermissionService>();
        services.AddScoped<IAdminIdentificationTypeService, ServerAdminIdentificationTypeService>();
        services.AddScoped<IAdminTenantService, ServerAdminTenantService>();
        services.AddScoped<IAdminUserSessionService, ServerAdminUserSessionService>();
        services.AddScoped<IAdminSessionService, ServerAdminSessionService>();
        services.AddScoped<IAdminApplicationService, ServerAdminApplicationService>();
        services.AddScoped<IAdminScopeService, ServerAdminScopeService>();
        services.AddScoped<IAdminValidationLogService,ServerAdminValidationLogService>();
        services.AddScoped<IAdminDashboardService, ServerAdminDashboardService>();
        services.AddScoped<IAdminHierarchyService, ServerAdminHierarchyService>();
        services.AddScoped<IAdminBrandingService, ServerBrandingService>();
        services.AddScoped<IAdminMfaService, ServerAdminMfaService>();

        // Second-factor support services. MfaSecretProtector lives here because Data Protection
        // is an ASP.NET Core facility, while the interface it implements lives in Core.
        services.AddScoped<IMfaSecretProtector, MfaSecretProtector>();
        services.AddScoped<ISessionExpiryEmailComposer, SessionExpiryEmailComposer>();

        return services;
    }
}
