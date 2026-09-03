// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Openddict.Queries;

public record GetApplicationsQuery(string? SearchTerm = null, int PageNumber = 1, int PageSize = 20) : IQuery<GetApplicationsPagedResponse>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}

public record GetApplicationsPagedResponse
{
    public IEnumerable<OpenIddictApplicationDescriptor> Data { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
