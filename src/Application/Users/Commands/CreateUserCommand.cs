// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Commands;

/// <summary>
/// Represents a command to create a new user.
/// </summary>
/// <param name="Name">The first name of the user.</param>
/// <param name="MiddleName">The middle name of the user.</param>
/// <param name="LastName">The last name of the user.</param>
/// <param name="UserName">The username for the user account.</param>
/// <param name="Email">The email address of the user.</param>
/// <param name="Password">The password for the user account.</param>
/// <param name="Identification">The identification number of the user.</param>
/// <param name="IdentificationTypeId">The unique identifier for the type of identification.</param>
/// <returns>A new instance of the <see cref="CreateUserCommand"/> class.</returns>
public record CreateUserCommand(
    string Name,
    string MiddleName,
    string LastName,
    string UserName, 
    string Email, 
    string Password,
    string Identification,
    IdentificationTypeId IdentificationTypeId,
    TenantId TenantId
) : ICommand
{
    public Guid CorrelationId() => Guid.NewGuid();
}



