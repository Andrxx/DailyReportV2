using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class AddReportDateAndRouteToOutcomingPatients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ReportDate",
                table: "OutcomingPatients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Route",
                table: "OutcomingPatients",
                type: "INTEGER",
                nullable: true);

            // Расчёт отчётных суток для уже сохранённых пациентов по СТАРОЙ границе 07:00.
            // Значение 07:00 здесь зашито намеренно: оно отражает правило, по которому
            // записи заводились в V1 (ключ "ReportTimeChange"), и не зависит от новой
            // границы из конфигурации. Смена границы на 07:30 затрагивает только будущие
            // записи, история не пересчитывается.
            //
            // В V1 пациенты отбирались ОКНОМ времени (Date > 07:00 && Date < 07:00
            // следующего дня), а Date — это момент сохранения. Здесь та же граница
            // применяется один раз, к дате: ReportDate становится ЧИСТОЙ ДАТОЙ, и дальше
            // выборка за сутки идёт строго по равенству, как в DepReportService.
            migrationBuilder.Sql(@"
UPDATE OutcomingPatients
SET ReportDate = date(
        CASE WHEN time(Date) < '07:00:00'
             THEN datetime(Date, '-1 day')
             ELSE datetime(Date)
        END)
WHERE ReportDate IS NULL
  AND Date IS NOT NULL
  AND Date <> ''
  AND Date NOT LIKE '0001-01-01%'");

            // Route у старых записей намеренно остаётся NULL: разбирать их текст в SQL
            // нельзя — SQLite lower() не приводит регистр кириллицы, и нормализация
            // молча ничего бы не нашла. Тип маршрута для них определяется в памяти,
            // см. OutPatientService.RouteFromText. Новые записи заполняют Route формой.
            //
            // Уникального индекса нет: за сутки может быть несколько пациентов.
            migrationBuilder.CreateIndex(
                name: "IX_OutcomingPatients_ReportDate",
                table: "OutcomingPatients",
                columns: new[] { "ReportDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutcomingPatients_ReportDate",
                table: "OutcomingPatients");

            migrationBuilder.DropColumn(
                name: "Route",
                table: "OutcomingPatients");

            migrationBuilder.DropColumn(
                name: "ReportDate",
                table: "OutcomingPatients");
        }
    }
}
