using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hephaestus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompanyUpdateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Companies",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Companies",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "Companies",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Companies",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_City",
                table: "Companies",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Neighborhood",
                table: "Companies",
                column: "Neighborhood");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_PhoneNumber",
                table: "Companies",
                column: "PhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_City",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Neighborhood",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_PhoneNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "Companies");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Companies",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Companies",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "Companies",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Companies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(36)",
                oldMaxLength: 36);
        }
    }
}
