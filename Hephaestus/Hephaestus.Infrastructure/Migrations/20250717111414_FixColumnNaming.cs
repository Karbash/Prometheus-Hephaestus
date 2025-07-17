using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hephaestus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixColumnNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyOperatingHours_companies_CompanyId",
                table: "CompanyOperatingHours");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanySocialMedia_companies_CompanyId",
                table: "CompanySocialMedia");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionQueues_orders_OrderId",
                table: "ProductionQueues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductionQueues",
                table: "ProductionQueues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MenuItemImages",
                table: "MenuItemImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanySocialMedia",
                table: "CompanySocialMedia");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyOperatingHours",
                table: "CompanyOperatingHours");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyImages",
                table: "CompanyImages");

            migrationBuilder.RenameTable(
                name: "Addresses",
                newName: "addresses");

            migrationBuilder.RenameTable(
                name: "ProductionQueues",
                newName: "production_queues");

            migrationBuilder.RenameTable(
                name: "MenuItemImages",
                newName: "menu_item_images");

            migrationBuilder.RenameTable(
                name: "CompanySocialMedia",
                newName: "company_social_media");

            migrationBuilder.RenameTable(
                name: "CompanyOperatingHours",
                newName: "company_operating_hours");

            migrationBuilder.RenameTable(
                name: "CompanyImages",
                newName: "company_images");

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "password_reset_tokens",
                newName: "token");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "password_reset_tokens",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "password_reset_tokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "password_reset_tokens",
                newName: "expires_at");

            migrationBuilder.RenameIndex(
                name: "IX_password_reset_tokens_Email_Token",
                table: "password_reset_tokens",
                newName: "IX_password_reset_tokens_email_token");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "addresses",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "addresses",
                newName: "street");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "addresses",
                newName: "state");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "addresses",
                newName: "number");

            migrationBuilder.RenameColumn(
                name: "Neighborhood",
                table: "addresses",
                newName: "neighborhood");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "addresses",
                newName: "longitude");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "addresses",
                newName: "latitude");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "addresses",
                newName: "country");

            migrationBuilder.RenameColumn(
                name: "Complement",
                table: "addresses",
                newName: "complement");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "addresses",
                newName: "city");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "addresses",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ZipCode",
                table: "addresses",
                newName: "zip_code");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "addresses",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "addresses",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "EntityType",
                table: "addresses",
                newName: "entity_type");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "addresses",
                newName: "entity_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "addresses",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionQueues_OrderId",
                table: "production_queues",
                newName: "IX_production_queues_OrderId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "menu_item_images",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "menu_item_images",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "menu_item_images",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "MenuItemId",
                table: "menu_item_images",
                newName: "menu_item_id");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "menu_item_images",
                newName: "image_url");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "menu_item_images",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "company_social_media",
                newName: "url");

            migrationBuilder.RenameColumn(
                name: "Platform",
                table: "company_social_media",
                newName: "platform");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "company_social_media",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "company_social_media",
                newName: "company_id");

            migrationBuilder.RenameIndex(
                name: "IX_CompanySocialMedia_CompanyId_Platform",
                table: "company_social_media",
                newName: "IX_company_social_media_company_id_platform");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "company_operating_hours",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "OpenTime",
                table: "company_operating_hours",
                newName: "open_time");

            migrationBuilder.RenameColumn(
                name: "IsClosed",
                table: "company_operating_hours",
                newName: "is_closed");

            migrationBuilder.RenameColumn(
                name: "DayOfWeek",
                table: "company_operating_hours",
                newName: "day_of_week");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "company_operating_hours",
                newName: "company_id");

            migrationBuilder.RenameColumn(
                name: "CloseTime",
                table: "company_operating_hours",
                newName: "close_time");

            migrationBuilder.RenameIndex(
                name: "IX_CompanyOperatingHours_CompanyId_DayOfWeek",
                table: "company_operating_hours",
                newName: "IX_company_operating_hours_company_id_day_of_week");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "company_images",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "company_images",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TenantId",
                table: "company_images",
                newName: "tenant_id");

            migrationBuilder.RenameColumn(
                name: "IsMain",
                table: "company_images",
                newName: "is_main");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "company_images",
                newName: "image_url");

            migrationBuilder.RenameColumn(
                name: "ImageType",
                table: "company_images",
                newName: "image_type");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "company_images",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CompanyId",
                table: "company_images",
                newName: "company_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_addresses",
                table: "addresses",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_production_queues",
                table: "production_queues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_menu_item_images",
                table: "menu_item_images",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_company_social_media",
                table: "company_social_media",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_company_operating_hours",
                table: "company_operating_hours",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_company_images",
                table: "company_images",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_company_operating_hours_companies_company_id",
                table: "company_operating_hours",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_company_social_media_companies_company_id",
                table: "company_social_media",
                column: "company_id",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_production_queues_orders_OrderId",
                table: "production_queues",
                column: "OrderId",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_company_operating_hours_companies_company_id",
                table: "company_operating_hours");

            migrationBuilder.DropForeignKey(
                name: "FK_company_social_media_companies_company_id",
                table: "company_social_media");

            migrationBuilder.DropForeignKey(
                name: "FK_production_queues_orders_OrderId",
                table: "production_queues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_addresses",
                table: "addresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_production_queues",
                table: "production_queues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_menu_item_images",
                table: "menu_item_images");

            migrationBuilder.DropPrimaryKey(
                name: "PK_company_social_media",
                table: "company_social_media");

            migrationBuilder.DropPrimaryKey(
                name: "PK_company_operating_hours",
                table: "company_operating_hours");

            migrationBuilder.DropPrimaryKey(
                name: "PK_company_images",
                table: "company_images");

            migrationBuilder.RenameTable(
                name: "addresses",
                newName: "Addresses");

            migrationBuilder.RenameTable(
                name: "production_queues",
                newName: "ProductionQueues");

            migrationBuilder.RenameTable(
                name: "menu_item_images",
                newName: "MenuItemImages");

            migrationBuilder.RenameTable(
                name: "company_social_media",
                newName: "CompanySocialMedia");

            migrationBuilder.RenameTable(
                name: "company_operating_hours",
                newName: "CompanyOperatingHours");

            migrationBuilder.RenameTable(
                name: "company_images",
                newName: "CompanyImages");

            migrationBuilder.RenameColumn(
                name: "token",
                table: "password_reset_tokens",
                newName: "Token");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "password_reset_tokens",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "password_reset_tokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "password_reset_tokens",
                newName: "ExpiresAt");

            migrationBuilder.RenameIndex(
                name: "IX_password_reset_tokens_email_token",
                table: "password_reset_tokens",
                newName: "IX_password_reset_tokens_Email_Token");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "Addresses",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "street",
                table: "Addresses",
                newName: "Street");

            migrationBuilder.RenameColumn(
                name: "state",
                table: "Addresses",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "number",
                table: "Addresses",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "neighborhood",
                table: "Addresses",
                newName: "Neighborhood");

            migrationBuilder.RenameColumn(
                name: "longitude",
                table: "Addresses",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "latitude",
                table: "Addresses",
                newName: "Latitude");

            migrationBuilder.RenameColumn(
                name: "country",
                table: "Addresses",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "complement",
                table: "Addresses",
                newName: "Complement");

            migrationBuilder.RenameColumn(
                name: "city",
                table: "Addresses",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Addresses",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "zip_code",
                table: "Addresses",
                newName: "ZipCode");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Addresses",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "Addresses",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "entity_type",
                table: "Addresses",
                newName: "EntityType");

            migrationBuilder.RenameColumn(
                name: "entity_id",
                table: "Addresses",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Addresses",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_production_queues_OrderId",
                table: "ProductionQueues",
                newName: "IX_ProductionQueues_OrderId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MenuItemImages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "MenuItemImages",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "MenuItemImages",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "menu_item_id",
                table: "MenuItemImages",
                newName: "MenuItemId");

            migrationBuilder.RenameColumn(
                name: "image_url",
                table: "MenuItemImages",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "MenuItemImages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "url",
                table: "CompanySocialMedia",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "platform",
                table: "CompanySocialMedia",
                newName: "Platform");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CompanySocialMedia",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "company_id",
                table: "CompanySocialMedia",
                newName: "CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_company_social_media_company_id_platform",
                table: "CompanySocialMedia",
                newName: "IX_CompanySocialMedia_CompanyId_Platform");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CompanyOperatingHours",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "open_time",
                table: "CompanyOperatingHours",
                newName: "OpenTime");

            migrationBuilder.RenameColumn(
                name: "is_closed",
                table: "CompanyOperatingHours",
                newName: "IsClosed");

            migrationBuilder.RenameColumn(
                name: "day_of_week",
                table: "CompanyOperatingHours",
                newName: "DayOfWeek");

            migrationBuilder.RenameColumn(
                name: "company_id",
                table: "CompanyOperatingHours",
                newName: "CompanyId");

            migrationBuilder.RenameColumn(
                name: "close_time",
                table: "CompanyOperatingHours",
                newName: "CloseTime");

            migrationBuilder.RenameIndex(
                name: "IX_company_operating_hours_company_id_day_of_week",
                table: "CompanyOperatingHours",
                newName: "IX_CompanyOperatingHours_CompanyId_DayOfWeek");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CompanyImages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "CompanyImages",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                table: "CompanyImages",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "is_main",
                table: "CompanyImages",
                newName: "IsMain");

            migrationBuilder.RenameColumn(
                name: "image_url",
                table: "CompanyImages",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "image_type",
                table: "CompanyImages",
                newName: "ImageType");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "CompanyImages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "company_id",
                table: "CompanyImages",
                newName: "CompanyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductionQueues",
                table: "ProductionQueues",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MenuItemImages",
                table: "MenuItemImages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanySocialMedia",
                table: "CompanySocialMedia",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyOperatingHours",
                table: "CompanyOperatingHours",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyImages",
                table: "CompanyImages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyOperatingHours_companies_CompanyId",
                table: "CompanyOperatingHours",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySocialMedia_companies_CompanyId",
                table: "CompanySocialMedia",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionQueues_orders_OrderId",
                table: "ProductionQueues",
                column: "OrderId",
                principalTable: "orders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
