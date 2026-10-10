using System.Globalization;
using DailyReport.Models;
using DailyReport.Services.Reporting;
using Microsoft.EntityFrameworkCore;

namespace DailyReport.Services.Reports
{
    /// <summary>
    /// Отказы от госпитализации и их маршрут — блок «Переводы и отказы» внизу
    /// итоговой сводки (перенос из V1).
    ///
    /// Ключевые отличия от V1:
    /// 1) сутки определяются полем reportDate, а не окном времени по Date;
    /// 2) тип маршрута выбирается формой (Route), а не угадывается сравнением строки
    ///    SubmitedTo с литералом — в базе V1 лежали «Отказ», «отказ», «АМБУЛАТОРНО»,
    ///    и отказы уезжали в строку «В другую МО»;
    /// 3) подсчёт взрослых и детей не бросает исключений. В V1 стоял int.Parse(AgeYears)
    ///    внутри пустого catch: одно значение «3.5» или пустое поле обнуляло все числа блока.
    /// </summary>
    public class OutPatientService
    {
        private readonly ApplicationContext _context;
        private readonly ReportingCalendar _calendar;

        public OutPatientService(ApplicationContext context, ReportingCalendar calendar)
        {
            _context = context;
            _calendar = calendar;
        }

        /// <summary>Отказы за отчётные сутки, в порядке ввода.</summary>
        public List<OutcomingPatient> GetForDay(DateOnly reportDate)
        {
            return _context.OutcomingPatients
                .AsNoTracking()
                .Where(p => p.ReportDate == reportDate)
                .OrderBy(p => p.Id)
                .ToList();
        }

        public bool Add(OutcomingPatient patient, DateOnly reportDate, out string error)
        {
            error = string.Empty;

            if (!Normalize(patient, out error))
            {
                return false;
            }

            if (Exists(patient.Name!, reportDate))
            {
                error = "Этот пациент уже записан за эти сутки.";
                return false;
            }

            patient.ReportDate = reportDate;
            patient.Date = _calendar.LocalNow;

            _context.OutcomingPatients.Add(patient);
            _context.SaveChanges();
            return true;
        }

        public bool Update(OutcomingPatient patient, out string error)
        {
            error = string.Empty;

            if (patient.Id == 0)
            {
                error = "Не выбрана запись пациента.";
                return false;
            }

            if (!Normalize(patient, out error))
            {
                return false;
            }

            if (patient.ReportDate is null)
            {
                error = "У записи не определены отчётные сутки.";
                return false;
            }

            if (Exists(patient.Name!, patient.ReportDate.Value, patient.Id))
            {
                error = "Этот пациент уже записан за эти сутки.";
                return false;
            }

            // Через найденную сущность, а не Update(patient): привязывать отсоединённый
            // объект нельзя, если он уже отслеживается в этом же контексте.
            // ReportDate и Date при правке не меняются — это сутки и факт сохранения.
            var stored = _context.OutcomingPatients.FirstOrDefault(p => p.Id == patient.Id);
            if (stored is null)
            {
                error = "Запись пациента не найдена.";
                return false;
            }

            stored.Name = patient.Name;
            stored.Gender = patient.Gender;
            stored.AgeYears = patient.AgeYears;
            stored.AgeMonth = patient.AgeMonth;
            stored.Shipped = patient.Shipped;
            stored.Diagnos = patient.Diagnos;
            stored.SubmitedFrom = patient.SubmitedFrom;
            stored.SubmitedTo = patient.SubmitedTo;
            stored.Route = patient.Route;

            _context.SaveChanges();
            return true;
        }

        public void Delete(int id)
        {
            var stored = _context.OutcomingPatients.FirstOrDefault(p => p.Id == id);
            if (stored is null)
            {
                return;
            }

            _context.OutcomingPatients.Remove(stored);
            _context.SaveChanges();
        }

        /// <summary>
        /// Числа для четырёх строк сводки. Отделение здесь не участвует: это общие
        /// цифры приёмного отделения, а по колонкам отделений их не разнести.
        /// </summary>
        public static OutRejectCounts CountByRoute(IReadOnlyList<OutcomingPatient> patients)
        {
            var counts = new OutRejectCounts();

            foreach (var patient in patients)
            {
                var route = ResolveRoute(patient);
                if (route is null)
                {
                    // Маршрут не заполнен — строка не определена, в подсчёт не идёт.
                    continue;
                }

                counts.Add(route.Value, IsChild(patient));
            }

            return counts;
        }

        /// <summary>
        /// Тип маршрута записи. У новых он выбран формой, у старых (Route = null)
        /// определяется по тексту — в базе V1 «Отказ», «отказ» и «АМБУЛАТОРНО»,
        /// и правило V1 их не узнавало.
        /// </summary>
        public static OutRoute? ResolveRoute(OutcomingPatient patient)
            => patient.Route ?? RouteFromText(patient.SubmitedTo);

        /// <summary>
        /// Разбор текста «Переведен» для старых записей. Пустое значение маршрута не
        /// означает «в другую МО» — это просто незаполненное поле, и в подсчёт оно не идёт.
        /// </summary>
        public static OutRoute? RouteFromText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var text = value.Trim().ToLowerInvariant();

