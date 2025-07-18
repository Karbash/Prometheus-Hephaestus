using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hephaestus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "tenant_id",
                table: "categories",
                type: "character varying(36)",
                maxLength: 36,
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(36)",
                oldMaxLength: 36);

            migrationBuilder.AddColumn<bool>(
                name: "is_global",
                table: "categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_global",
                table: "categories");

            migrationBuilder.AlterColumn<string>(
                name: "tenant_id",
                table: "categories",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(36)",
                oldMaxLength: 36,
                oldNullable: true,
                oldDefaultValue: "");
        }
    }
}
