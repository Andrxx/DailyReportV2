using DailyReport.Models;
using DailyReport.Models.Reports;
using DailyReport.Services.Reporting;
using Microsoft.EntityFrameworkCore;

namespace DailyReport.Services.Reports
{
    /// <summary>
    /// Автоматическое заполнение сводок отделений за текущие отчётные сутки.
    ///
    /// Отделение может не работать в этот день (выходной, праздник, пропуск подачи) — тогда
    /// строки за сутки нет. Если у отделения включён флаг «Брать прошлые значения»
    /// (<see cref="DepartmentSettings.AutoFillFromPrevious"/>), итоговая сводка создаёт строку
    /// сама из прошлой сводки этого же отделения: население то же, движение нулевое.
    ///
    /// Это единственная запись в БД со стороны итоговой сводки, и делает её ТОЛЬКО приложение
    /// автоматически. Пользователь по-прежнему не может править сводки отделений из сводки
    /// по больнице: форма ввода остаётся на странице отделения.
    ///
    /// Подстановка идёт только за ТЕКУЩИЕ отчётные сутки. Просмотр сводки за прошлую дату
    /// историю не дозаполняет.
    /// </summary>
    public class AutoFillService
    {
        /// <summary>
        /// Глубина поиска последней записи в режиме «последняя фактическая запись».
        /// Без ограничения давно не работавшее отделение подтянуло бы строку годичной давности.
        /// </summary>
        private const int MaxLookbackDays = 31;

        private readonly ApplicationContext _context;
        private readonly DepReportService _depReports;
        private readonly ReportingCalendar _calendar;

        public AutoFillService(
            ApplicationContext context,
            DepReportService depReports,
            ReportingCalendar calendar)
        {
            _context = context;
            _depReports = depReports;
            _calendar = calendar;
        }

        /// <summary>
        /// Создаёт недостающие строки за текущие отчётные сутки у отделений с включённым
        /// автозаполнением. Возвращает число созданных строк.
        /// </summary>
        public int EnsureFilled(DateOnly reportDate)
        {
            // Прошлые сутки не трогаем: открытие сводки за 10 октября не должно
            // создавать строку за 10 октября задним числом.
            if (reportDate != _calendar.Today)
            {
                return 0;
            }

            var departments = _context.Departments
                .AsNoTracking()
                .Where(d => d.Settings != null && d.Settings.AutoFillFromPrevious)
                .Select(d => new { d.Id, d.Settings!.AutoFillSource })
                .ToList();

            int created = 0;
            foreach (var dep in departments)
            {
                // Ручные данные не затираются никогда: подстановка только при отсутствии строки.
                if (_depReports.GetByNumberNoTracking(dep.Id, reportDate) is not null)
                {
                    continue;
                }

                DepReport? source = dep.AutoFillSource == AutoFillSource.LastAvailable
                    ? FindLastBefore(dep.Id, reportDate)
                    : _depReports.GetByNumberNoTracking(dep.Id, reportDate.AddDays(-1));

                if (source is null)
                {
                    continue; // подставлять нечего — это не ошибка
                }

                // Копируется вся строка (нозологии, кислород, уход, транспортабельность),
                // «Состояло» = прошлое «Состоит», движение обнуляется, дежурная м/с сбрасывается.
                DepReport copy = _depReports.RewriteReportForLastDay(source);
                copy.Id = 0;
                copy.depNumber = dep.Id;
                copy.reportDate = reportDate;
                copy.date = _calendar.LocalNow;

                try
                {
                    _depReports.AddOrUpdate(copy);
                    created++;
                }
                catch (DbUpdateException)
                {
                    // Гонка двух контуров: строку уже создал другой запрос.
                    // Уникальный индекс (depNumber, reportDate) не даёт дубля — это нормально.
                    _context.ChangeTracker.Clear();
                }
            }

            return created;
        }

        /// <summary>Последняя сводка отделения строго до указанных суток (в пределах окна поиска).</summary>
        private DepReport? FindLastBefore(int depNumber, DateOnly reportDate)
        {
            DateOnly from = reportDate.AddDays(-MaxLookbackDays);

            return _context.DepReports
                .AsNoTracking()
                .Where(r => r.depNumber == depNumber
                            && r.reportDate < reportDate
                            && r.reportDate >= from)
                .OrderByDescending(r => r.reportDate)
                .FirstOrDefault();
        }
    }
}
