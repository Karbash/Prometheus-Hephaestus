using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hephaestus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixConversationSessionForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_conversation_messages_conversation_sessions_session_id",
                table: "conversation_messages");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_conversation_sessions_session_id",
                table: "conversation_sessions",
                column: "session_id");

            migrationBuilder.AddForeignKey(
                name: "FK_conversation_messages_conversation_sessions_session_id",
                table: "conversation_messages",
                column: "session_id",
                principalTable: "conversation_sessions",
                principalColumn: "session_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_conversation_messages_conversation_sessions_session_id",
                table: "conversation_messages");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_conversation_sessions_session_id",
                table: "conversation_sessions");

            migrationBuilder.AddForeignKey(
                name: "FK_conversation_messages_conversation_sessions_session_id",
                table: "conversation_messages",
                column: "session_id",
                principalTable: "conversation_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
