// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Server.Authentication;

using Microsoft.AspNetCore.DataProtection;

/// <summary>
/// Namespaces every protector it hands out under a fixed purpose. Cookie authentication tickets
/// are protected with a <c>TicketDataFormat</c> built from <see cref="IDataProtectionProvider"/>,
/// so giving each cookie scheme its own purpose prefix is what keeps a ticket from one scheme
/// readable by the other: the derived key depends on the purpose chain, and the two chains do
/// not overlap.
/// </summary>
public sealed class PurposeDataProtectionProvider(IDataProtectionProvider inner, string purpose) : IDataProtectionProvider
{
    public IDataProtector CreateProtector(string protectorPurpose)
        => inner.CreateProtector($"{purpose}.{protectorPurpose}");
}