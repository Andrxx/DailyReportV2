using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class AddReportDateToDutyDocs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "reportDate",
                table: "DutyDocs",
                type: "TEXT",
                nullable: true);

            // Расчёт отчётных суток для уже сохранённых дежурных по СТАРОЙ границе 07:00.
            // Значение 07:00 здесь зашито намеренно: оно отражает правило, по которому
            // дежурные записывались в V1 (ключ "ReportTimeChange"), и не зависит от
            // новой границы из конфигурации. Смена границы на 07:30 затрагивает только
            // будущие записи, история не пересчитывается.
            //
            // В V1 дежурные выбирались ОКНОМ времени (dutyDate > 07:00 && dutyDate < 07:00
            // следующего дня), а dutyDate — это момент сохранения. Здесь та же граница
            // применяется один раз, к дате: reportDate становится ЧИСТОЙ ДАТОЙ, и дальше
            // выборка за сутки идёт строго по равенству, как в DepReportService.
            migrationBuilder.Sql(@"
UPDATE DutyDocs
SET reportDate = date(
        CASE WHEN time(dutyDate) < '07:00:00'
             THEN datetime(dutyDate, '-1 day')
             ELSE datetime(dutyDate)
        END)
WHERE reportDate IS NULL
  AND dutyDate IS NOT NULL
  AND dutyDate <> ''
  AND dutyDate NOT LIKE '0001-01-01%'");

            // Уникального индекса здесь, в отличие от DepReports, нет: за сутки в одном
            // блоке может дежурить несколько врачей. Индекс обычный — он нужен только
            // для выборки за сутки.
            migrationBuilder.CreateIndex(
                name: "IX_DutyDocs_reportDate_type",
                table: "DutyDocs",
                columns: new[] { "reportDate", "type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DutyDocs_reportDate_type",
                table: "DutyDocs");

            migrationBuilder.DropColumn(
                name: "reportDate",
                table: "DutyDocs");
        }
    }
}
