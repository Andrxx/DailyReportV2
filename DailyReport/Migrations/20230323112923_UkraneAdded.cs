using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class UkraneAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "existedChildrens",
                table: "DepReports",
                newName: "otherUkraneChildren");

            migrationBuilder.AddColumn<int>(
                name: "otherUkrane",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "otherUkraneChildren",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "existedChildren",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "otherUkrane",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "otherUkrane",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "otherUkraneChildren",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "existedChildren",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "otherUkrane",
                table: "DepReports");

            migrationBuilder.RenameColumn(
                name: "otherUkraneChildren",
                table: "DepReports",
                newName: "existedChildrens");
        }
    }
}
