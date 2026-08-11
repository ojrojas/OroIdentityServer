namespace IdentityServer.Client.Models.Dashboard;

public sealed record RecentEntityModel(
    string Name,
    string TypeKey,
    string Href,
    string AvatarSeed,
    DateTime CreatedAtUtc);

public sealed record DashboardStatsModel(
    int UsersCreatedToday,
    int RolesCreatedToday,
    int TenantsCreatedToday,
    int IdentificationTypesCreatedToday,
    int ConnectedUsers,
    List<RecentEntityModel> RecentlyCreated);
