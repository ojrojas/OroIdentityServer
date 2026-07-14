// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.Users.DomainEvents;

// Add UserDeletedEvent
public sealed record UserDeletedEvent(UserId UserId) : DomainEventBase;
