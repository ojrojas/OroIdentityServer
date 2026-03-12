using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OroIdentityServer.Services.OroIdentityServer.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Sessions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Sessions");
        }
    }
}
