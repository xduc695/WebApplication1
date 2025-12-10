using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class long12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JoinCode",
                table: "ClassSections",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSections_JoinCode",
                table: "ClassSections",
                column: "JoinCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClassSections_JoinCode",
                table: "ClassSections");

            migrationBuilder.DropColumn(
                name: "JoinCode",
                table: "ClassSections");
        }
    }
}
