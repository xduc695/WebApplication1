using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class GPSlocal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "AttendanceSessions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "AttendanceSessions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "AttendanceRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "AttendanceRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "AttendanceSessions");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "AttendanceSessions");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "AttendanceRecords");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "AttendanceRecords");
        }
    }
}
