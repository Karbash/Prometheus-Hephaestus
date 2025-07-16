using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hephaestus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditLogsUpdateByAll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdminId",
                table: "AuditLogs",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_AdminId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AuditLogs",
                newName: "AdminId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_AdminId");
        }
    }
}
