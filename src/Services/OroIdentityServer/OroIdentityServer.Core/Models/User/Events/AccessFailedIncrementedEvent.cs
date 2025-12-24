// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public partial class SecurityUser
{
    public sealed record AccessFailedIncrementedEvent(SecurityUserId SecurityUserId, int FailedCount) : DomainEventBase;
}
