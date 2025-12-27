// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public sealed record RoleClaimRemovedEvent(RoleId RoleId, RoleClaimType ClaimType, RoleClaimValue ClaimValue) : DomainEventBase;