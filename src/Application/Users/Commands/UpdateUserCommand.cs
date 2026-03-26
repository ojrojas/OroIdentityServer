// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Commands;

public record UpdateUserCommand(
    UserId UserId,
    string Name,
    string MiddleName,
    string LastName,
    string UserName, 
    string Email, 
    string Password,
    string Identification,
    IdentificationTypeId IdentificationTypeId,
    TenantId TenantId
) : ICommand<UpdateUserResponse>
{
    public Guid CorrelationId() => Guid.NewGuid();
}
