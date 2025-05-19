using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tbc.Migrations
{
    /// <inheritdoc />
    public partial class AddNodeTypeAndPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BotMessages_BotId",
                table: "BotMessages");

            migrationBuilder.AddColumn<string>(
                name: "NodeType",
                table: "BotMessages",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Payload",
                table: "BotMessages",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BotMessages_BotId_NodeType",
                table: "BotMessages",
                columns: new[] { "BotId", "NodeType" });

            migrationBuilder.CreateIndex(
                name: "IX_BotMessages_Timestamp",
                table: "BotMessages",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BotMessages_BotId_NodeType",
                table: "BotMessages");

            migrationBuilder.DropIndex(
                name: "IX_BotMessages_Timestamp",
                table: "BotMessages");

            migrationBuilder.DropColumn(
                name: "NodeType",
                table: "BotMessages");

            migrationBuilder.DropColumn(
                name: "Payload",
                table: "BotMessages");

            migrationBuilder.CreateIndex(
                name: "IX_BotMessages_BotId",
                table: "BotMessages",
                column: "BotId");
        }
    }
}
