// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public record UserSessionUpdatedEvent(
    UserSessionId UserSessionId, 
    DateTime? LastActivityAt = null, 
    DateTime? ExpiresAt = null, 
    string? IpAddress = null, 
    string? UserAgent = null, 
    string? Location = null) : DomainEventBase;
