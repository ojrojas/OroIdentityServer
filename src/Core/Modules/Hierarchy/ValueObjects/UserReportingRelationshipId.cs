// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
namespace OroIdentityServer.Core.Modules.Hierarchy.ValueObjects;

public sealed record UserReportingRelationshipId(Guid Value)
{
    public static UserReportingRelationshipId New() => new(Guid.CreateVersion7());
    public static UserReportingRelationshipId From(Guid value) => new(value);
}
