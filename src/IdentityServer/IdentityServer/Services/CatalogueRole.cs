// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace IdentityServer.Services;

/// <summary>
/// The canonical names of the roles seeded in the catalogue (see <c>DatabaseSeeder</c> and
/// <c>seedData.json</c>). The sign-in service reads these to translate a user's catalogue
/// memberships into the claim names that the [Authorize] policies and Razor page attributes
/// check.
/// </summary>
public static class CatalogueRole
{
    public const string Administrator = nameof(Administrator);
    public const string Admin = nameof(Admin);
    public const string Manager = nameof(Manager);
    public const string User = nameof(User);
}
