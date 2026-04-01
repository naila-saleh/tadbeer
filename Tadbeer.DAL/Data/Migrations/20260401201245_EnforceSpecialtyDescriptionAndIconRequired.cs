using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tadbeer.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSpecialtyDescriptionAndIconRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "Specialties",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Specialties",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Specialty_Description_NotEmpty",
                table: "Specialties",
                sql: "LEN(LTRIM(RTRIM([Description]))) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Specialty_Icon_NotEmpty",
                table: "Specialties",
                sql: "LEN(LTRIM(RTRIM([Icon]))) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Specialty_Description_NotEmpty",
                table: "Specialties");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Specialty_Icon_NotEmpty",
                table: "Specialties");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "Specialties",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Specialties",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }
    }
}
