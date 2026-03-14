using Microsoft.EntityFrameworkCore.Migrations;

namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure.Migrations
{
    public partial class AddRoleClaimUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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