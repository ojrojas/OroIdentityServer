// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Services.OroIdentityServer.Application.Queries;

public record GetIdentificationTypeByIdQuery(Guid Id) : IQuery<GetIdentificationTypeByIdResponse>
{
    public Guid CorrelationId() => Guid.NewGuid();
}