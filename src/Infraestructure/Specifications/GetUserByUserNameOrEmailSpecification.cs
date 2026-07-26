// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Infraestructure.Specifications;

public sealed class GetUserByUserNameOrEmailSpecification : Specification<User>
{
    public GetUserByUserNameOrEmailSpecification(string loginIdentifier)
    {
        var normalizedLoginIdentifier = loginIdentifier.ToUpperInvariant();
        Criteria = x =>
            x.NormalizedUserName == normalizedLoginIdentifier ||
            x.NormalizedEmail == normalizedLoginIdentifier;

        AddInclude(x => x.Roles);
        AddInclude(x => x.SecurityUser!);
    }
}
