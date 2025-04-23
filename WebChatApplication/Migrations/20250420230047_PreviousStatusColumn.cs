using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebChatApplication.Migrations
{
    /// <inheritdoc />
    public partial class PreviousStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "previous_status",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("df6b1476-c214-4103-90d0-6fae25e74bca"),
                column: "previous_status",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "previous_status",
                table: "users");
        }
    }
}
