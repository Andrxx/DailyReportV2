namespace DailyReport.Services.Security
{
    /// <summary>
    /// Права доступа приложения.
    /// Названия прав не зависят от способа подтверждения: сегодня это пароль из конфигурации,
    /// позже — роли и политики авторизации. Проверка права описана в <see cref="IAccessControl"/>.
    /// </summary>
    public static class AccessRights
    {
        /// <summary>Просмотр и правка сводки отделения за прошлые отчётные сутки.</summary>
        public const string PastReports = nameof(PastReports);
    }
}
