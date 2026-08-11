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
        services.AddScoped<ICurrentTenantContext, CurrentTenantContext>();
        services.AddTransient<TenantHeaderHandler>();

        services.AddHttpClient<IAdminUserService, AdminUserService>(httpClient => httpClient.BaseAddress = baseAddress).AddHttpMessageHandler<TenantHeaderHandler>();
        services.AddHttpClient<IAdminRoleService, AdminRoleService>(httpClient => httpClient.BaseAddress = baseAddress).AddHttpMessageHandler<TenantHeaderHandler>();
        services.AddHttpClient<IAdminPermissionService, AdminPermissionService>(httpClient => httpClient.BaseAddress = baseAddress).AddHttpMessageHandler<TenantHeaderHandler>();
        services.AddHttpClient<IAdminIdentificationTypeService, AdminIdentificationTypeService>(httpClient => httpClient.BaseAddress = baseAddress).AddHttpMessageHandler<TenantHeaderHandler>();
        services.AddHttpClient<IAdminTenantService, AdminTenantService>(httpClient => httpClient.BaseAddress = baseAddress).AddHttpMessageHandler<TenantHeaderHandler>();
        services.AddHttpClient<IAdminUserSessionService, AdminUserSessionService>(httpClient => httpClient.BaseAddress = baseAddress).AddHttpMessageHandler<TenantHeaderHandler>();
        services.AddHttpClient<IAdminSessionService, AdminSessionService>(httpClient => httpClient.BaseAddress = baseAddress).AddHttpMessageHandler<TenantHeaderHandler>();
        services.AddHttpClient<IAdminValidationLogService, AdminValidationLogService>(httpClient => httpClient.BaseAddress = baseAddress).AddHttpMessageHandler<TenantHeaderHandler>();
        services.AddHttpClient<IAdminApplicationService, AdminApplicationService>(httpClient => httpClient.BaseAddress = baseAddress).AddHttpMessageHandler<TenantHeaderHandler>();
        services.AddHttpClient<IAdminScopeService, AdminScopeService>(httpClient => httpClient.BaseAddress = baseAddress).AddHttpMessageHandler<TenantHeaderHandler>();

        return services;
    }

    /// <summary>
    /// UI services that replace the FluentUI providers (toasts and dialogs).
    /// Registered on both the WASM client and the server host, since components
    /// prerender on the server under InteractiveAuto.
    /// </summary>
    public static IServiceCollection AddIdentityServerUiServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrentTenantContext, CurrentTenantContext>();
        services.AddScoped<IToastService, ToastService>();
        services.AddScoped<IDialogService, DialogService>();

        return services;
    }
}
