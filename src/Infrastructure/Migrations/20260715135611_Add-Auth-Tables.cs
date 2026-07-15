using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_sessions_UserId",
                schema: "security",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "identity",
                table: "users");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "identity",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "identity",
                table: "users",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileCompleted",
                schema: "identity",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                schema: "security",
                table: "sessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActiveAt",
                schema: "security",
                table: "sessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenHash",
                schema: "security",
                table: "sessions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                schema: "security",
                table: "sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                schema: "security",
                table: "sessions",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "email_verification_codes",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Purpose = table.Column<int>(type: "integer", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestIpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_verification_codes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                schema: "identity",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                schema: "identity",
                table: "users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessions_RefreshTokenHash",
                schema: "security",
                table: "sessions",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sessions_UserId_RevokedAt_ExpiresAt",
                schema: "security",
                table: "sessions",
                columns: new[] { "UserId", "RevokedAt", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_email_verification_codes_Email_Purpose_ExpiresAt",
                schema: "security",
                table: "email_verification_codes",
                columns: new[] { "Email", "Purpose", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_verification_codes",
                schema: "security");

            migrationBuilder.DropIndex(
                name: "IX_users_Email",
                schema: "identity",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_Username",
                schema: "identity",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_sessions_RefreshTokenHash",
                schema: "security",
                table: "sessions");

            migrationBuilder.DropIndex(
                name: "IX_sessions_UserId_RevokedAt_ExpiresAt",
                schema: "security",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsProfileCompleted",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                schema: "security",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "LastActiveAt",
                schema: "security",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "RefreshTokenHash",
                schema: "security",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                schema: "security",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                schema: "security",
                table: "sessions");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "identity",
                table: "users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_UserId",
                schema: "security",
                table: "sessions",
                column: "UserId");
        }
    }
}
