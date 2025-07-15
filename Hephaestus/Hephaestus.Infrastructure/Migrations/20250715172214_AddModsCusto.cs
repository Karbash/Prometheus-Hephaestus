using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hephaestus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModsCusto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Customizations",
                table: "OrderItems");

            migrationBuilder.CreateTable(
                name: "Customization",
                columns: table => new
                {
                    OrderItemId = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customization", x => new { x.OrderItemId, x.Id });
                    table.ForeignKey(
                        name: "FK_Customization_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customization");

            migrationBuilder.AddColumn<string>(
                name: "Customizations",
                table: "OrderItems",
                type: "text",
                nullable: true);
        }
    }
}
