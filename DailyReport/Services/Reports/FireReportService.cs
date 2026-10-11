using DailyReport.Models;
using DailyReport.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace DailyReport.Services.Reports
{
    /// <summary>
    /// Сервис работы с пожарной сводкой (разрез 2). Отбор строк — по паре
    /// (подразделение, отчётные сутки пожарной сводки): граница суток у пожарной
    /// сводки своя, 08:00, и считается через <see cref="Reporting.FireReportingCalendar"/>.
    /// </summary>
    public class FireReportService
    {
        private readonly ApplicationContext _context;

        public FireReportService(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>Пожарная сводка подразделения за конкретные отчётные сутки (пожарные)</summary>
        public FireReport? GetByNumber(int depNumber, DateOnly reportDate)
        {
            return _context.FireReports
                .FirstOrDefault(rep => rep.DepNumber == depNumber && rep.ReportDate == reportDate);
        }

        /// <summary>
        /// Последняя сохранённая пожарная сводка подразделения, независимо от суток.
        /// Нужна долгохранимым подразделениям (охрана, общежитие): их значение берётся
        /// из последней строки, пока в неё не внесут изменения.
        /// </summary>
        public FireReport? GetLastByNumber(int depNumber)
        {
            return GetLastByNumberQuery(depNumber).FirstOrDefault();
        }

        /// <summary>
        /// То же, что <see cref="GetLastByNumber"/>, но без отслеживания сущности:
        /// результат переносится в новую строку, и конфликт трекера недопустим.
        /// </summary>
        public FireReport? GetLastByNumberNoTracking(int depNumber)
        {
            return GetLastByNumberQuery(depNumber).AsNoTracking().FirstOrDefault();
        }

        /// <summary>Все пожарные сводки за отчётные сутки (пожарные)</summary>
        public List<FireReport> GetForDay(DateOnly reportDate)
        {
            return _context.FireReports
                .Where(rep => rep.ReportDate == reportDate)
                .ToList();
        }

        /// <summary>Добавляет или обновляет пожарную сводку в БД</summary>
        public void AddOrUpdate(FireReport report)
        {
            _context.FireReports.Update(report);
            _context.SaveChanges();
        }

        /// <summary>
        /// Переносит значения в уже отслеживаемый экземпляр и сохраняет. Прикреплять
        /// клон через Update нельзя: контекст scoped, и в нём уже лежит сущность с тем же
        /// Id — EF бросает конфликт трекера (как в DepReportService.UpdateTracked).
        /// </summary>
        public void UpdateTracked(FireReport target, FireReport source)
        {
            _context.Entry(target).CurrentValues.SetValues(source);
            _context.SaveChanges();
        }

        private IQueryable<FireReport> GetLastByNumberQuery(int depNumber)
        {
            return _context.FireReports
                .Where(rep => rep.DepNumber == depNumber)
                .OrderByDescending(rep => rep.ReportDate)
                .ThenByDescending(rep => rep.Date);
        }
    }
}
