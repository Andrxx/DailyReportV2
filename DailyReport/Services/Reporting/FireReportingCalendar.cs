using Microsoft.Extensions.Options;

namespace DailyReport.Services.Reporting
{
    /// <summary>
    /// Отчётные сутки пожарной сводки. Граница своя — 08:00 (правило первой версии),
    /// она не совпадает с границей сводки отделений (07:30 из ReportingCalendar).
    /// Часовой пояс берётся из ReportingCalendar, чтобы не дублировать логику поиска
    /// пояса и запасного варианта UTC+03:00.
    /// </summary>
    public class FireReportingCalendar
    {
        private readonly ReportingCalendar _calendar;
        private readonly TimeOnly _dayCutoff;

        public TimeOnly DayCutoff => _dayCutoff;

        public FireReportingCalendar(ReportingCalendar calendar, IOptions<FireReportingOptions> options)
        {
            _calendar = calendar;
            _dayCutoff = ParseTime(options.Value.DayCutoff, new TimeOnly(8, 0));
        }

        /// <summary>
        /// Отчётные сутки пожарной сводки, к которым относится указанный момент.
        /// До границы 08:00 относятся вчерашние сутки, начиная с неё — сегодняшние.
        /// </summary>
        public DateOnly GetReportDay(DateTimeOffset instant)
        {
            var local = TimeZoneInfo.ConvertTime(instant, _calendar.TimeZone);
            var day = new DateOnly(local.Year, local.Month, local.Day);
            return local.TimeOfDay < _dayCutoff.ToTimeSpan() ? day.AddDays(-1) : day;
        }

        /// <summary>Отчётные сутки пожарной сводки «сейчас».</summary>
        public DateOnly Today => GetReportDay(DateTimeOffset.Now);

        /// <summary>Время настенных часов в отчётном поясе — отметка о времени сохранения.</summary>
        public DateTime LocalNow => _calendar.LocalNow;

        private static TimeOnly ParseTime(string? value, TimeOnly fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            if (TimeOnly.TryParse(value.Trim(), out var parsed))
            {
                return parsed;
            }

            throw new InvalidOperationException($"Некорректное время в настройке FireReporting:DayCutoff = \"{value}\". Ожидается ЧЧ:ММ.");
        }
    }
}
