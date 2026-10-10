namespace DailyReport.Services.Security
{
    /// <summary>
    /// Секция "Security" конфигурации приложения.
    /// Значения переопределяются переменными окружения, например Security__PastReportPassword.
    /// </summary>
    public class SecurityOptions
    {
        /// <summary>
        /// Пароль доступа к сводкам за прошлые отчётные сутки.
        /// Пустое значение — доступ к прошлым датам закрыт.
        /// </summary>
        public string PastReportPassword { get; set; } = string.Empty;
    }
}
