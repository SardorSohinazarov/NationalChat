using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The new unique (ChatId, UserId) index would fail on duplicate memberships; keep the oldest row.
            migrationBuilder.Sql(@"
                DELETE FROM chat.chat_members AS duplicate
                USING chat.chat_members AS original
                WHERE duplicate.""ChatId"" = original.""ChatId""
                  AND duplicate.""UserId"" = original.""UserId""
                  AND duplicate.""Id"" > original.""Id"";");

            migrationBuilder.DropIndex(
                name: "IX_chat_members_ChatId",
                schema: "chat",
                table: "chat_members");

            migrationBuilder.EnsureSchema(
                name: "organizations");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceAction",
                schema: "messaging",
                table: "messages",
                type: "integer",
                nullable: true,
                comment: "1 = GroupCreated, 2 = MembersAdded, 3 = MemberRemoved, 4 = MemberLeft, 5 = TitleChanged, 6 = PhotoChanged, 7 = MemberJoinedViaOrganization",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "1 = GroupCreated, 2 = MembersAdded, 3 = MemberRemoved, 4 = MemberLeft, 5 = TitleChanged, 6 = PhotoChanged");

            migrationBuilder.AddColumn<bool>(
                name: "AutoJoin",
                schema: "chat",
                table: "groups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                schema: "chat",
                table: "groups",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organization_domains",
                schema: "organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Domain = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_domains", x => x.Id);
                    table.ForeignKey(
                        name: "FK_organization_domains_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "organizations",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_members",
                schema: "organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false, comment: "1 = Member, 2 = Admin"),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_organization_members_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "organizations",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_members_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_groups_OrganizationId_AutoJoin",
                schema: "chat",
                table: "groups",
                columns: new[] { "OrganizationId", "AutoJoin" });

            migrationBuilder.CreateIndex(
                name: "IX_chat_members_ChatId_UserId",
                schema: "chat",
                table: "chat_members",
                columns: new[] { "ChatId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_domains_Domain",
                schema: "organizations",
                table: "organization_domains",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_domains_OrganizationId",
                schema: "organizations",
                table: "organization_domains",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_OrganizationId",
                schema: "organizations",
                table: "organization_members",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_UserId",
                schema: "organizations",
                table: "organization_members",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organizations_ShortName",
                schema: "organizations",
                table: "organizations",
                column: "ShortName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_groups_organizations_OrganizationId",
                schema: "chat",
                table: "groups",
                column: "OrganizationId",
                principalSchema: "organizations",
                principalTable: "organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_groups_organizations_OrganizationId",
                schema: "chat",
                table: "groups");

            migrationBuilder.DropTable(
                name: "organization_domains",
                schema: "organizations");

            migrationBuilder.DropTable(
                name: "organization_members",
                schema: "organizations");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "organizations");

            migrationBuilder.DropIndex(
                name: "IX_groups_OrganizationId_AutoJoin",
                schema: "chat",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "IX_chat_members_ChatId_UserId",
                schema: "chat",
                table: "chat_members");

            migrationBuilder.DropColumn(
                name: "AutoJoin",
                schema: "chat",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                schema: "chat",
                table: "groups");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceAction",
                schema: "messaging",
                table: "messages",
                type: "integer",
                nullable: true,
                comment: "1 = GroupCreated, 2 = MembersAdded, 3 = MemberRemoved, 4 = MemberLeft, 5 = TitleChanged, 6 = PhotoChanged",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "1 = GroupCreated, 2 = MembersAdded, 3 = MemberRemoved, 4 = MemberLeft, 5 = TitleChanged, 6 = PhotoChanged, 7 = MemberJoinedViaOrganization");

            migrationBuilder.CreateIndex(
                name: "IX_chat_members_ChatId",
                schema: "chat",
                table: "chat_members",
                column: "ChatId");
        }
    }
}
