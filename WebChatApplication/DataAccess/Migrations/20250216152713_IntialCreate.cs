using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebChatApplication.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class IntialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Chats_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Roles_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    username = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_banned = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_activity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Users_pkey", x => x.id);
                    table.ForeignKey(
                        name: "role_id_fk",
                        column: x => x.role_id,
                        principalTable: "Roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "BlacklistedUsers",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    friend_user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("BlacklistedUsers_pkey", x => new { x.user_id, x.friend_user_id });
                    table.ForeignKey(
                        name: "friend_user_id_fk",
                        column: x => x.friend_user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "user_id_fk",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    friend_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_accepted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Friends_pkey", x => new { x.user_id, x.friend_user_id });
                    table.ForeignKey(
                        name: "friend_user_id_fk",
                        column: x => x.friend_user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "user_id_fk",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chat_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sent_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Messages_pkey", x => x.id);
                    table.ForeignKey(
                        name: "FK_Messages_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "chat_id_fk",
                        column: x => x.chat_id,
                        principalTable: "Chats",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "UsersChats",
                columns: table => new
                {
                    ChatsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersChats", x => new { x.ChatsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UsersChats_Chats_ChatsId",
                        column: x => x.ChatsId,
                        principalTable: "Chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsersChats_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "User" },
                    { 2, "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlacklistedUsers_friend_user_id",
                table: "BlacklistedUsers",
                column: "friend_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_friend_user_id",
                table: "Friends",
                column: "friend_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_chat_id",
                table: "Messages",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_user_id",
                table: "Messages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_role_id",
                table: "Users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_UsersChats_UsersId",
                table: "UsersChats",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlacklistedUsers");

            migrationBuilder.DropTable(
                name: "Friends");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "UsersChats");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
