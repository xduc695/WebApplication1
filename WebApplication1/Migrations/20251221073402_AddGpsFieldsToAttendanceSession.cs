using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGpsFieldsToAttendanceSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AllowedRadius",
                table: "AttendanceSessions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TargetLatitude",
                table: "AttendanceSessions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TargetLongitude",
                table: "AttendanceSessions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedRadius",
                table: "AttendanceSessions");

            migrationBuilder.DropColumn(
                name: "TargetLatitude",
                table: "AttendanceSessions");

            migrationBuilder.DropColumn(
                name: "TargetLongitude",
                table: "AttendanceSessions");
        }
    }
}
