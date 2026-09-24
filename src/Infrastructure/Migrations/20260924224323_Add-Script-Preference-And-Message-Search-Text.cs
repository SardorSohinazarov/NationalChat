using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScriptPreferenceAndMessageSearchText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScriptPreference",
                schema: "identity",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                comment: "1 = Original, 2 = Latin, 3 = Cyrillic");

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                schema: "messaging",
                table: "messages",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScriptPreference",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "messaging",
                table: "messages");
        }
    }
}
