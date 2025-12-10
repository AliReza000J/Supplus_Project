using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopManagement.Infrastructure.EFCore.Migrations
{
    public partial class IdsUnifiedToLONG : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ProductId",
                table: "UserFavoriteProducts",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "ProductId",
                table: "ProductViews",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "UserFavoriteProducts",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ProductViews",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
