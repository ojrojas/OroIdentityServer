// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

/// <summary>
/// Represents a query to retrieve a user by their unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the user to retrieve.</param>
/// <remarks>
/// This query implements the <see cref="IQuery{TResponse}"/> interface, where the response type is <see cref="GetUserByIdQueryResponse"/>.
/// A new correlation ID is generated for each instance of this query.
/// </remarks>
public record GetUserByIdQuery(Guid Id) : IQuery<GetUserByIdQueryResponse>
{
    public Guid CorrelationId => Guid.NewGuid();
}
