// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Application.Commands;

public record CreateUserCommand(
    string Name,
    string MiddleName,
    string LastName,
    string UserName, 
    string Email, 
    string Password,
    string Identification,
    Guid IdentificationTypeId
) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}



