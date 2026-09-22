using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OroIdentityServer.Infraestructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class OpenIddictNativeArrayColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "OpenIddictTokens",
                type: "character varying(100)",
                nullable: true);

            migrationBuilder.Sql("""
                -- OpenIddict < 8 stored these string[] properties as JSON text. OpenIddict 8 maps
                -- them as native EF Core primitive collections, i.e. PostgreSQL arrays. Convert the
                -- existing JSON (["a","b"]) into a PostgreSQL array literal ({a,b}) via USING, since
                -- a plain ALTER COLUMN ... TYPE text[] has no assignment cast from text.
                ALTER TABLE "OpenIddictScopes"
                    ALTER COLUMN "Resources" TYPE text[]
                    USING CASE WHEN "Resources" IS NULL THEN NULL
                               WHEN translate("Resources", '[]" ', '') = '' THEN '{}'::text[]
                               ELSE string_to_array(translate("Resources", '[]" ', ''), ',') END;

                ALTER TABLE "OpenIddictAuthorizations"
                    ALTER COLUMN "Scopes" TYPE text[]
                    USING CASE WHEN "Scopes" IS NULL THEN NULL
                               WHEN translate("Scopes", '[]" ', '') = '' THEN '{}'::text[]
                               ELSE string_to_array(translate("Scopes", '[]" ', ''), ',') END;

                ALTER TABLE "OpenIddictApplications"
                    ALTER COLUMN "Requirements" TYPE text[]
                    USING CASE WHEN "Requirements" IS NULL THEN NULL
                               WHEN translate("Requirements", '[]" ', '') = '' THEN '{}'::text[]
                               ELSE string_to_array(translate("Requirements", '[]" ', ''), ',') END;

                ALTER TABLE "OpenIddictApplications"
                    ALTER COLUMN "RedirectUris" TYPE text[]
                    USING CASE WHEN "RedirectUris" IS NULL THEN NULL
                               WHEN translate("RedirectUris", '[]" ', '') = '' THEN '{}'::text[]
                               ELSE string_to_array(translate("RedirectUris", '[]" ', ''), ',') END;

                ALTER TABLE "OpenIddictApplications"
                    ALTER COLUMN "PostLogoutRedirectUris" TYPE text[]
                    USING CASE WHEN "PostLogoutRedirectUris" IS NULL THEN NULL
                               WHEN translate("PostLogoutRedirectUris", '[]" ', '') = '' THEN '{}'::text[]
                               ELSE string_to_array(translate("PostLogoutRedirectUris", '[]" ', ''), ',') END;

                ALTER TABLE "OpenIddictApplications"
                    ALTER COLUMN "Permissions" TYPE text[]
                    USING CASE WHEN "Permissions" IS NULL THEN NULL
                               WHEN translate("Permissions", '[]" ', '') = '' THEN '{}'::text[]
                               ELSE string_to_array(translate("Permissions", '[]" ', ''), ',') END;
                """);

            migrationBuilder.CreateTable(
                name: "OpenIddictResources",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Descriptions = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    DisplayNames = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApplicationId = table.Column<string>(type: "character varying(100)", nullable: true),
                    AuthorizationId = table.Column<string>(type: "character varying(100)", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LoginId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictSessions_OpenIddictApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpenIddictSessions_OpenIddictAuthorizations_AuthorizationId",
                        column: x => x.AuthorizationId,
                        principalTable: "OpenIddictAuthorizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_SessionId",
                table: "OpenIddictTokens",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictResources_Name",
                table: "OpenIddictResources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictSessions_ApplicationId",
                table: "OpenIddictSessions",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictSessions_AuthorizationId",
                table: "OpenIddictSessions",
                column: "AuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictSessions_LoginId",
                table: "OpenIddictSessions",
                column: "LoginId");

            migrationBuilder.AddForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictSessions_SessionId",
                table: "OpenIddictTokens",
                column: "SessionId",
                principalTable: "OpenIddictSessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpenIddictTokens_OpenIddictSessions_SessionId",
                table: "OpenIddictTokens");

            migrationBuilder.DropTable(
                name: "OpenIddictResources");

            migrationBuilder.DropTable(
                name: "OpenIddictSessions");

            migrationBuilder.DropIndex(
                name: "IX_OpenIddictTokens_SessionId",
                table: "OpenIddictTokens");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "OpenIddictTokens");

            migrationBuilder.Sql("""
                ALTER TABLE "OpenIddictScopes"
                    ALTER COLUMN "Resources" TYPE text
                    USING CASE WHEN "Resources" IS NULL THEN NULL ELSE to_json("Resources")::text END;

                ALTER TABLE "OpenIddictAuthorizations"
                    ALTER COLUMN "Scopes" TYPE text
                    USING CASE WHEN "Scopes" IS NULL THEN NULL ELSE to_json("Scopes")::text END;

                ALTER TABLE "OpenIddictApplications"
                    ALTER COLUMN "Requirements" TYPE text
                    USING CASE WHEN "Requirements" IS NULL THEN NULL ELSE to_json("Requirements")::text END;

                ALTER TABLE "OpenIddictApplications"
                    ALTER COLUMN "RedirectUris" TYPE text
                    USING CASE WHEN "RedirectUris" IS NULL THEN NULL ELSE to_json("RedirectUris")::text END;

                ALTER TABLE "OpenIddictApplications"
                    ALTER COLUMN "PostLogoutRedirectUris" TYPE text
                    USING CASE WHEN "PostLogoutRedirectUris" IS NULL THEN NULL ELSE to_json("PostLogoutRedirectUris")::text END;

                ALTER TABLE "OpenIddictApplications"
                    ALTER COLUMN "Permissions" TYPE text
                    USING CASE WHEN "Permissions" IS NULL THEN NULL ELSE to_json("Permissions")::text END;
                """);
        }
    }
}
