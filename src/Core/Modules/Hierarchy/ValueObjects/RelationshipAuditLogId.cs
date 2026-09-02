// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
namespace OroIdentityServer.Core.Modules.Hierarchy.ValueObjects;

public sealed record RelationshipAuditLogId(Guid Value)
{
    public static RelationshipAuditLogId New() => new(Guid.CreateVersion7());
    public static RelationshipAuditLogId From(Guid value) => new(value);
}
