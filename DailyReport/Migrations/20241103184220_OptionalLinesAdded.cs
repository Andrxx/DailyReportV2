using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class OptionalLinesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "optionalCare",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalCarelChildren",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalNozology",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalNozologyChildren",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalOxygen",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalOxygenChildren",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalSocial",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalSocialChildren",
                table: "FinalReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalCare",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalCarelChildren",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalNozology",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalNozologyChildren",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalOxygen",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalOxygenChildren",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalSocial",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "optionalSocialChildren",
                table: "DepReports",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ReportLine",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DepartmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    lineType = table.Column<string>(type: "TEXT", nullable: true),
                    Adults = table.Column<int>(type: "INTEGER", nullable: false),
                    Children = table.Column<int>(type: "INTEGER", nullable: false),
                    DepReportId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportLine", x => x.id);
                    table.ForeignKey(
                        name: "FK_ReportLine_DepReports_DepReportId",
                        column: x => x.DepReportId,
                        principalTable: "DepReports",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportLine_DepReportId",
                table: "ReportLine",
                column: "DepReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportLine");

            migrationBuilder.DropColumn(
                name: "optionalCare",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "optionalCarelChildren",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "optionalNozology",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "optionalNozologyChildren",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "optionalOxygen",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "optionalOxygenChildren",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "optionalSocial",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "optionalSocialChildren",
                table: "FinalReports");

            migrationBuilder.DropColumn(
                name: "optionalCare",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "optionalCarelChildren",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "optionalNozology",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "optionalNozologyChildren",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "optionalOxygen",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "optionalOxygenChildren",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "optionalSocial",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "optionalSocialChildren",
                table: "DepReports");
        }
    }
}
