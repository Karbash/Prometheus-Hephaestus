using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hephaestus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAddressIdAndOrderItemLegacyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "AdditionalIds",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "address_id",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "address_id",
                table: "companies");

            migrationBuilder.CreateTable(
                name: "orderitemadditional",
                columns: table => new
                {
                    orderitemid = table.Column<string>(type: "character varying(36)", nullable: false),
                    additionalid = table.Column<string>(type: "character varying(36)", nullable: false),
                    tenantid = table.Column<string>(type: "text", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderitemadditional", x => new { x.orderitemid, x.additionalid });
                    table.ForeignKey(
                        name: "FK_orderitemadditional_additionals_additionalid",
                        column: x => x.additionalid,
                        principalTable: "additionals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_orderitemadditional_order_items_orderitemid",
                        column: x => x.orderitemid,
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orderitemtag",
                columns: table => new
                {
                    orderitemid = table.Column<string>(type: "character varying(36)", nullable: false),
                    tagid = table.Column<string>(type: "character varying(36)", nullable: false),
                    tenantid = table.Column<string>(type: "text", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderitemtag", x => new { x.orderitemid, x.tagid });
                    table.ForeignKey(
                        name: "FK_orderitemtag_order_items_orderitemid",
                        column: x => x.orderitemid,
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_orderitemtag_tags_tagid",
                        column: x => x.tagid,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_orderitemadditional_additionalid",
                table: "orderitemadditional",
                column: "additionalid");

            migrationBuilder.CreateIndex(
                name: "IX_orderitemtag_tagid",
                table: "orderitemtag",
                column: "tagid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "orderitemadditional");

            migrationBuilder.DropTable(
                name: "orderitemtag");

            migrationBuilder.AddColumn<string>(
                name: "address_id",
                table: "orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalIds",
                table: "order_items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "order_items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "address_id",
                table: "customers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "address_id",
                table: "companies",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
