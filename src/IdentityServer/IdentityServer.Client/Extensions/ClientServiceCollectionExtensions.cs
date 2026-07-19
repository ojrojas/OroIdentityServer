using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Services;

namespace IdentityServer.Client.Extensions;

public static class ClientServiceCollectionExtensions
{
    /// <summary>
    /// Registers the admin API connector services with a fixed base address, known up front.
    /// Use from the WASM client's Program.cs, where builder.HostEnvironment.BaseAddress is available at startup.
    /// Backed by IHttpClientFactory (AddHttpClient), which is safe here because WASM has no per-request scope.
    /// </summary>
    public static IServiceCollection AddIdentityServerClientServices(this IServiceCollection services, Uri baseAddress)
    {
        services.AddHttpClient<IAdminUserService, AdminUserService>(httpClient => httpClient.BaseAddress = baseAddress);
        services.AddHttpClient<IAdminRoleService, AdminRoleService>(httpClient => httpClient.BaseAddress = baseAddress);
        services.AddHttpClient<IAdminPermissionService, AdminPermissionService>(httpClient => httpClient.BaseAddress = baseAddress);
        services.AddHttpClient<IAdminIdentificationTypeService, AdminIdentificationTypeService>(httpClient => httpClient.BaseAddress = baseAddress);
        services.AddHttpClient<IAdminTenantService, AdminTenantService>(httpClient => httpClient.BaseAddress = baseAddress);
        services.AddHttpClient<IAdminUserSessionService, AdminUserSessionService>(httpClient => httpClient.BaseAddress = baseAddress);
        services.AddHttpClient<IAdminSessionService, AdminSessionService>(httpClient => httpClient.BaseAddress = baseAddress);
        services.AddHttpClient<IAdminApplicationService, AdminApplicationService>(httpClient => httpClient.BaseAddress = baseAddress);
        services.AddHttpClient<IAdminScopeService, AdminScopeService>(httpClient => httpClient.BaseAddress = baseAddress);

        return services;
    }
}
