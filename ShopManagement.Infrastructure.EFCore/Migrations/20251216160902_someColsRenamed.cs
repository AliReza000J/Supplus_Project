using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopManagement.Infrastructure.EFCore.Migrations
{
    public partial class someColsRenamed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "Orders",
                newName: "PayMethod");

            migrationBuilder.RenameColumn(
                name: "Mobile",
                table: "Orders",
                newName: "RecieverMobile");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Orders",
                newName: "RecieverFullName");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Orders",
                newName: "RecieverAddress");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecieverMobile",
                table: "Orders",
                newName: "Mobile");

            migrationBuilder.RenameColumn(
                name: "RecieverFullName",
                table: "Orders",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "RecieverAddress",
                table: "Orders",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "PayMethod",
                table: "Orders",
                newName: "PaymentMethod");
        }
    }
}
