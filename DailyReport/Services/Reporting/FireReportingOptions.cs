namespace DailyReport.Services.Reporting
{
    /// <summary>
    /// Настройки отчётных суток пожарной сводки (секция FireReporting в appsettings.json).
    /// Граница у пожарной сводки своя и совпадает с правилом первой версии — 08:00.
    /// </summary>
    public class FireReportingOptions
    {
        /// <summary>
        /// Граница, начиная с которой открываются новые отчётные сутки пожарной сводки.
        /// Значение из первой версии — 08:00.
        /// </summary>
        public string DayCutoff { get; set; } = "08:00";
    }
}
