using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopManagement.Infrastructure.EFCore.Migrations
{
    public partial class Updated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InventoryId",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "IssueTrackingNo",
                table: "Orders",
                newName: "TrackingNo");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PaymentRefId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountApplied",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Picture",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CartKey",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "Carts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Picture",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ProductViews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductViews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserFavoriteProducts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteProducts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteProducts_UserId_ProductId",
                table: "UserFavoriteProducts",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductViews");

            migrationBuilder.DropTable(
                name: "UserFavoriteProducts");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PaymentRefId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DiscountApplied",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Picture",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "CartKey",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "Picture",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "TrackingNo",
                table: "Orders",
                newName: "IssueTrackingNo");

            migrationBuilder.AddColumn<long>(
                name: "RefId",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "InventoryId",
                table: "OrderItems",
                type: "bigint",
                nullable: true);
        }
    }
}
