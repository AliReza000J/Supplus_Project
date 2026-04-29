using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscountManagement.Infrastructure.EFCore.Migrations
{
    public partial class FullDiscountAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColleagueDiscounts");

            migrationBuilder.DropTable(
                name: "CustomerDiscounts");

            migrationBuilder.DropTable(
                name: "DiscountCodeUsers");

            migrationBuilder.DropTable(
                name: "DiscountCodeUsages");

            migrationBuilder.DropTable(
                name: "DiscountCodes");

            migrationBuilder.CreateTable(
                name: "ColleagueDiscounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    DiscountRate = table.Column<int>(type: "int", nullable: false),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColleagueDiscounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerDiscounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    DiscountRate = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductCount = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDiscounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CouponCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ApplyType = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<int>(type: "int", nullable: false),
                    MinOrderAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxDiscountQuantity = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MaxUsageCount = table.Column<int>(type: "int", nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    UsageLimitPerUser = table.Column<int>(type: "int", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscountTargets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: true),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    BrandName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DiscountId = table.Column<long>(type: "bigint", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountTargets_Discounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Discounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscountUserUsages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UsedCount = table.Column<int>(type: "int", nullable: false),
                    DiscountId = table.Column<long>(type: "bigint", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountUserUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountUserUsages_Discounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Discounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscountTargets_CategoryId",
                table: "DiscountTargets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountTargets_DiscountId",
                table: "DiscountTargets",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountTargets_ProductId",
                table: "DiscountTargets",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountUserUsages_DiscountId",
                table: "DiscountUserUsages",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountUserUsages_UserId",
                table: "DiscountUserUsages",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColleagueDiscounts");

            migrationBuilder.DropTable(
                name: "CustomerDiscounts");

            migrationBuilder.DropTable(
                name: "DiscountTargets");

            migrationBuilder.DropTable(
                name: "DiscountUserUsages");

            migrationBuilder.DropTable(
                name: "Discounts");
        }
    }
}
