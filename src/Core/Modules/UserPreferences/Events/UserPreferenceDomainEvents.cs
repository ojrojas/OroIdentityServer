// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Core.Modules.UserPreferences.Events;

public sealed record UserPreferenceCreatedDomainEvent(
    UserPreferenceId PreferenceId,
    string UserId,
    Guid TenantId,
    Language Language,
    AppTheme Theme)     : DomainEvent;

public sealed record UserPreferenceUpdatedDomainEvent(
    UserPreferenceId PreferenceId,
    string UserId,
    Guid TenantId)     : DomainEvent;

public sealed record DefaultCompanySetDomainEvent(
    UserPreferenceId PreferenceId,
    string UserId,
    Guid? CompanyId)     : DomainEvent;

public sealed record DashboardLayoutUpdatedDomainEvent(
    UserPreferenceId PreferenceId,
    string UserId)     : DomainEvent;
