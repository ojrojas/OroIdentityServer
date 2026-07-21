// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using OroIdentityServer.Core.Modules.UserSessions.Aggregates;

namespace OroIdentityServer.Application.Modules.UserSessions.Queries;

public record GetAllActiveUserSessionsQuery : IQuery<IEnumerable<UserSession>>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
