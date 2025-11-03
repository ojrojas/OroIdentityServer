// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.BuildingBlocks.ServiceDefaults;

public static class GetDestination
{
    public static IEnumerable<string> GetDestinations(Claim claim)
    {
        return claim.Type switch
        {
            Claims.Name or
            Claims.Subject
                => new[] { Destinations.AccessToken, Destinations.IdentityToken },

            _ => [Destinations.AccessToken],
        };
    }
}