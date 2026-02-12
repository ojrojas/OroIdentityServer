using System.Net.Http.Json;
using OroIdentity.Web.Client.Interfaces;
using OroIdentity.Web.Client.Models;

namespace OroIdentity.Web.Client.Services;

internal sealed class RolesClientService(
    ILogger<RolesClientService> logger,
    HttpClient httpClient) : IRolesService
{
    private readonly string ROUTEROLES = "api/v1/roles";

    public async Task CreateRoleAsync(RoleViewModel role, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating role with name: {RoleName}", role.Name);
        await httpClient.PostAsJsonAsync(ROUTEROLES, role, cancellationToken);
        logger.LogInformation("Role with name: {RoleName} created successfully", role.Name);
    }

    public async Task DeleteRoleAsync(string roleId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting role with ID: {RoleId}", roleId);
        await httpClient.DeleteAsync($"{ROUTEROLES}/{roleId}", cancellationToken);
        logger.LogInformation("Role with ID: {RoleId} deleted successfully", roleId);
    }

    public async Task<BaseResponseViewModel<IEnumerable<RoleViewModel>>> GetAllRolesAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all roles");
        var response = await httpClient.GetFromJsonAsync<BaseResponseViewModel<IEnumerable<RoleViewModel>>>(ROUTEROLES, cancellationToken);
        return response;
    }

    public async Task<RoleViewModel> GetRoleByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving role with ID: {RoleId}", roleId);
        return await httpClient.GetFromJsonAsync<RoleViewModel>($"{ROUTEROLES}/{roleId}", cancellationToken)
               ?? new RoleViewModel() { Name = string.Empty };
    }

    public async Task<RoleViewModel> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving role with name: {RoleName}", roleName);
        return await httpClient.GetFromJsonAsync<RoleViewModel>($"{ROUTEROLES}/{roleName}", cancellationToken)
               ?? new RoleViewModel() { Name = string.Empty };
    }

    public async Task UpdateRoleAsync(RoleViewModel role, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating role with ID: {RoleId}", role.RoleId);
        await httpClient.PutAsJsonAsync(ROUTEROLES, role, cancellationToken);
        logger.LogInformation("Role with ID: {RoleId} updated successfully", role.RoleId);
    }

   
}