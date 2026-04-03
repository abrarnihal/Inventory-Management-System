using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace coderush.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationIsPinned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "ChatConversation",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "ChatConversation");
        }
    }
}
