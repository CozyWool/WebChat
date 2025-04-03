using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebChatApplication.Migrations
{
    /// <inheritdoc />
    public partial class PasswordRecoveryAndEmailConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email_confirmation_token",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password_recovery_token",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "created_at", "email", "email_confirmation_token", "last_activity", "password_hash", "password_recovery_token", "role_id", "status", "username" },
                values: new object[] { new Guid("3d1813da-ab04-4045-97ed-e43f1c4fd51f"), new DateTime(2025, 4, 3, 11, 23, 19, 838, DateTimeKind.Utc).AddTicks(3711), "admin@gmail.com", null, new DateTime(2025, 4, 3, 11, 23, 19, 838, DateTimeKind.Utc).AddTicks(4604), "W/6/+1bJthrexPell8vPZFOcVxXzuZzWsNnZdXUAu0E=", null, 3, 1, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("3d1813da-ab04-4045-97ed-e43f1c4fd51f"));

            migrationBuilder.DropColumn(
                name: "email_confirmation_token",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_recovery_token",
                table: "users");
        }
    }
}
