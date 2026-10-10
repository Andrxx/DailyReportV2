using DailyReport.Models;
using DailyReport.Models.Reports;
using Microsoft.EntityFrameworkCore;

namespace DailyReport.Services.Reports
{
    /// <summary>
    /// Сервис работы со сводками отделений
    /// </summary>
    public class DepReportService
    {
        private readonly ApplicationContext _context;

        public DepReportService(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получаем отчет отделения за конкретные отчётные сутки
        /// </summary>
        public DepReport? GetByNumber(int depNumber, DateOnly reportDate)
        {
            return _context.DepReports
                .FirstOrDefault(rep => rep.depNumber == depNumber && rep.reportDate == reportDate);
        }

        /// <summary>
        /// Получаем отчет отделения за конкретные отчётные сутки без отслеживания сущности
        /// </summary>
        public DepReport? GetByNumberNoTracking(int depNumber, DateOnly reportDate)
        {
            return _context.DepReports
                .AsNoTracking()
                .FirstOrDefault(rep => rep.depNumber == depNumber && rep.reportDate == reportDate);
        }

        /// <summary>
        /// Добавляет или обновляет отчет в БД
        /// </summary>
        public void AddOrUpdate(DepReport report)
        {
            _context.DepReports.Update(report);
            _context.SaveChanges();
        }

        /// <summary>
        /// Переносит значения сводки в уже отслеживаемый экземпляр и сохраняет.
        /// Прикреплять клон через Update нельзя: контекст scoped, и в нём уже лежит
        /// сущность с тем же Id — EF бросает конфликт трекера.
        /// </summary>
        public void UpdateTracked(DepReport target, DepReport source)
        {
            _context.Entry(target).CurrentValues.SetValues(source);
            _context.SaveChanges();
        }

        /// <summary>
        /// Перезаписывает сводку данными другой сводки (коррекция с прошлых суток).
        /// Не меняет дату, нозологии и Id.
        /// </summary>
        public DepReport RewriteReportForLastDay(DepReport lastReport)
        {
            DepReport newReport = (DepReport)lastReport.Clone();
            newReport.present = lastReport.present;
            newReport.presentChildrens = lastReport.presentChildrens;
            newReport.existed = lastReport.present;
            newReport.existedChildren = lastReport.presentChildrens;
            // дежурная м/с за вчера к сегодняшним суткам отношения не имеет
            newReport.dutyNurse = null;
            newReport.income = 0;
            newReport.incomeChildrens = 0;
            newReport.outcome = 0;
            newReport.outcomeChildrens = 0;
            newReport.movedInDep = 0;
            newReport.movedOutDep = 0;
            newReport.movedInDepChildrens = 0;
            newReport.movedOutDepChildrens = 0;
            newReport.died = 0;
            newReport.diedChildrens = 0;
            return newReport;
        }
    }
}