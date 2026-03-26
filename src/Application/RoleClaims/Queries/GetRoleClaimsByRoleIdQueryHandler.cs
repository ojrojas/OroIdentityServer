// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Queries;

public class GetRoleClaimsByRoleIdQueryHandler(ILogger<GetRoleClaimsByRoleIdQueryHandler> logger, IRolesRepository roleRepository) : IQueryHandler<GetRoleClaimsByRoleIdQuery, GetRoleClaimsByRoleIdResponse>
{
    private readonly ILogger<GetRoleClaimsByRoleIdQueryHandler> _logger = logger;
    private readonly IRolesRepository _roleRepository = roleRepository;

    public async Task<GetRoleClaimsByRoleIdResponse> HandleAsync(GetRoleClaimsByRoleIdQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetRoleClaimsByRoleIdQuery for RoleId: {RoleId}", query.RoleId);

        var roleClaims = await _roleRepository.GetRoleClaimsByRoleIdAsync(query.RoleId, cancellationToken);

        _logger.LogInformation("Successfully retrieved RoleClaims for RoleId: {RoleId}", query.RoleId);

        return new GetRoleClaimsByRoleIdResponse
        {
            Data = roleClaims
        };
    }
}