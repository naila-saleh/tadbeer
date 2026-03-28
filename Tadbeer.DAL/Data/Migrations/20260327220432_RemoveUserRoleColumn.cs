using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tadbeer.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserRoleColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_City_Role",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Role_Status",
                table: "AspNetUsers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ApplicationUser_Role",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Status",
                table: "AspNetUsers",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Status",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_City_Role",
                table: "AspNetUsers",
                columns: new[] { "City", "Role" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Role_Status",
                table: "AspNetUsers",
                columns: new[] { "Role", "Status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ApplicationUser_Role",
                table: "AspNetUsers",
                sql: "[Role] IN ('Admin','Worker','User')");
        }
    }
}
