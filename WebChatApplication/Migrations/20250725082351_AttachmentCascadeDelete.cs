using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebChatApplication.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attachments_messages_message_id",
                table: "attachments");

            migrationBuilder.AddForeignKey(
                name: "FK_attachments_messages_message_id",
                table: "attachments",
                column: "message_id",
                principalTable: "messages",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_attachments_messages_message_id",
                table: "attachments");

            migrationBuilder.AddForeignKey(
                name: "FK_attachments_messages_message_id",
                table: "attachments",
                column: "message_id",
                principalTable: "messages",
                principalColumn: "id");
        }
    }
}
