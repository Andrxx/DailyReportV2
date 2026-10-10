namespace DailyReport.Services.Reporting
{
    /// <summary>
    /// Настройки отчётных суток (секция Reporting в appsettings.json).
    /// </summary>
    public class ReportingOptions
    {
        /// <summary>
        /// Регламентное начало отчётных суток. На расчёт не влияет, только подпись.
        /// </summary>
        public string DayStart { get; set; } = "06:00";

        /// <summary>
        /// Граница, начиная с которой открываются новые отчётные сутки.
        /// Единственное значение, участвующее в расчёте.
        /// </summary>
        public string DayCutoff { get; set; } = "07:30";

        /// <summary>
        /// Часовой пояс, в котором заданы границы суток. На сервере может быть UTC.
        /// </summary>
        public string TimeZone { get; set; } = "Europe/Moscow";
    }
}
