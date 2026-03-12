using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionAuthorizationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorizationId",
                table: "Sessions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizationId",
                table: "Sessions");
        }
    }
}
