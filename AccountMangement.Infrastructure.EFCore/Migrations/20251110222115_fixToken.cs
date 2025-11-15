using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountManagement.Infrastructure.EFCore.Migrations
{
    public partial class fixToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tokens_AccessToken",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_RefreshToken",
                table: "Tokens");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "Tokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "Tokens",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "Tokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AccessToken",
                table: "Tokens",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_AccessToken",
                table: "Tokens",
                column: "AccessToken");

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_RefreshToken",
                table: "Tokens",
                column: "RefreshToken",
                unique: true);
        }
    }
}
