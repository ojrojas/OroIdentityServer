// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Tenants.ValueObjects;

public static class TenantRole
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Member = "Member";

    public static bool IsValid(string role) => role is Admin or Manager or Member;
}
