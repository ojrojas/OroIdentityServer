using IdentityServer.Client.Interfaces;
using IdentityServer.Services;
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

        return services;
    }
}
