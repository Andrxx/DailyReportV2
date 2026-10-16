using DailyReport.Models;
using DailyReport.Models.PersonelFolder;
using DailyReport.Services.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DailyReport.Services.Reports
{
    /// <summary>
    /// Контакт технической службы (охрана, водитель, электрик, сантехник), который не хранится
    /// в БД, а задаётся раз и навсегда в конфигурации (секция <c>AddedNurses</c>), как в первой
    /// версии. Значение выводится в пожарной сводке рядом с дежурными сёстрами подразделения,
    /// указанного в поле <c>Department</c>.
    /// </summary>
    public sealed record AddedNurseContact(int Department, string Name, string? Phone);

    /// <summary>
    /// Дежурные медицинские сёстры пожарной сводки (разрез 2). Отбор строк, как в первой
    /// версии, идёт по окну времени ввода: сутки пожарной сводки начинаются в 08:00
    /// (<see cref="FireReportingCalendar.DayCutoff"/>) и длятся до 08:00 следующего дня.
    /// Поле <c>department</c> хранит номер отделения (совпадает с <see cref="Department.Id"/>),
    /// как в таблице DutyNurses первой версии.
    /// </summary>
    public class DutyNurseService
    {
        private readonly ApplicationContext _context;
        private readonly FireReportingCalendar _calendar;
        private readonly IConfiguration _configuration;

        public DutyNurseService(ApplicationContext context, FireReportingCalendar calendar, IConfiguration configuration)
        {
            _context = context;
            _calendar = calendar;
            _configuration = configuration;
        }

        /// <summary>Отчётные сутки пожарной сводки «сейчас».</summary>
        public DateOnly ReportDate => _calendar.Today;

        /// <summary>Дежурные сёстры за текущие отчётные сутки пожарной сводки, по отделениям.</summary>
        public List<DutyNurse> GetForDay()
        {
            var (start, end) = DayWindow();

            return _context.DutyNurses
                .AsNoTracking()
                .Where(n => n.dutyDate >= start && n.dutyDate < end)
                .OrderBy(n => n.department)
                .ThenBy(n => n.name)
                .ToList();
        }

        /// <summary>Дежурные сёстры конкретного отделения за текущие отчётные сутки.</summary>
        public List<DutyNurse> GetForDepartment(int depNumber)
        {
            var (start, end) = DayWindow();

            return _context.DutyNurses
                .AsNoTracking()
                .Where(n => n.department == depNumber && n.dutyDate >= start && n.dutyDate < end)
                .OrderBy(n => n.name)
                .ToList();
        }

        /// <summary>
        /// Назначен ли уже этот человек в этом отделении за текущие сутки. Сравнение ФИО —
        /// в памяти: SQLite lower() не работает с кириллицей, поэтому запрос ничего бы не нашёл.
        /// </summary>
        public bool Exists(int depNumber, string name)
        {
            var normalized = (name ?? string.Empty).Trim();

            var names = _context.DutyNurses
                .AsNoTracking()
                .Where(n => n.department == depNumber && n.dutyDate >= DayWindow().start && n.dutyDate < DayWindow().end)
                .Select(n => n.name)
                .ToList();

            return names.Any(n => string.Equals((n ?? string.Empty).Trim(), normalized, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Контакты технических служб из конфигурации (секция <c>AddedNurses</c>), как в первой
        /// версии. Читается по индексам (<c>AddedNurses:0:Name</c>, …): секции нет — пустой
        /// список. Порядок — как в конфигурации, он и попадает в сводку.
        /// </summary>
        public List<AddedNurseContact> GetConfiguredNurses()
        {
            var contacts = new List<AddedNurseContact>();

            for (var i = 0; ; i++)
            {
                var name = _configuration[$"AddedNurses:{i}:Name"];
                if (string.IsNullOrWhiteSpace(name))
                {
                    break;
                }

                var phone = _configuration[$"AddedNurses:{i}:Phone"];
                int.TryParse(_configuration[$"AddedNurses:{i}:Department"], out var department);

                contacts.Add(new AddedNurseContact(
                    department,
                    name.Trim(),
                    string.IsNullOrWhiteSpace(phone) ? null : phone.Trim()));
            }

            return contacts;
        }

        /// <summary>Добавляет дежурную сестру. Сутки задаёт время ввода (<see cref="FireReportingCalendar.LocalNow"/>).</summary>
        public void Add(DutyNurse nurse)
        {
            nurse.dutyDate = _calendar.LocalNow;
            _context.DutyNurses.Add(nurse);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var stored = _context.DutyNurses.FirstOrDefault(n => n.Id == id);
            if (stored is null)
            {
                return;
            }

            _context.DutyNurses.Remove(stored);
            _context.SaveChanges();
        }

        /// <summary>
        /// Границы отчётных суток пожарной сводки: от 08:00 текущей даты до 08:00 следующей.
        /// </summary>
        private (DateTime start, DateTime end) DayWindow()
        {
            var start = _calendar.Today.ToDateTime(_calendar.DayCutoff);
            return (start, start.AddDays(1));
        }
    }
}
