using Microsoft.EntityFrameworkCore.Migrations;

namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure.Migrations
{
    public partial class AddRoleClaimUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete duplicated role claim rows (keep the first by MIN(Id)) to allow creating unique index
            migrationBuilder.Sql(@"DELETE FROM \"RoleClaims\" r
USING (
  SELECT \"RoleId\",\"ClaimType\",\"ClaimValue\", MIN(\"Id\") as keep_id
  FROM \"RoleClaims\"
  GROUP BY \"RoleId\",\"ClaimType\",\"ClaimValue\"
  HAVING COUNT(*) > 1
) dup
WHERE r.\"RoleId\" = dup.\"RoleId\" AND r.\"ClaimType\" = dup.\"ClaimType\" AND r.\"ClaimValue\" = dup.\"ClaimValue\" AND r.\"Id\" <> dup.keep_id;");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId_ClaimType_ClaimValue_Unique",
                table: "RoleClaims",
                columns: new[] { "RoleId", "ClaimType", "ClaimValue" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoleClaims_RoleId_ClaimType_ClaimValue_Unique",
                table: "RoleClaims");
        }
    }
}