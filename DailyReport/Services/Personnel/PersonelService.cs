using DailyReport.Models;
using DailyReport.Models.PersonelFolder;
using Microsoft.EntityFrameworkCore;

namespace DailyReport.Services.Personnel
{
    /// <summary>
    /// Справочник персонала: врачи, рентгенологи, медсёстры, лаборанты.
    /// Из этого справочника заполняются выпадающие списки дежурных в итоговой сводке,
    /// поэтому ФИО и должность здесь — не украшение, а источник данных для отчёта.
    /// </summary>
    public class PersonelService
    {
        private readonly ApplicationContext _context;

        public PersonelService(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>Весь справочник, по ФИО. Без отслеживания: правки идут через Update.</summary>
        public List<Personel> GetAll()
        {
            return _context.Personels
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ThenBy(p => p.PersType)
                .ToList();
        }

        /// <summary>
        /// Названия ВСЕХ отделений больницы — динамически из таблицы Departments
        /// (порядок ShowOrder), а не из подразделений, встречающихся в справочнике:
        /// справочник может не покрывать все отделения, а список нужен полный.
        /// </summary>
        public List<string> GetDepartmentNames()
        {
            return _context.Departments
                .AsNoTracking()
                .Where(d => !string.IsNullOrEmpty(d.Name))
                .OrderBy(d => d.ShowOrder)
                .Select(d => d.Name!)
                .ToList();
        }

        /// <summary>Работники указанной должности, по ФИО — для списка выбора дежурного.</summary>
        public List<Personel> GetByPersType(string persType)
        {
            return _context.Personels
                .AsNoTracking()
                .Where(p => p.PersType == persType)
                .OrderBy(p => p.Name)
                .ToList();
        }

        /// <summary>
        /// Есть ли в справочнике человек с таким ФИО и такой должностью.
        /// Нужно для проверки дублей: два одинаковых ФИО в списке выбора дежурного
        /// неотличимы друг от друга, а отчёт потом не поймёт, кто это был.
        ///
        /// Сравнение без учёта регистра идёт в памяти: SQLite lower() не работает с
        /// кириллицей, поэтому lower(Name) в запросе ничего не находит.
        /// </summary>
        public bool Exists(string name, string persType, int exceptId = 0)
        {
            var normalized = (name ?? string.Empty).Trim();

            var names = _context.Personels
                .AsNoTracking()
                .Where(p => p.Id != exceptId && p.PersType == persType)
                .Select(p => p.Name)
                .ToList();

            return names.Any(n => string.Equals((n ?? string.Empty).Trim(), normalized, StringComparison.OrdinalIgnoreCase));
        }

        public bool Add(Personel person, out string error)
        {
            error = string.Empty;

            if (!Normalize(person, out error))
            {
                return false;
            }

            if (Exists(person.Name!, person.PersType!))
            {
                error = "В справочнике уже есть человек с таким ФИО и такой должностью.";
                return false;
            }

            _context.Personels.Add(person);
            _context.SaveChanges();
            return true;
        }

        public bool Update(Personel person, out string error)
        {
            error = string.Empty;

            if (person.Id == 0)
            {
                error = "Не выбрана запись справочника.";
                return false;
            }

            if (!Normalize(person, out error))
            {
                return false;
            }

            if (Exists(person.Name!, person.PersType!, person.Id))
            {
                error = "В справочнике уже есть человек с таким ФИО и такой должностью.";
                return false;
            }

            // Через найденную сущность, а не Update(person): привязывать отсоединённый
            // объект нельзя, если он уже отслеживается в этом же контексте.
            var stored = _context.Personels.FirstOrDefault(p => p.Id == person.Id);
            if (stored is null)
            {
                error = "Запись справочника не найдена.";
                return false;
            }

            _context.Entry(stored).CurrentValues.SetValues(person);
            _context.SaveChanges();
            return true;
        }

        public void Delete(int id)
        {
            var stored = _context.Personels.FirstOrDefault(p => p.Id == id);
            if (stored is null)
            {
                return;
            }

            _context.Personels.Remove(stored);
            _context.SaveChanges();
        }

        /// <summary>
        /// Обрезка пробелов и проверка обязательных полей. ФИО и должность обязательны:
        /// без них человек не попадёт ни в один список выбора.
        /// </summary>
        private static bool Normalize(Personel person, out string error)
        {
            person.Name = (person.Name ?? string.Empty).Trim();
            person.PersType = (person.PersType ?? string.Empty).Trim();
            person.Department = (person.Department ?? string.Empty).Trim();
            person.Phone = (person.Phone ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(person.Name))
            {
                error = "Укажите ФИО.";
                return false;
            }

            if (string.IsNullOrEmpty(person.PersType))
            {
                error = "Укажите должность.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
