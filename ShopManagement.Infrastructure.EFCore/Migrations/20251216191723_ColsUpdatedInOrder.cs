using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopManagement.Infrastructure.EFCore.Migrations
{
    public partial class ColsUpdatedInOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TrackingNo",
                table: "Orders",
                newName: "ShipTrackingNo");

            migrationBuilder.AddColumn<string>(
                name: "CardOwnerFullname",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardOwnerNationalCode",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReceiptPic",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardOwnerFullname",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CardOwnerNationalCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentReceiptPic",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ShipTrackingNo",
                table: "Orders",
                newName: "TrackingNo");
        }
    }
}
