using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBC.Migrations
{
    /// <inheritdoc />
    public partial class AddedJsonSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "BotInstances",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "BotInstances");
        }
    }
}
