// OroIdentityServer
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Application.Modules.Tenants.DTOs;

public record SubscriptionDto(
    Guid Id,
    string Plan,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive,
    int MaxCompanies,
    int MaxUsersPerCompany);
