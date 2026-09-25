using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecretChatsE2E : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Old rows kept their key on the server (not end-to-end) and have no device session to bind to.
            migrationBuilder.Sql("DELETE FROM chat.secret_chats;");

            migrationBuilder.DropIndex(
                name: "IX_secret_chats_ParticipantId",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropColumn(
                name: "EncryptionKey",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                schema: "chat",
                table: "secret_chats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                schema: "chat",
                table: "secret_chats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "InitiatorLastSeq",
                schema: "chat",
                table: "secret_chats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "InitiatorPublicKey",
                schema: "chat",
                table: "secret_chats",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "InitiatorSessionId",
                schema: "chat",
                table: "secret_chats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ParticipantLastSeq",
                schema: "chat",
                table: "secret_chats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "ParticipantPublicKey",
                schema: "chat",
                table: "secret_chats",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParticipantSessionId",
                schema: "chat",
                table: "secret_chats",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "chat",
                table: "secret_chats",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "1 = Pending, 2 = Active, 3 = Closed");

            migrationBuilder.CreateTable(
                name: "secret_messages",
                schema: "messaging",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SecretChatId = table.Column<int>(type: "integer", nullable: false),
                    SenderSessionId = table.Column<int>(type: "integer", nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false),
                    Ciphertext = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_secret_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_secret_messages_secret_chats_SecretChatId",
                        column: x => x.SecretChatId,
                        principalSchema: "chat",
                        principalTable: "secret_chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_secret_chats_InitiatorSessionId_Status",
                schema: "chat",
                table: "secret_chats",
                columns: new[] { "InitiatorSessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_secret_chats_ParticipantId_Status",
                schema: "chat",
                table: "secret_chats",
                columns: new[] { "ParticipantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_secret_chats_ParticipantSessionId_Status",
                schema: "chat",
                table: "secret_chats",
                columns: new[] { "ParticipantSessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_secret_messages_CreatedAt",
                schema: "messaging",
                table: "secret_messages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_secret_messages_SecretChatId_SenderSessionId_Seq",
                schema: "messaging",
                table: "secret_messages",
                columns: new[] { "SecretChatId", "SenderSessionId", "Seq" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_secret_chats_sessions_InitiatorSessionId",
                schema: "chat",
                table: "secret_chats",
                column: "InitiatorSessionId",
                principalSchema: "security",
                principalTable: "sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_secret_chats_sessions_ParticipantSessionId",
                schema: "chat",
                table: "secret_chats",
                column: "ParticipantSessionId",
                principalSchema: "security",
                principalTable: "sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_secret_chats_sessions_InitiatorSessionId",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropForeignKey(
                name: "FK_secret_chats_sessions_ParticipantSessionId",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropTable(
                name: "secret_messages",
                schema: "messaging");

            migrationBuilder.DropIndex(
                name: "IX_secret_chats_InitiatorSessionId_Status",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropIndex(
                name: "IX_secret_chats_ParticipantId_Status",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropIndex(
                name: "IX_secret_chats_ParticipantSessionId_Status",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropColumn(
                name: "InitiatorLastSeq",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropColumn(
                name: "InitiatorPublicKey",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropColumn(
                name: "InitiatorSessionId",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropColumn(
                name: "ParticipantLastSeq",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropColumn(
                name: "ParticipantPublicKey",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropColumn(
                name: "ParticipantSessionId",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "chat",
                table: "secret_chats");

            migrationBuilder.AddColumn<string>(
                name: "EncryptionKey",
                schema: "chat",
                table: "secret_chats",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_secret_chats_ParticipantId",
                schema: "chat",
                table: "secret_chats",
                column: "ParticipantId");
        }
    }
}
