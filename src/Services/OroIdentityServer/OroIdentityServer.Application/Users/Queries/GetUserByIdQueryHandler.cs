// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Services.OroIdentityServer.Application.Queries;

public class GetUserByIdQueryHandler(
    Logger<GetUserByIdQueryHandler> logger, IRepository<User> repository) 
    : IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResponse>
{
    public ValueTask<GetUserByIdQueryResponse> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
    {

        throw new NotImplementedException();
    }
}