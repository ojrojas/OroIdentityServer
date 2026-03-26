// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Queries;

public class GetRolesQueryHandler(
    ILogger<GetRolesQueryHandler> logger, IRolesRepository roleRepository
    )
    : IQueryHandler<GetRolesQuery, GetRolesResponse>
{
    private readonly IRolesRepository _roleRepository = roleRepository;
    private readonly ILogger<GetRolesQueryHandler> _logger = logger;

    public async Task<GetRolesResponse> HandleAsync(GetRolesQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetRolesQuery");

        try
        {
            var roles = await _roleRepository.GetAllRolesAsync(cancellationToken);

            _logger.LogInformation("Successfully retrieved roles");

            return new GetRolesResponse
            {
                Data = roles,
                StatusCode = 200,
                Message = "Roles retrieved successfully."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving roles");

            return new GetRolesResponse
            {
                Errors = ["An error occurred while retrieving roles."],
                StatusCode = 500,
                Message = "An error occurred while retrieving roles."
            };
        }
    }
}