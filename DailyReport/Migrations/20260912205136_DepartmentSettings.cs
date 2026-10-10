using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class DepartmentSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DepartmentSettings",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    ShowOnMainPage = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowOnFinalReport = table.Column<bool>(type: "INTEGER", nullable: false),
                    CountSpotsInFinalReport = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowOnFireReport = table.Column<bool>(type: "INTEGER", nullable: false),
                    ShowOnWards = table.Column<bool>(type: "INTEGER", nullable: false),
                    SimpleReportForm = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentSettings", x => x.DepartmentId);
                    table.ForeignKey(
                        name: "FK_DepartmentSettings_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // сидинг настроек для существующих отделений: роли по алиасам и флаги показа
            // как списки исключений конфигурации V1
            migrationBuilder.Sql(@"
INSERT INTO ""DepartmentSettings"" (""DepartmentId"", ""Type"", ""ShowOnMainPage"", ""ShowOnFinalReport"", ""CountSpotsInFinalReport"", ""ShowOnFireReport"", ""ShowOnWards"", ""SimpleReportForm"")
SELECT ""Id"",
  CASE ""Allias""
    WHEN 81 THEN 2 WHEN 82 THEN 3 WHEN 21 THEN 1
    WHEN 90 THEN 4 WHEN 91 THEN 5
    WHEN 101 THEN 6 WHEN 102 THEN 7 WHEN 103 THEN 8 WHEN 104 THEN 9
    ELSE 0 END,
  CASE WHEN ""Allias"" IN (101, 102, 103, 104) THEN 0 ELSE 1 END,
  CASE WHEN ""Allias"" IN (81, 82, 101, 102, 103, 104) THEN 0 ELSE 1 END,
  CASE WHEN ""Allias"" IN (21, 81, 82, 101, 102, 103, 104) THEN 0 ELSE 1 END,
  CASE WHEN ""Allias"" IN (11, 91, 21, 81, 82) THEN 0 ELSE 1 END,
  CASE WHEN ""Allias"" IN (21, 81, 82, 90, 101, 102, 103, 104) THEN 0 ELSE 1 END,
  CASE WHEN ""Allias"" IN (81, 82) THEN 1 ELSE 0 END
FROM ""Departments"";
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepartmentSettings");
        }
    }
}
