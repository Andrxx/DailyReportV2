using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class CareAdded1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "care",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "careDisodered",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "presentWithCare",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "presentWithCareChildren",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "presentWithCare",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "presentWithCareChildren",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "care",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "careDisodered",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "presentWithCare",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "presentWithCareChildren",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "presentWithCare",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "presentWithCareChildren",
                table: "DepReports");
        }
    }
}
