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

            // Invite links were never issued before; clear any stray values so the new unique index can be built.
            migrationBuilder.Sql(@"UPDATE chat.groups SET ""InviteLink"" = NULL;");

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
                comment: "1 = GroupCreated, 2 = MembersAdded, 3 = MemberRemoved, 4 = MemberLeft, 5 = TitleChanged, 6 = PhotoChanged, 7 = MemberJoinedViaOrganization, 8 = MemberJoinedViaInvite",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "1 = GroupCreated, 2 = MembersAdded, 3 = MemberRemoved, 4 = MemberLeft, 5 = TitleChanged, 6 = PhotoChanged");

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
                    Domain = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.Id);
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
                name: "IX_groups_InviteLink",
                schema: "chat",
                table: "groups",
                column: "InviteLink",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_groups_OrganizationId",
                schema: "chat",
                table: "groups",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_members_ChatId_UserId",
                schema: "chat",
                table: "chat_members",
                columns: new[] { "ChatId", "UserId" },
                unique: true);

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
                name: "IX_organizations_Domain",
                schema: "organizations",
                table: "organizations",
                column: "Domain",
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
                name: "organization_members",
                schema: "organizations");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "organizations");

            migrationBuilder.DropIndex(
                name: "IX_groups_InviteLink",
                schema: "chat",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "IX_groups_OrganizationId",
                schema: "chat",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "IX_chat_members_ChatId_UserId",
                schema: "chat",
                table: "chat_members");

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
                oldComment: "1 = GroupCreated, 2 = MembersAdded, 3 = MemberRemoved, 4 = MemberLeft, 5 = TitleChanged, 6 = PhotoChanged, 7 = MemberJoinedViaOrganization, 8 = MemberJoinedViaInvite");

            migrationBuilder.CreateIndex(
                name: "IX_chat_members_ChatId",
                schema: "chat",
                table: "chat_members",
                column: "ChatId");
        }
    }
}
