using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebChatApplication.Migrations
{
    /// <inheritdoc />
    public partial class ProfilePictureIsNullableAndAdminChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("af3aa042-cf34-41f8-877a-43c77b9bf83a"));

            migrationBuilder.AlterColumn<string>(
                name: "profile_picture_file_name",
                table: "users",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldDefaultValue: "user_default_pfp.png");

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "created_at", "email", "email_confirmation_token", "last_activity", "password_hash", "password_recovery_token", "profile_picture_file_name", "role_id", "status", "username" },
                values: new object[] { new Guid("df6b1476-c214-4103-90d0-6fae25e74bca"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@gmail.com", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "bxHPtLKhH1H2j+eNQCNxzR6hMCXByycYrF/fx3Oo/4Y=", null, null, 3, 1, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("df6b1476-c214-4103-90d0-6fae25e74bca"));

            migrationBuilder.AlterColumn<string>(
                name: "profile_picture_file_name",
                table: "users",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "user_default_pfp.png",
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "created_at", "email", "email_confirmation_token", "last_activity", "password_hash", "password_recovery_token", "role_id", "status", "username" },
                values: new object[] { new Guid("af3aa042-cf34-41f8-877a-43c77b9bf83a"), new DateTime(2025, 4, 5, 18, 23, 19, 291, DateTimeKind.Utc).AddTicks(2609), "admin@gmail.com", null, new DateTime(2025, 4, 5, 18, 23, 19, 291, DateTimeKind.Utc).AddTicks(3073), "Ugc4Bt8RYVH6mgiQK/dsOgz2d7exWjiqrVWFZkBEb6A=", null, 3, 1, "admin" });
        }
    }
}
