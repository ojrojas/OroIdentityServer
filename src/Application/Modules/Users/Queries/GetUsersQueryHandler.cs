// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Users.Queries;

public class GetUsersQueryHandler(
    ILogger<GetUserByEmailQueryHandler> logger,
    IUserRepository repository
) : IQueryHandler<GetUsersQuery, GetUsersQueryResponse>
{
    public async Task<GetUsersQueryResponse> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetUsersQuery");
        var data = await repository.GetAllUsersAsync(cancellationToken);

        if (data is null)
        {
            logger.LogWarning("No users found in the repository");
            return new GetUsersQueryResponse
            {
                Data = null,
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = "No users found."
            };
        }

        var userList = data.ToList();

        if (query.TenantId is { } tenantId)
            userList = userList.Where(u => u.TenantId?.Value == tenantId).ToList();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            userList = userList.Where(u =>
                (u.UserName != null && u.UserName.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (u.Email != null && u.Email.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (u.Name != null && u.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (u.LastName != null && u.LastName.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (u.Identification != null && u.Identification.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        var totalCount = userList.Count;
        var skip = (query.PageNumber - 1) * query.PageSize;
        var pagedUsers = userList.Skip(skip).Take(query.PageSize).ToList();

        GetUsersQueryResponse response = new()
        {
            Data = pagedUsers,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            StatusCode = (int)HttpStatusCode.OK,
            Message = "Users retrieved successfully."
        };

        logger.LogInformation("Successfully handled GetUsersQuery");
        return response;
    }
}