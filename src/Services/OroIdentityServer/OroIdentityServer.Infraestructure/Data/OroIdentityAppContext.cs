// OroIdentityServer
// Copyright (C) 2025 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure;

public class OroIdentityAppContext: AuditableDbContext
{
    public OroIdentityAppContext(
        DbContextOptions<OroIdentityAppContext> options, IOptions<UserInfo> optionsUser) 
        : base(options, optionsUser)
    {

    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<IdentificationType> IdentificationTypes { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RoleClaim> RoleClaims { get; set; }
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentificationType>(config =>
        {
            config.ToTable(nameof(IdentificationTypes));
            config.HasKey(x => x.Id);
            config.OwnsOne(x => x.IdentificationTypeName, nv =>
            {
                nv.Property(p => p.Value)
                //.HasColumnName("IdentificationTypeName")
                .HasMaxLength(50)
                .IsRequired();
            });
        });

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}