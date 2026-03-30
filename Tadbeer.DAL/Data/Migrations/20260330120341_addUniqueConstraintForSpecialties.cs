using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tadbeer.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class addUniqueConstraintForSpecialties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Specialties_Name",
                table: "Specialties",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Specialties_Name",
                table: "Specialties");
        }
    }
}
