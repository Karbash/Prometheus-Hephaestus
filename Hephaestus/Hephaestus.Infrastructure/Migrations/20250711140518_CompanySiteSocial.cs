using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hephaestus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompanySiteSocial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyImages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ImageType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyImages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyOperatingHours",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    DayOfWeek = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OpenTime = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    CloseTime = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyOperatingHours", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanySocialMedia",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CompanyId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Platform = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySocialMedia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItemImages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    MenuItemId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItemImages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyImages_CompanyId_ImageType",
                table: "CompanyImages",
                columns: new[] { "CompanyId", "ImageType" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyOperatingHours_CompanyId_DayOfWeek",
                table: "CompanyOperatingHours",
                columns: new[] { "CompanyId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySocialMedia_CompanyId_Platform",
                table: "CompanySocialMedia",
                columns: new[] { "CompanyId", "Platform" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemImages_MenuItemId",
                table: "MenuItemImages",
                column: "MenuItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyImages");

            migrationBuilder.DropTable(
                name: "CompanyOperatingHours");

            migrationBuilder.DropTable(
                name: "CompanySocialMedia");

            migrationBuilder.DropTable(
                name: "MenuItemImages");
        }
    }
}
