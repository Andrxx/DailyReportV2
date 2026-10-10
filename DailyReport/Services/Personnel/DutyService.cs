using DailyReport.Models;
using DailyReport.Models.PersonelFolder;
using DailyReport.Services.Reporting;
using Microsoft.EntityFrameworkCore;

namespace DailyReport.Services.Personnel
{
    /// <summary>
    /// Дежурная смена приёмного отделения, ОРИТ и рентгенологи — блок «Дежурная смена»
    /// внизу итоговой сводки (перенос из V1).
    ///
    /// Ключевое отличие от V1: сутки определяются полем reportDate, а не окном времени
    /// по dutyDate. В V1 отбиралось окном 07:00 → 07:00 следующего дня, и запись ровно
    /// на границе в него не попадала. Здесь reportDate — чистая дата, выборка идёт
    /// строго по равенству, а dutyDate остаётся только фактом сохранения.
    /// </summary>
    public class DutyService
    {
        /// <summary>Должность, из которой берутся дежурные в блоке.</summary>
        private const string DoctorPersType = "Врач";

        /// <summary>Должность рентгенологов — в V1 её в списке не было, выбрать было нельзя.</summary>
        private const string RentgenologistPersType = "Врач рентгенолог";

        private readonly ApplicationContext _context;
        private readonly ReportingCalendar _calendar;

        public DutyService(ApplicationContext context, ReportingCalendar calendar)
        {
            _context = context;
            _calendar = calendar;
        }

        /// <summary>
        /// Кого можно назначить в этот блок. Приёмное отделение и ОРИТ — обычные врачи,
        /// рентгенология — рентгенологи.
        /// </summary>
        public string GetPersTypeFor(DutyType type) => type == DutyType.Rentgenology
            ? RentgenologistPersType
            : DoctorPersType;

        /// <summary>Дежурные за отчётные сутки, по всем блокам сразу.</summary>
        public List<DutyDoc> GetForDay(DateOnly reportDate)
        {
            return _context.DutyDocs
                .AsNoTracking()
                .Where(d => d.reportDate == reportDate)
                .OrderBy(d => d.Id)
                .ToList();
        }

        /// <summary>Дежурные за отчётные сутки в одном блоке.</summary>
        public List<DutyDoc> GetForDay(DateOnly reportDate, DutyType type)
        {
            return _context.DutyDocs
                .AsNoTracking()
                .Where(d => d.reportDate == reportDate && d.type == type)
                .OrderBy(d => d.Id)
                .ToList();
        }

        public bool Add(DutyDoc doc, DateOnly reportDate, out string error)
        {
            error = string.Empty;

            if (!Normalize(doc, out error))
            {
                return false;
            }

            if (Exists(doc.dutyDoc!, doc.type, reportDate))
            {
                error = "Этот человек уже назначен в этом блоке за эти сутки.";
                return false;
            }

            doc.reportDate = reportDate;
            doc.dutyDate = _calendar.LocalNow;

            _context.DutyDocs.Add(doc);
            _context.SaveChanges();
            return true;
        }

        public bool Update(DutyDoc doc, out string error)
        {
            error = string.Empty;

            if (doc.Id == 0)
            {
                error = "Не выбрана запись дежурного.";
                return false;
            }

            if (!Normalize(doc, out error))
            {
                return false;
            }

            if (doc.reportDate is null)
            {
                error = "У записи не определены отчётные сутки.";
                return false;
            }

            if (Exists(doc.dutyDoc!, doc.type, doc.reportDate.Value, doc.Id))
            {
                error = "Этот человек уже назначен в этом блоке за эти сутки.";
                return false;
            }

            // Через найденную сущность, а не Update(doc): привязывать отсоединённый
            // объект нельзя, если он уже отслеживается в этом же контексте.
            var stored = _context.DutyDocs.FirstOrDefault(d => d.Id == doc.Id);
            if (stored is null)
            {
                error = "Запись дежурного не найдена.";
                return false;
            }

            _context.Entry(stored).CurrentValues.SetValues(doc);
            _context.SaveChanges();
            return true;
        }

        public void Delete(int id)
        {
            var stored = _context.DutyDocs.FirstOrDefault(d => d.Id == id);
            if (stored is null)
            {
                return;
            }

            _context.DutyDocs.Remove(stored);
            _context.SaveChanges();
        }

        /// <summary>
        /// Есть ли уже такой человек в этом блоке за эти сутки.
        /// Сравнение без учёта регистра делается в памяти, а не в SQL: SQLite функция
        /// lower() работает только с ASCII и кириллицу не трогает, поэтому запрос вида
        /// lower(dutyDoc) = ... молча ничего не находит.
        /// </summary>
        private bool Exists(string name, DutyType type, DateOnly reportDate, int exceptId = 0)
        {
            var normalized = (name ?? string.Empty).Trim();

            var sameDay = _context.DutyDocs
                .AsNoTracking()
                .Where(d => d.Id != exceptId && d.type == type && d.reportDate == reportDate)
                .Select(d => d.dutyDoc)
                .ToList();

            return sameDay.Any(n => string.Equals((n ?? string.Empty).Trim(), normalized, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Проверка обязательного ФИО и приведение «Отделения» к виду, который можно читать
        /// и сравнивать: в старых записях встречалось «4  6 7» с двумя пробелами.
        /// Сутки и тип здесь не меняются — они задаются блоком, из которого пришла запись.
        /// </summary>
        private static bool Normalize(DutyDoc doc, out string error)
        {
            doc.dutyDoc = (doc.dutyDoc ?? string.Empty).Trim();
            doc.departments = NormalizeDepartments(doc.departments);

            if (string.IsNullOrEmpty(doc.dutyDoc))
            {
                error = "Выберите дежурного.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        /// <summary>Обрезка краёв, схлопывание пробелов внутри, пустое значение — в null.</summary>
        private static string? NormalizeDepartments(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', parts);
        }
    }
}
