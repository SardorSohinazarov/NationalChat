using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecretFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "secret_files",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SecretChatId = table.Column<int>(type: "integer", nullable: false),
                    UploaderSessionId = table.Column<int>(type: "integer", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_secret_files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_secret_files_secret_chats_SecretChatId",
                        column: x => x.SecretChatId,
                        principalSchema: "chat",
                        principalTable: "secret_chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_secret_files_CreatedAt",
                schema: "messaging",
                table: "secret_files",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_secret_files_SecretChatId",
                schema: "messaging",
                table: "secret_files",
                column: "SecretChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "secret_files",
                schema: "messaging");
        }
    }
}
