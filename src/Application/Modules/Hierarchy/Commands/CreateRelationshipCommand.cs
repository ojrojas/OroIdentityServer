// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
namespace OroIdentityServer.Application.Modules.Hierarchy.Commands;

public record CreateRelationshipCommand(
    Guid TenantId,
    Guid UserId,
    Guid ReportsToUserId,
    string Type,
    int Priority,
    Guid? PerformedByUserId) : ICommand;
