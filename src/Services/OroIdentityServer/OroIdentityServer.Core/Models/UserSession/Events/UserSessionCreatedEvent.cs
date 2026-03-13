// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Core.Models;

public record UserSessionCreatedEvent(
    UserSessionId UserSessionId, 
    UserId UserId, 
    string Device, 
    string SessionToken, 
    DateTime ExpiresAt, 
    string? IpAddress = null, 
    string? UserAgent = null, 
    string? Location = null) : DomainEventBase;
