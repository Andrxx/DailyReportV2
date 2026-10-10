using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyReport.Migrations
{
    /// <inheritdoc />
    public partial class AddReportDateToDepReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "reportDate",
                table: "DepReports",
                type: "TEXT",
                nullable: true);

            // Расчёт отчётных суток для уже сохранённых сводок по СТАРОЙ границе 07:00.
            // Значение 07:00 здесь зашито намеренно: оно отражает правило, по которому
            // сводки записывались раньше, и не зависит от новой границы из конфигурации.
            // Смена границы на 07:30 затрагивает только будущие записи, история не пересчитывается.
            //
            // Строки, у которых date не разбирается как дата, и служебные записи
            // с date = '0001-01-01' остаются с reportDate = NULL. В SQLite значения NULL
            // в уникальном индексе считаются различными, поэтому они не мешают индексу.
            migrationBuilder.Sql(@"
UPDATE DepReports
SET reportDate = date(
        CASE WHEN time(date) < '07:00:00'
             THEN datetime(date, '-1 day')
             ELSE datetime(date)
        END)
WHERE reportDate IS NULL
  AND date IS NOT NULL
  AND date <> ''
  AND date NOT LIKE '0001-01-01%'");

            // Уникальный индекс требует, чтобы на пару (отделение, сутки) была одна строка.
            // Если в базе остались дубли, миграция упадёт - это намеренно:
            // молча удалять данные нельзя, сначала их надо разобрать.
            migrationBuilder.CreateIndex(
                name: "IX_DepReports_depNumber_reportDate",
                table: "DepReports",
                columns: new[] { "depNumber", "reportDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DepReports_depNumber_reportDate",
                table: "DepReports");

            migrationBuilder.DropColumn(
                name: "reportDate",
                table: "DepReports");
        }
    }
}
