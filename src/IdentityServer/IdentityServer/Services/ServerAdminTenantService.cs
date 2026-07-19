using System.Net;
using BuildingBlocks.CQRS.Abstractions;
using IdentityServer.Client.Interfaces;
using IdentityServer.Client.Models;
using IdentityServer.Client.Models.Tenants;
using OroIdentityServer.Application.Modules.Tenants.Commands;
using OroIdentityServer.Application.Modules.Tenants.DTOs;
using OroIdentityServer.Application.Modules.Tenants.Queries;

namespace IdentityServer.Services;

public class ServerAdminTenantService(IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher) : IAdminTenantService
{
    public async Task<ApiResponse<IEnumerable<TenantModel>>?> GetTenantsAsync(CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetTenantsQuery(), ct);
        return new ApiResponse<IEnumerable<TenantModel>>
        {
            Data = result.Data?.Select(MapTenant).ToList() ?? [],
            StatusCode = result.StatusCode,
            Message = result.Message,
            Errors = result.Errors
        };
    }

    public async Task<ApiResponse<TenantDetailModel>?> GetTenantByIdAsync(Guid id, CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetTenantByIdQuery(id), ct);
        return new ApiResponse<TenantDetailModel>
        {
            Data = result.Data is null ? null : MapTenantDetail(result.Data),
            StatusCode = result.StatusCode,
            Message = result.Message,
            Errors = result.Errors
        };
    }

    public async Task<ApiResponse<IEnumerable<TenantModel>>?> GetTenantsByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var result = await queryDispatcher.SendAsync(new GetTenantsByUserIdQuery(userId), ct);
        return new ApiResponse<IEnumerable<TenantModel>>
        {
            Data = result.Data?.Select(MapTenant).ToList() ?? [],
            StatusCode = result.StatusCode,
            Message = result.Message,
            Errors = result.Errors
        };
    }

    public async Task<HttpResponseMessage> CreateTenantAsync(CreateTenantRequest request, CancellationToken ct = default)
    {
        var command = new CreateTenantCommand(request.Name, request.Slug, request.OwnerId);
        var result = await commandDispatcher.SendAsync(command, ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.Created);
    }

    public async Task<HttpResponseMessage> UpdateTenantAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new UpdateTenantCommand(id, request.Name), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public async Task<HttpResponseMessage> ActivateTenantAsync(Guid id, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new ActivateTenantCommand(id), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public async Task<HttpResponseMessage> SuspendTenantAsync(Guid id, CancellationToken ct = default)
    {
        var result = await commandDispatcher.SendAsync(new SuspendTenantCommand(id), ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    public async Task<HttpResponseMessage> AddTenantUserAsync(Guid id, AddTenantUserRequest request, CancellationToken ct = default)
    {
        var command = new AddTenantUserCommand(id, request.UserId, request.Role);
        var result = await commandDispatcher.SendAsync(command, ct);
        return HttpResponseMessageFactory.FromResult(result, HttpStatusCode.NoContent);
    }

    private static TenantModel MapTenant(TenantDto tenant) => new(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, tenant.CreatedAtUtc, tenant.UserCount);

    private static TenantDetailModel MapTenantDetail(TenantDetailDto tenant) => new(
        tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, tenant.CreatedAtUtc, tenant.UserCount,
        tenant.Users.Select(u => new TenantUserModel(u.UserId, u.RoleId, u.IsActive, u.JoinedAtUtc)).ToList(),
        tenant.CurrentSubscription is null ? null : new SubscriptionModel(
            tenant.CurrentSubscription.Id, tenant.CurrentSubscription.Plan, tenant.CurrentSubscription.StartDate,
            tenant.CurrentSubscription.EndDate, tenant.CurrentSubscription.IsActive,
            tenant.CurrentSubscription.MaxCompanies, tenant.CurrentSubscription.MaxUsersPerCompany));
}
