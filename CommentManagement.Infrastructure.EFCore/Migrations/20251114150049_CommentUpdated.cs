using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommentManagement.Infrastructure.EFCore.Migrations
{
    public partial class CommentUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentReactions");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    OwnerRecordId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<int>(type: "int", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    DislikeCount = table.Column<int>(type: "int", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    IsCanceled = table.Column<bool>(type: "bit", nullable: false),
                    IsBuyer = table.Column<bool>(type: "bit", nullable: true),
                    AdminReply = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AdminReplyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommentReactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<long>(type: "bigint", nullable: false),
                    UserIdentifier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Reaction = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentReactions_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommentReactions_CommentId_UserIdentifier",
                table: "CommentReactions",
                columns: new[] { "CommentId", "UserIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IsCanceled",
                table: "Comments",
                column: "IsCanceled");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IsConfirmed",
                table: "Comments",
                column: "IsConfirmed");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_OwnerRecordId_Type",
                table: "Comments",
                columns: new[] { "OwnerRecordId", "Type" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommentReactions");

            migrationBuilder.DropTable(
                name: "Comments");
        }
    }
}
