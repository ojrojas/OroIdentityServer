// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Queries;

public class GetRoleClaimByIdQueryHandler(ILogger<GetRoleClaimByIdQueryHandler> logger, IRolesRepository roleRepository) : IQueryHandler<GetRoleClaimByIdQuery, GetRoleClaimByIdResponse>
{
    private readonly ILogger<GetRoleClaimByIdQueryHandler> _logger = logger;
    private readonly IRolesRepository _roleRepository = roleRepository;

    public async Task<GetRoleClaimByIdResponse> HandleAsync(GetRoleClaimByIdQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetRoleClaimByIdQuery for RoleClaimId: {RoleClaimId}", query.Id);

        // Convert RoleClaimId to Guid before calling the repository method
        var roleClaim = await _roleRepository.GetRoleClaimByIdAsync(query.Id.Value, cancellationToken);

        if (roleClaim == null)
        {
            _logger.LogWarning("RoleClaim not found for Id: {RoleClaimId}", query.Id);
            return new GetRoleClaimByIdResponse
            {
                Data = null,
                Errors = new List<string> { "RoleClaim not found." }
            };
        }

        _logger.LogInformation("Successfully retrieved RoleClaim with Id: {RoleClaimId}", query.Id);

        return new GetRoleClaimByIdResponse
        {
            Data = roleClaim
        };
    }
}