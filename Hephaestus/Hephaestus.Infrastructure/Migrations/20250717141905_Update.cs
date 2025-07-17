using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hephaestus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_promotion_usages_tenant_id",
                table: "promotion_usages");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "promotion_usages");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "menu_item_images");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "menu_item_additionals");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "tags",
                newName: "company_id");

            migrationBuilder.RenameIndex(
                name: "IX_tags_tenant_id",
                table: "tags",
                newName: "IX_tags_company_id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "promotions",
                newName: "company_id");

            migrationBuilder.RenameIndex(
                name: "IX_promotions_tenant_id",
                table: "promotions",
                newName: "IX_promotions_company_id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "menu_items",
                newName: "company_id");

            migrationBuilder.RenameIndex(
                name: "IX_menu_items_tenant_id",
                table: "menu_items",
                newName: "IX_menu_items_company_id");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "menu_item_tags",
                newName: "CompanyId");

            migrationBuilder.AddColumn<string>(
                name: "company_id",
                table: "promotion_usages",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "menu_item_tags",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_id",
                table: "menu_item_images",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "menu_item_additionals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_id",
                table: "menu_item_additionals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_promotion_usages_company_id",
                table: "promotion_usages",
                column: "company_id");

            migrationBuilder.AddForeignKey(
                name: "FK_menu_items_companies_company_id",
                table: "menu_items",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_menu_items_companies_company_id",
                table: "menu_items");

            migrationBuilder.DropIndex(
                name: "IX_promotion_usages_company_id",
                table: "promotion_usages");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "promotion_usages");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "menu_item_tags");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "menu_item_images");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "menu_item_additionals");

            migrationBuilder.DropColumn(
                name: "company_id",
                table: "menu_item_additionals");

            migrationBuilder.RenameColumn(
                name: "company_id",
                table: "tags",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_tags_company_id",
                table: "tags",
                newName: "IX_tags_tenant_id");

            migrationBuilder.RenameColumn(
                name: "company_id",
                table: "promotions",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_promotions_company_id",
                table: "promotions",
                newName: "IX_promotions_tenant_id");

            migrationBuilder.RenameColumn(
                name: "company_id",
                table: "menu_items",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "IX_menu_items_company_id",
                table: "menu_items",
                newName: "IX_menu_items_tenant_id");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "menu_item_tags",
                newName: "TenantId");

            migrationBuilder.AddColumn<string>(
                name: "tenant_id",
                table: "promotion_usages",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tenant_id",
                table: "menu_item_images",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "tenant_id",
                table: "menu_item_additionals",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_promotion_usages_tenant_id",
                table: "promotion_usages",
                column: "tenant_id");
        }
    }
}
