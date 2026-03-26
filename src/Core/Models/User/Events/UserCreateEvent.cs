// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Models;

public sealed record UserCreateEvent(
        UserId Id,
        string Name,
        string MiddleName,
        string LastName,
        string UserName,
        string Email,
        string Identification,
        IdentificationTypeId IdentificationTypeId,
        TenantId TenantId) : DomainEventBase;
