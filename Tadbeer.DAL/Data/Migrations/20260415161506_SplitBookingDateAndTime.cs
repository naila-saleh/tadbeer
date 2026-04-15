using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tadbeer.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitBookingDateAndTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "Bookings",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "Bookings",
                type: "time",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE b
                SET
                    StartTime = COALESCE(wh.StartTime, CAST('08:00:00' AS time)),
                    EndTime = COALESCE(wh.EndTime, CAST('09:00:00' AS time))
                FROM Bookings b
                LEFT JOIN WorkingHours wh ON wh.Id = b.WorkingHourId;

                UPDATE Bookings
                SET EndTime = CAST(DATEADD(minute, 60, CAST(StartTime AS datetime2)) AS time)
                WHERE EndTime <= StartTime;
            ");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "Bookings",
                type: "time",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "Bookings",
                type: "time",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_TimeRange",
                table: "Bookings",
                sql: "[StartTime] < [EndTime]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_TimeRange",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Bookings");
        }
    }
}
