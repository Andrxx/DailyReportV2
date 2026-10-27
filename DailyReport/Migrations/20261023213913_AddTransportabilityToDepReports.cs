using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportabilityToDepReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ambulatory",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ambulatoryChildrens",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "nonTransportable",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "nonTransportableChildrens",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "transportable",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "transportableChildrens",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ambulatory",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "ambulatoryChildrens",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "nonTransportable",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "nonTransportableChildrens",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "transportable",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "transportableChildrens",
                table: "DepReports");
        }
    }
}
