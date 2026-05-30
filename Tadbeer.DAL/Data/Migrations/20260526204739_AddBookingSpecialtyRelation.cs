using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tadbeer.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingSpecialtyRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SpecialtyId",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_SpecialtyId",
                table: "Bookings",
                column: "SpecialtyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Specialties_SpecialtyId",
                table: "Bookings",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Specialties_SpecialtyId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_SpecialtyId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SpecialtyId",
                table: "Bookings");
        }
    }
}
