using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class MeaslesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "measles",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "measlesChildren",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "measles",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "measlesChildren",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "measles",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "measlesChildren",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "measles",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "measlesChildren",
                table: "DepReports");
        }
    }
}
