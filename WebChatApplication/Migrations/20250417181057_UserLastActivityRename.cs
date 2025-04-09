using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebChatApplication.Migrations
{
    /// <inheritdoc />
    public partial class UserLastActivityRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "last_activity",
                table: "users",
                newName: "last_activity_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "last_activity_at",
                table: "users",
                newName: "last_activity");
        }
    }
}
