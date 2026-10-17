namespace IdentityServer.Client.Models.Tenants;

public sealed record TenantModel(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    DateTime CreatedAtUtc,
    int UserCount,
    bool RequireMfa = false);

public sealed record TenantDetailModel(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    DateTime CreatedAtUtc,
    int UserCount,
    List<TenantUserModel> Users,
    SubscriptionModel? CurrentSubscription,
    bool RequireMfa = false);

public sealed record TenantUserModel(Guid UserId, bool IsActive, DateTime JoinedAtUtc);

public sealed record SubscriptionModel(
    Guid Id,
    string Plan,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive,
    int MaxCompanies,
    int MaxUsersPerCompany);

public sealed record CreateTenantRequest(string Name, string Slug, Guid OwnerId);

public sealed record UpdateTenantRequest(string Name, bool RequireMfa = false);

public sealed record AddTenantUserRequest(Guid UserId);
