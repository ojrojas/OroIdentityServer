// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.

namespace OroIdentityServer.Application.Modules.Users.Commands;

public record UpdateUserCommand(
    Guid UserId,
    string Name,
    string MiddleName,
    string LastName,
    string UserName, 
    string Email, 
    string Password,
    string Identification,
    Guid IdentificationTypeId,
    Guid TenantId
) : ICommand<UpdateUserResponse>
{
    public Guid CorrelationId() => Guid.NewGuid();
}
