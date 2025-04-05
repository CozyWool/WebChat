using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebChatApplication.Migrations
{
    /// <inheritdoc />
    public partial class ProfilePictureAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("3d1813da-ab04-4045-97ed-e43f1c4fd51f"));

            migrationBuilder.AddColumn<string>(
                name: "profile_picture_file_name",
                table: "users",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "user_default_pfp.png");

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "created_at", "email", "email_confirmation_token", "last_activity", "password_hash", "password_recovery_token", "role_id", "status", "username" },
                values: new object[] { new Guid("af3aa042-cf34-41f8-877a-43c77b9bf83a"), new DateTime(2025, 4, 5, 18, 23, 19, 291, DateTimeKind.Utc).AddTicks(2609), "admin@gmail.com", null, new DateTime(2025, 4, 5, 18, 23, 19, 291, DateTimeKind.Utc).AddTicks(3073), "Ugc4Bt8RYVH6mgiQK/dsOgz2d7exWjiqrVWFZkBEb6A=", null, 3, 1, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("af3aa042-cf34-41f8-877a-43c77b9bf83a"));

            migrationBuilder.DropColumn(
                name: "profile_picture_file_name",
                table: "users");

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "created_at", "email", "email_confirmation_token", "last_activity", "password_hash", "password_recovery_token", "role_id", "status", "username" },
                values: new object[] { new Guid("3d1813da-ab04-4045-97ed-e43f1c4fd51f"), new DateTime(2025, 4, 3, 11, 23, 19, 838, DateTimeKind.Utc).AddTicks(3711), "admin@gmail.com", null, new DateTime(2025, 4, 3, 11, 23, 19, 838, DateTimeKind.Utc).AddTicks(4604), "W/6/+1bJthrexPell8vPZFOcVxXzuZzWsNnZdXUAu0E=", null, 3, 1, "admin" });
        }
    }
}
