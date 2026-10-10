using Microsoft.Extensions.Options;

namespace DailyReport.Services.Reporting
{
    /// <summary>
    /// Единая точка расчёта отчётных суток.
    /// Сутки — это чистая дата, а не интервал времени: форма открывает одни и те же
    /// сутки в 06:00 и в 09:00. Граница DayCutoff определяет только то, какие сутки
    /// считаются текущими.
    /// </summary>
    public class ReportingCalendar
    {
        private readonly TimeZoneInfo _timeZone;
        private readonly TimeOnly _dayCutoff;
        private readonly ILogger<ReportingCalendar> _logger;

        public TimeOnly DayStart { get; }
        public TimeOnly DayCutoff => _dayCutoff;
        public TimeZoneInfo TimeZone => _timeZone;

        public ReportingCalendar(IOptions<ReportingOptions> options, ILogger<ReportingCalendar> logger)
        {
            _logger = logger;

            var value = options.Value;
            DayStart = ParseTime(value.DayStart, new TimeOnly(6, 0), nameof(ReportingOptions.DayStart));
            _dayCutoff = ParseTime(value.DayCutoff, new TimeOnly(7, 30), nameof(ReportingOptions.DayCutoff));
            _timeZone = ResolveTimeZone(value.TimeZone);
        }

        /// <summary>
        /// Отчётные сутки, к которым относится указанный момент времени.
        /// До границы DayCutoff относятся вчерашние сутки, начиная с неё — сегодняшние.
        /// </summary>
        public DateOnly GetReportDay(DateTimeOffset instant)
        {
            var local = TimeZoneInfo.ConvertTime(instant, _timeZone);
            var day = new DateOnly(local.Year, local.Month, local.Day);
            return local.TimeOfDay < _dayCutoff.ToTimeSpan() ? day.AddDays(-1) : day;
        }

        /// <summary>
        /// Отчётные сутки для времени настенных часов, уже выраженного в отчётном поясе.
        /// Принимать DateTime.Now на сервере в UTC здесь нельзя — это другая стенка часов.
        /// </summary>
        public DateOnly GetReportDayFromWallClock(DateTime wallClock)
        {
            var day = DateOnly.FromDateTime(wallClock);
            return wallClock.TimeOfDay < _dayCutoff.ToTimeSpan() ? day.AddDays(-1) : day;
        }

        /// <summary>Отчётные сутки «сейчас».</summary>
        public DateOnly Today => GetReportDay(DateTimeOffset.Now);

        /// <summary>Открытые сутки, смещённые назад. Смещение только отрицательное.</summary>
        public DateOnly GetReportDay(int daysBack)
        {
            if (daysBack > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(daysBack), daysBack, "Смещение возможно только в прошлую сторону.");
            }

            return Today.AddDays(daysBack);
        }

        /// <summary>
        /// Время настенных часов в отчётном поясе — для отметок о времени сохранения.
        /// </summary>
        public DateTime LocalNow => TimeZoneInfo.ConvertTime(DateTimeOffset.Now, _timeZone).DateTime;

        private static TimeOnly ParseTime(string? value, TimeOnly fallback, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            if (TimeOnly.TryParse(value.Trim(), out var parsed))
            {
                return parsed;
            }

            throw new InvalidOperationException($"Некорректное время в настройке Reporting:{name} = \"{value}\". Ожидается ЧЧ:ММ.");
        }

        /// <summary>
        /// Пояс по идентификатору IANA (Europe/Moscow). На Windows тот же идентификатор
        /// не встроен, а на Linux в slim-образе может отсутствовать tzdata,
        /// поэтому у Europe/Moscow есть запасной вариант.
        /// </summary>
        private TimeZoneInfo ResolveTimeZone(string? id)
        {
            var name = string.IsNullOrWhiteSpace(id) ? "Europe/Moscow" : id.Trim();

            var zone = TryFind(name) ?? TryFind(ToWindowsId(name));
            if (zone is not null)
            {
                return zone;
            }

            if (name.Equals("Europe/Moscow", StringComparison.OrdinalIgnoreCase))
            {
                // Москва живёт на UTC+3 без перехода на летнее время с 2014 года.
                // Если на сервере не установлен tzdata, фиксированного смещения достаточно.
                _logger.LogWarning(
                    "Часовой пояс {TimeZone} не найден в системе, используется фиксированное смещение UTC+03:00. " +
                    "Установите пакет tzdata, чтобы учитывались правки часового пояса.", name);
                return TimeZoneInfo.CreateCustomTimeZone(name, TimeSpan.FromHours(3), name, name);
            }

            throw new InvalidOperationException(
                $"Часовой пояс \"{name}\" не найден в системе. Используйте идентификатор IANA, например Europe/Moscow.");
        }

        private static TimeZoneInfo? TryFind(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
                return null;
            }
            catch (InvalidTimeZoneException)
            {
                return null;
            }
        }

        private static string? ToWindowsId(string ianaId) => ianaId.ToLowerInvariant() switch
        {
            "europe/moscow" => "Russian Standard Time",
            "europe/kaliningrad" => "Kaliningrad Standard Time",
            _ => null,
        };
    }
}