            if (text.StartsWith("отказ", StringComparison.Ordinal))
            {
                return OutRoute.Reject;
            }

            if (text.StartsWith("амбулатор", StringComparison.Ordinal))
            {
                return OutRoute.Ambulatory;
            }

            return OutRoute.OtherHospital;
        }

        /// <summary>
        /// Ребёнок ли пациент. Год рождения в базе нет, поэтому как в V1 смотрим на
        /// возраст, но без исключений: «3.5» больше не роняет расчёт, а пустой возраст
        /// считаем взрослым, как в отчётности по приёмному отделению.
        /// </summary>
        public static bool IsChild(OutcomingPatient patient)
        {
            if (TryParseYears(patient.AgeYears, out var years))
            {
                return years < 18;
            }

            // Год не заполнен или испорчен: у младенца смотрим месяцы.
            if (int.TryParse((patient.AgeMonth ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var months))
            {
                return months is >= 0 and < 12;
            }

            return false;
        }

        private static bool TryParseYears(string? value, out int years)
        {
            years = 0;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            // В базе встречаются «3.5», «34.0» и запятая как десятичный разделитель.
            var text = value.Trim().Replace(',', '.');

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                years = (int)Math.Floor(parsed);
                return true;
            }

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out years);
        }

        /// <summary>
        /// Есть ли уже такой пациент за эти сутки. Сравнение без учёта регистра делается
        /// в памяти, а не в SQL: SQLite lower() работает только с ASCII и кириллицу не
        /// трогает, поэтому запрос lower(Name) = ... молча ничего не находит.
        /// </summary>
        private bool Exists(string name, DateOnly reportDate, int exceptId = 0)
        {
            var normalized = (name ?? string.Empty).Trim();

            var sameDay = _context.OutcomingPatients
                .AsNoTracking()
                .Where(p => p.Id != exceptId && p.ReportDate == reportDate)
                .Select(p => p.Name)
                .ToList();

            return sameDay.Any(n => string.Equals((n ?? string.Empty).Trim(), normalized, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Проверка обязательных полей и обрезка краёв. Сутки и тип маршрута здесь
        /// не меняются: сутки задаёт блок, маршрут — выбранный в форме.
        /// </summary>
        private static bool Normalize(OutcomingPatient patient, out string error)
        {
            patient.Name = Trim(patient.Name);
            patient.Gender = Trim(patient.Gender);
            patient.AgeYears = Trim(patient.AgeYears);
            patient.AgeMonth = Trim(patient.AgeMonth);
            patient.Shipped = Trim(patient.Shipped);
            patient.Diagnos = Trim(patient.Diagnos);
            patient.SubmitedFrom = Trim(patient.SubmitedFrom);
            patient.SubmitedTo = Trim(patient.SubmitedTo);

            if (string.IsNullOrEmpty(patient.Name))
            {
                error = "Укажите ФИО пациента.";
                return false;
            }

            if (patient.Route is null)
            {
                error = "Выберите маршрут пациента.";
                return false;
            }

            if (patient.Route == OutRoute.OtherHospital && string.IsNullOrEmpty(patient.SubmitedTo))
            {
                error = "Укажите, куда направлен пациент.";
                return false;
            }

            if (!string.IsNullOrEmpty(patient.AgeYears) && !TryParseYears(patient.AgeYears, out _))
            {
                error = "Возраст должен быть числом лет.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private static string? Trim(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }

    /// <summary>
    /// Числа четырёх строк сводки: три маршрута плюс итог, каждый — взрослые, дети и всего.
    /// Считает сам <see cref="OutPatientService.CountByRoute"/>, чтобы разбор возраста
    /// и маршрута жил в одном месте.
    /// </summary>
    public sealed class OutRejectCounts
    {
        public int RejectAdults { get; private set; }

        public int RejectChildren { get; private set; }

        public int AmbulatoryAdults { get; private set; }

        public int AmbulatoryChildren { get; private set; }

        public int OtherAdults { get; private set; }

        public int OtherChildren { get; private set; }

        public int RejectTotal => RejectAdults + RejectChildren;

        public int AmbulatoryTotal => AmbulatoryAdults + AmbulatoryChildren;

        public int OtherTotal => OtherAdults + OtherChildren;

        /// <summary>Взрослые, не госпитализированные никуда — по всем трём маршрутам.</summary>
        public int AdultsTotal => RejectAdults + AmbulatoryAdults + OtherAdults;

        /// <summary>Дети, не госпитализированные никуда.</summary>
        public int ChildrenTotal => RejectChildren + AmbulatoryChildren + OtherChildren;

        public int Total => AdultsTotal + ChildrenTotal;

        internal void Add(OutRoute route, bool isChild)
        {
            switch (route)
            {
                case OutRoute.Reject:
                    if (isChild) RejectChildren++; else RejectAdults++;
                    break;
                case OutRoute.Ambulatory:
                    if (isChild) AmbulatoryChildren++; else AmbulatoryAdults++;
                    break;
                default:
                    if (isChild) OtherChildren++; else OtherAdults++;
                    break;
            }
        }
    }
}