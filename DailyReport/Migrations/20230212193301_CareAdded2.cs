using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class CareAdded2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GLPS",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "GLPSChildren",
                table: "DepReports");

            migrationBuilder.AddColumn<int>(
                name: "sepsis",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sepsisChildren",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sepsis",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "sepsisChildren",
                table: "FinalReports");

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
        }
    }
}
