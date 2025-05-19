using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tbc.Migrations
{
    /// <inheritdoc />
    public partial class AddChatIdBotMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "BotMessages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "BotMessages");
        }
    }
}
