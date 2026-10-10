using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class CareAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GLPS",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GLPSChildren",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "care",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "careDisodered",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sepsis",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sepsisChildren",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GLPS",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "GLPSChildren",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "care",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "careDisodered",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "sepsis",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "sepsisChildren",
                table: "DepReports");
        }
    }
}
