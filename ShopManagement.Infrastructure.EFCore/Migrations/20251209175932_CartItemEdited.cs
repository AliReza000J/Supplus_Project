using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopManagement.Infrastructure.EFCore.Migrations
{
    public partial class CartItemEdited : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "Picture",
                table: "CartItems",
                newName: "CartKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CartKey",
                table: "CartItems",
                newName: "Picture");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "CartItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
