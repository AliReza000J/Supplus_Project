using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogManagement.Infrastructure.EFCore.Migrations
{
    public partial class ViewCountAddedToArticles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ArticleViews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticleId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleViews", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleViews");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Articles");
        }
    }
}
