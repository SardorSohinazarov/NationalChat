using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_groups_ChatId",
                schema: "chat",
                table: "groups");

            migrationBuilder.AddColumn<int>(
                name: "ServiceAction",
                schema: "messaging",
                table: "messages",
                type: "integer",
                nullable: true,
                comment: "1 = GroupCreated, 2 = MembersAdded, 3 = MemberRemoved, 4 = MemberLeft, 5 = TitleChanged, 6 = PhotoChanged");

            migrationBuilder.AddColumn<int>(
                name: "PhotoId",
                schema: "chat",
                table: "groups",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_groups_ChatId",
                schema: "chat",
                table: "groups",
                column: "ChatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_groups_PhotoId",
                schema: "chat",
                table: "groups",
                column: "PhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_groups_photos_PhotoId",
                schema: "chat",
                table: "groups",
                column: "PhotoId",
                principalSchema: "storage",
                principalTable: "photos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_groups_photos_PhotoId",
                schema: "chat",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "IX_groups_ChatId",
                schema: "chat",
                table: "groups");

            migrationBuilder.DropIndex(
                name: "IX_groups_PhotoId",
                schema: "chat",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "ServiceAction",
                schema: "messaging",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "PhotoId",
                schema: "chat",
                table: "groups");

            migrationBuilder.CreateIndex(
                name: "IX_groups_ChatId",
                schema: "chat",
                table: "groups",
                column: "ChatId");
        }
    }
}
