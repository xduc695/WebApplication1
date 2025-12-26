using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassMate.Api.Migrations
{
    /// <inheritdoc />
    public partial class v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "CourseResources");

            migrationBuilder.CreateTable(
                name: "CourseResourceFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseResourceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseResourceFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseResourceFiles_CourseResources_CourseResourceId",
                        column: x => x.CourseResourceId,
                        principalTable: "CourseResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseResourceFiles_CourseResourceId",
                table: "CourseResourceFiles",
                column: "CourseResourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseResourceFiles");

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "CourseResources",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
