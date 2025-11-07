namespace OroIdentityServer.OroIdentityServer.Infraestructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(OroIdentityAppContext context, string jsonFilePath)
    {
        Guid userCreateId  = Guid.CreateVersion7();
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"Seed data file not found: {jsonFilePath}");
        }

        var jsonData = await File.ReadAllTextAsync(jsonFilePath);
        var seedData = JsonSerializer.Deserialize<SeedData>(jsonData) ?? throw new InvalidOperationException("Failed to deserialize seed data.");
        if (!context.IdentificationTypes.Any())
        {
            context.IdentificationTypes.Add(
                new IdentificationType(new (seedData.IdentificationType)){
                    CreatedBy = userCreateId
                });
        }

        if (!context.Users.Any())
        {
            foreach (var user in seedData.Users)
            context.Users.Add(new User
            {
                Name = user.Name,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Identification = user.Identification,
                IdentificationTypeId = Guid.NewGuid(), // Replace with actual ID if needed
                SecurityUser = new SecurityUser
                {
                    PasswordHash = user.PasswordHash,
                    SecurityStamp = Guid.NewGuid().ToString()
                },
                CreatedBy = userCreateId
            });
        }

        if (!context.Roles.Any())
        {
            foreach (var role in seedData.Roles)
            {
                var newRole = new Role(new RoleName(role.Name))
                {
                    ConcurrencyStamp = Guid.NewGuid(),
                    CreatedBy = userCreateId
                };

                foreach (var roleClaim in role.RoleClaims)
                {
                    context.RoleClaims.Add(new RoleClaim
                    {
                        RoleId = newRole.Id,
                        ClaimType = roleClaim.ClaimType,
                        ClaimValue = roleClaim.ClaimValue,
                        CreatedBy = userCreateId
                    });
                }

                context.Roles.Add(newRole);
            }
        }

        if (!context.Set<UserRoles>().Any())
        {
            var adminRole = context.Roles.FirstOrDefault(r => r.RoleName.Value == "Administrator");
            var userRole = context.Roles.FirstOrDefault(r => r.RoleName.Value == "User");

            if (adminRole != null && userRole != null)
            {
                var pepe = context.Users.FirstOrDefault(u => u.Email == "pepe@example.com");
                var maria = context.Users.FirstOrDefault(u => u.Email == "maria@example.com");

                if (pepe != null)
                {
                    context.Set<UserRoles>().Add(new UserRoles
                    {
                        UserId = pepe.Id,
                        RoleId = adminRole.Id
                    });
                }

                if (maria != null)
                {
                    context.Set<UserRoles>().Add(new UserRoles
                    {
                        UserId = maria.Id,
                        RoleId = userRole.Id
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }
}

public class SeedData
{
    public string IdentificationType { get; set; } = string.Empty;
    public List<SeedUser> Users { get; set; } = new();
    public List<SeedRole> Roles { get; set; } = new();
}

public class SeedUser
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Identification { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}

public class SeedRole
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<SeedRoleClaim> RoleClaims { get; set; } = new();
}

public class SeedRoleClaim
{
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}