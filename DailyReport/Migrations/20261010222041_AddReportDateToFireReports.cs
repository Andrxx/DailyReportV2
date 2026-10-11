using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class AddReportDateToFireReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ReportDate",
                table: "FireReports",
                type: "TEXT",
                nullable: true);

            // Расчёт отчётных суток пожарной сводки для уже сохранённых строк по границе 08:00
            // (правило первой версии, значение зашито намеренно: от конфигурации не зависит).
            // Строки, у которых Date не разбирается как дата, и служебные записи
            // с Date = '0001-01-01' остаются с ReportDate = NULL: в SQLite значения NULL
            // в уникальном индексе считаются различными, поэтому они индексу не мешают.
            migrationBuilder.Sql(@"
UPDATE FireReports
SET ReportDate = date(
        CASE WHEN time(Date) < '08:00:00'
             THEN datetime(Date, '-1 day')
             ELSE datetime(Date)
        END)
WHERE ReportDate IS NULL
  AND Date IS NOT NULL
  AND Date <> ''
  AND Date NOT LIKE '0001-01-01%'");

            // Уникальный индекс требует одну строку на пару (подразделение, сутки). В базе
            // остались исторические дубли — несколько сохранений подразделения в рамках
            // суток (например, отделение 1: четыре строки «2023-04-03»). Из каждой группы
            // оставляем последнюю по времени строку: пожарная сводка по определению
            // «текущее значение», и старые сохранения того же дня не используются.
            migrationBuilder.Sql(@"
DELETE FROM FireReports
WHERE Id IN (
    SELECT Id FROM (
        SELECT Id,
               ROW_NUMBER() OVER (PARTITION BY DepNumber, ReportDate ORDER BY Date DESC, Id DESC) AS rn
        FROM FireReports
    )
    WHERE rn > 1
)");

            migrationBuilder.CreateIndex(
                name: "IX_FireReports_DepNumber_ReportDate",
                table: "FireReports",
                columns: new[] { "DepNumber", "ReportDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FireReports_DepNumber_ReportDate",
                table: "FireReports");

            migrationBuilder.DropColumn(
                name: "ReportDate",
                table: "FireReports");
        }
    }
}
