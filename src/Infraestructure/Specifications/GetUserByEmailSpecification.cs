// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetUserByEmailSpecification : Specification<User>
{
    public GetUserByEmailSpecification(string email) : base(x => x.NormalizedEmail == email.ToUpperInvariant())
    {
        AddInclude(x => x.Roles);
        // Catalogue role is required so password-reset and other email-keyed flows can
        // surface the user's role names back to the admin UI.
        AddInclude("Roles.Role");
        AddInclude(x => x.SecurityUser!);
    }
}
