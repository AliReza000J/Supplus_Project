using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopManagement.Infrastructure.EFCore.Migrations
{
    public partial class BrandAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Products",
                type: "nvarchar(65)",
                maxLength: 65,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Products",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Products",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(65)",
                oldMaxLength: 65);
        }
    }
}
