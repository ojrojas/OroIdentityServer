using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OroIdentityServer.Infraestructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMfaBruteForceLockout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedAttempts",
                table: "MfaCredentials",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFailedAtUtc",
                table: "MfaCredentials",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntilUtc",
                table: "MfaCredentials",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MfaCredentials_LockedUntilUtc",
                table: "MfaCredentials",
                column: "LockedUntilUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MfaCredentials_LockedUntilUtc",
                table: "MfaCredentials");

            migrationBuilder.DropColumn(
                name: "FailedAttempts",
                table: "MfaCredentials");

            migrationBuilder.DropColumn(
                name: "LastFailedAtUtc",
                table: "MfaCredentials");

            migrationBuilder.DropColumn(
                name: "LockedUntilUtc",
                table: "MfaCredentials");
        }
    }
}
