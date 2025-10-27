// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, GetUsersQueryResponse>
{
    public ValueTask<GetUsersQueryResponse> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}