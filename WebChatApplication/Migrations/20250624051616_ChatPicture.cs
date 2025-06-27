using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebChatApplication.Migrations
{
    /// <inheritdoc />
    public partial class ChatPicture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(75)",
                oldMaxLength: 75);

            migrationBuilder.AddColumn<string>(
                name: "chat_picture_file_name",
                table: "chats",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "chat_picture_file_name",
                table: "chats");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "users",
                type: "character varying(75)",
                maxLength: 75,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);
        }
    }
}
