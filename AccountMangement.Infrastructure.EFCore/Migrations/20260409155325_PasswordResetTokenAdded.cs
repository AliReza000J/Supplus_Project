using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountManagement.Infrastructure.EFCore.Migrations
{
    public partial class PasswordResetTokenAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsExpired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetTokens");
        }
    }
}
