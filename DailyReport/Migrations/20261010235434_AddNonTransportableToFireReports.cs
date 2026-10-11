using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class AddNonTransportableToFireReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NonTransportableAdults",
                table: "FireReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NonTransportableChildren",
                table: "FireReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NonTransportableAdults",
                table: "FireReports");

            migrationBuilder.DropColumn(
                name: "NonTransportableChildren",
                table: "FireReports");
        }
    }
}
