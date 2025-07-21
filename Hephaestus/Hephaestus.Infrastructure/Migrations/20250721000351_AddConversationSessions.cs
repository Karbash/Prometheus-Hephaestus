using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hephaestus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conversation_sessions",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    session_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    last_intent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    conversation_step = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_activity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    session_data_json = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "conversation_messages",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    session_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    intent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    response = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_openai = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_conversation_messages_conversation_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "conversation_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_conversation_messages_session_id",
                table: "conversation_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_messages_timestamp",
                table: "conversation_messages",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_messages_used_openai",
                table: "conversation_messages",
                column: "used_openai");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_sessions_is_active",
                table: "conversation_sessions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_sessions_last_activity",
                table: "conversation_sessions",
                column: "last_activity");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_sessions_phone_number",
                table: "conversation_sessions",
                column: "phone_number");

            migrationBuilder.CreateIndex(
                name: "IX_conversation_sessions_session_id",
                table: "conversation_sessions",
                column: "session_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "conversation_messages");

            migrationBuilder.DropTable(
                name: "conversation_sessions");
        }
    }
}
