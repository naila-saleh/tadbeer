using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tadbeer.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePhoneNumberOwnershipAndPrimaryOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhoneNumbers_AspNetUsers_WorkerId",
                table: "PhoneNumbers");

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "PhoneNumbers",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PhoneNumbers_WorkerId",
                table: "PhoneNumbers",
                newName: "IX_PhoneNumbers_UserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PhoneNumbers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddForeignKey(
                name: "FK_PhoneNumbers_AspNetUsers_UserId",
                table: "PhoneNumbers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhoneNumbers_AspNetUsers_UserId",
                table: "PhoneNumbers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PhoneNumbers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PhoneNumbers",
                newName: "WorkerId");

            migrationBuilder.RenameIndex(
                name: "IX_PhoneNumbers_UserId",
                table: "PhoneNumbers",
                newName: "IX_PhoneNumbers_WorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhoneNumbers_AspNetUsers_WorkerId",
                table: "PhoneNumbers",
                column: "WorkerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
